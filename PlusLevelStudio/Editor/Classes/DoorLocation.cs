using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlusStudioLevelFormat;

namespace PlusLevelStudio.Editor
{
    public class DoorLocation : IEditorVisualizable, IEditorCellModifier, IEditorDeletable
    {
        public string type;
        public IntVector2 position;
        public Direction direction;

        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.doorDisplays[type].gameObject;
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public void ModifyCells(EditorLevelData data, bool forEditor)
        {
            data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls & ~direction.ToBinary());
            IntVector2 pos2 = direction.ToIntVector2();
            data.cells[position.x + pos2.x, position.z + pos2.z].walls = (Nybble)(data.cells[position.x + pos2.x, position.z + pos2.z].walls & ~direction.GetOpposite().ToBinary());
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.doors.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            EditorController.Instance.RefreshCells();
            EditorController.Instance.RefreshLights();
            return true;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = EditorController.Instance.workerEc.CellFromPosition(position).CenterWorldPosition + (direction.ToVector3() * 5f);
            visualObject.transform.rotation = direction.ToRotation();
            DoorDisplay display = visualObject.GetComponent<DoorDisplay>();
            display.UpdateSides(position, direction);
        }
    }
}
