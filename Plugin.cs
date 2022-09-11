using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Kingmaker;
using Valve.VR;

namespace VRMaker
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.HerrFristi.VRMods.VRMaker";
        public const string PLUGIN_NAME = "VRMaker";
        public const string PLUGIN_VERSION = "0.0.1";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            CameraManagerObj = new CameraManager();

            InitSteamVR();
        }

        private static void InitSteamVR()
        {
            SteamVR.Initialize();
            SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;
        }

        // For temporary debug stuff
        public void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Logger.LogInfo("F1 pressed");

                if (!F1Pressed)
                {
                    F1Pressed = true;

                    float clipdistance = CameraManagerObj.ReduceNearClipping();
                    Logs.WriteInfo("new near clipping distance: ");
                    Logs.WriteInfo(clipdistance);

                    Camera MyCamera = Game.GetCamera();
                    // If we are not in firstperson
                    if (CameraManagerObj.CurrentCameraMode != CameraManager.VRCameraMode.FirstPerson)
                    {
                        // switch to first person
                        VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.GetPosition();
                        OriginalCameraParent = MyCamera.transform.parent;
                        MyCamera.transform.parent = VROrigin.transform;
                        CameraManagerObj.CurrentCameraMode = CameraManager.VRCameraMode.FirstPerson;
                    }
                    else
                    {
                        MyCamera.transform.parent = OriginalCameraParent;
                        CameraManagerObj.CurrentCameraMode = CameraManager.VRCameraMode.DemeoLike;
                    }
                }
            }
            else
            {
                F1Pressed = false;
            }

            if (CameraManagerObj.CurrentCameraMode == CameraManager.VRCameraMode.FirstPerson)
            {
                VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.GetPosition();
            }
        }

        CameraManager CameraManagerObj;

        bool F1Pressed = false;
        Transform OriginalCameraParent = null;
        GameObject VROrigin = new GameObject();

    }

}
