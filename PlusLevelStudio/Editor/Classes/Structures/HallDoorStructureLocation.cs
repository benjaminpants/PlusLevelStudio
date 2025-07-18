using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class SimpleLocation : IEditorVisualizable, IEditorDeletable
    {
        public HallDoorStructureLocation owner;
        public string prefab;
        public IntVector2 position;
        public Direction direction;
        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public virtual GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericStructureDisplays[prefab];
        }

        public virtual bool ValidatePosition(EditorLevelData data)
        {
            PlusStudioLevelFormat.Cell cell = data.GetCellSafe(position);
            if (cell == null) return false; // cell doesn't exist
            if (cell.type == 16) return false; // the cell is empty
            return true;
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public bool OnDelete(EditorLevelData data)
        {
            return owner.OnSubDelete(data, this, true);
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position.ToWorld();
            visualObject.transform.rotation = direction.ToRotation();
        }
    }

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
            simple.owner = this;
            return simple;
        }

        public bool OnSubDelete(EditorLevelData data, SimpleLocation local, bool deleteSelf)
        {
            myChildren.Remove(local);
            EditorController.Instance.RemoveVisual(local);
            if (myChildren.Count == 0 && deleteSelf)
            {
                OnDelete(data);
            }
            return true;
        }

        public override StructureInfo Compile()
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
                EditorController.Instance.AddVisual(myChildren[i]);
            }
        }

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            int childCount = reader.ReadInt32();
            for (int i = 0; i < childCount; i++)
            {
                SimpleLocation child = CreateNewChild();
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
                if (!myChildren[i].ValidatePosition(data))
                {
                    OnSubDelete(data, myChildren[i], false);
                }
            }
            return myChildren.Count > 0;
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte)0); // incase i change this in the future
            writer.Write(myChildren.Count);
            for (int i = 0; i < myChildren.Count; i++)
            {
                writer.Write(myChildren[i].position.ToByte());
                writer.Write((byte)myChildren[i].direction);
            }
        }
    }
}
