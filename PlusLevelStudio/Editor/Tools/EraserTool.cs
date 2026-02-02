using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class EraserTool : EditorTool
    {
        public override string id => "eraser";

        List<IntVector2> currentCells = new List<IntVector2>();
        bool erasing = false;

        public EraserTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/eraser");
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
            currentCells.Clear();
            erasing = false;
            EditorController.Instance.RefreshCells();
            EditorController.Instance.UnhighlightAllCells();
        }

        public override bool MousePressed()
        {
            erasing = true;
            return false;
        }

        public override bool MouseReleased()
        {
            if (currentCells.Count == 0) return true;
            EditorController.Instance.AddUndo();

            // now, find all areas we overlapped and get their cells, using a hashset to avoid duplicates
            HashSet<CellArea> overlappedAreas = new HashSet<CellArea>();
            for (int i = 0; i < currentCells.Count; i++)
            {
                CellArea foundArea = EditorController.Instance.levelData.AreaFromPos(currentCells[i], false);
                if (foundArea != null)
                {
                    overlappedAreas.Add(foundArea);
                }
            }
            foreach (var item in overlappedAreas)
            {
                // get all cells except the ones we cut
                List<IntVector2> cells = item.CalculateOwnedCells().ToList();
                currentCells.Do(x => cells.RemoveMatchingIntVector2s(x));
                EditorController.Instance.levelData.areas.Remove(item);
                ProcessRoom(item.roomId, cells);
                EditorController.Instance.levelData.RemoveUnusedRoom(item.roomId);
            }
            EditorController.Instance.RefreshCells(true);
            SoundPlayOneshot("Explosion");
            return true;
        }

        public void ProcessRoom(ushort roomId, List<IntVector2> cells)
        {
            if (cells.Count == 0) return;
            List<List<RectInt>> potRects = new List<List<RectInt>>();
            for (int i = 0; i < 16; i++)
            {
                potRects.Add(EditorHelpers.GenerateIdealRects(cells));
            }
            potRects.Sort(CompareRectLists);
            List<RectInt> rects = potRects[0];
            for (int i = 0; i < rects.Count; i++)
            {
                EditorController.Instance.levelData.areas.Add(new RectCellArea(rects[i].position.ToMystVector(), rects[i].size.ToMystVector(), roomId));
            }
        }

        /// <summary>
        /// Compares the two rect lists by their perimeter, thus prioritizing bigger areas even if those bigger areas would result in more areas overall.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int CompareRectLists(List<RectInt> a, List<RectInt> b)
        {
            int aScore = 0;
            int bScore = 0;
            foreach (RectInt r in a)
            {
                aScore += (r.size.x * 2) + (r.size.y * 2);
            }
            foreach (RectInt r in b)
            {
                bScore += (r.size.x * 2) + (r.size.y * 2);
            }
            return aScore.CompareTo(bScore);
        }

        public override void Update()
        {
            IntVector2 gridPos = EditorController.Instance.mouseGridPosition;
            if (EditorController.Instance.levelData.GetCellSafe(gridPos) == null)
            {
                EditorController.Instance.selector.DisableSelection();
                return;
            }
            EditorController.Instance.selector.SelectTile(gridPos);
            if (erasing)
            {
                if ((!currentCells.Contains(gridPos)) && (EditorController.Instance.levelData.RoomIdFromPos(gridPos, true) != 0))
                {
                    SoundPlayOneshot("Nana_Slip");
                    currentCells.Add(gridPos);
                    Cell cell = EditorController.Instance.workerEc.cells[gridPos.x, gridPos.z];
                    cell.Tile.gameObject.SetActive(false);
                }
            }
        }
    }
}
