using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public abstract class StructureLocation : IEditorDeletable, IEditorCellModifier, IEditorVisualizable
    {
        public string type;
        public abstract void CleanupVisual(GameObject visualObject);

        public abstract GameObject GetVisualPrefab();

        public abstract void InitializeVisual(GameObject visualObject);

        public virtual void ModifyCells(EditorLevelData data, bool forEditor)
        {
            
        }

        public virtual bool OccupiesWall(IntVector2 pos, Direction dir)
        {
            return false;
        }

        /// <summary>
        /// Called when cells change, use this to validate your structure.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract bool ValidatePosition(EditorLevelData data);

        /// <summary>
        /// Called when your structure needs to be shifted due to a level resize.
        /// </summary>
        /// <param name="worldOffset"></param>
        /// <param name="cellOffset"></param>
        /// <param name="sizeDifference"></param>
        public abstract void ShiftBy(Vector3 worldOffset, IntVector2 cellOffset, IntVector2 sizeDifference);

        /// <summary>
        /// Write the data for your structure to be saved in the editor level file.
        /// Do not write the type, as that has already been done.
        /// </summary>
        /// <param name="writer"></param>
        public abstract void Write(BinaryWriter writer);

        /// <summary>
        /// Read the data for your structure to be loaded from the editor level file.
        /// Do not read the type, as that has already been done.
        /// </summary>
        /// <param name="reader"></param>
        public abstract void ReadInto(BinaryReader reader);

        /// <summary>
        /// Compile this structure into the StructureInfo class that will later get converted into StructureData by the level loader.
        /// </summary>
        /// <returns></returns>
        public abstract StructureInfo Compile();

        public virtual bool OnDelete(EditorLevelData data)
        {
            data.structures.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            return true;
        }

        public abstract void UpdateVisual(GameObject visualObject);
    }
}
