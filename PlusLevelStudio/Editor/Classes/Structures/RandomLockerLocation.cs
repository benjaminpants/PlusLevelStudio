using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class RandomLockerLocation : RandomStructureLocation
    {
        public float cyanLockerChance = 0.05f;
        public MystIntVector2 minMaxLockerCount = new MystIntVector2(8, 12);
        public MystIntVector2 minMaxHallCount = new MystIntVector2(10, 20);

        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            
        }

        public override RandomStructureInfo CompileIntoRandom(EditorLevelData data, BaldiLevel level)
        {
            return new RandomStructureInfo()
            {
                type = type,
                info = new StructureParameterInfo()
                {
                    chance = new List<float>()
                    {
                        cyanLockerChance
                    },
                    minMax = new List<MystIntVector2>()
                    {
                        minMaxLockerCount,
                        minMaxHallCount
                    }
                }
            };
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
            cyanLockerChance = reader.ReadSingle();
            minMaxLockerCount = reader.ReadMystIntVector2();
            minMaxHallCount = reader.ReadMystIntVector2();
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
            writer.Write(cyanLockerChance);
            writer.Write(minMaxLockerCount);
            writer.Write(minMaxHallCount);
        }
    }
}
