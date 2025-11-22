using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PlusLevelStudio.Ingame
{
    public class PlacedLockdownEventDoor : LockdownDoor
    {
        static FieldInfo _events = AccessTools.Field(typeof(EnvironmentController), "events");
        static FieldInfo _doors = AccessTools.Field(typeof(LockdownEvent), "doors");
        public override void Initialize()
        {
            base.Initialize();
            ec.OnEnvironmentBeginPlay = (EnvironmentController.EnvironmentBeginPlay)Delegate.Combine(ec.OnEnvironmentBeginPlay, new EnvironmentController.EnvironmentBeginPlay(SearchForEventAndAdd));
        }

        public void SearchForEventAndAdd()
        {
            List<RandomEvent> events = (List<RandomEvent>)_events.GetValue(ec);
            RandomEvent lockdownEvent = events.Find(x => x.Type == RandomEventType.Lockdown && (x.GetType() == typeof(LockdownEvent))); // dont want to accidently include subclasses of LockdownEvent here
            if (lockdownEvent == null) return; // no point in going further
            ((List<Door>)_doors.GetValue(lockdownEvent)).Add(this);
        }
    }
}
