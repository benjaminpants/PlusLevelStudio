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

        public virtual GameObject GetVisualPrefab()
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
            IntVector2 pos2;
            if (!forEditor && !LevelStudioPlugin.Instance.doorIsTileBased[type])
            {
                data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls | direction.ToBinary());
                pos2 = direction.ToIntVector2();
                data.cells[position.x + pos2.x, position.z + pos2.z].walls = (Nybble)(data.cells[position.x + pos2.x, position.z + pos2.z].walls | direction.GetOpposite().ToBinary());
                return;
            }    
            data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls & ~direction.ToBinary());
            pos2 = direction.ToIntVector2();
            data.cells[position.x + pos2.x, position.z + pos2.z].walls = (Nybble)(data.cells[position.x + pos2.x, position.z + pos2.z].walls & ~direction.GetOpposite().ToBinary());
        }

        public virtual bool OnDelete(EditorLevelData data)
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

        public bool ValidatePosition(EditorLevelData data)
        {
            // dont allow stacking
            for (int i = 0; i < data.doors.Count; i++)
            {
                if (data.doors[i] == this) continue; // this is us
                if (data.doors[i].position == position && data.doors[i].direction == direction) return false; // door is clashing
                if (data.doors[i].position == (position + direction.ToIntVector2()) && data.doors[i].direction == direction.GetOpposite()) return false; // door is clashing
            }
            // dont allow doors ontop of windows (or vise versa)
            for (int i = 0; i < data.windows.Count; i++)
            {
                if (data.windows[i] == this) continue; // this is us
                if (data.windows[i].position == position && data.windows[i].direction == direction) return false; // window is clashing
                if (data.windows[i].position == (position + direction.ToIntVector2()) && data.windows[i].direction == direction.GetOpposite()) return false; // window is clashing
            }
            return (data.RoomIdFromPos(position, true) != 0 && data.RoomIdFromPos(position + direction.ToIntVector2(), true) != 0); // make sure both rooms we are facing are valid
        }
    }
}
