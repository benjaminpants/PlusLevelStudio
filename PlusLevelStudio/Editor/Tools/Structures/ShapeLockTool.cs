using PlusStudioLevelLoader;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public class ShapeLockTool : EditorTool
    {
        public string type;
        public override string id => "structure_" + type;
        EditorRoom foundRoom;
        internal ShapeLockTool(string type) : this(type, LevelStudioPlugin.Instance.uiAssetMan.Get<Sprite>("Tools/structure_" + type))
        {
        }

        public ShapeLockTool(string type, Sprite sprite)
        {
            this.sprite = sprite;
            this.type = type;
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
            foundRoom = null;
        }

        public override bool MousePressed()
        {
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                EditorController.Instance.HoldUndo();
                foundRoom = EditorController.Instance.levelData.RoomFromPos(EditorController.Instance.mouseGridPosition, true);
                ShapeLockStructureLocation structure = (ShapeLockStructureLocation)EditorController.Instance.AddOrGetStructureToData("shapelock", true);
                if (structure.lockedRooms.Find(x => x.room == foundRoom) != null)
                {
                    EditorController.Instance.CancelHeldUndo();
                    structure.DeleteIfInvalid();
                    return false;
                }
                EditorController.Instance.AddHeldUndo();
                structure.CreateAndAddRoom(type, foundRoom);
                EditorController.Instance.UpdateVisual(structure);
                return true;
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            EditorRoom oldRoom = foundRoom;
            foundRoom = EditorController.Instance.levelData.RoomFromPos(EditorController.Instance.mouseGridPosition, true);
            StructureLocation structure = EditorController.Instance.GetStructureData("shapelock");
            bool isValid = true;
            if (structure != null)
            {
                isValid = (((ShapeLockStructureLocation)structure).lockedRooms.Find(x => x.room == foundRoom) == null);
            }
            if (oldRoom != foundRoom)
            {
                if (oldRoom != null)
                {
                    EditorController.Instance.HighlightCells(EditorController.Instance.levelData.GetCellsOwnedByRoom(oldRoom), "none");
                }
                EditorController.Instance.HighlightCells(EditorController.Instance.levelData.GetCellsOwnedByRoom(foundRoom), isValid ? "yellow" : "red");
            }
        }
    }
}
