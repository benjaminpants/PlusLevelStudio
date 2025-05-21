using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class LightPlacement : IEditorVisualizable, IEditorDeletable
    {
        public IntVector2 position;
        public ushort lightGroup;

        public void CleanupVisual(GameObject visualObject)
        {
            
        }

        public GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.assetMan.Get<GameObject>("LightDisplay");
        }

        public void InitializeVisual(GameObject visualObject)
        {
            visualObject.GetComponent<EditorDeletableObject>().toDelete = this;
            UpdateVisual(visualObject);
        }

        public bool OnDelete(EditorLevelData data)
        {
            data.lights.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            EditorController.Instance.RefreshLights();
            return true;
        }

        public void UpdateVisual(GameObject visualObject)
        {
            visualObject.transform.position = EditorController.Instance.workerEc.CellFromPosition(position).CenterWorldPosition + (Vector3.up * 7f);
        }

        public bool ValidatePosition(EditorLevelData data)
        {
            return data.RoomIdFromPos(position, true) != 0;
        }
    }

    public class LightGroup
    {
        public Color color = new Color(1f, 1f, 1f);
        public int strength = 10;
    }
}
