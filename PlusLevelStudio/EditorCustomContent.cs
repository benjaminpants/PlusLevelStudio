using MTM101BaldAPI.AssetTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio
{
    /// <summary>
    /// A class containg all custom content for the specified floor.
    /// </summary>
    public class EditorCustomContent
    {
        public BaseGameManager gameManagerPre;

        public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        public void CleanupContent()
        {
            if (gameManagerPre != null)
            {
                UnityEngine.Object.Destroy(gameManagerPre.gameObject);
            }
            gameManagerPre = null;
            foreach (KeyValuePair<string, Texture2D> kvp in textures)
            {
                UnityEngine.Object.Destroy(kvp.Value);
            }
            textures.Clear();
        }

        public void ClearEntriesNotInPackage(EditorCustomContentPackage package)
        {
            List<EditorCustomContentEntry> textureEntries = package.GetAllOfType("texture");
            List<string> texturesKeyedForRemoval = new List<string>();
            foreach (KeyValuePair<string, Texture2D> kvp in textures)
            {
                if (textureEntries.Find(x => x.id == kvp.Key) == null)
                {
                    texturesKeyedForRemoval.Add(kvp.Key);
                }
            }
            for (int i = 0; i < texturesKeyedForRemoval.Count; i++)
            {
                UnityEngine.Object.Destroy(textures[texturesKeyedForRemoval[i]]);
                textures.Remove(texturesKeyedForRemoval[i]);
            }
        }

        public void LoadFromPackage(EditorCustomContentPackage package)
        {
            List<EditorCustomContentEntry> textureEntries = package.GetAllOfType("texture");
            foreach (EditorCustomContentEntry entry in textureEntries)
            {
                if (textures.ContainsKey(entry.id)) continue; // the texture is already loaded
                if (package.allowingFilePaths)
                {
                    textures.Add(entry.id, AssetLoader.TextureFromFile(Path.Combine(LevelStudioPlugin.customTexturePath, entry.filePath)));
                }
                else
                {
                    Texture2D texture = new Texture2D(128,128,TextureFormat.ARGB32, false);
                    texture.filterMode = FilterMode.Point;
                    texture.LoadImage(entry.data);
                    texture.name = entry.id;
                    textures.Add(entry.id, texture);
                }
            }

        }
    }

    public class EditorCustomContentPackage
    {
        public bool allowingFilePaths { get; private set; }

        public EditorCustomContentPackage(bool filePaths)
        {
            allowingFilePaths = filePaths;
        }

        /// <summary>
        /// Returns a copy of this EditorCustomContentPackage and all it's contents, but without using file name references.
        /// Will throw an exception if usingFilePaths is false.
        /// </summary>
        /// <returns></returns>
        public EditorCustomContentPackage AsFilePathless()
        {
            if (!allowingFilePaths) throw new InvalidOperationException("Can't convert to pathless if already pathless!");
            EditorCustomContentPackage newPackage = new EditorCustomContentPackage(false);
            for (int i = 0; i < entries.Count; i++)
            {
                newPackage.entries.Add(new EditorCustomContentEntry()
                {
                    contentType = entries[i].contentType,
                    id = entries[i].id,
                    data = entries[i].GetData()
                });
            }
            return newPackage;
        }

        public List<EditorCustomContentEntry> entries = new List<EditorCustomContentEntry>();

        public List<EditorCustomContentEntry> GetAllOfType(string type)
        {
            return entries.FindAll(x => x.contentType == type);
        }

        public const byte version = 1;

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(EditorCustomContentEntry.version);
            writer.Write(allowingFilePaths);
            writer.Write(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                if (allowingFilePaths)
                {
                    writer.Write(entries[i].usingFilePath);
                }
                entries[i].Write(writer, allowingFilePaths);
            }
        }

        public static EditorCustomContentPackage Read(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            byte entryVersion = reader.ReadByte();
            EditorCustomContentPackage package = new EditorCustomContentPackage(reader.ReadBoolean());
            int streamCount = reader.ReadInt32();
            for (int i = 0; i < streamCount; i++)
            {
                if ((version == 0) || (!package.allowingFilePaths))
                {
                    package.entries.Add(EditorCustomContentEntry.Read(reader, entryVersion, package.allowingFilePaths));
                }
                else
                {
                    package.entries.Add(EditorCustomContentEntry.Read(reader, entryVersion, reader.ReadBoolean()));
                }
            }
            return package;
        }
    }

    public class EditorCustomContentEntry
    {
        public EditorCustomContentEntry()
        {

        }

        public EditorCustomContentEntry(string contentType, string id, string filePath)
        {
            this.contentType = contentType;
            this.filePath = filePath;
            this.id = id;
        }

        public EditorCustomContentEntry(string contentType, string id, byte[] data)
        {
            this.contentType = contentType;
            this.id = id;
            this.data = data;
        }

        public string contentType = "undefined";
        public string filePath = string.Empty;
        public string id = string.Empty;
        public byte[] data = null;

        public bool usingFilePath => data == null;

        public const byte version = 0;

        /// <summary>
        /// Returns the data for the specified CustomContentEntry. If it has none, it will attempt to get it from filePath.
        /// </summary>
        /// <returns></returns>
        public byte[] GetData()
        {
            if (data != null)
            {
                return data;
            }
            byte[] fromPath = GetDataFromFilePath();
            if (fromPath == null) return new byte[0];
            return fromPath;
        }

        protected byte[] GetDataFromFilePath()
        {
            switch (contentType)
            {
                case "texture":
                    return File.ReadAllBytes(Path.Combine(LevelStudioPlugin.customTexturePath, filePath));
            }
            return null;
        }

        public void Write(BinaryWriter writer, bool writePath)
        {
            writer.Write(contentType);
            writer.Write(id);
            if (writePath)
            {
                writer.Write(filePath);
            }
            else
            {
                writer.Write(data.Length);
                writer.Write(data);
            }
        }

        public static EditorCustomContentEntry Read(BinaryReader reader, byte version, bool readPath)
        {
            EditorCustomContentEntry entry = new EditorCustomContentEntry();
            entry.contentType = reader.ReadString();
            entry.id = reader.ReadString();
            if (readPath)
            {
                entry.filePath = reader.ReadString();
            }
            else
            {
                entry.data = reader.ReadBytes(reader.ReadInt32());
            }
            return entry;
        }
    }
}
