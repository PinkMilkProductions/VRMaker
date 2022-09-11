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
        public const string PLUGIN_VERSION = "0.0.3";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

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

                    Camera MyCamera = Game.GetCamera();
                    // If we are not in firstperson
                    if (CameraManager.CurrentCameraMode != CameraManager.VRCameraMode.FirstPerson)
                    {
                        // switch to first person
                        VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.GetPosition();
                        OriginalCameraParent = MyCamera.transform.parent;
                        MyCamera.transform.parent = VROrigin.transform;
                        CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.FirstPerson;
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
                VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.GetPosition();
            }
        }

        bool F1Pressed = false;
        Transform OriginalCameraParent = null;
        GameObject VROrigin = new GameObject();

    }

}
