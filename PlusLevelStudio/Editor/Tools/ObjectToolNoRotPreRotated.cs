using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ObjectToolNoRotPreRotated : ObjectToolNoRotation
    {
        public Quaternion rotationPreset;
        internal ObjectToolNoRotPreRotated(string type, Quaternion rotation) : this(type, rotation, 0f)
        {
        }

        internal ObjectToolNoRotPreRotated(string type, Quaternion rotation, float offset) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/object_" + type), rotation, offset)
        {
        }

        public ObjectToolNoRotPreRotated(string type, Sprite sprite, Quaternion rotation, float offset) : base(type, sprite, offset)
        {
            rotationPreset = rotation;
        }

        public ObjectToolNoRotPreRotated(string type, Sprite sprite, Quaternion rotation) : this(type, sprite, rotation, 0f)
        {
        }

        protected override bool TryPlace(IntVector2 position)
        {
            EditorController.Instance.AddUndo();
            BasicObjectLocation local = new BasicObjectLocation();
            local.prefab = type;
            local.position = EditorController.Instance.mouseGridPosition.ToWorld();
            local.position += Vector3.up * verticalOffset;
            local.rotation = rotationPreset;
            EditorController.Instance.levelData.objects.Add(local);
            EditorController.Instance.AddVisual(local);
            SoundPlayOneshot("Slap");
            return true;
        }
    }
}
