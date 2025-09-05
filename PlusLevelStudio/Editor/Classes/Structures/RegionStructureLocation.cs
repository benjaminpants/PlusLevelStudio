using MTM101BaldAPI;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor
{
    public class RegionLocation : IEditorVisualizable, IEditorDeletable
    {
        public RegionStructureLocation myStructure;
        public EditorRoom room;
        public int id;
        public static Dictionary<int, Texture2D> regionTextures = new Dictionary<int, Texture2D>();
        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericStructureDisplays["region"];
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            visualObject.GetComponentInChildren<MeshRenderer>().material.SetMainTexture(regionTextures[id]);
            UpdateVisual(visualObject);
        }

        public bool OnDelete(EditorLevelData data)
        {
            bool returnValue = myStructure.DeleteRegion(this);
            myStructure.DeleteIfInvalid();
            return returnValue;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            IntVector2[] positions = EditorController.Instance.levelData.GetCellsOwnedByRoom(room);
            if (positions.Length == 0) return;
            IntVector2 smallestPosition = new IntVector2(int.MaxValue, int.MaxValue);
            IntVector2 largestPosition = new IntVector2(int.MinValue,int.MinValue);
            for (int i = 0; i < positions.Length; i++)
            {
                IntVector2 pos = positions[i];
                if ((pos.x <= smallestPosition.x))
                {
                    smallestPosition.x = pos.x;
                }
                if ((pos.z <= smallestPosition.z))
                {
                    smallestPosition.z = pos.z;
                }
                if ((pos.x >= largestPosition.x))
                {
                    largestPosition.x = pos.x;
                }
                if ((pos.z >= largestPosition.z))
                {
                    largestPosition.z = pos.z;
                }
            }
            IntVector2 dif = largestPosition - smallestPosition;
            visualObject.transform.position = ((largestPosition.ToWorld() + smallestPosition.ToWorld()) / 2f) + (Vector3.up * 10f);
            visualObject.transform.localScale = new Vector3(dif.x + 1f, 1f, dif.z + 1f);
            MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
            MeshRenderer renderer = visualObject.GetComponentInChildren<MeshRenderer>();
            renderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetVector("_Tiling", new Vector4(largestPosition.x + 1, largestPosition.z + 1));
            renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }

    public class RegionStructureLocation : StructureLocation
    {
        public List<RegionLocation> regions = new List<RegionLocation>();

        public override bool ShouldUpdateVisual(PotentialStructureUpdateReason reason)
        {
            return reason == PotentialStructureUpdateReason.CellChange;
        }

        public bool DeleteRegion(RegionLocation region)
        {
            EditorController.Instance.RemoveVisual(region);
            regions.Remove(region);
            return true;
        }

        public override void AddStringsToCompressor(StringCompressor compressor)
        {
            
        }

        public override void CleanupVisual(GameObject visualObject)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                EditorController.Instance.RemoveVisual(regions[i]);
            }
        }

        public override StructureInfo Compile(EditorLevelData data, BaldiLevel level)
        {
            StructureInfo info = new StructureInfo(type);
            for (int i = 0; i < regions.Count; i++)
            {
                info.data.Add(new StructureDataInfo()
                {
                    position = new MystIntVector2(data.rooms.IndexOf(regions[i].room), regions[i].id)
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
            for (int i = 0; i < regions.Count; i++)
            {
                EditorController.Instance.AddVisual(regions[i]);
            }
        }

        public const byte version = 0;

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            for (int i = 0; i < regions.Count; i++)
            {
                if (EditorController.Instance.GetVisual(regions[i]) != null)
                {
                    EditorController.Instance.UpdateVisual(regions[i]);
                }
                else
                {
                    EditorController.Instance.AddVisual(regions[i]);
                }
            }
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            for (int i = regions.Count - 1; i >= 0; i--)
            {
                if (!data.rooms.Contains(regions[i].room)) // its room is gone
                {
                    DeleteRegion(regions[i]);
                }
            }
            return regions.Count > 0;
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            byte version = reader.ReadByte();
            int regionCount = reader.ReadInt32();
            for (int i = 0; i < regionCount; i++)
            {
                RegionLocation region = new RegionLocation() { myStructure = this };
                region.room = data.RoomFromId(reader.ReadUInt16());
                region.id = reader.ReadInt32();
                regions.Add(region);
            }
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(version);
            writer.Write(regions.Count);
            for (int i = 0; i < regions.Count; i++)
            {
                writer.Write(data.IdFromRoom(regions[i].room));
                writer.Write(regions[i].id);
            }
        }
    }
}
