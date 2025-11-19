using MTM101BaldAPI;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class PremadeRoomLocation : IEditorPositionVerifyable, IEditorCellModifier, IEditorDeletable
    {
        public string room;
        public IntVector2 position;
        public Direction direction;
        public int doorId;
        public void ModifyCellDisplay(EditorController rec)
        {
            Texture2D premadeWall = LevelStudioPlugin.Instance.assetMan.Get<Texture2D>("Premade_Wall");
            RoomAsset roomAsset = GetRoomAsset();
            IntVector2 roomPivot = roomAsset.potentialDoorPositions[doorId];
            for (int i = 0; i < roomAsset.cells.Count; i++)
            {
                CellData cellData = roomAsset.cells[i];
                int rotatedBin = Directions.RotateBin(cellData.type, direction);
                IntVector2 newPos = cellData.pos.Adjusted(roomPivot, direction) + position;
                Cell cell = rec.workerEc.CellFromPosition(newPos);
                cell.Tile.gameObject.SetActive(true);
                cell.Tile.MeshRenderer.material.SetMainTexture(rec.GenerateTextureAtlas(premadeWall, premadeWall, premadeWall));
                cell.SetShape(rotatedBin, TileShapeMask.None);
                if (cell.Null)
                {
                    cell.Initialize();
                }
            }

        }

        public IntVector2 GetDoorPos()
        {
            return position;
        }

        public bool OwnsPosition(IntVector2 pos)
        {
            RoomAsset roomAsset = GetRoomAsset();
            IntVector2 roomPivot = roomAsset.potentialDoorPositions[doorId];
            for (int i = 0; i < roomAsset.cells.Count; i++)
            {
                CellData cellData = roomAsset.cells[i];
                IntVector2 newPos = cellData.pos.Adjusted(roomPivot, direction) + position;
                if (newPos == pos) return true;
            }
            return false;
        }

        public IntVector2[] CalculateOwnedCells()
        {
            List<IntVector2> result = new List<IntVector2>();
            RoomAsset roomAsset = GetRoomAsset();
            IntVector2 roomPivot = roomAsset.potentialDoorPositions[doorId];
            for (int i = 0; i < roomAsset.cells.Count; i++)
            {
                CellData cellData = roomAsset.cells[i];
                IntVector2 newPos = cellData.pos.Adjusted(roomPivot, direction) + position;
                result.Add(newPos);
            }
            return result.ToArray();
        }

        public RoomAsset GetRoomAsset()
        {
            return PlusStudioLevelLoader.LevelLoaderPlugin.Instance.roomAssetAliases[room];
        }

        public void ModifyLightsForEditor(EnvironmentController workerEc)
        {
            RoomAsset roomAsset = GetRoomAsset();
            IntVector2 roomPivot = roomAsset.potentialDoorPositions[doorId];
            for (int i = 0; i < roomAsset.lights.Count; i++)
            {
                LightSourceData lightData = roomAsset.lights[i];
                IntVector2 newPos = lightData.position.Adjusted(roomPivot, direction) + position;
                workerEc.GenerateLight(workerEc.CellFromPosition(newPos), lightData.color, lightData.strength);
            }
        }

        public bool ValidatePosition(EditorLevelData data)
        {
            RoomAsset roomAsset = GetRoomAsset();
            IntVector2 roomPivot = roomAsset.potentialDoorPositions[doorId];
            for (int i = 0; i < roomAsset.cells.Count; i++)
            {
                CellData cellData = roomAsset.cells[i];
                IntVector2 newPos = cellData.pos.Adjusted(roomPivot, direction) + position;
                PlusStudioLevelFormat.Cell cell = EditorController.Instance.levelData.GetCellSafe(newPos);
                if (cell == null) return false;
                if (cell.roomId != 0) return false;
                for (int j = 0; j < data.premadeRooms.Count; j++)
                {
                    if (data.premadeRooms[j] == this) continue;
                    if (data.premadeRooms[j].OwnsPosition(newPos)) return false;
                }
            }
            return true;
        }

        public void ModifyCells(EditorLevelData data, bool forEditor)
        {
            data.GetCellSafe(position + direction.GetOpposite().ToIntVector2()).walls &= (Nybble)~(direction.ToBinary());
            //compiled.cells[premadeRooms[i].position.x + cellOff.x, premadeRooms[i].position.z + cellOff.z].walls &= (Nybble)~(premadeRooms[i].direction.ToBinary());
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.premadeRooms.Remove(this);
            EditorController.Instance.RefreshCells();
            return true;
        }
    }
}
