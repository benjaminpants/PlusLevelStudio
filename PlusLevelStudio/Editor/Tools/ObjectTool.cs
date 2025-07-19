using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ObjectTool : DoorTool
    {
        public float verticalOffset = 0f;
        public override string id => "object_" + type;
        public ObjectTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/object_" + type), 0f)
        {
        }
        public ObjectTool(string type, Sprite sprite) : this(type, sprite, 0f)
        {
        }

        public ObjectTool(string type, Sprite sprite, float offset) : base(type, sprite)
        {
            verticalOffset = offset;
        }

        public ObjectTool(string type, float offset) : this(type)
        {
            verticalOffset = offset;
        }

        public override void OnPlaced(Direction dir)
        {
            EditorController.Instance.AddUndo();
            BasicObjectLocation local = new BasicObjectLocation();
            local.prefab = type;
            local.position = pos.Value.ToWorld();
            local.position += Vector3.up * verticalOffset;
            local.rotation = dir.ToRotation();
            EditorController.Instance.levelData.objects.Add(local);
            EditorController.Instance.AddVisual(local);

            EditorController.Instance.SwitchToTool(null);
        }
    }
}
