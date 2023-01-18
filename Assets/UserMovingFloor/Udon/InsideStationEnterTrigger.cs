
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UserMovingFloor
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InsideStationEnterTrigger : UdonSharpBehaviour
    {
        public VRCStation VRCStation;

        public override void Interact()
        {
            var player = Networking.LocalPlayer;
            Networking.SetOwner(player, VRCStation.gameObject);
            VRCStation.UseStation(player);
        }
    }
}
