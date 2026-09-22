using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Editor
{
    public class AreaRectCreateAction : AbstractEditorAction
    {
        IntVector2 origin;
        IntVector2 size;
        ushort roomId;

        public override void Perform(EditorController controller)
        {
            controller.levelData.areas.Add(new RectCellArea(origin, size, roomId));
            controller.toUpdate |= EditorUpdatables.Cells;
        }

        public override void Undo(EditorController controller)
        {
            controller.levelData.areas.RemoveAt(controller.levelData.areas.Count - 1);
            controller.toUpdate |= EditorUpdatables.Cells;
        }
    }

    public class AreaRectModifyAction : AbstractEditorAction
    {
        IntVector2 sizeDif;
        IntVector2 posDif;
        int index;

        public override void Perform(EditorController controller)
        {
            controller.levelData.areas[index].ResizeWithSafety(sizeDif, posDif);
            controller.toUpdate |= EditorUpdatables.Cells;
        }

        public override void Undo(EditorController controller)
        {
            controller.levelData.areas[index].ResizeWithSafety(sizeDif * -1, posDif * -1);
            controller.toUpdate |= EditorUpdatables.Cells;
        }
    }
}
