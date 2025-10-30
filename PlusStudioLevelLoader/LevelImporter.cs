using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusStudioLevelLoader
{
    public static class LevelImporter
    {

        internal static T CreateRoomAsset<T>(BaldiRoomAsset info, bool padBorders) where T : RoomAsset
        {
            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = info.name;
            ((UnityEngine.Object)asset).name = info.name;
            RoomSettings rc = LevelLoaderPlugin.Instance.roomSettings[info.type];
            asset.category = rc.category;
            asset.type = rc.type;
            asset.doorMats = rc.doorMat;
            asset.roomFunctionContainer = rc.container;
            asset.color = rc.color;
            asset.mapMaterial = rc.mapMaterial;
            asset.florTex = LevelLoaderPlugin.RoomTextureFromAlias(info.textureContainer.floor);
            asset.wallTex = LevelLoaderPlugin.RoomTextureFromAlias(info.textureContainer.wall);
            asset.ceilTex = LevelLoaderPlugin.RoomTextureFromAlias(info.textureContainer.ceiling);
            asset.activity = new ActivityData();
            asset.hasActivity = info.activity != null;
            if (asset.hasActivity)
            {
                asset.activity.position = info.activity.position.ToUnity();
                asset.activity.prefab = LevelLoaderPlugin.Instance.activityAliases[info.activity.type];
                asset.activity.direction = (Direction)info.activity.direction;
            }
            for (int i = 0; i < info.cells.Count; i++)
            {
                RoomCellInfo cell = info.cells[i];
                asset.cells.Add(new CellData()
                {
                    pos=cell.position.ToInt(),
                    type=(int)cell.walls
                });
            }
            for (int i = 0; i < info.items.Count; i++)
            {
                asset.items.Add(new ItemData()
                {
                    item = LevelLoaderPlugin.Instance.itemObjects[info.items[i].item],
                    position = info.items[i].position.ToUnity()
                });
            }
            for (int i = 0; i < info.itemSpawns.Count; i++)
            {
                asset.itemSpawnPoints.Add(new ItemSpawnPoint()
                {
                    position = info.itemSpawns[i].position.ToUnity(),
                    weight = info.itemSpawns[i].weight
                });
            }
            for (int i = 0; i < info.entitySafeCells.Count; i++)
            {
                asset.entitySafeCells.Add(info.entitySafeCells[i].ToInt());
            }
            for (int i = 0; i < info.eventSafeCells.Count; i++)
            {
                asset.eventSafeCells.Add(info.eventSafeCells[i].ToInt());
            }
            for (int i = 0; i < info.secretCells.Count; i++)
            {
                asset.secretCells.Add(info.secretCells[i].ToInt());
            }
            for (int i = 0; i < info.standardLightCells.Count; i++)
            {
                asset.standardLightCells.Add(info.standardLightCells[i].ToInt());
            }
            for (int i = 0; i < info.potentialDoorPositions.Count; i++)
            {
                asset.potentialDoorPositions.Add(info.potentialDoorPositions[i].ToInt());
            }
            for (int i = 0; i < info.forcedDoorPositions.Count; i++)
            {
                asset.forcedDoorPositions.Add(info.forcedDoorPositions[i].ToInt());
            }
            for (int i = 0; i < info.posters.Count; i++)
            {
                PosterObject po = LevelLoaderPlugin.PosterFromAlias(info.posters[i].poster);
                if (po == null)
                {
                    Debug.LogWarning("Missing poster: " + info.posters[i].poster);
                    continue;
                }
                asset.posterDatas.Add(new PosterData()
                {
                    position = info.posters[i].position.ToInt(),
                    direction = (Direction)info.posters[i].direction,
                    poster = po
                });
            }
            for (int i = 0; i < info.lights.Count; i++)
            {
                asset.lights.Add(new LightSourceData()
                {
                    prefab = LevelLoaderPlugin.Instance.lightTransforms[info.lights[i].prefab],
                    color = info.lights[i].color.ToStandard(),
                    position = info.lights[i].position.ToInt(),
                    strength = info.lights[i].strength
                });
            }
            for (int i = 0; i < info.basicObjects.Count; i++)
            {
                asset.basicObjects.Add(new BasicObjectData()
                {
                    prefab = LevelLoaderPlugin.Instance.basicObjects[info.basicObjects[i].prefab].transform,
                    position = info.basicObjects[i].position.ToUnity(),
                    rotation = info.basicObjects[i].rotation.ToUnity(),
                });
            }
            asset.maxItemValue = info.maxItemValue;
            asset.windowChance = info.windowChance;
            asset.windowObject = LevelLoaderPlugin.Instance.windowObjects[info.windowType];
            asset.posterChance = info.posterChance;
            // mark as out of bounds if appropiate
            // (we do this by marking it as out of bounds and then unmarking it if we find a single cell that isn't secret)
            asset.offLimits = true;
            for (int i = 0; i < asset.cells.Count; i++)
            {
                if (asset.secretCells.FindIndex(x => x == asset.cells[i].pos) == -1)
                {
                    asset.offLimits = false;
                    break;
                }
            }

            // pad cells
            if (!padBorders) return asset;
            IntVector2 lowestCell = new IntVector2(int.MaxValue, int.MaxValue);
            IntVector2 highestCell = new IntVector2(0,0);
            for (int i = 0; i < asset.cells.Count; i++)
            {
                CellData cell = asset.cells[i];
                if ((cell.pos.x <= lowestCell.x) && (cell.pos.z <= lowestCell.z))
                {
                    lowestCell = cell.pos;
                }
                if ((cell.pos.x >= highestCell.x) && (cell.pos.z >= highestCell.z))
                {
                    highestCell = cell.pos;
                }
            }
            for (int x = lowestCell.x; x <= highestCell.x; x++)
            {
                for (int z = lowestCell.z; z <= highestCell.z; z++)
                {
                    IntVector2 cellAtPos = new IntVector2(x, z);
                    if (asset.cells.Find(c => c.pos == cellAtPos) == null)
                    {
                        asset.cells.Add(new CellData()
                        {
                            pos = cellAtPos,
                            roomId = 0,
                            type = 0
                        });
                        asset.secretCells.Add(cellAtPos);
                        asset.blockedWallCells.Add(cellAtPos);
                    }
                }
            }
            return asset;
        }

        /// <summary>
        /// Creates an ExtendedRoomAsset from the specified BaldiRoomAsset.
        /// </summary>
        /// <param name="info">The BaldiRoomAsset to construct the ExtendedRoomAsset from</param>
        /// <param name="addPadding">If true, padding will be added in the form of secret and blocked cells, commonly used for non-square special rooms.</param>
        /// <returns></returns>
        public static ExtendedRoomAsset CreateRoomAsset(BaldiRoomAsset info, bool addPadding = false)
        {
            ExtendedRoomAsset extendedAsset = CreateRoomAsset<ExtendedRoomAsset>(info, addPadding);
            for (int i = 0; i < info.cells.Count; i++)
            {
                if (info.cells[i].coverage != PlusCellCoverage.None)
                {
                    extendedAsset.AddCellCoverage(info.cells[i].position.ToInt(), (CellCoverage)info.cells[i].coverage);
                }
            }
            return extendedAsset;
        }

        /// <summary>
        /// Creates an RoomAsset from the specified BaldiRoomAsset. You almost never need to use this version.
        /// </summary>
        /// <param name="info">The BaldiRoomAsset to construct the RoomAsset from</param>
        /// <param name="addPadding">If true, padding will be added in the form of secret and blocked cells, commonly used for non-square special rooms.</param>
        /// <returns></returns>
        public static RoomAsset CreateVanillaRoomAsset(BaldiRoomAsset info, bool addPadding = false)
        {
            RoomAsset asset = CreateRoomAsset<RoomAsset>(info, addPadding);
            for (int i = 0; i < info.cells.Count; i++)
            {
                if (info.cells[i].coverage != PlusCellCoverage.None)
                {
                    asset.blockedWallCells.Add(info.cells[i].position.ToInt());
                }
            }
            return asset;
        }

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
            // thanks mystman
            scene.forcedNpcs = new NPC[0];
            scene.potentialNPCs = new List<WeightedNPC>();
            scene.potentialStickers = new WeightedSticker[]
            {
                new WeightedSticker()
                {
                    selection = Sticker.BaldiPraise,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.Stamina,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.InventorySlot,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.YtpMulitplier,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.Shadows,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.DoorStop,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.Silence,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.Reach,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.MapRange,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.Elevator,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.TimeExtension,
                    weight = 100,
                },
                new WeightedSticker()
                {
                    selection = Sticker.Stealth,
                    weight = 100,
                }
            };

            return scene;
        }

        /// <summary>
        /// Creates a SceneObject from the specified BaldiLevel.
        /// The manager has to be assigned manually, and it is advised to rename the created objects from their auto generated names.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create a LevelAsset from the specified BaldiLevel.
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public static LevelAsset LoadLevelAsset(BaldiLevel level)
        {
            LevelAsset asset = ScriptableObject.CreateInstance<LevelAsset>();
            asset.name = "LoadedLevelAsset_" + level.levelSize.x + "_" + level.levelSize.y + "_" + level.rooms.Count;
            asset.levelSize = level.levelSize.ToInt();
            asset.tile = new CellData[level.levelSize.x * level.levelSize.y];
            asset.seed = level.seed;
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
            // NOTE: the code below is copy and pasted for creating RoomAssets. Keep that in mind when modifying this.
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
                    itemSpawnPoints = level.rooms[i].itemSpawns.Select(x => new ItemSpawnPoint()
                    {
                        weight = x.weight,
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
                data.offLimits = (asset.tile.Count(x => (x.roomId == i) && (x.type != 16)) == data.secretCells.Count) && (data.secretCells.Count != 0);
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
                    structureData.data.Add(new StructureData(converter.prefabAliases.ContainsKey(info.prefab) ? converter.prefabAliases[info.prefab] : null, info.position.ToStandard(), (Direction)info.direction, info.data));
                }
                asset.structures.Add(structureData);
            }
            // handle random structures
            for (int i = 0; i < level.randomStructures.Count; i++)
            {
                StructureWithParameters structureParamData = new StructureWithParameters();
                LoaderStructureData converter = LevelLoaderPlugin.Instance.structureAliases[level.randomStructures[i].type];
                structureParamData.prefab = converter.structure;
                structureParamData.parameters = new StructureParameters();
                structureParamData.parameters.chance = level.randomStructures[i].info.chance.ToArray();
                structureParamData.parameters.minMax = level.randomStructures[i].info.minMax.Select(x => x.ToStandard()).ToArray();
                structureParamData.parameters.prefab = level.randomStructures[i].info.prefab.Select(x => new WeightedGameObject() { selection = converter.prefabAliases[x.prefab], weight = x.weight }).ToArray();
                asset.randomGenStructures.Add(structureParamData);
            }

            for (int i = 0; i < level.posters.Count; i++)
            {
                PosterObject po = LevelLoaderPlugin.PosterFromAlias(level.posters[i].poster);
                if (po == null)
                {
                    Debug.LogWarning("Missing poster: " + level.posters[i].poster);
                    continue;
                }
                asset.posters.Add(new PosterData()
                {
                    position = level.posters[i].position.ToInt(),
                    direction = (Direction)level.posters[i].direction,
                    poster = po
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
