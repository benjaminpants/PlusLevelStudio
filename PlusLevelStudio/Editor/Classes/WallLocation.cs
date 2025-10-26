using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class WallLocation : IEditorVisualizable, IEditorCellModifier, IEditorDeletable, IEditorPositionVerifyable
    {
        public bool wallState;
        public IntVector2 position;
        public Direction direction;
        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            return wallState ? LevelStudioPlugin.Instance.wallVisual : LevelStudioPlugin.Instance.wallRemoveVisual;
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public void ModifyCells(EditorLevelData data, bool forEditor)
        {
            IntVector2 pos2;
            // set the wall
            if (wallState)
            {
                data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls | direction.ToBinary());
                pos2 = direction.ToIntVector2();
                data.cells[position.x + pos2.x, position.z + pos2.z].walls = (Nybble)(data.cells[position.x + pos2.x, position.z + pos2.z].walls | direction.GetOpposite().ToBinary());
                return;
            }
            // remove the wall
            data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls & ~direction.ToBinary());
            pos2 = direction.ToIntVector2();
            data.cells[position.x + pos2.x, position.z + pos2.z].walls = (Nybble)(data.cells[position.x + pos2.x, position.z + pos2.z].walls & ~direction.GetOpposite().ToBinary());
        }

        public void ModifyLightsForEditor(EnvironmentController workerEc)
        {
            throw new NotImplementedException();
        }

        public bool OnDelete(EditorLevelData data)
        {
            EditorController.Instance.levelData.walls.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            EditorController.Instance.RefreshCells();
            return true;
        }

        public bool ValidatePosition(EditorLevelData data)
        {
            return data.RoomFromPos(position, true) != null;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position.ToWorld();
            visualObject.transform.rotation = direction.ToRotation();
        }
    }
}
