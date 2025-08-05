using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

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
        static FieldInfo _generatedGauges = AccessTools.Field(typeof(Structure_PowerLever), "generatedGauges");
        static FieldInfo _generatedPowerLevers = AccessTools.Field(typeof(Structure_PowerLever), "generatedPowerLevers");
        static FieldInfo _emergencyLightPrefab = AccessTools.Field(typeof(Structure_PowerLever), "emergencyLightPrefab");
        static FieldInfo _poweredRooms = AccessTools.Field(typeof(Structure_PowerLever), "poweredRooms");
        static FieldInfo _startCoverage = AccessTools.Field(typeof(Structure_PowerLever), "startCoverage");
        static FieldInfo _breakerPrefab = AccessTools.Field(typeof(Structure_PowerLever), "breakerPrefab");
        static FieldInfo _maxPowerLevel = AccessTools.Field(typeof(Structure_PowerLever), "maxPowerLevel");
        static MethodInfo _BuildModel = AccessTools.Method(typeof(Structure_PowerLever), "BuildModel");

        float chanceForPoweredRoom = 0f;
        bool generatedBreaker = false;

        public void GenerateBreaker(Cell cell, Direction direction)
        {
            BreakerController breakerController = Instantiate<BreakerController>((BreakerController)_breakerPrefab.GetValue(this), cell.room.transform);
            breakerController.Initialize(ec, (List<PowerLeverController>)_generatedPowerLevers.GetValue(this), (List<PowerLeverGauge>)_generatedGauges.GetValue(this), (List<Cell>)_generatedEmergencyLights.GetValue(this), cell.position, (int)_maxPowerLevel.GetValue(this));
            breakerController.transform.position = cell.FloorWorldPosition;
            breakerController.transform.rotation = direction.ToRotation();
            cell.HardCover(breakerController.coverage.Rotated(direction));
            BreakerController.allBreakers.Add(breakerController);
        }

        public void GenerateLeverForRoom(RoomController chosenRoom, CableColor color, IntVector2 leverPosition, Direction leverDirection)
        {
            // a big blob of code mostly because we are not allowed to fail generating this lever
            List<Cell> list = new List<Cell>(chosenRoom.AllEventSafeCellsNoGarbage());
            list.FilterOutCellsThatDontFitCoverage((CellCoverage)_startCoverage.GetValue(this), CellCoverageType.All, false);
            if (list.Count <= 0)
            {
                Debug.LogWarning("Premade GenerateLeverForRoom failed the FitCoverage check, hacking using AllCells... (This will result in BUGGY cable generation...)");
                list = new List<Cell>(chosenRoom.cells); // "failing" is not something we can do here, so lets just hack our way around this
                list.FilterOutCellsThatDontFitCoverage((CellCoverage)_startCoverage.GetValue(this), CellCoverageType.All, false);
            }
            if (list.Count <= 0)
            {
                Debug.LogWarning("Premade GenerateLeverForRoom couldn't find ALL cells with filter????");
                list = new List<Cell>(chosenRoom.cells); // what the fuck did the user do to cause this
            }
            // in the original, a cell is grabbed at random, but to avoid using RNG here, lets try to find the closest cell to our lever to make the shortest path.
            Cell closestCell = null;
            float closestDistance = float.PositiveInfinity;
            for (int i = 0; i < list.Count; i++)
            {
                float distance = Vector2Int.Distance(new Vector2Int(list[i].position.x, list[i].position.z), new Vector2Int(leverPosition.x, leverPosition.z));
                if (distance < closestDistance)
                {
                    closestCell = list[i];
                    closestDistance = distance;
                }
            }
            if (closestCell == null) throw new Exception("Couldn't find closest cell for premade power levers??? What the fuck???");
            List<IntVector2> path;
            if (ec.FindPath(closestCell.position, leverPosition, PathType.Nav, out path))
            {
                _BuildModel.Invoke(this, new object[] { chosenRoom, color, path, leverDirection });
                return;
            }
            Debug.LogWarning("Couldn't create path for PowerLever???? Creating minimum path... (This will look weird ingame!)");
            path = new List<IntVector2>() { closestCell.position, leverPosition };
            _BuildModel.Invoke(this, new object[] { chosenRoom, color, path, leverDirection });
        }

        public override void Load(List<StructureData> data)
        {
            BreakerController.ResetStaticVariables();

            List<Cell> generatedEmergencyLights = (List<Cell>)_generatedEmergencyLights.GetValue(this);
            Color emergencyLightColor = (Color)_emergencyLightColor.GetValue(this);
            int emergencyLightStrength = (int)_emergencyLightStrength.GetValue(this);
            GameObject emergencyLightPrefab = (GameObject)_emergencyLightPrefab.GetValue(this);

            Queue<StructureData> queue = new Queue<StructureData>(data);

            _maxPowerLevel.SetValue(this, queue.Dequeue().data);
            chanceForPoweredRoom = queue.Dequeue().data / 100f;
            if (chanceForPoweredRoom == 0f)
            {
                chanceForPoweredRoom = -1f;
            }

            while (queue.Count > 0)
            {
                StructureData baseData = queue.Dequeue();
                switch (baseData.data)
                {
                    case 0: // emergency light
                        Cell lightCell = ec.CellFromPosition(baseData.position);
                        ec.GenerateLight(lightCell, emergencyLightColor, emergencyLightStrength);
                        builder.InstatiateEnvironmentObject(emergencyLightPrefab, lightCell, Direction.North);
                        lightCell.HardCover(CellCoverage.Up);
                        generatedEmergencyLights.Add(lightCell);
                        break;
                    case 1:
                        // END POINT IS WHERE THE *LEVER* SHOULD BE
                        IntVector2 leverPosition = baseData.position;
                        Direction leverDirection = baseData.direction;
                        CableColor color = (CableColor)(queue.Dequeue().data);
                        RoomController room = ec.rooms[queue.Dequeue().data];
                        GenerateLeverForRoom(room, color, leverPosition, leverDirection);
                        break;
                    case 2:
                        IntVector2 breakerPosition = baseData.position;
                        Direction breakerdirection = baseData.direction;
                        GenerateBreaker(ec.CellFromPosition(breakerPosition), breakerdirection);
                        generatedBreaker = true;
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

        public void RandomizePoweredRooms()
        {
            int maxPowerLevel = (int)_maxPowerLevel.GetValue(this);
            List<RoomController> poweredRooms = new List<RoomController>((List<RoomController>)_poweredRooms.GetValue(this));
            if (!generatedBreaker)
            {
                maxPowerLevel = poweredRooms.Count;
            }
            poweredRooms.Shuffle();
            int activelyPowered = 0;
            foreach (RoomController room in poweredRooms)
            {
                if ((UnityEngine.Random.value <= chanceForPoweredRoom) && (!(activelyPowered >= (maxPowerLevel - 1))))
                {
                    activelyPowered++;
                    room.SetPower(true);
                }
                else
                {
                    room.SetPower(false);
                }
            }
        }

        public void OnLoadingFinished(LevelLoader ll)
        {
            RandomizePoweredRooms();
        }

        public override void OnGenerationFinished(LevelBuilder lb)
        {
            
        }
    }
}
