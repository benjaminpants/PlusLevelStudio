using MTM101BaldAPI;
using PlusLevelStudio.Editor.SettingsUI;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class BreakerLocation : SimpleButtonLocation, IEditorSettingsable
    {
        public override void InitializeVisual(GameObject visualObject)
        {
            base.InitializeVisual(visualObject);
            SettingsComponent settings = visualObject.GetComponent<SettingsComponent>();
            settings.activateSettingsOn = this;
        }
        public void SettingsClicked()
        {
            EditorController.Instance.HoldUndo();
            EditorController.Instance.CreateUI<PowerLeverSettingsExchangeHandler>("PowerLeverBreakerConfig").Refresh();
        }
    }

    public class PowerLeverLocation : SimpleButtonLocation, IEditorSettingsable
    {
        public static Dictionary<CableColor, Texture2D> cableTex = new Dictionary<CableColor, Texture2D>();
        public CableColor color;
        public EditorRoom room;

        public override void InitializeVisual(GameObject visualObject)
        {
            base.InitializeVisual(visualObject);
            SettingsComponent settings = visualObject.GetComponent<SettingsComponent>();
            settings.activateSettingsOn = this;
        }

        public override bool ValidatePosition(EditorLevelData data, bool ignoreSelf)
        {
            if (!base.ValidatePosition(data, ignoreSelf)) return false;
            return data.rooms.Contains(room);
        }

        public void SettingsClicked()
        {
            EditorController.Instance.HoldUndo();
            EditorController.Instance.CreateUI<PowerLeverSettingsExchangeHandler>("PowerLeverConfig").Refresh();
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            base.UpdateVisual(visualObject);

            if (room != null)
            {
                visualObject.transform.Find("LineRenderer").gameObject.SetActive(true);
                Vector3[] allPositions = EditorController.Instance.levelData.GetCellsOwnedByRoom(room).Select(x => x.ToWorld()).ToArray();
                Vector3 centralPosition = allPositions[0];
                for (int i = 1; i < allPositions.Length; i++)
                {
                    centralPosition = (centralPosition + allPositions[i]);
                }
                centralPosition /= allPositions.Length;

                LineRenderer lineRenderer = visualObject.transform.Find("LineRenderer").GetComponent<LineRenderer>();
                lineRenderer.SetPositions(new Vector3[] { visualObject.transform.Find("Wall").position + Vector3.up * 7f, centralPosition + Vector3.up * 12 });
                lineRenderer.material.SetMainTexture(cableTex[color]);

            }
            else
            {
                visualObject.transform.Find("LineRenderer").gameObject.SetActive(false);
            }

            StructureLocation structure = EditorController.Instance.GetStructureData("powerlever");
            if (structure == null)
            {
                visualObject.transform.Find("PowerLeverGauge").gameObject.SetActive(false);
                return;
            }
            PowerLeverStructureLocation powerLeverStructure = (PowerLeverStructureLocation)structure; 
            visualObject.transform.Find("PowerLeverGauge").gameObject.SetActive(powerLeverStructure.breakers.Count > 0);
        }
    }

    public class PowerLeverStructureLocation : StructureLocation
    {
        public List<PointLocation> alarmLights = new List<PointLocation>();
        public List<PowerLeverLocation> powerLevers = new List<PowerLeverLocation>();
        public List<BreakerLocation> breakers = new List<BreakerLocation>();
        public int maxLevers = 3;
        public int poweredRoomChance = 10;

        public override bool ShouldUpdateVisual(PotentialStructureUpdateReason reason)
        {
            return reason == PotentialStructureUpdateReason.CellChange;
        }

        public override bool OccupiesWall(IntVector2 pos, Direction dir)
        {
            for (int i = 0; i < powerLevers.Count; i++)
            {
                if (powerLevers[i].position == pos && powerLevers[i].direction == dir) return true;
            }
            for (int i = 0; i < breakers.Count; i++)
            {
                if (breakers[i].position == pos && breakers[i].direction == dir) return true;
            }
            return false;
        }

        public PointLocation CreateAlarmLight()
        {
            return new PointLocation()
            {
                prefab = "powerlever_alarm",
                deleteAction = DeleteAlarmLight
            };
        }

        public BreakerLocation CreateBreaker()
        {
            return new BreakerLocation()
            {
                prefab = "powerlever_breaker",
                deleteAction = DeleteBreaker
            };
        }

        public PowerLeverLocation CreatePowerLever()
        {
            return new PowerLeverLocation()
            {
                prefab = "powerlever_lever",
                deleteAction=DeleteLever
            };
        }

        public bool DeleteBreaker(EditorLevelData data, SimpleLocation local, bool validatePosition)
        {
            EditorController.Instance.RemoveVisual(local);
            breakers.Remove((BreakerLocation)local);
            for (int i = 0; i < powerLevers.Count; i++)
            {
                EditorController.Instance.UpdateVisual(powerLevers[i]);
            }
            if (validatePosition)
            {
                DeleteIfInvalid();
            }
            return true;
        }

        public bool DeleteBreaker(EditorLevelData data, SimpleLocation local)
        {
            return DeleteBreaker(data, local, true);
        }

        public bool DeleteLever(EditorLevelData data, SimpleLocation local)
        {
            return DeleteLever(data, local, true);
        }

        public bool DeleteLever(EditorLevelData data, SimpleLocation local, bool validatePosition)
        {
            EditorController.Instance.RemoveVisual(local);
            powerLevers.Remove((PowerLeverLocation)local);
            if (validatePosition)
            {
                DeleteIfInvalid();
            }
            return true;
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
            for (int i = 0; i < powerLevers.Count; i++)
            {
                EditorController.Instance.RemoveVisual(powerLevers[i]);
            }
            for (int i = 0; i < breakers.Count; i++)
            {
                EditorController.Instance.RemoveVisual(breakers[i]);
            }
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo info = new StructureInfo(type);
            info.data.Add(new StructureDataInfo()
            {
                data=maxLevers
            });
            info.data.Add(new StructureDataInfo()
            {
                data = poweredRoomChance
            });
            for (int i = 0; i < alarmLights.Count; i++)
            {
                info.data.Add(new StructureDataInfo()
                {
                    data = 0,
                    position = alarmLights[i].position.ToByte()
                });
            }
            for (int i = 0; i < powerLevers.Count; i++)
            {
                info.data.Add(new StructureDataInfo()
                {
                    data = 1,
                    position = powerLevers[i].position.ToByte(),
                    direction = (PlusDirection)powerLevers[i].direction
                });
                info.data.Add(new StructureDataInfo()
                {
                    data = (int)powerLevers[i].color,
                });
                info.data.Add(new StructureDataInfo()
                {
                    data = data.rooms.IndexOf(powerLevers[i].room),
                });
            }
            for (int i = 0; i < breakers.Count; i++)
            {
                info.data.Add(new StructureDataInfo()
                {
                    data = 2,
                    position = breakers[i].position.ToByte(),
                    direction = (PlusDirection)breakers[i].direction
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
            for (int i = 0; i < powerLevers.Count; i++)
            {
                EditorController.Instance.AddVisual(powerLevers[i]);
            }
            for (int i = 0; i < breakers.Count; i++)
            {
                EditorController.Instance.AddVisual(breakers[i]);
            }
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            byte version = reader.ReadByte();
            if (version >= 2)
            {
                maxLevers = reader.ReadInt32();
                poweredRoomChance = reader.ReadInt32();
            }
            int alarmLightCount = reader.ReadInt32();
            for (int i = 0; i < alarmLightCount; i++)
            {
                PointLocation alarmLight = CreateAlarmLight();
                alarmLight.position = reader.ReadByteVector2().ToInt();
                alarmLights.Add(alarmLight);
            }
            int powerLeverCount = reader.ReadInt32();
            for (int i = 0; i < powerLeverCount; i++)
            {
                PowerLeverLocation powerLever = CreatePowerLever();
                powerLever.position = reader.ReadByteVector2().ToInt();
                powerLever.direction = (Direction)reader.ReadByte();
                powerLever.color = (CableColor)reader.ReadByte();
                powerLever.room = data.RoomFromId(reader.ReadUInt16());
                powerLevers.Add(powerLever);
            }
            if (version == 0) return;
            int breakerCount = reader.ReadInt32();
            for (int i = 0; i < breakerCount; i++)
            {
                BreakerLocation breaker = CreateBreaker();
                breaker.position = reader.ReadByteVector2().ToInt();
                breaker.direction = (Direction)reader.ReadByte();
                breakers.Add(breaker);
            }
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            for (int i = 0; i < alarmLights.Count; i++)
            {
                alarmLights[i].position -= cellOffset;
            }
            for (int i = 0; i < powerLevers.Count; i++)
            {
                powerLevers[i].position -= cellOffset;
            }
            for (int i = 0; i < breakers.Count; i++)
            {
                breakers[i].position -= cellOffset;
            }
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            for (int i = 0; i < alarmLights.Count; i++)
            {
                EditorController.Instance.UpdateVisual(alarmLights[i]);
            }
            for (int i = 0; i < powerLevers.Count; i++)
            {
                EditorController.Instance.UpdateVisual(powerLevers[i]);
            }
            for (int i = 0; i < breakers.Count; i++)
            {
                EditorController.Instance.UpdateVisual(breakers[i]);
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
            for (int i = (powerLevers.Count - 1); i >= 0; i--)
            {
                if (!powerLevers[i].ValidatePosition(data, true))
                {
                    DeleteLever(data, powerLevers[i], false);
                }
            }
            for (int i = (breakers.Count - 1); i >= 0; i--)
            {
                if (!breakers[i].ValidatePosition(data, true))
                {
                    DeleteBreaker(data, breakers[i], false);
                }
            }
            return (alarmLights.Count > 0) || (powerLevers.Count > 0) || (breakers.Count > 0);
        }

        const byte version = 2;

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(version);
            writer.Write(maxLevers);
            writer.Write(poweredRoomChance);
            writer.Write(alarmLights.Count);
            for (int i = 0; i < alarmLights.Count; i++)
            {
                writer.Write(alarmLights[i].position.ToByte());
            }
            writer.Write(powerLevers.Count);
            for (int i = 0; i < powerLevers.Count; i++)
            {
                writer.Write(powerLevers[i].position.ToByte());
                writer.Write((byte)powerLevers[i].direction);
                writer.Write((byte)powerLevers[i].color);
                writer.Write(data.IdFromRoom(powerLevers[i].room));
            }
            writer.Write(breakers.Count);
            for (int i = 0; i < breakers.Count; i++)
            {
                writer.Write(breakers[i].position.ToByte());
                writer.Write((byte)breakers[i].direction);
            }
        }
    }
}
