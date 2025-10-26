using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class BasicObjectLocation : IEditorVisualizable, IEditorDeletable, IEditorMovable, IEditorPositionVerifyable
    {
        public string prefab;
        public Vector3 position;
        public Quaternion rotation;
        bool moved = false;
        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public Transform GetTransform()
        {
            return EditorController.Instance.GetVisual(this).transform;
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.basicObjectDisplays[prefab].gameObject;
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorBasicObject>().AssignLocation(this);
            UpdateVisual(visualObject);
        }

        public void MoveUpdate(Vector3? position, Quaternion? rotation)
        {
            if (position.HasValue)
            {
                moved = true;
                this.position = position.Value;
            }
            if (rotation.HasValue)
            {
                moved = true;
                this.rotation = rotation.Value;
            }
            EditorController.Instance.UpdateVisual(this);
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.objects.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            return true;
        }

        public void Selected()
        {
            EditorController.Instance.GetVisual(this).GetComponent<EditorRendererContainer>().Highlight("yellow");
            EditorController.Instance.HoldUndo();
        }

        public void Unselected()
        {
            EditorController.Instance.GetVisual(this).GetComponent<EditorRendererContainer>().Highlight("none");
            if (!moved)
            {
                EditorController.Instance.CancelHeldUndo();
            }   
            else
            {
                EditorController.Instance.AddHeldUndo();
            }
            moved = false;
            if (!ValidatePosition(EditorController.Instance.levelData))
            {
                EditorController.Instance.RemoveVisual(this);
                EditorController.Instance.levelData.objects.Remove(this);
            }
        }

        public bool ValidatePosition(EditorLevelData data)
        {
            if (!EditorController.Instance.currentMode.allowOutOfRoomObjects) return data.RoomFromPos(new IntVector2(Mathf.RoundToInt((position.x - 5f) / 10f), Mathf.RoundToInt((position.z - 5f) / 10f)), true) != null;
            return data.GetCellSafe(Mathf.RoundToInt((position.x - 5f) / 10f), Mathf.RoundToInt((position.z - 5f) / 10f)) != null;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position;
            visualObject.transform.rotation = rotation;
        }
    }
}
