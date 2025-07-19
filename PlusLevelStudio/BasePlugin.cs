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

namespace PlusLevelStudio
{
    [BepInPlugin("mtm101.rulerp.baldiplus.levelstudio", "Plus Level Studio", "0.0.0.0")]
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
        public Dictionary<string, bool> doorIsTileBased = new Dictionary<string, bool>();
        public Dictionary<string, DoorDisplay> windowDisplays = new Dictionary<string, DoorDisplay>();
        public Dictionary<string, GameObject> exitDisplays = new Dictionary<string, GameObject>();
        public Dictionary<string, EditorBasicObject> basicObjectDisplays = new Dictionary<string, EditorBasicObject>();
        public Dictionary<string, GameObject> activityDisplays = new Dictionary<string, GameObject>();
        public Dictionary<string, Type> structureTypes = new Dictionary<string, Type>();
        public Dictionary<string, GameObject> genericStructureDisplays = new Dictionary<string, GameObject>();
        public Dictionary<string, GameObject> npcDisplays = new Dictionary<string, GameObject>();
        public List<string> selectableTextures = new List<string>();
        public GameObject pickupVisual;
        public GameObject posterVisual;

        public static string levelFilePath => Path.Combine(Application.persistentDataPath, "Custom Levels");

        private Dictionary<Texture2D, Sprite> smallIconsFromTextures = new Dictionary<Texture2D, Sprite>();

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
            harmony.PatchAllConditionals();
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

        public IEnumerator LoadEditorScene()
        {
            AsyncOperation waitForSceneLoad = SceneManager.LoadSceneAsync("Game");
            while (!waitForSceneLoad.isDone)
            {
                yield return null;
            }
            Shader.SetGlobalTexture("_Skybox", Resources.FindObjectsOfTypeAll<Cubemap>().First(x => x.name == "Cubemap_DayStandard"));
            Shader.SetGlobalColor("_SkyboxColor", Color.white);
            Shader.SetGlobalColor("_FogColor", Color.white);
            Shader.SetGlobalFloat("_FogStartDistance", 5f);
            Shader.SetGlobalFloat("_FogMaxDistance", 100f);
            Shader.SetGlobalFloat("_FogStrength", 0f);

            EditorController editorController = GameObject.Instantiate<EditorController>(assetMan.Get<EditorController>("MainEditorController"));

            // TODO: put code that actually does logic for assigning editor mode here instead of just creating it on the fly
            editorController.currentMode = new EditorMode()
            {
                id = "full",
                availableTools = new Dictionary<string, List<EditorTool>>()
                {
                    { "rooms", new List<EditorTool>()
                    {
                        new RoomTool("hall"),
                        new RoomTool("class"),
                        new RoomTool("faculty"),
                        new RoomTool("office"),
                        new RoomTool("closet"),
                        new RoomTool("reflex"),
                        new RoomTool("cafeteria"),
                        new RoomTool("outside"),
                        new RoomTool("library"),
                        new RoomTool("lightbulbtesting")
                    } },
                    { "doors", new List<EditorTool>()
                    {
                        new DoorTool("standard"),
                        new DoorTool("swinging"),
                        new DoorTool("oneway"),
                        new DoorTool("coinswinging"),
                        new DoorTool("swinging_silent"),
                        new DoorTool("autodoor"),
                        new DoorTool("flaps"),
                        new WindowTool("standard"),
                    } },
                    { "items", new List<EditorTool>()
                    {
                        new ItemTool("quarter"),
                        new ItemTool("dietbsoda"),
                        new ItemTool("bsoda"),
                        new ItemTool("zesty"),
                        new ItemTool("banana"),
                        new ItemTool("scissors"),
                        new ItemTool("boots"),
                        new ItemTool("nosquee"),
                        new ItemTool("keys"),
                        new ItemTool("tape"),
                        new ItemTool("clock"),
                        new ItemTool("swinglock"),
                        new ItemTool("whistle"),
                        new ItemTool("dirtychalk"),
                        new ItemTool("nametag"),
                        new ItemTool("inviselixer"),
                        new ItemTool("reachextend"),
                        new ItemTool("teleporter"),
                        new ItemTool("portalposter"),
                        new ItemTool("grapple"),
                        new ItemTool("apple"),
                        new ItemTool("buspass"),
                        new ItemTool("shapekey_circle"),
                        new ItemTool("shapekey_triangle"),
                        new ItemTool("shapekey_square"),
                        new ItemTool("shapekey_star"),
                        new ItemTool("shapekey_heart"),
                        new ItemTool("shapekey_weird"),
                        new ItemTool("points25", uiAssetMan.Get<Sprite>("Tools/items_points25")),
                        new ItemTool("points50", uiAssetMan.Get<Sprite>("Tools/items_points50")),
                        new ItemTool("points100", uiAssetMan.Get<Sprite>("Tools/items_points100")),
                    } },
                    { "lights", new List<EditorTool>()
                    {
                        new LightTool("fluorescent"),
                        new LightTool("caged"),
                        new LightTool("cordedhanging"),
                        new LightTool("standardhanging")
                    } },
                    { "activities", new List<EditorTool>()
                    {
                        new ActivityTool("notebook", 5f),
                        new ActivityTool("mathmachine", 0f),
                        new ActivityTool("mathmachine_corner", 0f),
                    } },
                    { "objects", new List<EditorTool>()
                    {
                        new ObjectTool("bigdesk"),
                        new ObjectTool("desk"),
                        new ObjectTool("chair"),
                        new ObjectTool("roundtable"),
                        new ObjectTool("cabinet"),
                        new ObjectTool("cafeteriatable"),
                        new ObjectTool("waterfountain"),
                        new ObjectTool("dietbsodamachine"),
                        new ObjectTool("bsodamachine"),
                        new ObjectTool("zestymachine"),
                        new ObjectTool("crazymachine_zesty"),
                        new ObjectTool("crazymachine_bsoda"),
                        new ObjectTool("payphone"),
                        new ObjectTool("tapeplayer", 5f),
                        new ObjectTool("locker"),
                        new ObjectTool("bluelocker"),
                        new ObjectTool("greenlocker"),
                        new ObjectTool("computer", 3.75f),
                        new ObjectTool("computer_off", 3.75f),
                        new ObjectTool("rounddesk"),
                        new ObjectTool("bookshelf"),
                        new ObjectTool("bookshelf_hole"),
                        new ObjectTool("pedestal"),
                        new ObjectTool("tree"),
                        new ObjectTool("appletree"),
                        new ObjectTool("bananatree"),
                        new ObjectTool("hopscotch"),
                        new ObjectTool("hoop"),
                        new ObjectTool("merrygoround"),
                        new ObjectTool("counter"),
                        new ObjectTool("examinationtable"),
                    } },
                    { "structures", new List<EditorTool>()
                    {
                        new HallDoorStructureTool("facultyonlydoor"),
                        new HallDoorWithButtonsTool("lockdowndoor")
                    } },
                    { "npcs", new List<EditorTool>()
                    {
                        new NPCTool("baldi"),
                        new NPCTool("principal"),
                        new NPCTool("sweep"),
                        new NPCTool("playtime"),
                        new NPCTool("bully"),
                        new NPCTool("crafters"),
                        new NPCTool("prize"),
                        new NPCTool("cloudy"),
                        new NPCTool("chalkface"),
                        new NPCTool("beans"),
                        new NPCTool("pomp"),
                        new NPCTool("test"),
                        new NPCTool("reflex"),
                    } },
                    { "posters", new List<EditorTool>()},
                    { "tools", new List<EditorTool>()
                    {
                        new ElevatorTool("elevator", true),
                        new ElevatorTool("elevator", false),
                        new SpawnpointTool(),
                        new MergeTool(),
                        new DeleteTool(),
                    } }
                },
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
                defaultTools = new string[] { "room_hall", "room_class", "room_faculty", "room_office", "room_closet", "light_fluorescent", "door_standard", "merge", "delete"}
            };

            List<PosterObject> allPosters = LevelLoaderPlugin.Instance.posterAliases.Values.Where(x => x.GetInstanceID() >= 0).ToList();
            allPosters.Sort((a, b) =>
            {
                int texNameCompare = a.baseTexture.name.CompareTo(b.baseTexture.name);
                if (texNameCompare != 0)
                {
                    return texNameCompare;
                }
                return a.name.CompareTo(b.name);
            });
            foreach (PosterObject poster in allPosters)
            {
                editorController.currentMode.availableTools["posters"].Add(new PosterTool(LevelLoaderPlugin.Instance.posterAliases.First(x => x.Value == poster).Key));
            }

            editorController.EditorModeAssigned();

        }

        public void GoToEditor()
        {
            StartCoroutine(LoadEditorScene());
        }

        IEnumerator FindObjectsAndSetupEditor()
        {
            List<Direction> directions = Directions.All();
            yield return 10;
            yield return "Grabbing necessary resources...";
            assetMan.Add<Mesh>("Quad", Resources.FindObjectsOfTypeAll<Mesh>().First(x => x.GetInstanceID() >= 0 && x.name == "Quad"));
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>().Where(x => x.GetInstanceID() >= 0).ToArray();
            EnvironmentController ecPrefab = Resources.FindObjectsOfTypeAll<EnvironmentController>().First(x => x.GetInstanceID() >= 0);
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

            Material spawnpointMat = new Material(assetMan.Get<Material>("tileAlpha"));
            spawnpointMat.name = "SpawnpointMat";
            spawnpointMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "SpawnpointSprite.png"));
            spawnpointMat.SetTexture("_LightMap", lightmaps["white"]);

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
            lightEdo.AddRenderer(lightSpriteRenderer, "white");

            // create door visuals
            EditorInterface.AddDoor<StandardDoorDisplay>("standard", false, assetMan.Get<Material>("doorMask"), null);
            EditorInterface.AddDoor<DoorDisplay>("swinging", true, assetMan.Get<Material>("swingingDoorMask"), new Material[] { assetMan.Get<Material>("SwingingDoorMat"), assetMan.Get<Material>("SwingingDoorMat") });
            EditorInterface.AddDoor<DoorDisplay>("oneway", true, assetMan.Get<Material>("swingingDoorMask"), new Material[] { assetMan.Get<Material>("OneWayRight"), assetMan.Get<Material>("OneWayWrong") });
            EditorInterface.AddDoor<DoorDisplay>("swinging_silent", true, assetMan.Get<Material>("swingingDoorMask"), new Material[] { silentDoorMat, silentDoorMat });
            EditorInterface.AddDoor<DoorDisplay>("coinswinging", true, assetMan.Get<Material>("swingingDoorMask"), new Material[] { assetMan.Get<Material>("CoinDoorMat"), assetMan.Get<Material>("CoinDoorMat") });
            EditorInterface.AddDoor<DoorDisplay>("autodoor", false, assetMan.Get<Material>("AutoDoorMask"), new Material[] { assetMan.Get<Material>("AutoDoorMat"), assetMan.Get<Material>("AutoDoorMat") });
            EditorInterface.AddDoor<DoorDisplay>("flaps", true, assetMan.Get<Material>("FlapDoorMask"), new Material[] { assetMan.Get<Material>("FlapDoorMat"), assetMan.Get<Material>("FlapDoorMat") });

            WindowObject standardWindowObject = Resources.FindObjectsOfTypeAll<WindowObject>().First(x => x.name == "WoodWindow" && x.GetInstanceID() >= 0);
            EditorInterface.AddWindow<DoorDisplay>("standard", standardWindowObject.mask, standardWindowObject.overlay);

            // elevators
            EditorInterface.AddExit("elevator", LevelLoaderPlugin.Instance.exitDatas["elevator"].prefab);

            // pickup visual
            pickupVisual = EditorInterface.CloneToPrefabStripMonoBehaviors(Resources.FindObjectsOfTypeAll<Pickup>().First(x => x.transform.parent == null && x.GetInstanceID() >= 0 && x.name == "Pickup").gameObject);
            pickupVisual.AddComponent<EditorDeletableObject>().AddRenderer(pickupVisual.transform.Find("ItemSprite").GetComponent<SpriteRenderer>(), "none");
            pickupVisual.name = "PickupVisual";
            pickupVisual.AddComponent<MovableObjectInteraction>().allowedAxis = MoveAxis.Horizontal;
            pickupVisual.layer = editorInteractableLayer;

            // poster visual
            GameObject posterVisualBase = new GameObject("PosterVisual");
            posterVisualBase.ConvertToPrefab(true);
            GameObject wallQuad = CreateQuad("PosterWall", new Material(assetMan.Get<Material>("tileAlpha")), new Vector3(0f,5f,4.999f), Vector3.zero);
            wallQuad.transform.SetParent(posterVisualBase.transform, true);
            BoxCollider collider = posterVisualBase.AddComponent<BoxCollider>();
            collider.size = new Vector3(10f,10f,0.02f);
            collider.center = wallQuad.transform.localPosition;
            posterVisualBase.layer = editorInteractableLayer;
            collider.gameObject.AddComponent<EditorDeletableObject>().AddRenderer(wallQuad.GetComponent<MeshRenderer>(), "none");
            posterVisual = posterVisualBase;

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
            // gotta fix this up
            EditorBasicObject appleTreeVisual = EditorInterface.AddObjectVisual("appletree", LevelLoaderPlugin.Instance.basicObjects["appletree"], true);
            appleTreeVisual.transform.Find("Sprite").Find("Pickup").Find("ItemSprite").GetComponent<SpriteRenderer>().sprite = ItemMetaStorage.Instance.FindByEnum(Items.Apple).value.itemSpriteLarge;
            EditorInterface.AddObjectVisual("bananatree", LevelLoaderPlugin.Instance.basicObjects["bananatree"], true);

            EditorInterface.AddObjectVisual("counter", LevelLoaderPlugin.Instance.basicObjects["counter"], true);
            EditorInterface.AddObjectVisual("examinationtable", LevelLoaderPlugin.Instance.basicObjects["examinationtable"], true);
            EditorInterface.AddObjectVisual("merrygoround", LevelLoaderPlugin.Instance.basicObjects["merrygoround"], true);

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
            mathMachineCollider.gameObject.AddComponent<EditorDeletableObject>().AddRendererRange(mathMachineVisual.GetComponentsInChildren<Renderer>(), "none");
            mathMachineVisual.gameObject.layer = LevelStudioPlugin.editorInteractableLayer;
            mathMachineCollider.gameObject.layer = LevelStudioPlugin.editorInteractableLayer;

            GameObject mathMachineCornerVisual = EditorInterface.CloneToPrefabStripMonoBehaviors(LevelLoaderPlugin.Instance.activityAliases["mathmachine_corner"].gameObject, new Type[] { typeof(TMP_Text) });
            mathMachineCornerVisual.name = mathMachineCornerVisual.name.Replace("_Stripped", "_Visual");
            DestroyImmediate(mathMachineCornerVisual.transform.Find("Buffer").gameObject);
            BoxCollider mathMachineCornerCollider = mathMachineCornerVisual.transform.Find("Model").GetComponent<BoxCollider>();
            MovableObjectInteraction mathMachineCornermovableObjectInteract = mathMachineCornerCollider.gameObject.AddComponent<MovableObjectInteraction>();
            mathMachineCornermovableObjectInteract.allowedRotations = RotateAxis.Flat;
            mathMachineCornermovableObjectInteract.allowedAxis = MoveAxis.All;
            mathMachineCornerCollider.gameObject.AddComponent<EditorDeletableObject>().AddRendererRange(mathMachineCornerVisual.GetComponentsInChildren<Renderer>(), "none");
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

            // npcs

            EditorInterface.AddNPCVisual("baldi", LevelLoaderPlugin.Instance.npcAliases["baldi"]);
            EditorInterface.AddNPCVisual("principal", LevelLoaderPlugin.Instance.npcAliases["principal"]);
            EditorInterface.AddNPCVisual("sweep", LevelLoaderPlugin.Instance.npcAliases["sweep"]);
            EditorInterface.AddNPCVisual("playtime", LevelLoaderPlugin.Instance.npcAliases["playtime"]);
            GameObject chalklesVisual = EditorInterface.AddNPCVisual("chalkface", LevelLoaderPlugin.Instance.npcAliases["chalkface"]);
            chalklesVisual.transform.Find("SpriteBase").Find("Sprite").gameObject.SetActive(true);
            chalklesVisual.GetComponent<EditorDeletableObject>().AddRenderer(chalklesVisual.GetComponentInChildren<Renderer>(), "none");
            EditorInterface.AddNPCVisual("bully", LevelLoaderPlugin.Instance.npcAliases["bully"]);
            EditorInterface.AddNPCVisual("beans", LevelLoaderPlugin.Instance.npcAliases["beans"]);
            EditorInterface.AddNPCVisual("prize", LevelLoaderPlugin.Instance.npcAliases["prize"]);
            EditorInterface.AddNPCVisual("crafters", LevelLoaderPlugin.Instance.npcAliases["crafters"]);
            EditorInterface.AddNPCVisual("pomp", LevelLoaderPlugin.Instance.npcAliases["pomp"]);
            EditorInterface.AddNPCVisual("test", LevelLoaderPlugin.Instance.npcAliases["test"]);
            EditorInterface.AddNPCVisual("cloudy", LevelLoaderPlugin.Instance.npcAliases["cloudy"]);
            EditorInterface.AddNPCVisual("reflex", LevelLoaderPlugin.Instance.npcAliases["reflex"]);

            yield return "Configuring Misc...";

            /*
            roomTextureAliases.Add("ElevatorBack", textures.First(x => x.name == "ElBack"));
            roomTextureAliases.Add("FactoryCeiling", textures.First(x => x.name == "Factory_Ceiling"));
            roomTextureAliases.Add("FactoryBeams", textures.First(x => x.name == "FactoryBeams_Texture"));
            roomTextureAliases.Add("LabFloor", textures.First(x => x.name == "LabFloor_Texture"));
            roomTextureAliases.Add("LabWall", textures.First(x => x.name == "LabWall_Texture"));
            roomTextureAliases.Add("LabCeiling", textures.First(x => x.name == "LabCeiling_Texture"));
            roomTextureAliases.Add("Ground2", textures.First(x => x.name == "ground2"));
            roomTextureAliases.Add("DiamondPlateFloor", textures.First(x => x.name == "DiamongPlateFloor"));
            roomTextureAliases.Add("Corn", textures.First(x => x.name == "Corn"));
            */
            selectableTextures.Add("HallFloor");
            selectableTextures.Add("Wall");
            selectableTextures.Add("Ceiling");
            selectableTextures.Add("BlueCarpet");
            selectableTextures.Add("WallWithMolding");
            selectableTextures.Add("TileFloor");
            selectableTextures.Add("ElevatorCeiling");
            selectableTextures.Add("Grass");
            selectableTextures.Add("Fence");
            selectableTextures.Add("JohnnyWall");
            selectableTextures.Add("None");
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
            selectableTextures.Add("Corn");

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
            // quick pause to create the gameloader prefab
            GameObject gameLoaderPreObject = new GameObject("EditorGameLoader");
            gameLoaderPreObject.ConvertToPrefab(true);
            GameLoader gameLoaderPre = gameLoaderPreObject.AddComponent<GameLoader>();
            gameLoaderPre.cgmPre = Resources.FindObjectsOfTypeAll<CoreGameManager>().First(x => x.name == "CoreGameManager" && x.GetInstanceID() >= 0);
            standardEditorController.gameLoaderPrefab = gameLoaderPre;
            standardEditorController.elevatorScreenPrefab = Resources.FindObjectsOfTypeAll<ElevatorScreen>().First(x => x.GetInstanceID() >= 0 && x.transform.parent == null);
            standardEditorController.gameObject.AddComponent<BillboardManager>();

            standardEditorController.spawnpointVisualPrefab = sm;

            assetMan.Add<EditorController>("MainEditorController", standardEditorController);

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
            SpritesFromPath(Path.Combine(AssetLoader.GetModPath(this), "UI", "Editor"), "");
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
