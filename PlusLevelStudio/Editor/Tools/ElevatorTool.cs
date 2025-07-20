using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ElevatorTool : EditorTool
    {
        public string type;
        protected IntVector2? pos;
        public bool isSpawn = false;
        public override string id => "exit_" + type + (isSpawn ? "_start" : "");
        internal ElevatorTool(string type, bool isSpawn) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/exit_" + type + (isSpawn ? "_start" : "")), isSpawn)
        {
        }
        public ElevatorTool(string type, Sprite sprite, bool isSpawn)
        {
            this.type = type;
            this.sprite = sprite;
            this.isSpawn = isSpawn;
        }

        public void OnPlaced(Direction dir)
        {
            EditorController.Instance.AddUndo();
            ExitLocation exitLocal = new ExitLocation();
            exitLocal.position = pos.Value;
            exitLocal.direction = dir;
            exitLocal.type = "elevator";
            exitLocal.isSpawn = isSpawn;
            // TODO: validation

            EditorController.Instance.levelData.exits.Add(exitLocal);
            EditorController.Instance.AddVisual(exitLocal);
            EditorController.Instance.RefreshCells();
            EditorController.Instance.RefreshLights();
            EditorController.Instance.SwitchToTool(null);
        }


        public override void Begin()
        {

        }

        public override bool Cancelled()
        {
            if (pos != null)
            {
                pos = null;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            pos = null;
        }

        public override bool MousePressed()
        {
            if (pos != null) return false;
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) == 0)
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
