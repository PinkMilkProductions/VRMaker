using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker;

namespace VRMaker
{
    class AttachedUi : MonoBehaviour
    {
        private Transform targetTransform;

        public static void Create<TAttachedUi>(Canvas canvas, float scale = 0)
            where TAttachedUi : AttachedUi
        {
            var instance = canvas.gameObject.AddComponent<TAttachedUi>();
            if (scale > 0) canvas.transform.localScale = Vector3.one * scale;
            canvas.renderMode = RenderMode.WorldSpace;
        }

        protected virtual void Update()
        {
            UpdateTransform();
        }

        public void SetTargetTransform(Transform target)
        {
            targetTransform = target;
        }

        private void UpdateTransform()
        {
            transform.position = Game.GetCamera().transform.position + Game.GetCamera().transform.forward;
            transform.rotation = Game.GetCamera().transform.rotation;
        }
    }
}
