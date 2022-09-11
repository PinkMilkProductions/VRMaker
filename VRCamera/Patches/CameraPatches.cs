using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Kingmaker;


namespace VRMaker
{
    [HarmonyPatch]
    class CameraPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.View.CameraRig), "OnEnable")]
        private static void FixNearClipping()
        {
            CameraManager.ReduceNearClipping();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.UI.KeyboardAccess), "Tick")]
        private static void CheckForPerspectiveToggle()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Logs.WriteInfo("F1 pressed");

                if (!F1Pressed)
                {
                    F1Pressed = true;

                    Camera MyCamera = Game.GetCamera();
                    // If we are not in firstperson
                    if (CameraManager.CurrentCameraMode != CameraManager.VRCameraMode.FirstPerson)
                    {
                        if (Game.Instance.Player.MainCharacter != null)
                        {
                            // switch to first person
                            VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.GetPosition();
                            OriginalCameraParent = MyCamera.transform.parent;
                            MyCamera.transform.parent = VROrigin.transform;
                            CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.FirstPerson;
                        }
                        
                    }
                    else
                    {
                        MyCamera.transform.parent = OriginalCameraParent;
                        CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.DemeoLike;
                    }
                }
            }
            else
            {
                F1Pressed = false;
            }

            if (CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.FirstPerson)
            {
                if (Game.Instance.Player.MainCharacter != null)
                {
                    VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.GetPosition();
                }
                    
            }
        }

        static bool F1Pressed = false;
        static Transform OriginalCameraParent = null;
        static GameObject VROrigin = new GameObject();
    }

}
