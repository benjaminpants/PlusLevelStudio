using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    /// <summary>
    /// An abstract "structure" that will only work correctly in the "room" editor mode.
    /// </summary>
    public abstract class RoomTechnicalStructureBase : StructureLocation
    {
        public abstract bool CaresAboutRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset);

        public abstract void CompileIntoRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset);

        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            return new StructureInfo("invalid");
        }
    }

    public abstract class RoomTechnicalStructurePoint : RoomTechnicalStructureBase
    {
        public IntVector2 position;

        public override bool CaresAboutRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset)
        {
            for (int i = 0; i < asset.cells.Count; i++)
            {
                if ((asset.cells[i].position.ToInt()) == (position - offset))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            return data.RoomIdFromPos(position, true) != 0;
        }

        public override GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericStructureDisplays[type];
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            position -= cellOffset;
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position.ToWorld() + Vector3.up * 11f;
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            position = reader.ReadByteVector2().ToInt();
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(position.ToByte());
        }
    }
}
