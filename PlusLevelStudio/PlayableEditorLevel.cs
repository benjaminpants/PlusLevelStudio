using PlusLevelStudio.Editor;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlusLevelStudio
{
    public class PlayableEditorLevel
    {
        public PlayableLevelMeta meta;
        public BaldiLevel data;

        public const byte version = 0;

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            meta.Write(writer);
            data.Write(writer);
        }

        public static PlayableEditorLevel Read(BinaryReader reader)
        {
            PlayableEditorLevel playable = new PlayableEditorLevel();
            byte version = reader.ReadByte();
            playable.meta = PlayableLevelMeta.Read(reader);
            playable.data = BaldiLevel.Read(reader);
            return playable;
        }
    }

    public class PlayableLevelMeta
    {
        public string name;
        public string gameMode;
        public EditorGameModeSettings modeSettings;

        public const byte version = 0; // so we can read files from earlier versions, we fudge the version number here

        public void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(name);
            writer.Write(gameMode);
            writer.Write(modeSettings != null);
            if (modeSettings == null) return;
            modeSettings.Write(writer);
        }

        public static PlayableLevelMeta Read(BinaryReader reader)
        {
            PlayableLevelMeta meta = new PlayableLevelMeta();
            byte version = reader.ReadByte();
            meta.name = reader.ReadString();
            meta.gameMode = reader.ReadString();
            if (!reader.ReadBoolean())
            {
                meta.modeSettings = LevelStudioPlugin.Instance.gameModeAliases[meta.gameMode].CreateSettings();
                return meta;
            }
            meta.modeSettings = LevelStudioPlugin.Instance.gameModeAliases[meta.gameMode].CreateSettings();
            meta.modeSettings.ReadInto(reader);
            return meta;
        }
    }
}
