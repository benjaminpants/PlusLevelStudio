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
    }

    public class RoomInfo
    {
        public string type;
        public TextureContainer textureContainer;

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

    public class LightInfo
    {
        public string prefab;
        public ByteVector2 position;
        public UnityColor color = new UnityColor(1f, 1f, 1f);
        public byte strength = 10;
    }
}
