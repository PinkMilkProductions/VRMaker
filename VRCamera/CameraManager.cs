using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker;

namespace VRMaker
{
    public static class CameraManager
    {
        static CameraManager()
        {
            CurrentCameraMode = VRCameraMode.UI;
            //Fix near plance clipping for main camera
            if (Camera.main != null)
            {
                Camera.main.nearClipPlane = NearClipPlaneDistance;
                Camera.main.farClipPlane = FarClipPlaneDistance;
            }

        }

        public static void ReduceNearClipping()
        {
            Camera CurrentCamera = Game.GetCamera();
            CurrentCamera.nearClipPlane = NearClipPlaneDistance;
            CurrentCamera.farClipPlane = FarClipPlaneDistance;
        }

        public static void TurnOffPostProcessing()
        {
            Camera CurrentCamera = Game.GetCamera();
            UnityEngine.PostProcessing.PostProcessingBehaviour PPbehaviour = CurrentCamera.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();
            PPbehaviour.enabled = false;
        }

        public static void AddSkyBox()
        {
            // ADD THE LOADED SKYBOX !!!!
            var SceneSkybox = GameObject.Instantiate(AssetLoader.Skybox, Vector3.zeroVector, Quaternion.identityQuaternion);
            SceneSkybox.transform.localScale = new Vector3(999999, 999999, 999999);
            SceneSkybox.transform.eulerAngles = new Vector3(270, 0, 0);
        }

        public enum VRCameraMode
        {
            DemeoLike,
            FirstPerson,
            Cutscene,
            UI
        }

        public static VRCameraMode CurrentCameraMode;
        public static float NearClipPlaneDistance = 0.01f;
        public static float FarClipPlaneDistance = 59999f;
        public static bool DisableParticles = false;
    }
    
}
