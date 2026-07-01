using Newtonsoft.Json.Linq;
using PlusLevelStudio.Editor;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio
{
    public class PlayableEditorLevel : IStudioLegacyKnowledgable, IStudioPlayable
    {
        public StudioLevelLegacyFlags legacyFlags { get; set; } = StudioLevelLegacyFlags.None;

        public ulong uniqueId = 0; // zero is reserved for "never had a uuid"
        public PlayableLevelMeta meta;
        public string filePath;
        public BaldiLevel data;

        public const byte version = 4;

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(uniqueId);
            meta.Write(writer);
            data.Write(writer);
        }

        public static PlayableEditorLevel Read(BinaryReader reader)
        {
            PlayableEditorLevel playable = new PlayableEditorLevel();
            byte version = reader.ReadByte();
            if (version >= 4)
            {
                playable.uniqueId = reader.ReadUInt64();
            }
            byte[] oldThumb = null;
            if ((version >= 1) && (version < 3)) // version 0 had no thumbnails, versions 1 and 2 had the thumbnail in PlayableEditorLevel, format 3 and above have it in the custom content.
            {
                int fileSize = reader.ReadInt32();
                if (fileSize > 0)
                {
                    oldThumb = reader.ReadBytes(fileSize);
                }
            }
            playable.meta = PlayableLevelMeta.Read(reader, false);
            if (oldThumb != null)
            {
                playable.meta.contentPackage.thumbnailEntry = new EditorCustomContentEntry()
                {
                    data = oldThumb,
                    contentType = "thumbnail",
                    id = "thumbnail",
                };
            }
            playable.data = BaldiLevel.Read(reader);
            if (version <= 1)
            {
                for (int i = 0; i < defaultStickers.Length; i++)
                {
                    playable.data.potentialStickers.Add(new WeightedID() { id = defaultStickers[i], weight = 100 });
                }
            }
            return playable;
        }

        public string GetName()
        {
            return meta.name;
        }

        public string GetAuthor()
        {
            return meta.author;
        }

        public string GetLocalizedGamemode()
        {
            return LocalizationManager.Instance.GetLocalizedText(LevelStudioPlugin.Instance.gameModeAliases[meta.gameMode].nameKey);
        }

        public EditorCustomContentEntry GetThumbnail()
        {
            return meta.contentPackage.thumbnailEntry;
        }

        public void Play()
        {
            EditorPlayModeManager.LoadLevel(this, (LevelStudioPlugin.Instance.gameModeAliases[meta.gameMode].supportsCampaigns && meta.playSettings.allowsRetries) ? 2 : 0, false);
        }

        // default stickers for older levels
        internal static string[] defaultStickers = new string[]
        {
            "lingering_hiding",
            "baldi_praise",
            "stamina",
            "elevator",
            "time_extension",
            "stealth",
            "inventory_slot",
            "silence",
            "reach",
            "map_range",
            "door_stop",
            "ytp_multiplier",
            "baldi_countdown"
        };
    }

    public class PlayableLevelMeta
    {
        public string name = "My Awesome Level!";
        public string author = "Unknown";
        public string gameMode = "standard";
        public EditorGameModeSettings modeSettings;
        public EditorCustomContentPackage contentPackage;
        public PlaymodeSettingsMeta playSettings = new PlaymodeSettingsMeta();

        public PlayableLevelMeta CompileContent()
        {
            return new PlayableLevelMeta()
            {
                name = name,
                author = author,
                gameMode = gameMode,
                modeSettings = (modeSettings == null ? null : modeSettings.MakeCopy()),
                contentPackage = contentPackage.AsFilePathless(),
                playSettings = new PlaymodeSettingsMeta(playSettings)
            };
        }

        public const byte version = 4;

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
            playSettings.Write(writer);
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
            meta.modeSettings = LevelStudioPlugin.Instance.gameModeAliases[meta.gameMode].CreateSettings();
            if (reader.ReadBoolean())
            {
                meta.modeSettings.ReadInto(reader);
            }
            if (version < 4)
            {
                if (version < 3)
                {
                    // maintain old behavior for old levels that weren't designed with pitstops or retries in mind
                    meta.playSettings.allowsRetries = false;
                    meta.playSettings.hasPitstop = false;
                }
                else
                {
                    meta.playSettings.hasPitstop = reader.ReadBoolean();
                    meta.playSettings.allowsRetries = reader.ReadBoolean();
                }
            }
            else
            {
                meta.playSettings = PlaymodeSettingsMeta.Read(reader);
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

    public class PlaymodeSettingsMeta
    {
        public bool hasPitstop = true;
        public bool allowsRetries = true;

        public const byte version = 0;
        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(hasPitstop);
            writer.Write(allowsRetries);
        }

        public static PlaymodeSettingsMeta Read(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            PlaymodeSettingsMeta settings = new PlaymodeSettingsMeta();
            settings.hasPitstop = reader.ReadBoolean();
            settings.allowsRetries = reader.ReadBoolean();
            return settings;
        }

        public PlaymodeSettingsMeta()
        {

        }

        public PlaymodeSettingsMeta(PlaymodeSettingsMeta toCopy)
        {
            hasPitstop = toCopy.hasPitstop;
            allowsRetries = toCopy.allowsRetries;
        }
    }

}
