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
using MidiPlayerTK;
using PlusLevelStudio.Editor.SettingsUI;
using static MidiPlayerTK.SFFile;

namespace PlusLevelStudio.Editor
{
    public class EditorController : Singleton<EditorController>
    {
        protected static FieldInfo _deltaThisFrame = AccessTools.Field(typeof(CursorController), "deltaThisFrame");
        protected static FieldInfo _results = AccessTools.Field(typeof(CursorController), "results");

        public HotSlotScript[] hotSlots = new HotSlotScript[9];
        public Dictionary<IEditorVisualizable, GameObject> objectVisuals = new Dictionary<IEditorVisualizable, GameObject>();

        public EditorTool currentTool => _currentTool;
        public BaseGameManager baseGameManagerPrefab;
        protected EditorTool _currentTool;

        public EditorMode currentMode;
        public EditorFileContainer currentFile;
        public EditorLevelData levelData
        {
            get
            {
                return currentFile.data;
            }
            set
            {
                currentFile.data = value;
            }
        }
        public Canvas canvas;

        public Tile[][] tiles = new Tile[0][];
        public GameObject[] uiObjects = new GameObject[3];
        // todo: maybe make this a tuple list with (GameObject, bool) where the bool represents if it's a blocking UI element?
        public List<GameObject> uiOverlays = new List<GameObject>();

        public EnvironmentController workerEc;
        public EnvironmentController ecPrefab;
        public RoomController workerRc;

        public CoreGameManager workerCgm;
        public CoreGameManager cgmPrefab;

        public Selector selector;
        public Selector selectorPrefab;

        public GridManager gridManagerPrefab;
        public GridManager gridManager;
        public TooltipController tooltipController;
        public RectTransform tooltipBase;

        public SpawnpointMoverAndVisualizerScript spawnpointVisualPrefab;
        public SpawnpointMoverAndVisualizerScript spawnpointVisual;

        protected Dictionary<PosterObject, Texture2D> generatedTextures = new Dictionary<PosterObject, Texture2D>();

        public bool toolboxOnNullTool;
        public GameCamera cameraPrefab;
        public GameCamera camera;
        public Vector3 cameraRotation = new Vector3(0f, 0f, 0f);
        public Vector2 screenSize = Vector2.one;
        public float calculatedScaleFactor = 1f;
        public Plane currentFloorPlane = new Plane(Vector3.up, 0f);
        public Ray mouseRay;
        public Vector3 mousePlanePosition = Vector3.zero;
        protected bool mousePressedLastFrame = false; // used for handling tools
        public TextTextureGenerator textTextureGenerator;
        public IntVector2 mouseGridPosition => mousePlanePosition.ToCellVector();

        // TODO: consider moving to selector?
        public float gridSnap = 0.25f;
        public float angleSnap = 11.25f;

        public string currentFileName = null;
        public bool hasUnsavedChanges = false;
        public static string lastPlayedLevel = null;

        public bool MovementEnabled
        {
            get
            {
                return (uiOverlays.Count == 0 && (uiObjects[1] == null || !uiObjects[1].activeSelf) && (CursorController.Instance != null && CursorController.Instance.cursorTransform.gameObject.activeSelf));
            }
        }

        public List<EditorRoomVisualManager> roomVisuals = new List<EditorRoomVisualManager>();

        public void SetupVisualsForRoom(EditorRoom room)
        {
            if (!LevelStudioPlugin.Instance.roomVisuals.ContainsKey(room.roomType)) return;
            room.myVisual = GameObject.Instantiate<EditorRoomVisualManager>(LevelStudioPlugin.Instance.roomVisuals[room.roomType]);
            roomVisuals.Add(room.myVisual);
            room.myVisual.myLevelData = levelData;
            room.myVisual.myRoom = room;
            room.myVisual.Initialize();
        }

        public void SetupVisualsForAllRooms()
        {
            while (roomVisuals.Count > 0)
            {
                CleanupVisualsForRoom(roomVisuals[0].myRoom);
            }
            foreach (EditorRoom room in levelData.rooms)
            {
                SetupVisualsForRoom(room);
            }
        }

        public void UpdateVisualsForRoom(EditorRoom room)
        {
            if (room.myVisual == null) return;
            room.myVisual.RoomUpdated();
        }

        public void CleanupVisualsForRoom(EditorRoom room)
        {
            if (room.myVisual == null) return;
            room.myVisual.Cleanup();
            roomVisuals.Remove(room.myVisual);
            GameObject.Destroy(room.myVisual.gameObject);
            room.myVisual = null;
        }

        protected IEditorInteractable heldInteractable = null;

        public int maxUndos = 15;
        public List<MemoryStream> undoStreams = new List<MemoryStream>();
        public MemoryStream currentlyHeldUndo = null;

        /// <summary>
        /// Adds the current state to the undo memory.
        /// Do this BEFORE you perform your operation!
        /// </summary>
        public void AddUndo()
        {
            if (currentlyHeldUndo != null)
            {
                Debug.LogWarning("Adding Undo while an Undo is being held! Discarding held undo...");
            }
            HoldUndo();
            AddHeldUndo();
        }

        /// <summary>
        /// Creates an undo, but doesn't add it right away.
        /// </summary>
        public void HoldUndo()
        {
            MemoryStream newStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(newStream, Encoding.Default, true);
            levelData.Write(writer);
            newStream.Seek(0, SeekOrigin.Begin);
            currentlyHeldUndo = newStream;
        }

        /// <summary>
        /// Cancels the currently held undo.
        /// </summary>
        public void CancelHeldUndo()
        {
            currentlyHeldUndo = null;
        }

        /// <summary>
        /// Adds the currently held undo.
        /// </summary>
        public void AddHeldUndo()
        {
            hasUnsavedChanges = true;
            if (undoStreams.Count >= maxUndos) //we already have 5 undos
            {
                undoStreams.RemoveAt(0); // memory streams dont need .Dispose to be called
            }
            undoStreams.Add(currentlyHeldUndo);
            currentlyHeldUndo = null;
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
            LoadEditorLevel(EditorLevelData.ReadFrom(reader), false);
            reader.Close();
        }

        static FieldInfo _TextTextureGenerator = AccessTools.Field(typeof(EnvironmentController), "TextTextureGenerator");
        public Texture2D GetOrGeneratePoster(PosterObject p)
        {
            if ((p.textData == null) || (p.textData.Length == 0))
            {
                return p.baseTexture;
            }
            if (generatedTextures.ContainsKey(p))
            {
                return generatedTextures[p];
            }
            Texture2D tex = ((TextTextureGenerator)_TextTextureGenerator.GetValue(workerEc)).GenerateTextTexture(p);
            generatedTextures.Add(p, tex);
            return tex;
        }

        public void LoadEditorLevelAndMeta(EditorFileContainer file)
        {
            currentFile = file;
            LoadEditorLevel(file.data);
            if (file.meta == null)
            {
                file.meta = new EditorFileMeta();
                UpdateMeta();
            }
            LoadToolbar(file.meta.toolbarTools);
            transform.position = file.meta.cameraPosition;
            transform.rotation = file.meta.cameraRotation;
            cameraRotation = file.meta.cameraRotation.eulerAngles;
        }

        public bool LoadEditorLevelFromFile(string path)
        {
            BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read));
            EditorFileContainer file = EditorFileContainer.ReadMindful(reader);
            if (file.meta == null)
            {
                file.meta = new EditorFileMeta();
                file.meta.editorMode = "full"; // if the file doesn't have EditorFileMeta it must be OLD.
            }
            if (file.meta.editorMode == currentMode.id)
            {
                LoadEditorLevelAndMeta(file);
            }
            else
            {
                TriggerError("File of mismatched editor mode!");
                reader.Close();
                return false;
            }
            //EditorController.Instance.LoadEditorLevel(EditorLevelData.ReadFrom(reader), true);
            reader.Close();
            currentFileName = Path.GetFileNameWithoutExtension(path);
            return true;
        }

        public void SaveEditorLevelToFile(string path)
        {
            UpdateMeta();
            Directory.CreateDirectory(LevelStudioPlugin.levelFilePath);
            BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write));
            currentFile.Write(writer);
            writer.Close();
            hasUnsavedChanges = false;
            currentFileName = Path.GetFileNameWithoutExtension(path);
        }

        public void LoadEditorLevel(EditorLevelData newData, bool wipeUndoHistory = true)
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
            foreach (WindowLocation item in levelData.windows)
            {
                AddVisual(item);
            }
            foreach (ExitLocation item in levelData.exits)
            {
                AddVisual(item);
            }
            foreach (ItemPlacement item in levelData.items)
            {
                AddVisual(item);
            }
            foreach (ItemSpawnPlacement item in levelData.itemSpawns)
            {
                AddVisual(item);
            }
            foreach (BasicObjectLocation item in levelData.objects)
            {
                AddVisual(item);
            }
            foreach (EditorRoom room in levelData.rooms)
            {
                if (room.activity == null) continue;
                AddVisual(room.activity);
            }
            foreach (StructureLocation item in levelData.structures)
            {
                AddVisual(item);
            }
            foreach (NPCPlacement item in levelData.npcs)
            {
                AddVisual(item);
            }
            foreach (PosterPlacement item in levelData.posters)
            {
                AddVisual(item);
            }
            foreach (WallLocation item in levelData.walls)
            {
                AddVisual(item);
            }
            RefreshCells(false);
            SetupVisualsForAllRooms(); // need to do this first before lighting
            RefreshLights();
            UpdateSpawnVisual();
            UpdateSkybox();
            CancelHeldUndo();
            if (wipeUndoHistory)
            {
                hasUnsavedChanges = false;
                undoStreams.Clear(); // memorystreams dont need .dispose
            }
        }

        public StructureLocation GetStructureData(string type)
        {
            StructureLocation foundStructure = levelData.structures.FirstOrDefault(x => x.type == type);
            return foundStructure;
        }

        public StructureLocation AddOrGetStructureToData(string type, bool onlyOne)
        {
            if (onlyOne)
            {
                StructureLocation foundStructure = levelData.structures.FirstOrDefault(x => x.type == type);
                if (foundStructure != null) return foundStructure;
            }
            StructureLocation structure = LevelStudioPlugin.Instance.ConstructStructureOfType(type);
            levelData.structures.Add(structure);
            AddVisual(structure);
            return structure;
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
            workerEc.standardDarkLevel = levelData.minLightColor;
            workerEc.lightMode = levelData.lightMode;
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
                // todo: evaluate performance impact, but honestly due to the convience regenerating each time provides, and the performance impact only grows bad with absurd level sizes... (and it could be due to other reasons)
                workerEc.GenerateLight(cell, group.color, group.strength);
                workerEc.RegenerateLight(cell);
            }
            // now handle anything that might change light
            HandleLightChanges(levelData.exits);
            HandleLightChanges(levelData.structures);
            roomVisuals.ForEach(x => x.ModifyLightsForEditor(workerEc));
            UpdateStructuresWithReason(PotentialStructureUpdateReason.LightChange);
            workerEc.UpdateQueuedLightChanges();
            LevelStudioPlugin.Instance.lightmaps["none"].Apply(false, false);
        }

        protected void HandleLightChanges(IEnumerable<IEditorCellModifier> todo)
        {
            foreach (var item in todo)
            {
                item.ModifyLightsForEditor(workerEc);
            }
        }

        public void RemoveVisual(IEditorVisualizable visualizable)
        {
            if (!objectVisuals.ContainsKey(visualizable)) return;
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

        public void UpdateGridHeight()
        {
            currentFloorPlane = new Plane(Vector3.up, -gridManager.Height);
        }

        public bool[,] CompileSafeCells(EditorLevelData data, float capsuleRadius)
        {
            bool[,] returnValue = new bool[data.mapSize.x, data.mapSize.z];
            Collider[] foundColliders;
            for (int x = 0; x < data.mapSize.x; x++)
            {
                for (int y = 0; y < data.mapSize.z; y++)
                {
                    if (data.cells[x, y].roomId == 0)
                    {
                        returnValue[x, y] = false;
                        continue; // this is an empty cell, do not bother
                    }
                    Vector3 cellWorldPos = new IntVector2(x, y).ToWorld();
                    foundColliders = Physics.OverlapCapsule(cellWorldPos - (Vector3.down * 4f), cellWorldPos - (Vector3.up * 4f), capsuleRadius);
                    returnValue[x, y] = true;
                    //Debug.Log("processing: " + x + "," + y);
                    for (int i = 0; i < foundColliders.Length; i++)
                    {
                        if (foundColliders[i] == null) continue;
                        if (foundColliders[i].isTrigger) continue;
                        if (!returnValue[x, y]) continue;
                        //Debug.Log(foundColliders[i].name);
                        returnValue[x, y] = false;
                        
                    }
                }
            }
            return returnValue;
        }

        public PlusCellCoverage[,] CompileBlockedCells(EditorLevelData data, float capsuleRadius, float approachWallCheck)
        {
            PlusCellCoverage[,] returnValue = new PlusCellCoverage[data.mapSize.x, data.mapSize.z];
            Collider[] foundColliders;
            for (int x = 0; x < data.mapSize.x; x++)
            {
                for (int y = 0; y < data.mapSize.z; y++)
                {
                    returnValue[x, y] = PlusCellCoverage.None;
                    if (data.cells[x, y].roomId == 0)
                    {
                        continue; // this is an empty cell, do not bother
                    }
                    PlusCellCoverage coverage = PlusCellCoverage.None;
                    List<Direction> allDirections = Directions.ClosedDirectionsFromBin(data.cells[x, y].type); // so we dont wrongly block walls with props on the other side.
                    while (allDirections.Count > 0)
                    {
                        Direction chosenDirection = allDirections[0];
                        allDirections.RemoveAt(0);
                        Vector3 cellWorldPos = new IntVector2(x, y).ToWorld() + (chosenDirection.ToVector3() * approachWallCheck);
                        foundColliders = Physics.OverlapCapsule(cellWorldPos - (Vector3.down * 4f), cellWorldPos - (Vector3.up * 4f), capsuleRadius);
                        for (int i = 0; i < foundColliders.Length; i++)
                        {
                            if (foundColliders[i] == null) continue;
                            if (foundColliders[i].isTrigger) continue;
                            coverage |= (PlusCellCoverage)chosenDirection.ToCoverage();
                        }

                        // check extra directions so we dont miss corners
                        Direction[] directionsToCheck = new Direction[] { chosenDirection.RotatedRelativeToNorth(Direction.East), chosenDirection.RotatedRelativeToNorth(Direction.West) };
                        for (int j = 0; j < directionsToCheck.Length; j++)
                        {
                            Vector3 secondCheckWorldPos = new IntVector2(x, y).ToWorld() + (chosenDirection.ToVector3() * approachWallCheck) + (directionsToCheck[j].ToVector3() * approachWallCheck);
                            foundColliders = Physics.OverlapCapsule(secondCheckWorldPos - (Vector3.down * 4f), secondCheckWorldPos - (Vector3.up * 4f), capsuleRadius);
                            for (int i = 0; i < foundColliders.Length; i++)
                            {
                                if (foundColliders[i] == null) continue;
                                if (foundColliders[i].isTrigger) continue;
                                coverage |= (PlusCellCoverage)chosenDirection.ToCoverage();
                            }
                        }
                    }
                    returnValue[x, y] = coverage;
                }
            }
            return returnValue;
        }

        public void UpdateStructuresWithReason(PotentialStructureUpdateReason reason)
        {
            foreach (StructureLocation structure in levelData.structures)
            {
                if (GetVisual(structure) == null) continue; // don't visual
                if (structure.ShouldUpdateVisual(reason))
                {
                    UpdateVisual(structure);
                }
            }
        }

        public Texture2D GenerateThumbnail()
        {
            selector.DisableSelection();
            UnhighlightAllCells();
            Vector3 oldPos = transform.position;
            Quaternion oldRot = transform.rotation;
            Camera renderCam = transform.gameObject.AddComponent<Camera>();
            RenderTexture tempRenderTex = RenderTexture.GetTemporary(64, 64, 0, RenderTextureFormat.ARGB32);
            renderCam.targetTexture = tempRenderTex; // use 4 bit color to add some dithering and if unity was SMART reduce file size.
            renderCam.targetTexture.filterMode = FilterMode.Point;
            renderCam.useOcclusionCulling = false;
            renderCam.clearFlags = CameraClearFlags.Skybox;
            renderCam.farClipPlane = 1000f;
            renderCam.cullingMask = LayerMask.GetMask("Default", "TransparentFX", "IgnoreRaycast", "Water", "PostProcessing", "CollidableEntities", "Ignore Raycast B", "Block Raycast", "Disabled", "Windows", "Maskable", "Mask", "Billboard");
            transform.position = levelData.PracticalSpawnPoint;
            transform.rotation = levelData.PracticalSpawnDirection.ToRotation();
            RenderTexture.active = tempRenderTex;
            renderCam.Render();
            Texture2D thumb = new Texture2D(renderCam.targetTexture.width, renderCam.targetTexture.height, TextureFormat.ARGB4444, false);
            thumb.name = "Thumbnail";
            thumb.filterMode = FilterMode.Point;
            thumb.ReadPixels(new Rect(0f,0f,64f,64f),0,0);
            thumb.Apply();

            RenderTexture.active = null;

            transform.position = oldPos;
            transform.rotation = oldRot;

            DestroyImmediate(renderCam);
            RenderTexture.ReleaseTemporary(tempRenderTex);
            return thumb;
        }

        public void ExportWithChecks()
        {
            if ((currentFileName == null) || currentFileName == string.Empty)
            {
                CreateUIPopup(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_NeedSaveToExport"), () =>
                {
                    uiObjects[0].GetComponent<EditorUIMainHandler>().SendInteractionMessage("saveAndExport", null);
                }, () => { });
                return;
            }
            if (hasUnsavedChanges)
            {
                CreateUIPopup(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_UnsavedChangesExport"), () =>
                {
                    uiObjects[0].GetComponent<EditorUIMainHandler>().SendInteractionMessage("saveAndExport", null);
                }, () => { Export(); });
                return;
            }
            Export();
        }

        public virtual void Export()
        {
            BaldiLevel level = Compile();
            PlayableEditorLevel playableLevel = new PlayableEditorLevel();
            playableLevel.data = level;
            playableLevel.meta = levelData.meta;
            playableLevel.texture = GenerateThumbnail();
            Directory.CreateDirectory(LevelStudioPlugin.levelExportPath);

            BinaryWriter writer = new BinaryWriter(new FileStream(Path.Combine(LevelStudioPlugin.levelExportPath, currentFileName + ".pbpl"), FileMode.Create, FileAccess.Write));
            playableLevel.Write(writer);
            writer.Close();
            Destroy(playableLevel.texture); // we've now written it to file, discard it to free up memory.
            Application.OpenURL("file://" + LevelStudioPlugin.levelExportPath);
        }

        public BaldiLevel Compile()
        {
            selector.DisableSelection();
            BaldiLevel level = levelData.Compile();
            for (int i = 0; i < levelData.objects.Count; i++)
            {
                EditorController.Instance.GetVisual(levelData.objects[i]).GetComponent<EditorBasicObject>().SetMode(false); // revert to regular hitboxes
            }
            level.entitySafeCells = CompileSafeCells(levelData, 1f);
            level.eventSafeCells = CompileSafeCells(levelData, 2f);
            level.coverage = CompileBlockedCells(levelData, 1.5f, 3.4f);
            for (int i = 0; i < levelData.objects.Count; i++)
            {
                EditorController.Instance.GetVisual(levelData.objects[i]).GetComponent<EditorBasicObject>().SetMode(true); // revert to editor hitboxes
            }
            return level;
        }

        public void CompileAndPlay()
        {
            BaldiLevel level = Compile();
            AccessTools.Field(typeof(Singleton<CoreGameManager>), "m_Instance").SetValue(null, null); // so coregamemanager gets created properly
            PlayableEditorLevel playableLevel = new PlayableEditorLevel();
            playableLevel.data = level;
            playableLevel.meta = levelData.meta;
            EditorPlayModeManager.LoadLevel(playableLevel, 0, true, lastPlayedLevel, currentMode.id);
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
            roomVisuals.ForEach(x => x.RoomUpdated());
        }

        /// <summary>
        /// Updates the visual for a specific object
        /// </summary>
        /// <param name="visualizable"></param>
        public void UpdateVisual(IEditorVisualizable visualizable)
        {
            if (!objectVisuals.ContainsKey(visualizable))
            {
                throw new Exception("Attempted to non-existant visual: " + visualizable.ToString() + "!");
            }
            visualizable.UpdateVisual(objectVisuals[visualizable]);
        }

        /// <summary>
        /// Gets the visual created for the specified IEditorVisualizable.
        /// </summary>
        /// <param name="visualizable"></param>
        public GameObject GetVisual(IEditorVisualizable visualizable)
        {
            if (!objectVisuals.ContainsKey(visualizable)) return null;
            return objectVisuals[visualizable];
        }

        /// <summary>
        /// Casts the current mouse ray to the specified plane
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="doubleSided"></param>
        /// <returns>The position the ray landed on.</returns>
        public Vector3? CastMouseRayToPlane(Plane plane, bool doubleSided)
        {
            float enteredAt;
            if (plane.Raycast(mouseRay, out enteredAt))
            {
                return plane.ClosestPointOnPlane(mouseRay.origin + (mouseRay.direction * enteredAt)); // im doing this because due to imprecision its not perfect otherwise
            }
            if (doubleSided)
            {
                plane.Flip();
                if (plane.Raycast(mouseRay, out enteredAt))
                {
                    plane.Flip();
                    return plane.ClosestPointOnPlane(mouseRay.origin + (mouseRay.direction * enteredAt));
                }
                plane.Flip();
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

        public virtual void PlayLevel()
        {
            if ((currentFileName != null) && (File.Exists(Path.Combine(LevelStudioPlugin.levelFilePath, currentFileName + ".ebpl"))))
            {
                lastPlayedLevel = currentFileName;
            }
            else
            {
                lastPlayedLevel = null;
            }
            CompileAndPlay();
        }

        public T CreateUI<T>(string name, string path) where T : UIExchangeHandler
        {
            T obj = UIBuilder.BuildUIFromFile<T>(canvas.GetComponent<RectTransform>(), name, path);
            /*obj.transform.SetAsFirstSibling();
            for (int i = 0; i < uiObjects.Length; i++)
            {
                uiObjects[i].transform.SetAsFirstSibling();
            }*/
            CursorController.Instance.transform.SetAsLastSibling();
            tooltipBase.transform.SetAsLastSibling();
            uiOverlays.Add(obj.gameObject);
            return obj;
        }

        internal T CreateUI<T>(string name) where T : UIExchangeHandler
        {
            return CreateUI<T>(name, Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", name + ".json"));
        }

        public void CreateUIPopup(string text, Action onYes, Action onNo)
        {
            EditorPopupExchangeHandler handler = CreateUI<EditorPopupExchangeHandler>("2ChoicePopUp");
            handler.transform.Find("Title").GetComponent<TextMeshProUGUI>().text = text;
            handler.OnYes = onYes;
            handler.OnNo = onNo;
        }

        public void CreateUIFileBrowser(string path, string startingFile, string extension, Func<string, bool> onSubmit)
        {
            EditorUIFileBrowser fileBrowser = EditorController.Instance.CreateUI<EditorUIFileBrowser>("FileBrowser");
            fileBrowser.Setup(path, extension, startingFile, onSubmit);

        }

        public void RemoveUI(GameObject obj)
        {
            uiOverlays.Remove(obj);
            Destroy(obj);
        }

        protected static FieldInfo _xMin = AccessTools.Field(typeof(TooltipController), "xMin");
        protected static FieldInfo _xMax = AccessTools.Field(typeof(TooltipController), "xMax");

        public void UpdateUI()
        {
            if ((float)Singleton<PlayerFileManager>.Instance.resolutionX / (float)Singleton<PlayerFileManager>.Instance.resolutionY >= 1.3333f)
            {
                // currently replacing RoundToInt with FloorToInt as a hacky solution to avoid elements getting cut off at certain resolutions.
                // TODO: find a more elegant/proper way of doing this, as a variety of resolutions that were rendering fine before are now rendering oddly!
                calculatedScaleFactor = (float)Mathf.FloorToInt((float)Singleton<PlayerFileManager>.Instance.resolutionY / 360f);
            }
            else
            {
                calculatedScaleFactor = (float)Mathf.FloorToInt((float)Singleton<PlayerFileManager>.Instance.resolutionY / 480f);
            }
            canvas.scaleFactor = calculatedScaleFactor;
            canvas.worldCamera = camera.canvasCam;
            screenSize = new Vector2(Screen.width / calculatedScaleFactor, Screen.height / calculatedScaleFactor);
            _xMin.SetValue(tooltipController, 0f);
            _xMax.SetValue(tooltipController, canvas.GetComponent<RectTransform>().rect.width);
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
            tooltipBase.anchoredPosition = CursorController.Instance.GetComponent<RectTransform>().anchoredPosition;
            UIBuilder.LoadGlobalDefinesFromFile(Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "GlobalDefines.json"));
            EditorUIGlobalSettingsHandler settings = UIBuilder.BuildUIFromFile<EditorUIGlobalSettingsHandler>(canvas.GetComponent<RectTransform>(), "GlobalSettings", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "GlobalSettings.json"));
            settings.pages = new GlobalSettingsUIExchangeHandler[currentMode.pages.Count];
            settings.pageKeys = new string[currentMode.pages.Count];
            for (int i = 0; i < currentMode.pages.Count; i++)
            {
                EditorGlobalPage page = currentMode.pages[i];
                GlobalSettingsUIExchangeHandler handler = (GlobalSettingsUIExchangeHandler)UIBuilder.BuildUIFromFile(page.managerType, settings.GetComponent<RectTransform>(), page.pageName, page.filePath);
                handler.handler = settings;
                settings.pages[i] = handler;
                settings.pageKeys[i] = page.pageKey;
                if (i != 0)
                {
                    handler.gameObject.SetActive(false);
                }
            }
            uiObjects[2] = settings.gameObject;
            uiObjects[2].transform.SetAsFirstSibling();
            uiObjects[2].SetActive(false);
            uiObjects[1] = UIBuilder.BuildUIFromFile<EditorUIToolboxHandler>(canvas.GetComponent<RectTransform>(), "Toolbox", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "Toolbox.json")).gameObject;
            uiObjects[1].transform.SetAsFirstSibling();
            uiObjects[1].SetActive(false);
            uiObjects[0] = UIBuilder.BuildUIFromFile<EditorUIMainHandler>(canvas.GetComponent<RectTransform>(), "Main", Path.Combine(AssetLoader.GetModPath(LevelStudioPlugin.Instance), "Data", "UI", "Main.json")).gameObject;
            uiObjects[0].transform.SetAsFirstSibling();
        }

        public void UpdateSkybox()
        {
            Shader.SetGlobalTexture("_Skybox", LevelLoaderPlugin.Instance.skyboxAliases[levelData.skybox]);
        }

        public void LoadToolbar(string[] tools)
        {
            for (int i = 0; i < hotSlots.Length; i++)
            {
                if (i >= tools.Length)
                {
                    hotSlots[i].currentTool = null;
                    continue;
                }
                foreach (List<EditorTool> list in currentMode.availableTools.Values)
                {
                    bool breakOut = false;
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j].id == tools[i])
                        {
                            hotSlots[i].currentTool = list[j];
                            breakOut = true;
                            break;
                        }
                    }
                    if (breakOut) break;
                }
                //hotSlots[i].currentTool = currentMode.availableTools.Values.Select(x => x.First(z => z.id == currentMode.defaultTools[i])).First();
            }
        }

        // called when the editor mode is assigned or re-assigned
        public virtual void EditorModeAssigned()
        {
            UpdateUI();
            LoadToolbar(currentMode.defaultTools);
            UpdateMeta();
            if (levelData.meta == null)
            {
                levelData.meta = new PlayableLevelMeta()
                {
                    name = "My Awesome Level!",
                    modeSettings = LevelStudioPlugin.Instance.gameModeAliases[currentMode.availableGameModes[0]].CreateSettings(),
                    gameMode = currentMode.availableGameModes[0],
                    author = Singleton<PlayerFileManager>.Instance.fileName
                };
            }
            if (!currentMode.caresAboutSpawn) return;
            spawnpointVisual = GameObject.Instantiate(spawnpointVisualPrefab);
            spawnpointVisual.UpdateTransform();
        }

        protected void UpdateMouseRay()
        {
            Vector3 pos = new Vector3((CursorController.Instance.LocalPosition.x / screenSize.x) * Screen.width, Screen.height + ((CursorController.Instance.LocalPosition.y / screenSize.y) * Screen.height));
            mouseRay = camera.camCom.ScreenPointToRay(pos);
            Vector3? pPos = CastMouseRayToPlane(currentFloorPlane, false);
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
            if (_currentTool == null && toolboxOnNullTool)
            {
                toolboxOnNullTool = false;
                uiObjects[1].SetActive(true);
            }
        }

        /// <summary>
        /// Switches the current tool and sets a flag that will cause the toolbox to re-appear when the tool is done.
        /// </summary>
        /// <param name="tool"></param>
        public void SwitchToToolToolbox(EditorTool tool)
        {
            SwitchToTool(tool);
            toolboxOnNullTool = true;
        }

        /// <summary>
        /// Attempts to get the slot the mouse is hovering over and replaces it with the specified tool.
        /// This will open the toolbox afterwards if it wasnt already.
        /// </summary>
        /// <param name="tool"></param>
        public void SwitchCurrentHoveringSlot(EditorTool tool)
        {
            uiObjects[1].SetActive(false); // so theres nothing in the cursors way
            List<RaycastResult> results = (List<RaycastResult>)_results.GetValue(CursorController.Instance);
            for (int i = 0; i < results.Count; i++)
            {
                if (!results[i].gameObject.activeInHierarchy) continue;
                HotSlotScript hotSlot = results[i].gameObject.GetComponent<HotSlotScript>();
                if (hotSlot == null) continue;
                hotSlot.currentTool = tool;
                break;
            }
            uiObjects[1].SetActive(true); // put it back
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

            if (!levelData.ResizeLevel(posDif, sizeDif, this))
            {
                TriggerError("RoomClipped");
                return;
            }
            UpdateSpawnVisual();
            RegenerateGridAndCells();
            // move camera so the change isn't noticable
            transform.position -= new Vector3(posDif.x * 10f, 0f, posDif.z * 10f);
        }


        public void UpdateSpawnVisual()
        {
            if (spawnpointVisual == null) return;
            spawnpointVisual.UpdateTransform();
        }

        void Update()
        {
            canvas.scaleFactor = calculatedScaleFactor;
            UpdateMouseRay();
            PlaySongIfNecessary();
            UpdateCamera();
            if (selector.currentState == SelectorState.Object)
            {
                uiObjects[0].GetComponent<EditorUIMainHandler>().SendInteractionMessage("showTranslateSettings", null);
            }
            else
            {
                uiObjects[0].GetComponent<EditorUIMainHandler>().SendInteractionMessage("hideTranslateSettings", null);
            }
            if (((List<RaycastResult>)_results.GetValue(CursorController.Instance)).Count == 0)
            {
                HandleClicking();
            }
#if DEBUG
            if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.R))
            {
                UpdateUI();
                EditorModeAssigned();
            }
#endif
        }

        protected bool HandleInteractableClicking()
        {
            RaycastHit info;
            if (currentTool == null)
            {
                if (Physics.Raycast(mouseRay, out info, 1000f, LevelStudioPlugin.editorHandleLayerMask))
                {
                    if (info.transform.TryGetComponent(out IEditorInteractable interactable))
                    {
                        if ((currentTool == null))
                        {
                            if (interactable.OnClicked())
                            {
                                heldInteractable = interactable;
                                return true;
                            }
                            return true;
                        }
                    }
                }
            }
            if (Physics.Raycast(mouseRay, out info, 1000f, LevelStudioPlugin.editorInteractableLayerMask))
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
                        return true;
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
                    mousePressedLastFrame = mousePressedThisFrame;
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
                AddUndo();
                area.ResizeWithSafety(sizeDif, posDif);
                RefreshCells();
            }
            HighlightCells(area.CalculateOwnedCells(), "yellow");
            selector.SelectArea(area.rect.Value, (IntVector2 sd, IntVector2 pd) => AreaResize(area, sd, pd));
            selector.ShowSettings(((area.rect.Value.min.ToMystVector().ToWorld() + area.rect.Value.max.ToMystVector().ToWorld()) / 2f) + Vector3.up * 25f - new Vector3(5f,0f,5f), () =>
            {
                OpenRoomSettings(levelData.RoomFromId(area.roomId));
            });
        }

        protected void OpenRoomSettings(EditorRoom room)
        {
            RoomSettingsExchangeHandler settings = CreateUI<RoomSettingsExchangeHandler>("RoomConfig");
            settings.AssignRoom(room);
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

        static FieldInfo _midiPlayer = AccessTools.Field(typeof(MusicManager), "midiPlayer"); // type: MidiFilePlayer

        protected virtual void PlaySongIfNecessary()
        {
            if (!Singleton<MusicManager>.Instance.MidiPlaying)
            {
                Singleton<MusicManager>.Instance.StopMidi();
                Singleton<MusicManager>.Instance.PlayMidi(LevelStudioPlugin.Instance.editorTracks[UnityEngine.Random.Range(0, LevelStudioPlugin.Instance.editorTracks.Count)], false);
            }
        }

        protected int mutedChannels = 0;
        public void SetChannelsMuted(bool inMenu)
        {
            if (inMenu)
            {
                mutedChannels++;
            }
            else
            {
                mutedChannels = Mathf.Max(mutedChannels - 1, 0);
            }

            bool shouldMute = mutedChannels > 0;
            MidiFilePlayer midiPlayer = (MidiFilePlayer)_midiPlayer.GetValue(Singleton<MusicManager>.Instance);
            for (int i = 0; i < midiPlayer.Channels.Length; i++)
            {
                midiPlayer.MPTK_ChannelEnableSet(i, shouldMute ? (i == 1 || i == 9) : true);
            }
        }

        protected virtual void UpdateCamera()
        {
            if (!MovementEnabled) return;
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
        public void RefreshCells(bool refreshLights = true, bool updateDataCells = true)
        {
            if (updateDataCells)
            {
                levelData.UpdateCells(true);
            }
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
                    EditorRoom room = levelData.RoomFromId(levelData.GetCellSafe(x,y).roomId);
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
            UpdateStructuresWithReason(PotentialStructureUpdateReason.CellChange);
            if (!refreshLights) return;
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

        protected virtual void UpdateMeta()
        {
            currentFile.meta.editorMode = currentMode.id;
            currentFile.meta.cameraPosition = transform.position;
            currentFile.meta.cameraRotation = transform.rotation;
            for (int i = 0; i < hotSlots.Length; i++)
            {
                currentFile.meta.toolbarTools[i] = hotSlots[i].currentTool.id;
            }
        }

        protected override void AwakeFunction()
        {
            currentFile = new EditorFileContainer();
            currentFile.meta = new EditorFileMeta();
            levelData = new EditorLevelData(new IntVector2(50,50));
            gridManager = GameObject.Instantiate(gridManagerPrefab);
            gridManager.editor = this;
            camera = GameObject.Instantiate(cameraPrefab);
            camera.UpdateTargets(transform,0);
            canvas.transform.SetParent(null);
            selector = GameObject.Instantiate(selectorPrefab);
            workerEc = GameObject.Instantiate(ecPrefab);
            workerEc.gameObject.SetActive(false);
            workerEc.name = "WorkerEnvironmentController";
            workerEc.lightMode = LightMode.Cumulative;
            textTextureGenerator = (TextTextureGenerator)_TextTextureGenerator.GetValue(workerEc);
            textTextureGenerator.transform.SetParent(null);
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
