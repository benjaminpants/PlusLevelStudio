using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public abstract class PointTool : EditorTool
    {
        public override void Update()
        {
            EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
        }

        public virtual bool ValidLocation(IntVector2 position)
        {
            return (EditorController.Instance.levelData.RoomIdFromPos(position, true) != 0);
        }

        protected abstract bool TryPlace(IntVector2 position);

        public override bool MousePressed()
        {
            if (ValidLocation(EditorController.Instance.mouseGridPosition))
            {
                return TryPlace(EditorController.Instance.mouseGridPosition);
            }
            return false;
        }

        public override bool MouseReleased()
        {
            return false;
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
    }
}
