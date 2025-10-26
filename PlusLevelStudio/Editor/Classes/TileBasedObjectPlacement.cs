using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class TileBasedObjectPlacement : IEditorVisualizable, IEditorDeletable, IEditorPositionVerifyable
    {
        public string type;
        public IntVector2 position;
        public Direction direction;
        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.tileBasedObjectDisplays[type];
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.tileBasedObjects.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            return true;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position.ToWorld();
            visualObject.transform.rotation = direction.ToRotation();
        }

        public bool ValidatePosition(EditorLevelData data)
        {
            return (data.RoomFromPos(position, true) != null);
        }
    }
}
