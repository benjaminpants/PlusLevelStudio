using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class RotohallVisual : MonoBehaviour
    {
        public MeshRenderer cylinder;
    }

    public class RotohallSimpleLocation : SimpleLocation
    {
        public bool clockwise = true;
        public SimpleButtonLocation button;

        public SimpleButtonLocation SetButton(IntVector2 position, Direction direction)
        {
            button = new SimpleButtonLocation();
            button.position = position;
            button.direction = direction;
            button.prefab = "button";
            button.deleteAction = DeleteButton;
            return button;
        }

        public bool DeleteButton(EditorLevelData data, SimpleLocation _)
        {
            return deleteAction(data, this);
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            base.CleanupVisual(visualObject);
            if (button == null) return;
            EditorController.Instance.RemoveVisual(button);
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            base.InitializeVisual(visualObject);
            // dont init button visual here
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            base.UpdateVisual(visualObject);
            visualObject.transform.rotation = Quaternion.identity;
            visualObject.GetComponent<RotohallVisual>().cylinder.transform.rotation = direction.ToRotation();
            if (button == null) return;
            EditorController.Instance.UpdateVisual(button);
        }

        public override bool ValidatePosition(EditorLevelData data, bool ignoreSelf)
        {
            bool baseValue = base.ValidatePosition(data, ignoreSelf);
            if (button == null) return baseValue;
            return baseValue && button.ValidatePosition(data, ignoreSelf);
        }
    }

    public class RotohallStructureLocation : StructureLocation
    {
        public List<RotohallSimpleLocation> rotohalls = new List<RotohallSimpleLocation>();
        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            for (int i = 0; i < rotohalls.Count; i++)
            {
                compressor.AddString(rotohalls[i].prefab);
            }
        }

        const byte version = 0;

        public override void CleanupVisual(GameObject visualObject)
        {
            for (int i = 0; i < rotohalls.Count; i++)
            {
                EditorController.Instance.RemoveVisual(rotohalls[i]);
            }
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo info = new StructureInfo(type);
            for (int i = 0; i < rotohalls.Count; i++)
            {
                info.data.Add(new StructureDataInfo()
                {
                    position = rotohalls[i].position.ToData(),
                    direction = (PlusDirection)rotohalls[i].direction,
                    data = rotohalls[i].clockwise ? 0 : 1,
                    prefab = rotohalls[i].prefab,
                });
                info.data.Add(new StructureDataInfo()
                {
                    position = rotohalls[i].button.position.ToData(),
                    direction = (PlusDirection)rotohalls[i].button.direction,
                    data = 2 // completely ignored by the structureBuilder and not technically necessary
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
            for (int i = 0; i < rotohalls.Count; i++)
            {
                EditorController.Instance.AddVisual(rotohalls[i].button);
                EditorController.Instance.AddVisual(rotohalls[i]);
            }
        }

        public bool DeleteRotohall(EditorLevelData data, SimpleLocation roto)
        {
            rotohalls.Remove((RotohallSimpleLocation)roto);
            EditorController.Instance.RemoveVisual(roto);
            DeleteIfInvalid();
            return true;
        }

        public RotohallSimpleLocation CreateRotohall(string prefab, IntVector2 position, Direction dir, bool clockwise)
        {
            RotohallSimpleLocation roto = new RotohallSimpleLocation()
            {
                position = position,
                direction = dir,
                prefab = prefab,
                deleteAction = DeleteRotohall,
                clockwise = clockwise
            };
            return roto;
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            byte version = reader.ReadByte();
            int rotoHallCount = reader.ReadInt32();
            for (int i = 0; i < rotoHallCount; i++)
            {
                RotohallSimpleLocation roto = CreateRotohall(compressor.ReadStoredString(reader), reader.ReadByteVector2().ToInt(), (Direction)reader.ReadByte(), reader.ReadBoolean());
                roto.SetButton(reader.ReadByteVector2().ToInt(), (Direction)reader.ReadByte());
                rotohalls.Add(roto);
            }
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            for (int i = 0; i < rotohalls.Count; i++)
            {
                rotohalls[i].position -= cellOffset;
            }
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            for (int i = 0; i < rotohalls.Count; i++)
            {
                EditorController.Instance.UpdateVisual(rotohalls[i]);
            }
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            for (int i = (rotohalls.Count - 1); i >= 0; i--)
            {
                if (!rotohalls[i].ValidatePosition(data, true))
                {
                    EditorController.Instance.RemoveVisual(rotohalls[i]);
                    rotohalls.RemoveAt(i);
                }
            }
            return (rotohalls.Count > 0);
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(version);
            writer.Write(rotohalls.Count);
            for (int i = 0; i < rotohalls.Count; i++)
            {
                compressor.WriteStoredString(writer, rotohalls[i].prefab);
                writer.Write(rotohalls[i].position.ToByte());
                writer.Write((byte)rotohalls[i].direction);
                writer.Write(rotohalls[i].clockwise);
                writer.Write(rotohalls[i].button.position.ToByte());
                writer.Write((byte)rotohalls[i].button.direction);
            }
        }
    }
}
