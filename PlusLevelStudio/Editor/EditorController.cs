using HarmonyLib;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using PlusLevelStudio.UI;
using MTM101BaldAPI.AssetTools;
using System.IO;
using UnityEngine.EventSystems;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using System.Linq;

namespace PlusLevelStudio.Editor
{
    public class EditorController : Singleton<EditorController>
    {
        protected static FieldInfo _deltaThisFrame = AccessTools.Field(typeof(CursorController), "deltaThisFrame");
        protected static FieldInfo _results = AccessTools.Field(typeof(CursorController), "results");

        public HotSlotScript[] hotSlots = new HotSlotScript[9];
        public Dictionary<IEditorVisualizable, GameObject> objectVisuals = new Dictionary<IEditorVisualizable, GameObject>();

        public EditorTool currentTool => _currentTool;
        protected EditorTool _currentTool;

        public EditorMode currentMode;
        public EditorLevelData levelData;
        public Canvas canvas;

        public Tile[][] tiles = new Tile[0][];
        public GameObject[] uiObjects = new GameObject[1];

        public EnvironmentController workerEc;
        public EnvironmentController ecPrefab;
        public RoomController workerRc;

        public CoreGameManager workerCgm;
        public CoreGameManager cgmPrefab;
        public GameLoader gameLoaderPrefab;
        public ElevatorScreen elevatorScreenPrefab;

        public Selector selector;
        public Selector selectorPrefab;

        public GridManager gridManagerPrefab;
        public GridManager gridManager;

        public GameCamera cameraPrefab;
        public GameCamera camera;
        public Vector3 cameraRotation = new Vector3(0f, 0f, 0f);
        public Vector2 screenSize = Vector2.one;
        public float calculatedScaleFactor = 1f;
        public Plane currentFloorPlane = new Plane(Vector3.up, 0f);
        public Ray mouseRay;
        public Vector3 mousePlanePosition = Vector3.zero;
        protected bool mousePressedLastFrame = false; // used for handling tools
        public IntVector2 mouseGridPosition => mousePlanePosition.ToCellVector();

        protected IEditorInteractable heldInteractable = null;

        public int maxUndos = 5;
        public List<MemoryStream> undoStreams = new List<MemoryStream>();

        /// <summary>
        /// Adds the current state to the undo memory.
        /// Do this BEFORE you perform your operation!
        /// </summary>
        public void AddUndo()
        {
            if (undoStreams.Count >= maxUndos) //we already have 5 undos
            {
                undoStreams.RemoveAt(0); // memory streams dont need .Dispose to be called
            }
            MemoryStream newStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(newStream, Encoding.Default, true);
            levelData.Write(writer);
            newStream.Seek(0, SeekOrigin.Begin);
            undoStreams.Add(newStream);
        }

        /// <summary>
        /// Pops the most recently added undo and loads it
        /// </summary>
        public void PopUndo()
        {
            if (undoStreams.Count == 0) return;
            MemoryStream recentUndo = undoStreams[undoStreams.Count - 1];
            undoStreams.Remove(recentUndo);
            BinaryReader reader = new BinaryReader(recentUndo);
            LoadEditorLevel(EditorLevelData.ReadFrom(reader));
            reader.Close();
        }

        public void LoadEditorLevel(EditorLevelData newData)
        {
            if (heldInteractable != null)
            {
                heldInteractable.OnReleased();
                heldInteractable = null;
            }
            SwitchToTool(null); // remove our current tool
            IEditorVisualizable[] visuals = objectVisuals.Keys.ToArray();
            foreach (var item in visuals)
            {
                RemoveVisual(item);
            }
            levelData = newData;
            RegenerateGridAndCells();
            foreach (LightPlacement item in levelData.lights)
            {
                AddVisual(item);
            }
            foreach (DoorLocation item in levelData.doors)
            {
                AddVisual(item);
            }
            RefreshCells();
            RefreshLights();
        }

        /// <summary>
        /// Adds the specified IEditorVisualizable into the editor visuals system.
        /// </summary>
        /// <param name="visualizable"></param>
        public void AddVisual(IEditorVisualizable visualizable)
        {
            GameObject visualPrefab = visualizable.GetVisualPrefab();
            GameObject visual;
            if (visualPrefab == null)
            {
                visual = new GameObject(visualizable.GetType().Name + "_Visual");
            }
            else
            {
                visual = GameObject.Instantiate<GameObject>(visualPrefab);
            }
            objectVisuals.Add(visualizable, visual);
            visualizable.InitializeVisual(visual);
        }

        static FieldInfo _lightMap = AccessTools.Field(typeof(EnvironmentController), "lightMap");
        public void RefreshLights()
        {
            workerEc.UpdateQueuedLightChanges(); // there shouldn't be any of these but just incase there somehow are, take care of them now

            // clear all lights
            LightController[,] lightMap = (LightController[,])_lightMap.GetValue(workerEc);
            for (int i = workerEc.lights.Count - 1; i >= 0; i--)
            {
                Cell lightCell = workerEc.lights[i];
                foreach (Cell cell in workerEc.lights[i].lightAffectingCells)
                {
                    lightMap[cell.position.x, cell.position.z].RemoveSource(lightCell);
                }
                lightCell.lightAffectingCells.Clear();
                workerEc.lights.Remove(lightCell);
                lightCell.room.lights.Remove(lightCell);
                lightCell.lightOn = true;
                lightCell.hasLight = false;
            }
            // re-initialize to take care of the rest
            workerEc.InitializeLighting();
            for (int i = 0; i < levelData.lights.Count; i++)
            {
                LightPlacement placement = levelData.lights[i];
                LightGroup group = levelData.lightGroups[placement.lightGroup];
                Cell cell = workerEc.CellFromPosition(placement.position);
                // todo: figure out if this is really what i should be doing?
                workerEc.GenerateLight(cell, group.color, group.strength);
                workerEc.RegenerateLight(cell);
            }
            workerEc.UpdateQueuedLightChanges();
            LevelStudioPlugin.Instance.lightmaps["none"].Apply(false, false);
        }

        public void RemoveVisual(IEditorVisualizable visualizable)
        {
            visualizable.CleanupVisual(objectVisuals[visualizable]);
            GameObject.DestroyImmediate(objectVisuals[visualizable]); // TODO: Destroy or DestroyImmediate?
            objectVisuals.Remove(visualizable);
        }

        public void DestroySelf()
        {
            Destroy(workerCgm.gameObject);
            Destroy(camera.gameObject);
            Destroy(canvas.gameObject);
            Destroy(gameObject);
        }

        public void CompileAndPlay()
        {
            BaldiLevel level = levelData.Compile();
            // write to file for testing purposes
            /*
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(Path.Combine(Application.streamingAssetsPath, "test.bpl")));
            level.Write(writer);
            writer.Close();
            BinaryReader reader = new BinaryReader(File.OpenRead(Path.Combine(Application.streamingAssetsPath, "test.bpl")));
            level = BaldiLevel.Read(reader);
            reader.Close();*/
            SceneObject sceneObj = LevelImporter.CreateSceneObject(level);
            sceneObj.manager = Resources.FindObjectsOfTypeAll<MainGameManager>().First(x => x.name == "Lvl1_MainGameManager");
            GameLoader loader = GameObject.Instantiate<GameLoader>(gameLoaderPrefab);
            ElevatorScreen screen = GameObject.Instantiate<ElevatorScreen>(elevatorScreenPrefab);
            AccessTools.Field(typeof(Singleton<CoreGameManager>), "m_Instance").SetValue(null, null); // so coregamemanager gets created properly
            loader.AssignElevatorScreen(screen);
            loader.Initialize(0);
            loader.LoadLevel(sceneObj);
            screen.Initialize();
            loader.SetSave(false);
            DestroySelf();
        }

        /// <summary>
        /// Updates all the object visuals.
        /// Only use when necessary!
        /// </summary>
        public void UpdateAllVisuals()
        {
            foreach (KeyValuePair<IEditorVisualizable, GameObject> kvp in objectVisuals)
            {
                kvp.Key.UpdateVisual(kvp.Value);
            }
        }

        /// <summary>
        /// Updates the visual for a specific object
        /// </summary>
        /// <param name="visualizable"></param>
        public void UpdateVisual(IEditorVisualizable visualizable)
        {
            visualizable.UpdateVisual(objectVisuals[visualizable]);
        }

        /// <summary>
        /// Casts the current mouse ray to the specified plane
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="doubleSided"></param>
        /// <returns>The position the ray landed on.</returns>
        protected Vector3? CastRayToPlane(Plane plane, bool doubleSided)
        {
            float enteredAt;
            if (plane.Raycast(mouseRay, out enteredAt))
            {
                return plane.ClosestPointOnPlane(mouseRay.origin + (mouseRay.direction * enteredAt)); // todo: evaluate. im doing this because due to imprecision its not perfect otherwise
            }
            if (doubleSided)
            {
                plane.Flip();
                if (plane.Raycast(mouseRay, out enteredAt))
                {
                    plane.Flip();
                    return plane.ClosestPointOnPlane(mouseRay.origin + (mouseRay.direction * enteredAt));
                }
            }
            return null;
        }

        // movement stuff
        Vector2 analogMove = Vector2.zero;
        private AnalogInputData movementData = new AnalogInputData()
        {
            steamAnalogId = "Movement",
            xAnalogId = "MovementX",
            yAnalogId = "MovementY",
            steamDeltaId = "",
            xDeltaId = "",
            yDeltaId = ""
        };

        public void UpdateUI()
        {
            if ((float)Singleton<PlayerFileManager>.Instance.resolutionX / (float)Singleton<PlayerFileManager>.Instance.resolutionY >= 1.3333f)
            {
                calculatedScaleFactor = (float)Mathf.RoundToInt((float)Singleton<PlayerFileManager>.Instance.resolutionY / 360f);
            }
            else
            {
                calculatedScaleFactor = (float)Mathf.FloorToInt((float)Singleton<PlayerFileManager>.Instance.resolutionY / 480f);
            }
            canvas.scaleFactor = calculatedScaleFactor;
            canvas.worldCamera = camera.canvasCam;
            screenSize = new Vector2(Screen.width / calculatedScaleFactor, Screen.height / calculatedScaleFactor);
            for (int i = 0; i < uiObjects.Length; i++)
            {
                if (uiObjects[i] != null)
                {
                    GameObject.Destroy(uiObjects[i]);
                }
            }
            CursorInitiator init = canvas.GetComponent<CursorInitiator>();
            init.screenSize = screenSize;
            init.Inititate();
            uiObjects[0] = UIBuilder.BuildUIFromFile<EditorUIMainHandler>(canvas, "Main", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "Main.json")).gameObject;
            uiObjects[0].transform.SetAsFirstSibling();
        }

        // called when the editor mode is assigned or re-assigned
        public void EditorModeAssigned()
        {
            for (int i = 0; i < hotSlots.Length; i++)
            {
                if (i >= currentMode.availableTools.Count) continue;
                hotSlots[i].currentTool = currentMode.availableTools[i];
            }
        }

        protected void UpdateMouseRay()
        {
            Vector3 pos = new Vector3((CursorController.Instance.LocalPosition.x / screenSize.x) * Screen.width, Screen.height + ((CursorController.Instance.LocalPosition.y / screenSize.y) * Screen.height));
            mouseRay = camera.camCom.ScreenPointToRay(pos);
            Vector3? pPos = CastRayToPlane(currentFloorPlane, false);
            if (pPos.HasValue)
            {
                mousePlanePosition = pPos.Value;
            }
        }

        public void TriggerError(string errorString)
        {
            Debug.LogWarning("Encountered error: " + errorString + "!");
        }

        /// <summary>
        /// Switches the current tool to the one passed in.
        /// </summary>
        /// <param name="tool"></param>
        public void SwitchToTool(EditorTool tool)
        {
            selector.DisableSelection(); // deselect whatever we had selected
            UnhighlightAllCells();
            if (_currentTool != null)
            {
                _currentTool.Exit();
            }
            _currentTool = tool;
            mousePressedLastFrame = false;
            if (_currentTool != null)
            {
                _currentTool.Begin();
            }
        }

        /// <summary>
        /// Resizes the grid to the respective size and shifts everything by the respective position.
        /// The camera is moved so no visible change is noticed
        /// </summary>
        /// <param name="posDif"></param>
        /// <param name="sizeDif"></param>
        public void ResizeGrid(IntVector2 posDif, IntVector2 sizeDif)
        {
            if (sizeDif.x == 0 && sizeDif.z == 0) return; // no point in doing anything
            IntVector2 targetSize = levelData.mapSize + sizeDif;
            if (targetSize.x > 255 || targetSize.z > 255) { TriggerError("LevelTooBig"); return; }
            if (targetSize.x < 1 || targetSize.z < 1) { TriggerError("LevelTooSmall"); return; }
            // TODO: INSERT LOGIC FOR OBJECTS

            if (!levelData.ResizeLevel(posDif, sizeDif, this))
            {
                TriggerError("RoomClipped");
                return;
            }

            RegenerateGridAndCells();
            // move camera so the change isn't noticable
            transform.position -= new Vector3(posDif.x * 10f, 0f, posDif.z * 10f);
        }

        void Update()
        {
            canvas.scaleFactor = calculatedScaleFactor;
            UpdateMouseRay();
            //PlaySongIfNecessary();
            UpdateCamera();
            /*
            if (Singleton<InputManager>.Instance.GetDigitalInput("Item1", true))
            {
                selector.SelectArea(new RectInt(new Vector2Int(0,0), new Vector2Int(3,7)), (sizeDif, posDif) =>
                {
                    selector.DisableSelection();
                });
                return;
            }*/
            if (((List<RaycastResult>)_results.GetValue(CursorController.Instance)).Count == 0)
            {
                HandleClicking();
            }
#if DEBUG
            if (Singleton<InputManager>.Instance.GetDigitalInput("Item1", true))
            {
                UpdateUI();
                EditorModeAssigned();
            }
#endif
        }

        protected bool HandleInteractableClicking()
        {
            if (Physics.Raycast(mouseRay, out RaycastHit info, 1000f, LevelStudioPlugin.editorInteractableLayerMask))
            {
                if (info.transform.TryGetComponent(out IEditorInteractable interactable))
                {
                    if ((currentTool == null) || interactable.InteractableByTool(currentTool))
                    {
                        if (interactable.OnClicked())
                        {
                            heldInteractable = interactable;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected void HandleClicking()
        {
            if (currentTool != null)
            {
                bool mousePressedThisFrame = Singleton<InputManager>.Instance.GetDigitalInput("Interact", false);
                if (mousePressedLastFrame != mousePressedThisFrame)
                {
                    if (mousePressedThisFrame)
                    {
                        if (HandleInteractableClicking()) return;
                        if (currentTool == null) return;
                        if (currentTool.MousePressed()) { SwitchToTool(null); return; }
                    }
                    else
                    {
                        if (currentTool.MouseReleased()) { SwitchToTool(null); return; }
                    }
                }
                mousePressedLastFrame = mousePressedThisFrame;
                currentTool.Update();
                if (Singleton<InputManager>.Instance.GetDigitalInput("Pause", true))
                {
                    if (currentTool.Cancelled())
                    {
                        SwitchToTool(null);
                    }
                }
                return;
            }
            if (heldInteractable != null)
            {
                if (Singleton<InputManager>.Instance.GetDigitalInput("Interact", false))
                {
                    // cancel the hold if our current interactable requests it
                    if (!heldInteractable.OnHeld())
                    {
                        heldInteractable.OnReleased();
                        heldInteractable = null;
                    }
                }
                else
                {
                    heldInteractable.OnReleased();
                    heldInteractable = null;
                }
                return;
            }
            if (Singleton<InputManager>.Instance.GetDigitalInput("Interact", true))
            {
                if (HandleInteractableClicking()) return;
                if (mouseGridPosition.x >= 0 && mouseGridPosition.z >= 0 && mouseGridPosition.x < levelData.mapSize.x && mouseGridPosition.z < levelData.mapSize.z)
                {
                    SelectTile(mouseGridPosition);
                }
            }
        }

        /// <summary>
        /// Highlights all the cells at the corresponding positions.
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="highlight">The lightmap/highlight to use.</param>
        public void HighlightCells(IntVector2[] positions, string highlight)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                workerEc.cells[positions[i].x, positions[i].z].Tile.MeshRenderer.material.SetTexture("_LightMap", LevelStudioPlugin.Instance.lightmaps[highlight]);
            }
        }

        /// <summary>
        /// Removes highlights from all cells.
        /// </summary>
        public void UnhighlightAllCells()
        {
            for (int x = 0; x < workerEc.cells.GetLength(0); x++)
            {
                for (int y = 0; y < workerEc.cells.GetLength(1); y++)
                {
                    workerEc.cells[x,y].Tile.MeshRenderer.material.SetTexture("_LightMap", LevelStudioPlugin.Instance.lightmaps["none"]);
                }
            }
        }

        protected void AreaResize(CellArea area, IntVector2 sizeDif, IntVector2 posDif)
        {
            HighlightCells(area.CalculateOwnedCells(), "none");
            if (sizeDif.x != 0 || sizeDif.z != 0)
            {
                area.ResizeWithSafety(sizeDif, posDif);
                RefreshCells();
            }
            HighlightCells(area.CalculateOwnedCells(), "yellow");
            selector.SelectArea(area.rect.Value, (IntVector2 sd, IntVector2 pd) => AreaResize(area, sd, pd));
        }

        protected virtual void SelectTile(IntVector2 tileSelected)
        {
            UnhighlightAllCells(); // TODO: investigate performance?
            CellArea area = levelData.AreaFromPos(tileSelected, true);
            if (area != null)
            {
                if (area.rect.HasValue)
                {
                    AreaResize(area, new IntVector2(), new IntVector2());
                }
                return;
            }
            selector.SelectTile(tileSelected);
        }

        protected virtual void PlaySongIfNecessary()
        {
            if (!Singleton<MusicManager>.Instance.MidiPlaying)
            {
                Singleton<MusicManager>.Instance.PlayMidi(LevelStudioPlugin.Instance.editorTracks[UnityEngine.Random.Range(0, LevelStudioPlugin.Instance.editorTracks.Count)], false);
            }
        }

        protected virtual void UpdateCamera()
        {
            if (Singleton<InputManager>.Instance.GetDigitalInput("UseItem", false))
            {
                Vector2 analog = CursorController.Movement;
                cameraRotation += (new Vector3(-analog.y, analog.x, 0f));
                cameraRotation.x = Mathf.Clamp(cameraRotation.x, -89f, 89f);
                transform.eulerAngles = cameraRotation;
            }
            Singleton<InputManager>.Instance.GetAnalogInput(movementData, out analogMove, out _);
            float moveSpeed = Singleton<InputManager>.Instance.GetDigitalInput("Run", false) ? 125f : 50f;
            transform.position += transform.forward * analogMove.y * Time.deltaTime * moveSpeed;
            transform.position += transform.right * analogMove.x * Time.deltaTime * moveSpeed;
        }

        public Texture2D GenerateTextureAtlas(Texture2D floor, Texture2D wall, Texture2D ceiling)
        {
            workerRc.florTex = floor;
            workerRc.wallTex = wall;
            workerRc.ceilTex = ceiling;
            workerRc.GenerateTextureAtlas();
            return workerRc.textureAtlas;
        }


        static FieldInfo _initialized = AccessTools.Field(typeof(Cell), "initalized"); // seriously mystman? "initalized"?
        public void RefreshCells()
        {
            levelData.UpdateCells(true);
            for (int x = 0; x < workerEc.cells.GetLength(0); x++)
            {
                for (int y = 0; y < workerEc.cells.GetLength(1); y++)
                {
                    if (levelData.cells[x, y].type == 16)
                    {
                        workerEc.cells[x, y].Tile.gameObject.SetActive(false);
                        if (!workerEc.cells[x, y].Null)
                        {
                            _initialized.SetValue(workerEc.cells[x, y], false);
                        }
                        continue;
                    }
                    EditorRoom room = levelData.RoomFromPos(new IntVector2(x,y), true);
                    // room shouldn't be null here, because if we've reached this point roomId wasn't zero
                    workerEc.cells[x, y].Tile.gameObject.SetActive(true);
                    workerEc.cells[x, y].Tile.MeshRenderer.material.SetMainTexture(GenerateTextureAtlas(room.floorTex, room.wallTex, room.ceilTex));
                    workerEc.cells[x, y].SetShape(levelData.cells[x, y].type, TileShapeMask.None);
                    if (workerEc.cells[x, y].Null)
                    {
                        workerEc.cells[x, y].Initialize();
                    }
                }
            }
            UnhighlightAllCells();
            RefreshLights();
        }

        protected void RegenerateGridAndCells()
        {
            gridManager.RegenerateGrid();
            if (workerEc.cells != null)
            {
                for (int x = 0; x < workerEc.cells.GetLength(0); x++)
                {
                    for (int y = 0; y < workerEc.cells.GetLength(1); y++)
                    {
                        GameObject.Destroy(workerEc.cells[x, y].Tile.gameObject); // bye bye tile
                    }
                }
            }
            workerEc.SetTileInstantiation(true);
            workerEc.levelSize = levelData.mapSize;
            workerEc.InitializeCells(levelData.mapSize);
            for (int x = 0; x < workerEc.cells.GetLength(0); x++)
            {
                for (int y = 0; y < workerEc.cells.GetLength(1); y++)
                {
                    workerEc.cells[x, y].LoadTile();
                    workerEc.cells[x, y].Tile.transform.SetParent(gridManager.transform, true);
                    if (levelData.cells[x,y].type == 16)
                    {
                        workerEc.cells[x, y].Tile.gameObject.SetActive(false);
                    }
                }
            }
            RefreshCells(); // TODO: check performance, potential clean up?
            UpdateAllVisuals();
        }

        protected override void AwakeFunction()
        {
            levelData = new EditorLevelData(new IntVector2(50,50));
            gridManager = GameObject.Instantiate(gridManagerPrefab);
            gridManager.editor = this;
            camera = GameObject.Instantiate(cameraPrefab);
            camera.UpdateTargets(transform,0);
            canvas.transform.SetParent(null);
            UpdateUI();
            selector = GameObject.Instantiate(selectorPrefab);
            workerEc = GameObject.Instantiate(ecPrefab);
            workerEc.gameObject.SetActive(false);
            workerEc.name = "WorkerEnvironmentController";
            workerEc.lightMode = LightMode.Cumulative;
            workerRc = workerEc.transform.Find("NullRoom").GetComponent<RoomController>();
            workerRc.ec = workerEc;
            workerRc.ReflectionInvoke("Awake", null);

            workerCgm = GameObject.Instantiate(cgmPrefab);
            workerCgm.gameObject.SetActive(true);
            workerCgm.gameObject.SetActive(false);
            RegenerateGridAndCells();
        }
    }
}
