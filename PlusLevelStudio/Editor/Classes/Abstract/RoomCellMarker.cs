using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public abstract class RoomCellMarker : CellMarker
    {
        public abstract void CompileIntoRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset);
        public virtual bool CaresAboutRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset)
        {
            for (int i = 0; i < asset.cells.Count; i++)
            {
                if ((asset.cells[i].position.ToInt()) == (position - offset))
                {
                    return true;
                }
            }
            return false;
        }

        public override void Compile(EditorLevelData data, BaldiLevel compiled)
        {
            // do nothing
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            return data.RoomIdFromPos(position, true) != 0;
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            base.UpdateVisual(visualObject);
            visualObject.transform.position += Vector3.up * 11f;
        }
    }
}
