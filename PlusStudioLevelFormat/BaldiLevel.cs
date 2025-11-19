using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace PlusStudioLevelFormat
{
    public class BaldiLevel
    {
        public ByteVector2 levelSize;
        public Cell[,] cells;
        public bool[,] entitySafeCells;
        public bool[,] eventSafeCells;
        public bool[,] secretCells;
        public PlusCellCoverage[,] coverage;
        public List<RoomInfo> rooms = new List<RoomInfo>();
        public List<LightInfo> lights = new List<LightInfo>();
        public List<TileObjectInfo> tileObjects = new List<TileObjectInfo>();
        public List<DoorInfo> doors = new List<DoorInfo>();
        public List<WindowInfo> windows = new List<WindowInfo>();
        public List<ExitInfo> exits = new List<ExitInfo>();
        public List<StructureInfo> structures = new List<StructureInfo>();
        public List<RandomStructureInfo> randomStructures = new List<RandomStructureInfo>();
        public List<NPCInfo> npcs = new List<NPCInfo>();
        public List<PosterInfo> posters = new List<PosterInfo>();
        public List<RoomPlacementInfo> premadeRoomPlacements = new List<RoomPlacementInfo>();

        public UnityVector3 spawnPoint = new UnityVector3(5f,5f,5f);
        public UnityVector3 UnitySpawnPoint
        {
            get
            {
                if (exits.Where(x => x.isSpawn).Count() == 0) return spawnPoint;
                ExitInfo exit = exits.Last(x => x.isSpawn);
                return new UnityVector3(exit.position.x * 10f + 5f, 5f, exit.position.y * 10f + 5f);
            }
        }
        public PlusDirection spawnDirection = PlusDirection.North;
        public static readonly byte version = 8;
        public string levelTitle = "WIP";
        public float timeLimit = 0f;

        public string skybox = "default";
        public UnityColor minLightColor = new UnityColor(0f,0f,0f);
        public PlusLightMode lightMode = PlusLightMode.Cumulative;

        // random event stuff
        public float initialRandomEventGap = 30f;
        public float minRandomEventGap = 45f;
        public float maxRandomEventGap = 180f;
        public List<string> randomEvents = new List<string>();

        // misc stuff
        public int seed = 0;
        public List<WeightedID> potentialStickers = new List<WeightedID>();

        public List<WeightedID> potentialStoreItems = new List<WeightedID>();
        public int storeItemCount = 0;

        public bool usesMap = true;

        /// <summary>
        /// Creates a new level that is properly initialized with the specified width and height
        /// </summary>
        /// <param name="size"></param>
        public BaldiLevel(ByteVector2 size)
        {
            Initialize(size);
        }

        /// <summary>
        /// Creates an uninitialized level. Call .Initialize before attempting to access cells or other such data!
        /// </summary>
        public BaldiLevel()
        {

        }

        /// <summary>
        /// Initializes the cells for the specified BaldiLevel
        /// </summary>
        /// <param name="size"></param>
        public void Initialize(ByteVector2 size)
        {
            levelSize = size;
            cells = new Cell[size.x, size.y];
            entitySafeCells = new bool[size.x, size.y];
            eventSafeCells = new bool[size.x, size.y];
            secretCells = new bool[size.x, size.y];
            coverage = new PlusCellCoverage[size.x, size.y];
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    cells[x, y] = new Cell(new ByteVector2(x, y));
                    entitySafeCells[x, y] = false;
                    eventSafeCells[x, y] = false;
                    secretCells[x, y] = false;
                    coverage[x, y] = PlusCellCoverage.None;
                }
            }
        }

        public static BaldiLevel Read(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            StringCompressor roomCompressor = StringCompressor.ReadStringDatabase(reader);
            StringCompressor objectsCompressor = StringCompressor.ReadStringDatabase(reader);
            BaldiLevel level = new BaldiLevel();
            // metadata
            level.levelTitle = reader.ReadString();
            level.timeLimit = reader.ReadSingle();
            if (version >= 3)
            {
                level.seed = reader.ReadInt32();
            }
            if (version >= 5)
            {
                level.usesMap = reader.ReadBoolean();
            }

            level.skybox = reader.ReadString();
            level.minLightColor = reader.ReadUnityColor();
            level.lightMode = (PlusLightMode)reader.ReadByte();

            level.initialRandomEventGap = reader.ReadSingle();
            level.minRandomEventGap = reader.ReadSingle();
            level.maxRandomEventGap = reader.ReadSingle();
            int eventCount = reader.ReadInt32();
            for (int i = 0; i < eventCount; i++)
            {
                level.randomEvents.Add(reader.ReadString());
            }
            level.spawnPoint = reader.ReadUnityVector3();
            level.spawnDirection = (PlusDirection)reader.ReadByte();
            // actual data
            level.Initialize(reader.ReadByteVector2());
            Nybble[] wallNybbles = reader.ReadNybbles();
            // todo: replace with proper method of reading back the nybbles
            int cellIndex = 0;
            for (int x = 0; x < level.levelSize.x; x++)
            {
                for (int y = 0; y < level.levelSize.y; y++)
                {
                    level.cells[x, y].walls = wallNybbles[cellIndex];
                    cellIndex++;
                }
            }
            for (int x = 0; x < level.levelSize.x; x++)
            {
                for (int y = 0; y < level.levelSize.y; y++)
                {
                    level.cells[x, y].roomId = reader.ReadUInt16();
                }
            }
            bool[] entitySafe = reader.ReadBoolArray();
            for (int x = 0; x < level.levelSize.x; x++)
            {
                for (int y = 0; y < level.levelSize.y; y++)
                {
                    level.entitySafeCells[x, y] = entitySafe[(x * level.levelSize.y) + y];
                }
            }
            bool[] eventSafe = reader.ReadBoolArray();
            for (int x = 0; x < level.levelSize.x; x++)
            {
                for (int y = 0; y < level.levelSize.y; y++)
                {
                    level.eventSafeCells[x, y] = eventSafe[(x * level.levelSize.y) + y];
                }
            }
            bool[] secretCells = reader.ReadBoolArray();
            for (int x = 0; x < level.levelSize.x; x++)
            {
                for (int y = 0; y < level.levelSize.y; y++)
                {
                    level.secretCells[x, y] = secretCells[(x * level.levelSize.y) + y];
                }
            }
            for (int x = 0; x < level.levelSize.x; x++)
            {
                for (int y = 0; y < level.levelSize.y; y++)
                {
                    level.coverage[x, y] = (PlusCellCoverage)reader.ReadByte();
                }
            }
            int roomCount = reader.ReadInt32();
            for (int i = 0; i < roomCount; i++)
            {
                RoomInfo room;
                if (version < 6)
                {
                    room = new RoomInfo(roomCompressor.ReadStoredString(reader), new TextureContainer(roomCompressor.ReadStoredString(reader), roomCompressor.ReadStoredString(reader), roomCompressor.ReadStoredString(reader)));
                }
                else
                {
                    room = new RoomInfo(roomCompressor.ReadStoredString(reader), reader.ReadString(), new TextureContainer(roomCompressor.ReadStoredString(reader), roomCompressor.ReadStoredString(reader), roomCompressor.ReadStoredString(reader)));
                }
                int itemCount = reader.ReadInt32();
                for (int j = 0; j < itemCount; j++)
                {
                    room.items.Add(new ItemInfo()
                    {
                        item = objectsCompressor.ReadStoredString(reader),
                        position = reader.ReadUnityVector2()
                    });
                }
                if (version >= 1)
                {
                    int itemSpawnCount = reader.ReadInt32();
                    for (int j = 0; j < itemSpawnCount; j++)
                    {
                        room.itemSpawns.Add(new ItemSpawnInfo()
                        {
                            weight = reader.ReadInt32(),
                            position = reader.ReadUnityVector2()
                        });
                    }
                }
                int basicObjectCount = reader.ReadInt32();
                for (int j = 0; j < basicObjectCount; j++)
                {
                    room.basicObjects.Add(new BasicObjectInfo()
                    {
                        prefab = objectsCompressor.ReadStoredString(reader),
                        position = reader.ReadUnityVector3(),
                        rotation = reader.ReadUnityQuaternion()
                    });
                }
                string activityName = objectsCompressor.ReadStoredString(reader);
                if (activityName != "null")
                {
                    ActivityInfo activity = new ActivityInfo()
                    {
                        type=activityName,
                        position=reader.ReadUnityVector3(),
                        direction=(PlusDirection)reader.ReadByte()
                    };
                    room.activity = activity;
                }
                level.rooms.Add(room);
            }
            int lightCount = reader.ReadInt32();
            for (int i = 0; i < lightCount; i++)
            {
                level.lights.Add(new LightInfo()
                {
                    prefab = objectsCompressor.ReadStoredString(reader),
                    position = reader.ReadByteVector2(),
                    color = reader.ReadUnityColor(),
                    strength = reader.ReadByte()
                });
            }
            int doorCount = reader.ReadInt32();
            for (int i = 0; i < doorCount; i++)
            {
                level.doors.Add(new DoorInfo()
                {
                    prefab=objectsCompressor.ReadStoredString(reader),
                    position = reader.ReadByteVector2(),
                    direction = (PlusDirection)reader.ReadByte(),
                    roomId = reader.ReadUInt16()
                });
            }
            int windowsCount = reader.ReadInt32();
            for (int i = 0; i < windowsCount; i++)
            {
                level.windows.Add(new WindowInfo()
                {
                    prefab = objectsCompressor.ReadStoredString(reader),
                    position = reader.ReadByteVector2(),
                    direction = (PlusDirection)reader.ReadByte()
                });
            }
            int tileObjectCount = reader.ReadInt32();
            for (int i = 0; i < tileObjectCount; i++)
            {
                level.tileObjects.Add(new TileObjectInfo()
                {
                    prefab = objectsCompressor.ReadStoredString(reader),
                    position = reader.ReadByteVector2(),
                    direction = (PlusDirection)reader.ReadByte(),
                });
            }
            int exitCount = reader.ReadInt32();
            for (int i = 0; i < exitCount; i++)
            {
                level.exits.Add(new ExitInfo()
                {
                    type = objectsCompressor.ReadStoredString(reader),
                    position = reader.ReadByteVector2(),
                    direction = (PlusDirection)reader.ReadByte(),
                    isSpawn = reader.ReadBoolean()
                });
            }
            int structureCount = reader.ReadInt32();
            for (int i = 0; i < structureCount; i++)
            {
                StructureInfo info = new StructureInfo();
                info.type = objectsCompressor.ReadStoredString(reader);
                int dataCount = reader.ReadInt32();
                for (int j = 0; j < dataCount; j++)
                {
                    if (version >= 2)
                    {
                        info.data.Add(new StructureDataInfo()
                        {
                            prefab = objectsCompressor.ReadStoredString(reader),
                            position = reader.ReadMystIntVector2(),
                            direction = (PlusDirection)reader.ReadByte(),
                            data = reader.ReadInt32()
                        });
                    }
                    else
                    {
                        info.data.Add(new StructureDataInfo()
                        {
                            prefab = objectsCompressor.ReadStoredString(reader),
                            position = new MystIntVector2(reader.ReadByte(), reader.ReadByte()),
                            direction = (PlusDirection)reader.ReadByte(),
                            data = reader.ReadInt32()
                        });
                    }
                }
                level.structures.Add(info);
            }
            int npcCount = reader.ReadInt32();
            for (int i = 0; i < npcCount; i++)
            {
                level.npcs.Add(new NPCInfo()
                {
                    npc=objectsCompressor.ReadStoredString(reader),
                    position=reader.ReadByteVector2(),
                });
            }
            int posterCount = reader.ReadInt32();
            for (int i = 0; i < posterCount; i++)
            {
                level.posters.Add(new PosterInfo()
                {
                    poster = objectsCompressor.ReadStoredString(reader),
                    position = reader.ReadByteVector2(),
                    direction = (PlusDirection)reader.ReadByte(),
                });
            }
            if (version <= 2) return level;
            int randomStructureCount = reader.ReadInt32();
            for (int i = 0; i < randomStructureCount; i++)
            {
                RandomStructureInfo randomInfo = new RandomStructureInfo();
                randomInfo.type = objectsCompressor.ReadStoredString(reader);
                int chanceCount = reader.ReadInt32();
                for (int j = 0; j < chanceCount; j++)
                {
                    randomInfo.info.chance.Add(reader.ReadSingle());
                }
                int minMaxCount = reader.ReadInt32();
                for (int j = 0; j < minMaxCount; j++)
                {
                    randomInfo.info.minMax.Add(reader.ReadMystIntVector2());
                }
                int prefabCount = reader.ReadInt32();
                for (int j = 0; j < prefabCount; j++)
                {
                    randomInfo.info.prefab.Add(new WeightedPrefab() { prefab = objectsCompressor.ReadStoredString(reader), weight = reader.ReadInt32() });
                }
                level.randomStructures.Add(randomInfo);
            }
            if (version <= 3) return level;
            int stickerCount = reader.ReadInt32();
            for (int i = 0; i < stickerCount; i++)
            {
                level.potentialStickers.Add(new WeightedID()
                {
                    id = reader.ReadString(),
                    weight = reader.ReadInt32()
                });
            }
            if (version >= 8)
            {
                level.storeItemCount = reader.ReadInt32();
                int storeCount = reader.ReadInt32();
                for (int i = 0; i < storeCount; i++)
                {
                    level.potentialStoreItems.Add(new WeightedID()
                    {
                        id = reader.ReadString(),
                        weight = reader.ReadInt32()
                    });
                }
            }
            if (version <= 7) return level;
            int premadeRoomCount = reader.ReadInt32();
            for (int i = 0; i < premadeRoomCount; i++)
            {
                RoomPlacementInfo info = new RoomPlacementInfo()
                {
                    room = roomCompressor.ReadStoredString(reader),
                    position = reader.ReadByteVector2(),
                    direction = (PlusDirection)reader.ReadByte(),
                    doorSpawnId = reader.ReadInt32(),
                };
                if (reader.ReadBoolean())
                {
                    info.textureOverride = new TextureContainer(roomCompressor.ReadStoredString(reader), roomCompressor.ReadStoredString(reader), roomCompressor.ReadStoredString(reader));
                }
                level.premadeRoomPlacements.Add(info);
            }
            return level;
        }

        public void Write(BinaryWriter writer)
        {
            StringCompressor roomCompressor = new StringCompressor();
            roomCompressor.AddStrings(rooms.Select(x => x.type));
            roomCompressor.AddStrings(rooms.Select(x => x.textureContainer.floor));
            roomCompressor.AddStrings(rooms.Select(x => x.textureContainer.wall));
            roomCompressor.AddStrings(rooms.Select(x => x.textureContainer.ceiling));
            roomCompressor.AddStrings(premadeRoomPlacements.Select(x => x.room));
            for (int i = 0; i < premadeRoomPlacements.Count; i++)
            {
                if (premadeRoomPlacements[i].textureOverride != null)
                {
                    roomCompressor.AddString(premadeRoomPlacements[i].textureOverride.floor);
                    roomCompressor.AddString(premadeRoomPlacements[i].textureOverride.wall);
                    roomCompressor.AddString(premadeRoomPlacements[i].textureOverride.ceiling);
                }
            }
            StringCompressor objectsCompressor = new StringCompressor();
            foreach (var room in rooms)
            {
                objectsCompressor.AddStrings(room.items.Select(x => x.item));
                objectsCompressor.AddStrings(room.basicObjects.Select(x => x.prefab));
                if (room.activity == null)
                {
                    objectsCompressor.AddString("null");
                }
                else
                {
                    objectsCompressor.AddString(room.activity.type);
                }
            }
            objectsCompressor.AddStrings(lights.Select(x => x.prefab));
            objectsCompressor.AddStrings(doors.Select(x => x.prefab));
            objectsCompressor.AddStrings(windows.Select(x => x.prefab));
            objectsCompressor.AddStrings(tileObjects.Select(x => x.prefab));
            objectsCompressor.AddStrings(exits.Select(x => x.type));
            objectsCompressor.AddStrings(npcs.Select(x => x.npc));
            objectsCompressor.AddStrings(posters.Select(x => x.poster));
            objectsCompressor.AddStrings(structures.Select(x => x.type));
            objectsCompressor.AddStrings(randomStructures.Select(x => x.type));
            objectsCompressor.AddStrings(randomStructures.SelectMany(x => x.info.prefab.Select(z => z.prefab)));
            foreach (var structure in structures)
            {
                objectsCompressor.AddStrings(structure.data.Where(x => x.prefab != null).Select(x => x.prefab));
            }
            objectsCompressor.FinalizeDatabase();
            roomCompressor.FinalizeDatabase();
            writer.Write(version);
            // write string databases
            roomCompressor.WriteStringDatabase(writer);
            objectsCompressor.WriteStringDatabase(writer);
            // write spawn position or other metadata
            writer.Write(levelTitle);
            writer.Write(timeLimit);
            writer.Write(seed);
            writer.Write(usesMap);

            writer.Write(skybox);
            writer.Write(minLightColor);
            writer.Write((byte)lightMode);

            writer.Write(initialRandomEventGap);
            writer.Write(minRandomEventGap);
            writer.Write(maxRandomEventGap);
            writer.Write(randomEvents.Count);
            for (int i = 0; i < randomEvents.Count; i++)
            {
                writer.Write(randomEvents[i]);
            }
            writer.Write(spawnPoint);
            writer.Write((byte)spawnDirection);
            // write level cell data split into nybble list for the walls and an array of room ids
            // this is done in two steps so the code for reading is easy, and writing a nybble by itself and not in a pair has no value over using a byte
            writer.Write(levelSize);
            List<Nybble> nybbles = new List<Nybble>();
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    nybbles.Add(cells[x,y].walls);
                }
            }
            writer.Write(nybbles.ToArray());
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    writer.Write(cells[x,y].roomId);
                }
            }
            List<bool> bools = new List<bool>();
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    bools.Add(entitySafeCells[x, y]);
                }
            }
            writer.Write(bools.ToArray()); // write entitySafeCells
            bools.Clear();
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    bools.Add(eventSafeCells[x, y]);
                }
            }
            writer.Write(bools.ToArray()); // write eventSafeCells
            bools.Clear();
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    bools.Add(secretCells[x, y]);
                }
            }
            writer.Write(bools.ToArray()); // write secretCells
            // write the coverage
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    writer.Write((byte)coverage[x, y]);
                }
            }
            // write rooms
            writer.Write(rooms.Count);
            for (int i = 0; i < rooms.Count; i++)
            {
                // write the type
                // then the floor, wall, and ceiling textures
                roomCompressor.WriteStoredString(writer, rooms[i].type);
                writer.Write(rooms[i].name);
                roomCompressor.WriteStoredString(writer, rooms[i].textureContainer.floor);
                roomCompressor.WriteStoredString(writer, rooms[i].textureContainer.wall);
                roomCompressor.WriteStoredString(writer, rooms[i].textureContainer.ceiling);
                writer.Write(rooms[i].items.Count);
                for (int j = 0; j < rooms[i].items.Count; j++)
                {
                    objectsCompressor.WriteStoredString(writer, rooms[i].items[j].item);
                    writer.Write(rooms[i].items[j].position);
                }
                writer.Write(rooms[i].itemSpawns.Count);
                for (int j = 0; j < rooms[i].itemSpawns.Count; j++)
                {
                    writer.Write(rooms[i].itemSpawns[j].weight);
                    writer.Write(rooms[i].itemSpawns[j].position);
                }
                writer.Write(rooms[i].basicObjects.Count);
                for (int j = 0; j < rooms[i].basicObjects.Count; j++)
                {
                    objectsCompressor.WriteStoredString(writer, rooms[i].basicObjects[j].prefab);
                    writer.Write(rooms[i].basicObjects[j].position);
                    writer.Write(rooms[i].basicObjects[j].rotation);
                }
                if (rooms[i].activity == null)
                {
                    objectsCompressor.WriteStoredString(writer, "null");
                }
                else
                {
                    objectsCompressor.WriteStoredString(writer, rooms[i].activity.type);
                    writer.Write(rooms[i].activity.position);
                    writer.Write((byte)rooms[i].activity.direction);
                }
            }
            writer.Write(lights.Count);
            for (int i = 0; i < lights.Count; i++)
            {
                objectsCompressor.WriteStoredString(writer, lights[i].prefab);
                writer.Write(lights[i].position);
                writer.Write(lights[i].color);
                writer.Write(lights[i].strength);
            }
            writer.Write(doors.Count);
            for (int i = 0; i < doors.Count; i++)
            {
                objectsCompressor.WriteStoredString(writer, doors[i].prefab);
                writer.Write(doors[i].position);
                writer.Write((byte)doors[i].direction);
                writer.Write(doors[i].roomId);
            }
            writer.Write(windows.Count);
            for (int i = 0; i < windows.Count; i++)
            {
                objectsCompressor.WriteStoredString(writer, windows[i].prefab);
                writer.Write(windows[i].position);
                writer.Write((byte)windows[i].direction);
            }
            writer.Write(tileObjects.Count);
            for (int i = 0; i < tileObjects.Count; i++)
            {
                objectsCompressor.WriteStoredString(writer, tileObjects[i].prefab);
                writer.Write(tileObjects[i].position);
                writer.Write((byte)tileObjects[i].direction);
            }
            writer.Write(exits.Count);
            for (int i = 0; i < exits.Count; i++)
            {
                objectsCompressor.WriteStoredString(writer, exits[i].type);
                writer.Write(exits[i].position);
                writer.Write((byte)exits[i].direction);
                writer.Write(exits[i].isSpawn);
            }
            writer.Write(structures.Count);
            for (int i = 0; i < structures.Count; i++)
            {
                objectsCompressor.WriteStoredString(writer, structures[i].type);
                writer.Write(structures[i].data.Count);
                for (int j = 0; j < structures[i].data.Count; j++)
                {
                    StructureDataInfo data = structures[i].data[j];
                    objectsCompressor.WriteStoredString(writer, data.prefab);
                    writer.Write(data.position);
                    writer.Write((byte)data.direction);
                    writer.Write(data.data);
                }
            }
            writer.Write(npcs.Count);
            for (int i = 0; i < npcs.Count; i++)
            {
                objectsCompressor.WriteStoredString(writer, npcs[i].npc);
                writer.Write(npcs[i].position);
            }
            writer.Write(posters.Count);
            for (int i = 0; i < posters.Count; i++)
            {
                objectsCompressor.WriteStoredString(writer, posters[i].poster);
                writer.Write(posters[i].position);
                writer.Write((byte)posters[i].direction);
            }
            writer.Write(randomStructures.Count);
            for (int i = 0; i < randomStructures.Count; i++)
            {
                objectsCompressor.WriteStoredString(writer, randomStructures[i].type);
                writer.Write(randomStructures[i].info.chance.Count);
                for (int j = 0; j < randomStructures[i].info.chance.Count; j++)
                {
                    writer.Write(randomStructures[i].info.chance[j]);
                }
                writer.Write(randomStructures[i].info.minMax.Count);
                for (int j = 0; j < randomStructures[i].info.minMax.Count; j++)
                {
                    writer.Write(randomStructures[i].info.minMax[j]);
                }
                writer.Write(randomStructures[i].info.prefab.Count);
                for (int j = 0; j < randomStructures[i].info.prefab.Count; j++)
                {
                    objectsCompressor.WriteStoredString(writer, randomStructures[i].info.prefab[j].prefab);
                    writer.Write(randomStructures[i].info.prefab[j].weight);
                }
            }
            writer.Write(potentialStickers.Count);
            for (int i = 0; i < potentialStickers.Count; i++)
            {
                writer.Write(potentialStickers[i].id);
                writer.Write(potentialStickers[i].weight);
            }
            writer.Write(storeItemCount);
            writer.Write(potentialStoreItems.Count);
            for (int i = 0; i < potentialStoreItems.Count; i++)
            {
                writer.Write(potentialStoreItems[i].id);
                writer.Write(potentialStoreItems[i].weight);
            }
            writer.Write(premadeRoomPlacements.Count);
            for (int i = 0; i < premadeRoomPlacements.Count; i++)
            {
                roomCompressor.WriteStoredString(writer, premadeRoomPlacements[i].room);
                writer.Write(premadeRoomPlacements[i].position);
                writer.Write((byte)premadeRoomPlacements[i].direction);
                writer.Write(premadeRoomPlacements[i].doorSpawnId);
                writer.Write(premadeRoomPlacements[i].textureOverride != null);
                if (premadeRoomPlacements[i].textureOverride != null)
                {
                    roomCompressor.WriteStoredString(writer, premadeRoomPlacements[i].textureOverride.floor);
                    roomCompressor.WriteStoredString(writer, premadeRoomPlacements[i].textureOverride.wall);
                    roomCompressor.WriteStoredString(writer, premadeRoomPlacements[i].textureOverride.ceiling);
                }
            }
        }
    }
}
