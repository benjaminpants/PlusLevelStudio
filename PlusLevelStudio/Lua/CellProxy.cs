using HarmonyLib;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Lua
{
    [MoonSharpUserData]
    public class LightProxy
    {
        private Cell _cell;

        public CellProxy cell
        {
            get
            {
                return new CellProxy(_cell);
            }
        }

        public LightProxy(Cell cell)
        {
            _cell = cell;
        }

        public ColorProxy color
        {
            get
            {
                return new ColorProxy(_cell.lightColor);
            }
            set
            {
                _cell.lightColor = value.ToColor();
                BaseGameManager.Instance.Ec.QueueLightSourceForRegenerate(_cell);
            }
        }

        public int strength
        {
            get
            {
                return _cell.lightStrength;
            }
            set
            {
                _cell.lightStrength = value;
                BaseGameManager.Instance.Ec.QueueLightSourceForRegenerate(_cell);
            }
        }

        public void SetPower(bool power)
        {
            _cell.SetPower(power);
        }
    }

    [MoonSharpUserData]
    public class CellProxy
    {
        private Cell cell;

        public RoomProxy GetRoom()
        {
            if (cell.room == null) return null; // how
            return new RoomProxy(cell.room);
        }

        public LightProxy GetLight()
        {
            if (!cell.hasLight) return null;
            if (cell.permanentLight) return null;
            return new LightProxy(cell);
        }

        public LightProxy SetLight(ColorProxy color, int strength)
        {
            if (cell.permanentLight) return null;
            if (cell.hasLight)
            {
                cell.lightColor = color.ToColor();
                cell.lightStrength = strength;
                BaseGameManager.Instance.Ec.QueueLightSourceForRegenerate(cell);
                return GetLight();
            }
            BaseGameManager.Instance.Ec.GenerateLight(cell, color.ToColor(), strength);
            return GetLight();
        }

        public void PressAllButtons()
        {
            GameButtonBase[] buttons = cell.ObjectBase.GetComponentsInChildren<GameButtonBase>();
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Clicked(0);
            }
        }

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
