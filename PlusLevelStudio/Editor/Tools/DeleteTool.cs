using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class DeleteTool : EditorTool
    {
        CellArea lastFoundArea = null;
        ExitLocation lastFoundExit = null;
        EditorDeletableObject lastFoundDeletable = null;
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
            if (lastFoundExit != null)
            {
                EditorController.Instance.HighlightCells(lastFoundExit.GetOwnedCells(), "none");
            }
            if (lastFoundDeletable != null)
            {
                lastFoundDeletable.Highlight("none");
            }
            lastFoundDeletable = null;
            lastFoundArea = null;
            lastFoundExit = null;
        }

        public override bool MousePressed()
        {
            if (lastFoundDeletable != null)
            {
                EditorController.Instance.AddUndo();
                return lastFoundDeletable.OnDelete(EditorController.Instance.levelData);
            }
            if (lastFoundExit != null)
            {
                EditorController.Instance.AddUndo();
                EditorController.Instance.levelData.exits.Remove(lastFoundExit);
                EditorController.Instance.RemoveVisual(lastFoundExit);
                EditorController.Instance.RefreshCells();
                EditorController.Instance.UpdateSpawnVisual();
            }
            if (lastFoundArea != null)
            {
                EditorController.Instance.AddUndo();
                EditorController.Instance.levelData.RemoveObjectsInArea(lastFoundArea);
                EditorController.Instance.levelData.areas.Remove(lastFoundArea);
                EditorController.Instance.levelData.ValidateActivityInRoom(EditorController.Instance.levelData.RoomFromId(lastFoundArea.roomId));
                EditorController.Instance.levelData.RemoveUnusedRoom(lastFoundArea.roomId);
                // ACK HACK
                EditorController.Instance.levelData.UpdateCells(true);
                EditorController.Instance.RefreshCells();
                return true;
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        // TODO: revise this code
        public override void Update()
        {
            if (Physics.Raycast(EditorController.Instance.mouseRay, out RaycastHit info, 1000f, LevelStudioPlugin.editorInteractableLayerMask))
            {
                EditorDeletableObject previousDeletabl = lastFoundDeletable;
                if (info.transform.TryGetComponent(out lastFoundDeletable))
                {
                    if (previousDeletabl != null)
                    {
                        if (previousDeletabl != lastFoundDeletable)
                        {
                            previousDeletabl.Highlight("none");
                        }
                    }
                    lastFoundDeletable.Highlight("red");
                    if (lastFoundArea != null)
                    {
                        EditorController.Instance.HighlightCells(lastFoundArea.CalculateOwnedCells(), "none");
                    }
                    if (lastFoundExit != null)
                    {
                        EditorController.Instance.HighlightCells(lastFoundExit.GetOwnedCells(), "none");
                    }
                    lastFoundExit = null;
                    lastFoundArea = null;
                    return;
                }
            }
            if (lastFoundDeletable != null)
            {
                lastFoundDeletable.Highlight("none");
            }
            lastFoundDeletable = null;
            if (lastFoundArea != null)
            {
                EditorController.Instance.HighlightCells(lastFoundArea.CalculateOwnedCells(), "none");
            }
            if (lastFoundExit != null)
            {
                EditorController.Instance.HighlightCells(lastFoundExit.GetOwnedCells(), "none");
            }
            ExitLocation foundExit = null;
            foreach (ExitLocation exit in EditorController.Instance.levelData.exits)
            {
                if (exit.CellOwned(EditorController.Instance.mouseGridPosition))
                {
                    foundExit = exit;
                    EditorController.Instance.HighlightCells(foundExit.GetOwnedCells(), "red");
                    break;
                }
            }
            lastFoundExit = foundExit;
            CellArea foundArea = EditorController.Instance.levelData.AreaFromPos(EditorController.Instance.mouseGridPosition, true);
            if (foundArea != null)
            {
                EditorController.Instance.HighlightCells(foundArea.CalculateOwnedCells(), "red");
            }
            lastFoundArea = foundArea;
        }
    }
}
