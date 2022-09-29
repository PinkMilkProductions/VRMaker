using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Profiling;
using Kingmaker;


namespace VRMaker
{
    [HarmonyPatch]
    class CameraPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.View.CameraRig), nameof(Kingmaker.View.CameraRig.OnEnable))]
        private static void FixNearClipping()
        {
            CameraManager.ReduceNearClipping();
            if (Plugin.HMDModel == "Vive MV")
            {
                Camera CurrentCamera = Game.GetCamera();
                CurrentCamera.GetComponent<Kingmaker.Visual.FogOfWar.FogOfWarScreenSpaceRenderer>().enabled = false;
            }
            
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.Visual.GammaAdjustment), nameof(Kingmaker.Visual.GammaAdjustment.Start))]
        private static void FixGamma(Kingmaker.Visual.GammaAdjustment __instance)
        {
            //Disables Gamma adjustment, temp fix to prevent both eyes having different gamma
            __instance.enabled = false;
        }

        // PERFORMANCE PATCHES - BEGIN

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Kingmaker.Visual.Lighting.ClusteredRenderer), nameof(Kingmaker.Visual.Lighting.ClusteredRenderer.OnPreRender))]
        private static void FixCulling(Kingmaker.Visual.Lighting.ClusteredRenderer __instance)
        {
            //return __instance.m_CullingGroup != null;
            if (__instance.m_CullingGroup == null)
            {
                __instance.OnPreCull();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Camera), "fieldOfView", MethodType.Setter)]
        private static bool suppressWarnings(Camera __instance)
        {
            if (__instance.stereoEnabled)
            {
                return false;
            }
            return true;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.Game), nameof(Kingmaker.Game.OnAreaLoaded))]
        private static void AreaLoadThings()
        {
            // Conditional, for performance testing
            if (CameraManager.DisableParticles)
            {
                var FoundParticles = UnityEngine.Object.FindObjectsOfType<ParticleSystem>();
                foreach (ParticleSystem Particle in FoundParticles)
                {
                    Particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
                Kingmaker.Visual.Particles.FxHelper.DestroyAll();
            }
            
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ParticleSystem), nameof(ParticleSystem.Play), new Type[] { })]
        private static void DisableParticles2(ParticleSystem __instance)
        {
            if (CameraManager.DisableParticles)
                __instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ParticleSystem), nameof(ParticleSystem.Play), new Type[] { typeof(bool) })]
        private static void DisableParticles3(ParticleSystem __instance)
        {
            if (CameraManager.DisableParticles)
                __instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // PERFORMANCE PATCHES - END

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Game), "ControllerMode", MethodType.Getter)]
        private static bool ForceGamePad(Game __instance)
        {
            __instance.m_ControllerMode = Game.ControllerModeType.Gamepad;
            return true;
        }

        //Movement (deprecated now that we are remapping inputs)
        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Kingmaker.UI._ConsoleUI.InputLayers.InGameLayer.InGameInputLayer), nameof(Kingmaker.UI._ConsoleUI.InputLayers.InGameLayer.InGameInputLayer.UpdateMovement))]
        private static void PassJoyStickMoveInput(Kingmaker.UI._ConsoleUI.InputLayers.InGameLayer.InGameInputLayer __instance)
        {
            if (CameraManager.LeftJoystick != Vector2.zero)
                __instance.m_MoveVector = CameraManager.LeftJoystick;
        }
        */

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.Game), nameof(Kingmaker.Game.Tick))]
        private static void HandleCamera()
        {
            if (CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.FirstPerson)
            {
                CameraManager.HandleFirstPersonCamera();
            }
            else
            {
                CameraManager.HandleDemeoCamera();
            }
        }

    }

}
