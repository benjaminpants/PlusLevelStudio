using MoonSharp.Interpreter;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Lua
{
    [MoonSharpUserData]
    public class ElevatorProxy
    {
        [MoonSharpHidden]
        public Elevator elevator;

        public void SetState(string state)
        {
            elevator.SetState(EnumExtensions.GetFromExtendedName<ElevatorState>(state));
        }

        public CellProxy cell
        {
            get
            {
                return new CellProxy(elevator.Door.ec.CellFromPosition(elevator.Door.position));
            }
        }

        public string state
        {
            get
            {
                return elevator.CurrentState.ToStringExtended();
            }
            set
            {
                SetState(state);
            }
        }

        public bool powered
        {
            get
            {
                return elevator.Powered;
            }
        }

        public bool gateIsOpen
        {
            get
            {
                return elevator.GateIsOpen;
            }
        }
    }
}
