
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace UserMovingFloor
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UserMovingFloorEventListener : Cyan.PlayerObjectPool.CyanPlayerObjectPoolEventListener
    {
        const int E_RIDING = 0b1;
        const int E_GETTING = 0b10;
        const int E_OUTSIDE = 0b100;
        const int E_CHANGED = 0b1000;

        const int NORMAL         = 0;
        const int GET_ON         = E_GETTING | E_RIDING;
        const int GET_OFF        = E_GETTING;
        const int RIDING         =             E_RIDING;
        const int RIDING_OUTSIDE =             E_RIDING | E_OUTSIDE;
        const int RIDING_CHANGED =             E_RIDING             | E_CHANGED;

        public Cyan.PlayerObjectPool.CyanPlayerObjectAssigner Pool;
        public Transform[] Targets;
        Component[] TargetUdons;
        public InsideVRCStationCompanion[] InsideChairs;

        UserParentController ParentController;
        UserPositionController PositionController;
        UserPositionVRCStationCompanion PositionVRCStationCompanion;
        int InsideIndex = -1;
        int State = NORMAL;

        float Horizontal = 0;
        float Vertical = 0;
        float Rotation = 0;

        void Start()
        {
            var udons = Pool.pooledUdon;
            var len = udons.Length;
            for (int i = 0; i < len; i++)
            {
                ((UdonBehaviour)udons[i]).SetProgramVariable("Targets", Targets);
            }
            
            var len2 = Targets.Length;
            TargetUdons = new Component[len2];
            for (var i = 0; i < len2; ++i)
            {
                TargetUdons[i] = (UdonBehaviour)Targets[i].GetComponent(typeof(UdonBehaviour));
            }
        }

        public override void _OnPlayerAssigned(VRCPlayerApi player, int poolIndex, UdonBehaviour poolObject)
        {
            poolObject.SetProgramVariable("Targets", Targets);
            poolObject.SendCustomEvent("ParentIndexMayChanged");
        }

        public override void _OnLocalPlayerAssigned()
        {
            var player = Networking.LocalPlayer;
            ParentController = (UserParentController)Pool._GetPlayerPooledUdon(player);
            PositionController = ParentController.UserPositionController;
            PositionVRCStationCompanion = PositionController.VRCStationCompanion;
            Networking.SetOwner(player, PositionVRCStationCompanion.gameObject);
        }


        public override void _OnPlayerUnassigned(VRCPlayerApi player, int poolIndex, UdonBehaviour poolObject)
        {

        }

        public override void InputMoveHorizontal(float value, UdonInputEventArgs args)
        {
            Horizontal = value;
        }

        public override void InputMoveVertical(float value, UdonInputEventArgs args)
        {
            Vertical = value;
        }

        public override void InputLookHorizontal(float value, UdonInputEventArgs args)
        {
            Rotation = value;
        }

        void Update()
        {
            if (ParentController == null) return;

            // 位置判定
            var player = Networking.LocalPlayer;
            var playerPosition = player.GetPosition();
            var len = Targets.Length;
            var closestPoints = new Vector3[len];
            int currentInsideIndex = -1;
            for (var i = 0; i < len; ++i)
            {
                var target = (UdonBehaviour)TargetUdons[i];
                if ((bool)target.GetProgramVariable("Moving"))
                {
                    var closestPoint = ((Collider)target.GetProgramVariable("InsideCollider")).ClosestPoint(playerPosition);
                    closestPoints[i] = closestPoint;
                    if (closestPoint == playerPosition)
                    {
                        currentInsideIndex = i;
                        break;
                    }
                }
            }

            // 状況判定
            if (InsideIndex == currentInsideIndex)
            {
                if (currentInsideIndex == -1)
                {
                    State = NORMAL;
                }
                else
                {
                    State = RIDING;
                }
            }
            else
            {
                if (InsideIndex == -1)
                {
                    State = GET_ON;
                }
                else if (currentInsideIndex == -1)
                {
                    var target = (UdonBehaviour)TargetUdons[InsideIndex];
                    // 移動中の対象から降りることが出来ない場合
                    if (!(bool)target.GetProgramVariable("CanGetOffWhileMoving") && (bool)target.GetProgramVariable("Moving"))
                    {
                        State = RIDING_OUTSIDE;
                    }
                    else
                    {
                        State = GET_OFF;
                    }
                }
                else
                {
                    State = RIDING_CHANGED;
                }
            }

            // 椅子固定
            switch (State)
            {
                case GET_ON:
                    var target = (UdonBehaviour)TargetUdons[currentInsideIndex];
                    PositionVRCStationCompanion.transform.parent = target.transform;
                    PositionVRCStationCompanion.transform.position = playerPosition;
                    PositionVRCStationCompanion.transform.rotation = player.GetRotation();
                    ParentController.ResetLocalOrigin(currentInsideIndex);
                    PositionVRCStationCompanion.UseStation(player);
                    // player.Immobilize(true);
                    target.SendCustomEvent("_OnGetOn");
                    InsideIndex = currentInsideIndex;
                    break;
                case GET_OFF:
                    ((UdonBehaviour)TargetUdons[InsideIndex]).SendCustomEvent("_OnGetOff");
                    // player.Immobilize(false);
                    ParentController.ResetLocalOrigin(currentInsideIndex);
                    PositionVRCStationCompanion.ExitStation(player);
                    InsideIndex = currentInsideIndex;
                    break;
                case RIDING_CHANGED:
                    PositionVRCStationCompanion.transform.parent = Targets[currentInsideIndex];
                    ParentController.ResetLocalOrigin(currentInsideIndex);
                    InsideIndex = currentInsideIndex;
                    break;
            }

            // 位置追従
            if (State == RIDING_OUTSIDE)
            {
                PositionVRCStationCompanion.transform.position = closestPoints[InsideIndex];
                PositionController.transform.position = PositionVRCStationCompanion.transform.localPosition;
            }
            else if ((State & E_RIDING) != 0)
            {
                var isVr = player.IsUserInVR();

                if (Rotation != 0f)
                {
                    if (isVr)
                    {
                        PositionController.transform.localRotation = PositionController.transform.localRotation * Quaternion.AngleAxis(Rotation * 5, Vector3.up);
                    }
                    else
                    {
                        PositionController.transform.localRotation *= Quaternion.AngleAxis(Rotation * 5, Vector3.up);
                    }
                }

                // position
                // 非VRでShift押していなければ歩行
                var playerSpeed = isVr || Input.GetKey(KeyCode.LeftShift)
                    ? player.GetRunSpeed()
                    : player.GetWalkSpeed();
                if (Vertical != 0f || Horizontal != 0f)
                {
                    var rotationY = isVr ? PositionController.transform.localRotation.eulerAngles.y : player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation.eulerAngles.y - Targets[InsideIndex].rotation.eulerAngles.y;
                    var playerMoved = Quaternion.Euler(0, rotationY, 0) * new Vector3(Horizontal, 0, Vertical) * playerSpeed * Time.deltaTime;
                    PositionController.transform.localPosition += playerMoved;
                }

                // 椅子から降りてしまっている場合のせる
                if (State != GET_ON && !PositionVRCStationCompanion.Sitting)
                {
                    // 内部の椅子に座っているか判定
                    var insideSitting = false;
                    var len2 = InsideChairs.Length;
                    for (int i = 0; i < len2; i++)
                    {
                        if (InsideChairs[i].Sitting)
                        {
                            insideSitting = true;
                            break;
                        }
                    }
                    // 内部のどの椅子にも座っていなければ
                    if (!insideSitting)
                    {
                        PositionVRCStationCompanion.UseStation(player);
                    }
                }
            }
        }
    }
}

