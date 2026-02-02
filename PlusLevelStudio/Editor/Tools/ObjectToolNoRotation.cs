using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ObjectToolNoRotation : PointTool
    {
        public string type;
        public override string id => "object_" + type;
        public float verticalOffset = 0f;
        internal ObjectToolNoRotation(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/object_" + type), 0f)
        {
        }

        internal ObjectToolNoRotation(string type, float offset) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/object_" + type), offset)
        {
        }

        public ObjectToolNoRotation(string type, Sprite sprite, float offset)
        {
            this.sprite = sprite;
            this.type = type;
            verticalOffset = offset;
        }

        public ObjectToolNoRotation(string type, Sprite sprite) : this(type, sprite, 0f)
        {
        }

        protected override bool TryPlace(IntVector2 position)
        {
            EditorController.Instance.AddUndo();
            BasicObjectLocation local = new BasicObjectLocation();
            local.prefab = type;
            local.position = EditorController.Instance.mouseGridPosition.ToWorld();
            local.position += Vector3.up * verticalOffset;
            EditorController.Instance.levelData.objects.Add(local);
            EditorController.Instance.AddVisual(local);
            SoundPlayOneshot("Slap");
            return true;
        }
    }
}
