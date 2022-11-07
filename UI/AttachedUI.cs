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
        public Canvas MyCanvas = null;

        public static void Create<TAttachedUi>(Canvas canvas, Transform target, float scale = 0)
            where TAttachedUi : AttachedUi
        {
            
            var instance = canvas.gameObject.AddComponent<TAttachedUi>();
            if (scale > 0) canvas.transform.localScale = Vector3.one * scale;
            canvas.renderMode = RenderMode.WorldSpace;

            instance.targetTransform = target;

            if (canvas.name == "Console_StaticCanvas")
            {
                instance.MyCanvas = canvas;
            }
        }

        protected virtual void Update()
        {
            if (!targetTransform)
            {
                Logs.WriteWarning($"Target transform for AttachedUi {name} is missing, destroying");
                Destroy(this);
                return;
            }

            if (MyCanvas)
            {
                MyCanvas.transform.parent = null;
            }

            UpdateTransform();
            //// The only way i seem to call this stuff
            ////Logs.WriteInfo("Update hook called");
            //Controllers.Update();
        }

        public void SetTargetTransform(Transform target)
        {
            targetTransform = target;
        }

        private void UpdateTransform()
        {
            //if (Game.GetCamera())
            //{
            //    transform.position = Game.GetCamera().transform.position + Game.GetCamera().transform.forward;
            //    transform.rotation = Game.GetCamera().transform.rotation;
            //}

            transform.position = targetTransform.position;
            //transform.rotation = targetTransform.rotation;
            transform.rotation = Game.GetCamera().transform.rotation;

            //if (MyCanvas)
            //{
            //    MyCanvas.transform.position = targetTransform.position;
            //    //MyCanvas.transform.rotation = targetTransform.rotation;
            //    MyCanvas.transform.rotation = Game.GetCamera().transform.rotation;
            //}

        }
    }
}
