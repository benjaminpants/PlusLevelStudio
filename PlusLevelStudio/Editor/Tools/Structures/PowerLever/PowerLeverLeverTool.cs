using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class PowerLeverLeverTool : EditorTool
    {
        public override string id => "structure_powerlever_lever_" + color.ToString();

        public override string descKey => "Ed_Tool_structure_powerlever_lever_Desc";

        EditorRoom currentRoom;
        PowerLeverLocation currentLever;
        PowerLeverStructureLocation currentStructure;
        IntVector2? pos;
        bool holdingUndo = false;
        CableColor color;

        public PowerLeverLeverTool(CableColor color)
        {
            this.color = color;
            sprite = LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/" + id);
        }

        public override void Begin()
        {
            EditorController.Instance.HoldUndo();
            holdingUndo = true;
            StructureLocation structrure = EditorController.Instance.GetStructureData("powerlever");
            if (structrure != null)
            {
                currentStructure = (PowerLeverStructureLocation)structrure;
            }
        }

        public override bool Cancelled()
        {
            if (currentLever != null)
            {
                currentRoom = null;
                EditorController.Instance.RemoveVisual(currentLever);
                currentLever = null;
                EditorController.Instance.selector.SelectRotation(pos.Value, DirectionSelected);
                return false;
            }
            if (pos != null)
            {
                pos = null;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            currentStructure = null;
            if (holdingUndo)
            {
                EditorController.Instance.CancelHeldUndo();
            }
            holdingUndo = false;
            if (currentLever != null)
            {
                EditorController.Instance.RemoveVisual(currentLever);
            }
            currentRoom = null;
            currentLever = null;
            pos = null;
            if (currentRoom != null)
            {
                EditorController.Instance.HighlightCells(EditorController.Instance.levelData.GetCellsOwnedByRoom(currentRoom), "none");
            }
            currentRoom = null;
        }

        public void DirectionSelected(Direction dir)
        {
            if (!EditorController.Instance.levelData.WallFree(pos.Value, dir, false))
            {
                EditorController.Instance.selector.SelectRotation(pos.Value, DirectionSelected);
                return;
            }
            currentLever = new PowerLeverLocation();
            currentLever.prefab = "powerlever_lever";
            currentLever.position = pos.Value;
            currentLever.direction = dir;
            currentLever.color = color;
            EditorController.Instance.AddVisual(currentLever);
            EditorController.Instance.selector.DisableSelection();
            SoundPlayOneshot("Sfx_Button_Press");
        }

        public override bool MousePressed()
        {
            if (currentRoom != null)
            {
                if (!RoomIsValid(currentRoom))
                {
                    return false;
                }
                PowerLeverStructureLocation structure = (PowerLeverStructureLocation)EditorController.Instance.AddOrGetStructureToData("powerlever", true);
                currentLever.room = currentRoom;
                currentLever.deleteAction = structure.DeleteLever;
                structure.powerLevers.Add(currentLever);
                EditorController.Instance.UpdateVisual(currentLever);
                holdingUndo = false;
                EditorController.Instance.AddHeldUndo();
                currentLever = null;
                SoundPlayOneshot("Sfx_Button_Unpress");
                return true;
            }
            if (pos != null) return false;
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                pos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(pos.Value, DirectionSelected);
                return false;
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public bool RoomIsValid(EditorRoom room)
        {
            if (room == null) return false;
            if (room.roomType == "hall") return false;
            if (currentStructure == null) return true;
            return currentStructure.powerLevers.Find(x => x.room == room) == null;
        }

        public override void Update()
        {
            if (currentLever != null)
            {
                EditorRoom foundRoom = EditorController.Instance.levelData.RoomFromPos(EditorController.Instance.mouseGridPosition, true);
                if (foundRoom != currentRoom)
                {
                    EditorController.Instance.HighlightCells(EditorController.Instance.levelData.GetCellsOwnedByRoom(currentRoom), "none");
                    if (foundRoom != null)
                    {
                        EditorController.Instance.HighlightCells(EditorController.Instance.levelData.GetCellsOwnedByRoom(foundRoom), RoomIsValid(foundRoom) ? "yellow" : "red");
                    }
                    currentRoom = foundRoom;
                }
            }
            if (pos == null)
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
