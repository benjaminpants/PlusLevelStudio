using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Ingame
{
    public class Structure_PowerLeverEditor : Structure_PowerLever
    {
        LevelBuilder builder;
        public override void Initialize(EnvironmentController ec, StructureParameters parameters)
        {
            base.Initialize(ec, parameters);
            builder = GameObject.FindObjectsOfType<LevelBuilder>().First(); // MYSTMAN WHY
        }

        static FieldInfo _emergencyLightColor = AccessTools.Field(typeof(Structure_PowerLever), "emergencyLightColor");
        static FieldInfo _emergencyLightStrength = AccessTools.Field(typeof(Structure_PowerLever), "emergencyLightStrength");
        static FieldInfo _generatedEmergencyLights = AccessTools.Field(typeof(Structure_PowerLever), "generatedEmergencyLights");
        static FieldInfo _emergencyLightPrefab = AccessTools.Field(typeof(Structure_PowerLever), "emergencyLightPrefab");

        public override void Load(List<StructureData> data)
        {
            BreakerController.ResetStaticVariables();
            bool generatedBreaker = false;

            List<Cell> generatedEmergencyLights = (List<Cell>)_generatedEmergencyLights.GetValue(this);
            Color emergencyLightColor = (Color)_emergencyLightColor.GetValue(this);
            int emergencyLightStrength = (int)_emergencyLightStrength.GetValue(this);
            GameObject emergencyLightPrefab = (GameObject)_emergencyLightPrefab.GetValue(this);

            Queue<StructureData> queue = new Queue<StructureData>(data);
            while (queue.Count > 0)
            {
                StructureData baseData = queue.Dequeue();
                switch (baseData.data)
                {
                    case 0:
                        Cell lightCell = ec.CellFromPosition(baseData.position);
                        ec.GenerateLight(lightCell, emergencyLightColor, emergencyLightStrength);
                        builder.InstatiateEnvironmentObject(emergencyLightPrefab, lightCell, Direction.North);
                        lightCell.HardCover(CellCoverage.Up);
                        generatedEmergencyLights.Add(lightCell);
                        break;
                    default:
                        throw new NotImplementedException("Unknown type encountered when generating power levers (possible de-sync?): " + baseData.data);
                }
            }
            // If we haven't generated any breakers, we have to disable all of these ourselves, otherwise they'd all be on.
            if (!generatedBreaker)
            {
                for (int i = 0; i < generatedEmergencyLights.Count; i++)
                {
                    generatedEmergencyLights[i].SetLight(false);
                    generatedEmergencyLights[i].SetPower(true);
                }
            }
        }

        public override void OnGenerationFinished(LevelBuilder lb)
        {
            //base.OnGenerationFinished(lb);
        }
    }
}
