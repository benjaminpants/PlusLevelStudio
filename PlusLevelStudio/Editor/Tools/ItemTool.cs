using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ItemTool : PointTool
    {
        public bool useItemName;
        public string item;
        public override string id => "item_" + item;

        public override string titleKey
        {
            get
            {
                if (!useItemName)
                {
                    return base.titleKey;
                }
                string itemNameKey = PlusStudioLevelLoader.LevelLoaderPlugin.Instance.itemObjects[item].nameKey;
                if (LocalizationManager.Instance == null)
                {
                    return itemNameKey;
                }
                if (LocalizationManager.Instance.GetLocalizedText(itemNameKey) != itemNameKey)
                {
                    return itemNameKey;
                }
                if (LocalizationManager.Instance.GetLocalizedText(base.titleKey) != base.titleKey)
                {
                    return base.titleKey;
                }
                return itemNameKey;
            }
        }

        static Sprite ResolveSprite(string item)
        {
            string customSpriteKey = "Tools/item_" + item;
            if (LevelStudioPlugin.Instance.uiAssetMan.ContainsKey(customSpriteKey))
            {
                return LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>(customSpriteKey);
            }
            return PlusStudioLevelLoader.LevelLoaderPlugin.Instance.itemObjects[item].itemSpriteSmall;
        }

        public ItemTool(string item) : this(item, ResolveSprite(item), true)
        {
            this.item = item;
        }

        public ItemTool(string item, bool useItemNameAsToolName) : this(item, ResolveSprite(item), useItemNameAsToolName)
        {
            this.item = item;
        }

        public ItemTool(string item, Sprite sprite) : this(item, sprite, true)
        {
            this.item = item;
            this.sprite = sprite;
        }

        public ItemTool(string item, Sprite sprite, bool useItemNameAsToolName)
        {
            useItemName = useItemNameAsToolName;
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
            if (LevelLoaderPlugin.Instance.itemObjects[item].audPickupOverride != null)
            {
                SoundPlayOneshot(LevelLoaderPlugin.Instance.itemObjects[item].audPickupOverride);
            }
            else
            {
                SoundPlayOneshot("ItemPickup");
            }
            return true;
        }
    }
}
