using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class EventUnsafeCellLocation : CellMarker
    {
        public EventUnsafeCellLocation()
        {
            type = "eventunsafe";
        }
        public override void Compile(EditorLevelData data, BaldiLevel compiled)
        {
            compiled.eventSafeCells[position.x, position.z] = false;
        }

        public override bool ValidatePosition(EditorLevelData data)
        {
            return data.RoomIdFromPos(position, true) != 0;
        }

        public override void UpdateVisual(GameObject visualObject)
        {
            base.UpdateVisual(visualObject);
            visualObject.transform.position += Vector3.up * 11.1f;
        }
    }

    public class EntityUnsafeCellLocation : CellMarker
    {
        public EntityUnsafeCellLocation()
        {
            type = "entityunsafe";
        }
        public override void Compile(EditorLevelData data, BaldiLevel compiled)
        {
            compiled.entitySafeCells[position.x, position.z] = false;
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
