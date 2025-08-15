using PlusLevelStudio.Editor.SettingsUI;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class SteamValveLocation : PointLocation, IEditorSettingsable
    {
        public byte strength = 7;
        public SimpleButtonLocation valve;

        public SteamValveLocation()
        {
            prefab = "steamvalve";
            valve = new SimpleButtonLocation();
            valve.deleteAction = OnButtonDelete;
            valve.prefab = "valve";
        }

        protected bool OnButtonDelete(EditorLevelData data, SimpleLocation _)
        {
            return OnDelete(data);
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            base.InitializeVisual(visualObject);
            visualObject.GetComponent<SettingsComponent>().activateSettingsOn = this;
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            if (!EditorController.Instance.GetVisual(valve)) return;
            EditorController.Instance.RemoveVisual(valve);
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            base.UpdateVisual(visualObject);
            visualObject.transform.position += Vector3.up * 10f;
            Transform radiusVis = visualObject.transform.Find("RadiusVisual");
            radiusVis.transform.position = position.ToWorld() + (Vector3.up * 0.02f);
            radiusVis.transform.localScale = Vector3.one * Mathf.Max((strength * 2),1) * 10f;
        }

        public override bool ValidatePosition(EditorLevelData data, bool ignoreSelf)
        {
            return base.ValidatePosition(data, ignoreSelf) && valve.ValidatePosition(data, ignoreSelf);
        }

        public void SettingsClicked()
        {
            EditorController.Instance.HoldUndo();
            SteamValveSettingsExchangeHandler settings = EditorController.Instance.CreateUI<SteamValveSettingsExchangeHandler>("SteamValveConfig");
            settings.myValve = this;
            settings.Refresh();
        }
    }
    public class SteamValveStructureLocation : StructureLocation
    {
        public List<SteamValveLocation> valves = new List<SteamValveLocation>();
        public byte startOnChance = 50;

        public SteamValveLocation CreateValve()
        {
            SteamValveLocation valve = new SteamValveLocation();
            valve.deleteAction = OnDeleteValve;
            return valve;
        }
        public bool OnDeleteValve(EditorLevelData data, PointLocation point, bool deleteIfInvalid)
        {
            SteamValveLocation valve = (SteamValveLocation)point;
            valves.Remove(valve);
            EditorController.Instance.RemoveVisual(valve);
            if (deleteIfInvalid)
            {
                DeleteIfInvalid(); 
            }
            return true;
        }

        public bool OnDeleteValve(EditorLevelData data, PointLocation point)
        {
            OnDeleteValve(data, point, true);
            return true;
        }

        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo info = new StructureInfo(type);
            info.data.Add(new StructureDataInfo() { data = startOnChance });
            for (int i = 0; i < valves.Count; i++)
            {
                info.data.Add(new StructureDataInfo()
                {
                    data=valves[i].strength,
                    position=valves[i].position.ToData()
                });
                info.data.Add(new StructureDataInfo()
                {
                    position=valves[i].valve.position.ToData(),
                    direction=(PlusDirection)valves[i].valve.direction
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
            for (int i = 0; i < valves.Count; i++)
            {
                EditorController.Instance.AddVisual(valves[i]);
                EditorController.Instance.AddVisual(valves[i].valve);
            }
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            for (int i = 0; i < valves.Count; i++)
            {
                valves[i].position -= cellOffset;
                valves[i].valve.position -= cellOffset;
            }
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            for (int i = 0; i < valves.Count; i++)
            {
                EditorController.Instance.UpdateVisual(valves[i]);
            }
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            for (int i = valves.Count - 1; i >= 0; i--)
            {
                if (!valves[i].ValidatePosition(data, true))
                {
                    OnDeleteValve(data, valves[i], false);
                }
            }
            return valves.Count > 0;
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            byte version = reader.ReadByte();
            if (version == 0)
            {
                startOnChance = 100;
            }
            else
            {
                startOnChance = reader.ReadByte();
            }
            int valveCount = reader.ReadInt32();
            for (int i = 0; i < valveCount; i++)
            {
                SteamValveLocation valve = CreateValve();
                valve.position = reader.ReadByteVector2().ToInt();
                valve.strength = reader.ReadByte();
                valve.valve.position = reader.ReadByteVector2().ToInt();
                valve.valve.direction = (Direction)reader.ReadByte();
                valves.Add(valve);
            }
        }

        const byte version = 1;

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(version);
            writer.Write(startOnChance);
            writer.Write(valves.Count);
            for (int i = 0; i < valves.Count; i++)
            {
                writer.Write(valves[i].position.ToByte());
                writer.Write(valves[i].strength);
                writer.Write(valves[i].valve.position.ToByte());
                writer.Write((byte)valves[i].valve.direction);
            }
        }
    }
}
