using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Kingmaker;


namespace VRMaker
{
    [HarmonyPatch]
    class CameraPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Camera), "Start")]
    }
}
