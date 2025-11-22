using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class StudentSpawnerStructureLocation : RandomStructureLocation
    {
        public ushort minStudents = 4;
        public ushort maxStudents = 8;

        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override RandomStructureInfo CompileIntoRandom(EditorLevelData data, BaldiLevel level)
        {
            RandomStructureInfo info = new RandomStructureInfo(type);
            info.info.minMax.Add(new MystIntVector2(minStudents, maxStudents));
            return info;
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            
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
            byte version = reader.ReadByte();
            minStudents = reader.ReadUInt16();
            maxStudents = reader.ReadUInt16();
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

        const byte version = 0;
        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(version);
            writer.Write(minStudents);
            writer.Write(maxStudents);
        }
    }
}
