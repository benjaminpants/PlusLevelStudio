﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MTM101BaldAPI;
using PlusLevelStudio.Ingame;

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

        public int GetZone()
        {
            if (roomController.TryGetComponent<EditorRegionMarker>(out EditorRegionMarker marker))
            {
                return marker.region;
            }
            return 0;
        }

        public string category
        {
            get
            {
                return EnumExtensions.GetExtendedName<RoomCategory>((int)roomController.category);
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
