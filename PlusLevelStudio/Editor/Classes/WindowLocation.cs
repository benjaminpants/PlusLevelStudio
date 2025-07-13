using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class WindowLocation : DoorLocation
    {
        public override GameObject GetVisualPrefab()
        {
            return LevelStudioPlugin.Instance.windowDisplays[type].gameObject;
        }

        public override bool OnDelete(EditorLevelData data)
        {
            data.windows.Remove(this);
            EditorController.Instance.RemoveVisual(this);
            EditorController.Instance.RefreshCells();
            EditorController.Instance.RefreshLights();
            return true;
        }
    }
}
