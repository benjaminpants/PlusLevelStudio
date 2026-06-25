using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{

    public enum WallState
    {
        RemoveWall,
        AddWall,
        Oneway
    }

    public class WallLocation : IEditorVisualizable, IEditorCellModifier, IEditorDeletable, IEditorPositionVerifyable
    {
        public WallState wallState;
        public IntVector2 position;
        public Direction direction;
        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            switch (wallState)
            {
                case WallState.AddWall:
                    return LevelStudioPlugin.Instance.wallVisual;
                case WallState.RemoveWall:
                    return LevelStudioPlugin.Instance.wallRemoveVisual;
                case WallState.Oneway:
                    return LevelStudioPlugin.Instance.oneWayWallVisual;
            }
            return null;
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public void ModifyCells(EditorLevelData data, bool forEditor)
        {
            IntVector2 pos2 = direction.ToIntVector2();
            switch (wallState)
            {
                case WallState.AddWall:
                    data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls | direction.ToBinary());
                    data.cells[position.x + pos2.x, position.z + pos2.z].walls = (Nybble)(data.cells[position.x + pos2.x, position.z + pos2.z].walls | direction.GetOpposite().ToBinary());
                    break;
                case WallState.RemoveWall:
                    data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls & ~direction.ToBinary());
                    data.cells[position.x + pos2.x, position.z + pos2.z].walls = (Nybble)(data.cells[position.x + pos2.x, position.z + pos2.z].walls & ~direction.GetOpposite().ToBinary());
                    break;
                case WallState.Oneway:
                    data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls & ~direction.ToBinary());
                    data.cells[position.x + pos2.x, position.z + pos2.z].walls = (Nybble)(data.cells[position.x + pos2.x, position.z + pos2.z].walls | direction.GetOpposite().ToBinary());
                    break;
            }
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
