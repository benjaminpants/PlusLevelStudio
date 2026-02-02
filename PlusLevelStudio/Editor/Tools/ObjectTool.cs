using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ObjectTool : PlaceAndRotateTool
    {
        public string type;
        public float verticalOffset = 0f;
        public override string id => "object_" + type;
        internal ObjectTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/object_" + type), 0f)
        {
        }

        internal ObjectTool(string type, float offset) : this(type)
        {
            verticalOffset = offset;
        }

        public ObjectTool(string type, Sprite sprite) : this(type, sprite, 0f)
        {
        }

        public ObjectTool(string type, Sprite sprite, float offset)
        {
            this.type = type;
            this.sprite = sprite;
            verticalOffset = offset;
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            EditorController.Instance.AddUndo();
            BasicObjectLocation local = new BasicObjectLocation();
            local.prefab = type;
            local.position = position.ToWorld();
            local.position += Vector3.up * verticalOffset;
            local.rotation = dir.ToRotation();
            EditorController.Instance.levelData.objects.Add(local);
            EditorController.Instance.AddVisual(local);
            SoundPlayOneshot("Slap");
            return true;
        }
    }
}
