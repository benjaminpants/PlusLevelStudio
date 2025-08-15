using HarmonyLib;
using PlusLevelStudio.UI;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using MTM101BaldAPI;
using MTM101BaldAPI.Reflection;

namespace PlusLevelStudio.Ingame
{
    public class Structure_SteamValvesEditor : Structure_SteamValves
    {
        LevelBuilder builder;
        public override void Initialize(EnvironmentController ec, StructureParameters parameters)
        {
            base.Initialize(ec, parameters);
            builder = GameObject.FindObjectsOfType<LevelBuilder>().First(); // MYSTMAN WHY
        }

        static FieldInfo _steamValvePrefab = AccessTools.Field(typeof(Structure_SteamValves), "steamValvePrefab");
        static FieldInfo _valvePre = AccessTools.Field(typeof(Structure_SteamValves), "valvePre");
        static FieldInfo _spreadMap = AccessTools.Field(typeof(SteamValveController), "spreadMap");
        static FieldInfo _generatedValves = AccessTools.Field(typeof(Structure_SteamValves), "generatedValves");

        float startOnChance = 0f;

        public override void Load(List<StructureData> data)
        {
            List<GameButtonBase> generatedValves = (List<GameButtonBase>)_generatedValves.GetValue(this);
            SteamValveController steamValvePrefab = (SteamValveController)_steamValvePrefab.GetValue(this);
            GameButtonBase valvePre = (GameButtonBase)_valvePre.GetValue(this);
            Queue<StructureData> queue = new Queue<StructureData>(data);
            startOnChance = queue.Dequeue().data / 100f;
            if (startOnChance == 0f)
            {
                startOnChance = -1f;
            }

            while (queue.Count > 0)
            {
                StructureData steamSpawn = queue.Dequeue();
                StructureData valveSpawn = queue.Dequeue();
                Cell cell = ec.CellFromPosition(steamSpawn.position);
                SteamValveController valveController = builder.InstatiateEnvironmentObject(steamValvePrefab.gameObject, cell, Direction.North).GetComponent<SteamValveController>();
                GameButtonBase valve = GameButton.Build(valvePre, ec, valveSpawn.position, valveSpawn.direction);
                generatedValves.Add(valve);
                valve.SetUp(valveController);
                valveController.Initialize(cell.position, steamSpawn.data);
                cell.HardCover(valveController.coverage);
                ec.QueueDijkstraMap((DijkstraMap)_spreadMap.GetValue(valveController));
            }
        }

        public void OnLoadingFinished(LevelBuilder _)
        {
            List<GameButtonBase> generatedValves = (List<GameButtonBase>)_generatedValves.GetValue(this);
            generatedValves.Shuffle();
            foreach (GameButtonBase valve in generatedValves)
            {
                valve.Set(UnityEngine.Random.value <= startOnChance);
            }
        }
    }
}
