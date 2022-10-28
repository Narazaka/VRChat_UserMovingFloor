
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UserMovingFloor
{
    public class UserMovingFloorEventListener : UdonSharpBehaviour
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

        UserPositionController PositionController;
        UserPositionVRCStationCompanion PositionVRCStationCompanion;
        int InsideIndex = -1;
        int State = NORMAL;

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

        public void _OnLocalPlayerAssigned()
        {
            var player = Networking.LocalPlayer;
            PositionController = (UserPositionController)Pool._GetPlayerPooledUdon(player);
            PositionVRCStationCompanion = PositionController.VRCStationCompanion;
            Networking.SetOwner(player, PositionVRCStationCompanion.gameObject);
        }

        void Update()
        {
            if (PositionController == null) return;

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
                    PositionController.ResetLocalOrigin(currentInsideIndex);
                    PositionVRCStationCompanion.UseStation(player);
                    // player.Immobilize(true);
                    target.SendCustomEvent("_OnGetOn");
                    InsideIndex = currentInsideIndex;
                    break;
                case GET_OFF:
                    ((UdonBehaviour)TargetUdons[InsideIndex]).SendCustomEvent("_OnGetOff");
                    // player.Immobilize(false);
                    PositionController.ResetLocalOrigin(currentInsideIndex);
                    PositionVRCStationCompanion.ExitStation(player);
                    InsideIndex = currentInsideIndex;
                    break;
                case RIDING_CHANGED:
                    PositionVRCStationCompanion.transform.parent = Targets[currentInsideIndex];
                    PositionController.ResetLocalOrigin(currentInsideIndex);
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

                // rotation
                if (isVr)
                {
                    var rotation = Input.GetAxis("Joy2 Axis 4");
                    if (rotation != 0)
                        PositionController.transform.localRotation = PositionController.transform.localRotation * Quaternion.AngleAxis(rotation * 5, Vector3.up);
                }
                else
                {
                    var rotation = Input.GetAxis("Mouse X");
                    if (rotation != 0)
                        PositionController.transform.localRotation *= Quaternion.AngleAxis(rotation * 5, Vector3.up);
                }

                // position
                var vertical = Input.GetAxis("Vertical");
                var horizontal = Input.GetAxis("Horizontal");
                // 非VRでShift押していなければ歩行
                var playerSpeed = isVr || Input.GetKey(KeyCode.LeftShift)
                    ? player.GetRunSpeed()
                    : player.GetWalkSpeed();
                if (vertical != 0f || horizontal != 0f)
                {
                    var rotationY = isVr ? PositionController.transform.localRotation.eulerAngles.y : player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation.eulerAngles.y - Targets[InsideIndex].rotation.eulerAngles.y;
                    var playerMoved = Quaternion.Euler(0, rotationY, 0) * new Vector3(horizontal, 0, vertical) * playerSpeed * Time.deltaTime;
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

