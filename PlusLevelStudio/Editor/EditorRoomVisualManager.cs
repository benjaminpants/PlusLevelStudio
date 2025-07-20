using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public abstract class EditorRoomVisualManager : MonoBehaviour
    {
        public EditorRoom myRoom;
        public EditorLevelData myLevelData;

        public virtual void Cleanup()
        {

        }

        public virtual void Initialize()
        {

        }

        public virtual void ModifyLightsForEditor(EnvironmentController workerEc)
        {

        }

        public virtual void RoomUpdated()
        {

        }
    }

    public class OutsideRoomVisualManager : EditorRoomVisualManager
    {
        public Color color = Color.white;
        public override void ModifyLightsForEditor(EnvironmentController workerEc)
        {
            ushort myId = myLevelData.IdFromRoom(myRoom);
            foreach (PlusStudioLevelFormat.Cell cell in myLevelData.cells)
            {
                if (cell.roomId == myId)
                {
                    Cell properCell = workerEc.CellFromPosition(cell.position.x,cell.position.y);
                    workerEc.GenerateLight(properCell, color, 1);
                    workerEc.RegenerateLight(properCell);
                }
            }
            /*IntVector2[] foundCells = myLevelData.GetCellsOwnedByRoom(myRoom);
            for (int i = 0; i < foundCells.Length; i++)
            {
                Cell cell = workerEc.CellFromPosition(foundCells[i]);
                workerEc.GenerateLight(cell, color, 1);
                workerEc.RegenerateLight(cell);
            }*/
        }
    }
}
