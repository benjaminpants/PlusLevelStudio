using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PlusStudioLevelFormat
{
    public class BaldiLevel
    {
        public ByteVector2 levelSize;
        public Cell[,] cells;
        public List<RoomInfo> rooms = new List<RoomInfo>();
        public List<LightInfo> lights = new List<LightInfo>();
        public List<TileObjectInfo> tileObjects = new List<TileObjectInfo>();
        public List<DoorInfo> doors = new List<DoorInfo>();
        public List<WindowInfo> windows = new List<WindowInfo>();
        public List<ExitInfo> exits = new List<ExitInfo>();
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
        public static readonly byte version = 0;

        public BaldiLevel(ByteVector2 size)
        {
            levelSize = size;
            cells = new Cell[size.x, size.y];
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    cells[x, y] = new Cell(new ByteVector2(x, y));
                }
            }
        }

        public static BaldiLevel Read(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            StringCompressor roomCompressor = StringCompressor.ReadStringDatabase(reader);
            StringCompressor objectsCompressor = StringCompressor.ReadStringDatabase(reader);
            UnityVector3 spawnPoint = reader.ReadUnityVector3();
            PlusDirection spawnDirection = (PlusDirection)reader.ReadByte();
            BaldiLevel level = new BaldiLevel(reader.ReadByteVector2());
            level.spawnPoint = spawnPoint;
            level.spawnDirection = spawnDirection;
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
            int roomCount = reader.ReadInt32();
            for (int i = 0; i < roomCount; i++)
            {
                RoomInfo room = new RoomInfo(roomCompressor.ReadStoredString(reader), new TextureContainer(roomCompressor.ReadStoredString(reader), roomCompressor.ReadStoredString(reader), roomCompressor.ReadStoredString(reader)));
                int itemCount = reader.ReadInt32();
                for (int j = 0; j < itemCount; j++)
                {
                    room.items.Add(new ItemInfo()
                    {
                        item = objectsCompressor.ReadStoredString(reader),
                        position = reader.ReadUnityVector2()
                    });
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
            return level;
        }

        public void Write(BinaryWriter writer)
        {
            StringCompressor roomCompressor = new StringCompressor();
            roomCompressor.AddStrings(rooms.Select(x => x.type));
            roomCompressor.AddStrings(rooms.Select(x => x.textureContainer.floor));
            roomCompressor.AddStrings(rooms.Select(x => x.textureContainer.wall));
            roomCompressor.AddStrings(rooms.Select(x => x.textureContainer.ceiling));
            StringCompressor objectsCompressor = new StringCompressor();
            foreach (var room in rooms)
            {
                objectsCompressor.AddStrings(room.items.Select(x => x.item));
                objectsCompressor.AddStrings(room.basicObjects.Select(x => x.prefab));
            }
            objectsCompressor.AddStrings(lights.Select(x => x.prefab));
            objectsCompressor.AddStrings(doors.Select(x => x.prefab));
            objectsCompressor.AddStrings(windows.Select(x => x.prefab));
            objectsCompressor.AddStrings(tileObjects.Select(x => x.prefab));
            objectsCompressor.AddStrings(exits.Select(x => x.type));
            objectsCompressor.FinalizeDatabase();
            roomCompressor.FinalizeDatabase();
            writer.Write(version);
            // write string databases
            roomCompressor.WriteStringDatabase(writer);
            objectsCompressor.WriteStringDatabase(writer);
            // write spawn position or other metadata
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
            // write rooms
            writer.Write(rooms.Count);
            for (int i = 0; i < rooms.Count; i++)
            {
                // write the type
                // then the floor, wall, and ceiling textures
                roomCompressor.WriteStoredString(writer, rooms[i].type);
                roomCompressor.WriteStoredString(writer, rooms[i].textureContainer.floor);
                roomCompressor.WriteStoredString(writer, rooms[i].textureContainer.wall);
                roomCompressor.WriteStoredString(writer, rooms[i].textureContainer.ceiling);
                writer.Write(rooms[i].items.Count);
                for (int j = 0; j < rooms[i].items.Count; j++)
                {
                    objectsCompressor.WriteStoredString(writer, rooms[i].items[j].item);
                    writer.Write(rooms[i].items[j].position);
                }
                writer.Write(rooms[i].basicObjects.Count);
                for (int j = 0; j < rooms[i].basicObjects.Count; j++)
                {
                    objectsCompressor.WriteStoredString(writer, rooms[i].basicObjects[j].prefab);
                    writer.Write(rooms[i].basicObjects[j].position);
                    writer.Write(rooms[i].basicObjects[j].rotation);
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
        }
    }
}
