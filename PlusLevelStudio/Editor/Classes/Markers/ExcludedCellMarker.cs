using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class ExcludedCellMarker : RoomCellMarker
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

        public override void CompileIntoRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset)
        {
            asset.cellsExcludedFromRoomGroup.Add((position - offset).ToByte());
        }
    }
}
