using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace VRMaker
{
    public class VRInputManager
    {
        static VRInputManager()
        {
            SetUpListeners();
        }

        public static void SetUpListeners()
        {
            SteamVR_Actions._default.LeftTrigger.AddOnStateDownListener(TriggerLeftDown, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.RightGrab.AddOnStateDownListener(GrabRightDown, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.RightGrab.AddOnStateUpListener(GrabRightUp, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.LeftGrab.AddOnStateDownListener(GrabLeftDown, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.LeftGrab.AddOnStateUpListener(GrabLeftUp, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.SwitchPOV.AddOnStateDownListener(OnSwitchPOVDown, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.SwitchPOV.AddOnStateUpListener(OnSwitchPOVUp, SteamVR_Input_Sources.Any);

            SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateRightHand);
            SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateLeftHand);
        }




        public static void OnSwitchPOVDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            CameraManager.SwitchPOV();
            CameraManager.SpawnHands();
        }
        public static void OnSwitchPOVUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            Logs.WriteInfo("SwitchPOVButton is UP");
        }

        public static void TriggerLeftDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            Logs.WriteInfo("TriggerLeft is Down");
        }

        public static void GrabRightDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            CameraManager.RightHandGrab = true;
        }

        public static void GrabRightUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            CameraManager.RightHandGrab = false;
        }

        public static void GrabLeftDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            CameraManager.LeftHandGrab = true;
        }

        public static void GrabLeftUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            CameraManager.LeftHandGrab = false;
        }

        public static void UpdateRightHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
        {
            if (CameraManager.RightHand)
            {
                CameraManager.RightHand.transform.localPosition = fromAction.localPosition;
            }

        }

        public static void UpdateLeftHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
        {
            if (CameraManager.LeftHand)
            {
                CameraManager.LeftHand.transform.localPosition = fromAction.localPosition;
            }
        }
    }
}
