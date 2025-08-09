using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class WallTool : PlaceAndRotateTool
    {
        public bool placeWall;
        public override string id => placeWall ? "wallplacer" : "wallremover";

        internal WallTool(bool place)
        {
            placeWall = place;
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/" + (placeWall ? "wallplacer" : "wallremover"));
        }

        protected override bool TryPlace(IntVector2 position, Direction dir)
        {
            EditorController.Instance.AddUndo();
            WallLocation wall = new WallLocation();
            wall.wallState = placeWall;
            wall.position = position;
            wall.direction = dir;
            EditorController.Instance.levelData.walls.Add(wall);
            EditorController.Instance.AddVisual(wall);
            EditorController.Instance.RefreshCells();
            EditorController.Instance.SwitchToTool(null);
            return true;
        }
    }
}
