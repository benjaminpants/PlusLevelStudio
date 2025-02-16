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

namespace PlusLevelStudio
{
    [BepInPlugin("mtm101.rulerp.baldiplus.levelstudio", "Plus Level Studio", "0.0.0.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi")]
    public class LevelStudioPlugin : BaseUnityPlugin
    {
        public static LevelStudioPlugin Instance;
        public bool isFucked = false;
        public AssetManager assetMan = new AssetManager();
        public List<string> editorTracks = new List<string>();
        public Dictionary<string, Texture2D> lightmaps = new Dictionary<string, Texture2D>();

        void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("mtm101.rulerp.baldiplus.levelstudio");
            LoadingEvents.RegisterOnLoadingScreenStart(Info, LoadAssets());
            LoadingEvents.RegisterOnAssetsLoaded(Info, FindObjectsAndSetupEditor(), false);
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
            Shader.SetGlobalTexture("_Skybox", SkyboxMetaStorage.Instance.Find((x) => x.value.name == "Cubemap_DayStandard").value);
            Shader.SetGlobalColor("_SkyboxColor", Color.white);
            Shader.SetGlobalColor("_FogColor", Color.white);
            Shader.SetGlobalFloat("_FogStartDistance", 5f);
            Shader.SetGlobalFloat("_FogMaxDistance", 100f);
            Shader.SetGlobalFloat("_FogStrength", 0f);

            GameCamera cam = GameObject.Instantiate<GameCamera>(assetMan.Get<GameCamera>("gameCam"));

            EditorController editorController = GameObject.Instantiate<EditorController>(assetMan.Get<EditorController>("MainEditorController"));
        }

        public void GoToEditor()
        {
            StartCoroutine(LoadEditorScene());
        }

        IEnumerator FindObjectsAndSetupEditor()
        {
            yield return 4;
            yield return "Grabbing necessary resources...";
            assetMan.Add<Mesh>("Quad", Resources.FindObjectsOfTypeAll<Mesh>().First(x => x.GetInstanceID() >= 0 && x.name == "Quad"));
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>().Where(x => x.GetInstanceID() >= 0).ToArray();
            assetMan.Add<Material>("tileAlpha", materials.First(x => x.name == "TileBase_Alpha"));
            yield return "Finding GameCamera...";
            assetMan.Add<GameCamera>("gameCam", Resources.FindObjectsOfTypeAll<GameCamera>().First());
            yield return "Setting up materials...";
            Material gridMat = new Material(assetMan.Get<Material>("tileAlpha"));
            gridMat.name = "EditorGridMaterial";
            gridMat.SetMainTexture(AssetLoader.TextureFromMod(this, "Editor", "FloorGrid.png"));
            gridMat.SetTexture("_LightMap", lightmaps["white"]);
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
            standardEditorController.ReflectionSetVariable("destroyOnLoad", false);
            standardEditorController.cameraPrefab = assetMan.Get<GameCamera>("gameCam");
            standardEditorController.canvas = editorCanvas;
            standardEditorController.gridMaterial = gridMat;

            assetMan.Add<EditorController>("MainEditorController", standardEditorController);
        }

        IEnumerator LoadAssets()
        {
            yield return 3;
            yield return "Creating solid color lightmaps...";
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
