using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ItemTool : EditorTool
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
                ItemPlacement itemPlace = new ItemPlacement();
                itemPlace.item = item;
                itemPlace.position = new Vector2(EditorController.Instance.mouseGridPosition.x * 10f + 5f, EditorController.Instance.mouseGridPosition.z * 10f + 5f);
                EditorController.Instance.AddVisual(itemPlace);
                EditorController.Instance.levelData.items.Add(itemPlace);
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
