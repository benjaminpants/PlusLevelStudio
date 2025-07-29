using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public class EditorGameMode
    {
        public BaseGameManager prefab;
        /// <summary>
        /// The path for the settings page JSON file. This will be ignored if CreateSettings returns null.
        /// </summary>
        public string settingsPagePath;
        /// <summary>
        /// The type for the settings page. This will be ignored if CreateSettings returns null.
        /// </summary>
        public Type settingsPageType;

        public string nameKey;
        public string descKey;

        /// <summary>
        /// Creates the settings for the specified mode. Return null to indicate that this mode has no configurable settings.
        /// </summary>
        /// <returns></returns>
        public virtual EditorGameModeSettings CreateSettings()
        {
            return null;
        }
    }
    public abstract class EditorGameModeSettings
    {
        public abstract void Write(BinaryWriter writer);
        public abstract void ReadInto(BinaryReader reader);
    }
}
