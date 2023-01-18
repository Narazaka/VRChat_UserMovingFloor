
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UserMovingFloor
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class UserParentController : UdonSharpBehaviour
    {
        public UserPositionVRCStationCompanion VRCStationCompanion;
        public UserPositionController UserPositionController;
        // Who is the current owner of this object. Null if object is not currently in use. 
        [HideInInspector]
        public VRCPlayerApi Owner;
        [UdonSynced, HideInInspector]
        public int ParentIndex = -1;
        [HideInInspector]
        public int PreviousParentIndex = -1;
        [HideInInspector]
        public Transform[] Targets;

        public void _OnOwnerSet()
        {
            if (Owner.isLocal) Networking.SetOwner(Owner, UserPositionController.gameObject);
        }

        public void _OnCleanup()
        {
            // Cleanup the object here
        }

        public void ResetLocalOrigin(int parentIndex)
        {
            ParentIndex = parentIndex;
            RequestSerialization();
            UserPositionController.ResetLocalOrigin();
        }

        public override void OnDeserialization()
        {
            ParentIndexMayChanged();
        }

        public void ParentIndexMayChanged()
        {
            if (PreviousParentIndex != ParentIndex && Targets.Length > 0)
            {
                VRCStationCompanion.transform.parent = ParentIndex == -1 ? UserPositionController.transform : Targets[ParentIndex];
                PreviousParentIndex = ParentIndex;
            }
        }
    }
}
