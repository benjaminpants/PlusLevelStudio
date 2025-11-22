using HarmonyLib;
using MTM101BaldAPI.Registers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Ingame
{
    public class Structure_LockdownEventDoors : StructureBuilder
    {
        System.Random structureRandom = null;
        static FieldInfo _events = AccessTools.Field(typeof(EnvironmentController), "events");

        public override void GenerateInPremadeMap(System.Random rng)
        {
            structureRandom = rng;
        }

        // OnGenerationFinished is called at the completely wrong time, so we must use OnLoadingFinished
        public void OnLoadingFinished(LevelLoader ll)
        {
            List<RandomEvent> events = (List<RandomEvent>)_events.GetValue(ll.Ec);
            RandomEvent lockdownEvent = events.Find(x => x.Type == RandomEventType.Lockdown && (x.GetType() == typeof(LockdownEvent))); // dont want to accidently include subclasses of LockdownEvent here
            bool performCleanup = false;
            if (lockdownEvent == null)
            {
                Debug.LogWarning("Cannot find lockdown event for Structure_LockdownEventDoors! Creating event...");
                performCleanup = true;
                lockdownEvent = GameObject.Instantiate<RandomEvent>(RandomEventMetaStorage.Instance.Get(RandomEventType.Lockdown).value);
                lockdownEvent.Initialize(ll.Ec, structureRandom);
            }
            Debug.Log(structureRandom);
            Debug.Log(lockdownEvent);
            lockdownEvent.AfterUpdateSetup(structureRandom);
            Debug.Log("finished!");
            if (performCleanup)
            {
                Destroy(lockdownEvent.gameObject);
            }
        }
    }
}
