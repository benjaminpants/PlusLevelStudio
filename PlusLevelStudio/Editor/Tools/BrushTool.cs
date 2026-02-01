using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class BrushTool : EditorTool
    {
        public override string id => "paintbrush";

        List<IntVector2> currentCells = new List<IntVector2>();
        bool painting = false;
        EditorRoom targetRoom = null;

        public BrushTool()
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/paintbrush");
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
            painting = false;
            EditorController.Instance.RefreshCells();
            EditorController.Instance.UnhighlightAllCells();
        }

        public override bool MousePressed()
        {
            painting = true;
            targetRoom = EditorController.Instance.levelData.RoomFromPos(EditorController.Instance.mouseGridPosition, true);
            if (targetRoom == null)
            {
                targetRoom = EditorController.Instance.levelData.hall;
            }
            EditorController.Instance.HighlightCells(EditorController.Instance.levelData.GetCellsOwnedByRoom(targetRoom), "light_yellow");
            return false;
        }

        public override bool MouseReleased()
        {
            if (currentCells.Count == 0) return true;
            EditorController.Instance.AddUndo();
            List<List<RectInt>> potRects = new List<List<RectInt>>();
            for (int i = 0; i < 16; i++)
            {
                potRects.Add(GenerateIdealRects(currentCells));
            }
            potRects.Sort(CompareRectLists);
            List<RectInt> rects = potRects[0];
            ushort roomId = EditorController.Instance.levelData.IdFromRoom(targetRoom);
            for (int i = 0; i < rects.Count; i++)
            {
                EditorController.Instance.levelData.areas.Add(new RectCellArea(rects[i].position.ToMystVector(), rects[i].size.ToMystVector(), roomId));
            }
            SoundPlayOneshot("Ben_Splat");
            return true;
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
            if (painting)
            {
                if ((!currentCells.Contains(gridPos)) && (EditorController.Instance.levelData.RoomIdFromPos(gridPos, true) == 0))
                {
                    SoundPlayOneshot("Nana_Sput");
                    currentCells.Add(gridPos);
                    // room shouldn't be null here, because if we've reached this point roomId wasn't zero
                    Cell cell = EditorController.Instance.workerEc.cells[gridPos.x, gridPos.z];
                    cell.Tile.gameObject.SetActive(true);
                    cell.Tile.MeshRenderer.material.SetMainTexture(EditorController.Instance.GenerateTextureAtlas(targetRoom.floorTex, targetRoom.wallTex, targetRoom.ceilTex));
                    cell.SetShape(0, TileShapeMask.None);
                    cell.Tile.MeshRenderer.material.SetTexture("_LightMap", LevelStudioPlugin.Instance.lightmaps["yellow"]);
                    //EditorController.Instance.RefreshCells();
                }
            }
        }

        public List<RectInt> GenerateIdealRects(List<IntVector2> cells)
        {
            cells = new List<IntVector2>(cells);
            List<RectInt> rects = new List<RectInt>();
            while (cells.Count > 0)
            {
                IntVector2 lowestCell = cells[UnityEngine.Random.Range(0, cells.Count)];
                RectInt currentRect = new RectInt(new Vector2Int(lowestCell.x,lowestCell.z), new Vector2Int(1,1));
                List<Direction> allDirections = Directions.All();
                allDirections.Shuffle();
                while (allDirections.Count > 0)
                {
                    // try all directions
                    // TODO: do we ACTUALLY need to determine the order of expansion directions? does it actually matter
                    RectInt highestExpansionRect = currentRect;
                    int highestExpansionIndex = 0;
                    for (int i = 0; i < allDirections.Count; i++)
                    {
                        // try each direction, expanding as far as it can go.
                        RectInt demoRect = new RectInt(currentRect.position, currentRect.size);
                        bool expandedToMaxSize = false;
                        // keep expanding until we hit something
                        while (!expandedToMaxSize)
                        {
                            EditorExtensions.CalculateDifferencesForHandleDrag(allDirections[i], 1, out IntVector2 sizeDif, out IntVector2 posDif);
                            demoRect.size += sizeDif.ToUnityVector();
                            demoRect.position += posDif.ToUnityVector();
                            foreach (Vector2Int pos in demoRect.allPositionsWithin)
                            {
                                if ((cells.FindIndex(x => (x.x == pos.x) && (x.z == pos.y)) == -1))
                                {
                                    demoRect.size -= sizeDif.ToUnityVector();
                                    demoRect.position -= posDif.ToUnityVector();
                                    expandedToMaxSize = true;
                                    break;
                                }
                            }
                        }
                        if ((demoRect.size.x * demoRect.size.y) > (highestExpansionRect.size.x * highestExpansionRect.size.y))
                        {
                            highestExpansionRect = demoRect;
                            highestExpansionIndex = i;
                        }
                    }
                    allDirections.RemoveAt(highestExpansionIndex);
                    currentRect = highestExpansionRect;
                }
                foreach (Vector2Int pos in currentRect.allPositionsWithin)
                {
                    if (cells.RemoveAll(x => x.x == pos.x && x.z == pos.y) == 0)
                    {
                        Debug.LogWarning("Failed to remove position from area? Did we expand OOB?");
                    }
                }
                rects.Add(currentRect);
            }
            return rects;
        }
    }
}
