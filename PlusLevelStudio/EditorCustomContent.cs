using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.UI;
using PlusLevelStudio.Editor;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio
{
    /// <summary>
    /// A class containg all custom content for the specified floor.
    /// </summary>
    public class EditorCustomContent : IStudioLegacyKnowledgable
    {
        private static List<Action<EditorCustomContent>> onCreation = new List<Action<EditorCustomContent>>();

        public EditorCustomContent()
        {
            AddHandlers();
        }

        protected void AddHandlers()
        {
            handlers.Add(new CustomRoomTextureContentHandler());
            handlers.Add(new CustomImagePosterContentHandler());
            handlers.Add(new CustomBaldiSaysPosterContentHandler());
            handlers.Add(new CustomChalkboardPosterContentHandler());
            handlers.Add(new BulletInPosterContentHandler("bulletinposter", BaldiFonts.ComicSans18));
            handlers.Add(new BulletInPosterContentHandler("bulletinsmallposter", BaldiFonts.ComicSans12));
            handlers.Add(new CustomNPCContentHandler());
            foreach (var item in onCreation)
            {
                item.Invoke(this);
            }
        }

        public static void AddCreationCallback(Action<EditorCustomContent> cb)
        {
            onCreation.Add(cb);
        }

        public EditorCustomContent(EditorCustomContentPackage package)
        {
            AddHandlers();
            LoadFromPackage(package);
            legacyFlags |= package.legacyFlags;
        }

        public EditorCustomContentHandler GetHandlerFor(string type)
        {
            return handlers.Find(x => x.handledTypes.Contains(type));
        }


        public BaseGameManager gameManagerPre;

        public List<EditorCustomContentHandler> handlers = new List<EditorCustomContentHandler>();

        public StudioLevelLegacyFlags legacyFlags { get; set; } = StudioLevelLegacyFlags.None;

        public void CleanupContent()
        {
            if (gameManagerPre != null)
            {
                UnityEngine.Object.Destroy(gameManagerPre.gameObject);
            }
            gameManagerPre = null;
            foreach (var item in handlers)
            {
                item.CleanupContent();
            }
        }

        public void ClearAndCleanupEntriesNotInPackage(EditorCustomContentPackage package)
        {
            foreach (var item in handlers)
            {
                item.ClearAndCleanupEntriesNotInPackage(package);
            }
        }

        public void ClearEntriesNotInEditor(EditorController editor, EditorCustomContentPackage package)
        {
            foreach (var item in handlers)
            {
                item.ClearEntriesNotInEditor(editor, package);
            }
        }

        protected Texture2D LoadTextureFromPathOrData(EditorCustomContentEntry entry, string basePath)
        {
            Texture2D returnVal;
            if (entry.usingFilePath)
            {
                returnVal = AssetLoader.TextureFromFile(Path.Combine(basePath, entry.filePath));
            }
            else
            {
                returnVal = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                returnVal.filterMode = FilterMode.Point;
                returnVal.LoadImage(entry.data);
                returnVal.name = entry.id;
            }
            return returnVal;
        }

        public void LoadFromPackage(EditorCustomContentPackage package)
        {
            foreach (var item in handlers)
            {
                item.LoadFromPackage(package);
            }
        }
    }

    public class EditorCustomContentPackage : IStudioLegacyKnowledgable
    {
        public bool allowingFilePaths { get; private set; }
        public StudioLevelLegacyFlags legacyFlags { get; set; } = StudioLevelLegacyFlags.None;

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

        public EditorCustomContentEntry thumbnailEntry
        {
            get
            {
                return entries.Find(x => x.contentType == "thumbnail");
            }
            set
            {
                EditorCustomContentEntry currentThumb = thumbnailEntry;
                if (currentThumb != null)
                {
                    entries.Remove(currentThumb);
                }
                if (value == null) return;
                entries.Add(value);
            }
        }

        public Texture2D GenerateThumbnailTexture()
        {
            EditorCustomContentEntry entry = thumbnailEntry;
            if (entry == null) return null;
            Texture2D returnVal;
            if (entry.usingFilePath)
            {
                returnVal = AssetLoader.TextureFromFile(Path.Combine(LevelStudioPlugin.customThumbnailsPath, entry.filePath));
            }
            else
            {
                returnVal = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                returnVal.filterMode = FilterMode.Point;
                try
                {
                    returnVal.LoadImage(entry.data);
                    returnVal.name = entry.id;
                }
                catch (Exception E)
                {
                    Debug.LogWarning(E);
                    GameObject.Destroy(returnVal);
                    return null;
                }
            }
            return returnVal;
        }

        public List<EditorCustomContentEntry> GetAllOfType(string type)
        {
            return entries.FindAll(x => x.contentType == type);
        }

        public List<EditorCustomContentEntry> GetAllOfTypes(params string[] types)
        {
            return entries.FindAll(x => types.Contains(x.contentType));
        }

        public const byte version = 2;

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
                entries[i].Write(writer, (entries[i].usingFilePath) && allowingFilePaths);
            }
        }

        public static EditorCustomContentPackage Read(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            byte entryVersion = reader.ReadByte();
            EditorCustomContentPackage package = new EditorCustomContentPackage(reader.ReadBoolean());
            if (version < 2)
            {
                package.legacyFlags |= StudioLevelLegacyFlags.BeforeNPCCustom;
            }
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
                case "thumbnail":
                    return File.ReadAllBytes(Path.Combine(LevelStudioPlugin.customThumbnailsPath, filePath));
                case "texture":
                    return File.ReadAllBytes(Path.Combine(LevelStudioPlugin.customTexturePath, filePath));
                case "imageposter":
                    return File.ReadAllBytes(Path.Combine(LevelStudioPlugin.customPostersPath, filePath));
                case "chalkboardposter":
                case "baldisaysposter":
                    throw new Exception("Attempted to get text poster from filePath even though that shouldn't happen?!");
                case "npc":
                    throw new Exception("Attempted to get NPC data from filepath?!");
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
