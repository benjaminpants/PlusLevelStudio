using PlusLevelStudio.Editor;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace PlusLevelStudio.Campaigns
{
    public class PlayableEditorCampaign : IStudioPlayable
    {
        public string name = "My Awesome Campaign!";
        public string author = "Unknown";
        public EditorCustomContentPackage contentPackage = new EditorCustomContentPackage(false);

        public List<PlayableEditorLevel> levels = new List<PlayableEditorLevel>();

        public const byte version = 0;

        public void ImportLevels(List<PlayableEditorLevel> toImport)
        {
            levels.Clear();
            foreach (PlayableEditorLevel item in toImport)
            {
                PlayableEditorLevel level = new PlayableEditorLevel()
                {
                    data = item.data,
                    meta = new PlayableLevelMeta() { gameMode = item.meta.gameMode }
                };
                if (item.meta.modeSettings != null)
                {
                    level.meta.modeSettings = item.meta.modeSettings.MakeCopy();
                }
                levels.Add(level);
                if (item.meta.contentPackage == null) continue;
                foreach (EditorCustomContentEntry entry in item.meta.contentPackage.entries)
                {
                    if (entry.contentType == "thumbnail") continue;
                    if (contentPackage.entries.Find(x => x.id == entry.id && x.contentType == entry.contentType) != null) continue;
                    contentPackage.entries.Add(new EditorCustomContentEntry(entry.contentType, entry.id, entry.GetData()));
                }
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(name);
            writer.Write(author);
            contentPackage.Write(writer);
            writer.Write(levels.Count);
            for (int i = 0; i < levels.Count; i++)
            {
                writer.Write(levels[i].meta.gameMode);
                writer.Write(levels[i].meta.modeSettings != null);
                if (levels[i].meta.modeSettings != null)
                {
                    levels[i].meta.modeSettings.Write(writer);
                }
                levels[i].data.Write(writer);
            }
        }

        public static PlayableEditorCampaign Read(BinaryReader reader)
        {
            PlayableEditorCampaign camp = new PlayableEditorCampaign();
            byte version = reader.ReadByte();
            camp.name = reader.ReadString();
            camp.author = reader.ReadString();
            camp.contentPackage = EditorCustomContentPackage.Read(reader);
            int levelCount = reader.ReadInt32();
            for (int i = 0; i < levelCount; i++)
            {
                PlayableEditorLevel level = new PlayableEditorLevel();
                level.meta = new PlayableLevelMeta();
                level.meta.name = string.Empty;
                level.meta.author = string.Empty;
                level.meta.contentPackage = null;
                level.meta.gameMode = reader.ReadString();
                if (reader.ReadBoolean())
                {
                    level.meta.modeSettings = LevelStudioPlugin.Instance.gameModeAliases[level.meta.gameMode].CreateSettings();
                    level.meta.modeSettings.ReadInto(reader);
                }
                level.data = BaldiLevel.Read(reader);
                camp.levels.Add(level);
            }
            return camp;
        }

        public string GetName()
        {
            return name;
        }

        public string GetAuthor()
        {
            return author;
        }

        public string GetLocalizedGamemode()
        {
            string baseL = levels[0].meta.gameMode;
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].meta.gameMode != baseL) return LocalizationManager.Instance.GetLocalizedText("Ed_GameMode_Campaign_Mixed");
            }
            return string.Format(LocalizationManager.Instance.GetLocalizedText("Ed_GameMode_Campaign"), LocalizationManager.Instance.GetLocalizedText(LevelStudioPlugin.Instance.gameModeAliases[baseL].nameKey));
        }

        public EditorCustomContentEntry GetThumbnail()
        {
            return contentPackage.thumbnailEntry;
        }

        public void Play()
        {
            EditorPlayModeManager.LoadCampaign(this, 2, LifeMode.Normal);
        }
    }
}
