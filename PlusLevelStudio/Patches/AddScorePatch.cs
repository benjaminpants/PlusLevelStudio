using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(HighScoreManager))]
    [HarmonyPatch("AddScore")]
    class AddScorePatch
    {
        static bool Prefix(string ___currentLevelId, out int rank)
        {
            if (string.IsNullOrEmpty(___currentLevelId))
            {
                rank = 0;
                return false;
            }
            rank = -1;
            return true;
        }
    }
}
