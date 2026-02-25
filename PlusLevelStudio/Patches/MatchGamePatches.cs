using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(MatchActivity))]
    [HarmonyPatch("ReInit")]
    class MatchGameInitPatch
    {
        static void Postfix(MatchActivity __instance, List<Sprite> ___potentialBalloonSprites, MatchActivityBalloon ___balloonPrefab, ref MatchActivityBalloon[] ___balloon)
        {
            List<Vector3> foundSpawns = new List<Vector3>();
            for (int i = 0; i < __instance.room.objectObject.transform.childCount; i++)
            {
                if (__instance.room.objectObject.transform.GetChild(i).name == "EditorMatchBalloonMarker(Clone)")
                {
                    foundSpawns.Add(__instance.room.objectObject.transform.GetChild(i).position);
                }
            }
            if (foundSpawns.Count == 0) return; // we got nothing to do
            // if its odd and we don't already have enough, add an extra random spawn to prevent problems
            if (((foundSpawns.Count % 2) != 0) && (foundSpawns.Count < ___balloon.Length))
            {
                foundSpawns.Add(__instance.room.RandomEventSafeCellNoGarbage().CenterWorldPosition);
            }
            // destroy excess balloons
            for (int i = 0; i < ___balloon.Length; i++)
            {
                if (i < foundSpawns.Count) continue;
                if (___balloon[i] != null)
                {
                    GameObject.Destroy(___balloon[i].gameObject);
                }
            }
            MatchActivityBalloon[] originalArray = ___balloon;
            ___balloon = new MatchActivityBalloon[foundSpawns.Count];
            for (int i = 0; i < ___balloon.Length; i++)
            {
                ___balloon[i] = originalArray[i];
            }
            foundSpawns.Shuffle();
            while (foundSpawns.Count > ___balloon.Length)
            {
                foundSpawns.RemoveAt(0);
            }
            for (int i = 0; i < foundSpawns.Count; i += 2)
            {
                if (___balloon[i] != null)
                {
                    if (!___balloon[i].gameObject.activeSelf)
                    {
                        GameObject.Destroy(___balloon[i]);
                        GameObject.Destroy(___balloon[i + 1]);
                    }
                    else
                    {
                        ___balloon[i].GetComponent<Entity>().Teleport(foundSpawns[i]);
                        ___balloon[i + 1].GetComponent<Entity>().Teleport(foundSpawns[i + 1]);
                        continue;
                    }
                }
                Sprite sprite = ___potentialBalloonSprites[UnityEngine.Random.Range(0, ___potentialBalloonSprites.Count)];
                // create the balloons
                ___balloon[i] = UnityEngine.Object.Instantiate<MatchActivityBalloon>(___balloonPrefab, __instance.room.transform);
                ___balloon[i + 1] = UnityEngine.Object.Instantiate<MatchActivityBalloon>(___balloonPrefab, __instance.room.transform);
                // initialize them with dummy positions
                ___balloon[i].Initialize(__instance, __instance.room, ___balloon[i + 1], sprite, __instance.room.cells[0].position);
                ___balloon[i + 1].Initialize(__instance, __instance.room, ___balloon[i], sprite, __instance.room.cells[0].position);
                // teleport them to their proper positions
                ___balloon[i].GetComponent<Entity>().Teleport(foundSpawns[i]);
                ___balloon[i + 1].GetComponent<Entity>().Teleport(foundSpawns[i + 1]);
                ___potentialBalloonSprites.Remove(sprite);
            }
        }
    }

    [HarmonyPatch(typeof(MatchActivity))]
    [HarmonyPatch("CompletionCheck")]
    class MatchGameCompletionCheckPatch
    {
        static void Postfix(MatchActivity __instance, MatchActivityBalloon[] ___balloon, bool ___completed)
        {
            if (___completed) return;
            for (int i = 0; i < ___balloon.Length; i++)
            {
                if (!___balloon[i].Completed)
                {
                    return;
                }
            }
            __instance.Completed(0, true);
        }
    }
}
