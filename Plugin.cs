using BepInEx;
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
            InitSteamVR();
        }

        private static void InitSteamVR()
        {
            SteamVR.Initialize();
            SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;
        }

        public void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                // switch to first person
                Camera MyCamera = Game.GetCamera();
                DummyObject.transform.position = Game.Instance.Player.MainCharacter.Value.GetPosition();
                MyCamera.transform.parent = DummyObject.transform;
                Firstperson = true;
            }

            if (Firstperson)
            {
                DummyObject.transform.position = Game.Instance.Player.MainCharacter.Value.GetPosition();
            }

        }

        GameObject DummyObject = new GameObject();
        bool Firstperson = false;
    }

}
