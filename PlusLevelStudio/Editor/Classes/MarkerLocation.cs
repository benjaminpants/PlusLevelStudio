using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public abstract class MarkerLocation : IEditorDeletable, IEditorVisualizable, IEditorCellModifier, IEditorPositionVerifyable
    {
        public string type;
        public abstract void CleanupVisual(GameObject visualObject);
        public abstract GameObject GetVisualPrefab();
        public abstract void InitializeVisual(GameObject visualObject);

        public virtual bool OnDelete(EditorLevelData data)
        {
            data.markers.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            return true;
        }

        /// <summary>
        /// Called before writing, passes in a string compressor you can add your structures strings to reduce file size.
        /// Be sure to use the appropiate methods from the compressor in your reader and writer.
        /// </summary>
        /// <param name="compressor"></param>
        public abstract void AddStringsToCompressor(StringCompressor compressor);

        /// <summary>
        /// Compiles this marker. Don't change anything in data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="compiled"></param>
        public abstract void Compile(EditorLevelData data, BaldiLevel compiled);

        /// <summary>
        /// Write the data for the marker to be saved in the editor level file.
        /// Do not write the type, as that has already been done.
        /// </summary>
        /// <param name="writer"></param>
        public abstract void Write(EditorLevelData data, BinaryWriter writer, StringCompressor compressor);

        /// <summary>
        /// Read the data for the marker to be loaded from the editor level file.
        /// Do not read the type, as that has already been done.
        /// </summary>
        /// <param name="reader"></param>
        public abstract void ReadInto(EditorLevelData data, BinaryReader reader, StringCompressor compressor);

        /// <summary>
        /// Called when your marker needs to be shifted due to a level resize.
        /// </summary>
        /// <param name="worldOffset"></param>
        /// <param name="cellOffset"></param>
        /// <param name="sizeDifference"></param>
        public abstract void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference);

        public abstract void UpdateVisual(GameObject visualObject);

        public abstract bool ValidatePosition(EditorLevelData data);

        public virtual void ModifyLightsForEditor(EnvironmentController workerEc)
        {

        }

        public void ModifyCells(EditorLevelData data, bool forEditor)
        {
            throw new NotImplementedException();
        }
    }
}
