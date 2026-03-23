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
        public string name = "";
        public string roomType;
        public TextureContainer textureContainer;
        public ActivityLocation activity;
        public EditorRoomVisualManager myVisual;

        public Texture2D floorTex => LevelLoaderPlugin.Instance.roomTextureAliases[textureContainer.floor];
        public Texture2D wallTex => LevelLoaderPlugin.Instance.roomTextureAliases[textureContainer.wall];
        public Texture2D ceilTex => LevelLoaderPlugin.Instance.roomTextureAliases[textureContainer.ceiling];

        public EditorRoom(string roomType, TextureContainer container)
        {
            this.roomType = roomType;
            textureContainer = new TextureContainer(container);
        }

        public EditorRoom(string roomType, string name, TextureContainer container)
        {
            this.roomType = roomType;
            this.name = name;
            textureContainer = new TextureContainer(container);
        }

        public EditorRoom()
        {
            textureContainer = new TextureContainer();
        }
    }
}
