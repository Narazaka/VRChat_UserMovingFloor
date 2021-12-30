
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UserMovingFloor
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UserPositionVRCStationCompanion : UdonSharpBehaviour
    {
        public VRCStation VRCStation;
        [HideInInspector]
        public bool Sitting;

        void Start()
        {
            DisableInteractive = true;
        }

        void OnEnable()
        {
            DisableInteractive = true;
        }

        public override void OnStationEntered(VRCPlayerApi player)
        {
            Sitting = true;
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            Sitting = false;
        }

        public void UseStation(VRCPlayerApi player)
        {
            VRCStation.UseStation(player);
        }

        public void ExitStation(VRCPlayerApi player)
        {
            VRCStation.ExitStation(player);
        }
    }
}
