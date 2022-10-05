﻿using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace VRMaker
{
    internal static class RewiredAddons
    {
        
        internal static CustomController CreateRewiredController()
        {
            HardwareControllerMap_Game hcMap = new HardwareControllerMap_Game(
                "VRControllers",
                new ControllerElementIdentifier[]
                {
                    new ControllerElementIdentifier(0, "MoveX", "MoveXPos", "MoveXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(1, "MoveY", "MoveYPos", "MoveYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(2, "LeftStickX", "NavigateXPos", "NavigateXNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(3, "LeftStickY", "NavigateYPos", "NavigateYNeg", ControllerElementType.Axis, true),
                    new ControllerElementIdentifier(4, "action_confirm", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(5, "actionbar", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(6, "halt", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(7, "pause", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(8, "group", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(9, "menus", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(10, "Confirm", "", "", ControllerElementType.Button, true),
                    new ControllerElementIdentifier(11, "FuncAdditional", "", "", ControllerElementType.Button, true),
                },
                new int[] { },
                new int[] { },
                new AxisCalibrationData[]
                {
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true),
                    new AxisCalibrationData(true, 0.1f, 0, -1, 1, false, true)
                },
                new AxisRange[]
                {
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Full,
                    AxisRange.Full
                },
                new HardwareAxisInfo[]
                {
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None),
                    new HardwareAxisInfo(AxisCoordinateMode.Absolute, false, SpecialAxisType.None)
                },
                new HardwareButtonInfo[] { },
                null
            );

            ReInput.UserData.AddCustomController();
            CustomController_Editor newController = ReInput.UserData.customControllers.Last();
            newController.name = "VRControllers";
            foreach (ControllerElementIdentifier element in hcMap.elementIdentifiers.Values)
            {
                if (element.elementType == ControllerElementType.Axis)
                {
                    newController.AddAxis();
                    newController.elementIdentifiers.RemoveAt(newController.elementIdentifiers.Count - 1);
                    newController.elementIdentifiers.Add(element);
                    CustomController_Editor.Axis newAxis = newController.axes.Last();
                    newAxis.name = element.name;
                    newAxis.elementIdentifierId = element.id;
                    newAxis.deadZone = hcMap.hwAxisCalibrationData[newController.axisCount - 1].deadZone;
                    newAxis.zero = 0;
                    newAxis.min = hcMap.hwAxisCalibrationData[newController.axisCount - 1].min;
                    newAxis.max = hcMap.hwAxisCalibrationData[newController.axisCount - 1].max;
                    newAxis.invert = hcMap.hwAxisCalibrationData[newController.axisCount - 1].invert;
                    newAxis.axisInfo = hcMap.hwAxisInfo[newController.axisCount - 1];
                    newAxis.range = hcMap.hwAxisRanges[newController.axisCount - 1];
                }
                else if (element.elementType == ControllerElementType.Button)
                {
                    newController.AddButton();
                    newController.elementIdentifiers.RemoveAt(newController.elementIdentifiers.Count - 1);
                    newController.elementIdentifiers.Add(element);
                    CustomController_Editor.Button newButton = newController.buttons.Last();
                    newButton.name = element.name;
                    newButton.elementIdentifierId = element.id;
                }
            }

            CustomController customController = ReInput.controllers.CreateCustomController(newController.id);

            customController.useUpdateCallbacks = false;

            return customController;
        }

        internal static CustomControllerMap CreateUIMap(int controllerID)
        {
            List<ActionElementMap> uiElementMaps = new List<ActionElementMap>()
            {
                new ActionElementMap(0, ControllerElementType.Axis  , 2 , Pole.Positive, AxisRange.Full, false), //LeftStickX (UI X axis)
                new ActionElementMap(1, ControllerElementType.Axis  , 3 , Pole.Positive, AxisRange.Full, false), //LeftStickY (UI Y axis)
                new ActionElementMap(8, ControllerElementType.Button, 10 , Pole.Positive, AxisRange.Positive, false), //Confirm (UI Confirm)
                new ActionElementMap(17, ControllerElementType.Button, 11 , Pole.Positive, AxisRange.Positive, false) //FuncAdditional (UI back)
            };

            return CreateCustomMap("VRUI", 2, controllerID, uiElementMaps);
        }

        /*
        internal static CustomControllerMap CreateGameplayMap(int controllerID)
        {
            
            List<ActionElementMap> defaultElementMaps = new List<ActionElementMap>()
            {
                new ActionElementMap(0 , ControllerElementType.Axis  , 0 , Pole.Positive, AxisRange.Full, false), //MoveHor
                new ActionElementMap(1 , ControllerElementType.Axis  , 1 , Pole.Positive, AxisRange.Full, false), //MoveVer
                new ActionElementMap(16, ControllerElementType.Axis  , 2 , Pole.Positive, AxisRange.Full, false), //LookHor
                new ActionElementMap(17, ControllerElementType.Axis  , 3 , Pole.Positive, AxisRange.Full, false), //LookVer
                new ActionElementMap(4 , ControllerElementType.Button, 7 , Pole.Positive, AxisRange.Full, false), //Jump
                new ActionElementMap(5 , ControllerElementType.Button, 6, Pole.Positive, AxisRange.Full, false), //Interact
                new ActionElementMap(6 , ControllerElementType.Button, 12 , Pole.Positive, AxisRange.Full, false), //Equipment
                new ActionElementMap(7 , ControllerElementType.Button, 8 , Pole.Positive, AxisRange.Full, false), //Primary
                new ActionElementMap(8 , ControllerElementType.Button, 9 , Pole.Positive, AxisRange.Full, false), //Secondary
                new ActionElementMap(9 , ControllerElementType.Button, 10 , Pole.Positive, AxisRange.Full, false), //Utility
                new ActionElementMap(10, ControllerElementType.Button, 11 , Pole.Positive, AxisRange.Full, false), //Special
                new ActionElementMap(18, ControllerElementType.Button, 13, Pole.Positive, AxisRange.Full, false), //Sprint
                new ActionElementMap(19, ControllerElementType.Button, 15, Pole.Positive, AxisRange.Full, false), //Scoreboard or Profile
                new ActionElementMap(28, ControllerElementType.Button, 14, Pole.Positive, AxisRange.Full, false), //Ping
                new ActionElementMap(100, ControllerElementType.Button, 26, Pole.Positive, AxisRange.Full, false), //ExtraSkill1
                new ActionElementMap(101, ControllerElementType.Button, 27, Pole.Positive, AxisRange.Full, false), //ExtraSkill2
                new ActionElementMap(102, ControllerElementType.Button, 28, Pole.Positive, AxisRange.Full, false), //ExtraSkill3
                new ActionElementMap(103, ControllerElementType.Button, 29, Pole.Positive, AxisRange.Full, false), //ExtraSkill4
                new ActionElementMap(351, ControllerElementType.Button, 30, Pole.Positive, AxisRange.Full, false), //PushToTalk
                new ActionElementMap(400, ControllerElementType.Button, 31, Pole.Positive, AxisRange.Full, false) //BuySkill
            };

            return CreateCustomMap("VRDefault", 0, controllerID, defaultElementMaps);
            
        }
        */

        
        private static CustomControllerMap CreateCustomMap(string mapName, int categoryId, int controllerId, List<ActionElementMap> actionElementMaps)
        {
            ReInput.UserData.CreateCustomControllerMap(categoryId, controllerId, 0);

            ControllerMap_Editor newMap = ReInput.UserData.customControllerMaps.Last();
            newMap.name = mapName;

            foreach (ActionElementMap elementMap in actionElementMaps)
            {
                newMap.AddActionElementMap();
                ActionElementMap newElementMap = newMap.GetActionElementMap(newMap.ActionElementMaps.Count() - 1);
                newElementMap.actionId = elementMap.actionId;
                newElementMap.elementType = elementMap.elementType;
                newElementMap.elementIdentifierId = elementMap.elementIdentifierId;
                newElementMap.axisContribution = elementMap.axisContribution;
                if (elementMap.elementType == ControllerElementType.Axis)
                    newElementMap.axisRange = elementMap.axisRange;
                newElementMap.invert = elementMap.invert;
            }

            Logs.WriteInfo("newMap name: ");
            Logs.WriteInfo(newMap.name);

            foreach (ActionElementMap testmap in newMap.actionElementMaps)
            {
                Logs.WriteInfo("action ID: ");
                Logs.WriteInfo(testmap.actionId);
                Logs.WriteInfo("elementIdentifierID: ");
                Logs.WriteInfo(testmap.elementIdentifierId);
                Logs.WriteInfo("elementType: ");
                Logs.WriteInfo(testmap.elementType);
            }

            return ReInput.UserData.fmpEtOISxUiBDFMiRddLezwxpaK(categoryId, controllerId, 0);
        }
        
        
    }
}
