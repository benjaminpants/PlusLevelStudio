using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace PlusLevelStudio.Editor
{
    public class ExitLocation : IEditorCellModifier, IEditorVisualizable
    {
        public string type;
        public IntVector2 position;
        public Direction direction;
        public bool isSpawn;

        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.exitDisplays[type];
        }

        public void InitializeVisual(GameObject visualObject)
        {
            UpdateVisual(visualObject);
        }

        public bool CellOwned(IntVector2 pos)
        {
            LoaderExitData exitData = LevelLoaderPlugin.Instance.exitDatas[type];
            RoomAsset asset = exitData.room;
            IntVector2 roomPivot = asset.potentialDoorPositions[0]; // weird

            foreach (CellData cellData in asset.cells)
            {
                if (pos == cellData.pos.Adjusted(roomPivot, direction) + position) return true;
            }
            return false;
        }

        public IntVector2[] GetOwnedCells()
        {
            List<IntVector2> intVec = new List<IntVector2>();
            LoaderExitData exitData = LevelLoaderPlugin.Instance.exitDatas[type];
            RoomAsset asset = exitData.room;
            IntVector2 roomPivot = asset.potentialDoorPositions[0]; // weird

            foreach (CellData cellData in asset.cells)
            {
                intVec.Add(cellData.pos.Adjusted(roomPivot, direction) + position);
            }
            return intVec.ToArray();
        }


        public void ModifyCells(EditorLevelData data, bool forEditor)
        {
            LoaderExitData exitData = LevelLoaderPlugin.Instance.exitDatas[type];
            RoomAsset asset = exitData.room;
            IntVector2 roomPivot = asset.potentialDoorPositions[0]; // weird

            // figure out our room
            ushort roomId = data.RoomIdFromPos(position + direction.ToIntVector2(), true);
            if (roomId == 0)
            {
                roomId = 1; // default to hall.
            }

            foreach (CellData cellData in asset.cells)
            {
                if (forEditor)
                {
                    PlusStudioLevelFormat.Cell cell = data.GetCellSafe(cellData.pos.Adjusted(roomPivot, direction) + position);
                    cell.walls = (Nybble)Directions.RotateBin(cellData.type, direction);
                    cell.roomId = roomId;
                }
                // find go to the opposite direction in the opposite tile and clear.
                // this does waste some time applying to itself but this is the simplest way to make sure
                // elevators with custom room assets dont break
                List<Direction> openDirs = Directions.OpenDirectionsFromBin(Directions.RotateBin(cellData.type, direction));
                foreach (Direction dir in openDirs)
                {
                    PlusStudioLevelFormat.Cell nextCell = data.GetCellSafe(cellData.pos.Adjusted(roomPivot, direction) + position + dir.ToIntVector2());
                    nextCell.walls = (Nybble)(nextCell.walls & ~dir.GetOpposite().ToBinary());
                }
            }
            if (!forEditor) return;
            // todo: adjust to use the much more straight forward vanilla logic for handling this
            PlusStudioLevelFormat.Cell targCell = data.GetCellSafe(new IntVector2(1, 0).Adjusted(roomPivot, direction) + position);
            targCell.walls = (Nybble)0;
            targCell.roomId = 0;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = new Vector3((float)position.x * 10f + 5f, 0f, (float)position.z * 10f + 5f);
            visualObject.transform.rotation = direction.ToRotation();
        }
    }
}
