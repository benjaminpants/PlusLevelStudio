using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlusStudioLevelLoader;

namespace PlusLevelStudio.Editor
{
    public class EditorRoom
    {
        public string roomType;
        public TextureContainer textureContainer;
        public ActivityLocation activity;

        public Texture2D floorTex => LevelLoaderPlugin.RoomTextureFromAlias(textureContainer.floor);
        public Texture2D wallTex => LevelLoaderPlugin.RoomTextureFromAlias(textureContainer.wall);
        public Texture2D ceilTex => LevelLoaderPlugin.RoomTextureFromAlias(textureContainer.ceiling);

        public EditorRoom(string roomType, TextureContainer container)
        {
            this.roomType = roomType;
            textureContainer = new TextureContainer(container);
        }

        public EditorRoom()
        {
            textureContainer = new TextureContainer();
        }
    }
}
