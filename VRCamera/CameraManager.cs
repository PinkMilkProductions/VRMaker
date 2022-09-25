using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker;
using Valve.VR;

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

            Camera OriginalCamera = Game.GetCamera();
            // If we are not in firstperson
            if (CameraManager.CurrentCameraMode != CameraManager.VRCameraMode.FirstPerson)
            {
                Logs.WriteInfo("Got past cameramod check");
                if (Game.Instance.Player.MainCharacter != null)
                {
                    Logs.WriteInfo("Got past maincharacter exist check");
                    // switch to first person
                    VROrigin.transform.parent = null;
                    VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.EyePosition;

                    VROrigin.transform.LookAt(Game.Instance.Player.MainCharacter.Value.OrientationDirection);

                    if (!OriginalCameraParent)
                    {
                        OriginalCameraParent = OriginalCamera.transform.parent;
                    }

                    OriginalCamera.transform.parent = VROrigin.transform;
                    if (RightHand)
                        RightHand.transform.parent = VROrigin.transform;
                    if (LeftHand)
                        LeftHand.transform.parent = VROrigin.transform;
                    CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.FirstPerson;
                }

            }
            else
            {
                VROrigin.transform.position = OriginalCameraParent.position;
                VROrigin.transform.rotation = OriginalCameraParent.rotation;
                VROrigin.transform.localScale = OriginalCameraParent.localScale;

                VROrigin.transform.parent = OriginalCameraParent;

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
                LeftHand.transform.parent = VROrigin.transform;
            }
        }

        public static void HandleDemeoCamera()
        {
            if ((CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.DemeoLike)
                && RightHand && LeftHand)
            {
                Kingmaker.View.CameraRig TheCameraRig = Kingmaker.Game.Instance.UI.GetCameraRig();
                Camera CurrentCamera = TheCameraRig.Camera;
                if (!VROrigin.GetComponent<Rigidbody>())
                {
                    Rigidbody tempvar = VROrigin.AddComponent<Rigidbody>();
                    tempvar.useGravity = false;
                }
                    
                Rigidbody VROriginPhys = VROrigin.GetComponent<Rigidbody>();
                if (RightHandGrab && LeftHandGrab)
                {
                    // SCALING
                    if (InitialHandDistance == 0f)
                    {
                        InitialHandDistance = Vector3.Distance(CameraManager.RightHand.transform.position, CameraManager.LeftHand.transform.position);
                        ZoomOrigin = VROrigin.transform.position;
                    }
                    float HandDistance = Vector3.Distance(CameraManager.RightHand.transform.position, CameraManager.LeftHand.transform.position);
                    float scale = HandDistance / InitialHandDistance;

                    VROrigin.transform.position = Vector3.LerpUnclamped(Game.Instance.Player.MainCharacter.Value.GetPosition(), ZoomOrigin, scale);

                    // ROTATING
                    if (InitialRotation == true)
                    {
                        InitialRotationPoint = Vector3.Lerp(CameraManager.LeftHand.transform.position, CameraManager.RightHand.transform.position, 0.5f);
                        PreviousRotationVector = CameraManager.LeftHand.transform.position - InitialRotationPoint;
                        PreviousRotationVector.y = 0;
                        InitialRotation = false;
                    }
                    Vector3 RotationVector = CameraManager.LeftHand.transform.position - CameraManager.RightHand.transform.position;
                    RotationVector.y = 0;

                    float Angle = Vector3.SignedAngle(PreviousRotationVector, RotationVector, Vector3.up);
                    //if (Angle > 1f)
                    //    Angle = 0;
                    Angle = Angle / 2;
     
                    VROrigin.transform.RotateAround(InitialRotationPoint, Vector3.up, Angle);

                    PreviousRotationVector = RotationVector;
                }
                else if (RightHandGrab || LeftHandGrab)
                {
                    InitialHandDistance = 0f;
                    InitialRotation = true;

                    // MOVING CAMERA
                    SpeedScalingFactor = Mathf.Clamp(Math.Abs(Vector3.Distance(Game.Instance.Player.MainCharacter.Value.GetPosition(), VROrigin.transform.position)), 1.0f, FarClipPlaneDistance);
                    if (RightHandGrab)
                    {
                        Vector3 ScaledSpeed = SteamVR_Actions._default.RightHandPose.velocity * SpeedScalingFactor;
                        VROriginPhys.velocity = new Vector3(ScaledSpeed.x, - ScaledSpeed.y, ScaledSpeed.z);
                    }
                    if (LeftHandGrab)
                    {
                        Vector3 ScaledSpeed = SteamVR_Actions._default.LeftHandPose.velocity * SpeedScalingFactor;
                        VROriginPhys.velocity = new Vector3(ScaledSpeed.x, - ScaledSpeed.y, ScaledSpeed.z);
                    }
                }
                else
                {
                    InitialHandDistance = 0f;
                    InitialRotation = true;
                    VROriginPhys.velocity = Vector3.zero;
                }
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

        public static bool RightHandGrab = false;
        public static bool LeftHandGrab = false;

        public static float InitialHandDistance = 0f;
        public static bool InitialRotation = true;
        public static Vector3 PreviousRotationVector = Vector3.zero;
        public static Vector3 InitialRotationPoint = Vector3.zero;
        public static Vector3 ZoomOrigin = Vector3.zero;
        public static float SpeedScalingFactor = 1f;

    }
    
}
