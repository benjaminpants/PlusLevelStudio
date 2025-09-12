using System;
using System.IO;
using UnityEngine;
using PlusStudioLevelFormat;

namespace PlusLevelStudio.Editor
{
    public class LegacyRoomTechnicalStructure : StructureLocation
    {
        public IntVector2 position;
        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            throw new NotImplementedException();
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            throw new NotImplementedException();
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            throw new NotImplementedException();
        }

        public override GameObject GetVisualPrefab()
        {
            throw new NotImplementedException();
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            throw new NotImplementedException();
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            position = reader.ReadByteVector2().ToInt();
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            throw new NotImplementedException();
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            throw new NotImplementedException();
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            throw new NotImplementedException();
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            throw new NotImplementedException();
        }
    }
}
