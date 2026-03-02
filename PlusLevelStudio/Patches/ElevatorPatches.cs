using HarmonyLib;
using PlusLevelStudio.Lua;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(ElevatorManager))]
    [HarmonyPatch("ExitAvailable")]
    [HarmonyPatch(MethodType.Getter)]
    class ElevatorExitAvailablePatch
    {
        static bool Prefix(ElevatorManager __instance, ref bool __result, int ___foundOutOfOrderElevators)
        {
            if (!(Singleton<BaseGameManager>.Instance is CustomChallengeManager)) return true;
            CustomChallengeManager cgm = (CustomChallengeManager)Singleton<BaseGameManager>.Instance;
            __result = cgm.escapeSequenceActive && (___foundOutOfOrderElevators >= __instance.TotalOutOfOrderElevators);
            return false;
        }
    }

    [HarmonyPatch(typeof(ElevatorManager))]
    [HarmonyPatch("ShouldFail")]
    class ElevatorShouldFailPatch
    {
        static bool Prefix(ElevatorManager __instance, ref bool __result, int ___foundOutOfOrderElevators)
        {
            if (!(Singleton<BaseGameManager>.Instance is CustomChallengeManager)) return true;
            CustomChallengeManager cgm = (CustomChallengeManager)Singleton<BaseGameManager>.Instance;
            __result = cgm.escapeSequenceActive && (___foundOutOfOrderElevators < __instance.TotalOutOfOrderElevators);
            return false;
        }
    }
}
