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
    //[HarmonyPatch]
    class InputPatches
    {
        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Kingmaker.MainMenu), nameof(Kingmaker.MainMenu.Update))]
        public static void UpdateRewiredControllerStuff()
        {
            Logs.WriteInfo("Update hook called");
            Controllers.Update();
        }
        */
    }
}
