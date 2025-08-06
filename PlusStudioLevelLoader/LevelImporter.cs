using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlusStudioLevelLoader
{
    public static class LevelImporter
    {

        public static SceneObject CreateEmptySceneObject()
        {
            SceneObject scene = ScriptableObject.CreateInstance<SceneObject>();
            scene.levelNo = -1;
            scene.levelTitle = "WIP";
            scene.extraAsset = ScriptableObject.CreateInstance<ExtendedExtraLevelDataAsset>();
            scene.extraAsset.name = "WIPExtraAsset";
            scene.extraAsset.minLightColor = Color.white;
            scene.extraAsset.npcSpawnPoints = new List<IntVector2>();
            scene.name = "WIP";
            scene.skybox = LevelLoaderPlugin.Instance.assetMan.Get<Cubemap>("Cubemap_DayStandard");

            return scene;
        }

        public static SceneObject CreateSceneObject(BaldiLevel level)
        {
            SceneObject scene = CreateEmptySceneObject();
            scene.levelAsset = LoadLevelAsset(level);
            ExtendedExtraLevelDataAsset extendedAsset = (ExtendedExtraLevelDataAsset)scene.extraAsset;
            scene.extraAsset.lightMode = LightMode.Cumulative;
            scene.extraAsset.minLightColor = Color.black;
            scene.extraAsset.name = "LoadedExtraAsset_" + level.levelSize.x + "_" + level.levelSize.y + "_" + level.rooms.Count;
            for (int i = 0; i < level.npcs.Count; i++)
            {
                scene.extraAsset.npcsToSpawn.Add(LevelLoaderPlugin.Instance.npcAliases[level.npcs[i].npc]);
                scene.extraAsset.npcSpawnPoints.Add(level.npcs[i].position.ToInt());
            }
            scene.extraAsset.minEventGap = level.minRandomEventGap;
            scene.extraAsset.maxEventGap = level.maxRandomEventGap;
            scene.extraAsset.initialEventGap = level.initialRandomEventGap;
            extendedAsset.timeOutEvent = LevelLoaderPlugin.Instance.randomEventAliases["timeout"];
            extendedAsset.timeOutTime = level.timeLimit;
            extendedAsset.lightMode = (LightMode)level.lightMode;
            extendedAsset.minLightColor = level.minLightColor.ToStandard();
            scene.skybox = LevelLoaderPlugin.Instance.skyboxAliases[level.skybox];
            scene.levelTitle = level.levelTitle;
            return scene;
        }

        public static LevelAsset LoadLevelAsset(BaldiLevel level)
        {
            LevelAsset asset = ScriptableObject.CreateInstance<LevelAsset>();
            asset.name = "LoadedLevelAsset_" + level.levelSize.x + "_" + level.levelSize.y + "_" + level.rooms.Count;
            asset.levelSize = level.levelSize.ToInt();
            asset.tile = new CellData[level.levelSize.x * level.levelSize.y];
            for (int x = 0; x < level.levelSize.x; x++)
            {
                for (int y = 0; y < level.levelSize.y; y++)
                {
                    asset.tile[x * level.levelSize.y + y] = new CellData()
                    {
                        pos = new IntVector2(x, y),
                        type = level.cells[x, y].type,
                        roomId = Mathf.Max(level.cells[x, y].roomId - 1, 0)
                    };
                }
            }
            // TODO: move this to a seperate method, we'll have to implement ConvertFromData for ExtendedLevelAsset
            for (int i = 0; i < level.rooms.Count; i++)
            {
                RoomSettings settings = LevelLoaderPlugin.Instance.roomSettings[level.rooms[i].type];
                ExtendedRoomData data = new ExtendedRoomData()
                {
                    name = level.rooms[i].type + "_" + i,
                    florTex = LevelLoaderPlugin.RoomTextureFromAlias(level.rooms[i].textureContainer.floor),
                    wallTex = LevelLoaderPlugin.RoomTextureFromAlias(level.rooms[i].textureContainer.wall),
                    ceilTex = LevelLoaderPlugin.RoomTextureFromAlias(level.rooms[i].textureContainer.ceiling),
                    color = settings.color,
                    mapMaterial = settings.mapMaterial,
                    category = settings.category,
                    type = settings.type,
                    doorMats = settings.doorMat,
                    hasActivity = level.rooms[i].activity != null,
                    roomFunctionContainer = settings.container,
                    activity = new ActivityData(),
                    items = level.rooms[i].items.Select(x => new ItemData()
                    {
                        item = LevelLoaderPlugin.Instance.itemObjects[x.item],
                        position = x.position.ToUnity()
                    }).ToList(),
                    basicObjects = level.rooms[i].basicObjects.Select(x => new BasicObjectData()
                    {
                        position = x.position.ToUnity(),
                        rotation = x.rotation.ToUnity(),
                        prefab = LevelLoaderPlugin.Instance.basicObjects[x.prefab].transform
                    }).ToList()
                };
                if (data.hasActivity)
                {
                    data.activity.position = level.rooms[i].activity.position.ToUnity();
                    data.activity.prefab = LevelLoaderPlugin.Instance.activityAliases[level.rooms[i].activity.type];
                    data.activity.direction = (Direction)level.rooms[i].activity.direction;
                }
                CellData[] foundCells = asset.tile.Where(x => x.roomId == i).ToArray();
                for (int j = 0; j < foundCells.Length; j++)
                {
                    if (level.entitySafeCells[foundCells[j].pos.x, foundCells[j].pos.z])
                    {
                        data.entitySafeCells.Add(foundCells[j].pos);
                    }
                    if (level.eventSafeCells[foundCells[j].pos.x, foundCells[j].pos.z])
                    {
                        data.eventSafeCells.Add(foundCells[j].pos);
                    }
                    if (level.secretCells[foundCells[j].pos.x, foundCells[j].pos.z])
                    {
                        data.secretCells.Add(foundCells[j].pos);
                    }
                    if (level.coverage[foundCells[j].pos.x, foundCells[j].pos.z] != PlusCellCoverage.None)
                    {
                        data.AddCellCoverage(foundCells[j].pos, (CellCoverage)level.coverage[foundCells[j].pos.x, foundCells[j].pos.z]);
                    }
                }
                asset.rooms.Add(data);
            }
            asset.spawnDirection = (Direction)level.spawnDirection;
            asset.spawnPoint = level.spawnPoint.ToUnity();

            for (int i = 0; i < level.lights.Count; i++)
            {
                asset.lights.Add(new LightSourceData()
                {
                    color = level.lights[i].color.ToStandard(),
                    position = level.lights[i].position.ToInt(),
                    prefab = LevelLoaderPlugin.Instance.lightTransforms[level.lights[i].prefab],
                    strength = level.lights[i].strength,
                });
            }

            for (int i = 0; i < level.doors.Count; i++)
            {
                asset.doors.Add(new DoorData((int)level.doors[i].roomId - 1, LevelLoaderPlugin.Instance.doorPrefabs[level.doors[i].prefab], level.doors[i].position.ToInt(), (Direction)level.doors[i].direction));
            }
            for (int i = 0; i < level.windows.Count; i++)
            {
                asset.windows.Add(new WindowData()
                {
                    position = level.windows[i].position.ToInt(),
                    direction = (Direction)level.windows[i].direction,
                    window = LevelLoaderPlugin.Instance.windowObjects[level.windows[i].prefab]
                });
            }
            for (int i = 0; i < level.tileObjects.Count; i++)
            {
                asset.tbos.Add(new TileBasedObjectData()
                {
                    direction = (Direction)level.tileObjects[i].direction,
                    position = level.tileObjects[i].position.ToInt(),
                    prefab = LevelLoaderPlugin.Instance.tileBasedObjectPrefabs[level.tileObjects[i].prefab]
                });
            }
            for (int i = 0; i < level.exits.Count; i++)
            {
                asset.exits.Add(new ExitData()
                {
                    position = level.exits[i].position.ToInt(),
                    direction = (Direction)level.exits[i].direction,
                    spawn = level.exits[i].isSpawn,
                    prefab = LevelLoaderPlugin.Instance.exitDatas[level.exits[i].type].prefab,
                    room = LevelLoaderPlugin.Instance.exitDatas[level.exits[i].type].room
                });
            }
            for (int i = 0; i < level.structures.Count; i++)
            {
                StructureBuilderData structureData = new StructureBuilderData();
                LoaderStructureData converter = LevelLoaderPlugin.Instance.structureAliases[level.structures[i].type];
                structureData.prefab = converter.structure;
                for (int j = 0; j < level.structures[i].data.Count; j++)
                {
                    StructureDataInfo info = level.structures[i].data[j];
                    structureData.data.Add(new StructureData(converter.prefabAliases.ContainsKey(info.prefab) ? converter.prefabAliases[info.prefab] : null, info.position.ToInt(), (Direction)info.direction, info.data));
                }
                asset.structures.Add(structureData);
            }
            for (int i = 0; i < level.posters.Count; i++)
            {
                asset.posters.Add(new PosterData()
                {
                    position = level.posters[i].position.ToInt(),
                    direction = (Direction)level.posters[i].direction,
                    poster = LevelLoaderPlugin.Instance.posterAliases[level.posters[i].poster]
                });
            }
            for (int i = 0; i < level.randomEvents.Count; i++)
            {
                asset.events.Add(LevelLoaderPlugin.Instance.randomEventAliases[level.randomEvents[i]]);
            }
            return asset;
        }
    }
}
