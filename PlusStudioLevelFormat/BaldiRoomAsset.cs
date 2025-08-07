using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlusStudioLevelFormat
{
    public class RoomCellInfo
    {
        public ByteVector2 position;
        public Nybble walls = new Nybble(0);
        public PlusCellCoverage coverage = PlusCellCoverage.None;

        public RoomCellInfo()
        {

        }

        public RoomCellInfo(Cell cell)
        {
            position = cell.position;
            walls = cell.walls;
        }
    }

    // copying and pasting instead of inheriting because i dont want or need any of the other functionality of RoomInfo
    public class BaldiRoomAsset
    {
        public string name;
        public string type;
        public string windowType;
        public TextureContainer textureContainer;
        public List<ItemInfo> items = new List<ItemInfo>();
        public List<BasicObjectInfo> basicObjects = new List<BasicObjectInfo>();
        public ActivityInfo activity;
        public List<RoomCellInfo> cells = new List<RoomCellInfo>();
        public List<ItemSpawnInfo> itemSpawns = new List<ItemSpawnInfo>();
        public List<ByteVector2> potentialDoorPositions = new List<ByteVector2>();
        public List<ByteVector2> forcedDoorPositions = new List<ByteVector2>();
        public List<ByteVector2> entitySafeCells = new List<ByteVector2>();
        public List<ByteVector2> eventSafeCells = new List<ByteVector2>();
        public List<ByteVector2> standardLightCells = new List<ByteVector2>();
        public List<PosterInfo> posters = new List<PosterInfo>();
        public int maxItemValue = 100;
        public float windowChance = 0f;
        public float posterChance = 0f;


        const byte version = 0;
        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(name);
            writer.Write(type);
            writer.Write(textureContainer.floor);
            writer.Write(textureContainer.wall);
            writer.Write(textureContainer.ceiling);
            writer.Write(windowType);
            writer.Write(maxItemValue);
            writer.Write(windowChance);
            writer.Write(posterChance);

            if (activity == null)
            {
                writer.Write("null");
            }
            else
            {
                writer.Write(activity.type);
                writer.Write(activity.position);
                writer.Write((byte)activity.direction);
            }

            writer.Write(cells.Count);
            for (int i = 0; i < cells.Count; i++)
            {
                writer.Write(cells[i].position);
                writer.Write(cells[i].walls);
                writer.Write((byte)cells[i].coverage);
            }

            writer.Write(basicObjects.Count);
            for (int i = 0; i < basicObjects.Count; i++)
            {
                writer.Write(basicObjects[i].prefab);
                writer.Write(basicObjects[i].position);
                writer.Write(basicObjects[i].rotation);
            }

            writer.Write(posters.Count);
            for (int i = 0; i < posters.Count; i++)
            {
                writer.Write(posters[i].poster);
                writer.Write(posters[i].position);
                writer.Write((byte)posters[i].direction);
            }

            writer.Write(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                writer.Write(items[i].item);
                writer.Write(items[i].position);
            }

            writer.Write(itemSpawns.Count);
            for (int i = 0; i < itemSpawns.Count; i++)
            {
                writer.Write(itemSpawns[i].position);
                writer.Write(itemSpawns[i].weight);
            }

            writer.Write(potentialDoorPositions.Count);
            for (int i = 0; i < potentialDoorPositions.Count; i++)
            {
                writer.Write(potentialDoorPositions[i]);
            }
            writer.Write(forcedDoorPositions.Count);
            for (int i = 0; i < forcedDoorPositions.Count; i++)
            {
                writer.Write(forcedDoorPositions[i]);
            }
            writer.Write(standardLightCells.Count);
            for (int i = 0; i < standardLightCells.Count; i++)
            {
                writer.Write(standardLightCells[i]);
            }
            writer.Write(entitySafeCells.Count);
            for (int i = 0; i < entitySafeCells.Count; i++)
            {
                writer.Write(entitySafeCells[i]);
            }
            writer.Write(eventSafeCells.Count);
            for (int i = 0; i < eventSafeCells.Count; i++)
            {
                writer.Write(eventSafeCells[i]);
            }
        }

        public static BaldiRoomAsset Read(BinaryReader reader)
        {
            BaldiRoomAsset info = new BaldiRoomAsset();
            byte version = reader.ReadByte();
            info.name = reader.ReadString();
            info.type = reader.ReadString();
            info.textureContainer = new TextureContainer(reader.ReadString(), reader.ReadString(), reader.ReadString());
            info.windowType = reader.ReadString();
            info.maxItemValue = reader.ReadInt32();
            info.windowChance = reader.ReadSingle();
            info.posterChance = reader.ReadSingle();

            string activityId = reader.ReadString();
            if (activityId != "null")
            {
                info.activity = new ActivityInfo()
                {
                    type = activityId,
                    position = reader.ReadUnityVector3(),
                    direction = (PlusDirection)reader.ReadByte(),
                };
            }
            int cellCount = reader.ReadInt32();
            for (int i = 0; i < cellCount; i++)
            {
                RoomCellInfo cell = new RoomCellInfo();
                cell.position = reader.ReadByteVector2();
                cell.walls = (Nybble)reader.ReadByte();
                cell.coverage = (PlusCellCoverage)reader.ReadByte();
                info.cells.Add(cell);
            }
            int basicObjectCount = reader.ReadInt32();
            for (int i = 0; i < basicObjectCount; i++)
            {
                info.basicObjects.Add(new BasicObjectInfo()
                {
                    prefab = reader.ReadString(),
                    position = reader.ReadUnityVector3(),
                    rotation = reader.ReadUnityQuaternion()
                });
            }

            int posterCount = reader.ReadInt32();
            for (int i = 0; i < posterCount; i++)
            {
                info.posters.Add(new PosterInfo()
                {
                    poster = reader.ReadString(),
                    position = reader.ReadByteVector2(),
                    direction = (PlusDirection)reader.ReadByte()
                });
            }

            int itemCount = reader.ReadInt32();
            for (int i = 0; i < itemCount; i++)
            {
                info.items.Add(new ItemInfo()
                {
                    item = reader.ReadString(),
                    position = reader.ReadUnityVector2()
                });
            }
            int itemSpawnCount = reader.ReadInt32();
            for (int i = 0; i < itemSpawnCount; i++)
            {
                info.itemSpawns.Add(new ItemSpawnInfo()
                {
                    position=reader.ReadUnityVector2(),
                    weight=reader.ReadInt32()
                });
            }

            int potentialDoorPositionCount = reader.ReadInt32();
            for (int i = 0; i < potentialDoorPositionCount; i++)
            {
                info.potentialDoorPositions.Add(reader.ReadByteVector2());
            }

            int forcedDoorPositionCount = reader.ReadInt32();
            for (int i = 0; i < forcedDoorPositionCount; i++)
            {
                info.forcedDoorPositions.Add(reader.ReadByteVector2());
            }

            int standardLightCellCount = reader.ReadInt32();
            for (int i = 0; i < standardLightCellCount; i++)
            {
                info.standardLightCells.Add(reader.ReadByteVector2());
            }

            int entitySafeCellCount = reader.ReadInt32();
            for (int i = 0; i < entitySafeCellCount; i++)
            {
                info.entitySafeCells.Add(reader.ReadByteVector2());
            }

            int eventSafeCellCount = reader.ReadInt32();
            for (int i = 0; i < eventSafeCellCount; i++)
            {
                info.eventSafeCells.Add(reader.ReadByteVector2());
            }
            return info;
        }
    }
}
