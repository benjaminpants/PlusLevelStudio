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
                if (ec.rooms[data[i].position.x].gameObject.GetComponent<EditorRegionMarker>()) return;
                ec.rooms[data[i].position.x].gameObject.AddComponent<EditorRegionMarker>().region = data[i].position.z;
            }
        }
    }

    public class EditorRegionMarker : MonoBehaviour
    {
        public int region;
    }

    public class EditorRegionMarkFunction : RoomFunction
    {
        public int region;
        public override void Initialize(RoomController room)
        {
            base.Initialize(room);
            room.gameObject.AddComponent<EditorRegionMarker>().region = region;
        }
    }
}
