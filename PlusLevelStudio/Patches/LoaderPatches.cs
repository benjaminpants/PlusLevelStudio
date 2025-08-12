using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using PlusStudioLevelLoader;
using UnityEngine;
using PlusLevelStudio.Editor;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(LevelLoaderPlugin))]
    [HarmonyPatch("RoomTextureFromAlias")]
    static class LoaderPatches
    {
        static bool Prefix(string alias, ref Texture2D __result)
        {
            if (EditorPlayModeManager.Instance != null)
            {
                if (EditorPlayModeManager.Instance.customContent.textures.ContainsKey(alias))
                {
                    __result = EditorPlayModeManager.Instance.customContent.textures[alias];
                    return false;
                }
            }
            if (EditorController.Instance == null) return true;
            if (EditorController.Instance.customContent.textures.ContainsKey(alias))
            {
                __result = EditorController.Instance.customContent.textures[alias];
                return false;
            }
            return true;
        }
    }
}
