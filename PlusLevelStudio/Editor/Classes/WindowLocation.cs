using PlusStudioLevelFormat;
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

        public override void ModifyCells(EditorLevelData data, bool forEditor)
        {
            IntVector2 pos2;
            if (!forEditor)
            {
                data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls | direction.ToBinary());
                pos2 = direction.ToIntVector2();
                data.cells[position.x + pos2.x, position.z + pos2.z].walls = (Nybble)(data.cells[position.x + pos2.x, position.z + pos2.z].walls | direction.GetOpposite().ToBinary());
                return;
            }
            data.cells[position.x, position.z].walls = (Nybble)(data.cells[position.x, position.z].walls & ~direction.ToBinary());
            pos2 = direction.ToIntVector2();
            data.cells[position.x + pos2.x, position.z + pos2.z].walls = (Nybble)(data.cells[position.x + pos2.x, position.z + pos2.z].walls & ~direction.GetOpposite().ToBinary());
        }
    }
}
