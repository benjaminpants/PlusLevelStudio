using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class DoorTool : EditorTool
    {
        public string doorType;
        public override string id => "door_" + doorType;
        protected IntVector2? pos;

        public DoorTool(string type)
        {
            doorType = type;
            //sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/light");
        }

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            return true;
        }

        public override void Exit()
        {
            pos = null;
        }

        public virtual void OnPlaced(Direction dir)
        {
            Debug.Log(dir);
            DoorLocation doorPos = new DoorLocation();
            doorPos.position = pos.Value;
            doorPos.type = doorType;
            doorPos.direction = dir;
            EditorController.Instance.levelData.doors.Add(doorPos);
            EditorController.Instance.AddVisual(doorPos);
            EditorController.Instance.RefreshCells();
            EditorController.Instance.RefreshLights();
            EditorController.Instance.SwitchToTool(null);
        }

        public override bool MousePressed()
        {
            if (pos != null) return false;
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                pos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(pos.Value, OnPlaced);
                return false;
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            if (pos == null)
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
