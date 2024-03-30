using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker;
using Valve.VR;
using DG.Tweening;
using TurnBased.Controllers;

namespace VRMaker
{
    public static class CameraManager
    {
        static CameraManager()
        {
            CurrentCameraMode = VRCameraMode.UI;

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
            SceneSkybox = GameObject.Instantiate(AssetLoader.Skybox, Vector3.zeroVector, Quaternion.identityQuaternion);
            SceneSkybox.transform.localScale = new Vector3(999999, 999999, 999999);
            SceneSkybox.transform.eulerAngles = new Vector3(270, 0, 0);
        }

        public static void SwitchPOV()
        {
            Logs.WriteInfo("Entered SwitchPOV function");

            Logs.WriteInfo("AddedSkyBox");

            Camera OriginalCamera = Game.GetCamera();
            // If we are not in firstperson
            if (CameraManager.CurrentCameraMode != CameraManager.VRCameraMode.FirstPerson)
            {
                Logs.WriteInfo("Got past cameramode check");
                if (Game.Instance.Player.MainCharacter != null)
                {
                    Logs.WriteInfo("Got past maincharacter exist check");
                    //Determine the character we want to switch to
                    Kingmaker.EntitySystem.Entities.UnitEntityData FirstPerson = DetermineFirstPerson();
                    // switch to first person
                    VROrigin.transform.parent = null;
                    VROrigin.transform.position = FirstPerson.GetPosition();

                    VROrigin.transform.LookAt(FirstPerson.EyePosition + FirstPerson.OrientationDirection);
                    VROrigin.transform.Rotate(45, 0, 0, Space.Self);

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
                // UnHide the previous firstperson Entity
                if (PrevFirstPerson != null)
                {
                    foreach (Renderer rend in PrevFirstPerson.View.Renderers)
                    {
                        rend.enabled = true;
                    }
                }


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
                // Add physics to the VROrigin
                if (!VROrigin.GetComponent<Rigidbody>())
                {
                    Rigidbody tempvar = VROrigin.AddComponent<Rigidbody>();
                    tempvar.useGravity = false;
                }    
                Rigidbody VROriginPhys = VROrigin.GetComponent<Rigidbody>();

                // If we are grabbing with both our hands
                if (RightHandGrab && LeftHandGrab)
                {
                    // SCALING
                    // Setup
                    if (InitialHandDistance == 0f)
                    {
                        InitialHandDistance = Vector3.Distance(CameraManager.RightHand.transform.position, CameraManager.LeftHand.transform.position);
                        ZoomOrigin = VROrigin.transform.position;
                    }
                    float HandDistance = Vector3.Distance(CameraManager.RightHand.transform.position, CameraManager.LeftHand.transform.position);
                    float scale = HandDistance / InitialHandDistance;

                    // Do the actual distance scaling
                    VROrigin.transform.position = Vector3.LerpUnclamped(Game.Instance.Player.MainCharacter.Value.GetPosition(), ZoomOrigin, scale);

                    // ROTATING
                    // Setup
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
                    Angle = Angle / 2;
                    
                    // Do the actual rotating
                    VROrigin.transform.RotateAround(InitialRotationPoint, Vector3.up, Angle);

                    PreviousRotationVector = RotationVector;
                }
                else if (RightHandGrab || LeftHandGrab)
                {
                    // Reset scaling/rotating flags
                    InitialHandDistance = 0f;
                    InitialRotation = true;

                    // MOVING CAMERA
                    SpeedScalingFactor = Mathf.Clamp(Math.Abs(Vector3.Distance(Game.Instance.Player.MainCharacter.Value.GetPosition(), VROrigin.transform.position)), 1.0f, FarClipPlaneDistance);
                    if (RightHandGrab)
                    {
                        Vector3 ScaledSpeed = SteamVR_Actions._default.RightHandPose.velocity * SpeedScalingFactor;
                        Vector3 AdjustedSpeed = new Vector3(- ScaledSpeed.x, - ScaledSpeed.y,- ScaledSpeed.z);
                        VROriginPhys.velocity = VROrigin.transform.rotation * AdjustedSpeed;
                    }
                    if (LeftHandGrab)
                    {
                        Vector3 ScaledSpeed = SteamVR_Actions._default.LeftHandPose.velocity * SpeedScalingFactor;
                        Vector3 AdjustedSpeed = new Vector3(- ScaledSpeed.x, - ScaledSpeed.y, - ScaledSpeed.z);
                        VROriginPhys.velocity = VROrigin.transform.rotation * AdjustedSpeed;
                    }
                }
                else
                {
                    // Reset flags + stop extra camera movement
                    InitialHandDistance = 0f;
                    InitialRotation = true;
                    VROriginPhys.velocity = Vector3.zero;
                }
            }
        }

        public static void HandleFirstPersonCamera()
        {
            if (CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.FirstPerson)
            {
                Kingmaker.EntitySystem.Entities.UnitEntityData FirstPerson = DetermineFirstPerson();

                // POSITION
                // Attach our origin to the Main Character's (this function gets called every tick)
                // CameraManager.VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.GetPosition();
                //VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.EyePosition;

                CameraManager.VROrigin.transform.position = FirstPerson.Position;

                //ROTATION
                //Vector3 RotationEulers = new Vector3(0, Turnrate * RightJoystick.x, 0);
                //VROrigin.transform.Rotate(RotationEulers);
                VROrigin.transform.Rotate(0, Turnrate * RightJoystick.x, 0, Space.World);

                if (PrevFirstPerson != FirstPerson)
                {
                    // Hide the firstperson Entity
                    foreach (Renderer rend in FirstPerson.View.Renderers)
                    {
                        rend.enabled = false;
                    }
                    if (PrevFirstPerson != null)
                    {
                        // UnHide the previous firstperson Entity
                        foreach (Renderer rend in PrevFirstPerson.View.Renderers)
                        {
                            rend.enabled = true;
                        }
                    }

                }

                PrevFirstPerson = FirstPerson;
                //this stuff might be interesting
                //Firstperson.IsCurrentUnit
            }


        }

        public static Kingmaker.EntitySystem.Entities.UnitEntityData DetermineFirstPerson()
        {
            //Determine the first person character
            if (Game.Instance.Player.IsInCombat)
            {
                TurnController currentTurn = Game.Instance.TurnBasedCombatController.CurrentTurn;
                if (currentTurn == null)
                    return Kingmaker.Game.Instance.Player.GetCharactersList(Player.CharactersList.ActiveUnits).FirstOrDefault();
                if (currentTurn.Unit.IsPlayerFaction)
                    return currentTurn.Unit;
            }
            return Kingmaker.Game.Instance.Player.GetCharactersList(Player.CharactersList.ActiveUnits).FirstOrDefault();
        }


            public static Vector3 GetRightHandForward ()
        {
            Quaternion dummyrotation = RightHand.transform.rotation;
            // 45 degrees for Oculus
            dummyrotation = dummyrotation * Quaternion.Euler(45, 0, 0);
            return dummyrotation * Vector3.forward;
        }

        public static void HandleSkyBox()
        {
            if  (CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.FirstPerson)
            {
                if (!SceneSkybox)
                {
                    Kingmaker.Visual.LocalMap.LocalMapArea closest = Kingmaker.Visual.LocalMap.LocalMapArea.GetClosest(Game.Instance.Player.MainCharacter.Value.GetPosition());

                    if (closest.AreaPart != null)
                    {
                        if (!closest.AreaPart.IsIndoor)
                        {
                            // ADD A SKYBOX
                            CameraManager.AddSkyBox();
                        }
                    }
                }
                

            }
            else if (SceneSkybox)
            {
                // DESTROY THE PREVIOUS SKYBOX
                UnityEngine.Object.Destroy(SceneSkybox);
            }

        }



        public enum VRCameraMode
        {
            DemeoLike,
            FirstPerson,
            Cutscene,
            UI
        }

        //Strictly camera stuff
        public static VRCameraMode CurrentCameraMode;
        public static float NearClipPlaneDistance = 0.1f;
        public static float FarClipPlaneDistance = 59999f;
        public static bool DisableParticles = false;

        // VR Origin and body stuff
        public static Transform OriginalCameraParent = null;
        public static GameObject VROrigin = new GameObject();
        public static GameObject LeftHand = null;
        public static GameObject RightHand = null;
        public static Kingmaker.EntitySystem.Entities.UnitEntityData PrevFirstPerson = null;

        // VR Input stuff
        public static bool RightHandGrab = false;
        public static bool LeftHandGrab = false;
        public static Vector2 LeftJoystick = Vector2.zero;
        public static Vector2 RightJoystick = Vector2.zero;

        // Demeo-like camera stuff
        public static float InitialHandDistance = 0f;
        public static bool InitialRotation = true;
        public static Vector3 PreviousRotationVector = Vector3.zero;
        public static Vector3 InitialRotationPoint = Vector3.zero;
        public static Vector3 ZoomOrigin = Vector3.zero;
        public static float SpeedScalingFactor = 1f;

        // FIrst person camera stuff
        public static float Turnrate = 3f;

        //SKybox stuff
        public static GameObject SceneSkybox = null;

    }
    
}
