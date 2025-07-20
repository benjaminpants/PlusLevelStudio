using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{

    public class NPCTool : EditorTool
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

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            return true;
        }

        public override void Exit()
        {
            
        }

        public override bool MousePressed()
        {
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                EditorController.Instance.AddUndo();
                NPCPlacement placement = new NPCPlacement();
                placement.npc = npc;
                placement.position = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.levelData.npcs.Add(placement);
                EditorController.Instance.AddVisual(placement);
                return true;
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
        }
    }
}
