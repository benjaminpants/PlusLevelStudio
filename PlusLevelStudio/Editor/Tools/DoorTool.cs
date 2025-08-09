using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class DoorTool : PlaceAndRotateTool
    {
        public string type;
        public override string id => "door_" + type;

        internal DoorTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/door_" + type))
        {
            
        }

        public DoorTool(string type, Sprite sprite)
        {
            this.type = type;
            this.sprite = sprite;
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            DoorLocation doorPos = new DoorLocation();
            doorPos.position = position;
            doorPos.type = type;
            doorPos.direction = dir;
            if (!doorPos.ValidatePosition(EditorController.Instance.levelData)) return false;
            EditorController.Instance.AddUndo();
            EditorController.Instance.levelData.doors.Add(doorPos);
            EditorController.Instance.AddVisual(doorPos);
            EditorController.Instance.RefreshCells();
            EditorController.Instance.RefreshLights();
            EditorController.Instance.SwitchToTool(null);
            return true;
        }
    }
}
