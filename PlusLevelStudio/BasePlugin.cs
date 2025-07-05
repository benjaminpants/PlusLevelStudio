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

        public Dictionary<string, DoorDisplay> doorDisplays = new Dictionary<string, DoorDisplay>();

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
                availableTools = new List<EditorTool>()
                {
                    new RoomTool("hall"),
                    new RoomTool("class"),
                    new RoomTool("faculty"),
                    new MergeTool(),
                    new DeleteTool(),
                    new LightTool("fluorescent"),
                    new DoorTool("standard")
                }
            };

            editorController.EditorModeAssigned();

        }

        public void GoToEditor()
        {
            StartCoroutine(LoadEditorScene());
        }

        IEnumerator FindObjectsAndSetupEditor()
        {
            List<Direction> directions = Directions.All();
            yield return 9;
            yield return "Grabbing necessary resources...";
            assetMan.Add<Mesh>("Quad", Resources.FindObjectsOfTypeAll<Mesh>().First(x => x.GetInstanceID() >= 0 && x.name == "Quad"));
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>().Where(x => x.GetInstanceID() >= 0).ToArray();
            EnvironmentController ecPrefab = Resources.FindObjectsOfTypeAll<EnvironmentController>().First(x => x.GetInstanceID() >= 0);
            assetMan.Add<Material>("tileAlpha", materials.First(x => x.name == "TileBase_Alpha"));
            assetMan.Add<Material>("spriteBillboard", materials.First(x => x.name == "SpriteStandard_Billboard"));
            assetMan.Add<Material>("doorMask", materials.First(x => x.name == "DoorMask"));
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

            Material gridArrowMat = new Material(gridMat);
            gridArrowMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "GridArrow.png"));
            gridArrowMat.name = "GridArrowMaterial";

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

            yield return "Creating Worker CoreGameManager...";
            CoreGameManager cgm = Resources.FindObjectsOfTypeAll<CoreGameManager>().First(x => x.name == "CoreGameManager" && x.GetInstanceID() >= 0);
            CoreGameManager workerCgm = GameObject.Instantiate<CoreGameManager>(cgm, MTM101BaldiDevAPI.prefabTransform);
            workerCgm.ReflectionSetVariable("destroyOnLoad", true);
            workerCgm.gameObject.SetActive(false);
            workerCgm.name = "WorkerCoreGameManager";

            yield return "Creating editor prefab visuals...";
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
            lightEdo.myRenderers = new List<Renderer> { lightSpriteRenderer };

            // create door visuals
            GameObject standardDoorDisplayObject = new GameObject("StandardDoorVisual");
            standardDoorDisplayObject.transform.SetParent(MTM101BaldiDevAPI.prefabTransform);
            GameObject sideAQuad = CreateQuad("SideA", assetMan.Get<Material>("doorMask"), Vector3.zero, Vector3.zero);
            GameObject sideBQuad = CreateQuad("SideB", assetMan.Get<Material>("doorMask"), Vector3.zero, new Vector3(0f,180f,0f));
            sideAQuad.transform.SetParent(standardDoorDisplayObject.transform);
            sideBQuad.transform.SetParent(standardDoorDisplayObject.transform);
            EditorDeletableObject doorDisplayDeletable = standardDoorDisplayObject.AddComponent<EditorDeletableObject>();
            doorDisplayDeletable.myRenderers.Add(sideAQuad.GetComponent<MeshRenderer>());
            doorDisplayDeletable.myRenderers.Add(sideBQuad.GetComponent<MeshRenderer>());
            DoorDisplay standardDoorDisplayBehavior = standardDoorDisplayObject.AddComponent<DoorDisplay>();
            standardDoorDisplayBehavior.sideA = sideAQuad.GetComponent<MeshRenderer>();
            standardDoorDisplayBehavior.sideB = sideBQuad.GetComponent<MeshRenderer>();
            EditorDeletableObject doorDelete = standardDoorDisplayObject.AddComponent<EditorDeletableObject>();
            doorDelete.myRenderers = new List<Renderer>() { standardDoorDisplayBehavior.sideA, standardDoorDisplayBehavior.sideB };
            standardDoorDisplayObject.AddComponent<BoxCollider>().size = new Vector3(10f,10f,0.5f);
            standardDoorDisplayObject.layer = editorInteractableLayer;
            doorDisplays.Add("standard", standardDoorDisplayBehavior);

            yield return "Setting up Editor Controller...";
            GameObject editorControllerObject = new GameObject("StandardEditorController");
            editorControllerObject.ConvertToPrefab(true);
            Canvas editorCanvas = UIHelpers.CreateBlankUIScreen("EditorCanvas", true, false);
            editorCanvas.transform.SetParent(editorControllerObject.transform, false);
            editorCanvas.gameObject.SetActive(true);
            editorCanvas.referencePixelsPerUnit = 100f;
            editorCanvas.gameObject.AddComponent<PlaneDistance>();
            editorCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            UIHelpers.AddCursorInitiatorToCanvas(editorCanvas).useRawPosition = true;
            EditorController standardEditorController = editorControllerObject.AddComponent<EditorController>();
            standardEditorController.ReflectionSetVariable("destroyOnLoad", true);
            standardEditorController.cameraPrefab = assetMan.Get<GameCamera>("gameCam");
            standardEditorController.canvas = editorCanvas;
            standardEditorController.selectorPrefab = selector;
            standardEditorController.gridManagerPrefab = gridManager;
            standardEditorController.ecPrefab = ecPrefab;
            standardEditorController.cgmPrefab = workerCgm;
            // quick pause to create the gameloader prefab
            GameObject gameLoaderPreObject = new GameObject("EditorGameLoader");
            gameLoaderPreObject.ConvertToPrefab(true);
            GameLoader gameLoaderPre = gameLoaderPreObject.AddComponent<GameLoader>();
            gameLoaderPre.cgmPre = Resources.FindObjectsOfTypeAll<CoreGameManager>().First(x => x.name == "CoreGameManager" && x.GetInstanceID() >= 0);
            standardEditorController.gameLoaderPrefab = gameLoaderPre;
            standardEditorController.elevatorScreenPrefab = Resources.FindObjectsOfTypeAll<ElevatorScreen>().First(x => x.GetInstanceID() >= 0 && x.transform.parent == null);

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

            UIBuilder.elementBuilders.Add("image", new ImageBuilder());
            UIBuilder.elementBuilders.Add("imageButton", new ButtonBuilder());
            UIBuilder.elementBuilders.Add("hotslot", new HotSlotBuilder());
            UIBuilder.elementBuilders.Add("hotslotSpecial", new SpecialHotSlotBuilder());
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
            yield return 3;
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
        }
    }
}
