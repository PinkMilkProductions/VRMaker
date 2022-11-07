using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMaker
{
    class AssetLoader
    {
        private const string assetsDir = "VRMakerAssets/AssetBundles/";

        public static GameObject Skybox;
        public static GameObject LeftHandBase;
        public static GameObject RightHandBase;

        public AssetLoader()
        {
            var SkyboxBundle = LoadBundle("skyboxassetbundle");
            Skybox = LoadAsset<GameObject>(SkyboxBundle, "CustomAssets/SkyboxGO.prefab");
            LeftHandBase = LoadAsset<GameObject>(SkyboxBundle, "SteamVR/Prefabs/vr_glove_left_model_slim.prefab");
            RightHandBase = LoadAsset<GameObject>(SkyboxBundle, "SteamVR/Prefabs/vr_glove_right_model_slim.prefab");
        }

        private T LoadAsset<T>(AssetBundle bundle, string prefabName) where T : UnityEngine.Object
        {
            var asset = bundle.LoadAsset<T>($"Assets/{prefabName}");
            if (asset)
                return asset;
            else
            {
                Logs.WriteError($"Failed to load asset {prefabName}");
                return null;
            }
                
        }

        private static AssetBundle LoadBundle(string assetName)
        {
            var myLoadedAssetBundle =
                AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, Path.Combine(assetsDir, assetName)));

            Logs.WriteInfo("PluginPath: " + Paths.PluginPath);
            if (myLoadedAssetBundle == null)
            {
                Logs.WriteError($"Failed to load AssetBundle {assetName}");
                return null;
            }

            return myLoadedAssetBundle;
        }

    }
}
