using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class SimpleLeverLocation : SimpleButtonLocation
    {
        public Func<SimpleLeverLocation, bool> shouldBeDown;
        public override void UpdateVisual(GameObject visualObject)
        {
            base.UpdateVisual(visualObject);
            visualObject.GetComponent<LeverVisual>().SetDirection(shouldBeDown(this));
        }
    }

    public class SimpleButtonLocation : SimpleLocation
    {
        public override bool ValidatePosition(EditorLevelData data, bool ignoreSelf)
        {
            bool ret = base.ValidatePosition(data, ignoreSelf);
            if (ret == false) return false;
            if (!data.WallFree(position, direction, ignoreSelf))
            {
                return false;
            }
            return true;
        }
    }


    public class SimpleLocation : IEditorVisualizable, IEditorDeletable
    {
        public Func<EditorLevelData, SimpleLocation, bool> deleteAction;
        public string prefab;
        public IntVector2 position;
        public Direction direction;
        public void CleanupVisual(GameObject visualObject)
        {

        }

        public virtual GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericStructureDisplays[prefab];
        }

        public virtual bool ValidatePosition(EditorLevelData data, bool ignoreSelf)
        {
            PlusStudioLevelFormat.Cell cell = data.GetCellSafe(position);
            if (cell == null) return false; // cell doesn't exist
            if (cell.type == 16) return false; // the cell is empty
            return true;
        }

        public virtual void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public bool OnDelete(EditorLevelData data)
        {
            return deleteAction(data, this);
        }

        public virtual void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position.ToWorld();
            visualObject.transform.rotation = direction.ToRotation();
        }
    }

    public class PointLocation : IEditorVisualizable, IEditorDeletable
    {
        public Func<EditorLevelData, PointLocation, bool> deleteAction;
        public string prefab;
        public IntVector2 position;
        public virtual void CleanupVisual(GameObject visualObject)
        {

        }

        public virtual GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.genericStructureDisplays[prefab];
        }

        public virtual bool ValidatePosition(EditorLevelData data, bool ignoreSelf)
        {
            PlusStudioLevelFormat.Cell cell = data.GetCellSafe(position);
            if (cell == null) return false; // cell doesn't exist
            if (cell.type == 16) return false; // the cell is empty
            return true;
        }

        public virtual void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public bool OnDelete(EditorLevelData data)
        {
            return deleteAction(data, this);
        }

        public virtual void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = position.ToWorld();
        }
    }
}
