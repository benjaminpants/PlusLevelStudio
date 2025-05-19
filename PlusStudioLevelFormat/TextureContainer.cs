using System;

namespace PlusStudioLevelFormat
{
    public class TextureContainer
    {
        public string floor = "null";
        public string wall = "null";
        public string ceiling = "null";

        public TextureContainer()
        {
        }

        public TextureContainer(TextureContainer container)
        {
            floor = container.floor;
            wall = container.wall;
            ceiling = container.ceiling;
        }

        public TextureContainer(string floor, string wall, string ceiling)
        {
            this.floor = floor;
            this.wall = wall;
            this.ceiling = ceiling;
        }
    }
}
