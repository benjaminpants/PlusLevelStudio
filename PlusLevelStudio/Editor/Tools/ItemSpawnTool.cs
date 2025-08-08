using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ItemSpawnTool : EditorTool
    {
        public int weight;
        public override string id => "itemspawn_" + weight;

        public ItemSpawnTool(int weight) : this(weight, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/itemspawn"))
        {
            this.weight = weight;
        }

        public ItemSpawnTool(int weight, Sprite sprite)
        {
            this.weight = weight;
            this.sprite = sprite;
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
                ItemSpawnPlacement itemPlace = new ItemSpawnPlacement();
                itemPlace.weight = weight;
                itemPlace.position = new Vector2(EditorController.Instance.mouseGridPosition.x * 10f + 5f, EditorController.Instance.mouseGridPosition.z * 10f + 5f);
                EditorController.Instance.AddVisual(itemPlace);
                EditorController.Instance.levelData.itemSpawns.Add(itemPlace);
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
