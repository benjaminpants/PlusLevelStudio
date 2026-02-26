using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class NPCExtraTool : NPCTool
    {
        public override string titleKey => LocalizationManager.Instance.GetLocalizedText(base.titleKey) + " (" + LevelLoaderPlugin.Instance.npcAliases[npc].name + ")";

        public NPCExtraTool(string npc, Sprite sprite) : base(npc, sprite)
        {
        }

        internal NPCExtraTool(string npc, string originalNpcSprite) : base(originalNpcSprite)
        {
            this.npc = npc;
        }
    }
}
