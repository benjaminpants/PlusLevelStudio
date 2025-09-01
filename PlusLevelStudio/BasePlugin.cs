using BepInEx;
using HarmonyLib;
using System;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using System.Collections;
using UnityEngine;
using MTM101BaldAPI.Registers;
using System.Linq;
using UnityEngine.SceneManagement;
using MTM101BaldAPI.UI;
using PlusLevelStudio.Editor;
using System.Collections.Generic;
using System.IO;
using MTM101BaldAPI.Reflection;
using PlusLevelStudio.UI;
using PlusLevelStudio.Editor.Tools;
using TMPro;
using PlusStudioLevelLoader;
using PlusStudioLevelFormat;
using MTM101BaldAPI.ObjectCreation;
using PlusLevelStudio.Editor.GlobalSettingsMenus;
using PlusLevelStudio.Ingame;
using PlusLevelStudio.Editor.ModeSettings;
using MoonSharp.Interpreter;
using PlusLevelStudio.Lua;
using PlusLevelStudio.Editor.Tools.Customs;

namespace PlusLevelStudio
{
    [BepInPlugin("mtm101.rulerp.baldiplus.levelstudio", "Plus Level Studio", "1.0.0.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    [BepInDependency("mtm101.rulerp.baldiplus.levelstudioloader")]
    public class LevelStudioPlugin : BaseUnityPlugin
    {
        public static LevelStudioPlugin Instance;
        public bool isFucked = false;
        public AssetManager assetMan = new AssetManager();
        // a rare sight, seeing two asset mans. Usually, I'd combine these into one asset manager.
        // however, I want the code for creating UI to be as simple as possible to write
        // and having to prepend some prefix like "UI/" would be really annoying
        public AssetManager uiAssetMan = new AssetManager();
        public List<string> editorTracks = new List<string>();
        public Dictionary<string, Texture2D> lightmaps = new Dictionary<string, Texture2D>();
        public const int editorInteractableLayer = 13; // CollidableEntities
        public const int editorInteractableLayerMask = 1 << editorInteractableLayer;

        public const int editorHandleLayer = 12; // ClickableEntities
        public const int editorHandleLayerMask = 1 << editorHandleLayer;

        public Dictionary<string, DoorDisplay> doorDisplays = new Dictionary<string, DoorDisplay>();
        public Dictionary<string, DoorIngameStatus> doorIngameStatus = new Dictionary<string, DoorIngameStatus>();
        public Dictionary<string, DoorDisplay> windowDisplays = new Dictionary<string, DoorDisplay>();
        public Dictionary<string, GameObject> exitDisplays = new Dictionary<string, GameObject>();
        public Dictionary<string, EditorBasicObject> basicObjectDisplays = new Dictionary<string, EditorBasicObject>();
        public Dictionary<string, GameObject> activityDisplays = new Dictionary<string, GameObject>();
        public Dictionary<string, Type> structureTypes = new Dictionary<string, Type>();
        public Dictionary<string, Type> markerTypes = new Dictionary<string, Type>();
        public Dictionary<string, GameObject> genericStructureDisplays = new Dictionary<string, GameObject>();
        public Dictionary<string, GameObject> genericMarkerDisplays = new Dictionary<string, GameObject>();
        public Dictionary<string, GameObject> npcDisplays = new Dictionary<string, GameObject>();
        public List<string> selectableTextures = new List<string>();
        public List<string> selectableSkyboxes = new List<string>();
        public Dictionary<string, Sprite> eventSprites = new Dictionary<string, Sprite>();
        public Dictionary<string, Sprite> skyboxSprites = new Dictionary<string, Sprite>();
        public Dictionary<string, EditorRoomVisualManager> roomVisuals = new Dictionary<string, EditorRoomVisualManager>();
        public Dictionary<string, EditorGameMode> gameModeAliases = new Dictionary<string, EditorGameMode>();
        public GameObject pickupVisual;
        public GameObject posterVisual;
        public GameObject wallVisual;
        public GameObject wallRemoveVisual;

        public Dictionary<string, EditorMode> modes = new Dictionary<string, EditorMode>();

        public static string playableLevelPath => Path.Combine(basePath, "Playables");
        public static string basePath => Path.Combine(Application.persistentDataPath, "Level Studio");
        public static string levelFilePath => Path.Combine(basePath, "Editor Levels");
        public static string oldLevelFilePath => Path.Combine(Application.persistentDataPath, "Custom Levels");
        public static string levelExportPath => Path.Combine(basePath, "Exports");
        public static string luaPath => Path.Combine(basePath, "LuaScripts");

        public static string customContentPath => Path.Combine(basePath, "User Content");
        public static string customTexturePath => Path.Combine(customContentPath, "Textures");
        public static string customPostersPath => Path.Combine(customContentPath, "Posters");

        private Dictionary<Texture2D, Sprite> smallIconsFromTextures = new Dictionary<Texture2D, Sprite>();

        private Texture2D baldiSaysTexture;
        private Texture2D chalkTexture;
        private Texture2D bulletinTexture;
        // TODO: move to API?
        internal PosterObject GenerateBaldiSaysPoster(string name, string text)
        {
            PosterObject newPoster = ScriptableObject.CreateInstance<PosterObject>();
            newPoster.name = name;
            newPoster.baseTexture = baldiSaysTexture;
            newPoster.textData = new PosterTextData[]
            {
                new PosterTextData()
                {
                    alignment = TextAlignmentOptions.Center,
                    color = Color.black,
                    font = BaldiFonts.ComicSans18.FontAsset(),
                    fontSize = (int)BaldiFonts.ComicSans18.FontSize(),
                    position = new IntVector2(110,57),
                    size = new IntVector2(128,192),
                    style = FontStyles.Normal,
                    textKey = text
                }
            };
            return newPoster;
        }

        internal PosterObject GenerateChalkPoster(string name, string text)
        {
            PosterObject newPoster = ScriptableObject.CreateInstance<PosterObject>();
            newPoster.name = name;
            newPoster.baseTexture = chalkTexture;
            newPoster.textData = new PosterTextData[]
            {
                new PosterTextData()
                {
                    alignment = TextAlignmentOptions.Center,
                    color = Color.white,
                    font = BaldiFonts.SmoothComicSans24.FontAsset(),
                    fontSize = (int)BaldiFonts.SmoothComicSans24.FontSize(),
                    position = new IntVector2(24,88),
                    size = new IntVector2(208,128),
                    style = FontStyles.Normal,
                    textKey = text
                }
            };
            return newPoster;
        }

        internal PosterObject GenerateBulletInPoster(string name, string text, BaldiFonts font)
        {
            PosterObject newPoster = ScriptableObject.CreateInstance<PosterObject>();
            newPoster.name = name;
            newPoster.baseTexture = bulletinTexture;
            newPoster.textData = new PosterTextData[]
            {
                new PosterTextData()
                {
                    alignment = TextAlignmentOptions.Center,
                    color = Color.black,
                    font = font.FontAsset(),
                    fontSize = (int)font.FontSize(),
                    position = new IntVector2(68,58),
                    size = new IntVector2(120,144),
                    style = FontStyles.Normal,
                    textKey = text
                }
            };
            return newPoster;
        }

        public Sprite GenerateOrGetSmallPosterSprite(PosterObject obj)
        {
            if (smallIconsFromTextures.ContainsKey(obj.baseTexture))
            {
                return smallIconsFromTextures[obj.baseTexture];
            }
            Texture2D smallTex = new Texture2D(32,32, TextureFormat.RGBA32, false);
            smallTex.filterMode = FilterMode.Point;
            smallTex.name = obj.baseTexture.name + "_Tiny";
            Texture2D texToCopy = obj.baseTexture;
            if (!obj.baseTexture.isReadable)
            {
                texToCopy = obj.baseTexture.MakeReadableCopy(false);
            }
            Color[] colors = MaterialModifier.GetColorsForTileTexture(texToCopy, 32);
            smallTex.SetPixels(colors);
            smallTex.Apply();
            Sprite generatedSprite = AssetLoader.SpriteFromTexture2D(smallTex, 1f);
            smallIconsFromTextures.Add(obj.baseTexture, generatedSprite);
            return generatedSprite;
        }

        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.levelstudio");
            LoadingEvents.RegisterOnAssetsLoaded(Info, LoadAssets(), LoadingEventOrder.Start);
            LoadingEvents.RegisterOnAssetsLoaded(Info, FindObjectsAndSetupEditor(), LoadingEventOrder.Pre);
            LoadingEvents.RegisterOnAssetsLoaded(Info, SetupModes(), LoadingEventOrder.Post);
            harmony.PatchAllConditionals();
            UserData.RegisterAssembly();
            if (Directory.Exists(oldLevelFilePath))
            {
                string[] levels = Directory.GetFiles(oldLevelFilePath, "*.ebpl");
                Directory.CreateDirectory(Path.Combine(oldLevelFilePath, "Editor Levels"));
                for (int i = 0; i < levels.Length; i++)
                {
                    File.Move(levels[i], Path.Combine(Path.Combine(oldLevelFilePath, "Editor Levels"), Path.GetFileName(levels[i])));
                }
                Directory.Move(oldLevelFilePath, basePath);
            }

            Directory.CreateDirectory(levelFilePath); // this will also create the base path
            Directory.CreateDirectory(levelExportPath);
            Directory.CreateDirectory(playableLevelPath);
            Directory.CreateDirectory(luaPath);
            Directory.CreateDirectory(customTexturePath); // this will also create the custom content path
            Directory.CreateDirectory(customPostersPath);

            EditorController.maxUndos = Config.Bind("General",
                "Max Undos",
                15,
                "Determines the maximum amount of undos. 2 is the minimum, 0 or below will allow for infinite undos.\nNote that the higher this number is, the more memory the game will consume.").Value;
            if (EditorController.maxUndos <= 0)
            {
                EditorController.maxUndos = int.MaxValue;
            }
            EditorController.maxUndos = Mathf.Max(EditorController.maxUndos, 2);
        }

        void AddSolidColorLightmap(string name, Color color)
        {
            Texture2D tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            Color[] colors = new Color[256 * 256];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            tex.SetPixels(0, 0, 256, 256, colors);
            tex.Apply();
            lightmaps.Add(name, tex);
        }

        public StructureLocation ConstructStructureOfType(string type)
        {
            StructureLocation structure = (StructureLocation)LevelStudioPlugin.Instance.structureTypes[type].GetConstructor(new Type[0]).Invoke(new object[0]);
            structure.type = type;
            return structure;
        }

        public MarkerLocation ConstructMarkerOfType(string type)
        {
            MarkerLocation marker = (MarkerLocation)LevelStudioPlugin.Instance.markerTypes[type].GetConstructor(new Type[0]).Invoke(new object[0]);
            marker.type = type;
            return marker;
        }

        public IEnumerator LoadEditorScene(string modeToLoad, string pathToLoad = null, string loadedLevel = null)
        {
            while (Singleton<AdditiveSceneManager>.Instance.Busy)
            {
                yield return null;
            }
            Singleton<AdditiveSceneManager>.Instance.LoadScene("Game");
            while (Singleton<AdditiveSceneManager>.Instance.Busy)
            {
                yield return null;
            }
            GameObject.Destroy(GameObject.FindObjectOfType<GameInitializer>());
            Shader.SetGlobalTexture("_Skybox", Resources.FindObjectsOfTypeAll<Cubemap>().First(x => x.name == "Cubemap_DayStandard"));
            Shader.SetGlobalColor("_SkyboxColor", Color.white);
            Shader.SetGlobalColor("_FogColor", Color.white);
            Shader.SetGlobalFloat("_FogStartDistance", 5f);
            Shader.SetGlobalFloat("_FogMaxDistance", 100f);
            Shader.SetGlobalFloat("_FogStrength", 0f);

            EditorMode targetMode = modes[modeToLoad];

            EditorController editorController = GameObject.Instantiate<EditorController>(targetMode.prefab);

            editorController.currentMode = targetMode;

            editorController.EditorModeAssigned();

            if (pathToLoad != null)
            {
                editorController.LoadEditorLevelFromFile(pathToLoad);
            }
            if (loadedLevel != null)
            {
                EditorController.lastPlayedLevel = loadedLevel;
                editorController.currentFileName = loadedLevel;
            }

        }

        public void GoToEditor(string mode)
        {
            StartCoroutine(LoadEditorScene(mode));
        }

        public static bool editorModesDefined = false;

        IEnumerator SetupModes()
        {
            yield return 3;
            yield return "Setting up editor gamemode definitions and managers...";

            string settingsPagePath = Path.Combine(AssetLoader.GetModPath(this), "Data", "UI", "ModeSettings");

            gameModeAliases.Add("standard", new MainGameMode()
            {
                prefab=assetMan.Get<BaseGameManager>("EditorMainGameManager"),
                hasSettingsPage = true,
                settingsPagePath = Path.Combine(settingsPagePath, "MainSettings.json"),
                settingsPageType = typeof(MainModeSettingsPageUIExchangeHandler),
                nameKey ="Ed_GameMode_Standard",
                descKey="Ed_GameMode_Standard_Desc"
            });

            EditorGrappleChallengeManager editorGrappleChallenge = GameObject.Instantiate<GrappleChallengeManager>(Resources.FindObjectsOfTypeAll<GrappleChallengeManager>().First(x => x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform).gameObject.SwapComponent<GrappleChallengeManager, EditorGrappleChallengeManager>();
            editorGrappleChallenge.name = "EditorGrappleChallengeManager";

            EditorStealthyChallengeManager editorStealthyChallenge = GameObject.Instantiate<StealthyChallengeManager>(Resources.FindObjectsOfTypeAll<StealthyChallengeManager>().First(x => x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform).gameObject.SwapComponent<StealthyChallengeManager, EditorStealthyChallengeManager>();
            editorStealthyChallenge.name = "EditorStealthyChallengeManager";

            EditorSpeedyChallengeManager editorSpeedyChallenge = GameObject.Instantiate<SpeedyChallengeManager>(Resources.FindObjectsOfTypeAll<SpeedyChallengeManager>().First(x => x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform).gameObject.SwapComponent<SpeedyChallengeManager, EditorSpeedyChallengeManager>();
            editorSpeedyChallenge.name = "EditorSpeedyChallengeManager";

            gameModeAliases.Add("grapple", new EditorGameMode()
            {
                prefab = editorGrappleChallenge,
                nameKey = "Ed_GameMode_Grapple",
                descKey = "Ed_GameMode_Grapple_Desc"
            });

            gameModeAliases.Add("stealthy", new StealthyGameMode()
            {
                prefab = editorStealthyChallenge,
                nameKey = "Ed_GameMode_Stealthy",
                descKey = "Ed_GameMode_Stealthy_Desc",
                hasSettingsPage = true,
                settingsPagePath = Path.Combine(settingsPagePath, "StealthySettings.json"),
                settingsPageType = typeof(StealthyChallengeSettingsPageUIExchangeHandler),
            });

            gameModeAliases.Add("speedy", new EditorGameMode()
            {
                prefab = editorSpeedyChallenge,
                nameKey = "Ed_GameMode_Speedy",
                descKey = "Ed_GameMode_Speedy_Desc"
            });

            yield return "Setting up CustomChallengeManager...";
            CustomChallengeManager luaManager = new BaseGameManagerBuilder<CustomChallengeManager>()
                .SetLevelNumber(0)
                .SetNPCSpawnMode(GameManagerNPCAutomaticSpawn.Never)
                .SetObjectName("CustomChallengeManager")
                .Build();

            gameModeAliases.Add("custom", new CustomChallengeGameMode()
            {
                prefab = luaManager,
                nameKey = "Ed_GameMode_Custom",
                descKey = "Ed_GameMode_Custom_Desc",
                hasSettingsPage = true,
                settingsPagePath = Path.Combine(settingsPagePath, "CustomChallengeSettings.json"),
                settingsPageType = typeof(CustomChallengeSettingsPageUIExchangeHandler),
            });


            yield return "Setting up editor modes...";
            // setup modes

            GlobalStructurePage factoryBoxStructurePage = new GlobalStructurePage()
            {
                nameKey = "Ed_GlobalStructure_FactoryBox_Title",
                descKey = "Ed_GlobalStructure_FactoryBox_Desc",
                structureToSpawn = "factorybox"
            };

            string editorModePath = Path.Combine(AssetLoader.GetModPath(this), "Data", "UI", "GlobalPages");

            // full mode
            EditorMode fullMode = new EditorMode()
            {
                id = "full",
                availableTools = new Dictionary<string, List<EditorTool>>(),
                categoryOrder = new string[] {
                    "rooms",
                    "doors",
                    "npcs",
                    "items",
                    "activities",
                    "objects",
                    "structures",
                    "lights",
                    "posters",
                    "tools"
                },
                defaultTools = new string[] { "room_hall", "room_class", "room_faculty", "room_office", "light_fluorescent", "door_swinging", "door_standard", "merge", "delete" },
                prefab = assetMan.Get<EditorController>("MainEditorController"),
                vanillaComplaint = false,
                pages = new List<EditorGlobalPage>()
                {
                    new EditorGlobalPage()
                    {
                        filePath = Path.Combine(editorModePath, "MetaSettings.json"),
                        managerType = typeof(MetaSettingsExchangeHandler),
                        pageName = "MetaSettings",
                        pageKey = "Ed_GlobalPage_MetaSettings"
                    },
                    new EditorGlobalPage()
                    {
                        filePath = Path.Combine(editorModePath, "LevelSettings.json"),
                        managerType = typeof(LevelSettingsExchangeHandler),
                        pageName = "LevelSettings",
                        pageKey = "Ed_GlobalPage_LevelSettings"
                    },
                    new EditorGlobalPage()
                    {
                        filePath = Path.Combine(editorModePath, "VisualAndLights.json"),
                        managerType = typeof(VisualsAndLightsUIExchangeHandler),
                        pageName = "VisualAndLightSettings",
                        pageKey = "Ed_GlobalPage_VisualSettings"
                    },
                    new EditorGlobalPage()
                    {
                        filePath = Path.Combine(editorModePath, "GlobalStructures.json"),
                        managerType = typeof(GlobalStructuresExchangeHandler),
                        pageName = "GlobalStructures",
                        pageKey = "Ed_GlobalPage_GlobalStructures"
                    },
                    new EditorGlobalPage()
                    {
                        filePath = Path.Combine(editorModePath, "ModeSettings.json"),
                        managerType = typeof(ModeSettingsUIExchangeHandler),
                        pageName = "ModeSettings",
                        pageKey = "Ed_GlobalPage_ModeSettings"
                    }
                },
                globalStructures = new List<GlobalStructurePage>()
                {
                    factoryBoxStructurePage
                },
                availableGameModes = new List<string>()
                {
                    "standard",
                    "grapple",
                    "stealthy",
                    "speedy",
                    "custom"
                }
            };

            EditorInterfaceModes.AddVanillaRooms(fullMode);
            EditorInterfaceModes.AddVanillaDoors(fullMode);
            EditorInterfaceModes.AddVanillaNPCs(fullMode);
            EditorInterfaceModes.AddVanillaItems(fullMode);
            EditorInterfaceModes.AddVanillaActivities(fullMode, true);
            EditorInterfaceModes.AddVanillaObjects(fullMode);
            EditorInterfaceModes.AddVanillaStructures(fullMode, true);
            EditorInterfaceModes.AddVanillaLights(fullMode);
            EditorInterfaceModes.AddVanillaPosters(fullMode);
            EditorInterfaceModes.AddToolsToCategory(fullMode, "posters", new EditorTool[]
            {
                new CustomPosterTool(),
                new BaldiSaysPosterTool(),
                new ChalkboardPosterTool(),
                new BulletinBoardPosterTool(),
                new BulletinBoardSmallPosterTool()
            });
            EditorInterfaceModes.AddVanillaToolTools(fullMode);
            EditorInterfaceModes.AddVanillaEvents(fullMode, true);

            modes.Add("full", fullMode);

            CompliantEditorController compliantEditorController = GameObject.Instantiate<EditorController>(assetMan.Get<EditorController>("MainEditorController"), MTM101BaldiDevAPI.prefabTransform).gameObject.SwapComponent<EditorController, CompliantEditorController>();
            compliantEditorController.name = "CompliantEditorController";

            EditorMode complaintMode = new EditorMode()
            {
                id = "compliant",
                availableTools = new Dictionary<string, List<EditorTool>>(),
                categoryOrder = new string[] {
                    "rooms",
                    "doors",
                    "npcs",
                    "items",
                    "activities",
                    "objects",
                    "structures",
                    "lights",
                    "posters",
                    "tools"
                },
                pages = new List<EditorGlobalPage>()
                {
                    new EditorGlobalPage()
                    {
                        filePath = Path.Combine(editorModePath, "LevelSettings.json"),
                        managerType = typeof(LevelSettingsExchangeHandler),
                        pageName = "LevelSettings",
                        pageKey = "Ed_GlobalPage_LevelSettings"
                    },
                    new EditorGlobalPage()
                    {
                        filePath = Path.Combine(editorModePath, "VisualAndLights.json"),
                        managerType = typeof(VisualsAndLightsUIExchangeHandler),
                        pageName = "VisualAndLightSettings",
                        pageKey = "Ed_GlobalPage_VisualSettings"
                    },
                    new EditorGlobalPage()
                    {
                        filePath = Path.Combine(editorModePath, "GlobalStructures.json"),
                        managerType = typeof(GlobalStructuresExchangeHandler),
                        pageName = "GlobalStructures",
                        pageKey = "Ed_GlobalPage_GlobalStructures"
                    },
                },
                globalStructures = new List<GlobalStructurePage>()
                {
                    
                },
                defaultTools = new string[] { "room_hall", "room_class", "room_faculty", "room_office", "light_fluorescent", "door_swinging", "door_standard", "merge", "delete" },
                vanillaComplaint = true,
                prefab = compliantEditorController,
                availableGameModes = new List<string>()
                {
                    "standard",
                }
            };

            EditorInterfaceModes.AddVanillaRooms(complaintMode);
            EditorInterfaceModes.AddVanillaDoors(complaintMode);
            EditorInterfaceModes.AddVanillaNPCs(complaintMode);
            EditorInterfaceModes.AddVanillaItems(complaintMode);
            EditorInterfaceModes.AddVanillaActivities(complaintMode, false);
            EditorInterfaceModes.AddVanillaObjects(complaintMode);
            EditorInterfaceModes.AddVanillaStructures(complaintMode, false);
            EditorInterfaceModes.AddVanillaLights(complaintMode);
            EditorInterfaceModes.AddVanillaPosters(complaintMode);
            EditorInterfaceModes.AddVanillaToolTools(complaintMode);
            EditorInterfaceModes.AddVanillaEvents(complaintMode, false);

            modes.Add("compliant", complaintMode);

            RoomEditorController rce = GameObject.Instantiate<EditorController>(assetMan.Get<EditorController>("MainEditorController"), MTM101BaldiDevAPI.prefabTransform).gameObject.SwapComponent<EditorController, RoomEditorController>();
            rce.name = "RoomEditorController";

            EditorMode roomsMode = new EditorMode()
            {
                id = "rooms",
                availableTools = new Dictionary<string, List<EditorTool>>(),
                categoryOrder = new string[] {
                    "rooms",
                    "lights",
                    "activities",
                    "objects",
                    "posters",
                    "items",
                    "tools"
                },
                defaultTools = new string[] { "room_class", "room_faculty", "room_office", "technical_potentialdoor", "technical_lightspot", "technical_nosafe", "itemspawn_100", "merge", "delete" },
                vanillaComplaint = true,
                allowOutOfRoomObjects = false,
                caresAboutSpawn = false,
                prefab = rce,
                pages = new List<EditorGlobalPage>()
                {
                    new EditorGlobalPage()
                    {
                        filePath = Path.Combine(editorModePath, "VisualAndLights.json"),
                        managerType = typeof(VisualsAndLightsUIExchangeHandler),
                        pageName = "VisualAndLightSettings",
                        pageKey = "Ed_GlobalPage_LightPreview"
                    },
                },
                availableGameModes = new List<string>()
                {
                    "standard",
                }
            };

            EditorInterfaceModes.AddVanillaRooms(roomsMode);
            roomsMode.availableTools["rooms"].RemoveAt(roomsMode.availableTools["rooms"].FindIndex(x => x.id == "room_hall")); // because halls aren't supported quite yet
            EditorInterfaceModes.AddVanillaObjects(roomsMode);
            EditorInterfaceModes.AddVanillaActivities(roomsMode, false);
            EditorInterfaceModes.AddToolsToCategory(roomsMode, "items", new EditorTool[]
            {
                new ItemSpawnTool(100),
                new ItemSpawnTool(50),
                new ItemSpawnTool(25)
            }, true);
            EditorInterfaceModes.AddVanillaItems(roomsMode);
            EditorInterfaceModes.AddVanillaPosters(roomsMode);
            EditorInterfaceModes.AddToolToCategory(roomsMode, "lights", new PointTechnicalStructureTool("lightspot"), true);
            EditorInterfaceModes.AddVanillaLights(roomsMode);

            EditorInterfaceModes.AddToolsToCategory(roomsMode, "tools", new EditorTool[]
            {
                new PointTechnicalStructureTool("potentialdoor"),
                new PointTechnicalStructureTool("forceddoor"),
                new PointTechnicalStructureTool("nosafe"),
            }, true);
            EditorInterfaceModes.AddVanillaToolTools(roomsMode);

            modes.Add("rooms", roomsMode);

            for (int i = 0; i < EditorInterfaceModes.toCallAfterEditorMode.Count; i++)
            {
                foreach (EditorMode mode in modes.Values)
                {
                    EditorInterfaceModes.toCallAfterEditorMode[i](mode, mode.vanillaComplaint);
                }
            }

            editorModesDefined = true;
        }

        IEnumerator FindObjectsAndSetupEditor()
        {
            List<Direction> directions = Directions.All();
            yield return 14;
            yield return "Grabbing necessary resources...";
            assetMan.Add<Mesh>("Quad", Resources.FindObjectsOfTypeAll<Mesh>().First(x => x.GetInstanceID() >= 0 && x.name == "Quad"));
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>().Where(x => x.GetInstanceID() >= 0).ToArray();
            assetMan.Add<Material>("tileAlpha", materials.First(x => x.name == "TileBase_Alpha"));
            assetMan.Add<Material>("spriteBillboard", materials.First(x => x.name == "SpriteStandard_Billboard"));
            assetMan.Add<Material>("doorMask", materials.First(x => x.name == "DoorMask"));
            assetMan.Add<Material>("swingingDoorMask", materials.First(x => x.name == "SwingDoorMask"));
            assetMan.Add<Material>("SwingingDoorMat", materials.First(x => x.name == "SwingDoorTexture_Closed"));
            assetMan.Add<Material>("CoinDoorMat", materials.First(x => x.name == "CoinDoor"));
            assetMan.Add<Material>("AutoDoorMask", materials.First(x => x.name == "AutoDoorMask"));
            assetMan.Add<Material>("AutoDoorMat", materials.First(x => x.name == "AutoDoor_Closed"));
            assetMan.Add<Material>("FlapDoorMask", materials.First(x => x.name == "FlapDoorMask"));
            assetMan.Add<Material>("FlapDoorMat", materials.First(x => x.name == "FlapDoorTexture_Closed"));

            assetMan.Add<Material>("OneWayRight", materials.First(x => x.name == "SwingDoorRightWay_Closed"));
            assetMan.Add<Material>("OneWayWrong", materials.First(x => x.name == "SwingDoorTextureOneWay_Closed"));
            Texture2D[] allTextures = Resources.FindObjectsOfTypeAll<Texture2D>().Where(x => x.GetInstanceID() >= 0).ToArray();
            baldiSaysTexture = allTextures.First(x => x.name == "BaldiSpeaksPoster");
            chalkTexture = allTextures.First(x => x.name == "chk_blank");
            bulletinTexture = allTextures.First(x => x.name == "BulletinBoard_Blank");

            string[] cableNames = Enum.GetNames(typeof(CableColor));
            for (int i = 0; i < cableNames.Length; i++)
            {
                PowerLeverLocation.cableTex.Add((CableColor)Enum.Parse(typeof(CableColor), cableNames[i]), AssetLoader.TextureFromMod(this, "Editor", "CableTextures", cableNames[i] + ".png"));
            }


            yield return "Finding GameCamera...";
            assetMan.Add<GameCamera>("gameCam", Resources.FindObjectsOfTypeAll<GameCamera>().First());
            yield return "Setting up materials...";
            Material gridMat = new Material(assetMan.Get<Material>("tileAlpha"));
            gridMat.name = "EditorGridMaterial";
            gridMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "FloorGrid.png"));
            gridMat.SetTexture("_LightMap", lightmaps["white"]);

            Material selectMat = new Material(gridMat);
            selectMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "FloorSelect.png"));
            selectMat.name = "EditorSelectMaterial";

            Material arrowMat = new Material(gridMat);
            arrowMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "FloorArrow.png"));
            arrowMat.name = "EditorArrowMaterial";

            Material handleArrowMat = new Material(gridMat);
            handleArrowMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "3DArrow.png"));
            handleArrowMat.name = "Editor3DArrowMaterial";

            Material selectorLatticeMat = new Material(gridMat);
            selectorLatticeMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "SelectorLattice.png"));
            selectorLatticeMat.name = "EditorSelectorLatticeMaterial";

            Material silentDoorMat = new Material(assetMan.Get<Material>("SwingingDoorMat"));
            silentDoorMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "SwingDoorSilent.png"));
            silentDoorMat.name = "SilentSwingDoorDisplayMat";
            silentDoorMat.MarkAsNeverUnload();

            Material gridArrowMat = new Material(gridMat);
            gridArrowMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "GridArrow.png"));
            gridArrowMat.name = "GridArrowMaterial";

            Material wallAddMat = new Material(gridMat);
            wallAddMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "WallAdd.png"));
            wallAddMat.name = "WallAddMaterial";

            Material wallRemoveMat = new Material(wallAddMat);
            wallRemoveMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "WallRemove.png"));
            wallRemoveMat.name = "WallRemoveMaterial";

            Material spawnpointMat = new Material(assetMan.Get<Material>("tileAlpha"));
            spawnpointMat.name = "SpawnpointMat";
            spawnpointMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "SpawnpointSprite.png"));
            spawnpointMat.SetTexture("_LightMap", lightmaps["white"]);

            Material forcedDoorMat = new Material(assetMan.Get<Material>("tileAlpha"));
            forcedDoorMat.name = "ForcedDoorMat";
            forcedDoorMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "ForcedDoor.png"));
            forcedDoorMat.SetTexture("_LightMap", lightmaps["white"]);

            Material potentialDoorMat = new Material(assetMan.Get<Material>("tileAlpha"));
            potentialDoorMat.name = "PotentialDoorMat";
            potentialDoorMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "PotentialDoor.png"));
            potentialDoorMat.SetTexture("_LightMap", lightmaps["white"]);

            Material forcedUnsafeMat = new Material(assetMan.Get<Material>("tileAlpha"));
            forcedUnsafeMat.name = "ForcedUnsafeMat";
            forcedUnsafeMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "ForcedUnsafeCell.png"));
            forcedUnsafeMat.SetTexture("_LightMap", lightmaps["white"]);

            Material floorRadMat = new Material(assetMan.Get<Material>("tileAlpha"));
            floorRadMat.name = "FloorRadiusMap";
            floorRadMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "FloorRadius.png"));
            floorRadMat.SetTexture("_LightMap", lightmaps["white"]);

            yield return "Setting up GridManager...";
            GameObject gridManagerObject = new GameObject("GridManager");
            gridManagerObject.ConvertToPrefab(true);
            GridManager gridManager = gridManagerObject.AddComponent<GridManager>();

            gridManager.gridCellTemplate = CreateQuad("GridCell", gridMat, Vector3.zero, new Vector3(90f, 0f, 0f));
            gridManager.gridCellTemplate.ConvertToPrefab(true);

            for (int i = 0; i < directions.Count; i++)
            {
                GameObject dirQuad = CreateQuad("GridSelect_" + directions[i].ToString(), gridArrowMat, directions[i].ToVector3() * 10f, new Vector3(90f, directions[i].ToDegrees(), 0f));
                dirQuad.transform.localScale *= 2f;
                dirQuad.transform.SetParent(gridManager.transform, true);
                dirQuad.layer = editorInteractableLayer;
                dirQuad.AddComponent<MeshCollider>();
                GridArrow arrow = dirQuad.AddComponent<GridArrow>();
                arrow.direction = directions[i];
                arrow.grid = gridManager;
                gridManager.arrowObjects[i] = dirQuad;
            }

            yield return "Setting up selector...";
            GameObject selectorObject = new GameObject("Selector");
            selectorObject.ConvertToPrefab(true);
            GameObject tileQuad = CreateQuad("TileSelection", selectMat, Vector3.zero, new Vector3(90f, 0f, 0f));
            tileQuad.transform.SetParent(selectorObject.transform, false);
            tileQuad.gameObject.SetActive(false);

            Selector selector = selectorObject.AddComponent<Selector>();
            selector.tileSelector = tileQuad;

            for (int i = 0; i < directions.Count; i++)
            {
                GameObject dirQuad = CreateQuad("DirSelect_" + directions[i].ToString(), arrowMat, directions[i].ToVector3() * 10f, new Vector3(90f, directions[i].ToDegrees(), 0f));
                dirQuad.transform.SetParent(selectorObject.transform, true);
                dirQuad.gameObject.SetActive(false);
                dirQuad.layer = editorInteractableLayer;
                dirQuad.AddComponent<MeshCollider>();
                SelectorArrow arrow = dirQuad.AddComponent<SelectorArrow>();
                arrow.direction = directions[i];
                arrow.selector = selector;
                selector.tileArrows[i] = dirQuad;
            }

            GameObject settingsObject = new GameObject("SettingsGear");
            settingsObject.transform.SetParent(selectorObject.transform, true);

            GameObject settingsSpriteObject = new GameObject("Sprite");
            settingsSpriteObject.transform.SetParent(settingsObject.transform);
            settingsSpriteObject.layer = LayerMask.NameToLayer("Billboard");
            settingsSpriteObject.transform.localPosition = Vector3.zero;
            SpriteRenderer settingsSpriteRenderer = settingsSpriteObject.AddComponent<SpriteRenderer>();
            settingsSpriteRenderer.material = assetMan.Get<Material>("spriteBillboard");
            settingsSpriteRenderer.sprite = AssetLoader.SpriteFromMod(this, Vector2.one / 2f, 32f, "Editor", "SettingsGear.png");
            settingsSpriteRenderer.material.SetTexture("_LightMap", lightmaps["white"]);
            BoxCollider settingsCollider = settingsObject.AddComponent<BoxCollider>();
            settingsCollider.gameObject.layer = editorInteractableLayer;
            settingsCollider.size = Vector3.one * 2f;
            selector.gearButton = settingsObject.AddComponent<SettingsWorldButton>();

            // create handles

            GameObject moveHandleBase = new GameObject("MoveHandleBase");
            moveHandleBase.transform.SetParent(selectorObject.transform);
            MoveHandles handles = moveHandleBase.AddComponent<MoveHandles>();

            GameObject handleModelBase = AssetLoader.ModelFromModManualMaterials(this, new Dictionary<string, Material>
            {
                { "base", handleArrowMat }
            }, "Models", "arrow.obj");
            handleModelBase.SetActive(false);
            handleModelBase.transform.localScale = Vector3.one * 2f;
            handleModelBase.GetComponentsInChildren<MeshRenderer>().Do(x =>
            {
                x.gameObject.layer = LayerMask.NameToLayer("Overlay");
                x.transform.localScale = new Vector3(1f, 1f, -1f); // fix the arrow facing the wrong way because i modeled it wrong
            });
            handleModelBase.layer = editorHandleLayer;
            handleModelBase.AddComponent<HandleArrow>().myHandles = handles;
            handleModelBase.AddComponent<BoxCollider>().size = new Vector3(0.3f, 0.3f, 1.1f);

            GameObject handleZ = GameObject.Instantiate(handleModelBase);
            handleZ.transform.SetParent(moveHandleBase.transform);
            handleZ.SetActive(true);
            handleZ.name = "HandleZ";
            handleZ.transform.forward = Vector3.forward;
            handleZ.transform.position += handleZ.transform.forward;
            handleZ.GetComponentsInChildren<MeshRenderer>().Do(x =>
            {
                x.material.SetTexture("_LightMap", lightmaps["blue"]);
            });
            handles.arrows[0] = handleZ.GetComponent<HandleArrow>();

            GameObject handleY = GameObject.Instantiate(handleModelBase);
            handleY.transform.SetParent(moveHandleBase.transform);
            handleY.SetActive(true);
            handleY.name = "HandleY";
            handleY.transform.forward = Vector3.up;
            handleY.transform.position += handleY.transform.forward;
            handleY.GetComponentsInChildren<MeshRenderer>().Do(x =>
            {
                x.material.SetTexture("_LightMap", lightmaps["green"]);
            });
            handles.arrows[1] = handleY.GetComponent<HandleArrow>();

            GameObject handleX = GameObject.Instantiate(handleModelBase);
            handleX.transform.SetParent(moveHandleBase.transform);
            handleX.SetActive(true);
            handleX.name = "HandleX";
            handleX.transform.forward = Vector3.right;
            handleX.transform.position += handleX.transform.forward;
            handleX.GetComponentsInChildren<MeshRenderer>().Do(x =>
            {
                x.material.SetTexture("_LightMap", lightmaps["red"]);
            });
            handles.arrows[2] = handleX.GetComponent<HandleArrow>();

            selector.moveHandles = handles;
            handles.mySelector = selector;

            // create the lattices

            GameObject baseLattice = new GameObject("BaseLattice");
            GameObject topQuad = CreateQuad("TopQuad", selectorLatticeMat, Vector3.zero, Vector3.zero);
            GameObject bottomQuad = CreateQuad("BottomQuad", selectorLatticeMat, Vector3.zero, Vector3.zero);
            topQuad.transform.SetParent(baseLattice.transform, true);
            bottomQuad.transform.SetParent(baseLattice.transform, true);
            topQuad.transform.localScale = Vector3.one;
            bottomQuad.transform.localScale = -Vector3.one;
            baseLattice.layer = editorHandleLayer;
            baseLattice.AddComponent<BoxCollider>().size = new Vector3(1f,1f,0.025f);
            baseLattice.GetComponentsInChildren<MeshRenderer>().Do(x => x.gameObject.layer = LayerMask.NameToLayer("Overlay"));
            baseLattice.AddComponent<HandleLattice>();


            GameObject xzLattice = GameObject.Instantiate(baseLattice);
            xzLattice.transform.SetParent(handles.transform, true);
            xzLattice.transform.forward = Vector3.up;
            xzLattice.transform.position = (Vector3.forward + Vector3.right) * 0.75f;
            xzLattice.GetComponentsInChildren<MeshRenderer>().Do(x => x.material.SetTexture("_LightMap", lightmaps["green"]));
            xzLattice.GetComponent<HandleLattice>().myHandles = handles;
            handles.lattices[0] = xzLattice.GetComponent<HandleLattice>();
            xzLattice.name = "XZLattice";

            GameObject xyLattice = GameObject.Instantiate(baseLattice);
            xyLattice.transform.SetParent(handles.transform, true);
            xyLattice.transform.forward = Vector3.forward;
            xyLattice.transform.position = (Vector3.right + Vector3.up) * 0.75f;
            xyLattice.GetComponentsInChildren<MeshRenderer>().Do(x => x.material.SetTexture("_LightMap", lightmaps["blue"]));
            xyLattice.GetComponent<HandleLattice>().myHandles = handles;
            handles.lattices[1] = xyLattice.GetComponent<HandleLattice>();
            xyLattice.name = "XYLattice";

            GameObject zyLattice = GameObject.Instantiate(baseLattice);
            zyLattice.transform.SetParent(handles.transform, true);
            zyLattice.transform.forward = Vector3.right;
            zyLattice.transform.position = (Vector3.forward + Vector3.up) * 0.75f;
            zyLattice.GetComponentsInChildren<MeshRenderer>().Do(x => x.material.SetTexture("_LightMap", lightmaps["red"]));
            zyLattice.GetComponent<HandleLattice>().myHandles = handles;
            handles.lattices[2] = zyLattice.GetComponent<HandleLattice>();
            zyLattice.name = "ZYLattice";

            // create the rings
            GameObject ringModelBase = AssetLoader.ModelFromModManualMaterials(this, new Dictionary<string, Material>
            {
                { "base", handleArrowMat }
            }, "Models", "ring.obj");
            ringModelBase.SetActive(false);
            ringModelBase.transform.localScale = Vector3.one * 3f;
            ringModelBase.GetComponentsInChildren<MeshRenderer>().Do(x =>
            {
                x.gameObject.layer = LayerMask.NameToLayer("Overlay");
                x.transform.localScale = new Vector3(1f, 1f, 1f); // fix the arrow facing the wrong way because i modeled it wrong
            });
            ringModelBase.layer = editorHandleLayer;
            ringModelBase.AddComponent<HandleRing>().myHandles = handles;
            ringModelBase.AddComponent<MeshCollider>().sharedMesh = ringModelBase.transform.Find("ring").GetComponent<MeshFilter>().mesh;

            GameObject yawRing = GameObject.Instantiate(ringModelBase);
            yawRing.transform.SetParent(handles.transform, true);
            yawRing.GetComponentsInChildren<MeshRenderer>().Do(x => x.material.SetTexture("_LightMap", lightmaps["green"]));
            yawRing.name = "YawRing";
            yawRing.GetComponent<HandleRing>().axisVector = Vector3.up;
            yawRing.SetActive(true);
            handles.rings[0] = yawRing.GetComponent<HandleRing>();

            GameObject rollRing = GameObject.Instantiate(ringModelBase);
            rollRing.transform.SetParent(handles.transform, true);
            rollRing.GetComponentsInChildren<MeshRenderer>().Do(x => x.material.SetTexture("_LightMap", lightmaps["red"]));
            rollRing.name = "RollRing";
            rollRing.GetComponent<HandleRing>().axisVector = Vector3.right;
            rollRing.transform.up = Vector3.right;
            rollRing.SetActive(true);
            handles.rings[1] = rollRing.GetComponent<HandleRing>();

            GameObject pitchRing = GameObject.Instantiate(ringModelBase);
            pitchRing.transform.SetParent(handles.transform, true);
            pitchRing.GetComponentsInChildren<MeshRenderer>().Do(x => x.material.SetTexture("_LightMap", lightmaps["blue"]));
            pitchRing.name = "PitchRing";
            pitchRing.GetComponent<HandleRing>().axisVector = Vector3.forward;
            pitchRing.transform.up = Vector3.forward;
            pitchRing.SetActive(true);
            handles.rings[2] = pitchRing.GetComponent<HandleRing>();

            DestroyImmediate(baseLattice);
            DestroyImmediate(ringModelBase);
            DestroyImmediate(handleModelBase);

            GameObject dummyTransform = new GameObject("DummyTransform");
            dummyTransform.transform.SetParent(handles.transform, true);
            handles.dummyTransform = dummyTransform.transform;

            yield return "Creating Worker CoreGameManager...";
            CoreGameManager cgm = Resources.FindObjectsOfTypeAll<CoreGameManager>().First(x => x.name == "CoreGameManager" && x.GetInstanceID() >= 0);
            CoreGameManager workerCgm = GameObject.Instantiate<CoreGameManager>(cgm, MTM101BaldiDevAPI.prefabTransform);
            workerCgm.ReflectionSetVariable("destroyOnLoad", true);
            workerCgm.gameObject.SetActive(false);
            workerCgm.name = "WorkerCoreGameManager";

            yield return "Setting up EditorPlayModeManager...";

            GameObject playModeGameObject = new GameObject("EditorPlayModeManager");
            playModeGameObject.ConvertToPrefab(true);
            EditorPlayModeManager playModeManager = playModeGameObject.AddComponent<EditorPlayModeManager>();

            ChallengeWin winCopy = GameObject.Instantiate<ChallengeWin>(Resources.FindObjectsOfTypeAll<ChallengeWin>().First(x => x.GetInstanceID() >= 0 && x.transform.parent.name == "GrappleChallengeManager"), playModeGameObject.transform);
            winCopy.name = "GameWin";
            winCopy.transform.Find("Canvas").Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Congratulation! You won!";
            playModeManager.winScreen = winCopy;
            playModeManager.winText = winCopy.transform.Find("Canvas").Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

            yield return "Creating editor ingame objects...";
            GameLock[] foundLocks = Resources.FindObjectsOfTypeAll<GameLock>().Where(x => x.GetInstanceID() >= 0).ToArray();
            Structure_LockedRoom lockedRoomBuilder = new GameObject("LockedRoomBuilder").AddComponent<Structure_LockedRoom>();
            lockedRoomBuilder.gameObject.ConvertToPrefab(true);
            LevelLoaderPlugin.Instance.structureAliases.Add("shapelock", new LoaderStructureData(lockedRoomBuilder, new Dictionary<string, GameObject>()
            {
                { "shapelock_circle", foundLocks.First(x => x.name == "GameLock_0").gameObject },
                { "shapelock_triangle", foundLocks.First(x => x.name == "GameLock_1").gameObject },
                { "shapelock_square", foundLocks.First(x => x.name == "GameLock_2").gameObject },
                { "shapelock_star", foundLocks.First(x => x.name == "GameLock_3").gameObject },
                { "shapelock_heart", foundLocks.First(x => x.name == "GameLock_4").gameObject },
                { "shapelock_weird", foundLocks.First(x => x.name == "GameLock_5").gameObject },
            }));

            Structure_PowerLeverEditor editorPowerLeverBuilder = GameObject.Instantiate<Structure_PowerLever>(Resources.FindObjectsOfTypeAll<Structure_PowerLever>().First(x => x.name == "PowerLeverConstructor" && x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform).gameObject.SwapComponent<Structure_PowerLever, Structure_PowerLeverEditor>();
            editorPowerLeverBuilder.name = "EditorPowerLeverConstructor";
            LevelLoaderPlugin.Instance.structureAliases.Add("powerlever", new LoaderStructureData(editorPowerLeverBuilder));

            Structure_LevelBoxEditor editorFactoryBoxBuilder = GameObject.Instantiate<Structure_LevelBox>(Resources.FindObjectsOfTypeAll<Structure_LevelBox>().First(x => x.name == "FactoryBoxConstructor" && x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform).gameObject.SwapComponent<Structure_LevelBox, Structure_LevelBoxEditor>();
            editorFactoryBoxBuilder.name = "EditorFactoryBoxConstructor";
            LevelLoaderPlugin.Instance.structureAliases.Add("factorybox", new LoaderStructureData(editorFactoryBoxBuilder));

            yield return "Creating editor prefab visuals...";

            // create light display
            GameObject lightDisplayObject = new GameObject("LightVisual");
            lightDisplayObject.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            GameObject lightSpriteObject = new GameObject("Sprite");
            lightSpriteObject.transform.SetParent(lightDisplayObject.transform);
            lightSpriteObject.layer = LayerMask.NameToLayer("Billboard");
            lightSpriteObject.transform.localPosition = Vector3.up * 2f;
            SpriteRenderer lightSpriteRenderer = lightSpriteObject.AddComponent<SpriteRenderer>();
            lightSpriteRenderer.material = assetMan.Get<Material>("spriteBillboard");
            lightSpriteRenderer.sprite = AssetLoader.SpriteFromMod(this, Vector2.one / 2f, 32f, "Editor", "Lightbulb.png");
            lightSpriteRenderer.material.SetTexture("_LightMap", lightmaps["white"]);
            assetMan.Add<GameObject>("LightDisplay", lightDisplayObject);
            BoxCollider boxC = lightDisplayObject.AddComponent<BoxCollider>();
            boxC.size = new Vector3(1f,2f,1f);
            boxC.center += Vector3.up * 2f;
            boxC.gameObject.layer = editorInteractableLayer;
            EditorDeletableObject lightEdo = boxC.gameObject.AddComponent<EditorDeletableObject>();
            boxC.gameObject.AddComponent<SettingsComponent>();
            boxC.gameObject.AddComponent<EditorRendererContainer>().AddRenderer(lightSpriteRenderer, "white");
            lightEdo.renderContainer = boxC.gameObject.GetComponent<EditorRendererContainer>();

            // create door visuals
            EditorInterface.AddDoor<StandardDoorDisplay>("standard", DoorIngameStatus.AlwaysDoor, assetMan.Get<Material>("doorMask"), null);
            EditorInterface.AddDoor<DoorDisplay>("swinging", DoorIngameStatus.Smart, assetMan.Get<Material>("swingingDoorMask"), new Material[] { assetMan.Get<Material>("SwingingDoorMat"), assetMan.Get<Material>("SwingingDoorMat") });
            EditorInterface.AddDoor<DoorDisplay>("oneway", DoorIngameStatus.AlwaysObject, assetMan.Get<Material>("swingingDoorMask"), new Material[] { assetMan.Get<Material>("OneWayRight"), assetMan.Get<Material>("OneWayWrong") });
            EditorInterface.AddDoor<DoorDisplay>("swinging_silent", DoorIngameStatus.Smart, assetMan.Get<Material>("swingingDoorMask"), new Material[] { silentDoorMat, silentDoorMat });
            EditorInterface.AddDoor<DoorDisplay>("coinswinging", DoorIngameStatus.AlwaysObject, assetMan.Get<Material>("swingingDoorMask"), new Material[] { assetMan.Get<Material>("CoinDoorMat"), assetMan.Get<Material>("CoinDoorMat") });
            EditorInterface.AddDoor<DoorDisplay>("autodoor", DoorIngameStatus.AlwaysDoor, assetMan.Get<Material>("AutoDoorMask"), new Material[] { assetMan.Get<Material>("AutoDoorMat"), assetMan.Get<Material>("AutoDoorMat") });
            EditorInterface.AddDoor<DoorDisplay>("flaps", DoorIngameStatus.AlwaysObject, assetMan.Get<Material>("FlapDoorMask"), new Material[] { assetMan.Get<Material>("FlapDoorMat"), assetMan.Get<Material>("FlapDoorMat") });

            WindowObject standardWindowObject = Resources.FindObjectsOfTypeAll<WindowObject>().First(x => x.name == "WoodWindow" && x.GetInstanceID() >= 0);
            EditorInterface.AddWindow<DoorDisplay>("standard", standardWindowObject.mask, standardWindowObject.overlay);

            // elevators
            EditorInterface.AddExit("elevator", LevelLoaderPlugin.Instance.exitDatas["elevator"].prefab);

            // pickup visual
            pickupVisual = EditorInterface.CloneToPrefabStripMonoBehaviors(Resources.FindObjectsOfTypeAll<Pickup>().First(x => x.transform.parent == null && x.GetInstanceID() >= 0 && x.name == "Pickup").gameObject);
            pickupVisual.AddComponent<EditorRendererContainer>().AddRenderer(pickupVisual.transform.Find("ItemSprite").GetComponent<SpriteRenderer>(), "none");
            pickupVisual.AddComponent<EditorDeletableObject>().renderContainer = pickupVisual.GetComponent<EditorRendererContainer>();
            pickupVisual.name = "PickupVisual";
            pickupVisual.AddComponent<MovableObjectInteraction>().allowedAxis = MoveAxis.Horizontal;
            pickupVisual.layer = editorInteractableLayer;

            // poster visual
            GameObject posterVisualBase = new GameObject("PosterVisual");
            posterVisualBase.ConvertToPrefab(true);
            GameObject posterQuad = CreateQuad("PosterWall", new Material(assetMan.Get<Material>("tileAlpha")), new Vector3(0f,5f,4.999f), Vector3.zero);
            posterQuad.transform.SetParent(posterVisualBase.transform, true);
            BoxCollider posterCollider = posterVisualBase.AddComponent<BoxCollider>();
            posterCollider.size = new Vector3(10f,10f,0.02f);
            posterCollider.center = posterQuad.transform.localPosition;
            posterVisualBase.layer = editorInteractableLayer;
            posterCollider.gameObject.AddComponent<EditorRendererContainer>().AddRenderer(posterQuad.GetComponent<MeshRenderer>(), "none");
            posterCollider.gameObject.AddComponent<EditorDeletableObject>().renderContainer = posterCollider.gameObject.GetComponent<EditorRendererContainer>();
            posterVisual = posterVisualBase;

            // wall add visual
            GameObject wallVisualBase = new GameObject("WallAddVisual");
            wallVisualBase.ConvertToPrefab(true);
            GameObject wallQuad = CreateQuad("WallVisualA", wallAddMat, new Vector3(0f, 5f, 4.999f), Vector3.zero);
            wallQuad.transform.SetParent(wallVisualBase.transform, true);
            GameObject wallQuadB = CreateQuad("WallVisualB", wallAddMat, new Vector3(0f, 5f, 5.001f), new Vector3(0f,180f,0f));
            wallQuadB.transform.SetParent(wallVisualBase.transform, true);
            BoxCollider wallCollider = wallVisualBase.AddComponent<BoxCollider>();
            wallCollider.size = new Vector3(10f, 10f, 0.015f);
            wallCollider.center = wallQuad.transform.localPosition;
            wallVisualBase.layer = editorInteractableLayer;
            wallCollider.gameObject.AddComponent<EditorRendererContainer>().AddRendererRange(wallCollider.GetComponentsInChildren<MeshRenderer>(), "white");
            wallCollider.gameObject.AddComponent<EditorDeletableObject>().renderContainer = wallCollider.gameObject.GetComponent<EditorRendererContainer>();
            wallVisual = wallVisualBase;

            GameObject wallRemoveBase = GameObject.Instantiate(wallVisualBase, MTM101BaldiDevAPI.prefabTransform);
            wallRemoveBase.name = "WallRemoveVisual";
            wallRemoveBase.GetComponent<EditorDeletableObject>();
            wallRemoveBase.GetComponent<EditorRendererContainer>().myRenderers.ForEach(x => x.material = wallRemoveMat);
            wallRemoveVisual = wallRemoveBase;


            // object visuals
            EditorInterface.AddObjectVisualWithMeshCollider("desk", LevelLoaderPlugin.Instance.basicObjects["desk"], true);
            EditorInterface.AddObjectVisualWithMeshCollider("chair", LevelLoaderPlugin.Instance.basicObjects["chair"], true);
            EditorInterface.AddObjectVisualWithMeshCollider("bigdesk", LevelLoaderPlugin.Instance.basicObjects["bigdesk"], true);
            EditorInterface.AddObjectVisual("waterfountain", LevelLoaderPlugin.Instance.basicObjects["waterfountain"], true);
            EditorInterface.AddObjectVisual("rounddesk", LevelLoaderPlugin.Instance.basicObjects["rounddesk"], true);
            EditorInterface.AddObjectVisual("roundtable", LevelLoaderPlugin.Instance.basicObjects["roundtable"], true);

            EditorInterface.AddObjectVisual("computer", LevelLoaderPlugin.Instance.basicObjects["computer"], true);
            EditorInterface.AddObjectVisual("computer_off", LevelLoaderPlugin.Instance.basicObjects["computer_off"], true);
            EditorInterface.AddObjectVisual("locker", LevelLoaderPlugin.Instance.basicObjects["locker"], true);
            EditorInterface.AddObjectVisual("bluelocker", LevelLoaderPlugin.Instance.basicObjects["bluelocker"], true);
            EditorInterface.AddObjectVisual("greenlocker", LevelLoaderPlugin.Instance.basicObjects["greenlocker"], true);

            EditorInterface.AddObjectVisual("bookshelf", LevelLoaderPlugin.Instance.basicObjects["bookshelf"], true);
            EditorInterface.AddObjectVisual("bookshelf_hole", LevelLoaderPlugin.Instance.basicObjects["bookshelf_hole"], true);

            EditorInterface.AddObjectVisual("cabinet", LevelLoaderPlugin.Instance.basicObjects["cabinet"], true);
            EditorBasicObject pedestalVisual = EditorInterface.AddObjectVisual("pedestal", LevelLoaderPlugin.Instance.basicObjects["pedestal"], true);
            pedestalVisual.GetComponent<CapsuleCollider>().height = 3f;
            pedestalVisual.GetComponent<CapsuleCollider>().center = new Vector3(0f,2f,0f);

            EditorInterface.AddObjectVisualWithMeshCollider("cafeteriatable", LevelLoaderPlugin.Instance.basicObjects["cafeteriatable"], true);
            EditorInterface.AddObjectVisual("hoop", LevelLoaderPlugin.Instance.basicObjects["hoop"], true);
            EditorInterface.AddObjectVisualWithCustomBoxCollider("hopscotch", LevelLoaderPlugin.Instance.basicObjects["hopscotch"], new Vector3(30f,0.01f,30f), Vector3.zero);
            EditorInterface.AddObjectVisual("tree", LevelLoaderPlugin.Instance.basicObjects["tree"], true);
            EditorInterface.AddObjectVisual("pinetree", LevelLoaderPlugin.Instance.basicObjects["pinetree"], true);
            EditorInterface.AddObjectVisual("picnictable", LevelLoaderPlugin.Instance.basicObjects["picnictable"], true);
            // gotta fix this up
            EditorBasicObject appleTreeVisual = EditorInterface.AddObjectVisual("appletree", LevelLoaderPlugin.Instance.basicObjects["appletree"], true);
            appleTreeVisual.transform.Find("Sprite").Find("Pickup").Find("ItemSprite").GetComponent<SpriteRenderer>().sprite = ItemMetaStorage.Instance.FindByEnum(Items.Apple).value.itemSpriteLarge;
            EditorInterface.AddObjectVisual("bananatree", LevelLoaderPlugin.Instance.basicObjects["bananatree"], true);

            EditorInterface.AddObjectVisual("counter", LevelLoaderPlugin.Instance.basicObjects["counter"], true);
            EditorInterface.AddObjectVisual("examinationtable", LevelLoaderPlugin.Instance.basicObjects["examinationtable"], true);
            EditorInterface.AddObjectVisual("merrygoround", LevelLoaderPlugin.Instance.basicObjects["merrygoround"], true);
            EditorInterface.AddObjectVisual("tent", LevelLoaderPlugin.Instance.basicObjects["tent"], true);

            // the ones with no hitboxes
            EditorInterface.AddObjectVisualWithCustomSphereCollider("picnicbasket", LevelLoaderPlugin.Instance.basicObjects["picnicbasket"], 2f, Vector3.up);
            EditorInterface.AddObjectVisualWithCustomSphereCollider("rock", LevelLoaderPlugin.Instance.basicObjects["rock"], 1f, Vector3.up);
            EditorInterface.AddObjectVisualWithCustomSphereCollider("decor_banana", LevelLoaderPlugin.Instance.basicObjects["decor_banana"], 1f, Vector3.up);
            EditorInterface.AddObjectVisualWithCustomSphereCollider("decor_globe", LevelLoaderPlugin.Instance.basicObjects["decor_globe"], 1f, Vector3.up);
            EditorInterface.AddObjectVisualWithCustomSphereCollider("decor_lunch", LevelLoaderPlugin.Instance.basicObjects["decor_lunch"], 1f, Vector3.up);
            EditorInterface.AddObjectVisualWithCustomSphereCollider("decor_notebooks", LevelLoaderPlugin.Instance.basicObjects["decor_notebooks"], 1f, Vector3.up);
            EditorInterface.AddObjectVisualWithCustomSphereCollider("decor_papers", LevelLoaderPlugin.Instance.basicObjects["decor_papers"], 1f, Vector3.up);
            EditorInterface.AddObjectVisualWithCustomSphereCollider("decor_pencilnotes", LevelLoaderPlugin.Instance.basicObjects["decor_pencilnotes"], 1f, Vector3.up);
            EditorInterface.AddObjectVisualWithCustomSphereCollider("decor_zoneflag", LevelLoaderPlugin.Instance.basicObjects["decor_zoneflag"], 1f, Vector3.up);
            EditorInterface.AddObjectVisualWithCustomCapsuleCollider("plant", LevelLoaderPlugin.Instance.basicObjects["plant"], 1f, 7f, 1, Vector3.up * 3.5f);
            EditorInterface.AddObjectVisualWithCustomBoxCollider("ceilingfan", LevelLoaderPlugin.Instance.basicObjects["ceilingfan"], new Vector3(10f,2f,10f), Vector3.up * 9f);
            EditorInterface.AddObjectVisualWithCustomSphereCollider("exitsign", LevelLoaderPlugin.Instance.basicObjects["exitsign"], 1f, Vector3.down);

            EditorBasicObject arrowObjectVisual = EditorInterface.AddObjectVisualWithCustomSphereCollider("arrow", LevelLoaderPlugin.Instance.basicObjects["arrow"], 1f, Vector3.zero);
            AnimatedSpriteRotator[] rotators = arrowObjectVisual.GetComponentsInChildren<AnimatedSpriteRotator>();
            // this was originally a more generic solution until i had to figure out that i needed to rotate the sprites. yuck.
            for (int i = 0; i < rotators.Length; i++)
            {
                SpriteRotationMap[] map = (SpriteRotationMap[])rotators[i].ReflectionGetVariable("spriteMap");
                if (map.Length == 0) continue; // how?
                SpriteRenderer target = (SpriteRenderer)rotators[i].ReflectionGetVariable("renderer");
                SpriteRotator regularRotator = rotators[i].gameObject.AddComponent<SpriteRotator>();
                regularRotator.ReflectionSetVariable("spriteRenderer", target);
                Sprite[] spriteSheet = (Sprite[])map[0].ReflectionGetVariable("spriteSheet");
                Sprite[] alteredSheet = new Sprite[spriteSheet.Length];
                for (int h = 0; h < spriteSheet.Length; h++)
                {
                    alteredSheet[h] = spriteSheet[(h + 9) % spriteSheet.Length];
                }
                regularRotator.ReflectionSetVariable("sprites", alteredSheet); //mystman why
                GameObject.DestroyImmediate(rotators[i]);
            }

            // machines
            EditorInterface.AddObjectVisual("dietbsodamachine", LevelLoaderPlugin.Instance.basicObjects["dietbsodamachine"], true);
            EditorInterface.AddObjectVisualWithMeshCollider("bsodamachine", LevelLoaderPlugin.Instance.basicObjects["bsodamachine"], true);
            EditorInterface.AddObjectVisualWithMeshCollider("zestymachine", LevelLoaderPlugin.Instance.basicObjects["zestymachine"], true);
            EditorInterface.AddObjectVisualWithMeshCollider("crazymachine_bsoda", LevelLoaderPlugin.Instance.basicObjects["crazymachine_bsoda"], true);
            EditorInterface.AddObjectVisualWithMeshCollider("crazymachine_zesty", LevelLoaderPlugin.Instance.basicObjects["crazymachine_zesty"], true);
            EditorInterface.AddObjectVisual("payphone", LevelLoaderPlugin.Instance.basicObjects["payphone"], true);
            EditorInterface.AddObjectVisual("tapeplayer", LevelLoaderPlugin.Instance.basicObjects["tapeplayer"], true);

            // activities
            GameObject notebookVisual = EditorInterface.AddActivityVisual("notebook", Resources.FindObjectsOfTypeAll<Notebook>().First(x => x.GetInstanceID() >= 0).gameObject);
            notebookVisual.GetComponent<MovableObjectInteraction>().allowedAxis = MoveAxis.Horizontal; // notebooks are just activities that instantly spawn their book so the Y value does nothing.
            GameObject mathMachineVisual = EditorInterface.CloneToPrefabStripMonoBehaviors(LevelLoaderPlugin.Instance.activityAliases["mathmachine"].gameObject, new Type[] { typeof(TMP_Text) });
            mathMachineVisual.name = mathMachineVisual.name.Replace("_Stripped", "_Visual");
            DestroyImmediate(mathMachineVisual.transform.Find("Buffer").gameObject);
            BoxCollider mathMachineCollider = mathMachineVisual.transform.Find("Model").GetComponent<BoxCollider>();
            MovableObjectInteraction mathMachineMovableObjectInteract = mathMachineCollider.gameObject.AddComponent<MovableObjectInteraction>();
            mathMachineMovableObjectInteract.allowedRotations = RotateAxis.Flat;
            mathMachineMovableObjectInteract.allowedAxis = MoveAxis.All;
            mathMachineCollider.gameObject.AddComponent<EditorRendererContainer>().AddRendererRange(mathMachineVisual.GetComponentsInChildren<Renderer>(), "none");
            mathMachineCollider.gameObject.AddComponent<EditorDeletableObject>().renderContainer = mathMachineCollider.gameObject.GetComponent<EditorRendererContainer>();
            mathMachineVisual.gameObject.layer = LevelStudioPlugin.editorInteractableLayer;
            mathMachineCollider.gameObject.layer = LevelStudioPlugin.editorInteractableLayer;
            EditorInterface.AddActivityVisual("balloonbuster", LevelLoaderPlugin.Instance.activityAliases["balloonbuster"].gameObject);
            EditorInterface.AddActivityVisual("matchmachine", LevelLoaderPlugin.Instance.activityAliases["matchmachine"].gameObject);

            GameObject mathMachineCornerVisual = EditorInterface.CloneToPrefabStripMonoBehaviors(LevelLoaderPlugin.Instance.activityAliases["mathmachine_corner"].gameObject, new Type[] { typeof(TMP_Text) });
            mathMachineCornerVisual.name = mathMachineCornerVisual.name.Replace("_Stripped", "_Visual");
            DestroyImmediate(mathMachineCornerVisual.transform.Find("Buffer").gameObject);
            BoxCollider mathMachineCornerCollider = mathMachineCornerVisual.transform.Find("Model").GetComponent<BoxCollider>();
            MovableObjectInteraction mathMachineCornermovableObjectInteract = mathMachineCornerCollider.gameObject.AddComponent<MovableObjectInteraction>();
            mathMachineCornermovableObjectInteract.allowedRotations = RotateAxis.Flat;
            mathMachineCornermovableObjectInteract.allowedAxis = MoveAxis.All;
            mathMachineCornerCollider.gameObject.AddComponent<EditorRendererContainer>().AddRendererRange(mathMachineCornerVisual.GetComponentsInChildren<Renderer>(), "none");
            mathMachineCornerCollider.gameObject.AddComponent<EditorDeletableObject>().renderContainer = mathMachineCornerCollider.gameObject.GetComponent<EditorRendererContainer>();
            mathMachineCornerVisual.gameObject.layer = LevelStudioPlugin.editorInteractableLayer;
            mathMachineCornerCollider.gameObject.layer = LevelStudioPlugin.editorInteractableLayer;

            LevelStudioPlugin.Instance.activityDisplays.Add("mathmachine", mathMachineVisual);
            LevelStudioPlugin.Instance.activityDisplays.Add("mathmachine_corner", mathMachineCornerVisual);

            // structures
            GameObject facultyOnlyDoorVisual = EditorInterface.AddStructureGenericVisual("facultyonlydoor", Resources.FindObjectsOfTypeAll<FacultyOnlyDoor>().First(x => x.GetInstanceID() >= 0 && x.name == "FacultyOnlyDoor").gameObject);
            DestroyImmediate(facultyOnlyDoorVisual.GetComponent<SphereCollider>());
            BoxCollider facultyOnlyCollider = facultyOnlyDoorVisual.AddComponent<BoxCollider>();
            BoxCollider facultyOnlyOGCollider = facultyOnlyDoorVisual.transform.Find("ShutCollider").GetComponent<BoxCollider>();
            facultyOnlyCollider.size = facultyOnlyOGCollider.size;
            facultyOnlyCollider.center = facultyOnlyOGCollider.center + (Vector3.up * 10f);
            structureTypes.Add("facultyonlydoor", typeof(HallDoorStructureLocation));
            EditorInterface.AddStructureGenericVisual("button", Resources.FindObjectsOfTypeAll<GameButton>().First(x => x.GetInstanceID() >= 0 && x.name == "GameButton" && x.transform.parent == null).gameObject);

            GameLever lever = Resources.FindObjectsOfTypeAll<GameLever>().First(x => x.GetInstanceID() >= 0 && x.name == "GameLever" && x.transform.parent == null);

            GameObject leverVisual = EditorInterface.AddStructureGenericVisual("lever", lever.gameObject);
            LeverVisual leverVisualComp = leverVisual.AddComponent<LeverVisual>();
            leverVisualComp.target = leverVisual.GetComponentInChildren<MeshRenderer>();
            leverVisualComp.leverDownMaterial = (Material)lever.ReflectionGetVariable("offMat");
            leverVisualComp.leverUpMaterial = (Material)lever.ReflectionGetVariable("onMat");
            
            GameObject lockdownDoorVisual = EditorInterface.AddStructureGenericVisual("lockdowndoor", Resources.FindObjectsOfTypeAll<LockdownDoor>().First(x => x.GetInstanceID() >= 0 && x.name == "LockdownDoor").gameObject);
            lockdownDoorVisual.GetComponent<BoxCollider>().center += Vector3.up * 10f; // fix the collision
            lockdownDoorVisual.AddComponent<SettingsComponent>().offset = new Vector3(0f,25f,0f);
            structureTypes.Add("lockdowndoor", typeof(LockdownDoorStructureLocation));

            GameObject shutLockdownDoorVisual = EditorInterface.AddStructureGenericVisual("lockdowndoor_shut", Resources.FindObjectsOfTypeAll<LockdownDoor>().First(x => x.GetInstanceID() >= 0 && x.name == "LockdownDoor").gameObject);
            shutLockdownDoorVisual.GetComponent<BoxCollider>();
            shutLockdownDoorVisual.transform.Find("LockdownDoor_Model").transform.position += Vector3.down * 10f;
            shutLockdownDoorVisual.AddComponent<SettingsComponent>().offset = new Vector3(0f, 20f, 5f);

            // shape locks

            LoaderStructureData shapeLockData = LevelLoaderPlugin.Instance.structureAliases["shapelock"];

            foreach (KeyValuePair<string, GameObject> kvp in shapeLockData.prefabAliases)
            {
                if (kvp.Value.GetInstanceID() < 0) continue;
                GameObject lockObj = EditorInterface.AddStructureGenericVisual(kvp.Key, kvp.Value);
                lockObj.GetComponentInChildren<Collider>().isTrigger = true;
            }

            structureTypes.Add("shapelock", typeof(ShapeLockStructureLocation));

            // conveyor belts

            GameObject beltVisualObject = new GameObject("EditorBeltVisualManager");
            beltVisualObject.ConvertToPrefab(true);
            EditorBeltVisualManager beltVisualManager = beltVisualObject.AddComponent<EditorBeltVisualManager>();
            beltVisualManager.beltRenderPre = Resources.FindObjectsOfTypeAll<MeshRenderer>().First(x => x.name == "ConveyorBelt" && x.GetInstanceID() >= 0 && x.transform.parent == null);
            //beltVisualManager.slider = beltVisualObject.AddComponent<TextureSlider>();
            beltVisualManager.collider = beltVisualObject.AddComponent<BoxCollider>();
            beltVisualManager.collider.isTrigger = true;
            beltVisualManager.gameObject.layer = editorInteractableLayer;
            beltVisualManager.renderContainer = beltVisualObject.AddComponent<EditorRendererContainer>();
            beltVisualObject.AddComponent<EditorDeletableObject>().renderContainer = beltVisualManager.renderContainer;
            genericStructureDisplays.Add("conveyorbelt", beltVisualObject);
            structureTypes.Add("conveyorbelt", typeof(ConveyorBeltStructureLocation));

            // vent
            GameObject ventVisualObject = EditorInterface.CloneToPrefabStripMonoBehaviors(Resources.FindObjectsOfTypeAll<VentController>().First(x => x.GetInstanceID() >= 0).gameObject);
            VentVisualManager ventVisualManager = ventVisualObject.AddComponent<VentVisualManager>();
            ventVisualObject.layer = editorInteractableLayer;
            ventVisualManager.ventPieceBendPrefab = Resources.FindObjectsOfTypeAll<MeshRenderer>().First(x => x.GetInstanceID() >= 0 && x.name == "Vent_Bend").transform;
            ventVisualManager.ventPieceStraightPrefab = Resources.FindObjectsOfTypeAll<MeshRenderer>().First(x => x.GetInstanceID() >= 0 && x.name == "Vent_Straight").transform;
            ventVisualManager.ventPieceVerticalBendPrefab = Resources.FindObjectsOfTypeAll<MeshRenderer>().First(x => x.GetInstanceID() >= 0 && x.name == "Vent_VerticalBend").transform;
            ventVisualManager.exitGrateTransform = ventVisualObject.transform.Find("Vent_Grate_Exit");
            ventVisualManager.entryGrate = ventVisualObject.transform.Find("Vent_Grate");
            ventVisualManager.container = ventVisualObject.AddComponent<EditorRendererContainer>();
            ventVisualManager.container.AddRendererRange(ventVisualObject.GetComponentsInChildren<Renderer>(), "none");
            ventVisualObject.AddComponent<EditorDeletableObject>().renderContainer = ventVisualManager.container;
            genericStructureDisplays.Add("vent", ventVisualObject);
            structureTypes.Add("vent", typeof(VentStructureLocation));

            // power levers
            GameObject alarmLightVisualObject = EditorInterface.AddStructureGenericVisual("powerlever_alarm", Resources.FindObjectsOfTypeAll<GameObject>().First(x => x.GetInstanceID() >= 0 && x.name == "AlarmLight"));
            alarmLightVisualObject.layer = editorInteractableLayer;
            SphereCollider alarmVisualCollider = alarmLightVisualObject.AddComponent<SphereCollider>();
            alarmVisualCollider.radius = 1f;
            alarmVisualCollider.center = Vector3.up * 9f;
            alarmLightVisualObject.AddComponent<EditorDeletableObject>().renderContainer = alarmLightVisualObject.GetComponent<EditorRendererContainer>();

            GameObject powerLeverVisual = EditorInterface.AddStructureGenericVisual("powerlever_lever", lever.gameObject);
            powerLeverVisual.name = "PowerLever_Visual";
            powerLeverVisual.AddComponent<SettingsComponent>().offset = Vector3.up * 15f;
            GameObject powerLeverGaugeVisual = EditorInterface.CloneToPrefabStripMonoBehaviors(Resources.FindObjectsOfTypeAll<PowerLeverGauge>().First(x => x.name == "PowerLeverGauge" && x.GetInstanceID() >= 0).gameObject);
            powerLeverGaugeVisual.transform.Find("Box").gameObject.SetActive(true);
            powerLeverGaugeVisual.transform.Find("Indicator").gameObject.SetActive(true);
            powerLeverGaugeVisual.transform.SetParent(powerLeverVisual.transform);
            powerLeverGaugeVisual.name = "PowerLeverGauge";
            powerLeverVisual.GetComponent<EditorRendererContainer>().AddRendererRange(powerLeverGaugeVisual.GetComponentsInChildren<Renderer>(), "none");
            GameObject powerLeverLineRenderObject = new GameObject("LineRenderer");
            powerLeverLineRenderObject.transform.SetParent(powerLeverVisual.transform);
            LineRenderer powerLeverLineRenderer = powerLeverLineRenderObject.gameObject.AddComponent<LineRenderer>();
            powerLeverLineRenderer.useWorldSpace = true;
            powerLeverLineRenderer.material = new Material(Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name == "Shader Graphs/Standard"));
            powerLeverLineRenderer.material.name = "Power Lever Line Renderer";
            powerLeverLineRenderer.widthMultiplier = 0.5f;

            // power lever breaker visual stuff
            GameObject breakerVisual = EditorInterface.AddStructureGenericVisual("powerlever_breaker", Resources.FindObjectsOfTypeAll<BreakerController>().First(x => x.name == "PowerBreaker" && x.GetInstanceID() >= 0).gameObject);
            breakerVisual.AddComponent<SettingsComponent>().offset = Vector3.up * 15f;
            breakerVisual.GetComponent<EditorRendererContainer>().myRenderers.Clear();
            breakerVisual.GetComponent<EditorRendererContainer>().defaultHighlights.Clear();
            breakerVisual.GetComponent<EditorRendererContainer>().AddRendererRange(breakerVisual.GetComponentsInChildren<Renderer>(), "none");

            structureTypes.Add("powerlever", typeof(PowerLeverStructureLocation));

            // factory boxes
            structureTypes.Add("factorybox", typeof(FactoryBoxStructureLocation));

            // steam valves
            GameValve valve = Resources.FindObjectsOfTypeAll<GameValve>().First(x => x.GetInstanceID() >= 0 && x.name == "GameValve" && x.transform.parent == null);
            EditorInterface.AddStructureGenericVisual("valve", valve.gameObject);

            GameObject steamVisualObject = new GameObject("SteamVisual");
            steamVisualObject.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            GameObject steamSpriteObject = new GameObject("Sprite");
            steamSpriteObject.transform.SetParent(steamVisualObject.transform);
            steamSpriteObject.layer = LayerMask.NameToLayer("Billboard");
            steamSpriteObject.transform.localPosition = Vector3.up * 2f;
            SpriteRenderer steamSpriteRenderer = steamSpriteObject.AddComponent<SpriteRenderer>();
            steamSpriteRenderer.material = assetMan.Get<Material>("spriteBillboard");
            steamSpriteRenderer.sprite = AssetLoader.SpriteFromMod(this, Vector2.one / 2f, 32f, "Editor", "SteamIndicator.png");
            steamSpriteRenderer.material.SetTexture("_LightMap", lightmaps["white"]);
            BoxCollider steamBoxC = steamVisualObject.AddComponent<BoxCollider>();
            steamBoxC.size = new Vector3(1f, 2f, 1f);
            steamBoxC.center += Vector3.up * 2f;
            steamBoxC.gameObject.layer = editorInteractableLayer;
            GameObject radVis = CreateQuad("RadiusVisual", floorRadMat, Vector3.zero, new Vector3(90f,0f,0f));
            radVis.transform.SetParent(steamVisualObject.transform, true);
            EditorDeletableObject steamEdo = steamBoxC.gameObject.AddComponent<EditorDeletableObject>();
            steamEdo.gameObject.AddComponent<SettingsComponent>();
            steamEdo.gameObject.AddComponent<EditorRendererContainer>().AddRendererRange(steamVisualObject.GetComponentsInChildren<Renderer>(), "white");
            steamEdo.renderContainer = steamBoxC.gameObject.GetComponent<EditorRendererContainer>();
            genericStructureDisplays.Add("steamvalve", steamVisualObject);
            structureTypes.Add("steamvalves", typeof(SteamValveStructureLocation));

            Structure_SteamValvesEditor steamValveStructure = GameObject.Instantiate<Structure_SteamValves>(Resources.FindObjectsOfTypeAll<Structure_SteamValves>().First(x => x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform).gameObject.SwapComponent<Structure_SteamValves, Structure_SteamValvesEditor>();
            steamValveStructure.name = "SteamValvesEditor";
            LevelLoaderPlugin.Instance.structureAliases.Add("steamvalves", new LoaderStructureData(steamValveStructure));

            // npcs

            EditorInterface.AddNPCVisual("baldi", LevelLoaderPlugin.Instance.npcAliases["baldi"]);
            EditorInterface.AddNPCVisual("principal", LevelLoaderPlugin.Instance.npcAliases["principal"]);
            EditorInterface.AddNPCVisual("sweep", LevelLoaderPlugin.Instance.npcAliases["sweep"]);
            EditorInterface.AddNPCVisual("playtime", LevelLoaderPlugin.Instance.npcAliases["playtime"]);
            GameObject chalklesVisual = EditorInterface.AddNPCVisual("chalkface", LevelLoaderPlugin.Instance.npcAliases["chalkface"]);
            chalklesVisual.transform.Find("SpriteBase").Find("Sprite").gameObject.SetActive(true);
            chalklesVisual.GetComponent<EditorRendererContainer>().AddRenderer(chalklesVisual.GetComponentInChildren<Renderer>(), "none");
            EditorInterface.AddNPCVisual("bully", LevelLoaderPlugin.Instance.npcAliases["bully"]);
            EditorInterface.AddNPCVisual("beans", LevelLoaderPlugin.Instance.npcAliases["beans"]);
            EditorInterface.AddNPCVisual("prize", LevelLoaderPlugin.Instance.npcAliases["prize"]);
            EditorInterface.AddNPCVisual("crafters", LevelLoaderPlugin.Instance.npcAliases["crafters"]);
            EditorInterface.AddNPCVisual("pomp", LevelLoaderPlugin.Instance.npcAliases["pomp"]);
            EditorInterface.AddNPCVisual("test", LevelLoaderPlugin.Instance.npcAliases["test"]);
            EditorInterface.AddNPCVisual("cloudy", LevelLoaderPlugin.Instance.npcAliases["cloudy"]);
            EditorInterface.AddNPCVisual("reflex", LevelLoaderPlugin.Instance.npcAliases["reflex"]);

            // rooms
            EditorInterface.AddRoomVisualManager<OutsideRoomVisualManager>("outside");

            yield return "Configuring Misc...";

            selectableTextures.Add("HallFloor");
            selectableTextures.Add("Wall");
            selectableTextures.Add("Ceiling");
            selectableTextures.Add("BlueCarpet");
            selectableTextures.Add("WallWithMolding");
            selectableTextures.Add("TileFloor");
            selectableTextures.Add("ElevatorCeiling");
            selectableTextures.Add("PlasticTable");
            selectableTextures.Add("Grass");
            selectableTextures.Add("Fence");
            selectableTextures.Add("None");
            selectableTextures.Add("BasicFloor");
            selectableTextures.Add("JohnnyWall");
            selectableTextures.Add("PlaceholderFloor");
            selectableTextures.Add("PlaceholderWall");
            selectableTextures.Add("PlaceholderCeiling");
            selectableTextures.Add("SaloonWall");
            selectableTextures.Add("MaintenanceFloor");
            selectableTextures.Add("RedBrickWall");
            selectableTextures.Add("FactoryCeiling");
            selectableTextures.Add("LabFloor");
            selectableTextures.Add("LabWall");
            selectableTextures.Add("LabCeiling");
            selectableTextures.Add("Ground2");
            selectableTextures.Add("DiamondPlateFloor");
            selectableTextures.Add("Vent");
            selectableTextures.Add("Corn");
            selectableTextures.Add("Black");

            yield return "Setting up GameManagers...";

            SoundObject balAllBooks = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, "Sounds", "Ingame", "BAL_AllNotebooks_Generic.wav"), "Vfx_BAL_Tutorial_AllNotebooks_0", SoundType.Voice, Color.green);
            balAllBooks.additionalKeys = new SubtitleTimedKey[]
            {
                new SubtitleTimedKey()
                {
                    time=5.5f,
                    key="Vfx_BAL_AllNotebooks_3"
                },
                new SubtitleTimedKey()
                {
                    time=9f,
                    key="Vfx_BAL_AllNotebooks_4"
                },
                new SubtitleTimedKey()
                {
                    time=15.5f,
                    key="Vfx_BAL_AllNotebooks_5"
                }
            };

            EditorMainGameManager emg = new MainGameManagerBuilder<EditorMainGameManager>()
                .SetAllNotebooksSound(balAllBooks)
                .SetHappyBaldi(Resources.FindObjectsOfTypeAll<HappyBaldi>().First(x => x.name == "HappyBaldi" && x.GetInstanceID() >= 0 && (x.transform.parent == null)))
                .SetObjectName("EditorMainGameManager")
                .SetLevelNumber(99)
                .Build();

            assetMan.Add<BaseGameManager>("EditorMainGameManager", emg);

            yield return "Setting up Editor Controller...";
            GameObject editorControllerObject = new GameObject("StandardEditorController");
            editorControllerObject.ConvertToPrefab(true);
            Canvas editorCanvas = UIHelpers.CreateBlankUIScreen("EditorCanvas", true, false);
            editorCanvas.transform.SetParent(editorControllerObject.transform, false);
            editorCanvas.gameObject.SetActive(true);
            editorCanvas.referencePixelsPerUnit = 100f;
            editorCanvas.gameObject.AddComponent<PlaneDistance>();
            editorCanvas.renderMode = RenderMode.ScreenSpaceCamera;

            GameObject spawnPointVisual = new GameObject("SpawnpointVisual");
            GameObject spawnPointVisualVisual = CreateQuad("Visual", spawnpointMat, Vector3.zero, Vector3.zero);
            spawnPointVisualVisual.transform.forward = Vector3.down;
            spawnPointVisualVisual.transform.SetParent(spawnPointVisual.transform, true);
            spawnPointVisual.ConvertToPrefab(true);
            spawnPointVisual.AddComponent<BoxCollider>().size = new Vector3(10f, 0.025f, 10f);
            spawnPointVisual.layer = editorInteractableLayer;
            SpawnpointMoverAndVisualizerScript sm = spawnPointVisual.AddComponent<SpawnpointMoverAndVisualizerScript>();

            sm.actualTransform = new GameObject("SpawnDummy").transform;
            sm.actualTransform.SetParent(sm.transform);

            // set up tooltips
            RectTransform tooltipBase = workerCgm.pauseScreen.transform.Find("Options").Find("TooltipBase").GetComponent<RectTransform>();
            RectTransform editorTooltip = GameObject.Instantiate<RectTransform>(tooltipBase, editorCanvas.transform);
            editorTooltip.name = "TooltipBase";
            editorTooltip.anchoredPosition = Vector2.zero;
            TooltipController toolTipController = editorCanvas.gameObject.AddComponent<TooltipController>();
            toolTipController.ReflectionSetVariable("tooltipBgRect", editorTooltip.transform.Find("Tooltip").Find("BG").GetComponent<RectTransform>());
            toolTipController.ReflectionSetVariable("tooltipTmp", editorTooltip.transform.Find("Tooltip").Find("Tmp").GetComponent<TextMeshProUGUI>());
            toolTipController.ReflectionSetVariable("tooltipRect", editorTooltip.transform.Find("Tooltip").GetComponent<RectTransform>());

            EnvironmentController ecPrefab = GameObject.Instantiate<EnvironmentController>(Resources.FindObjectsOfTypeAll<EnvironmentController>().First(x => x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform);
            ecPrefab.name = "WorkerEnvironmentController";
            Tile newTilePrefab = GameObject.Instantiate<Tile>(Resources.FindObjectsOfTypeAll<Tile>().First(x => x.GetInstanceID() >= 0), MTM101BaldiDevAPI.prefabTransform);
            newTilePrefab.name = "EditorTile";
            newTilePrefab.MeshRenderer.material = assetMan.Get<Material>("tileAlpha");
            ecPrefab.ReflectionSetVariable("tilePre", newTilePrefab);

            UIHelpers.AddCursorInitiatorToCanvas(editorCanvas).useRawPosition = true;
            EditorController standardEditorController = editorControllerObject.AddComponent<EditorController>();
            standardEditorController.ReflectionSetVariable("destroyOnLoad", true);
            standardEditorController.cameraPrefab = assetMan.Get<GameCamera>("gameCam");
            standardEditorController.canvas = editorCanvas;
            standardEditorController.selectorPrefab = selector;
            standardEditorController.gridManagerPrefab = gridManager;
            standardEditorController.ecPrefab = ecPrefab;
            standardEditorController.cgmPrefab = workerCgm;
            standardEditorController.tooltipController = toolTipController;
            standardEditorController.tooltipBase = editorTooltip;
            standardEditorController.baseGameManagerPrefab = emg;
            // quick pause to create the gameloader prefab
            GameObject gameLoaderPreObject = new GameObject("EditorGameLoader");
            gameLoaderPreObject.ConvertToPrefab(true);
            GameLoader gameLoaderPre = gameLoaderPreObject.AddComponent<GameLoader>();
            gameLoaderPre.cgmPre = Resources.FindObjectsOfTypeAll<CoreGameManager>().First(x => x.name == "CoreGameManager" && x.GetInstanceID() >= 0);
            assetMan.Add<GameLoader>("gameLoaderPrefab", gameLoaderPre);
            assetMan.Add<ElevatorScreen>("elevatorScreenPrefab", Resources.FindObjectsOfTypeAll<ElevatorScreen>().First(x => x.GetInstanceID() >= 0 && x.transform.parent == null));
            assetMan.Add<EditorPlayModeManager>("playModeManager", playModeManager);
            standardEditorController.gameObject.AddComponent<BillboardManager>();

            standardEditorController.spawnpointVisualPrefab = sm;

            assetMan.Add<EditorController>("MainEditorController", standardEditorController);

            yield return "Setting up misc editor stuff...";

            GameObject genericPlaneVisual = new GameObject("genericPlaneVisual");
            GameObject genericPlaneVisualVisual = CreateQuad("Visual", null, Vector3.zero, Vector3.zero);
            genericPlaneVisualVisual.transform.forward = Vector3.down;
            genericPlaneVisualVisual.transform.SetParent(genericPlaneVisual.transform, true);
            genericPlaneVisual.AddComponent<BoxCollider>().size = new Vector3(10f, 0.025f, 10f);
            genericPlaneVisual.layer = editorInteractableLayer;
            EditorRendererContainer genericPlaneRenderContainer = genericPlaneVisual.AddComponent<EditorRendererContainer>();
            genericPlaneRenderContainer.AddRenderer(genericPlaneVisualVisual.GetComponent<MeshRenderer>(), "white");
            genericPlaneVisual.AddComponent<EditorDeletableObject>().renderContainer = genericPlaneRenderContainer;

            GameObject potentialDoorVisual = GameObject.Instantiate(genericPlaneVisual, MTM101BaldiDevAPI.prefabTransform);
            potentialDoorVisual.name = "PotentialDoorPrefab";
            potentialDoorVisual.GetComponentInChildren<MeshRenderer>().material = potentialDoorMat;
            genericStructureDisplays.Add("technical_potentialdoor", potentialDoorVisual);

            GameObject forcedDoorVisual = GameObject.Instantiate(genericPlaneVisual, MTM101BaldiDevAPI.prefabTransform);
            forcedDoorVisual.name = "ForcedDoorPrefab";
            forcedDoorVisual.GetComponentInChildren<MeshRenderer>().material = forcedDoorMat;
            genericStructureDisplays.Add("technical_forceddoor", forcedDoorVisual);

            GameObject unsafeCellVisual = GameObject.Instantiate(genericPlaneVisual, MTM101BaldiDevAPI.prefabTransform);
            unsafeCellVisual.name = "UnsafeCellPrefab";
            unsafeCellVisual.GetComponentInChildren<MeshRenderer>().material = forcedUnsafeMat;
            genericStructureDisplays.Add("technical_nosafe", unsafeCellVisual);
            structureTypes.Add("technical_potentialdoor", typeof(PotentialDoorLocation));
            structureTypes.Add("technical_forceddoor", typeof(ForcedDoorLocation));
            structureTypes.Add("technical_nosafe", typeof(UnsafeCellLocation));
            structureTypes.Add("technical_lightspot", typeof(RoomLightLocation));

            GameObject technicalLightVisual = GameObject.Instantiate(lightDisplayObject, MTM101BaldiDevAPI.prefabTransform);
            technicalLightVisual.name = "TechnicalLightVisual";
            DestroyImmediate(technicalLightVisual.GetComponentInChildren<SettingsComponent>());
            technicalLightVisual.GetComponentInChildren<SpriteRenderer>().sprite = AssetLoader.SpriteFromMod(this, Vector2.one / 2f, 32f, "Editor", "LightbulbRoom.png");
            genericStructureDisplays.Add("technical_lightspot", technicalLightVisual);

            // okay we're done with this
            Destroy(genericPlaneVisual);

            // markers
            GameObject matchBalloonVisual = EditorInterface.AddMarkerGenericVisual("matchballoon", Resources.FindObjectsOfTypeAll<MatchActivityBalloon>().First(x => x.name == "MatchBalloon_0" && x.GetInstanceID() >= 0).gameObject);
            MovableObjectInteraction matchBalloonMove = matchBalloonVisual.AddComponent<MovableObjectInteraction>();
            matchBalloonMove.allowedRotations = RotateAxis.None;
            matchBalloonMove.allowedAxis = MoveAxis.Horizontal;
            GameObject matchBalloonIngame = new GameObject("EditorMatchBalloonMarker");
            matchBalloonIngame.ConvertToPrefab(true);
            LevelLoaderPlugin.Instance.basicObjects.Add("matchballoon", matchBalloonIngame);
            markerTypes.Add("matchballoon", typeof(MatchBalloonMarker));

            yield return "Setting up UI assets...";

            Sprite[] allVanillaSprites = Resources.FindObjectsOfTypeAll<Sprite>().Where(x => x.GetInstanceID() >= 0).ToArray();

            uiAssetMan.Add<Sprite>("BackArrow_0", allVanillaSprites.First(x => x.name == "BackArrow_0"));
            uiAssetMan.Add<Sprite>("BackArrow_1", allVanillaSprites.First(x => x.name == "BackArrow_1"));
            uiAssetMan.Add<Sprite>("ItemSlot_0", allVanillaSprites.First(x => x.name == "ItemSlot_Dynamic_0"));
            uiAssetMan.Add<Sprite>("ItemSlot_1", allVanillaSprites.First(x => x.name == "ItemSlot_Dynamic_1"));
            uiAssetMan.Add<Sprite>("ItemSlot_2", allVanillaSprites.First(x => x.name == "ItemSlot_Dynamic_2"));
            uiAssetMan.Add<Sprite>("QuestionMark0", allVanillaSprites.First(x => x.name == "QMark_Sheet_0"));
            uiAssetMan.Add<Sprite>("QuestionMark1", allVanillaSprites.First(x => x.name == "QMark_Sheet_1"));
            uiAssetMan.Add<Sprite>("ChalkBoardStandard", allVanillaSprites.First(x => x.name == "ChalkBoardStandard"));
            uiAssetMan.Add<Sprite>("BackArrow_0", allVanillaSprites.First(x => x.name == "BackArrow_0"));
            uiAssetMan.Add<Sprite>("MenuArrowLeft", allVanillaSprites.First(x => x.name == "MenuArrowSheet_2"));
            uiAssetMan.Add<Sprite>("MenuArrowLeftHigh", allVanillaSprites.First(x => x.name == "MenuArrowSheet_0"));
            uiAssetMan.Add<Sprite>("MenuArrowRight", allVanillaSprites.Where(x => x.name == "MenuArrowSheet_3").First());
            uiAssetMan.Add<Sprite>("MenuArrowRightHigh", allVanillaSprites.First(x => x.name == "MenuArrowSheet_1"));
            uiAssetMan.Add<Sprite>("OptionsClipboard", allVanillaSprites.First(x => x.name == "OptionsClipboard"));
            uiAssetMan.Add<Sprite>("Check", allVanillaSprites.First(x => x.name == "YCTP_IndicatorsSheet_0"));
            uiAssetMan.Add<Sprite>("CheckBox", allVanillaSprites.First(x => x.name == "CheckBox"));

            uiAssetMan.Add<Sprite>("Segment0", allVanillaSprites.First(x => x.name == "Segment_Sheet_0"));
            uiAssetMan.Add<Sprite>("Segment1", allVanillaSprites.First(x => x.name == "Segment_Sheet_1"));
            uiAssetMan.Add<Sprite>("Segment2", allVanillaSprites.First(x => x.name == "Segment_Sheet_2"));
            uiAssetMan.Add<Sprite>("Segment3", allVanillaSprites.First(x => x.name == "Segment_Sheet_3"));
            uiAssetMan.Add<Sprite>("Segment4", allVanillaSprites.First(x => x.name == "Segment_Sheet_4"));
            uiAssetMan.Add<Sprite>("Segment5", allVanillaSprites.First(x => x.name == "Segment_Sheet_5"));
            uiAssetMan.Add<Sprite>("Segment6", allVanillaSprites.First(x => x.name == "Segment_Sheet_6"));
            uiAssetMan.Add<Sprite>("Segment7", allVanillaSprites.First(x => x.name == "Segment_Sheet_7"));
            uiAssetMan.Add<Sprite>("Segment8", allVanillaSprites.First(x => x.name == "Segment_Sheet_8"));
            uiAssetMan.Add<Sprite>("Segment9", allVanillaSprites.First(x => x.name == "Segment_Sheet_9"));
            uiAssetMan.Add<Sprite>("SegmentD", allVanillaSprites.First(x => x.name == "Segment_Sheet_10"));
            uiAssetMan.Add<Sprite>("BaldiSpeaksPoster", AssetLoader.SpriteFromTexture2D(baldiSaysTexture, 1f));
            uiAssetMan.Add<Sprite>("chk_blank", AssetLoader.SpriteFromTexture2D(chalkTexture, 1f));
            uiAssetMan.Add<Sprite>("BulletinBoard_Blank", AssetLoader.SpriteFromTexture2D(bulletinTexture, 1f));

            UIBuilder.elementBuilders.Add("image", new ImageBuilder());
            UIBuilder.elementBuilders.Add("imageButton", new ButtonBuilder());
            UIBuilder.elementBuilders.Add("hotslot", new HotSlotBuilder());
            UIBuilder.elementBuilders.Add("hotslottoolbox", new HotSlotToolboxBuilder());
            UIBuilder.elementBuilders.Add("blocker", new BlockerBuilder());
            UIBuilder.elementBuilders.Add("text", new TextBuilder());
            UIBuilder.elementBuilders.Add("dragdetect", new DragDetectorBuilder());
            UIBuilder.elementBuilders.Add("textbox", new TextBoxBuilder());
            UIBuilder.elementBuilders.Add("textbutton", new TextButtonBuilder());
            UIBuilder.elementBuilders.Add("rawimage", new RawImageBuilder());
            UIBuilder.elementBuilders.Add("rawimagebutton", new RawImageButtonBuilder());
            UIBuilder.elementBuilders.Add("segment", new DigitalNumberBuilder());
            UIBuilder.elementBuilders.Add("checkbox", new CheckboxBuilder());
            SpritesFromPath(Path.Combine(AssetLoader.GetModPath(this), "UI", "Editor"), "");

            List<string> eventSpritesToPrepare = new List<string>
            {
                "fog",
                "flood",
                "brokenruler",
                "party",
                "mysteryroom",
                "testprocedure",
                "gravitychaos",
                "studentshuffle",
                "balderdash"
            };

            for (int i = 0; i < eventSpritesToPrepare.Count; i++)
            {
                eventSprites.Add(eventSpritesToPrepare[i], uiAssetMan.Get<Sprite>("RandomEvents/" + eventSpritesToPrepare[i]));
            }

            skyboxSprites.Add("default", uiAssetMan.Get<Sprite>("Skyboxes/default"));
            skyboxSprites.Add("daystandard", uiAssetMan.Get<Sprite>("Skyboxes/daystandard"));
            skyboxSprites.Add("twilight", uiAssetMan.Get<Sprite>("Skyboxes/twilight"));
            skyboxSprites.Add("void", uiAssetMan.Get<Sprite>("Skyboxes/void"));

            selectableSkyboxes.Add("daystandard");
            selectableSkyboxes.Add("twilight");
            selectableSkyboxes.Add("void");
            selectableSkyboxes.Add("default");
        }

        void SpritesFromPath(string path, string prefix)
        {
            string[] paths = Directory.GetFiles(path, "*.png");
            for (int i = 0; i < paths.Length; i++)
            {
                Texture2D texture = AssetLoader.TextureFromFile(paths[i]);
                uiAssetMan.Add<Sprite>(prefix + texture.name, AssetLoader.SpriteFromTexture2D(texture, 1f));
            }
            string[] subDirectories = Directory.GetDirectories(path);
            for (int i = 0; i < subDirectories.Length; i++)
            {
                SpritesFromPath(subDirectories[i], prefix + new DirectoryInfo(subDirectories[i]).Name + "/");
            }
        }

        public static GameObject CreateQuad(string name, Material mat, Vector3 position, Vector3 rotation)
        {
            GameObject newQuad = new GameObject(name);
            newQuad.gameObject.AddComponent<MeshFilter>().mesh = Instance.assetMan.Get<Mesh>("Quad");
            newQuad.AddComponent<MeshRenderer>().material = mat;
            newQuad.transform.position = position;
            newQuad.transform.eulerAngles = rotation;
            newQuad.transform.localScale *= 10f;
            return newQuad;
        }

        IEnumerator LoadAssets()
        {
            yield return 4;
            yield return "Creating solid color lightmaps...";
            lightmaps.Add("none", Resources.FindObjectsOfTypeAll<Texture2D>().First(x => x.GetInstanceID() >= 0 && x.name == "LightMap"));
            AddSolidColorLightmap("white", Color.white);
            AddSolidColorLightmap("yellow", Color.yellow);
            AddSolidColorLightmap("red", Color.red);
            AddSolidColorLightmap("green", Color.green);
            AddSolidColorLightmap("blue", Color.blue);
            yield return "Loading title assets...";
            assetMan.Add<Sprite>("EditorButton", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "UI", "TitleScreen", "EditorButton.png"), 1f));
            assetMan.Add<Sprite>("EditorButtonGlow", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "UI", "TitleScreen", "EditorButtonGlow.png"), 1f));
            assetMan.Add<Sprite>("EditorButtonFail", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, "UI", "TitleScreen", "EditorButtonFail.png"), 1f));
            assetMan.Add<Sprite>("ChalkBackground", AssetLoader.SpriteFromMod(this, Vector2.one / 2f, 1f, "UI", "TitleScreen", "ChalkWMath480_256c.png"));
            assetMan.Add<Sprite>("MPlayButton", AssetLoader.SpriteFromMod(this, Vector2.one / 2f, 1f, "UI", "TitleScreen", "MPlayButton.png"));
            assetMan.Add<Sprite>("MPlayButtonHover", AssetLoader.SpriteFromMod(this, Vector2.one / 2f, 1f, "UI", "TitleScreen", "MPlayButtonHover.png"));
            assetMan.Add<Sprite>("MDiscardButton", AssetLoader.SpriteFromMod(this, Vector2.one / 2f, 1f, "UI", "TitleScreen", "MDiscardButton.png"));
            assetMan.Add<Sprite>("MDiscardButtonHover", AssetLoader.SpriteFromMod(this, Vector2.one / 2f, 1f, "UI", "TitleScreen", "MDiscardButtonHover.png"));
            assetMan.Add<Sprite>("PlayLevelBorder", AssetLoader.SpriteFromMod(this, Vector2.one / 2f, 1f, "UI", "TitleScreen", "PlayLevelBorder.png"));
            assetMan.Add<Texture2D>("IconMissing", AssetLoader.TextureFromMod(this, "UI", "TitleScreen", "IconMissing.png"));
            Sprite[] baseSprites = Resources.FindObjectsOfTypeAll<Sprite>().Where(x => x.GetInstanceID() >= 0).ToArray();

            assetMan.Add<Sprite>("BackArrow", baseSprites.First(x => x.name == "BackArrow_0"));
            assetMan.Add<Sprite>("BackArrowHighlight", baseSprites.First(x => x.name == "BackArrow_1"));
            yield return "Loading MIDIs...";
            string[] midiPaths = Directory.GetFiles(Path.Combine(AssetLoader.GetModPath(this), "MIDIs"), "*.mid");
            for (int i = 0; i < midiPaths.Length; i++)
            {
                editorTracks.Add(AssetLoader.MidiFromFile(midiPaths[i], "editorTrack_" + Path.GetFileNameWithoutExtension(midiPaths[i])));
            }
            yield return "Loading Localization...";
            AssetLoader.LocalizationFromMod(this);
        }
    }
}
