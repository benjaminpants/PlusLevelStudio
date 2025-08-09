using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ItemTool : PointTool
    {
        public string item;
        public override string id => "item_" + item;

        public override string titleKey => PlusStudioLevelLoader.LevelLoaderPlugin.Instance.itemObjects[item].nameKey;

        public ItemTool(string item) : this(item, PlusStudioLevelLoader.LevelLoaderPlugin.Instance.itemObjects[item].itemSpriteSmall)
        {
            this.item = item;
        }

        public ItemTool(string item, Sprite sprite)
        {
            this.item = item;
            this.sprite = sprite;
        }

        protected override bool TryPlace(IntVector2 position)
        {
            EditorController.Instance.AddUndo();
            ItemPlacement itemPlace = new ItemPlacement();
            itemPlace.item = item;
            itemPlace.position = new Vector2(EditorController.Instance.mouseGridPosition.x * 10f + 5f, EditorController.Instance.mouseGridPosition.z * 10f + 5f);
            EditorController.Instance.AddVisual(itemPlace);
            EditorController.Instance.levelData.items.Add(itemPlace);
            return true;
        }
    }
}
