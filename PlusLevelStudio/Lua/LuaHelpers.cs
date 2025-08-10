using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Lua
{
    public static class LuaHelpers
    {
        public static string GetIDFromItemObject(ItemObject itemObject)
        {
            foreach (KeyValuePair<string, ItemObject> kvp in LevelLoaderPlugin.Instance.itemObjects)
            {
                if (kvp.Value == itemObject)
                {
                    return kvp.Key;
                }
            }
            foreach (KeyValuePair<string, ItemObject> kvp in LevelLoaderPlugin.Instance.itemObjects)
            {
                if (kvp.Value.itemType == itemObject.itemType)
                {
                    return kvp.Key;
                }
            }
            return "unknown";
        }

        public static string GetIDFromNPC(NPC npc)
        {
            foreach (KeyValuePair<string, NPC> kvp in LevelLoaderPlugin.Instance.npcAliases)
            {
                if (kvp.Value.name == npc.name.Replace("(Clone)", ""))
                {
                    return kvp.Key;
                }
            }
            foreach (KeyValuePair<string, NPC> kvp in LevelLoaderPlugin.Instance.npcAliases)
            {
                if (kvp.Value.Character == npc.Character)
                {
                    return kvp.Key;
                }
            }
            return "unknown";
        }
    }
}
