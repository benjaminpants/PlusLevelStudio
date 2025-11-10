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
                CellCoverage baseCoverage = extendedAsset.coverages[i];
                CellCoverage finalCoverage = baseCoverage & (CellCoverage.Up | CellCoverage.Down | CellCoverage.Center); // preserve the up down and center coverages since they aren't orientation specific (even though technically the editor doesnt set these coverages yet)
                List<Direction> directionsToRotate = new List<Direction>();
                for (int j = 0; j < 4; j++)
                {
                    if (baseCoverage.HasFlag((CellCoverage)((Direction)j).ToBinary()))
                    {
                        directionsToRotate.Add((Direction)j);
                    }
                }
                for (int j = 0; j < directionsToRotate.Count; j++)
                {
                    finalCoverage |= directionsToRotate[j].RotatedRelativeToNorth(direction).ToCoverage();
                }
                ___ec.CellFromPosition(extendedAsset.coverageCells[i].Adjusted(roomPivot, direction) + position).HardCover(finalCoverage);
            }
        }
    }

    [HarmonyPatch(typeof(LevelBuilder))]
    [HarmonyPatch("CreateItem", new Type[] { typeof(RoomController), typeof(ItemObject), typeof(Vector2), typeof(bool), typeof(bool) })]
    class CreateItemPatch
    {
        static void Prefix(LevelBuilder __instance, ItemObject item, out Pickup __state)
        {
            __state = null;
            if (item.itemType == Items.StickerPack && item.item is ITM_StickerPack)
            {
                __state = __instance.pickupPre;
                __instance.pickupPre = LevelLoaderPlugin.Instance.stickerPickupPre;
            }
        }

        static void Postfix(LevelBuilder __instance, Pickup __state)
        {
            if (__state == null) return;
            __instance.pickupPre = __state;
        }
    }
}
