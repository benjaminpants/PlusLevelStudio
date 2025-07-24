using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(CoreGameManager))]
    [HarmonyPatch("ReturnToMenu")]
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
