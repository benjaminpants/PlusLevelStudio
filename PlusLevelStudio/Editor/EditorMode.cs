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
        public bool supportsNPCProperties = true;
        public bool caresAboutSpawn = true;
        public List<string> availableRandomEvents = new List<string>();
        public List<string> availableGameModes = new List<string>();
        public List<EditorGlobalPage> pages = new List<EditorGlobalPage>();
    }
}
