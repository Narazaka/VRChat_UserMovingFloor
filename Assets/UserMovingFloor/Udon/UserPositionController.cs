
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UserMovingFloor
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class UserPositionController : UdonSharpBehaviour
    {
        public UserPositionVRCStationCompanion VRCStationCompanion;
        // Who is the current owner of this object. Null if object is not currently in use. 
        [HideInInspector]
        public VRCPlayerApi Owner;
        [UdonSynced, HideInInspector]
        public int ParentIndex = -1;
        [HideInInspector]
        public int PreviousParentIndex;
        [HideInInspector]
        public GameObject[] Targets;

        public void _OnOwnerSet()
        {
            // Initialize the object here
        }

        public void _OnCleanup()
        {
            // Cleanup the object here
        }

        void Start()
        {
            DisableInteractive = true;
        }

        void OnEnable()
        {
            DisableInteractive = true;
        }

        public void ResetLocalOrigin(int parentIndex)
        {
            ParentIndex = parentIndex;
            transform.position = VRCStationCompanion.transform.localPosition;
            transform.rotation = Quaternion.AngleAxis(VRCStationCompanion.transform.localEulerAngles.y, Vector3.up);
        }

        void Update()
        {
            VRCStationCompanion.transform.localPosition = transform.localPosition;
            VRCStationCompanion.transform.localRotation = transform.localRotation;
        }

        public override void OnDeserialization()
        {
            if (PreviousParentIndex != ParentIndex)
            {
                VRCStationCompanion.transform.parent = ParentIndex == -1 ? transform : Targets[ParentIndex].transform;
            }
            PreviousParentIndex = ParentIndex;
        }
    }
}
