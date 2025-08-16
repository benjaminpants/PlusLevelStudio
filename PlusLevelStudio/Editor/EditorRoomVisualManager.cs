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

        /// <summary>
        /// Called when this is about to be destroyed.
        /// </summary>
        public virtual void Cleanup()
        {

        }

        /// <summary>
        /// Called when this is initially created.
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Used to modify the lights.
        /// </summary>
        /// <param name="workerEc"></param>
        public virtual void ModifyLightsForEditor(EnvironmentController workerEc)
        {

        }

        /// <summary>
        /// Called whenever this room is updated, supposedly. Only appears to be called when every visual gets refreshed.
        /// </summary>
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
