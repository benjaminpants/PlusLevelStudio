using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ObjectToolSubtile : SubTilePlaceTool
    {
        public string type;
        public float verticalOffset = 0f;
        public override string id => "object_" + type;

        internal ObjectToolSubtile(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/object_" + type), 0f)
        {
        }

        internal ObjectToolSubtile(string type, float verticalOffset) : this(type)
        {
            
        }

        internal ObjectToolSubtile(string type, float verticalOffset, bool offsetOnlyWalls, float addOffset) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/object_" + type), verticalOffset, offsetOnlyWalls, addOffset)
        {
            
        }

        public ObjectToolSubtile(string type, Sprite sprite) : this(type, sprite, 0f)
        {
        }

        public ObjectToolSubtile(string type, Sprite sprite, float verticalOffset) : this(type, sprite, verticalOffset, false, 0f)
        {
        }

        public ObjectToolSubtile(string type, Sprite sprite, float verticalOffset, bool offsetOnlyWalls, float addOffset)
        {
            this.type = type;
            this.sprite = sprite;
            this.verticalOffset = verticalOffset;
            additionalNonCenterOffset = addOffset;
            onlyApplyOffsetAgainstWalls = offsetOnlyWalls;
        }

        protected override bool TryPlace(Vector3 position, Quaternion rotation)
        {
            EditorController.Instance.AddUndo();
            BasicObjectLocation local = new BasicObjectLocation();
            local.prefab = type;
            local.position = position;
            local.position += Vector3.up * verticalOffset;
            local.rotation = rotation;
            EditorController.Instance.levelData.objects.Add(local);
            EditorController.Instance.AddVisual(local);
            SoundPlayOneshot("Slap");
            return true;
        }
    }

    public class ObjectToolSubtileNoRotation : ObjectToolSubtile
    {
        public ObjectToolSubtileNoRotation(string type, Sprite sprite) : base(type, sprite)
        {
        }

        public ObjectToolSubtileNoRotation(string type, Sprite sprite, float verticalOffset) : base(type, sprite, verticalOffset)
        {
        }

        public ObjectToolSubtileNoRotation(string type, Sprite sprite, float verticalOffset, bool offsetOnlyWalls, float addOffset) : base(type, sprite, verticalOffset, offsetOnlyWalls, addOffset)
        {
        }

        internal ObjectToolSubtileNoRotation(string type) : base(type)
        {
        }

        internal ObjectToolSubtileNoRotation(string type, float verticalOffset) : base(type, verticalOffset)
        {
        }

        internal ObjectToolSubtileNoRotation(string type, float verticalOffset, bool offsetOnlyWalls, float addOffset) : base(type, verticalOffset, offsetOnlyWalls, addOffset)
        {
        }

        protected override Quaternion CalculateRotation(Direction[] directions)
        {
            return Quaternion.identity;
        }
    }
}
