using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMaker
{
    public abstract class UiTarget : MonoBehaviour
    {
        private const float rotationSmoothTime = 0.3f;
        protected Transform CameraTransform;
        private Vector3 previousForward;
        private Quaternion rotationVelocity;
        private Quaternion targetRotation;
        public Transform TargetTransform { get; protected set; }
        protected abstract float MinAngleDelta { get; }

        public void SetUp(Camera camera)
        {
            if (!camera)
            {
                Logs.WriteInfo("Aborting Target setup, camera not valid");
                return;
            }
            CameraTransform = camera.transform;
            previousForward = GetCameraForward();
            Logs.WriteInfo("Setting up Target for camera: " + camera.name);
        }

        protected virtual void Update()
        {
            UpdateTransform();
        }

        protected abstract Vector3 GetCameraForward();

        private void UpdateTransform()
        {
            if (!CameraTransform) return;

            var cameraForward = GetCameraForward();
            var unsignedAngleDelta = Vector3.Angle(previousForward, cameraForward);

            if (unsignedAngleDelta > MinAngleDelta)
            {
                targetRotation = Quaternion.LookRotation(cameraForward);
                previousForward = cameraForward;
            }

            transform.rotation = MathHelper.SmoothDamp(
                transform.rotation,
                targetRotation,
                ref rotationVelocity,
                rotationSmoothTime);

            transform.position = CameraTransform.position;
        }
    }
}
