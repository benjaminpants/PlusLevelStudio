using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public class GlobalStructurePage
    {
        public string structureToSpawn;
        /// <summary>
        /// The path for the settings page JSON file. Ignored if settingsPageType is null.
        /// </summary>
        public string settingsPagePath = string.Empty;
        /// <summary>
        /// The type for the settings page.
        /// </summary>
        public Type settingsPageType = null;

        public string nameKey;
        public string descKey;
    }
}
