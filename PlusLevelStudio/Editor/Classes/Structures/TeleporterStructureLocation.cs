using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using PlusLevelStudio;
using PlusStudioLevelLoader;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor
{
    public class TeleporterMachineLocation : IEditorVisualizable, IEditorDeletable, IEditorMovable
    {
        public TeleporterLocation myTeleporter;
        public Vector2 position;
        public float direction;
        public void CleanupVisual(GameObject visualObject)
        {

        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericStructureDisplays["teleporter_machine"];
        }

        public virtual bool ValidatePosition(EditorLevelData data, bool ignoreSelf)
        {
            PlusStudioLevelFormat.Cell cell = data.GetCellSafe(new IntVector2(Mathf.RoundToInt((position.x - 5f) / 10f), Mathf.RoundToInt((position.y - 5f) / 10f)));
            if (cell == null) return false; // cell doesn't exist
            if (cell.type == 16) return false; // the cell is empty
            if (!RoomValid(data.RoomFromId(cell.roomId))) return false; // our cell isn't a valid room
            return true;
        }

        public bool RoomValid(EditorRoom room)
        {
            return TeleporterLocation.RoomValid(room);
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            visualObject.GetComponent<MovableObjectInteraction>().target = this;
            UpdateVisual(visualObject);
        }

        public bool OnDelete(EditorLevelData data)
        {
            return myTeleporter.OnDelete(data);
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = new Vector3(position.x, 0f, position.y);
            visualObject.transform.eulerAngles = new Vector3(0f, direction, 0f);
        }

        bool hasMoved = false;

        public void Selected()
        {
            EditorController.Instance.GetVisual(this).GetComponent<EditorRendererContainer>().Highlight("yellow");
            EditorController.Instance.HoldUndo();
        }

        public void Unselected()
        {
            EditorController.Instance.GetVisual(this).GetComponent<EditorRendererContainer>().Highlight("none");
            if (hasMoved)
            {
                EditorController.Instance.AddHeldUndo();
            }
            else
            {
                EditorController.Instance.CancelHeldUndo();
            }
            myTeleporter.myStructure.DeleteIfInvalid();
            hasMoved = false;
        }

        public void MoveUpdate(Vector3? position, Quaternion? rotation)
        {
            if (position.HasValue)
            {
                this.position = new Vector2(position.Value.x,position.Value.z);
                hasMoved = true;
            }
            if (rotation.HasValue)
            {
                direction = rotation.Value.eulerAngles.y;
                hasMoved = true;
            }
            EditorController.Instance.UpdateVisual(this);
        }

        public Transform GetTransform()
        {
            return EditorController.Instance.GetVisual(this).transform;
        }
    }
    public class TeleporterLocation : IEditorVisualizable, IEditorDeletable, IEditorMovable
    {
        public TeleporterStructureLocation myStructure;
        public TeleporterMachineLocation machine;
        public Vector2 position;
        public float direction;
        public void CleanupVisual(GameObject visualObject)
        {
            EditorController.Instance.RemoveVisual(machine);
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericStructureDisplays["teleporter_buttons"];
        }

        public bool ValidatePosition(EditorLevelData data, bool ignoreSelf)
        {
            PlusStudioLevelFormat.Cell cell = data.GetCellSafe(new IntVector2(Mathf.RoundToInt((position.x - 5f) / 10f), Mathf.RoundToInt((position.y - 5f) / 10f)));
            if (cell == null) return false; // cell doesn't exist
            if (cell.type == 16) return false; // the cell is empty
            if (!RoomValid(data.RoomFromId(cell.roomId))) return false; // our cell isn't a valid room
            return machine.ValidatePosition(data, ignoreSelf);
        }

        public static bool RoomValid(EditorRoom room)
        {
            if (room == null) return false;
            return room.roomType.StartsWith("teleportroom_");
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            visualObject.GetComponent<MovableObjectInteraction>().target = this;
            if (EditorController.Instance.GetVisual(machine) == null)
            {
                EditorController.Instance.AddVisual(machine);
            }
            UpdateVisual(visualObject);
        }

        public bool OnDelete(EditorLevelData data)
        {
            return myStructure.DeleteTeleporter(this);
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = new Vector3(position.x,0f,position.y);
            visualObject.transform.eulerAngles = new Vector3(0f, direction, 0f);
            EditorController.Instance.UpdateVisual(machine);
        }

        bool hasMoved = false;

        public void Selected()
        {
            EditorController.Instance.GetVisual(this).GetComponent<EditorRendererContainer>().Highlight("yellow");
            EditorController.Instance.HoldUndo();
        }

        public void Unselected()
        {
            EditorController.Instance.GetVisual(this).GetComponent<EditorRendererContainer>().Highlight("none");
            if (hasMoved)
            {
                EditorController.Instance.AddHeldUndo();
            }
            else
            {
                EditorController.Instance.CancelHeldUndo();
            }
            myStructure.DeleteIfInvalid();
            hasMoved = false;
        }

        public void MoveUpdate(Vector3? position, Quaternion? rotation)
        {
            if (position.HasValue)
            {
                this.position = new Vector2(position.Value.x, position.Value.z);
                hasMoved = true;
            }
            if (rotation.HasValue)
            {
                direction = rotation.Value.eulerAngles.y;
                hasMoved = true;
            }
            EditorController.Instance.UpdateVisual(this);
        }

        public Transform GetTransform()
        {
            return EditorController.Instance.GetVisual(this).transform;
        }
    }


    public class TeleporterStructureLocation : StructureLocation
    {
        public List<TeleporterLocation> teleporters = new List<TeleporterLocation>();
        public bool DeleteTeleporter(TeleporterLocation location)
        {
            return DeleteTeleporter(location, true);
        }

        public bool DeleteTeleporter(TeleporterLocation location, bool validateSelf)
        {
            EditorController.Instance.RemoveVisual(location);
            bool returnValue = teleporters.Remove(location);
            if (validateSelf)
            {
                DeleteIfInvalid();
            }
            return returnValue;
        }

        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            for (int i = 0; i < teleporters.Count; i++)
            {
                EditorController.Instance.RemoveVisual(teleporters[i]);
            }
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo info = new StructureInfo(type);
            for (int i = 0; i < teleporters.Count; i++)
            {
                info.data.Add(new StructureDataInfo()
                {
                    data = data.rooms.IndexOf(data.RoomFromPos(new Vector3(teleporters[i].position.x, 0f, teleporters[i].position.y).ToCellVector(), true))
                });
                info.data.Add(new StructureDataInfo()
                {
                    data = teleporters[i].direction.ConvertToIntNoRecast(),
                    position = new MystIntVector2(teleporters[i].position.x.ConvertToIntNoRecast(), teleporters[i].position.y.ConvertToIntNoRecast())
                });
                info.data.Add(new StructureDataInfo()
                {
                    data = teleporters[i].machine.direction.ConvertToIntNoRecast(),
                    position = new MystIntVector2(teleporters[i].machine.position.x.ConvertToIntNoRecast(), teleporters[i].machine.position.y.ConvertToIntNoRecast())
                });
            }
            return info;
        }

        public override GameObject GetVisualPrefab()
        {
            return null;
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            for (int i = 0; i < teleporters.Count; i++)
            {
                EditorController.Instance.AddVisual(teleporters[i]);
            }
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            for (int i = 0; i < teleporters.Count; i++)
            {
                Vector2 offsetPos = new Vector2(worldOffset.x, worldOffset.z);
                teleporters[i].position -= offsetPos;
                teleporters[i].machine.position -= offsetPos;
            }
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            for (int i = 0; i < teleporters.Count; i++)
            {
                EditorController.Instance.UpdateVisual(teleporters[i]);
            }
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            List<EditorRoom> rooms = new List<EditorRoom>();
            for (int i = (teleporters.Count - 1); i >= 0; i--)
            {
                if (!teleporters[i].ValidatePosition(data, true))
                {
                    DeleteTeleporter(teleporters[i], false);
                }
                else
                {
                    EditorRoom baseRoom = data.RoomFromPos(new IntVector2(Mathf.RoundToInt((teleporters[i].position.x - 5f) / 10f), Mathf.RoundToInt((teleporters[i].position.y - 5f) / 10f)), true);
                    EditorRoom machineRoom = data.RoomFromPos(new IntVector2(Mathf.RoundToInt((teleporters[i].machine.position.x - 5f) / 10f), Mathf.RoundToInt((teleporters[i].machine.position.y - 5f) / 10f)), true);
                    if (baseRoom != machineRoom)
                    {
                        DeleteTeleporter(teleporters[i], false);
                    }
                    else
                    {
                        if (!rooms.Contains(baseRoom))
                        {
                            rooms.Add(baseRoom);
                        }
                        else
                        {
                            DeleteTeleporter(teleporters[i], false);
                        }
                    }
                }
            }
            return teleporters.Count > 0;
        }

        public const byte version = 0;

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            byte version = reader.ReadByte();
            int teleporterCount = reader.ReadInt32();
            for (int i = 0; i < teleporterCount; i++)
            {
                TeleporterLocation teleLocation = new TeleporterLocation();
                teleLocation.position = reader.ReadUnityVector2().ToUnity();
                teleLocation.direction = reader.ReadSingle();
                teleLocation.machine = new TeleporterMachineLocation();
                teleLocation.machine.position = reader.ReadUnityVector2().ToUnity();
                teleLocation.machine.direction = reader.ReadSingle();
                teleLocation.myStructure = this;
                teleLocation.machine.myTeleporter = teleLocation;
                teleporters.Add(teleLocation);
            }
        }
        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(version);
            writer.Write(teleporters.Count);
            for (int i = 0; i < teleporters.Count; i++)
            {
                writer.Write(teleporters[i].position.ToData());
                writer.Write(teleporters[i].direction);
                writer.Write(teleporters[i].machine.position.ToData());
                writer.Write(teleporters[i].machine.direction);
            }
        }
    }
}
