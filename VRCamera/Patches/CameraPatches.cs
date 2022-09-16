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
            //Also test this postprocess disable thing
            //CameraManager.TurnOffPostProcessing();
            // Start performance profiler
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.Visual.GammaAdjustment), nameof(Kingmaker.Visual.GammaAdjustment.Start))]
        private static void FixGamma(Kingmaker.Visual.GammaAdjustment __instance)
        {
            //Disables Gamma adjustment, temp fix to prevent both eyes having different gamma
            __instance.enabled = false;
        }

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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.Game), nameof(Kingmaker.Game.OnAreaLoaded))]
        private static void DisableParticles()
        {
            var FoundParticles = UnityEngine.Object.FindObjectsOfType<ParticleSystem>();
            foreach (ParticleSystem Particle in FoundParticles)
            {
                Particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            Kingmaker.Visual.Particles.FxHelper.DestroyAll();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ParticleSystem), nameof(ParticleSystem.Play), new Type[] { })]
        private static void DisableParticles2(ParticleSystem __instance)
        {
            __instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ParticleSystem), nameof(ParticleSystem.Play), new Type[] { typeof(bool) })]
        private static void DisableParticles3(ParticleSystem __instance)
        {
            __instance.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.UI.KeyboardAccess), nameof(Kingmaker.UI.KeyboardAccess.Tick))]
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
