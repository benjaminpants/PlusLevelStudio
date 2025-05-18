using System;
using System.Collections.Generic;
using System.Text;
using PlusStudioLevelFormat;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PlusLevelStudio.Editor
{
    public class EditorLevelData
    {
        public IntVector2 mapSize = new IntVector2(50,50);
        public PlusStudioLevelFormat.Cell[,] cells;
        public List<CellArea> areas = new List<CellArea>();


        public void UpdateCells(bool forEditor)
        {
            List<Direction> allDirections = Directions.All();
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    cells[x, y].roomId = RoomIdFromPos(cells[x, y].position, forEditor);
                    cells[x, y].walls = new Nybble(0);
                }
            }
            // secondary for loop because the first pass through we have no clue who our neighbors are
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    if (cells[x, y].roomId == 0) continue; // if we are roomId 0 this check does nothing for us/wastes resources
                    // for all the directions, check if the room next to us matches the room we belong to
                    // if it does, then go on.
                    for (int i = 0; i < allDirections.Count; i++)
                    {
                        Direction dir = allDirections[i];
                        IntVector2 vec2 = dir.ToIntVector2();
                        PlusStudioLevelFormat.Cell nearbyTile = GetTileSafe(cells[x, y].position.x + vec2.x, cells[x, y].position.y + vec2.z);
                        if ((nearbyTile == null) || (nearbyTile.roomId != cells[x, y].roomId))
                        {
                            cells[x, y].walls |= (Nybble)(1 << Directions.BitPosition(dir));
                        }
                    }
                }
            }
        }

        // TODO: figure out if we even NEED to manually recalculate all cells, or if we'd just be better off moving only areas
        public bool ResizeLevel(IntVector2 posDif, IntVector2 sizeDif)
        {
            IntVector2 newMapSize = mapSize + sizeDif;
            // now check areas
            for (int i = 0; i < areas.Count; i++)
            {
                ByteVector2[] owned = areas[i].CalculateOwnedCells();
                for (int j = 0; j < owned.Length; j++)
                {
                    IntVector2 result = owned[j].ToInt() - posDif;
                    if (result.x < 0 || result.z < 0 || result.x >= newMapSize.x || result.z >= newMapSize.z)
                    {
                        return false;
                    }
                }
            }
            // only if all checks pass do we shift everything
            for (int i = 0; i < areas.Count; i++)
            {
                areas[i].origin = (areas[i].origin.ToInt() - posDif).ToByte();
            }

            mapSize = newMapSize;
            cells = new PlusStudioLevelFormat.Cell[mapSize.x, mapSize.z];
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    cells[x, y] = new PlusStudioLevelFormat.Cell(new ByteVector2(x, y));
                }
            }
            return true;
        }

        public EditorLevelData(IntVector2 mapSize)
        {
            this.mapSize = mapSize;
            cells = new PlusStudioLevelFormat.Cell[mapSize.x, mapSize.z];
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    cells[x, y] = new PlusStudioLevelFormat.Cell(new ByteVector2(x,y));
                }
            }
        }

        public bool AreaValid(CellArea area)
        {
            for (int i = 0; i < areas.Count; i++)
            {
                if (areas[i] == area) continue;
                if (areas[i].CollidesWith(area)) return false;
            }
            ByteVector2[] ownedCells = area.CalculateOwnedCells();
            for (int i = 0; i < ownedCells.Length; i++)
            {
                if (GetTileSafe(ownedCells[i].x, ownedCells[i].y) == null) return false;
            }
            return true;
        }

        public PlusStudioLevelFormat.Cell GetTileSafe(int x, int y)
        {
            if (x < 0) return null;
            if (x >= mapSize.x) return null;
            if (y < 0) return null;
            if (y >= mapSize.z) return null;
            return cells[x, y];
        }

        public ushort RoomIdFromPos(ByteVector2 vector, bool forEditor)
        {
            CellArea area = AreaFromPos(vector, forEditor);
            if (area == null) return 0;
            return area.roomId;
        }

        public CellArea AreaFromPos(ByteVector2 vector, bool forEditor)
        {
            foreach (CellArea area in areas)
            {
                if (area.editorOnly && !forEditor) continue;
                if (area.VectorIsInArea(vector))
                {
                    return area;
                }
            }
            return null;
        }
    }
}
