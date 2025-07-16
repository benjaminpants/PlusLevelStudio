using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class BasicObjectLocation : IEditorVisualizable, IEditorDeletable, IEditorMovable
    {
        public string prefab;
        public Vector3 position;
        public Quaternion rotation;
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

        public void MoveUpdate(Vector3 moveBy)
        {
            position += moveBy;
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
            //throw new NotImplementedException();
        }

        public void Unselected()
        {
            //throw new NotImplementedException();
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position;
            visualObject.transform.rotation = rotation;
        }
    }
}
