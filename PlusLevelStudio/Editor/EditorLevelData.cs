using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    /// <summary>
    /// The level data class used by the editor itself.
    /// </summary>
    public class EditorLevelData
    {
        public IntVector2 mapSize = new IntVector2(50,50);
        public PlusStudioLevelFormat.Cell[,] cells;
        public List<CellArea> areas = new List<CellArea>();
        public List<LightGroup> lightGroups = new List<LightGroup>() { new LightGroup() };
        public List<LightPlacement> lights = new List<LightPlacement>();
        public List<EditorRoom> rooms = new List<EditorRoom>();
        public List<DoorLocation> doors = new List<DoorLocation>();
        public List<WindowLocation> windows = new List<WindowLocation>();
        public List<ExitLocation> exits = new List<ExitLocation>();
        public List<ItemPlacement> items = new List<ItemPlacement>();
        public List<ItemSpawnPlacement> itemSpawns = new List<ItemSpawnPlacement>();
        public List<BasicObjectLocation> objects = new List<BasicObjectLocation>();
        public List<StructureLocation> structures = new List<StructureLocation>();
        public List<NPCPlacement> npcs = new List<NPCPlacement>();
        public List<PosterPlacement> posters = new List<PosterPlacement>();
        public List<WallLocation> walls = new List<WallLocation>();
        public List<MarkerLocation> markers = new List<MarkerLocation>();
        public string elevatorTitle = "WIP";

        public string skybox = "daystandard";
        public Color minLightColor = new Color(0f, 0f, 0f);
        public LightMode lightMode = LightMode.Cumulative;

        public float timeLimit = 0f;
        public EditorRoom hall => rooms[0];

        public Vector3 spawnPoint = new Vector3(5f,5f,5f);
        public Direction spawnDirection = Direction.North;

        // random event stuff
        public float initialRandomEventGap = 30f;
        public float minRandomEventGap = 45f;
        public float maxRandomEventGap = 180f;
        public List<string> randomEvents = new List<string>();

        public PlayableLevelMeta meta;

        public Vector3 PracticalSpawnPoint
        {
            get
            {
                if (exits.Where(x => x.isSpawn).Count() == 0) return spawnPoint;
                ExitLocation exit = exits.Last(x => x.isSpawn);
                return new Vector3(exit.position.x * 10f + 5f, 5f, exit.position.z * 10f + 5f);
            }
        }

        public Direction PracticalSpawnDirection
        {
            get
            {
                if (exits.Where(x => x.isSpawn).Count() == 0) return spawnDirection;
                ExitLocation exit = exits.Last(x => x.isSpawn);
                return exit.direction;
            }
        }

        public Dictionary<string, TextureContainer> defaultTextures = new Dictionary<string, TextureContainer>();
        private static List<Action<Dictionary<string, TextureContainer>>> defaultTextureActions = new List<Action<Dictionary<string, TextureContainer>>>();

        /// <summary>
        /// Adds the specified action to be called whenever default textures need to be defined or redefined.
        /// </summary>
        /// <param name="action"></param>
        public static void AddDefaultTextureAction(Action<Dictionary<string, TextureContainer>> action)
        {
            defaultTextureActions.Add(action);
        }

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
            defaultTextures.Add("lightbulbtesting", new TextureContainer("MaintenanceFloor", "RedBrickWall", "ElevatorCeiling"));
            defaultTextures.Add("mystery", new TextureContainer("Black", "Black", "Black"));
            defaultTextures.Add("wormhole_room", new TextureContainer("Vent", "Vent", "Vent"));
            defaultTextures.Add("teleportroom_1", new TextureContainer("LabFloor", "LabWall", "LabCeiling"));
            defaultTextures.Add("teleportroom_2", new TextureContainer("LabFloor", "LabWall", "LabCeiling"));
            defaultTextures.Add("teleportroom_3", new TextureContainer("LabFloor", "LabWall", "LabCeiling"));
            defaultTextures.Add("teleportroom_4", new TextureContainer("LabFloor", "LabWall", "LabCeiling"));

            defaultTextures.Add("class_mathmachine", new TextureContainer("BlueCarpet", "WallWithMolding", "Ceiling"));
            defaultTextures.Add("class_matchactivity", new TextureContainer("BlueCarpet", "WallWithMolding", "Ceiling"));
            defaultTextures.Add("class_balloonbuster", new TextureContainer("BlueCarpet", "WallWithMolding", "Ceiling"));

            for (int i = 0; i < defaultTextureActions.Count; i++)
            {
                defaultTextureActions[i].Invoke(defaultTextures);
            }
        }

        public IntVector2[] GetCellsOwnedByRoom(EditorRoom room)
        {
            CellArea[] foundAreas = areas.Where(x => x.roomId == IdFromRoom(room)).ToArray();
            List<IntVector2> foundOwnedCells = new List<IntVector2>();
            foreach (CellArea area in foundAreas)
            {
                foundOwnedCells.AddRange(area.CalculateOwnedCells());
            }
            return foundOwnedCells.ToArray();
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
            EditorController.Instance.CleanupVisualsForRoom(toRemove);
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
            for (int i = exits.Count - 1; i >= 0; i--)
            {
                if (!exits[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(exits[i]);
                    }
                    exits.RemoveAt(i);
                    changedSomething = true;
                }
            }
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
            for (int i = structures.Count - 1; i >= 0; i--)
            {
                if (!structures[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(structures[i]);
                    }
                    structures.RemoveAt(i);
                    changedSomething = true;
                }
            }
            for (int i = posters.Count - 1; i >= 0; i--)
            {
                if (!posters[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(posters[i]);
                    }
                    posters.RemoveAt(i);
                    changedSomething = true;
                }
            }
            for (int i = walls.Count - 1; i >= 0; i--)
            {
                if (!walls[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(walls[i]);
                    }
                    walls.RemoveAt(i);
                    changedSomething = true;
                }
            }
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (!objects[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(objects[i]);
                    }
                    objects.RemoveAt(i);
                    changedSomething = true;
                }
            }
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (!items[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(items[i]);
                    }
                    items.RemoveAt(i);
                    changedSomething = true;
                }
            }
            for (int i = itemSpawns.Count - 1; i >= 0; i--)
            {
                if (!itemSpawns[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(itemSpawns[i]);
                    }
                    itemSpawns.RemoveAt(i);
                    changedSomething = true;
                }
            }
            for (int i = markers.Count - 1; i >= 0; i--)
            {
                if (!markers[i].ValidatePosition(this))
                {
                    if (updateVisuals)
                    {
                        EditorController.Instance.RemoveVisual(markers[i]);
                    }
                    markers.RemoveAt(i);
                    changedSomething = true;
                }
            }
            return changedSomething;
        }

        public void ValidateActivityInRoom(EditorRoom room)
        {
            if (room.activity == null) return;
            room.activity.SetupDeleteIfInvalid();
        }

        public void RemoveObjectsInArea(CellArea area)
        {
            List<IntVector2> ownedCells = area.CalculateOwnedCells().ToList();

            for (int i = objects.Count - 1; i >= 0; i--)
            {
                IntVector2 calculatedPosition = new IntVector2(Mathf.RoundToInt((objects[i].position.x - 5f) / 10f), Mathf.RoundToInt((objects[i].position.z - 5f) / 10f));
                if (ownedCells.Contains(calculatedPosition))
                {
                    objects[i].OnDelete(this);
                }
            }
            for (int i = items.Count - 1; i >= 0; i--)
            {
                IntVector2 calculatedPosition = new IntVector2(Mathf.RoundToInt((items[i].position.x - 5f) / 10f), Mathf.RoundToInt((items[i].position.y - 5f) / 10f));
                if (ownedCells.Contains(calculatedPosition))
                {
                    items[i].OnDelete(this);
                }
            }
            for (int i = itemSpawns.Count - 1; i >= 0; i--)
            {
                IntVector2 calculatedPosition = new IntVector2(Mathf.RoundToInt((itemSpawns[i].position.x - 5f) / 10f), Mathf.RoundToInt((itemSpawns[i].position.y - 5f) / 10f));
                if (ownedCells.Contains(calculatedPosition))
                {
                    itemSpawns[i].OnDelete(this);
                }
            }
            for (int i = npcs.Count - 1; i >= 0; i--)
            {
                if (ownedCells.Contains(npcs[i].position))
                {
                    npcs[i].OnDelete(this);
                }
            }
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
            ApplyCellModifiers(doors, forEditor);
            ApplyCellModifiers(windows, forEditor);
            ApplyCellModifiers(exits, forEditor);
            ApplyCellModifiers(walls, forEditor);
            ApplyCellModifiers(structures, forEditor);
            ValidatePlacements(forEditor);
        }

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

            IntVector2 oldMapSize = mapSize;
            PlusStudioLevelFormat.Cell[,] oldCells = cells;
            mapSize = newMapSize;
            // we recreate the cells from the old iteration so validation checks pass
            cells = new PlusStudioLevelFormat.Cell[mapSize.x, mapSize.z];
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    cells[x, y] = new PlusStudioLevelFormat.Cell(new ByteVector2(x, y));
                    if (((x + posDif.x) >= 0 && ((x + posDif.x) < oldMapSize.x)) && ((y + posDif.z) >= 0 && ((y + posDif.z) < oldMapSize.z)))
                    {
                        PlusStudioLevelFormat.Cell cellAt = oldCells[(x + posDif.x), (y + posDif.z)];
                        cells[x, y].walls = cellAt.walls;
                        cells[x, y].roomId = cellAt.roomId;
                    }
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
            for (int i = 0; i < items.Count; i++)
            {
                items[i].position -= new Vector2(posDif.x * 10f, posDif.z * 10f);
            }
            for (int i = 0; i < itemSpawns.Count; i++)
            {
                itemSpawns[i].position -= new Vector2(posDif.x * 10f, posDif.z * 10f);
            }
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].position -= new Vector3(posDif.x * 10f, 0f, posDif.z * 10f);
            }
            for (int i = 0; i < rooms.Count; i++)
            {
                if (rooms[i].activity == null) continue;
                rooms[i].activity.position -= new Vector3(posDif.x * 10f, 0f, posDif.z * 10f);
            }
            for (int i = 0; i < structures.Count; i++)
            {
                structures[i].ShiftBy(new Vector3(posDif.x * 10f, 0f, posDif.z * 10f), posDif, sizeDif);
            }
            for (int i = 0; i < npcs.Count; i++)
            {
                npcs[i].position -= posDif;
            }
            for (int i = 0; i < posters.Count; i++)
            {
                posters[i].position -= posDif;
            }
            for (int i = 0; i < walls.Count; i++)
            {
                walls[i].position -= posDif;
            }
            for (int i = 0; i < markers.Count; i++)
            {
                markers[i].ShiftBy(new Vector3(posDif.x * 10f, 0f, posDif.z * 10f), posDif, sizeDif);
            }
            spawnPoint -= new Vector3(posDif.x * 10f, 0f, posDif.z * 10f);
            ValidatePlacements(true);
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

        public bool GetSmartDoorPosition(IntVector2 pos, Direction dir, out IntVector2 outPos, out Direction outDir)
        {
            outPos = pos;
            outDir = dir;
            // stop us from unnecessary changing our position and dir if we are already in a non-hall room
            PlusStudioLevelFormat.Cell cellOnOurSide = GetCellSafe(pos);
            if (cellOnOurSide == null) return false;
            EditorRoom roomOnOurSide = RoomFromId(cellOnOurSide.roomId);
            if (roomOnOurSide == null) return false;
            if (roomOnOurSide.roomType != "hall") return true;
            PlusStudioLevelFormat.Cell cellOnOtherSide = GetCellSafe(pos + dir.ToIntVector2());
            if (cellOnOtherSide != null)
            {
                EditorRoom roomOnOtherSide = RoomFromId(cellOnOtherSide.roomId);
                if ((roomOnOtherSide != null))
                {
                    if (roomOnOtherSide.roomType != "hall")
                    {
                        outPos = pos + dir.ToIntVector2();
                        outDir = dir.GetOpposite();
                        return true;
                    }
                }
            }
            return false;
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

        public void FinalizeCompile(BaldiLevel toFinalize)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    if (toFinalize.cells[x, y].roomId == 0) continue;
                    if (toFinalize.rooms[toFinalize.cells[x, y].roomId - 1].type == "mystery")
                    {
                        toFinalize.secretCells[x, y] = true;
                    }
                }
            }
        }

        public BaldiLevel Compile()
        {
            BaldiLevel compiled = new BaldiLevel(mapSize.ToByte());
            compiled.levelTitle = elevatorTitle;
            compiled.timeLimit = timeLimit;
            compiled.skybox = skybox;
            compiled.spawnPoint = spawnPoint.ToData();
            compiled.spawnDirection = (PlusDirection)spawnDirection;
            compiled.randomEvents = new List<string>(randomEvents);
            compiled.minLightColor = minLightColor.ToData();
            compiled.lightMode = (PlusLightMode)lightMode;
            compiled.initialRandomEventGap = initialRandomEventGap;
            compiled.minRandomEventGap = minRandomEventGap;
            compiled.maxRandomEventGap = maxRandomEventGap;
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
                RoomInfo room = new RoomInfo(rooms[i].roomType, new TextureContainer(rooms[i].textureContainer));
                if (rooms[i].activity != null)
                {
                    room.activity = new ActivityInfo()
                    {
                        type = rooms[i].activity.type,
                        position = rooms[i].activity.position.ToData(),
                        direction = (PlusDirection)rooms[i].activity.direction
                    };
                }
                compiled.rooms.Add(room);
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
            for (int i = 0; i < doors.Count; i++)
            {
                string typeToCompileAs = doors[i].type;
                bool usedSmartPosition = GetSmartDoorPosition(doors[i].position, doors[i].direction, out IntVector2 smartPosition, out Direction smartDirection);
                bool shouldBeTile = false;
                switch (LevelStudioPlugin.Instance.doorIngameStatus[doors[i].type])
                {
                    case DoorIngameStatus.AlwaysDoor:
                        shouldBeTile = false;
                        break;
                    case DoorIngameStatus.AlwaysObject:
                        shouldBeTile = true;
                        break;
                    case DoorIngameStatus.Smart:
                        shouldBeTile = !usedSmartPosition;
                        break;
                }
                // HACKS FOR MYSTERY DOORS
                if (doors[i].type == "standard")
                {
                    if (RoomFromPos(smartPosition, false).roomType == "mystery")
                    {
                        typeToCompileAs = "mysterydoor";
                        shouldBeTile = false;
                    }
                    else if (RoomFromPos(doors[i].position + doors[i].direction.ToIntVector2(), false).roomType == "mystery")
                    {
                        smartPosition = doors[i].position + doors[i].direction.ToIntVector2();
                        smartDirection = doors[i].direction.GetOpposite();
                        typeToCompileAs = "mysterydoor";
                        shouldBeTile = false;
                    }
                }
                if (shouldBeTile)
                {
                    compiled.tileObjects.Add(new TileObjectInfo()
                    {
                        prefab = typeToCompileAs,
                        position = doors[i].position.ToByte(),
                        direction = (PlusDirection)doors[i].direction,
                    });
                }
                else
                {
                    compiled.doors.Add(new DoorInfo()
                    {
                        prefab = typeToCompileAs,
                        position = smartPosition.ToByte(),
                        direction = (PlusDirection)smartDirection,
                        roomId = GetCellSafe(smartPosition.x, smartPosition.z).roomId
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
            for (int i = 0; i < items.Count; i++)
            {
                ushort roomId = (ushort)Mathf.Max(GetCellSafe(Mathf.RoundToInt((items[i].position.x - 5f) / 10f), Mathf.RoundToInt((items[i].position.y - 5f) / 10f)).roomId, 1);
                compiled.rooms[roomId - 1].items.Add(new ItemInfo()
                {
                    item = items[i].item,
                    position = items[i].position.ToData()
                });
            }
            for (int i = 0; i < itemSpawns.Count; i++)
            {
                ushort roomId = (ushort)Mathf.Max(GetCellSafe(Mathf.RoundToInt((itemSpawns[i].position.x - 5f) / 10f), Mathf.RoundToInt((itemSpawns[i].position.y - 5f) / 10f)).roomId, 1);
                compiled.rooms[roomId - 1].itemSpawns.Add(new ItemSpawnInfo()
                {
                    weight = itemSpawns[i].weight,
                    position = itemSpawns[i].position.ToData()
                });
            }
            for (int i = 0; i < objects.Count; i++)
            {
                ushort roomId = (ushort)Mathf.Max(GetCellSafe(Mathf.RoundToInt((objects[i].position.x - 5f) / 10f), Mathf.RoundToInt((objects[i].position.z - 5f) / 10f)).roomId, 1);
                compiled.rooms[roomId - 1].basicObjects.Add(new BasicObjectInfo()
                {
                    prefab = objects[i].prefab,
                    position = objects[i].position.ToData(),
                    rotation = objects[i].rotation.ToData()
                });
            }
            for (int i = 0; i < structures.Count; i++)
            {
                compiled.structures.Add(structures[i].Compile(this, compiled));
            }
            for (int i = 0; i < npcs.Count; i++)
            {
                compiled.npcs.Add(new NPCInfo()
                {
                    npc = npcs[i].npc,
                    position = npcs[i].position.ToByte()
                });
            }
            for (int i = 0; i < posters.Count; i++)
            {
                compiled.posters.Add(new PosterInfo()
                {
                    poster = posters[i].type,
                    direction = (PlusDirection)posters[i].direction,
                    position = posters[i].position.ToByte()
                });
            }
            FinalizeCompile(compiled);
            return compiled;
        }

        public const byte version = 9;

        public bool WallFree(IntVector2 pos, Direction dir, bool ignoreSelf)
        {
            PlusStudioLevelFormat.Cell cell = GetCellSafe(pos);
            if (cell == null) return false;
            if ((((int)cell.walls) & dir.ToBinary()) == 0)
            {
                return false;
            }
            int thingsOccupying = 0;
            for (int i = 0; i < structures.Count; i++)
            {
                if (structures[i].OccupiesWall(pos, dir))
                {
                    thingsOccupying++;
                }
            }
            for (int i = 0; i < posters.Count; i++)
            {
                if (posters[i].OccupiesWall(pos, dir))
                {
                    thingsOccupying++;
                }
            }
            if (ignoreSelf)
            {
                thingsOccupying--;
            }
            return thingsOccupying <= 0;
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            StringCompressor stringComp = new StringCompressor();
            stringComp.AddStrings(lights.Select(x => x.type));
            stringComp.AddStrings(doors.Select(x => x.type));
            stringComp.AddStrings(exits.Select(x => x.type));
            stringComp.AddStrings(windows.Select(x => x.type));
            stringComp.AddStrings(items.Select(x => x.item));
            stringComp.AddStrings(objects.Select(x => x.prefab));
            stringComp.AddStrings(npcs.Select(x => x.npc));
            stringComp.AddStrings(posters.Select(x => x.type));
            stringComp.AddStrings(rooms.Select(x => x.roomType));
            stringComp.AddStrings(rooms.Select(x => x.textureContainer.floor));
            stringComp.AddStrings(rooms.Select(x => x.textureContainer.wall));
            stringComp.AddStrings(rooms.Select(x => x.textureContainer.ceiling));
            stringComp.AddStrings(rooms.Where(x => x.activity != null).Select(x => x.activity.type));
            stringComp.AddStrings(structures.Select(x => x.type));
            stringComp.AddStrings(markers.Select(x => x.type));
            structures.ForEach(x => x.AddStringsToCompressor(stringComp));
            markers.ForEach(x => x.AddStringsToCompressor(stringComp));
            stringComp.AddString("null");
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
                // write our activity
                if (rooms[i].activity == null)
                {
                    stringComp.WriteStoredString(writer, "null");
                }
                else
                {
                    stringComp.WriteStoredString(writer, rooms[i].activity.type);
                    writer.Write(rooms[i].activity.position.ToData());
                    writer.Write((byte)rooms[i].activity.direction);
                }
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
            writer.Write(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                stringComp.WriteStoredString(writer, items[i].item);
                writer.Write(items[i].position.ToData());
            }
            writer.Write(itemSpawns.Count);
            for (int i = 0; i < itemSpawns.Count; i++)
            {
                writer.Write(itemSpawns[i].weight);
                writer.Write(itemSpawns[i].position.ToData());
            }
            writer.Write(objects.Count);
            for (int i = 0; i < objects.Count; i++)
            {
                stringComp.WriteStoredString(writer, objects[i].prefab);
                writer.Write(objects[i].position.ToData());
                writer.Write(objects[i].rotation.ToData());
            }
            writer.Write(structures.Count);
            for (int i = 0; i < structures.Count; i++)
            {
                stringComp.WriteStoredString(writer, structures[i].type);
                structures[i].Write(this, writer, stringComp);
            }
            writer.Write(npcs.Count);
            for (int i = 0; i < npcs.Count; i++)
            {
                stringComp.WriteStoredString(writer, npcs[i].npc);
                writer.Write(npcs[i].position.ToByte());
            }
            writer.Write(posters.Count);
            for (int i = 0; i < posters.Count; i++)
            {
                stringComp.WriteStoredString(writer, posters[i].type);
                writer.Write(posters[i].position.ToByte());
                writer.Write((byte)posters[i].direction);
            }
            WallLocation[] addWalls = walls.Where(x => x.wallState).ToArray();
            writer.Write(addWalls.Length);
            for (int i = 0; i < addWalls.Length; i++)
            {
                writer.Write(addWalls[i].position.ToByte());
                writer.Write((byte)addWalls[i].direction);
            }
            WallLocation[] removeWalls = walls.Where(x => !x.wallState).ToArray();
            writer.Write(removeWalls.Length);
            for (int i = 0; i < removeWalls.Length; i++)
            {
                writer.Write(removeWalls[i].position.ToByte());
                writer.Write((byte)removeWalls[i].direction);
            }
            writer.Write(markers.Count);
            for (int i = 0; i < markers.Count; i++)
            {
                stringComp.WriteStoredString(writer, markers[i].type);
                markers[i].Write(this, writer, stringComp);
            }
            writer.Write(spawnPoint.ToData());
            writer.Write((byte)spawnDirection);
            writer.Write(elevatorTitle);
            writer.Write(timeLimit);
            writer.Write(initialRandomEventGap);
            writer.Write(minRandomEventGap);
            writer.Write(maxRandomEventGap);
            writer.Write(randomEvents.Count);
            // dont need to use the string compressor because these strings only appear once so it'd save no space (infact it'd take up space due to extra bytes needed for the indexes)
            for (int i = 0; i < randomEvents.Count; i++)
            {
                writer.Write(randomEvents[i]);
            }
            writer.Write(skybox);
            writer.Write(minLightColor.ToData());
            writer.Write((byte)lightMode);
            meta.Write(writer);
        }

        public static EditorLevelData ReadFrom(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            if (version > EditorLevelData.version) throw new Exception("Attempted to read file with newer version number than the one supported! (Got: " + version + " expected: " + EditorLevelData.version + " or below)");
            StringCompressor stringComp = StringCompressor.ReadStringDatabase(reader);
            string title = "WIP";
            if (version == 2) // this was only written here for one version
            {
                title = reader.ReadString();
            }
            EditorLevelData levelData = new EditorLevelData(new IntVector2(reader.ReadByte(), reader.ReadByte()));
            levelData.elevatorTitle = title;
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
                EditorRoom room = new EditorRoom(stringComp.ReadStoredString(reader), new TextureContainer(stringComp.ReadStoredString(reader), stringComp.ReadStoredString(reader), stringComp.ReadStoredString(reader)));
                string activityName = stringComp.ReadStoredString(reader);
                if (activityName != "null")
                {
                    ActivityLocation activity = new ActivityLocation()
                    {
                        type=activityName,
                        position=reader.ReadUnityVector3().ToUnity(),
                        direction=(Direction)reader.ReadByte()
                    };
                    activity.Setup(room);
                }
                levelData.rooms.Add(room);
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
            int itemCount = reader.ReadInt32();
            for (int i = 0; i < itemCount; i++)
            {
                levelData.items.Add(new ItemPlacement()
                {
                    item = stringComp.ReadStoredString(reader),
                    position = reader.ReadUnityVector2().ToUnity(),
                });
            }
            if (version >= 8)
            {
                int itemSpawnCount = reader.ReadInt32();
                for (int i = 0; i < itemSpawnCount; i++)
                {
                    levelData.itemSpawns.Add(new ItemSpawnPlacement()
                    {
                        weight = reader.ReadInt32(),
                        position = reader.ReadUnityVector2().ToUnity(),
                    });
                }
            }
            int objectCount = reader.ReadInt32();
            for (int i = 0; i < objectCount; i++)
            {
                levelData.objects.Add(new BasicObjectLocation()
                {
                    prefab = stringComp.ReadStoredString(reader),
                    position = reader.ReadUnityVector3().ToUnity(),
                    rotation = reader.ReadUnityQuaternion().ToUnity()
                });
            }
            int structureCount = reader.ReadInt32();
            for (int i = 0; i < structureCount; i++)
            {
                string type = stringComp.ReadStoredString(reader);
                StructureLocation structure = LevelStudioPlugin.Instance.ConstructStructureOfType(type);
                structure.ReadInto(levelData, reader, stringComp);
                levelData.structures.Add(structure);
            }
            int npcCount = reader.ReadInt32();
            for (int i = 0; i < npcCount; i++)
            {
                levelData.npcs.Add(new NPCPlacement()
                {
                    npc=stringComp.ReadStoredString(reader),
                    position=reader.ReadByteVector2().ToInt()
                });
            }
            int posterCount = reader.ReadInt32();
            for (int i = 0; i < posterCount; i++)
            {
                levelData.posters.Add(new PosterPlacement()
                {
                    type=stringComp.ReadStoredString(reader),
                    position=reader.ReadByteVector2().ToInt(),
                    direction=(Direction)reader.ReadByte()
                });
            }
            if (version < 7)
            {
                levelData.meta = new PlayableLevelMeta()
                {
                    name = levelData.elevatorTitle,
                    modeSettings = LevelStudioPlugin.Instance.gameModeAliases["standard"].CreateSettings(),
                    gameMode = "standard", // all versions of EditorLevelData before this point only officially support the "full" mode, so we can assume standard here
                    contentPackage = new EditorCustomContentPackage(true)
                };
            }
            if (version == 0)
            {
                levelData.spawnPoint = reader.ReadUnityVector3().ToUnity();
                levelData.spawnDirection = (Direction)reader.ReadByte();
                return levelData;
            }
            int addWallsCount = reader.ReadInt32();
            for (int i = 0; i < addWallsCount; i++)
            {
                levelData.walls.Add(new WallLocation()
                {
                    wallState = true,
                    position=reader.ReadByteVector2().ToInt(),
                    direction=(Direction)reader.ReadByte()
                });
            }
            int removeWallsCount = reader.ReadInt32();
            for (int i = 0; i < removeWallsCount; i++)
            {
                levelData.walls.Add(new WallLocation()
                {
                    wallState = false,
                    position = reader.ReadByteVector2().ToInt(),
                    direction = (Direction)reader.ReadByte()
                });
            }
            if (version >= 9)
            {
                int markerCount = reader.ReadInt32();
                for (int i = 0; i < markerCount; i++)
                {
                    string type = stringComp.ReadStoredString(reader);
                    MarkerLocation marker = LevelStudioPlugin.Instance.ConstructMarkerOfType(type);
                    marker.ReadInto(levelData, reader, stringComp);
                    levelData.markers.Add(marker);
                }
            }
            levelData.spawnPoint = reader.ReadUnityVector3().ToUnity();
            levelData.spawnDirection = (Direction)reader.ReadByte();
            if (version < 3) return levelData;
            levelData.elevatorTitle = reader.ReadString();
            if (version >= 4)
            {
                levelData.timeLimit = reader.ReadSingle();
            }
            levelData.initialRandomEventGap = reader.ReadSingle();
            levelData.minRandomEventGap = reader.ReadSingle();
            levelData.maxRandomEventGap = reader.ReadSingle();
            int randomEventCount = reader.ReadInt32();
            for (int i = 0; i < randomEventCount; i++)
            {
                levelData.randomEvents.Add(reader.ReadString());
            }
            if (version <= 4) return levelData;
            levelData.skybox = reader.ReadString();
            if (version <= 5) return levelData;
            levelData.minLightColor = reader.ReadUnityColor().ToStandard();
            levelData.lightMode = (LightMode)reader.ReadByte();
            if (version <= 6) return levelData;
            levelData.meta = PlayableLevelMeta.Read(reader, true);
            return levelData;
        }

    }
}
