using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker;

namespace VRMaker
{
    public class CameraManager
    {
        public CameraManager()
        {
            CurrentCameraMode = VRCameraMode.UI;
        }

        public enum VRCameraMode
        {
            DemeoLike,
            FirstPerson,
            Cutscene,
            UI
        }

        public VRCameraMode CurrentCameraMode;
    }
}
