using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class PowerLeverStructureLocation : StructureLocation
    {
        public List<PointLocation> alarmLights = new List<PointLocation>();

        public PointLocation CreateAlarmLight()
        {
            return new PointLocation()
            {
                prefab = "powerlever_alarm",
                deleteAction = DeleteAlarmLight

            };
        }

        public bool DeleteAlarmLight(EditorLevelData data, PointLocation local, bool validatePosition)
        {
            EditorController.Instance.RemoveVisual(local);
            alarmLights.Remove(local);
            if (validatePosition)
            {
                DeleteIfInvalid();
            }
            return true;
        }

        public bool DeleteAlarmLight(EditorLevelData data, PointLocation local)
        {
            return DeleteAlarmLight(data, local, true);
        }

        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            for (int i = 0; i < alarmLights.Count; i++)
            {
                EditorController.Instance.RemoveVisual(alarmLights[i]);
            }
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo info = new StructureInfo(type);
            for (int i = 0; i < alarmLights.Count; i++)
            {
                info.data.Add(new StructureDataInfo()
                {
                    data = 0,
                    position = alarmLights[i].position.ToByte()
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
            for (int i = 0; i < alarmLights.Count; i++)
            {
                EditorController.Instance.AddVisual(alarmLights[i]);
            }
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            byte version = reader.ReadByte();
            int alarmLightCount = reader.ReadInt32();
            for (int i = 0; i < alarmLightCount; i++)
            {
                PointLocation alarmLight = CreateAlarmLight();
                alarmLight.position = reader.ReadByteVector2().ToInt();
                alarmLights.Add(alarmLight);
            }
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            for (int i = 0; i < alarmLights.Count; i++)
            {
                alarmLights[i].position -= cellOffset;
            }
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            for (int i = 0; i < alarmLights.Count; i++)
            {
                EditorController.Instance.UpdateVisual(alarmLights[i]);
            }
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            for (int i = (alarmLights.Count - 1); i >= 0; i--)
            {
                if (!alarmLights[i].ValidatePosition(data, true))
                {
                    DeleteAlarmLight(data, alarmLights[i], false);
                }
            }
            return alarmLights.Count > 0;
        }

        const byte version = 0;

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(version);
            writer.Write(alarmLights.Count);
            for (int i = 0; i < alarmLights.Count; i++)
            {
                writer.Write(alarmLights[i].position.ToByte());
            }
        }
    }
}
