using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
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
    }
}
