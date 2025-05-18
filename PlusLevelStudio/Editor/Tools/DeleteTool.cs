using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class DeleteTool : EditorTool
    {
        CellArea lastFoundArea = null;
        public override string id => "delete";

        public DeleteTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/delete");
        }

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            return true;
        }

        public override void Exit()
        {
            if (lastFoundArea != null)
            {
                EditorController.Instance.HighlightCells(lastFoundArea.CalculateOwnedCells(), "none");
            }
            lastFoundArea = null;
        }

        public override bool MousePressed()
        {
            if (lastFoundArea != null)
            {
                EditorController.Instance.levelData.areas.Remove(lastFoundArea); // TODO: switch this out for the appropiate area removal logic
                EditorController.Instance.RefreshCells();
                return true;
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            CellArea foundArea = EditorController.Instance.levelData.AreaFromPos(EditorController.Instance.mouseGridPosition, true);
            if (lastFoundArea != null)
            {
                EditorController.Instance.HighlightCells(lastFoundArea.CalculateOwnedCells(), "none");
            }
            if (foundArea != null)
            {
                EditorController.Instance.HighlightCells(foundArea.CalculateOwnedCells(), "red");
            }
            lastFoundArea = foundArea;
        }
    }
}
