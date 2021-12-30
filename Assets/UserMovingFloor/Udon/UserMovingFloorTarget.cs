
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UserMovingFloor
{
    public class UserMovingFloorTarget : UdonSharpBehaviour
    {
        public Collider[] RideColliders;

        public Collider InsideCollider;
        public bool CanGetOffWhileMoving;
        public bool Moving;

        public void _OnGetOn()
        {
            var len = RideColliders.Length;
            for (int i = 0; i < len; i++)
                RideColliders[i].enabled = false;
        }
        public void _OnGetOff()
        {
            var len = RideColliders.Length;
            for (int i = 0; i < len; i++)
                RideColliders[i].enabled = true;
        }
    }
}
