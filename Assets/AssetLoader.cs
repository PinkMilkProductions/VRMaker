using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace VRMaker
{
    class AssetLoader
    {
        public static GameObject Skybox;
        public AssetLoader()
        {
            var SkyboxBundle = LoadBundle("skyboxassetbundle");
            Skybox = LoadAsset<GameObject>(SkyboxBundle, "SkyboxGO.prefab");
        }

        private T LoadAsset<T>(AssetBundle bundle, string prefabName) where T : UnityEngine.Object
        {
            //return bundle.LoadAsset<T>($"assets/{prefabName}");
            var asset = bundle.LoadAsset<T>($"Assets/CustomAssets/{prefabName}");
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
                AssetBundle.LoadFromFile(
                    $"{Plugin.gamePath}/VRMaker/Assets/{assetName}");
            if (myLoadedAssetBundle == null)
            {
                Logs.WriteError($"Failed to load AssetBundle {assetName}");
                return null;
            }

            return myLoadedAssetBundle;
        }

    }
}
