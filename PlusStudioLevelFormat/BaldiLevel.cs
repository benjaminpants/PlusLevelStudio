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
        public List<DoorInfo> doors = new List<DoorInfo>();
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

        public void Write(BinaryWriter writer)
        {
            StringCompressor roomCompressor = new StringCompressor();
            roomCompressor.AddStrings(rooms.Select(x => x.type));
            roomCompressor.AddStrings(rooms.Select(x => x.textureContainer.floor));
            roomCompressor.AddStrings(rooms.Select(x => x.textureContainer.wall));
            roomCompressor.AddStrings(rooms.Select(x => x.textureContainer.ceiling));
            StringCompressor objectsCompressor = new StringCompressor();
            objectsCompressor.AddStrings(lights.Select(x => x.prefab));
            objectsCompressor.AddStrings(doors.Select(x => x.prefab));
            objectsCompressor.FinalizeDatabase();
            roomCompressor.FinalizeDatabase();
            writer.Write(version);
            roomCompressor.WriteStringDatabase(writer);
            objectsCompressor.WriteStringDatabase(writer);
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
            }
        }
    }
}
