using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class WallTool : EditorTool
    {
        public bool placeWall;
        public override string id => placeWall ? "wallplacer" : "wallremover";
        protected IntVector2? pos;

        internal WallTool(bool place)
        {
            placeWall = place;
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/" + (placeWall ? "wallplacer" : "wallremover"));
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

        public void OnPlaced(Direction dir)
        {
            EditorController.Instance.AddUndo();
            WallLocation wall = new WallLocation();
            wall.wallState = placeWall;
            wall.position = pos.Value;
            wall.direction = dir;
            EditorController.Instance.levelData.walls.Add(wall);
            EditorController.Instance.AddVisual(wall);
            EditorController.Instance.RefreshCells();
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
