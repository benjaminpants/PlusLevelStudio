using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{

    public class NPCTool : PointTool
    {
        public string npc;
        public override string id => "npc_" + npc;
        public override string titleKey => LevelLoaderPlugin.Instance.npcAliases[npc].Poster.textData[0].textKey;
        public override string descKey => LevelLoaderPlugin.Instance.npcAliases[npc].Poster.textData[1].textKey;

        internal NPCTool(string npc) : this(npc, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/npc_" + npc))
        {
        }

        public NPCTool(string npc, Sprite sprite)
        {
            this.sprite = sprite;
            this.npc = npc;
        }

        protected override bool TryPlace(IntVector2 position)
        {
            EditorController.Instance.AddUndo();
            NPCPlacement placement = new NPCPlacement();
            placement.npc = npc;
            placement.position = EditorController.Instance.mouseGridPosition;
            EditorController.Instance.levelData.npcs.Add(placement);
            EditorController.Instance.AddVisual(placement);
            SoundPlayOneshot("Slap");
            return true;
        }
    }
}
