using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class DummyRandomStructureLocation : RandomStructureLocation
    {
        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            
        }

        public override RandomStructureInfo CompileIntoRandom(EditorLevelData data, BaldiLevel level)
        {
            return new RandomStructureInfo() { type = type };
        }

        public override GameObject GetVisualPrefab()
        {
            return null;
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            reader.ReadByte();
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            return true;
        }
        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write((byte)0);
        }
    }
}
