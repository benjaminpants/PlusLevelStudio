using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    /// <summary>
    /// Contains the important editor definitions for a specific mode
    /// </summary>
    public class EditorMode
    {
        public string id;
        public Dictionary<string, List<EditorTool>> availableTools = new Dictionary<string, List<EditorTool>>();
        public string[] defaultTools = new string[0];
        public string[] categoryOrder = new string[0];
        public bool supportsNPCProperties = true;
        public bool showSpawnpoint = true;
    }
}
