using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusStudioLevelLoader
{
    public class LoaderStructureData
    {
        public StructureBuilder structure;

        public Dictionary<string, GameObject> prefabAliases = new Dictionary<string, GameObject>();

        public LoaderStructureData()
        {

        }

        public LoaderStructureData(StructureBuilder structure)
        {
            this.structure = structure;
        }

        public LoaderStructureData(StructureBuilder structure, Dictionary<string, GameObject> prefabAliases) : this(structure)
        {
            this.prefabAliases = prefabAliases;
        }
    }
}
