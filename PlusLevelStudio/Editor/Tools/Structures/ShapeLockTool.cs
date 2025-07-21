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

        }

        public override bool MousePressed()
        {
            if (EditorController.Instance.levelData.RoomIdFromPos(EditorController.Instance.mouseGridPosition, true) != 0)
            {
                EditorController.Instance.HoldUndo();
                EditorRoom room = EditorController.Instance.levelData.RoomFromPos(EditorController.Instance.mouseGridPosition, true);
                ShapeLockStructureLocation structure = (ShapeLockStructureLocation)EditorController.Instance.AddOrGetStructureToData("shapelock", true);
                if (structure.lockedRooms.Find(x => x.room == room) != null)
                {
                    EditorController.Instance.CancelHeldUndo();
                    structure.DeleteIfInvalid();
                    return false;
                }
                EditorController.Instance.AddHeldUndo();
                structure.CreateAndAddRoom(type, room);
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
            EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
        }
    }
}
