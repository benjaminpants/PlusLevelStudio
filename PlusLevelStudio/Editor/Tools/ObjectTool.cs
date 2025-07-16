using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ObjectTool : DoorTool
    {
        public override string id => "object_" + type;
        public ObjectTool(string type) : base(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/object_" + type))
        {
        }
        public ObjectTool(string type, Sprite sprite) : base(type, sprite)
        {
        }

        public override void OnPlaced(Direction dir)
        {
            EditorController.Instance.AddUndo();
            BasicObjectLocation local = new BasicObjectLocation();
            local.prefab = type;
            local.position = pos.Value.ToWorld();
            local.rotation = dir.ToRotation();
            EditorController.Instance.levelData.objects.Add(local);
            EditorController.Instance.AddVisual(local);

            EditorController.Instance.SwitchToTool(null);
        }
    }
}
