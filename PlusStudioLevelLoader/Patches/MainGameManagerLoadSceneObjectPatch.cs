using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusStudioLevelLoader.Patches
{
    [HarmonyPatch(typeof(MainGameManager))]
    [HarmonyPatch("LoadSceneObject")]
    class LoadSceneObjectPatch
    {
        static void Prefix(MainGameManager __instance)
        {
            if (Singleton<CoreGameManager>.Instance.sceneObject.extraAsset == null) return;
            if (!(Singleton<CoreGameManager>.Instance.sceneObject.extraAsset is ExtendedExtraLevelDataAsset)) return;
            __instance.levelObject.finalLevel = ((ExtendedExtraLevelDataAsset)Singleton<CoreGameManager>.Instance.sceneObject.extraAsset).finalLevel;
        }
    }
}
