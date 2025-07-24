using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PlusStudioLevelLoader.Patches
{
    [HarmonyPatch(typeof(ExtraLevelData))]
    [HarmonyPatch("ConvertFromAsset")]
    class ConvertFromAssetPatch
    {
        static void Postfix(ExtraLevelDataAsset asset, ref ExtraLevelData __result)
        {
            if (!(asset is ExtendedExtraLevelDataAsset)) return;
            ExtendedExtraLevelDataAsset extendedAsset = (ExtendedExtraLevelDataAsset)asset;
            ExtendedExtraLevelData extendedData = new ExtendedExtraLevelData();
            FieldInfo[] fields = typeof(ExtraLevelData).GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i].SetValue(extendedData, fields[i].GetValue(__result));
            }
            __result = extendedData;
            extendedData.timeOutTime = extendedAsset.timeOutTime;
            extendedData.timeOutEvent = extendedAsset.timeOutEvent;
        }
    }
}
