using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public abstract class PostionMarkerPlaceTool : PointTool
    {
        public string type;
        public override string id => "marker_" + type;
        public float verticalOffset = 0f;
        internal PostionMarkerPlaceTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/marker_" + type), 0f)
        {
        }

        internal PostionMarkerPlaceTool(string type, float offset) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/marker_" + type), offset)
        {
        }

        public PostionMarkerPlaceTool(string type, Sprite sprite, float offset)
        {
            this.sprite = sprite;
            this.type = type;
            verticalOffset = offset;
        }

        public PostionMarkerPlaceTool(string type, Sprite sprite) : this(type, sprite, 0f)
        {
        }

        protected abstract PositionMarker MakeMarker();

        protected override bool TryPlace(IntVector2 position)
        {
            EditorController.Instance.AddUndo();
            PositionMarker local = MakeMarker();
            local.type = type;
            local.position = EditorController.Instance.mouseGridPosition.ToWorld();
            local.position += Vector3.up * verticalOffset;
            if (!local.ValidatePosition(EditorController.Instance.levelData)) return false;
            EditorController.Instance.levelData.markers.Add(local);
            EditorController.Instance.AddVisual(local);
            return true;
        }
    }
}
