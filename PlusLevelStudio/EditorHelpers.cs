using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio
{
    public static class EditorHelpers
    {
        public static List<RectInt> GenerateIdealRects(List<IntVector2> cells)
        {
            cells = new List<IntVector2>(cells);
            List<RectInt> rects = new List<RectInt>();
            while (cells.Count > 0)
            {
                IntVector2 lowestCell = cells[UnityEngine.Random.Range(0, cells.Count)];
                RectInt currentRect = new RectInt(new Vector2Int(lowestCell.x, lowestCell.z), new Vector2Int(1, 1));
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
