using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMaker
{
    class StaticUiTarget : UiTarget
    {
        protected override float MinAngleDelta => 20f;

        public static StaticUiTarget Create(Transform UiTargetParent)
        {
            var instance = new GameObject(nameof(StaticUiTarget)).AddComponent<StaticUiTarget>();
            // In rai's original code this is used to keep things alive
            instance.transform.SetParent(UiTargetParent, false);
            instance.TargetTransform = new GameObject("InteractiveUiTargetTransform").transform;
            instance.TargetTransform.SetParent(instance.transform, false);
            instance.TargetTransform.localPosition = Vector3.forward;
            return instance;
        }

        protected override Vector3 GetCameraForward()
        {
            return CameraTransform.forward;
        }
    }
}
