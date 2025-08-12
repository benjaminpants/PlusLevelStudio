using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MTM101BaldAPI;

namespace PlusLevelStudio.Lua
{
    [MoonSharpUserData]
    public class RoomProxy
    {
        private RoomController roomController;

        public RoomProxy(RoomController rc)
        {
            roomController = rc;
        }

        public bool IsHall()
        {
            return roomController.type == RoomType.Hall;
        }

        public string GetCategory()
        {
            return EnumExtensions.GetExtendedName<RoomCategory>((int)roomController.category);
        }

        public List<CellProxy> GetCells()
        {
            return roomController.cells.Select(x => new CellProxy(x)).ToList();
        }

        public List<LightProxy> GetLights()
        {
            return roomController.lights.Select(x => new LightProxy(x)).ToList();
        }

        public static bool operator ==(RoomProxy a, RoomProxy b) => a.roomController == b.roomController;
        public static bool operator !=(RoomProxy a, RoomProxy b) => a.roomController != b.roomController;
    }
}
