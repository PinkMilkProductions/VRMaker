using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMaker
{
    public class MBHelper : MonoBehaviour
    {
        public static MBHelper Create()
        {
            var instance = new GameObject(nameof(MBHelper)).AddComponent<MBHelper>();

            // Do some unrelated stuff we still want to do on creation time
            instance.OnCreate();

            return instance;
        }

        protected virtual void Update()
        {
            //Logs.WriteInfo("Update hook called");
            Controllers.Update();
        }

        public void OnCreate()
        {
            // Test
            Controllers.Init();
        }
    }
}
