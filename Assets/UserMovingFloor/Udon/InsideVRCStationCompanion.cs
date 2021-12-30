
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UserMovingFloor
{
    public class InsideVRCStationCompanion : UdonSharpBehaviour
    {
        [HideInInspector]
        public bool Sitting;

        public override void Interact()
        {
            Networking.LocalPlayer.UseAttachedStation();
        }

        public override void OnStationEntered(VRCPlayerApi player)
        {
            Sitting = true;
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            Sitting = false;
        }
    }
}
