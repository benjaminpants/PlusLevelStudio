using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.Tools
{
    public abstract class PlaceAndRotateTool : EditorTool
    {
        protected IntVector2? pos;
        public override void Begin()
        {

        }

        public override bool Cancelled()
        {
            if (pos != null)
            {
                pos = null;
                return false;
            }
            return true;
        }

        public override void Exit()
        {
            pos = null;
        }

        /// <summary>
        /// Attempt to "place" at the specified position and direction.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dir"></param>
        /// <returns>Whether the placement was successful. If it was, the tool is returned.</returns>
        protected abstract bool TryPlace(IntVector2 position, Direction dir);

        protected void Place(Direction dir)
        {
            if (TryPlace(pos.Value, dir))
            {
                EditorController.Instance.SwitchToTool(null);
                return;
            }
            EditorController.Instance.selector.SelectRotation(pos.Value, Place);
        }

        public override bool MousePressed()
        {
            if (pos != null) return false;
            if (ValidLocation(EditorController.Instance.mouseGridPosition))
            {
                pos = EditorController.Instance.mouseGridPosition;
                EditorController.Instance.selector.SelectRotation(pos.Value, Place);
                return false;
            }
            return false;
        }

        public virtual bool ValidLocation(IntVector2 position)
        {
            return (EditorController.Instance.levelData.RoomIdFromPos(position, true) != 0);
        }

        public override bool MouseReleased()
        {
            return false;
        }

        public override void Update()
        {
            if (pos == null)
            {
                EditorController.Instance.selector.SelectTile(EditorController.Instance.mouseGridPosition);
            }
        }
    }
}
