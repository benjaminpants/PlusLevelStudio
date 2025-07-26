using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class EditorBeltVisualManager : MonoBehaviour
    {
        public List<MeshRenderer> beltRenderers = new List<MeshRenderer>();
        public EditorDeletableObject deletable;
        public MeshRenderer beltRenderPre;
        public BoxCollider collider;
        public TextureSlider slider;
        public int length;

        public void InitializeVisuals(int length)
        {
            foreach (var item in beltRenderers)
            {
                Destroy(item.gameObject);
            }
            this.length = length;
            beltRenderers.Clear();
            deletable.myRenderers.Clear();
            for (int i = 0; i < length; i++)
            {
                MeshRenderer clone = GameObject.Instantiate<MeshRenderer>(beltRenderPre, transform);
                clone.transform.localPosition = Vector3.forward * i * 10f;
                deletable.AddRenderer(clone, "none");
                beltRenderers.Add(clone);
            }
            collider.size = new Vector3(10f,0.1f, length * 10f);
            collider.center = Vector3.forward * (length - 1) * 5f;
        }
    }


    public class ConveyorBeltLocation : IEditorVisualizable, IEditorDeletable
    {
        public IntVector2 startPosition;
        public IntVector2 endPosition => startPosition + (direction.ToIntVector2() * (distance - 1));
        public byte distance;
        public Direction direction;
        public ConveyorBeltStructureLocation owner;
        public int buttonIndex = -1;

        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public bool ValidatePosition(EditorLevelData data)
        {
            PlusStudioLevelFormat.Cell cellStart = data.GetCellSafe(startPosition);
            PlusStudioLevelFormat.Cell cellEnd = data.GetCellSafe(endPosition);
            if (cellStart == null) return false;
            if (cellStart.roomId == 0) return false;
            if (cellEnd == null) return false;
            if (cellEnd.roomId == 0) return false;
            return true;
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericStructureDisplays["conveyorbelt"];
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorBeltVisualManager>().InitializeVisuals(distance);
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public void UpdateVisual(GameObject visualObject)
        {
            if (visualObject.GetComponent<EditorBeltVisualManager>().length != distance)
            {
                visualObject.GetComponent<EditorBeltVisualManager>().InitializeVisuals(distance);
            }
            visualObject.transform.position = startPosition.ToWorld();
            visualObject.transform.rotation = direction.ToRotation();
        }

        public bool OnDelete(EditorLevelData data)
        {
            owner.DeleteBelt(data, this);
            return true;
        }
    }

    public class ConveyorBeltStructureLocation : StructureLocation
    {
        public List<ConveyorBeltLocation> belts = new List<ConveyorBeltLocation>();
        public List<SimpleButtonLocation> buttons = new List<SimpleButtonLocation>();
        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            
        }

        // the code is all set up for multiple belts connecting to 1 but due to a bug it goes unused
        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo info = new StructureInfo(type);
            if (belts.Count == 0)
            {
                Debug.LogWarning("Compiling Conveyor belts with no belts???");
                return info;
            }
            Dictionary<int, List<ConveyorBeltLocation>> kvps = new Dictionary<int, List<ConveyorBeltLocation>>();
            for (int i = 0; i < belts.Count; i++)
            {
                if (!kvps.ContainsKey(belts[i].buttonIndex))
                {
                    kvps.Add(belts[i].buttonIndex, new List<ConveyorBeltLocation>());
                }
                kvps[belts[i].buttonIndex].Add(belts[i]);
            }
            foreach (var kvp in kvps)
            {
                if (kvp.Key == -1) continue; //we want to handle this one seperately
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    ConveyorBeltLocation belt = kvp.Value[i];
                    info.data.Add(new StructureDataInfo()
                    {
                        position = belt.startPosition.ToByte(),
                        direction = (PlusDirection)belt.direction,
                        data = 0
                    });
                    info.data.Add(new StructureDataInfo()
                    {
                        position = (belt.startPosition + (belt.direction.ToIntVector2() * (belt.distance - 1))).ToByte(),
                        data = 0
                    });
                }
                info.data.Add(new StructureDataInfo()
                {
                    data = 1,
                    position = buttons[kvp.Key].position.ToByte(),
                    direction = (PlusDirection)buttons[kvp.Key].direction,
                });
            }
            if (kvps.ContainsKey(-1))
            {
                for (int i = 0; i < kvps[-1].Count; i++)
                {
                    ConveyorBeltLocation belt = kvps[-1][i];
                    info.data.Add(new StructureDataInfo()
                    {
                        position = belt.startPosition.ToByte(),
                        direction = (PlusDirection)belt.direction,
                        data = 0
                    });
                    info.data.Add(new StructureDataInfo()
                    {
                        position = (belt.startPosition + (belt.direction.ToIntVector2() * (belt.distance - 1))).ToByte(),
                        data = 0
                    });
                }
            }
            /*
            for (int i = 0; i < belts.Count; i++)
            {
                // mystman12 why not just use data to pass in length and then use -1 for buttons
                info.data.Add(new StructureDataInfo()
                {
                    position = belts[i].startPosition.ToByte(),
                    direction = (PlusDirection)belts[i].direction,
                    data = 0
                });
                info.data.Add(new StructureDataInfo()
                {
                    position = (belts[i].startPosition + (belts[i].direction.ToIntVector2() * (belts[i].distance - 1))).ToByte(),
                    data = 1
                });
                ConnectableButtonLocation[] connectedButtons = buttons.Where(x => x.index == i).ToArray();
                for (int j = 0; j < connectedButtons.Length; j++)
                {
                    info.data.Add(new StructureDataInfo()
                    {
                        data = 1,
                        position = connectedButtons[j].position.ToByte(),
                        direction = (PlusDirection)connectedButtons[j].direction,
                    });
                }
            }*/
            return info;
        }

        public override GameObject GetVisualPrefab()
        {
            return null;
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            for (int i = 0; i < belts.Count; i++)
            {
                EditorController.Instance.AddVisual(belts[i]);
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                EditorController.Instance.AddVisual(buttons[i]);
            }
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            for (int i = 0; i < belts.Count; i++)
            {
                belts[i].startPosition -= cellOffset;
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].position -= cellOffset;
            }
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            for (int i = 0; i < belts.Count; i++)
            {
                EditorController.Instance.UpdateVisual(belts[i]);
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                EditorController.Instance.UpdateVisual(buttons[i]);
            }
        }

        public void DeleteBelt(EditorLevelData data, ConveyorBeltLocation belt, bool validateAfter = true)
        {
            int index = belts.IndexOf(belt);
            if (index == -1) throw new Exception("Attempted to delete belt not in belt list!");
            Dictionary<ConveyorBeltLocation, SimpleButtonLocation> toBeltMappings = new Dictionary<ConveyorBeltLocation, SimpleButtonLocation>();
            for (int i = 0; i < belts.Count; i++)
            {
                if (belts[i].buttonIndex == -1) continue;
                toBeltMappings.Add(belts[i], buttons[belts[i].buttonIndex]);
            }
            for (int i = buttons.Count - 1; i >= 0; i--)
            {
                bool buttonOrphaned = true;
                foreach (ConveyorBeltLocation item in belts)
                {
                    if (item == belt) continue;
                    if (item.buttonIndex == i)
                    {
                        buttonOrphaned = false;
                        break;
                    }
                }
                if (buttonOrphaned)
                {
                    OnButtonDelete(data, buttons[i]);
                }
            }
            for (int i = 0; i < belts.Count; i++)
            {
                if (!toBeltMappings.ContainsKey(belts[i])) continue;
                belts[i].buttonIndex = buttons.IndexOf(toBeltMappings[belts[i]]);
            }
            EditorController.Instance.RemoveVisual(belt);
            belts.Remove(belt);
            if (validateAfter)
            {
                ValidatePosition(data);
            }
        }

        public ConveyorBeltLocation CreateBelt()
        {
            return new ConveyorBeltLocation() { owner = this };
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            for (int i = belts.Count - 1; i >= 0; i--)
            {
                if (!belts[i].ValidatePosition(data))
                {
                    DeleteBelt(data, belts[i], false);
                }
            }
            for (int i = buttons.Count - 1; i >= 0; i--)
            {
                if (!buttons[i].ValidatePosition(data, true))
                {
                    EditorController.Instance.RemoveVisual(buttons[i]);
                    buttons.RemoveAt(i);
                }
            }
            if (belts.Count == 0) return false;
            return true;
        }

        public SimpleButtonLocation CreateButton()
        {
            SimpleButtonLocation button = new SimpleButtonLocation();
            button.prefab = "button";
            button.deleteAction = OnButtonDelete;
            return button;
        }

        public bool OnButtonDelete(EditorLevelData data, SimpleLocation local)
        {
            int myIndex = buttons.IndexOf((SimpleButtonLocation)local);
            if (myIndex == -1) throw new Exception("Attempted to delete button we don't own!");
            for (int i = 0; i < belts.Count; i++)
            {
                if (belts[i].buttonIndex == myIndex)
                {
                    belts[i].buttonIndex = -1;
                }
            }
            buttons.Remove((SimpleButtonLocation)local);
            EditorController.Instance.RemoveVisual(local);
            ValidatePosition(data);
            return true;
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            byte version = reader.ReadByte();
            int beltCount = reader.ReadInt32();
            for (int i = 0; i < beltCount; i++)
            {
                ConveyorBeltLocation beltLocation = new ConveyorBeltLocation();
                beltLocation.startPosition = reader.ReadByteVector2().ToInt();
                beltLocation.distance = reader.ReadByte();
                beltLocation.direction = (Direction)reader.ReadByte();
                beltLocation.buttonIndex = reader.ReadInt32();
                belts.Add(beltLocation);
            }
            int buttonCount = reader.ReadInt32();
            for (int i = 0; i < buttonCount; i++)
            {
                SimpleButtonLocation button = CreateButton();
                button.position = reader.ReadByteVector2().ToInt();
                button.direction = (Direction)reader.ReadByte();
                buttons.Add(button);
            }
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write((byte)0);
            writer.Write(belts.Count);
            for (int i = 0; i < belts.Count; i++)
            {
                writer.Write(belts[i].startPosition.ToByte());
                writer.Write(belts[i].distance);
                writer.Write((byte)belts[i].direction);
                writer.Write(belts[i].buttonIndex);
            }
            writer.Write(buttons.Count);
            for (int i = 0; i < buttons.Count; i++)
            {
                writer.Write(buttons[i].position.ToByte());
                writer.Write((byte)buttons[i].direction);
            }
        }
    }
}
