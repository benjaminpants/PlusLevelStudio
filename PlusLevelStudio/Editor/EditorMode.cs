using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{

    public class EditorGlobalPage
    {
        /// <summary>
        /// The name of the category object.
        /// </summary>
        public string pageName;
        /// <summary>
        /// The localization key for the name of the page ingame.
        /// </summary>
        public string pageKey;
        /// <summary>
        /// The path to load the page UI from.
        /// </summary>
        public string filePath;
        /// <summary>
        /// The type of the manager. Should inherit from GlobalSettingsUIExchangeHandler
        /// </summary>
        public Type managerType;
    }

    /// <summary>
    /// Contains the important editor definitions for a specific mode, along with its prefab.
    /// </summary>
    public class EditorMode
    {
        public string id;
        public Dictionary<string, List<EditorTool>> availableTools = new Dictionary<string, List<EditorTool>>();
        public EditorController prefab;
        public string[] defaultTools = new string[0];
        public string[] categoryOrder = new string[0];
        public bool vanillaComplaint = false;
        public bool allowOutOfRoomObjects = true;
        public bool caresAboutSpawn = true;
        public List<string> availableRandomEvents = new List<string>();
        public List<string> availableGameModes = new List<string>();
        public List<EditorGlobalPage> pages = new List<EditorGlobalPage>();
        public List<GlobalStructurePage> globalStructures = new List<GlobalStructurePage>();
        public List<GlobalStructurePage> globalRandomStructures = new List<GlobalStructurePage>();

        public EditorMode MakeCopy()
        {
            EditorMode mode = new EditorMode();
            mode.id = id;
            mode.defaultTools = defaultTools;
            mode.categoryOrder = categoryOrder;
            mode.vanillaComplaint = vanillaComplaint;
            mode.allowOutOfRoomObjects = allowOutOfRoomObjects;
            mode.caresAboutSpawn = caresAboutSpawn;
            mode.availableGameModes = new List<string>(availableGameModes);
            mode.availableRandomEvents = new List<string>(availableRandomEvents);
            mode.pages = new List<EditorGlobalPage>(pages);
            mode.globalStructures = new List<GlobalStructurePage>(globalStructures);
            mode.globalRandomStructures = new List<GlobalStructurePage>(globalRandomStructures);
            foreach (var item in availableTools)
            {
                mode.availableTools.Add(item.Key, new List<EditorTool>(item.Value));
            }
            return mode;
        }
    }
}
