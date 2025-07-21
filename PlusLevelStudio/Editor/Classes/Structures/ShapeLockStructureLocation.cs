using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class ShapeLockRoomLocation : IEditorDeletable
    {
        public EditorRoom room;
        public string lockType;
        public List<GameObject> allocatedLocks = new List<GameObject>();
        public ShapeLockStructureLocation owner;

        public bool OnDelete(EditorLevelData data)
        {
            owner.OnRoomDelete(this, true);
            return true;
        }
    }
    public class ShapeLockStructureLocation : StructureLocation
    {
        public ShapeLockStructureLocation()
        {
            
        }

        public List<ShapeLockRoomLocation> lockedRooms = new List<ShapeLockRoomLocation>();
        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            compressor.AddStrings(lockedRooms.Select(x => x.lockType));
        }
        
        public void OnRoomDelete(ShapeLockRoomLocation room, bool performCheck)
        {
            lockedRooms.Remove(room);
            ClearAllAllocatedLocksForRoom(room);
            if (!performCheck) return;
            DeleteIfInvalid();
        }

        public void DeleteIfInvalid()
        {
            if (!ValidatePosition(EditorController.Instance.levelData))
            {
                EditorController.Instance.levelData.structures.Remove(this);
                EditorController.Instance.RemoveVisual(this);
            }
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            for (int i = 0; i < lockedRooms.Count; i++)
            {
                ClearAllAllocatedLocksForRoom(lockedRooms[i]);
            }
        }

        public override StructureInfo Compile(EditorLevelData editorLevel, BaldiLevel level)
        {
            StructureInfo myInfo = new StructureInfo();
            myInfo.type = type;
            for (int i = 0; i < lockedRooms.Count; i++)
            {
                StructureDataInfo data = new StructureDataInfo();
                data.data = editorLevel.rooms.IndexOf(lockedRooms[i].room);
                data.prefab = lockedRooms[i].lockType;
                myInfo.data.Add(data);
            }
            return myInfo;
        }

        public override GameObject GetVisualPrefab()
        {
            return null;
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            for (int i = 0; i < lockedRooms.Count; i++)
            {
                UpdateVisualForRoom(lockedRooms[i]);
            }
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            byte version = reader.ReadByte();
            ushort lockedCount = reader.ReadUInt16();
            for (int i = 0; i < lockedCount; i++)
            {
                CreateAndAddRoom(compressor.ReadStoredString(reader), data.RoomFromId(reader.ReadUInt16()));
            }
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            // nothing needs to be done here
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            for (int i = 0; i < lockedRooms.Count; i++)
            {
                UpdateVisualForRoom(lockedRooms[i]);
            }
        }

        public void ClearAllAllocatedLocksForRoom(ShapeLockRoomLocation room)
        {
            for (int i = room.allocatedLocks.Count - 1; i >= 0; i--)
            {
                GameObject.Destroy(room.allocatedLocks[i]);
                room.allocatedLocks.Remove(room.allocatedLocks[i]);
            }
        }

        public ShapeLockRoomLocation CreateAndAddRoom(string type, EditorRoom targetRoom)
        {
            ShapeLockRoomLocation shrl = new ShapeLockRoomLocation();
            shrl.lockType = type;
            shrl.owner = this;
            shrl.room = targetRoom;
            lockedRooms.Add(shrl);
            return shrl;
        }

        public void UpdateVisualForRoom(ShapeLockRoomLocation room)
        {
            List<DoorLocation> doorsWithoutVisual = EditorController.Instance.levelData.doors.Where(x => x.DoorConnectedToRoom(EditorController.Instance.levelData, room.room, true)).ToList();
            List<GameObject> unusedObjects = new List<GameObject>(room.allocatedLocks);
            while (doorsWithoutVisual.Count > 0)
            {
                DoorLocation target = doorsWithoutVisual[0];
                doorsWithoutVisual.RemoveAt(0);
                if (LevelStudioPlugin.Instance.doorIngameStatus[target.type] == DoorIngameStatus.AlwaysObject) continue; // we dont need to check for smart doors because by this point all doors outside of objects will become regular doors
                GameObject lockVisual;
                if (unusedObjects.Count > 0)
                {
                    lockVisual = unusedObjects[0];
                    unusedObjects.RemoveAt(0);
                }
                else
                {
                    lockVisual = GameObject.Instantiate(LevelStudioPlugin.Instance.genericStructureDisplays[room.lockType]);
                    lockVisual.GetComponent<EditorDeletableObject>().toDelete = room;
                    room.allocatedLocks.Add(lockVisual);
                }
                IntVector2 targetPos = target.position;
                Direction targetDirection = target.direction;
                // we want the lock visual to face outwards but apparently we do that by flipping if the tile lands on the room??
                if (EditorController.Instance.levelData.RoomFromPos(targetPos, true) != room.room)
                {
                    targetPos = target.position + target.direction.ToIntVector2();
                    targetDirection = target.direction.GetOpposite();
                }
                lockVisual.transform.position = targetPos.ToWorld() + (Vector3.up * 5f);
                lockVisual.transform.rotation = targetDirection.ToRotation();
            }
            while (unusedObjects.Count > 0)
            {
                GameObject target = unusedObjects[0];
                unusedObjects.RemoveAt(0);
                GameObject.Destroy(target);
                room.allocatedLocks.Remove(target);
            }
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            for (int i = lockedRooms.Count - 1; i >= 0; i--)
            {
                if (!data.rooms.Contains(lockedRooms[i].room)) // its room is gone
                {
                    OnRoomDelete(lockedRooms[i], false);
                }
            }
            return lockedRooms.Count > 0;
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write((byte)0);
            writer.Write((ushort)lockedRooms.Count);
            for (int i = 0; i < lockedRooms.Count; i++)
            {
                compressor.WriteStoredString(writer, lockedRooms[i].lockType);
                writer.Write(data.IdFromRoom(lockedRooms[i].room));
            }
        }

        public override bool ShouldUpdateVisual(PotentialStructureUpdateReason reason)
        {
            return reason == PotentialStructureUpdateReason.CellChange;
        }
    }
}
