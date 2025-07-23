using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(CoreGameManager))]
    [HarmonyPatch("Quit")]
    class CGMQuitPatch
    {
        static void Postfix()
        {
            if (Singleton<EditorPlayModeManager>.Instance)
            {
                Singleton<EditorPlayModeManager>.Instance.OnExit();
            }
        }
    }
}
