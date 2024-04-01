using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Kingmaker;
using Kingmaker.TurnBasedMode;
using Kingmaker.Utility;
using Rewired.ComponentControls.Effects;
using Kingmaker.Visual;

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
            "ExplorerCanvas",
            //"LoadingScreen"  // The LoadingScreen
        };

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Kingmaker.UI.UICanvas), "Start")]
        private static bool MoveCanvasesToWorldSpace(Kingmaker.UI.UICanvas __instance)
        {
            try
            {
                // This check for !Camera.main needs to stay here,
                // because without it the map texture will some times be broken. Dunno why.
                //if (!Camera.main || IsCanvasToIgnore(__instance.name)) return true;
                float UIScale = 0.00045f;

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
                if (canvas.name == "LoadingScreen")
                {
                    UIScale = 10;
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
                AttachedUi.Create<StaticUi>(canvas, MyUiTarget.TargetTransform, UIScale);

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
        private static bool WorldUIMarkersFixScreen(Vector3 position, ref Vector3 __result)
        {
            __result = position;
            return false;
        }

        // THIS WORKS FOR THE CHARACTER MARKERS, PUTS THEM IN A 3D POSITION INDEPENDANT OF THE CONSOLE_STATICCANVAS
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.UI.PointMarker.PointMarker), nameof(Kingmaker.UI.PointMarker.PointMarker.CalculateMarkerPosition))]
        private static void CharacterMarkersToWorld(Kingmaker.UI.PointMarker.PointMarker __instance)
        {
            __instance.transform.position = __instance.Character.Position;
        }

        // THIS WORKS FOR OVERTIPS, PUTS THEM IN A 3D POSITION INDEPENDANT OF THE CONSOLE_DYNAMICCANVAS
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.UI._ConsoleUI.Overtips.OvertipViewBase), nameof(Kingmaker.UI._ConsoleUI.Overtips.OvertipViewBase.UpdatePosition))]
        private static void OvertipsToWorld(Kingmaker.UI._ConsoleUI.Overtips.OvertipViewBase __instance)
        {
            if (__instance.ViewModel.EntityPosition != null)
            {
                __instance.transform.position = __instance.ViewModel.EntityPosition;
                if (CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.FirstPerson)
                    __instance.transform.localScale = Vector3.one * 5;
                else
                    __instance.transform.localScale = Vector3.one * 20;
            }

        }

        // HOLY SHIT THIS FIXES THE COMBAT PATH RENDERING
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.TurnBasedMode.PathVisualizer), nameof(Kingmaker.TurnBasedMode.PathVisualizer.Update))]
        private static void FixCombatPathRendering(Kingmaker.TurnBasedMode.PathVisualizer __instance)
        {
            __instance.m_Renderer.material.shader = Shader.Find("Hidden/Internal-Colored");
            if (CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.FirstPerson)
            {
                __instance.m_Renderer.startWidth = 25.0f;
                __instance.m_Renderer.endWidth = 25.0f;
            }
            else
            {
                __instance.m_Renderer.startWidth = 100.0f;
                __instance.m_Renderer.endWidth = 100.0f;
            }
        }

        // Camera patch specifically for controlling the Turnbased Combat Movement
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnityEngine.Camera), nameof(UnityEngine.Camera.ScreenPointToRay), new[] { typeof(Vector3)})]
        private static bool VRMouseRaycast(ref UnityEngine.Ray __result)
        {

            if (CameraManager.RightHand)
            {
                __result = new Ray(CameraManager.RightHand.transform.position, CameraManager.GetRightHandForward());
                return false;
            }
            else
            {
                __result = new Ray(Game.GetCamera().transform.position, Game.GetCamera().transform.forward);
                return false;
            }

        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(Kingmaker.TurnBasedMode.PathVisualizer), nameof(Kingmaker.TurnBasedMode.PathVisualizer.UpdateVisualPath), new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(int), typeof(PathVisualizer.VisualPathSettings) })]
        private static bool LineRendererToWorldspacePatch(Kingmaker.TurnBasedMode.PathVisualizer __instance)
        {
            __instance.m_Renderer.useWorldSpace = true;
            return true;
        }

        // Attempt to fix the Kingdom Events UI
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Kingmaker.UI._ConsoleUI.Kingdom.Events.KingdomEventDetailsView), nameof(Kingmaker.UI._ConsoleUI.Kingdom.Events.KingdomEventDetailsView.PlayAnimation))]
        private static bool FixKingdomEventUI(bool state, Kingmaker.UI._ConsoleUI.Kingdom.Events.KingdomEventDetailsView __instance)
        {
            __instance.gameObject.SetActive(state);
            if (state)
            {
                Canvas Console_StaticCanvas = GameObject.Find("Console_StaticCanvas").GetComponent<Canvas>();
                Console_StaticCanvas.transform.localPosition = Vector3.zero;
                Console_StaticCanvas.transform.localScale = Vector3.one;
            }
            //__instance.m_Window.localPosition = Vector3.zero;
            //__instance.m_Window.localScale = Vector3.one;
            //__instance.m_Window.localRotation = Quaternion.identity;
            return true;
        }

        // Fancy tweening hook that doesn't work yet
        // Attempt to fix the Kingdom Events UI Camera moving to the leaders
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Kingmaker.Controllers.KingdomCameraController), nameof(Kingmaker.Controllers.KingdomCameraController.StartCameraTween), new[] { typeof(Transform)})]
        //private static void FixKingdomLeadersCameraPanning(Transform anchor, Kingmaker.Controllers.KingdomCameraController __instance)
        //{
        //    Logs.WriteInfo("CameraPanning Hook activated!");
        //    if (Game.GetCamera().transform.parent != CameraManager.VROrigin.transform)
        //    {
        //        Logs.WriteInfo("VROrigin wasn't the parent of the tweening camera, setting it now...");
        //        CameraManager.VROrigin.transform.position = Game.GetCamera().transform.parent.position;
        //        CameraManager.VROrigin.transform.rotation = Game.GetCamera().transform.parent.rotation;
        //        CameraManager.VROrigin.transform.localScale = Game.GetCamera().transform.parent.localScale;
        //        Game.GetCamera().transform.parent = CameraManager.VROrigin.transform;
        //    }
        //    __instance.m_CurrentTween = anchor.GetComponent<CameraTweenAnchor>();
        //    if (__instance.m_CurrentTween)
        //    {
        //        Logs.WriteInfo("CameraPanning Hook Tweening check succesful!");
        //        __instance.m_CurrentTween.StartTween(CameraManager.VROrigin.transform, __instance.m_PrevTween);
        //    }
        //}

        // Attempt to fix the Kingdom Events UI Camera moving to the leaders
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.Controllers.KingdomCameraController), nameof(Kingmaker.Controllers.KingdomCameraController.StartCameraTween), new[] { typeof(Transform) })]
        private static void FixKingdomLeadersCameraPanning(Transform anchor, Kingmaker.Controllers.KingdomCameraController __instance)
        {
            Logs.WriteInfo("CameraPanning Hook activated!");
            if (Game.GetCamera().transform.parent != CameraManager.VROrigin.transform)
            {
                Logs.WriteInfo("VROrigin wasn't the parent of the tweening camera, setting it now...");
                CameraManager.VROrigin.transform.position = Game.GetCamera().transform.parent.position;
                CameraManager.VROrigin.transform.rotation = Game.GetCamera().transform.parent.rotation;
                CameraManager.VROrigin.transform.localScale = Game.GetCamera().transform.parent.localScale;
                Game.GetCamera().transform.parent = CameraManager.VROrigin.transform;
            }
            CameraManager.VROrigin.transform.position = anchor.position;
            CameraManager.VROrigin.transform.rotation.SetEulerAngles(0, anchor.rotation.eulerAngles.y, 0);
        }
    }
}
