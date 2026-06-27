using HarmonyLib;
using PlusLevelStudio.Lua;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(PitstopGameManager))]
    [HarmonyPatch("CallSpecialManagerFunction")]
    class PitstopCallSpecialManagerFunctionPatch
    {
        static bool Prefix(PitstopGameManager __instance, int val)
        {
            if (val != 0) return true;
            if (EditorPlayModeManager.Instance == null) return true;
            Singleton<CoreGameManager>.Instance.Quit();
            return false;
        }
    }
}
