using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class ConnectableButtonLocation : SimpleButtonLocation
    {
        public int index;
    }

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
        public List<ConnectableButtonLocation> buttons = new List<ConnectableButtonLocation>();
        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            
        }

        // how the fuck does this annoying shit work
        // NOTE: so this is completely wrong. why did i set it up like this. its multiple BELTS per BUTTON. not BUTTONS per BELT.
        // the buttons are coded like they are for multiple BUTTONS per BELT when it really needed to be the belts with the button index.
        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo info = new StructureInfo(type);
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
            }
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
            Dictionary<ConnectableButtonLocation, ConveyorBeltLocation> toBeltMappings = new Dictionary<ConnectableButtonLocation, ConveyorBeltLocation>();
            for (int i = 0; i < buttons.Count; i++)
            {
                toBeltMappings.Add(buttons[i], belts[buttons[i].index]);
            }
            for (int i = buttons.Count - 1; i >= 0; i--)
            {
                if (buttons[i].index == index)
                {
                    OnButtonDelete(data, buttons[i]);
                }
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].index = belts.IndexOf(toBeltMappings[buttons[i]]);
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
            for (int i = buttons.Count - 1; i >= 0; i--)
            {
                if (!buttons[i].ValidatePosition(data, true))
                {
                    EditorController.Instance.RemoveVisual(buttons[i]);
                    buttons.RemoveAt(i);
                }
            }
            for (int i = belts.Count - 1; i >= 0; i--)
            {
                if (!belts[i].ValidatePosition(data))
                {
                    DeleteBelt(data, belts[i], false);
                }
            }
            if (belts.Count == 0) return false;
            return true;
        }

        public ConnectableButtonLocation CreateButton()
        {
            ConnectableButtonLocation button = new ConnectableButtonLocation();
            button.prefab = "button";
            button.deleteAction = OnButtonDelete;
            return button;
        }

        public bool OnButtonDelete(EditorLevelData data, SimpleLocation local)
        {
            buttons.Remove((ConnectableButtonLocation)local);
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
                belts.Add(beltLocation);
            }
            int buttonCount = reader.ReadInt32();
            for (int i = 0; i < buttonCount; i++)
            {
                ConnectableButtonLocation button = CreateButton();
                button.position = reader.ReadByteVector2().ToInt();
                button.direction = (Direction)reader.ReadByte();
                button.index = reader.ReadInt32();
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
            }
            writer.Write(buttons.Count);
            for (int i = 0; i < buttons.Count; i++)
            {
                writer.Write(buttons[i].position.ToByte());
                writer.Write((byte)buttons[i].direction);
                writer.Write(buttons[i].index);
            }
        }
    }
}
