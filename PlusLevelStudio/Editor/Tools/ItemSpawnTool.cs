using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ItemSpawnTool : PointTool
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

        protected override bool TryPlace(IntVector2 position)
        {
            EditorController.Instance.AddUndo();
            ItemSpawnPlacement itemPlace = new ItemSpawnPlacement();
            itemPlace.weight = weight;
            itemPlace.position = new Vector2(EditorController.Instance.mouseGridPosition.x * 10f + 5f, EditorController.Instance.mouseGridPosition.z * 10f + 5f);
            EditorController.Instance.AddVisual(itemPlace);
            EditorController.Instance.levelData.itemSpawns.Add(itemPlace);
            return true;
        }
    }
}
