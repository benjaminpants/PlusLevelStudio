using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PlusLevelStudio.Editor.Ingame
{
    public class Structure_LockedRoom : StructureBuilder
    {
        public override void Load(List<StructureData> data)
        {
            base.Load(data);
            for (int i = 0; i < data.Count; i++)
            {
                GameLock chosenLock = data[i].prefab.GetComponent<GameLock>();
                LoadForRoom(ec.rooms[data[i].data], chosenLock);
            }
        }


        static FieldInfo _locks = AccessTools.Field(typeof(LockedRoomFunction), "locks");
        public void LoadForRoom(RoomController room, GameLock lck)
        {
            PreplacedLockedRoomFunction roomFunction = room.functionObject.AddComponent<PreplacedLockedRoomFunction>();
            room.functions.AddFunction(roomFunction);
            roomFunction.Initialize(room);
            List<GameLock> locks = (List<GameLock>)_locks.GetValue(roomFunction);
            foreach (Door door in room.doors)
            {
                GameLock gameLock = Instantiate(lck, room.functionObject.transform);
                locks.Add(gameLock);
                if (door.aTile.room == room)
                {
                    gameLock.transform.position = door.aTile.CenterWorldPosition;
                    gameLock.transform.rotation = door.direction.ToRotation();
                    gameLock.Initialize(roomFunction, room.ec, door.aTile.position, door.direction);
                }
                else
                {
                    gameLock.transform.position = door.bTile.CenterWorldPosition;
                    gameLock.transform.rotation = door.direction.GetOpposite().ToRotation();
                    gameLock.Initialize(roomFunction, room.ec, door.bTile.position, door.direction.GetOpposite());
                }
            }
            foreach (Door door in room.doors)
            {
                door.Lock(true);
            }
        }
    }

    public class PreplacedLockedRoomFunction : LockedRoomFunction
    {
        public override void Initialize(RoomController room)
        {
            this.room = room;
            //base.Initialize(room);
        }
        public override void AfterRoomValuesCalculated(LevelBuilder builder, System.Random rng)
        {
            // no
        }
    }
}
