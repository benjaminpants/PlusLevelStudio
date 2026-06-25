using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class WallTool : PlaceAndRotateTool
    {
        public WallState placeWall;
        private string _id;
        public override string id => _id;

        internal WallTool(string id, WallState place)
        {
            _id = id;
            placeWall = place;
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/" + _id);
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
            SoundPlayOneshot("Slap");
            return true;
        }
    }
}
