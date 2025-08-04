using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class HallDoorStructureLocation : StructureLocation
    {
        public List<SimpleLocation> myChildren = new List<SimpleLocation>();
        public override void CleanupVisual(GameObject visualObject)
        {
            for (int i = 0; i < myChildren.Count; i++)
            {
                EditorController.Instance.RemoveVisual(myChildren[i]);
            }
        }

        public virtual SimpleLocation CreateNewChild() // virtual incase someone wants to inherit HallDoorStructureLocation
        {
            SimpleLocation simple = new SimpleLocation();
            simple.prefab = type;
            simple.deleteAction = OnSubDelete;
            return simple;
        }

        public virtual bool OnSubDelete(EditorLevelData data, SimpleLocation local, bool deleteSelf)
        {
            myChildren.Remove(local);
            EditorController.Instance.RemoveVisual(local);
            if (myChildren.Count == 0 && deleteSelf)
            {
                OnDelete(data);
            }
            return true;
        }

        public bool OnSubDelete(EditorLevelData data, SimpleLocation local)
        {
            return OnSubDelete(data, local, true);
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo finalStructure = new StructureInfo();
            finalStructure.type = type;
            for (int i = 0; i < myChildren.Count; i++)
            {
                finalStructure.data.Add(new StructureDataInfo()
                {
                    position = myChildren[i].position.ToByte(),
                    direction = (PlusDirection)myChildren[i].direction,
                    prefab = myChildren[i].prefab,
                });
            }
            return finalStructure;
        }

        public override GameObject GetVisualPrefab()
        {
            return null;
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            for (int i = 0; i < myChildren.Count; i++)
            {
                if (EditorController.Instance.GetVisual(myChildren[i]) != null) continue;
                EditorController.Instance.AddVisual(myChildren[i]);
            }
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            byte version = reader.ReadByte();
            int childCount = reader.ReadInt32();
            for (int i = 0; i < childCount; i++)
            {
                SimpleLocation child = CreateNewChild();
                if (version > 0)
                {
                    if (version > 1)
                    {
                        child.prefab = compressor.ReadStoredString(reader);
                    }
                    else
                    {
                        child.prefab = reader.ReadString();
                    }
                }
                child.position = reader.ReadByteVector2().ToInt();
                child.direction = (Direction)reader.ReadByte();
                myChildren.Add(child);
            }
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            for (int i = 0; i < myChildren.Count; i++)
            {
                myChildren[i].position -= cellOffset;
                //EditorController.Instance.UpdateVisual(myChildren[i]);
            }
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            for (int i = 0; i < myChildren.Count; i++)
            {
                if (EditorController.Instance.GetVisual(myChildren[i]) == null)
                {
                    EditorController.Instance.AddVisual(myChildren[i]);
                    continue;
                }    
                EditorController.Instance.UpdateVisual(myChildren[i]);
            }
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            for (int i = myChildren.Count - 1; i >= 0; i--)
            {
                if (!myChildren[i].ValidatePosition(data, true))
                {
                    OnSubDelete(data, myChildren[i], false);
                }
            }
            return myChildren.Count > 0;
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write((byte)2); // incase i change this in the future
            writer.Write(myChildren.Count);
            for (int i = 0; i < myChildren.Count; i++)
            {
                compressor.WriteStoredString(writer, myChildren[i].prefab);
                writer.Write(myChildren[i].position.ToByte());
                writer.Write((byte)myChildren[i].direction);
            }
        }

        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            compressor.AddStrings(myChildren.Select(x => x.prefab));
        }
    }

    public class HallDoorStructureLocationWithLevers : HallDoorStructureLocationWithButtons
    {
        public override string buttonPrefab => "lever";

        public override SimpleButtonLocation CreateNewButton()
        {
            SimpleLeverLocation simple = new SimpleLeverLocation();
            simple.prefab = buttonPrefab;
            simple.deleteAction = OnButtonDelete;
            simple.shouldBeDown = ShouldLeverBeDown;
            return simple;
        }

        public virtual bool ShouldLeverBeDown(SimpleLeverLocation lever)
        {
            return false;
        }
    }

    public class HallDoorStructureLocationWithButtons : HallDoorStructureLocation
    {
        public List<SimpleLocation> buttons = new List<SimpleLocation>();
        public virtual string buttonPrefab => "button";

        public virtual SimpleButtonLocation CreateNewButton()
        {
            SimpleButtonLocation simple = new SimpleButtonLocation();
            simple.prefab = buttonPrefab;
            simple.deleteAction = OnButtonDelete;
            return simple;
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            base.CleanupVisual(visualObject);
            for (int i = 0; i < buttons.Count; i++)
            {
                EditorController.Instance.RemoveVisual(buttons[i]);
            }
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo finalStructure = new StructureInfo();
            finalStructure.type = type;
            for (int i = 0; i < myChildren.Count; i++)
            {
                finalStructure.data.Add(new StructureDataInfo()
                {
                    position = myChildren[i].position.ToByte(),
                    direction = (PlusDirection)myChildren[i].direction,
                    prefab = myChildren[i].prefab,
                });
                // this is how we establish a button/lever
                finalStructure.data.Add(new StructureDataInfo()
                {
                    position = buttons[i].position.ToByte(),
                    direction = (PlusDirection)buttons[i].direction,
                    data = 1,
                });
            }
            return finalStructure;
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            base.InitializeVisual(visualObject);
            for (int i = 0; i < buttons.Count; i++)
            {
                EditorController.Instance.AddVisual(buttons[i]);
            }
        }

        public override bool OccupiesWall(IntVector2 pos, Direction dir)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i].position == pos && buttons[i].direction == dir)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            bool initValue = base.ValidatePosition(data);
            for (int i = buttons.Count - 1; i >= 0; i--)
            {
                if (!buttons[i].ValidatePosition(data, true))
                {
                    OnButtonDelete(data, buttons[i], false);
                }
            }
            return initValue && myChildren.Count > 0;
        }

        // we just call the delete method of our associated object since that will delete us as well
        public virtual bool OnButtonDelete(EditorLevelData data, SimpleLocation local, bool deleteSelf)
        {
            int associatedIndex = buttons.IndexOf(local);
            return OnSubDelete(data, myChildren[associatedIndex], deleteSelf);
        }

        public bool OnButtonDelete(EditorLevelData data, SimpleLocation local)
        {
            return OnButtonDelete(data, local, false);
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            base.ShiftBy(worldOffset, cellOffset, sizeDifference);
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].position -= cellOffset;
            }
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            base.UpdateVisual(visualObject);
            for (int i = 0; i < buttons.Count; i++)
            {
                if (EditorController.Instance.GetVisual(buttons[i]) == null)
                {
                    EditorController.Instance.AddVisual(buttons[i]);
                }
                EditorController.Instance.UpdateVisual(buttons[i]);
            }
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            base.Write(data, writer, compressor);
            writer.Write((byte)1);
            writer.Write(buttons.Count);
            for (int i = 0; i < buttons.Count; i++)
            {
                compressor.WriteStoredString(writer, buttons[i].prefab);
                writer.Write(buttons[i].position.ToByte());
                writer.Write((byte)buttons[i].direction);
            }
        }
        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            base.ReadInto(data, reader, compressor);
            byte version = reader.ReadByte();
            int buttonCount = reader.ReadInt32();
            for (int i = 0; i < buttonCount; i++)
            {
                SimpleLocation button = CreateNewButton();
                if (version > 0)
                {
                    button.prefab = compressor.ReadStoredString(reader);
                }
                button.position = reader.ReadByteVector2().ToInt();
                button.direction = (Direction)reader.ReadByte();
                buttons.Add(button);
            }
        }

        public override bool OnSubDelete(EditorLevelData data, SimpleLocation local, bool deleteSelf)
        {
            int oldIndex = myChildren.IndexOf(local);
            bool val = base.OnSubDelete(data, local, deleteSelf);
            EditorController.Instance.RemoveVisual(buttons[oldIndex]);
            buttons.RemoveAt(oldIndex);
            return val;
        }

        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            base.AddStringsToCompressor(compressor);
            compressor.AddStrings(buttons.Select(x => x.prefab));
        }
    }
}
