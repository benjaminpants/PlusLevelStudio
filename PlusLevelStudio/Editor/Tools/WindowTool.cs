using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class WindowTool : DoorTool
    {
        public override string id => "window_" + type;
        public WindowTool(string type) : base(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/window_" + type))
        {
        }

        public override void OnPlaced(Direction dir)
        {
            WindowLocation doorPos = new WindowLocation();
            doorPos.position = pos.Value;
            doorPos.type = type;
            doorPos.direction = dir;
            if (!doorPos.ValidatePosition(EditorController.Instance.levelData))
            {
                EditorController.Instance.selector.SelectRotation(pos.Value, OnPlaced); // try again
                return;
            }
            EditorController.Instance.AddUndo();
            EditorController.Instance.levelData.windows.Add(doorPos);
            EditorController.Instance.AddVisual(doorPos);
            EditorController.Instance.RefreshCells();
            EditorController.Instance.RefreshLights();
            EditorController.Instance.SwitchToTool(null);
        }
    }
}
