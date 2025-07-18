using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class SpawnpointMoverAndVisualizerScript : MonoBehaviour, IEditorMovable, IEditorInteractable
    {
        public Transform actualTransform;
        bool moved = false;
        public Transform GetTransform()
        {
            return actualTransform;
        }

        public bool InteractableByTool(EditorTool tool)
        {
            return false;
        }

        public void MoveUpdate(Vector3? position, Quaternion? rotation)
        {
            if (position != null)
            {
                moved = true;
                EditorController.Instance.levelData.spawnPoint = new Vector3(position.Value.x, 5f, position.Value.z);
                UpdateTransform();
            }
            if (rotation != null)
            {
                moved = true;
                EditorController.Instance.levelData.spawnDirection = Directions.DirFromVector3(rotation.Value * Vector3.forward, 45f);
                UpdateTransform();
            }
        }

        public void UpdateTransform()
        {
            transform.position = new Vector3(EditorController.Instance.levelData.PracticalSpawnPoint.x, 10f, EditorController.Instance.levelData.PracticalSpawnPoint.z);
            transform.rotation = EditorController.Instance.levelData.PracticalSpawnDirection.ToRotation();
            actualTransform.position = EditorController.Instance.levelData.PracticalSpawnPoint;
            actualTransform.rotation = EditorController.Instance.levelData.PracticalSpawnDirection.ToRotation();
        }

        public bool OnClicked()
        {
            EditorController.Instance.selector.SelectObject(this, MoveAxis.Horizontal, RotateAxis.Flat);
            return false;
        }

        public bool OnHeld()
        {
            throw new NotImplementedException();
        }

        public void OnReleased()
        {
            throw new NotImplementedException();
        }

        public void Selected()
        {
            UpdateTransform();
            EditorController.Instance.HoldUndo();
            moved = false;
        }

        public void Unselected()
        {
            if (moved)
            {
                EditorController.Instance.AddHeldUndo();
            }
            else
            {
                EditorController.Instance.CancelHeldUndo();
            }
        }
    }
}
