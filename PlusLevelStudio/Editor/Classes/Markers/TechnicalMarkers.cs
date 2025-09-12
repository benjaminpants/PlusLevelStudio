using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class PotentialDoorLocation : RoomCellMarker
    {
        public override void CompileIntoRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset)
        {
            asset.potentialDoorPositions.Add((position - offset).ToByte());
        }
    }

    public class ForcedDoorLocation : RoomCellMarker
    {
        public override void CompileIntoRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset)
        {
            asset.forcedDoorPositions.Add((position - offset).ToByte());
        }
    }

    public class RoomLightLocation : RoomCellMarker
    {
        public override void ModifyLightsForEditor(EnvironmentController workerEc)
        {
            LightGroup groupZero = EditorController.Instance.levelData.lightGroups[0];
            workerEc.GenerateLight(workerEc.CellFromPosition(position), groupZero.color, groupZero.strength);
        }
        public override void CompileIntoRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset)
        {
            asset.standardLightCells.Add((position - offset).ToByte());
        }
    }
}
