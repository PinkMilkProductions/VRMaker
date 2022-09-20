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

        public static void SwitchPOV()
        {
            Logs.WriteInfo("Entered SwitchPOV function");

            // ADD A SKYBOX
            CameraManager.AddSkyBox();

            Logs.WriteInfo("AddedSkyBox");

            Camera MyCamera = Game.GetCamera();
            // If we are not in firstperson
            if (CameraManager.CurrentCameraMode != CameraManager.VRCameraMode.FirstPerson)
            {
                Logs.WriteInfo("Got past cameramod check");
                if (Game.Instance.Player.MainCharacter != null)
                {
                    Logs.WriteInfo("Got past maincharacter exist check");
                    // switch to first person
                    VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.GetPosition();
                    OriginalCameraParent = MyCamera.transform.parent;
                    MyCamera.transform.parent = VROrigin.transform;
                    if (RightHand)
                        RightHand.transform.parent = VROrigin.transform;
                    if (LeftHand)
                        LeftHand.transform.parent = VROrigin.transform;
                    CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.FirstPerson;
                }

            }
            else
            {
                MyCamera.transform.parent = OriginalCameraParent;
                if (RightHand)
                    RightHand.transform.parent = OriginalCameraParent;
                if (LeftHand)
                    LeftHand.transform.parent = OriginalCameraParent;

                CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.DemeoLike;
            }
        }

        public static void SpawnHands()
        {
            if (!RightHand)
            {
                RightHand = GameObject.Instantiate(AssetLoader.RightHandBase, Vector3.zeroVector, Quaternion.identityQuaternion);
                RightHand.transform.parent = VROrigin.transform;
            }
            if (!LeftHand)
            {
                LeftHand = GameObject.Instantiate(AssetLoader.LeftHandBase, Vector3.zeroVector, Quaternion.identityQuaternion);
                RightHand.transform.parent = VROrigin.transform;
            }


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

        public static Transform OriginalCameraParent = null;
        public static GameObject VROrigin = new GameObject();
        public static GameObject LeftHand = null;
        public static GameObject RightHand = null;
    }
    
}
