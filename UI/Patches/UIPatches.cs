using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Kingmaker;

namespace VRMaker
{
    [HarmonyPatch]
    class UIPatches
    {
        private static readonly string[] canvasesToDisable =
        {
            "BlackBars", // Cinematic black bars.
            "Camera" // Disposable camera.
            //"Canvas", // This is used for the loading screen, do not disable
            //"Console_StaticCanvas" // This is the main ingame HUD that behaves weirdly with our code
        };

        private static readonly string[] canvasesToIgnore =
        {
            "com.sinai.unityexplorer_Root", // UnityExplorer.
            "com.sinai.unityexplorer.MouseInspector_Root", // UnityExplorer.
            "ExplorerCanvas"
        };

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Kingmaker.UI.UICanvas), "Start")]
        private static bool MoveCanvasesToWorldSpace(Kingmaker.UI.UICanvas __instance)
        {
            try
            {
                // This check for !Camera.main needs to stay here,
                // because without it the map texture will some times be broken. Dunno why.
                //if (!Camera.main || IsCanvasToIgnore(__instance.name)) return;

                if (!Plugin.MyHelper)
                    Plugin.MyHelper = MBHelper.Create();

                var canvas = __instance.GetComponent<Canvas>(); ;

                if (!canvas) return true;

                if (IsCanvasToDisable(canvas.name))
                {
                    canvas.enabled = false;
                    return true;
                }

                Logs.WriteInfo("Current Canvas name: " + canvas.name);

                if (canvas.name == "Console_StaticCanvas")
                {
                    //canvas.enabled = false;
                    //canvas.enabled = true;
                    //canvas.transform.parent = null;
                }

                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) return true;

                //LayerHelper.SetLayer(canvas, GameLayer.UI);

                // Canvases with graphic raycasters are the ones that receive click events.
                // Those need to be handled differently, with colliders for the laser ray.
                //if (canvas.GetComponent<GraphicRaycaster>())
                //    AttachedUi.Create<InteractiveUi>(canvas, StageInstance.GetInteractiveUiTarget(), 0.002f);
                //else
                StaticUiTarget MyUiTarget = StaticUiTarget.Create(Plugin.MyHelper.gameObject.transform);
                //MyUiTarget.SetUp(Kingmaker.Game.GetCamera());
                MyUiTarget.SetUp(Kingmaker.Game.GetCamera());
                AttachedUi.Create<StaticUi>(canvas, MyUiTarget.TargetTransform, 0.00045f);

                return false;

                //// Test
                //Controllers.Init();
            }
            catch (Exception exception)
            {
                Logs.WriteWarning($"Failed to move canvas to world space ({__instance.name}): {exception}");
                return true;
            }
        }

        private static bool IsCanvasToIgnore(string canvasName)
        {
            foreach (var s in canvasesToIgnore)
                if (Equals(s, canvasName))
                    return true;
            return false;
        }

        private static bool IsCanvasToDisable(string canvasName)
        {
            foreach (var s in canvasesToDisable)
                if (Equals(s, canvasName))
                    return true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnityEngine.Camera), nameof(UnityEngine.Camera.WorldToScreenPoint), new[] { typeof(Vector3) })]
        private static bool WorldUIMarkersFix(Vector3 position, ref Vector3 __result)
        {
            __result = position;
            return true;
        }
    }
}
