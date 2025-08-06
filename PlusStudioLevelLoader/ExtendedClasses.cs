using System;
using System.Collections.Generic;
using System.Text;

namespace PlusStudioLevelLoader
{
    [Serializable]
    public class ExtendedExtraLevelData : ExtraLevelData
    {
        public float timeOutTime = 0f;
        public RandomEvent timeOutEvent;
    }

    public class ExtendedExtraLevelDataAsset : ExtraLevelDataAsset
    {
        public float timeOutTime = 0f;
        public RandomEvent timeOutEvent;
    }

    public class ExtendedRoomAsset : RoomAsset
    {
        public List<IntVector2> coverageCells = new List<IntVector2>();
        public List<CellCoverage> coverages = new List<CellCoverage>();
        public void AddCellCoverage(IntVector2 cell, CellCoverage coverage)
        {
            coverageCells.Add(cell);
            coverages.Add(coverage);
        }
        public CellCoverage GetCellCoverage(IntVector2 cell)
        {
            int cellIndex = coverageCells.IndexOf(cell);
            if (cellIndex == -1) return CellCoverage.None;
            return coverages[cellIndex];
        }
    }

    [Serializable]
    public class ExtendedRoomData : RoomData
    {
        public List<IntVector2> coverageCells = new List<IntVector2>();
        public List<CellCoverage> coverages = new List<CellCoverage>();
        public void AddCellCoverage(IntVector2 cell, CellCoverage coverage)
        {
            coverageCells.Add(cell);
            coverages.Add(coverage);
        }
        public CellCoverage GetCellCoverage(IntVector2 cell)
        {
            int cellIndex = coverageCells.IndexOf(cell);
            if (cellIndex == -1) return CellCoverage.None;
            return coverages[cellIndex];
        }
    }
}
