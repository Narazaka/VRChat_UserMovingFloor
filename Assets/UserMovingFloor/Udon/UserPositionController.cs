
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UserMovingFloor
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UserPositionController : UdonSharpBehaviour
    {
        public UserPositionVRCStationCompanion VRCStationCompanion;

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

        public void ResetLocalOrigin()
        {
            transform.position = VRCStationCompanion.transform.localPosition;
            transform.rotation = Quaternion.AngleAxis(VRCStationCompanion.transform.localEulerAngles.y, Vector3.up);
        }

        void Update()
        {
            VRCStationCompanion.transform.localPosition = transform.localPosition;
            VRCStationCompanion.transform.localRotation = transform.localRotation;
        }
    }
}
