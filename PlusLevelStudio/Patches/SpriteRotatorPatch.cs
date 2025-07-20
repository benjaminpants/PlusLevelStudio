using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(SpriteRotator))]
    [HarmonyPatch("Update")]
    class SpriteRotatorPatch
    {
        static bool Prefix(Transform ___cam)
        {
            return (___cam != null);
        }
    }
}
