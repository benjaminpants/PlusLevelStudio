using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusStudioLevelLoader
{
    public static class LevelImporter
    {

        public static SceneObject CreateEmptySceneObject()
        {
            SceneObject scene = ScriptableObject.CreateInstance<SceneObject>();
            scene.levelNo = -1;
            scene.levelTitle = "WIP";
            scene.extraAsset = ScriptableObject.CreateInstance<ExtraLevelDataAsset>();
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
            scene.extraAsset.lightMode = LightMode.Cumulative;
            scene.extraAsset.minLightColor = Color.black;
            scene.extraAsset.name = "LoadedExtraAsset_" + level.levelSize.x + "_" + level.levelSize.y + "_" + level.rooms.Count; ;
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
            for (int i = 0; i < level.rooms.Count; i++)
            {
                RoomSettings settings = LevelLoaderPlugin.Instance.roomSettings[level.rooms[i].type];
                asset.rooms.Add(new RoomData()
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
                    hasActivity = false,
                    roomFunctionContainer = settings.container,
                    activity = new ActivityData(),
                });
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
            return asset;
        }
    }
}
