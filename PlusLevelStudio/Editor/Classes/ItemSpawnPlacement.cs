using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlusStudioLevelLoader;
using MTM101BaldAPI.Registers;

namespace PlusLevelStudio.Editor
{
    public class ItemSpawnPlacement : IEditorDeletable, IEditorVisualizable, IEditorMovable
    {
        public Vector2 position;
        public int weight = 100;
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

        public void MoveUpdate(Vector3? position, Quaternion? rotation)
        {
            if (position.HasValue)
            {
                moved = true;
                this.position = new Vector2(position.Value.x, position.Value.z);
            }
            EditorController.Instance.UpdateVisual(this);
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.itemSpawns.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            return true;
        }

        public void Selected()
        {
            EditorController.Instance.HoldUndo();
            EditorController.Instance.GetVisual(this).GetComponent<EditorRendererContainer>().Highlight("yellow");
        }

        public void Unselected()
        {
            if (moved)
            {
                EditorController.Instance.AddHeldUndo();
            }
            EditorController.Instance.GetVisual(this).GetComponent<EditorRendererContainer>().Highlight("none");
            moved = false;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.GetComponentInChildren<SpriteRenderer>().sprite = ItemMetaStorage.Instance.FindByEnum(Items.None).value.itemSpriteLarge;
            visualObject.transform.position = new Vector3(position.x, 5f, position.y);
        }
    }
}
