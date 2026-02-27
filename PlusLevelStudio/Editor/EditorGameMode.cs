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
        /// The path for the settings page JSON file. This will be ignored if hasSettingsPage is false.
        /// </summary>
        public string settingsPagePath;
        /// <summary>
        /// The type for the settings page. This will be ignored if hasSettingsPage is false.
        /// </summary>
        public Type settingsPageType;

        public string nameKey;
        public string descKey;
        public bool hasSettingsPage = false;

        /// <summary>
        /// Creates the settings for the specified mode. Return null to indicate that this mode has no configurable settings.
        /// </summary>
        /// <returns></returns>
        public virtual EditorGameModeSettings CreateSettings()
        {
            return null;
        }

        /// <summary>
        /// Called when an old level made in this mode attempts to load in editor. flagsToHandle is the levelData's LegacyFlags.
        /// 
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="flagsToHandle"></param>
        /// <returns></returns>
        public virtual void AttemptToUpdateLegacyLevel(EditorController controller, StudioLevelLegacyFlags flagsToHandle)
        {
            
        }

        public virtual void ApplyDefaultNPCProperties(string npc, NPCProperties props)
        {

        }
    }

    public abstract class EditorGameModeSettings
    {
        public abstract void Write(BinaryWriter writer);
        public abstract void ReadInto(BinaryReader reader);

        /// <summary>
        /// Makes a copy of the settings by writing into a memory stream and then reading that back.
        /// </summary>
        /// <returns></returns>
        public EditorGameModeSettings MakeCopy()
        {
            MemoryStream memoryStream = new MemoryStream();
            Write(new BinaryWriter(memoryStream));
            memoryStream.Position = 0;
            EditorGameModeSettings settings = (EditorGameModeSettings)Activator.CreateInstance(GetType());
            settings.ReadInto(new BinaryReader(memoryStream));
            return settings;
        }

        public abstract void ApplySettingsToManager(BaseGameManager manager);

        public EditorGameModeSettings()
        {

        }
    }
}
