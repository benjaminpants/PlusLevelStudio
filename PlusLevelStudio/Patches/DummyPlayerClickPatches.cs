using HarmonyLib;
using PlusLevelStudio.Editor;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(PlayerClick))]
    [HarmonyPatch("PostCameraUpdate")]
    class DummyPlayerClickPatches
    {
        static bool Prefix(PlayerClick __instance)
        {
            return !(__instance is DummyPlayerClick);
        }
    }
}
