using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Ingame
{
    public class Structure_EditorRegions : StructureBuilder
    {
        public override void Load(List<StructureData> data)
        {
            base.Load(data);
            for (int i = 0; i < data.Count; i++)
            {
                ec.rooms[data[i].position.x].gameObject.AddComponent<EditorRegionMarker>().region = data[i].position.z;
            }
        }
    }

    public class EditorRegionMarker : MonoBehaviour
    {
        public int region;
    }
}
