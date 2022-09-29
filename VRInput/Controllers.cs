﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using Rewired;
//using RoR2;
//using RoR2.GamepadVibration;
//using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
//using VRMod.Inputs;
//using VRMod.Inputs.Legacy;

namespace VRMaker
{
    public class Controllers
    {
        private static CustomController vrControllers;
        private static CustomControllerMap vrGameplayMap;
        private static CustomControllerMap vrUIMap;

        private static bool hasRecentered;
        private static bool initializedMainPlayer;
        private static bool initializedLocalUser;

        private static BaseInput[] inputs;
        private static List<BaseInput> modInputs = new List<BaseInput>();

        internal static int leftJoystickID { get; private set; }
        internal static int rightJoystickID { get; private set; }

        internal static int ControllerID => vrControllers.id;

        internal static void Init()
        {
            ReInput.InputSourceUpdateEvent += UpdateVRInputs;

            RoR2Application.onUpdate += Update;

            On.RoR2.UI.InputBindingControl.Awake += DisableControllerBinds;


            IL.RoR2.PlayerCharacterMasterController.Update += ControllerMovementDirection;

            On.RoR2.UI.MainMenu.ProfileMainMenuScreen.SetMainProfile += (orig, self, profile) =>
            {
                orig(self, profile);
                if (initializedLocalUser)
                {
                    initializedLocalUser = false;
                    RoR2Application.onUpdate += Update;
                }
            };

            SetupControllerInputs();
        }

        



        internal static void ApplyRemaps(string bodyName)
        {
            if (!skillBindingOverrides.Exists(x => x.bodyName == bodyName))
            {
                VRMod.StaticLogger.LogInfo(String.Format("No binding overrides found for \'{0}\'. Using default binding.", bodyName));
                RevertRemap();
                return;
            }

            VRMod.StaticLogger.LogInfo(String.Format("Binding overrides were found for \'{0}\'. Applying overrides.", bodyName));
            SkillBindingOverride bindingOverride = skillBindingOverrides.FirstOrDefault(x => x.bodyName == bodyName);

            if (bindingOverride.bodyName == bodyName)
            {
                int[] originalSkillBindingIDs = new int[]
                {
                    (ModConfig.LeftDominantHand.Value ? 9 : 8),
                    (ModConfig.LeftDominantHand.Value ? 8 : 9),
                    (ModConfig.LeftDominantHand.Value ? 11 : 10),
                    (ModConfig.LeftDominantHand.Value ? 10 : 11)
                };

                ActionElementMap[] newMapOrder = new ActionElementMap[]
                {
                    vrGameplayMap.GetElementMapsWithAction(7 + (int)bindingOverride.dominantTrigger)[0],
                    vrGameplayMap.GetElementMapsWithAction(7 + (int)bindingOverride.nonDominantTrigger)[0],
                    vrGameplayMap.GetElementMapsWithAction(7 + (int)bindingOverride.nonDominantGrip)[0],
                    vrGameplayMap.GetElementMapsWithAction(7 + (int)bindingOverride.dominantGrip)[0]
                };

                ControllerMap controllerMap = Utils.localInputPlayer.controllers.maps.GetMap(vrControllers, vrGameplayMap.id);

                for (int i = 0; i < 4; i++)
                {
                    ActionElementMap elementMap = newMapOrder[i];

                    if (elementMap.elementIdentifierId == originalSkillBindingIDs[i]) continue;

                    if (!controllerMap.ReplaceElementMap(elementMap.id, elementMap.actionId, elementMap.axisContribution, originalSkillBindingIDs[i], elementMap.elementType, elementMap.axisRange, elementMap.invert))
                    {
                        VRMod.StaticLogger.LogError("An error occured while trying to override skill bindings.");
                    }
                }
            }
        }

        internal static void RevertRemap()
        {
            ControllerMap map = Utils.localInputPlayer.controllers.maps.GetMap(vrControllers, vrGameplayMap.id);

            int[] originalSkillBindingIDs = new int[]
            {
                (ModConfig.LeftDominantHand.Value ? 9 : 8),
                (ModConfig.LeftDominantHand.Value ? 8 : 9),
                (ModConfig.LeftDominantHand.Value ? 11 : 10),
                (ModConfig.LeftDominantHand.Value ? 10 : 11)
            };

            for (int i = 0; i < 4; i++)
            {
                ActionElementMap elementMap = vrGameplayMap.GetElementMapsWithAction(7 + i)[0];

                if (elementMap.elementIdentifierId == originalSkillBindingIDs[i]) continue;

                if (!map.ReplaceElementMap(elementMap.id, elementMap.actionId, elementMap.axisContribution, originalSkillBindingIDs[i], elementMap.elementType, elementMap.axisRange, elementMap.invert))
                {
                    VRMod.StaticLogger.LogError("An error occured while trying to revert skill binding overrides.");
                }
            }
        }

        internal static void ChangeDominanceDependantMaps()
        {
            Player player = Utils.localInputPlayer;

            for (int i = 7; i < 11; i++)
            {
                ActionElementMap map = vrGameplayMap.GetElementMapsWithAction(i)[0];
                int elementIdentifier = map.elementIdentifierId;

                if (elementIdentifier == 8) elementIdentifier = 9;
                else if (elementIdentifier == 9) elementIdentifier = 8;
                else if (elementIdentifier == 10) elementIdentifier = 11;
                else if (elementIdentifier == 11) elementIdentifier = 10;

                bool result = player.controllers.maps.GetMap(vrControllers, vrGameplayMap.id).ReplaceElementMap(map.id, map.actionId, map.axisContribution, elementIdentifier, map.elementType, map.axisRange, map.invert);

                if (!result)
                {
                    VRMod.StaticLogger.LogError("Failed to remap");
                    return;
                }
            }
        }

        private static void ControllerMovementDirection(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(x => x.MatchStloc(5));
            c.EmitDelegate<Func<Transform, Transform>>((headTransform) =>
            {
                if (!ModConfig.ControllerMovementDirection.Value) return headTransform;

                if (MotionControls.HandsReady)
                {
                    // If controller movement tracking is enabled, replace the base camera transform with the controller transform.
                    return MotionControls.GetHandByDominance(false).muzzle.transform;
                }
                else
                {
                    // See below for handling of the case where hands are not ready.
                    return headTransform;
                }
            });

            c.GotoNext(x => x.MatchLdloca(6));
            c.GotoNext(x => x.MatchLdloca(6));

            c.Emit(OpCodes.Ldloc_S, (byte)6);
            c.EmitDelegate<Func<Vector2, Vector2>>((vector) =>
            {
                if (!ModConfig.ControllerMovementDirection.Value || MotionControls.HandsReady) return vector;

                // Special case only for if hands are not ready; in this case, we can't assume a hand transform exists, so we fall back to old logic.
                // Note: this will only provide y-axis rotation (yaw); z-axis rotation (pitch) will still be controlled by the head.

                Quaternion controllerRotation = InputTracking.GetLocalRotation(XRNode.LeftHand);
                Quaternion headRotation = Camera.main.transform.localRotation;

                float angleDifference = headRotation.eulerAngles.y - controllerRotation.eulerAngles.y;

                return Quaternion.Euler(new Vector3(0, 0, angleDifference)) * vector;
            });
            c.Emit(OpCodes.Stloc_S, (byte)6);

            c.GotoNext(x => x.MatchCallvirt<Transform>("get_right"));
            c.Index += 1;
            c.EmitDelegate<Func<Vector3, Vector3>>((vector) =>
            {
                if (!ModConfig.ControllerMovementDirection.Value || !MotionControls.HandsReady) return vector;
                // In flight mode, clamp the controller's left-right vector to the xz plane (normal to Vector3.up) so that only pitch and yaw are affected, not roll.
                return Vector3.ProjectOnPlane(vector, Vector3.up).normalized * vector.magnitude;
            });
        }


        private static void DisableControllerBinds(On.RoR2.UI.InputBindingControl.orig_Awake orig, InputBindingControl self)
        {
            orig(self);

            if (ModConfig.InitialMotionControlsValue && self.inputSource == MPEventSystem.InputSource.Gamepad && self.button)
            {
                self.button.interactable = false;
                self.button = null;
            }
        }

        private static void SetupControllerInputs()
        {
            vrControllers = RewiredAddons.CreateRewiredController();
            vrUIMap = RewiredAddons.CreateUIMap(vrControllers.id);
            vrGameplayMap = RewiredAddons.CreateGameplayMap(vrControllers.id);

                inputs = new BaseInput[]
                {
                    new VectorInput(SteamVR_Actions.gameplay_Move, 0, 1),
                    new SimulatedVectorInput(SteamVR_Actions.gameplay_Look, 2, 3, null, SteamVR_Actions.gameplay_LookRight, null, SteamVR_Actions.gameplay_LookLeft),
                    new VectorInput(SteamVR_Actions.ui_Navigate, 4, 5),
                    new ButtonInput(SteamVR_Actions.gameplay_Interact, 6),
                    new ButtonInput(SteamVR_Actions.gameplay_Jump, 7),
                    new ButtonInput(SteamVR_Actions.gameplay_PrimarySkill, 8),
                    new ButtonInput(SteamVR_Actions.gameplay_SecondarySkill, 9),
                    new ButtonInput(SteamVR_Actions.gameplay_UtilitySkill, 10),
                    new ButtonInput(SteamVR_Actions.gameplay_SpecialSkill, 11),
                    new ButtonInput(SteamVR_Actions.gameplay_UseEquipment, 12),
                    new ButtonInput(SteamVR_Actions.gameplay_Sprint, 13),
                    new ButtonInput(SteamVR_Actions.gameplay_Ping, 14),
                    new HoldableButtonInput(SteamVR_Actions.gameplay_ScoreboardAndProfile, 15, SteamVR_Actions.gameplay_HoldScoreboardAndProfile),
                    new ButtonInput(SteamVR_Actions.ui_Submit, 17),
                    new ButtonInput(SteamVR_Actions.ui_Cancel, 18),
                    new ButtonInput(SteamVR_Actions.ui_ReadyAndContinue, 19),
                    new ButtonInput(SteamVR_Actions.ui_TabLeft, 20),
                    new ButtonInput(SteamVR_Actions.ui_TabRight, 21),
                    new ButtonInput(SteamVR_Actions.ui_SubmenuLeft, 22),
                    new ButtonInput(SteamVR_Actions.ui_SubmenuRight, 23),
                    new ReleaseButtonInput(SteamVR_Actions.ui_Pause, 24),
                    new ButtonInput(SteamVR_Actions.ui_RecenterHMD, 25)
                };
        }

        private static void Update()
        {
            if (!initializedMainPlayer)
            {
                if (AddVRController(LocalUserManager.GetRewiredMainPlayer()))
                    initializedMainPlayer = true;
            }

            LocalUser localUser = LocalUserManager.GetFirstLocalUser();

            if (localUser != null)
            {
                if (AddVRController(localUser.inputPlayer))
                {
                    initializedLocalUser = true;
                    RoR2Application.onUpdate -= Update;
                }
            }
        }

        internal static bool AddVRController(Player inputPlayer)
        {
            if (!inputPlayer.controllers.ContainsController(vrControllers))
            {
                inputPlayer.controllers.AddController(vrControllers, false);
                vrControllers.enabled = true;
            }

            if (inputPlayer.controllers.maps.GetAllMaps(ControllerType.Custom).ToList().Count < 2)
            {
                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, 2, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrUIMap);
                if (inputPlayer.controllers.maps.GetMap(ControllerType.Custom, vrControllers.id, 0, 0) == null)
                    inputPlayer.controllers.maps.AddMap(vrControllers, vrGameplayMap);
                if (!vrGameplayMap.enabled)
                    vrGameplayMap.enabled = true;
                if (!vrUIMap.enabled)
                    vrUIMap.enabled = true;
            }

            return inputPlayer.controllers.ContainsController(vrControllers) && inputPlayer.controllers.maps.GetAllMaps(ControllerType.Custom).ToList().Count >= 2;
        }

        private static void UpdateVRInputs()
        {
            if (ModConfig.InitialOculusModeValue)
            {
                string[] joyNames = Input.GetJoystickNames();

                if (leftJoystickID != -1 && (leftJoystickID >= joyNames.Length || joyNames[leftJoystickID] == null || !joyNames[leftJoystickID].ToLower().Contains("left")))
                    leftJoystickID = -1;

                if (rightJoystickID != -1 && (rightJoystickID >= joyNames.Length || joyNames[rightJoystickID] == null || !joyNames[rightJoystickID].ToLower().Contains("right")))
                    rightJoystickID = -1;

                if (leftJoystickID == -1)
                {
                    for (int i = 0; i < joyNames.Length; i++)
                    {
                        string joyName = joyNames[i].ToLower();
                        if (joyName.Contains("left"))
                        {
                            leftJoystickID = i;
                        }
                    }
                }

                if (rightJoystickID == -1)
                {
                    for (int i = 0; i < joyNames.Length; i++)
                    {
                        string joyName = joyNames[i].ToLower();
                        if (joyName.Contains("right"))
                        {
                            rightJoystickID = i;
                        }
                    }
                }

                if (leftJoystickID == -1 || rightJoystickID == -1) return;

                for (int i = 0; i < vrControllers.elementCount; i++)
                {
                    if (i < vrControllers.axisCount) vrControllers.ClearAxisValueById(i);
                    else vrControllers.ClearButtonValueById(i);
                }
            }

            foreach (BaseInput input in inputs)
            {
                input.UpdateValues(vrControllers);
            }

            foreach (BaseInput input in modInputs)
            {
                input.UpdateValues(vrControllers);
            }
        }
    }
}