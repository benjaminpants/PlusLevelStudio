using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class NPCExtraTool : NPCTool
    {
        public override string titleKey => "Ed_Tool_" + id + "_Title";

        public NPCExtraTool(string npc, Sprite sprite) : base(npc, sprite)
        {
        }

        internal NPCExtraTool(string npc, string originalNpcSprite) : base(originalNpcSprite)
        {
            this.npc = npc;
        }
    }
}
