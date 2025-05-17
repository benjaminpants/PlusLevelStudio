using System;
using System.Collections.Generic;
using System.Text;
using PlusStudioLevelFormat;

namespace PlusLevelStudio.Editor
{
    public class EditorLevelData
    {
        public IntVector2 mapSize = new IntVector2(50,50);
        public PlusStudioLevelFormat.Cell[,] cells;

        public bool ResizeLevel(IntVector2 posDif, IntVector2 sizeDif)
        {
            List<PlusStudioLevelFormat.Cell> cellList = new List<PlusStudioLevelFormat.Cell>();
            IntVector2 newMapSize = mapSize + sizeDif;
            // TODO: add optimization for when posDif.x and posDif.z == 0
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    IntVector2 calculatedNewPos = (cells[x, y].position.ToInt() - posDif);
                    // if we are out of bounds
                    if (calculatedNewPos.x < 0 || calculatedNewPos.z < 0 || calculatedNewPos.x >= newMapSize.x || calculatedNewPos.z >= newMapSize.z)
                    {
                        if (cells[x, y].roomId != 0)
                        {
                            return false;
                        }
                        continue;
                    }
                    cells[x, y].position = calculatedNewPos.ToByte();
                    cellList.Add(cells[x, y]);
                }
            }
            mapSize = newMapSize;
            cells = new PlusStudioLevelFormat.Cell[mapSize.x, mapSize.z];
            for (int i = 0; i < cellList.Count; i++)
            {
                cells[cellList[i].position.x, cellList[i].position.y] = cellList[i];
            }
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    if (cells[x, y] == null)
                    {
                        cells[x, y] = new PlusStudioLevelFormat.Cell(new ByteVector2(x, y));
                    }
                }
            }
            return true;
        }

        public EditorLevelData(IntVector2 mapSize)
        {
            this.mapSize = mapSize;
            cells = new PlusStudioLevelFormat.Cell[mapSize.x, mapSize.z];
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.z; y++)
                {
                    cells[x, y] = new PlusStudioLevelFormat.Cell(new ByteVector2(x,y));
                }
            }
        }
    }
}
