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
        public List<EditorTool> availableTools = new List<EditorTool>();
        public bool supportsNPCProperties = true;
    }
}
