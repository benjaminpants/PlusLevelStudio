using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Lua
{
    [MoonSharpUserData]
    public class CellProxy
    {
        private Cell cell;

        public IntVector2Proxy position
        {
            get
            {
                return new IntVector2Proxy(cell.position);
            }
        }

        public Vector3Proxy FloorWorldPosition
        {
            get
            {
                return new Vector3Proxy(cell.FloorWorldPosition);
            }
        }

        public Vector3Proxy CenterWorldPosition
        {
            get
            {
                return new Vector3Proxy(cell.CenterWorldPosition);
            }
        }

        public CellProxy(Cell cell)
        {
            this.cell = cell;
        }
    }
}
