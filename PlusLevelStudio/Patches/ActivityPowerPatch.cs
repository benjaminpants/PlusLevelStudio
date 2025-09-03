using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(Activity))]
    [HarmonyPatch("SetPower")]
    static class ActivityPowerPatch
    {
        static bool Prefix(Activity __instance)
        {
            return (!(__instance is NoActivity));
        }
    }
}
