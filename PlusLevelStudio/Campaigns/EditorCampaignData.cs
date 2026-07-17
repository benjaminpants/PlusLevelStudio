using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlusLevelStudio.Campaigns
{
    public class EditorCampaignMeta
    {
        public string name = "My Awesome Campaign!";
        public string author = "Unknown";
        public string lifeMode = "normal";
    }

    public class EditorCampaignDataEntry
    {
        public string filePath;
        public PlayableEditorLevel level;
        private PlaymodeSettingsMeta _settings;
        public PlaymodeSettingsMeta settings => (level == null) ? _settings : level.meta.playSettings;

        /// <summary>
        /// Loads the level from the filePath variable and transfers the settings over there
        /// </summary>
        public void LoadLevelFromFilePath()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(filePath));
            level = PlayableEditorLevel.Read(reader);
            reader.Close();
            level.meta.playSettings = _settings;
            _settings = null;
        }

        public EditorCampaignDataEntry(string filePath, PlaymodeSettingsMeta settings)
        {
            this.filePath = filePath;
            _settings = settings;
        }

        public EditorCampaignDataEntry(string filePath, PlayableEditorLevel level)
        {
            this.filePath = filePath;
            this.level = level;
        }
    }

    public class EditorCampaignData
    {
        public string thumbPath = string.Empty;
        public EditorCampaignMeta meta = new EditorCampaignMeta();
        public List<EditorCampaignDataEntry> levelNamesAndMeta = new List<EditorCampaignDataEntry>();

        public const byte version = 1;

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(meta.name);
            writer.Write(meta.author);
            writer.Write(meta.lifeMode);
            writer.Write(thumbPath);
            writer.Write(levelNamesAndMeta.Count);
            for (int i = 0; i < levelNamesAndMeta.Count; i++)
            {
                writer.Write(PathHelpers.GetRelativePath(LevelStudioPlugin.levelExportPath, levelNamesAndMeta[i].filePath));
                levelNamesAndMeta[i].settings.Write(writer);
            }
        }

        public static EditorCampaignData Read(BinaryReader reader)
        {
            EditorCampaignData data = new EditorCampaignData();
            byte version = reader.ReadByte();
            data.meta.name = reader.ReadString();
            data.meta.author = reader.ReadString();
            data.meta.lifeMode = reader.ReadString();
            if (version >= 1)
            {
                data.thumbPath = reader.ReadString();
            }
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                data.levelNamesAndMeta.Add(new EditorCampaignDataEntry(Path.Combine(LevelStudioPlugin.levelExportPath, reader.ReadString()), PlaymodeSettingsMeta.Read(reader)));
            }
            return data;
        }
    }
}
