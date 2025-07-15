using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlusStudioLevelLoader;

namespace PlusLevelStudio.Editor
{
    public class ItemPlacement : IEditorDeletable, IEditorVisualizable, IEditorObjectMovable
    {
        public Vector2 position;
        public string item;
        protected bool gotMoved = false;
        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public Bounds GetBounds()
        {
            return new Bounds(new Vector3(position.x,5f, position.y), Vector3.one * 2f);
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

        public void Move(Vector3 moveBy)
        {
            gotMoved = true;
            position = new Vector2(position.x + moveBy.x, position.y + moveBy.z);
            EditorController.Instance.UpdateVisual(this);
        }

        public void MoveEnd()
        {
            if (!gotMoved)
            {
                EditorController.Instance.CancelHeldUndo();
                return;
            }
            EditorController.Instance.AddHeldUndo();
            gotMoved = false;
        }

        public void MoveHighlight(bool highlight)
        {
            // TODO: should I really be doing this?
            EditorController.Instance.GetVisual(this).GetComponent<EditorDeletableObject>().Highlight(highlight ? "yellow" : "none");
        }

        public void MoveStart()
        {
            EditorController.Instance.HoldUndo();
            // none
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.items.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            return true;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.GetComponentInChildren<SpriteRenderer>().sprite = LevelLoaderPlugin.Instance.itemObjects[item].itemSpriteLarge;
            visualObject.transform.position = new Vector3(position.x, 5f, position.y);
        }
    }
}
