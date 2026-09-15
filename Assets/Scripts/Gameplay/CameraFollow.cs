using UnityEngine;

namespace ChronoBlade.Gameplay
{
    // Simple smoothed follow — a lightweight stand-in for a full Cinemachine rig (the package
    // is installed for later) so the camera isn't dead-locked to the origin during a demo.
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothTime = 0.15f;
        public Vector3 offset = new Vector3(0f, 0f, -10f);

        Vector3 _velocity;

        void LateUpdate()
        {
            if (target == null) return;
            Vector3 desired = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
