using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public abstract class CellMarker : MarkerLocation
    {
        public IntVector2 position;
        public override void AddStringsToCompressor(StringCompressor compressor)
        {

        }

        public override void CleanupVisual(GameObject visualObject)
        {

        }

        public override GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericMarkerDisplays[type];
        }

        public override void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public override void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor)
        {
            position = reader.ReadByteVector2().ToInt();
        }

        public override void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference)
        {
            position -= cellOffset;
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position.ToWorld();
        }

        public override void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor)
        {
            writer.Write(position.ToByte());
        }
    }
}
