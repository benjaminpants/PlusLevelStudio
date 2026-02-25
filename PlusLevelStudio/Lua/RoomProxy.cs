using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using MoonSharp.Interpreter;
using MTM101BaldAPI;
using PlusLevelStudio.Ingame;
using PlusStudioLevelLoader;

namespace PlusLevelStudio.Lua
{
    [MoonSharpUserData]
    public class RoomProxy
    {
        private RoomController roomController;

        static FieldInfo _activity = AccessTools.Field(typeof(RoomController), "activity");
        static FieldInfo _notebook = AccessTools.Field(typeof(Activity), "notebook");

        public RoomProxy(RoomController rc)
        {
            roomController = rc;
        }

        public bool IsHall()
        {
            return roomController.type == RoomType.Hall;
        }

        public int GetZone()
        {
            if (roomController.TryGetComponent<EditorRegionMarker>(out EditorRegionMarker marker))
            {
                return marker.region;
            }
            return 0;
        }

        public bool hasActivity
        {
            get
            {
                return ((Activity)_activity.GetValue(roomController)) != null;
            }
        }

        public bool activityCompleted
        {
            get
            {
                if (!hasActivity) return false;
                return ((Activity)_activity.GetValue(roomController)).IsCompleted;
            }
        }

        public bool RespawnActivity()
        {
            Activity act = ((Activity)_activity.GetValue(roomController));
            if (act == null) return false;
            if (!act.IsCompleted) return false;
            if (!((Notebook)_notebook.GetValue(act)).hidden) return false;
            act.InstantReset();
            roomController.ec.notebookTotal++;
            Singleton<BaseGameManager>.Instance.CollectNotebooks(0);
            return true;
        }

        public string category
        {
            get
            {
                return EnumExtensions.GetExtendedName<RoomCategory>((int)roomController.category);
            }
        }

        // we have to do some fancy things here to preserve and silently remove the Room0_ that BB+'s level loader adds to room names
        public string name
        {
            get
            {
                string result = roomController.name;
                // remove characters until we reach the first underscore
                while (true)
                {
                    if (result[0] == '_')
                    {
                        result = result.Remove(0, 1);
                        break;
                    }
                    result = result.Remove(0,1);
                }
                return result;
            }
            set
            {
                string firstHalf = "";
                int index = 0;
                // keep adding characters to first half until we reach the first underscore
                while (true)
                {
                    if (roomController.name[index] == '_') break;
                    firstHalf += roomController.name[index];
                    index++;
                }
                roomController.name = firstHalf + value;
            }
        }

        public bool powered
        {
            get
            {
                return roomController.Powered;
            }
            set
            {
                roomController.SetPower(value);
            }
        }

        public ColorProxy mapColor
        {
            get
            {
                return new ColorProxy(roomController.color);
            }
            set
            {
                roomController.color = value.ToColor();
                foreach (Cell cell in roomController.cells)
                {
                    roomController.ec.map.UpdateTile(cell.position.x, cell.position.z, cell.ConstBin, roomController);
                }
            }
        }

        public void LockAllDoors()
        {
            for (int i = 0; i < roomController.doors.Count; i++)
            {
                roomController.doors[i].Shut();
                roomController.doors[i].Lock(false);
            }
        }

        public void UnlockAllDoors()
        {
            for (int i = 0; i < roomController.doors.Count; i++)
            {
                roomController.doors[i].Unlock();
            }
        }

        public void LockAllDoorsTimed(float time)
        {
            for (int i = 0; i < roomController.doors.Count; i++)
            {
                roomController.doors[i].Shut();
                roomController.doors[i].LockTimed(time);
            }
        }

        public List<CellProxy> GetCells()
        {
            return roomController.cells.Select(x => new CellProxy(x)).ToList();
        }

        public CellProxy GetRandomEntitySafeCell()
        {
            return new CellProxy(roomController.RandomEntitySafeCellNoGarbage());
        }

        public List<CellProxy> GetEntitySafeCells()
        {
            return roomController.AllEntitySafeCellsNoGarbage().Select(x => new CellProxy(x)).ToList();
        }

        public List<LightProxy> GetLights()
        {
            return roomController.lights.Select(x => new LightProxy(x)).ToList();
        }

        public bool RespawnItem(string itemId)
        {
            bool respawnAvailable = false;
            foreach (Pickup pickup in roomController.pickups)
            {
                if (!pickup.gameObject.activeSelf)
                {
                    respawnAvailable = true;
                    break;
                }
            }
            if (!respawnAvailable) return false;
            roomController.ec.RespawnItemInRoom(LevelLoaderPlugin.Instance.itemObjects[itemId], roomController);
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj is RoomProxy proxy &&
                   EqualityComparer<RoomController>.Default.Equals(roomController, proxy.roomController);
        }

        public override int GetHashCode()
        {
            return -1905862734 + EqualityComparer<RoomController>.Default.GetHashCode(roomController);
        }

        public static bool operator ==(RoomProxy a, RoomProxy b) => a.roomController == b.roomController;
        public static bool operator !=(RoomProxy a, RoomProxy b) => a.roomController != b.roomController;
    }
}
