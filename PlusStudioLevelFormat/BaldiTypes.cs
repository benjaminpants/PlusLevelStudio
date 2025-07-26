using System;
using System.Collections.Generic;
using System.Text;

namespace PlusStudioLevelFormat
{
    public class Cell
    {
        public ByteVector2 position;
        public ushort roomId = 0;
        public int type => (roomId == 0) ? 16 : walls;
        public Nybble walls = new Nybble(0);

        public Cell(ByteVector2 pos)
        {
            position = pos;
        }

        public Cell(Cell cell)
        {
            position = cell.position;
            roomId = cell.roomId;
            walls = cell.walls;
        }
    }

    public class ActivityInfo
    {
        public string type;
        public UnityVector3 position;
        public PlusDirection direction;
    }

    public class RoomInfo
    {
        public string type;
        public TextureContainer textureContainer;
        public List<ItemInfo> items = new List<ItemInfo>();
        public List<BasicObjectInfo> basicObjects = new List<BasicObjectInfo>();
        public ActivityInfo activity;

        public RoomInfo(string roomType, TextureContainer container)
        {
            type = roomType;
            textureContainer = new TextureContainer(container);
        }

        public RoomInfo()
        {
            textureContainer = new TextureContainer();
        }
    }

    public class StructureInfo
    {
        public string type;
        public List<StructureDataInfo> data = new List<StructureDataInfo>();

        public StructureInfo()
        {

        }

        public StructureInfo(string type)
        {
            this.type = type;
        }
    }

    public class StructureDataInfo
    {
        public string prefab = string.Empty;
        public ByteVector2 position = new ByteVector2();
        public PlusDirection direction = PlusDirection.Null;
        public int data = 0;
    }

    public class LightInfo
    {
        public string prefab;
        public ByteVector2 position;
        public UnityColor color = new UnityColor(1f, 1f, 1f);
        public byte strength = 10;
    }

    public class NPCInfo
    {
        public string npc;
        public ByteVector2 position;
    }

    public class PosterInfo
    {
        public string poster;
        public ByteVector2 position;
        public PlusDirection direction = PlusDirection.Null;
    }

    public class DoorInfo
    {
        public string prefab;
        public ByteVector2 position;
        public ushort roomId;
        public PlusDirection direction = PlusDirection.Null;
    }

    public class TileObjectInfo
    {
        public string prefab;
        public ByteVector2 position;
        public PlusDirection direction = PlusDirection.Null;
    }

    public class WindowInfo
    {
        public string prefab;
        public ByteVector2 position;
        public PlusDirection direction = PlusDirection.Null;
    }

    public class ExitInfo
    {
        public string type;
        public ByteVector2 position;
        public PlusDirection direction = PlusDirection.Null;
        public bool isSpawn = false;
    }

    public class ItemInfo
    {
        public string item;
        public UnityVector2 position;
    }

    public class BasicObjectInfo
    {
        public string prefab;
        public UnityVector3 position;
        public UnityQuaternion rotation;
        //public bool replaceable;
    }

    public enum PlusDirection
    {
        North,
        East,
        South,
        West,
        Null
    }
}
