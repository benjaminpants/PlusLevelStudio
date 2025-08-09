using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class WindowTool : PlaceAndRotateTool
    {
        public string type;
        public override string id => "window_" + type;
        internal WindowTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/window_" + type))
        {
        }
        public WindowTool(string type, Sprite sprite)
        {
            this.type = type;
            this.sprite = sprite;
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            WindowLocation doorPos = new WindowLocation();
            doorPos.position = position;
            doorPos.type = type;
            doorPos.direction = dir;
            if (!doorPos.ValidatePosition(EditorController.Instance.levelData)) return false;
            EditorController.Instance.AddUndo();
            EditorController.Instance.levelData.windows.Add(doorPos);
            EditorController.Instance.AddVisual(doorPos);
            EditorController.Instance.RefreshCells();
            EditorController.Instance.RefreshLights();
            EditorController.Instance.SwitchToTool(null);
            return true;
        }
    }
}
