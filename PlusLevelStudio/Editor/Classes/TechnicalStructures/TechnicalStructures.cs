using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class PotentialDoorLocation : RoomTechnicalStructurePoint
    {
        public override void CompileIntoRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset)
        {
            asset.potentialDoorPositions.Add((position - offset).ToByte());
        }
    }

    public class ForcedDoorLocation : RoomTechnicalStructurePoint
    {
        public override void CompileIntoRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset)
        {
            asset.forcedDoorPositions.Add((position - offset).ToByte());
        }
    }

    public class UnsafeCellLocation : RoomTechnicalStructurePoint
    {
        public override void CompileIntoRoom(EditorLevelData data, BaldiLevel compiled, IntVector2 offset, BaldiRoomAsset asset)
        {
            int indexOfSafeCell = asset.entitySafeCells.FindIndex(0, x => x == (position - offset).ToByte());
            if (indexOfSafeCell != -1)
            {
                asset.entitySafeCells.RemoveAt(indexOfSafeCell);
            }
            indexOfSafeCell = asset.eventSafeCells.FindIndex(0, x => x == (position - offset).ToByte());
            if (indexOfSafeCell != -1)
            {
                asset.eventSafeCells.RemoveAt(indexOfSafeCell);
            }
        }
    }

    public class RoomLightLocation : RoomTechnicalStructurePoint
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
