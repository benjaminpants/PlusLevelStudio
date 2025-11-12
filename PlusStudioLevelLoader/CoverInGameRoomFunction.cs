using System;
using System.Collections.Generic;
using System.Text;

namespace PlusStudioLevelLoader
{
    public class CoverInGameRoomFunction : RoomFunction
    {
        public override void OnGenerationFinished()
        {
            foreach (Cell cell in room.cells)
            {
                if (!hardCover)
                {
                    cell.SoftCover(coverage);
                }
                else
                {
                    cell.HardCover(coverage);
                }
            }
        }

        public bool hardCover;

        public CellCoverage coverage = (CellCoverage)(-1);
    }
}
