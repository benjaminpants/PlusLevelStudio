using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class HiddenCellMarker : CellMarker
    {
        public override void Compile(EditorLevelData data, BaldiLevel compiled)
        {
            compiled.secretCells[position.x, position.z] = true;
            compiled.entitySafeCells[position.x, position.z] = false;
            compiled.eventSafeCells[position.x, position.z] = false;
            compiled.coverage[position.x, position.z] = PlusCellCoverage.North | PlusCellCoverage.South | PlusCellCoverage.West | PlusCellCoverage.East | PlusCellCoverage.Up | PlusCellCoverage.Down | PlusCellCoverage.Center; // cover us entirely
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
