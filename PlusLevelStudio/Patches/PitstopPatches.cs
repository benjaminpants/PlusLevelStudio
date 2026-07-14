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

    [HarmonyPatch(typeof(PitstopGameManager))]
    [HarmonyPatch("PrepareLevelData")]
    class PitstopPrepareLevelDataPatch
    {
        static void Prefix(PitstopGameManager __instance, ref int ___tierOneTripLevel, ref WeightedFieldTrip[] ___tierOneTrips)
        {
            if (!Singleton<EditorPlayModeManager>.Instance) return;
            PlaymodeSettingsMeta playSettings = Singleton<EditorPlayModeManager>.Instance.GetSettingsFor(Singleton<CoreGameManager>.Instance.nextLevel);
            if (playSettings.fieldTrip == "none")
            {
                ___tierOneTripLevel = int.MinValue;
                return;
            }
            ___tierOneTripLevel = Singleton<CoreGameManager>.Instance.nextLevel.levelNo;
            ___tierOneTrips = new WeightedFieldTrip[]
            {
                new WeightedFieldTrip()
                {
                    selection = LevelStudioPlugin.Instance.fieldTrips[playSettings.fieldTrip],
                    weight = 10000
                }
            };
        }
    }
}
