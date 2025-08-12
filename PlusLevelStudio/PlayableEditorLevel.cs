using Newtonsoft.Json.Linq;
using PlusLevelStudio.Editor;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio
{
    public class PlayableEditorLevel
    {
        public PlayableLevelMeta meta;
        public Texture2D texture; // TODO: consider moving to PlayableLevelMeta to allow for custom icons.
        public string filePath;
        public BaldiLevel data;

        public const byte version = 1;

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            if (texture == null)
            {
                writer.Write(0);
            }
            else
            {
                byte[] pngData = ImageConversion.EncodeToPNG(texture);
                writer.Write(pngData.Length);
                writer.Write(pngData);
            }
            meta.Write(writer);
            data.Write(writer);
        }

        public static PlayableEditorLevel Read(BinaryReader reader)
        {
            PlayableEditorLevel playable = new PlayableEditorLevel();
            byte version = reader.ReadByte();
            if (version >= 1)
            {
                int fileSize = reader.ReadInt32();
                if (fileSize > 0)
                {
                    byte[] textureData = reader.ReadBytes(fileSize);
                    playable.texture = new Texture2D(2, 2, TextureFormat.RGBA4444, false);
                    try
                    {
                        ImageConversion.LoadImage(playable.texture, textureData);
                        playable.texture.filterMode = FilterMode.Point;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Invalid thumbnail data " + e.ToString());
                        UnityEngine.Object.Destroy(playable.texture);
                        playable.texture = null;
                    }
                }
            }
            playable.meta = PlayableLevelMeta.Read(reader, false);
            playable.data = BaldiLevel.Read(reader);
            return playable;
        }
    }

    public class PlayableLevelMeta
    {
        public string name = "My Awesome Level!";
        public string author = "Unknown";
        public string gameMode = "standard";
        public EditorGameModeSettings modeSettings;
        public EditorCustomContentPackage contentPackage;

        public PlayableLevelMeta CompileContent()
        {
            return new PlayableLevelMeta()
            {
                name = name,
                author = author,
                gameMode = gameMode,
                modeSettings = modeSettings.MakeCopy(),
                contentPackage = contentPackage.AsFilePathless()
            };
        }

        public const byte version = 2;

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(name);
            writer.Write(author);
            writer.Write(gameMode);
            writer.Write(modeSettings != null);
            if (modeSettings != null)
            {
                modeSettings.Write(writer);
            }
            contentPackage.Write(writer);
        }

        public static PlayableLevelMeta Read(BinaryReader reader, bool oldVersionsUsePaths)
        {
            PlayableLevelMeta meta = new PlayableLevelMeta();
            byte version = reader.ReadByte();
            meta.name = reader.ReadString();
            if (version >= 1)
            {
                meta.author = reader.ReadString();
            }
            meta.gameMode = reader.ReadString();
            if (!reader.ReadBoolean())
            {
                meta.modeSettings = LevelStudioPlugin.Instance.gameModeAliases[meta.gameMode].CreateSettings();
            }
            else
            {
                meta.modeSettings = LevelStudioPlugin.Instance.gameModeAliases[meta.gameMode].CreateSettings();
                meta.modeSettings.ReadInto(reader);
            }
            if (version < 2)
            {
                meta.contentPackage = new EditorCustomContentPackage(oldVersionsUsePaths);
            }
            else
            {
                meta.contentPackage = EditorCustomContentPackage.Read(reader);
            }
            return meta;
        }
    }
}
