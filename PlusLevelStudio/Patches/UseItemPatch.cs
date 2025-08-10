using HarmonyLib;
using PlusLevelStudio.Lua;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(ItemManager))]
    [HarmonyPatch("UseItem")]
    class UseItemPatch
    {
        static bool Prefix(ItemManager __instance)
        {
            if (__instance.maxItem < 0) return true;
            if (BaseGameManager.Instance is CustomChallengeManager)
            {
                return ((CustomChallengeManager)BaseGameManager.Instance).OnUseItem(__instance, __instance.items[__instance.selectedItem], __instance.selectedItem);
            }
            return true;
        }
    }
}
