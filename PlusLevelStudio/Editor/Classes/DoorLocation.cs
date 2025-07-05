using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlusStudioLevelFormat;

namespace PlusLevelStudio.Editor
{
    public class DoorLocation : IEditorVisualizable, IEditorCellModifier
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
            UpdateVisual(visualObject);
        }

        public void ModifyCells(EditorLevelData data, bool forEditor)
        {
            data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls & ~direction.ToBinary());
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = EditorController.Instance.workerEc.CellFromPosition(position).CenterWorldPosition + (direction.ToVector3() * 5f);
            DoorDisplay display = visualObject.GetComponent<DoorDisplay>();
            display.UpdateSides(position, direction);
        }
    }
}
