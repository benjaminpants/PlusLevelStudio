using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PlusLevelStudio.Editor;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using UnityEngine;
using UnityEngine.UIElements;

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
        public List<DoorLocation> doors = new List<DoorLocation>();
        // TODO: TileBasedObject data
        public List<WindowLocation> windows = new List<WindowLocation>();
        public List<ExitLocation> exits = new List<ExitLocation>();
        public EditorRoom hall => rooms[0];

        private Dictionary<string, TextureContainer> defaultTextures = new Dictionary<string, TextureContainer>();

        // TODO: CHANGE THIS!
        public void DefineDefaultTextures()
        {
            defaultTextures.Add("hall", new TextureContainer("HallFloor", "Wall", "Ceiling"));
            defaultTextures.Add("class", new TextureContainer("BlueCarpet", "WallWithMolding", "Ceiling"));
            defaultTextures.Add("faculty", new TextureContainer("BlueCarpet", "SaloonWall", "Ceiling"));
            defaultTextures.Add("office", new TextureContainer("BlueCarpet", "WallWithMolding", "Ceiling"));
            defaultTextures.Add("closet", new TextureContainer("TileFloor", "Wall", "Ceiling"));
            defaultTextures.Add("reflex", new TextureContainer("HallFloor", "WallWithMolding", "ElevatorCeiling"));
            defaultTextures.Add("library", new TextureContainer("BlueCarpet", "WallWithMolding", "Ceiling"));
            defaultTextures.Add("cafeteria", new TextureContainer("HallFloor", "Wall", "Ceiling"));
            defaultTextures.Add("outside", new TextureContainer("Grass", "Fence", "None"));
            defaultTextures.Add("shop", new TextureContainer("HallFloor", "JohnnyWall", "Ceiling"));
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

        // TODO: consider interface for ValidatePosition to avoid repeated code..?
        public bool ValidatePlacements(bool updateVisuals)
        {
            bool changedSomething = false;
            for (int i = lights.Count - 1; i >= 0; i--)
            {
                if (!lights[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(lights[i]);
                    }
                    lights.RemoveAt(i);
                    changedSomething = true;
                }
            }
            for (int i = doors.Count - 1; i >= 0; i--)
            {
                if (!doors[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(doors[i]);
                    }
                    doors.RemoveAt(i);
                    changedSomething = true;
                }
            }
            for (int i = windows.Count - 1; i >= 0; i--)
            {
                if (!windows[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(windows[i]);
                    }
                    windows.RemoveAt(i);
                    changedSomething = true;
                }
            }
            return changedSomething;
        }


        protected void ApplyCellModifiers(IEnumerable<IEditorCellModifier> modifiers, bool forEditor)
        {
            foreach (IEditorCellModifier cellMod in modifiers)
            {
                cellMod.ModifyCells(this, forEditor);
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
                        PlusStudioLevelFormat.Cell nearbyTile = GetCellSafe(cells[x, y].position.x + vec2.x, cells[x, y].position.y + vec2.z);
                        if ((nearbyTile == null) || (nearbyTile.roomId != cells[x, y].roomId))
                        {
                            cells[x, y].walls |= (Nybble)(1 << Directions.BitPosition(dir));
                        }
                    }
                }
            }
            ValidatePlacements(forEditor);
            ApplyCellModifiers(doors, forEditor);
            ApplyCellModifiers(windows, forEditor);
            ApplyCellModifiers(exits, forEditor);
        }

        // TODO: figure out if we even NEED to manually recalculate all cells, or if we'd just be better off moving only areas
        public bool ResizeLevel(IntVector2 posDif, IntVector2 sizeDif, EditorController toAddUndo = null)
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
            if (toAddUndo != null)
            {
                toAddUndo.AddUndo();
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
            for (int i = 0; i < doors.Count; i++)
            {
                doors[i].position -= posDif;
            }
            for (int i = 0; i < windows.Count; i++)
            {
                windows[i].position -= posDif;
            }
            for (int i = 0; i < exits.Count; i++)
            {
                exits[i].position -= posDif;
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
                if (GetCellSafe(ownedCells[i].x, ownedCells[i].z) == null) return false;
            }
            return true;
        }

        public PlusStudioLevelFormat.Cell GetCellSafe(int x, int y)
        {
            if (x < 0) return null;
            if (x >= mapSize.x) return null;
            if (y < 0) return null;
            if (y >= mapSize.z) return null;
            return cells[x, y];
        }

        public PlusStudioLevelFormat.Cell GetCellSafe(IntVector2 vec2)
        {
            return GetCellSafe(vec2.x, vec2.z);
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

        public BaldiLevel Compile()
        {
            BaldiLevel compiled = new BaldiLevel(mapSize.ToByte());
            UpdateCells(false); // update our cells
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    compiled.cells[x, y] = new PlusStudioLevelFormat.Cell(cells[x,y]);
                }
            }
            for (int i = 0; i < rooms.Count; i++)
            {
                compiled.rooms.Add(new RoomInfo(rooms[i].roomType, new TextureContainer(rooms[i].textureContainer)));
            }
            for (int i = 0; i < lights.Count; i++)
            {
                LightGroup group = lightGroups[lights[i].lightGroup];
                compiled.lights.Add(new LightInfo()
                {
                    color = group.color.ToData(),
                    position = lights[i].position.ToByte(),
                    prefab = lights[i].type,
                    strength = (byte)group.strength
                });
            }
            // TODO: during compilation, figure out which side of us is the side with the room (if any) and prioritize that room as our room.
            // this is to stop mrs pomp from dying a slow slow death if someone didnt intentionally click INSIDE the room
            for (int i = 0; i < doors.Count; i++)
            {
                if (LevelStudioPlugin.Instance.doorIsTileBased[doors[i].type])
                {
                    compiled.tileObjects.Add(new TileObjectInfo()
                    {
                        prefab = doors[i].type,
                        position = doors[i].position.ToByte(),
                        direction = (PlusDirection)doors[i].direction,
                    });
                }
                else
                {
                    compiled.doors.Add(new DoorInfo()
                    {
                        prefab = doors[i].type,
                        position = doors[i].position.ToByte(),
                        direction = (PlusDirection)doors[i].direction,
                        roomId = GetCellSafe(doors[i].position.x, doors[i].position.z).roomId
                    });
                }
            }
            for (int i = 0; i < windows.Count; i++)
            {
                compiled.windows.Add(new WindowInfo()
                {
                    prefab = windows[i].type,
                    position = windows[i].position.ToByte(),
                    direction = (PlusDirection)windows[i].direction
                });
            }
            for (int i = 0; i < exits.Count; i++)
            {
                compiled.exits.Add(new ExitInfo()
                {
                    type = exits[i].type,
                    position = exits[i].position.ToByte(),
                    direction = (PlusDirection)exits[i].direction,
                    isSpawn = exits[i].isSpawn
                });
            }
            return compiled;
        }

        public const byte version = 0;

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            StringCompressor stringComp = new StringCompressor();
            stringComp.AddStrings(lights.Select(x => x.type));
            stringComp.AddStrings(doors.Select(x => x.type));
            stringComp.AddStrings(exits.Select(x => x.type));
            stringComp.AddStrings(windows.Select(x => x.type));
            stringComp.AddStrings(rooms.Select(x => x.roomType));
            stringComp.AddStrings(rooms.Select(x => x.textureContainer.floor));
            stringComp.AddStrings(rooms.Select(x => x.textureContainer.wall));
            stringComp.AddStrings(rooms.Select(x => x.textureContainer.ceiling));
            stringComp.FinalizeDatabase();
            stringComp.WriteStringDatabase(writer);
            writer.Write((byte)mapSize.x);
            writer.Write((byte)mapSize.z);
            writer.Write(areas.Count);
            for (int i = 0; i < areas.Count; i++)
            {
                areas[i].Write(writer);
            }
            writer.Write(rooms.Count);
            for (int i = 0; i < rooms.Count; i++)
            {
                stringComp.WriteStoredString(writer, rooms[i].roomType);
                stringComp.WriteStoredString(writer, rooms[i].textureContainer.floor);
                stringComp.WriteStoredString(writer, rooms[i].textureContainer.wall);
                stringComp.WriteStoredString(writer, rooms[i].textureContainer.ceiling);
            }

            writer.Write((ushort)lightGroups.Count); // we will only allow ushort.Max amount of light groups because why the fuck would you ever need more?
            for (int i = 0; i < lightGroups.Count; i++)
            {
                writer.Write(lightGroups[i].color.ToData());
                writer.Write(lightGroups[i].strength);
            }
            writer.Write(lights.Count);
            for (int i = 0; i < lights.Count; i++)
            {
                stringComp.WriteStoredString(writer, lights[i].type);
                writer.Write(lights[i].position.ToByte());
                writer.Write(lights[i].lightGroup);
            }
            writer.Write(doors.Count);
            for (int i = 0; i < doors.Count; i++)
            {
                stringComp.WriteStoredString(writer, doors[i].type);
                writer.Write(doors[i].position.ToByte());
                writer.Write((byte)doors[i].direction);
            }
            writer.Write(windows.Count);
            for (int i = 0; i < windows.Count; i++)
            {
                stringComp.WriteStoredString(writer, windows[i].type);
                writer.Write(windows[i].position.ToByte());
                writer.Write((byte)windows[i].direction);
            }
            writer.Write(exits.Count);
            for (int i = 0; i < exits.Count; i++)
            {
                stringComp.WriteStoredString(writer, exits[i].type);
                writer.Write(exits[i].position.ToByte());
                writer.Write((byte)exits[i].direction);
                writer.Write(exits[i].isSpawn);
            }
        }

        public static EditorLevelData ReadFrom(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            StringCompressor stringComp = StringCompressor.ReadStringDatabase(reader);
            EditorLevelData levelData = new EditorLevelData(new IntVector2(reader.ReadByte(), reader.ReadByte()));
            levelData.lightGroups.Clear();
            levelData.rooms.Clear();
            int areaCount = reader.ReadInt32();
            for (int i = 0; i < areaCount; i++)
            {
                // PLACEHOLDER: TODO: actually handle types
                reader.ReadString();
                levelData.areas.Add(new RectCellArea(new IntVector2(), new IntVector2(), 0).ReadInto(reader));
            }
            int roomCount = reader.ReadInt32();
            for (int i = 0; i < roomCount; i++)
            {
                levelData.rooms.Add(new EditorRoom(stringComp.ReadStoredString(reader), new TextureContainer(stringComp.ReadStoredString(reader), stringComp.ReadStoredString(reader), stringComp.ReadStoredString(reader))));
            }
            ushort lightGroupCount = reader.ReadUInt16();
            for (int i = 0; i < lightGroupCount; i++)
            {
                levelData.lightGroups.Add(new LightGroup() { 
                    color = reader.ReadUnityColor().ToStandard(),
                    strength = reader.ReadByte()
                });
            }
            int lightCount = reader.ReadInt32();
            for (int i = 0; i < lightCount; i++)
            {
                levelData.lights.Add(new LightPlacement()
                {
                    type = stringComp.ReadStoredString(reader),
                    position = reader.ReadByteVector2().ToInt(),
                    lightGroup = reader.ReadUInt16(),
                });
            }
            int doorCount = reader.ReadInt32();
            for (int i = 0; i < doorCount; i++)
            {
                levelData.doors.Add(new DoorLocation()
                {
                    type = stringComp.ReadStoredString(reader),
                    position = reader.ReadByteVector2().ToInt(),
                    direction = (Direction)reader.ReadByte()
                });
            }
            int windowCount = reader.ReadInt32();
            for (int i = 0; i < windowCount; i++)
            {
                levelData.windows.Add(new WindowLocation()
                {
                    type = stringComp.ReadStoredString(reader),
                    position = reader.ReadByteVector2().ToInt(),
                    direction = (Direction)reader.ReadByte()
                });
            }
            int exitCount = reader.ReadInt32();
            for (int i = 0; i < exitCount; i++)
            {
                levelData.exits.Add(new ExitLocation()
                {
                    type = stringComp.ReadStoredString(reader),
                    position = reader.ReadByteVector2().ToInt(),
                    direction = (Direction)reader.ReadByte(),
                    isSpawn = reader.ReadBoolean()
                });
            }
            return levelData;
        }

    }
}
