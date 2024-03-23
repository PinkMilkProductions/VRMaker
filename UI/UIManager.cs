using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker;
using ProBuilder2.Common;
using Kingmaker.UI;

namespace VRMaker
{
    public class UIManager
    {
        public static void UpdateOvertips()
        {
            Logs.WriteInfo("Current amount of Overtips in list: " + Plugin.MyHelper.Overtips.Length);
            foreach (Kingmaker.UI.Overtip.OvertipComponent Overtip in Plugin.MyHelper.Overtips)
            {
                if (Overtip)
                {
                    if (Overtip.OvertipController.MapObject.View)
                    {
                        RectTransform rectTransform = (RectTransform)Overtip.transform;
                        rectTransform.anchoredPosition3D = Overtip.OvertipController.MapObject.View.transform.position + Vector3.up * 2f;
                        Overtip.transform.position = Overtip.OvertipController.MapObject.View.transform.position + Vector3.up * 2f;
                        Logs.WriteInfo("Overtip " + Overtip.name + " set to position: " + Overtip.transform.position);
                    }
                }
                else
                {
                    Logs.WriteInfo("Removing null Overtip");
                    Plugin.MyHelper.Overtips.Remove(Overtip);
                }
            }
        }
    }
}
