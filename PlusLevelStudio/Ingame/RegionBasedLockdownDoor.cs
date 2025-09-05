using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Ingame
{
    public class Structure_RegionBasedDoor : Structure_HallDoor
    {
        public void OnLoadingFinished(LevelLoader loader)
        {
            GameObject.FindObjectsOfType<RegionBasedLockdownDoor>().Do(x => x.InitializeRegions()); // not quite a "good" way of doing this, but it works, and isn't TOO hacky. Besides, the only thing that should be placing RegionBasedLockdownDoor is this.
        }
    }

    public class RegionBasedLockdownDoor : LockdownDoor
    {
        protected bool regionsInitialized = false;
        public int regionToCheck = 1;
        public List<RoomController> remainingRooms = new List<RoomController>();

        public override void Initialize()
        {
            base.Initialize();
        }

        public void InitializeRegions()
        {
            if (regionsInitialized) return;
            for (int i = 0; i < ec.rooms.Count; i++)
            {
                RoomController room = ec.rooms[i];
                if (!room.TryGetComponent<EditorRegionMarker>(out EditorRegionMarker marker)) continue;
                if (marker.region != regionToCheck) continue;
                if (room.notebookCollected) continue;
                if (!room.HasIncompleteActivity) continue;
                remainingRooms.Add(room);
            }
            regionsInitialized = true;
        }

        void Update()
        {
            if (!regionsInitialized) return;
            if (open) return;
            for (int i = (remainingRooms.Count - 1); i >= 0; i--)
            {
                if (remainingRooms[i].notebookCollected)
                {
                    remainingRooms.RemoveAt(i);
                }
            }
            if (remainingRooms.Count > 0) return;
            Open(true, false);
        }
    }
}
