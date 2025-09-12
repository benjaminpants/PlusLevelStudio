using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class CellMarkerTool : PointTool
    {
        public string type;
        public override string id => "marker_" + type;

        internal CellMarkerTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/marker_" + type))
        {
        }

        public CellMarkerTool(string type, Sprite sprite)
        {
            this.sprite = sprite;
            this.type = type;
        }

        protected override bool TryPlace(IntVector2 position)
        {
            EditorController.Instance.AddUndo();
            CellMarker point = (CellMarker)LevelStudioPlugin.Instance.ConstructMarkerOfType(type);
            point.position = EditorController.Instance.mouseGridPosition;
            EditorController.Instance.levelData.markers.Add(point);
            EditorController.Instance.AddVisual(point);
            EditorController.Instance.UpdateVisual(point);
            EditorController.Instance.RefreshLights();
            return true;
        }
    }
}
