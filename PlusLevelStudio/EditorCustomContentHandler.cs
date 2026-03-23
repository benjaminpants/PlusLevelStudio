using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.UI;
using PlusLevelStudio.Editor;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio
{
    public abstract class EditorCustomContentHandler
    {
        public bool inEditor = false;

        public string[] handledTypes = new string[0];

        public abstract bool AddElementOfType(EditorCustomContentEntry entry);

        public virtual List<EditorCustomContentEntry> GetHandledEntries(EditorCustomContentPackage package)
        {
            return package.GetAllOfTypes(handledTypes);
        }

        public abstract void ClearEntriesNotInEditor(EditorController edCont, EditorCustomContentPackage package);

        public abstract void ClearAndCleanupEntriesNotInPackage(EditorCustomContentPackage package);

        public abstract void LoadFromPackage(EditorCustomContentPackage package);

        public abstract void CleanupContent();
    }

    public class CustomRoomTextureContentHandler : EditorCustomContentHandler
    {
        public ExtensibleDictionaryExtension<Texture2D> extend = new ExtensibleDictionaryExtension<Texture2D>();

        public CustomRoomTextureContentHandler()
        {
            handledTypes = new string[] { "texture" };
        }

        public override bool AddElementOfType(EditorCustomContentEntry entry)
        {
            if (extend.dictionary.ContainsKey(entry.id)) return false; // the texture is already loaded
            LevelLoaderPlugin.Instance.roomTextureAliases.AddExtensionIfNotPresent(extend);
            extend.dictionary.Add(entry.id, LoadTextureFromPathOrData(entry, LevelStudioPlugin.customTexturePath));
            return true;
        }

        public override void CleanupContent()
        {
            foreach (var item in extend.dictionary)
            {
                UnityEngine.Object.Destroy(item.Value);
            }
            extend.dictionary.Clear();
            LevelLoaderPlugin.Instance.roomTextureAliases.extends.Remove(extend);
        }

        public override void ClearAndCleanupEntriesNotInPackage(EditorCustomContentPackage package)
        {
            List<EditorCustomContentEntry> textureEntries = package.GetAllOfType("texture");
            List<string> texturesKeyedForRemoval = new List<string>();
            foreach (KeyValuePair<string, Texture2D> kvp in extend.dictionary)
            {
                if (textureEntries.Find(x => x.id == kvp.Key) == null)
                {
                    texturesKeyedForRemoval.Add(kvp.Key);
                }
            }
            for (int i = 0; i < texturesKeyedForRemoval.Count; i++)
            {
                UnityEngine.Object.Destroy(extend.dictionary[texturesKeyedForRemoval[i]]);
                extend.dictionary.Remove(texturesKeyedForRemoval[i]);
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

        public override void LoadFromPackage(EditorCustomContentPackage package)
        {
            LevelLoaderPlugin.Instance.roomTextureAliases.AddExtensionIfNotPresent(extend);
            List<EditorCustomContentEntry> textureEntries = package.GetAllOfType("texture");
            foreach (EditorCustomContentEntry entry in textureEntries)
            {
                if (extend.dictionary.ContainsKey(entry.id)) continue; // the texture is already loaded
                extend.dictionary.Add(entry.id, LoadTextureFromPathOrData(entry, LevelStudioPlugin.customTexturePath));
            }
        }

        public override void ClearEntriesNotInEditor(EditorController edCont, EditorCustomContentPackage package)
        {
            List<EditorCustomContentEntry> entriesQueuedForDeletion = new List<EditorCustomContentEntry>();
            List<EditorCustomContentEntry> textureEntries = package.GetAllOfType("texture");
            foreach (EditorCustomContentEntry entry in textureEntries)
            {
                if (edCont.levelData.rooms.Count(x => x.textureContainer.UsesTexture(entry.id)) == 0)
                {
                    entriesQueuedForDeletion.Add(entry);
                }
            }
            entriesQueuedForDeletion.Do(x => package.entries.Remove(x));
        }
    }

    public class CustomNPCContentHandler : EditorCustomContentHandler
    {
        public ExtensibleDictionaryExtension<NPC> extend = new ExtensibleDictionaryExtension<NPC>();
        public List<GameObject> gameObjects = new List<GameObject>();

        public CustomNPCContentHandler()
        {
            handledTypes = new string[] { "npc" };
        }

        public override bool AddElementOfType(EditorCustomContentEntry entry)
        {
            throw new NotImplementedException();
        }

        public override void CleanupContent()
        {
            foreach (var item in gameObjects)
            {
                GameObject.Destroy(item);
            }
            gameObjects.Clear();
            extend.dictionary.Clear();
            LevelLoaderPlugin.Instance.npcAliases.extends.Remove(extend);
        }

        public override void ClearEntriesNotInEditor(EditorController edCont, EditorCustomContentPackage package)
        {
            // nothing necessary
        }

        public override void ClearAndCleanupEntriesNotInPackage(EditorCustomContentPackage package)
        {
            // not necessary
        }

        public override void LoadFromPackage(EditorCustomContentPackage package)
        {
            // in the editor this will do nothing, as editor doesnt have custom npc content but we do it anyway.
            LevelLoaderPlugin.Instance.npcAliases.AddExtensionIfNotPresent(extend);
            List<EditorCustomContentEntry> npcEntries = GetHandledEntries(package);
            foreach (EditorCustomContentEntry entry in npcEntries)
            {
                MemoryStream stream = new MemoryStream(entry.GetData());
                BinaryReader reader = new BinaryReader(stream);
                byte version = reader.ReadByte(); // just incase
                string characterBase = reader.ReadString();
                NPCProperties properties = LevelStudioPlugin.Instance.ConstructNPCPropertiesOfType(characterBase);
                properties.ReadInto(reader);
                GameObject[] createdObjects = properties.GeneratePrefabs(LevelLoaderPlugin.Instance.npcAliases[characterBase]);
                gameObjects.AddRange(createdObjects);
                extend.dictionary.Add(entry.id, createdObjects[0].GetComponent<NPC>());
                reader.Close();
            }
        }
    }

    public class CustomImagePosterContentHandler : EditorCustomContentHandler
    {
        public ExtensibleDictionaryExtension<PosterObject> extend = new ExtensibleDictionaryExtension<PosterObject>();

        public CustomImagePosterContentHandler()
        {
            handledTypes = new string[] { "imageposter" };
        }

        public override void CleanupContent()
        {
            foreach (KeyValuePair<string, PosterObject> kvp in extend.dictionary)
            {
                UnityEngine.Object.Destroy(kvp.Value);
            }
            extend.dictionary.Clear();
            LevelLoaderPlugin.Instance.posterAliases.extends.Remove(extend);
        }

        public override void ClearAndCleanupEntriesNotInPackage(EditorCustomContentPackage package)
        {
            List<EditorCustomContentEntry> posterEntries = GetHandledEntries(package);
            List<string> posterEntriesKeyedForRemoval = new List<string>();
            foreach (KeyValuePair<string, PosterObject> kvp in extend.dictionary)
            {
                if (posterEntries.Find(x => x.id == kvp.Key) == null)
                {
                    posterEntriesKeyedForRemoval.Add(kvp.Key);
                }
            }
            for (int i = 0; i < posterEntriesKeyedForRemoval.Count; i++)
            {
                UnityEngine.Object.Destroy(extend.dictionary[posterEntriesKeyedForRemoval[i]]);
                extend.dictionary.Remove(posterEntriesKeyedForRemoval[i]);
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

        public override void LoadFromPackage(EditorCustomContentPackage package)
        {
            LevelLoaderPlugin.Instance.posterAliases.AddExtensionIfNotPresent(extend);
            List<EditorCustomContentEntry> imagePosterEntries = GetHandledEntries(package);
            foreach (EditorCustomContentEntry entry in imagePosterEntries)
            {
                if (extend.dictionary.ContainsKey(entry.id)) continue; // the poster is already made
                PosterObject posterObj = ObjectCreators.CreatePosterObject(LoadTextureFromPathOrData(entry, LevelStudioPlugin.customPostersPath), new PosterTextData[0]);
                posterObj.name = entry.id;
                extend.dictionary.Add(entry.id, posterObj);
            }
        }

        public override bool AddElementOfType(EditorCustomContentEntry entry)
        {
            if (extend.dictionary.ContainsKey(entry.id)) return false; // the poster is already made
            LevelLoaderPlugin.Instance.posterAliases.AddExtensionIfNotPresent(extend);
            PosterObject posterObj = ObjectCreators.CreatePosterObject(LoadTextureFromPathOrData(entry, LevelStudioPlugin.customPostersPath), new PosterTextData[0]);
            posterObj.name = entry.id;
            extend.dictionary.Add(entry.id, posterObj);
            return true;
        }

        public override void ClearEntriesNotInEditor(EditorController edCont, EditorCustomContentPackage package)
        {
            List<EditorCustomContentEntry> entriesQueuedForDeletion = new List<EditorCustomContentEntry>();
            List<EditorCustomContentEntry> posterEntries = GetHandledEntries(package);
            foreach (EditorCustomContentEntry entry in posterEntries)
            {
                if (edCont.levelData.posters.Count(x => x.type == entry.id) == 0)
                {
                    entriesQueuedForDeletion.Add(entry);
                }
            }
            entriesQueuedForDeletion.Do(x => package.entries.Remove(x));
        }
    }

    public abstract class CustomTextPosterContentHandler : EditorCustomContentHandler
    {
        public ExtensibleDictionaryExtension<PosterObject> extend = new ExtensibleDictionaryExtension<PosterObject>();

        public override void CleanupContent()
        {
            foreach (KeyValuePair<string, PosterObject> kvp in extend.dictionary)
            {
                UnityEngine.Object.Destroy(kvp.Value);
            }
            extend.dictionary.Clear();
            LevelLoaderPlugin.Instance.posterAliases.extends.Remove(extend);
        }

        public override void ClearAndCleanupEntriesNotInPackage(EditorCustomContentPackage package)
        {
            List<EditorCustomContentEntry> posterEntries = package.GetAllOfTypes(handledTypes);
            List<string> posterEntriesKeyedForRemoval = new List<string>();
            foreach (KeyValuePair<string, PosterObject> kvp in extend.dictionary)
            {
                if (posterEntries.Find(x => x.id == kvp.Key) == null)
                {
                    posterEntriesKeyedForRemoval.Add(kvp.Key);
                }
            }
            for (int i = 0; i < posterEntriesKeyedForRemoval.Count; i++)
            {
                UnityEngine.Object.Destroy(extend.dictionary[posterEntriesKeyedForRemoval[i]]);
                extend.dictionary.Remove(posterEntriesKeyedForRemoval[i]);
            }
        }

        public abstract PosterObject GeneratePosterObject(string id, string text);

        public override void LoadFromPackage(EditorCustomContentPackage package)
        {
            LevelLoaderPlugin.Instance.posterAliases.AddExtensionIfNotPresent(extend);
            List<EditorCustomContentEntry> posterEntries = package.GetAllOfTypes(handledTypes);
            foreach (EditorCustomContentEntry entry in posterEntries)
            {
                if (extend.dictionary.ContainsKey(entry.id)) continue; // the texture is already loaded
                PosterObject posterObj = GeneratePosterObject(entry.id, Encoding.Unicode.GetString(entry.GetData()));
                extend.dictionary.Add(entry.id, posterObj);
            }
        }

        public override bool AddElementOfType(EditorCustomContentEntry entry)
        {
            if (extend.dictionary.ContainsKey(entry.id)) return false;
            LevelLoaderPlugin.Instance.posterAliases.AddExtensionIfNotPresent(extend);
            PosterObject posterObj = GeneratePosterObject(entry.id, Encoding.Unicode.GetString(entry.GetData()));
            extend.dictionary.Add(entry.id, posterObj);
            return true;
        }

        public override void ClearEntriesNotInEditor(EditorController edCont, EditorCustomContentPackage package)
        {
            List<EditorCustomContentEntry> entriesQueuedForDeletion = new List<EditorCustomContentEntry>();
            List<EditorCustomContentEntry> posterEntries = GetHandledEntries(package);
            foreach (EditorCustomContentEntry entry in posterEntries)
            {
                if (edCont.levelData.posters.Count(x => x.type == entry.id) == 0)
                {
                    entriesQueuedForDeletion.Add(entry);
                }
            }
            entriesQueuedForDeletion.Do(x => package.entries.Remove(x));
        }
    }

    public class CustomBaldiSaysPosterContentHandler : CustomTextPosterContentHandler
    {
        public CustomBaldiSaysPosterContentHandler()
        {
            handledTypes = new string[] { "baldisaysposter" };
        }

        public override PosterObject GeneratePosterObject(string id, string text)
        {
            return LevelStudioPlugin.Instance.GenerateBaldiSaysPoster(id, text);
        }
    }

    public class CustomChalkboardPosterContentHandler : CustomTextPosterContentHandler
    {
        public CustomChalkboardPosterContentHandler()
        {
            handledTypes = new string[] { "chalkboardposter" };
        }

        public override PosterObject GeneratePosterObject(string id, string text)
        {
            return LevelStudioPlugin.Instance.GenerateChalkPoster(id, text);
        }
    }

    public class BulletInPosterContentHandler : CustomTextPosterContentHandler
    {
        protected BaldiFonts font;
        public BulletInPosterContentHandler(string[] handledTypes, BaldiFonts fnt)
        {
            this.handledTypes = handledTypes;
            font = fnt;
        }

        public BulletInPosterContentHandler(string handledType, BaldiFonts fnt) : this(new string[] { handledType }, fnt)
        {
        }

        public override PosterObject GeneratePosterObject(string id, string text)
        {
            return LevelStudioPlugin.Instance.GenerateBulletInPoster(id, text, font);
        }
    }
}
