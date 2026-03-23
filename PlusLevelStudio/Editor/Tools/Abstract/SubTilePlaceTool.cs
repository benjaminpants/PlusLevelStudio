using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public abstract class SubTilePlaceTool : EditorTool
    {
        protected float additionalNonCenterOffset;
        protected bool onlyApplyOffsetAgainstWalls;
        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            return true;
        }

        public override void Exit()
        {
            
        }

        protected const float subTileSize = 10f / 3f;

        protected virtual Vector3 CalculatePosition(IntVector2 cellPosition, Direction[] directions)
        {
            Vector3 res = cellPosition.ToWorld();
            for (int i = 0; i < directions.Length; i++)
            {
                if (directions[i] == Direction.Null) continue;
                res += directions[i].ToVector3() * subTileSize;
                PlusStudioLevelFormat.Cell cell = EditorController.Instance.levelData.GetCellSafe(cellPosition);
                if (cell == null) continue;
                if ((!onlyApplyOffsetAgainstWalls) || ((cell.walls & directions[i].ToBinary()) > 0))
                {
                    res += directions[i].ToVector3() * additionalNonCenterOffset;
                }
            }
            return res;
        }

        protected virtual Quaternion CalculateRotation(Direction[] directions)
        {
            if (directions[0] == Direction.Null)
            {
                return Direction.North.ToRotation();
            }
            if (directions[1] == Direction.Null)
            {
                return directions[0].ToRotation();
            }
            return Quaternion.Lerp(directions[0].ToRotation(), directions[1].ToRotation(), 0.5f);
        }

        public virtual bool ValidLocation(IntVector2 position)
        {
            return (EditorController.Instance.levelData.RoomIdFromPos(position, true) != 0);
        }

        protected abstract bool TryPlace(Vector3 position, Quaternion rotation);

        public override bool MousePressed()
        {
            if (ValidLocation(EditorController.Instance.selector.selectedTile))
            {
                return TryPlace(CalculatePosition(EditorController.Instance.selector.selectedTile, EditorController.Instance.selector.selectedSubTileDirections), CalculateRotation(EditorController.Instance.selector.selectedSubTileDirections));
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            EditorController.Instance.selector.SelectNearestSubTile(EditorController.Instance.mousePlanePosition);
        }
    }
}
