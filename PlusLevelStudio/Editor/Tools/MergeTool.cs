using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class MergeTool : EditorTool
    {
        public override string id => "merge";
        EditorRoom currentRoom = null;
        CellArea currentHoveredArea = null;
        EditorRoom lastHoveredRoom = null;
        ushort currentRoomId => EditorController.Instance.levelData.IdFromRoom(currentRoom);

        public MergeTool() 
        {
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/merge");
        }

        public void HighlightAllAreasBelongingToRoom(ushort roomId, string highlight)
        {
            if (roomId == 0) return;
            foreach (CellArea area in EditorController.Instance.levelData.areas)
            {
                if (area.roomId != roomId) continue;
                EditorController.Instance.HighlightCells(area.CalculateOwnedCells(), highlight);
            }
        }

        public override void Begin()
        {
            
        }

        public override bool Cancelled()
        {
            HighlightAllAreasBelongingToRoom(currentRoomId, "none");
            if (currentRoom != null)
            {
                if (currentHoveredArea != null)
                {
                    EditorController.Instance.HighlightCells(currentHoveredArea.CalculateOwnedCells(), "none");
                }
                currentRoom = null;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            currentRoom = null;
            currentHoveredArea = null;
        }

        public override bool MousePressed()
        {
            if (currentRoom != null)
            {
                if (currentHoveredArea == null) return false; // hovering over nothing
                if (EditorController.Instance.levelData.RoomFromId(currentHoveredArea.roomId).roomType != currentRoom.roomType) return false; // room is from different type
                EditorController.Instance.AddUndo();
                ushort oldId = currentHoveredArea.roomId;
                currentHoveredArea.roomId = currentRoomId;
                EditorController.Instance.levelData.RemoveUnusedRoom(oldId); // check to see if the room of the area we just removed is unused now, if it is, remove it
                EditorController.Instance.RefreshCells();
                HighlightAllAreasBelongingToRoom(currentRoomId, "yellow");
                currentHoveredArea = null;
                return false;
            }
            currentRoom = lastHoveredRoom; // this does nothing if we are hovering over empty space
            HighlightAllAreasBelongingToRoom(currentRoomId, "yellow");
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            // player has selected a room
            if (currentRoom != null)
            {
                CellArea hoveringArea = EditorController.Instance.levelData.AreaFromPos(EditorController.Instance.mouseGridPosition, true);
                if (currentHoveredArea != null)
                {
                    EditorController.Instance.HighlightCells(currentHoveredArea.CalculateOwnedCells(), "none");
                }
                if ((hoveringArea != null) && (hoveringArea.roomId != currentRoomId))
                {
                    if (EditorController.Instance.levelData.RoomFromId(hoveringArea.roomId).roomType == currentRoom.roomType) // somehow this is getting null?
                    {
                        EditorController.Instance.HighlightCells(hoveringArea.CalculateOwnedCells(), "green");
                    }
                    else
                    {
                        EditorController.Instance.HighlightCells(hoveringArea.CalculateOwnedCells(), "red");
                    }
                }
                else
                {
                    hoveringArea = null;
                }

                currentHoveredArea = hoveringArea;
                return;
            }
            // player hasn't selected a room
            EditorRoom hoveringRoom = EditorController.Instance.levelData.RoomFromPos(EditorController.Instance.mouseGridPosition, true);
            if (hoveringRoom != lastHoveredRoom)
            {
                HighlightAllAreasBelongingToRoom(EditorController.Instance.levelData.IdFromRoom(lastHoveredRoom), "none");
                HighlightAllAreasBelongingToRoom(EditorController.Instance.levelData.IdFromRoom(hoveringRoom), "yellow");
            }
            lastHoveredRoom = hoveringRoom;
        }
    }
}
