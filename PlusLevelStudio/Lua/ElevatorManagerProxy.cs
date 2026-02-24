using MoonSharp.Interpreter;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlusLevelStudio.Lua
{
    [MoonSharpUserData]
    public class ElevatorManagerProxy
    {
        [MoonSharpHidden]
        public ElevatorManager elevatorManager;

        public void SetIntendedElevatorState(ElevatorProxy elevator, string state)
        {
            elevatorManager.SetIntendedElevatorState(elevator.elevator, EnumExtensions.GetFromExtendedName<ElevatorState>(state));
        }

        public string GetIntendedElevatorState(ElevatorProxy elevator)
        {
            return elevatorManager.GetIntendedElevatorState(elevator.elevator).ToStringExtended();
        }

        public void SetTotalOutOfOrderElevators(int total)
        {
            elevatorManager.SetTotalOutOfOrderElevators(total);
        }

        public void SetAllElevators(string state)
        {
            elevatorManager.SetAllElevators(EnumExtensions.GetFromExtendedName<ElevatorState>(state));
        }

        public List<ElevatorProxy> GetElevators()
        {
            return elevatorManager.Elevators.Select(x => new ElevatorProxy() { elevator = x }).ToList();
        }
    }
}
