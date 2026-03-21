using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class ExcludedCellMarker : CellMarker
    {
        public override void Compile(EditorLevelData data, BaldiLevel compiled)
        {
            compiled.excludedFromRoomGroupCells[position.x, position.z] = true;
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            return data.RoomIdFromPos(position, true) != 0;
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            base.UpdateVisual(visualObject);
            visualObject.transform.position += Vector3.up * 11.2f;
        }
    }
}
