using System.Reflection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEditor;
using Kingmaker;
using Rewired;
using Valve.VR;


namespace VRMaker
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.HerrFristi.VRMods.VRMaker";
        public const string PLUGIN_NAME = "VRMaker";
        public const string PLUGIN_VERSION = "0.0.4";

        public static string gameExePath = Process.GetCurrentProcess().MainModule.FileName;
        public static string gamePath = Path.GetDirectoryName(gameExePath);
        public static string HMDModel = "";


        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            new AssetLoader();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            InitSteamVR();

            Game.s_Instance.ControllerMode = Game.ControllerModeType.Gamepad;
        }

        private static void InitSteamVR()
        {
            SteamVR_Actions.PreInitialize();
            SteamVR.Initialize();
            SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;

            VRInputManager MyVRInputManager = new VRInputManager();

            //Get the type of HMD (for Pimax bugfixing)
            // PIMAX 5K Plus = Vive MV
            HMDModel = UnityEngine.XR.XRDevice.model;
            Logs.WriteInfo(HMDModel);

        }

    }

}
