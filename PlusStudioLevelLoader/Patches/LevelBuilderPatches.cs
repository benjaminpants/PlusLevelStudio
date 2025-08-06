using HarmonyLib;
using Rewired.Utils.Classes.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusStudioLevelLoader.Patches
{
    [HarmonyPatch(typeof(LevelBuilder))]
    [HarmonyPatch("LoadIntoExistingRoom")]
    class LoadIntoExistingRoomPatch
    {
        static void Postfix(RoomController room, RoomAsset asset, IntVector2 position, IntVector2 roomPivot, Direction direction, EnvironmentController ___ec)
        {
            if (!(asset is ExtendedRoomAsset)) return;
            ExtendedRoomAsset extendedAsset = (ExtendedRoomAsset)asset;
            for (int i = 0; i < extendedAsset.coverages.Count; i++)
            {
                // TODO: rotate coverage
                ___ec.CellFromPosition(extendedAsset.coverageCells[i].Adjusted(roomPivot, direction) + position).HardCover(extendedAsset.coverages[i]);
            }
        }
    }
}
