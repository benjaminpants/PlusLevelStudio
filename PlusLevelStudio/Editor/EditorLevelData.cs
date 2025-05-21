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
        public List<LightGroup> lightGroups = new List<LightGroup>() { new LightGroup() };
        public List<LightPlacement> lights = new List<LightPlacement>();
        public List<EditorRoom> rooms = new List<EditorRoom>();
        public EditorRoom hall => rooms[0];

        private Dictionary<string, TextureContainer> defaultTextures = new Dictionary<string, TextureContainer>();

        // TODO: CHANGE THIS!
        public void DefineDefaultTextures()
        {
            defaultTextures.Add("hall", new TextureContainer("HallFloor", "Wall", "Ceiling"));
            defaultTextures.Add("class", new TextureContainer("BlueCarpet", "WallWithMolding", "Ceiling"));
            defaultTextures.Add("faculty", new TextureContainer("BlueCarpet", "SaloonWall", "Ceiling"));
        }

        public EditorRoom CreateRoomWithDefaultSettings(string type)
        {
            return new EditorRoom(type, defaultTextures[type]);
        }

        public void RemoveUnusedRoom(ushort idToCheck)
        {
            if (idToCheck == 1) return; // NEVER DELETE THE HALLWAY ROOM
            for (int i = 0; i < areas.Count; i++)
            {
                if (areas[i].roomId == idToCheck) return;
            }
            RemoveRoom(rooms[idToCheck - 1]);
        }

        // TODO: implement where needed
        public EditorRoom RoomFromId(ushort roomId)
        {
            if (roomId == 0) return null;
            return rooms[roomId - 1];
        }

        public ushort IdFromRoom(EditorRoom room)
        {
            if (room == null) return 0;
            return (ushort)(rooms.IndexOf(room) + 1);
        }

        public void RemoveRoom(EditorRoom toRemove)
        {
            Dictionary<EditorRoom, ushort> oldValues = new Dictionary<EditorRoom, ushort>();
            Dictionary<EditorRoom, ushort> newValues = new Dictionary<EditorRoom, ushort>();
            foreach (EditorRoom rm in rooms)
            {
                oldValues.Add(rm, (ushort)(rooms.IndexOf(rm) + 1));
            }
            rooms.Remove(toRemove);
            foreach (EditorRoom rm in rooms)
            {
                newValues.Add(rm, (ushort)(rooms.IndexOf(rm) + 1));
            }
            Dictionary<ushort, ushort> oldToNew = new Dictionary<ushort, ushort>();
            foreach (KeyValuePair<EditorRoom, ushort> kvp in newValues)
            {
                oldToNew.Add(oldValues[kvp.Key], kvp.Value);
            }
            for (int i = 0; i < areas.Count; i++)
            {
                areas[i].roomId = oldToNew[areas[i].roomId];
            }
            UpdateCells(true);
        }

        public void ValidatePlacements(bool updateVisuals)
        {
            for (int i = lights.Count - 1; i >= 0; i--)
            {
                if (!lights[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(lights[i]);
                    }
                    lights.RemoveAt(i);
                }
            }
        }


        public void UpdateCells(bool forEditor)
        {
            List<Direction> allDirections = Directions.All();
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    cells[x, y].roomId = RoomIdFromPos(cells[x, y].position.ToInt(), forEditor);
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
            ValidatePlacements(forEditor); // TODO: figure out
        }

        // TODO: figure out if we even NEED to manually recalculate all cells, or if we'd just be better off moving only areas
        public bool ResizeLevel(IntVector2 posDif, IntVector2 sizeDif)
        {
            IntVector2 newMapSize = mapSize + sizeDif;
            // now check areas
            for (int i = 0; i < areas.Count; i++)
            {
                IntVector2[] owned = areas[i].CalculateOwnedCells();
                for (int j = 0; j < owned.Length; j++)
                {
                    IntVector2 result = owned[j] - posDif;
                    if (result.x < 0 || result.z < 0 || result.x >= newMapSize.x || result.z >= newMapSize.z)
                    {
                        return false;
                    }
                }
            }
            // only if all checks pass do we shift everything
            for (int i = 0; i < areas.Count; i++)
            {
                areas[i].origin = (areas[i].origin - posDif);
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

            for (int i = 0; i < lights.Count; i++)
            {
                lights[i].position -= posDif;
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
            DefineDefaultTextures();
            rooms.Add(CreateRoomWithDefaultSettings("hall"));
        }

        public bool AreaValid(CellArea area)
        {
            for (int i = 0; i < areas.Count; i++)
            {
                if (areas[i] == area) continue;
                if (areas[i].CollidesWith(area)) return false;
            }
            IntVector2[] ownedCells = area.CalculateOwnedCells();
            for (int i = 0; i < ownedCells.Length; i++)
            {
                if (GetTileSafe(ownedCells[i].x, ownedCells[i].z) == null) return false;
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

        public ushort RoomIdFromPos(IntVector2 vector, bool forEditor)
        {
            CellArea area = AreaFromPos(vector, forEditor);
            if (area == null) return 0;
            return area.roomId;
        }

        public EditorRoom RoomFromPos(IntVector2 vector, bool forEditor)
        {
            ushort id = RoomIdFromPos(vector, forEditor);
            if (id == 0) return null;
            return rooms[id - 1];
        }

        public CellArea AreaFromPos(IntVector2 vector, bool forEditor)
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
