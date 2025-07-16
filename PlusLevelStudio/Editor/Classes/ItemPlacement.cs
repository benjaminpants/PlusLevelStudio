using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlusStudioLevelLoader;

namespace PlusLevelStudio.Editor
{
    public class ItemPlacement : IEditorDeletable, IEditorVisualizable, IEditorMovable
    {
        public Vector2 position;
        public string item;
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
            return LevelStudioPlugin.Instance.pickupVisual;
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            visualObject.GetComponent<MovableObjectInteraction>().target = this;
            UpdateVisual(visualObject);
        }

        public void MoveUpdate(Vector3 moveBy, float gridSnap)
        {
            moved = true;
            if (gridSnap == 0)
            {
                position += new Vector2(moveBy.x, moveBy.z);
                EditorController.Instance.UpdateVisual(this);
                return;
            }
            position = new Vector2(Mathf.Round((position.x + moveBy.x) / gridSnap) * gridSnap, Mathf.Round((position.y + moveBy.z) / gridSnap) * gridSnap);
            EditorController.Instance.UpdateVisual(this);
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.items.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            return true;
        }

        public void Selected()
        {
            EditorController.Instance.HoldUndo();
            EditorController.Instance.GetVisual(this).GetComponent<EditorDeletableObject>().Highlight("yellow");
        }

        public void Unselected()
        {
            if (moved)
            {
                EditorController.Instance.AddHeldUndo();
            }
            EditorController.Instance.GetVisual(this).GetComponent<EditorDeletableObject>().Highlight("none");
            moved = false;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.GetComponentInChildren<SpriteRenderer>().sprite = LevelLoaderPlugin.Instance.itemObjects[item].itemSpriteLarge;
            visualObject.transform.position = new Vector3(position.x, 5f, position.y);
        }
    }
}
