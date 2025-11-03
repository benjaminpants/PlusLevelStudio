using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using PlusStudioLevelLoader;
using UnityEngine;
using PlusLevelStudio.Editor;

namespace PlusLevelStudio
{
    public abstract class EditorLoaderModifier<T> : AssetIdStorageModifier<T> where T : class
    {
        public virtual EditorCustomContent GetCustomContent()
        {
            if (EditorPlayModeManager.Instance != null)
            {
                return EditorPlayModeManager.Instance.customContent;
            }
            if (EditorController.Instance == null) return null;
            return EditorController.Instance.customContent;
        }
    }

    public class LoaderTextureModifier : EditorLoaderModifier<Texture2D>
    {
        public override bool ContainsKey(string id)
        {
            if (GetCustomContent() == null) return false;
            return GetCustomContent().GetForEntryType("texture").Contains(id);
        }

        public override Texture2D Get(string id)
        {
            if (GetCustomContent() == null) return null;
            return (Texture2D)GetCustomContent().GetForEntryType("texture").Get(id);
        }

        public override List<KeyValuePair<string, Texture2D>> GetEntries()
        {
            throw new NotImplementedException();
        }
    }

    /*
    [HarmonyPatch(typeof(LevelLoaderPlugin))]
    [HarmonyPatch("RoomTextureFromAlias")]
    static class RoomTextureAliasPatch
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

    [HarmonyPatch(typeof(LevelLoaderPlugin))]
    [HarmonyPatch("PosterFromAlias")]
    static class PosterAliasPatch
    {
        static bool Prefix(string alias, ref PosterObject __result)
        {
            if (EditorPlayModeManager.Instance != null)
            {
                if (EditorPlayModeManager.Instance.customContent.posters.ContainsKey(alias))
                {
                    __result = EditorPlayModeManager.Instance.customContent.posters[alias];
                    return false;
                }
            }
            if (EditorController.Instance == null) return true;
            if (EditorController.Instance.customContent.posters.ContainsKey(alias))
            {
                __result = EditorController.Instance.customContent.posters[alias];
                return false;
            }
            return true;
        }
    }*/
}
