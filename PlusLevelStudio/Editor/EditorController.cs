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

namespace PlusLevelStudio.Editor
{
    public class EditorController : Singleton<EditorController>
    {
        protected static FieldInfo _deltaThisFrame = AccessTools.Field(typeof(CursorController), "deltaThisFrame");
        protected static FieldInfo _results = AccessTools.Field(typeof(CursorController), "results");

        public HotSlotScript[] hotSlots = new HotSlotScript[9];

        public EditorTool currentTool => _currentTool;
        protected EditorTool _currentTool;

        public EditorMode currentMode;
        public EditorLevelData levelData;
        public Canvas canvas;

        public Tile[][] tiles = new Tile[0][];
        public GameObject[] uiObjects = new GameObject[1];

        public EnvironmentController workerEc;
        public EnvironmentController ecPrefab;

        public CoreGameManager workerCgm;
        public CoreGameManager cgmPrefab;

        //public Material tileAlphaMaterial;

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
            hotSlots[0].currentTool = currentMode.availableTools[0];
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
            if (sizeDif.x == 0 && sizeDif.z == 0) return; // no point in doing anything?
            IntVector2 targetSize = levelData.mapSize + sizeDif;
            if (targetSize.x > 255 || targetSize.z > 255) { TriggerError("LevelTooBig"); return; }
            if (targetSize.x < 1 || targetSize.z < 1) { TriggerError("LevelTooSmall"); return; }
            // TODO: INSERT LOGIC FOR HANDLING AREAS AND OBJECTS
            // IF A RESIZE WOULD CUT OFF AN AREA IT SHOULD TRIGGER A "AREA IN THE WAY" ERROR
            // IF POSDIF IS ZERO THEN SKIP ALL THAT LOGIC BECAUSE ITS UNNECESSARY

            if (!levelData.ResizeLevel(posDif, sizeDif))
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

        protected void HandleClicking()
        {
            if (currentTool != null)
            {
                bool mousePressedThisFrame = Singleton<InputManager>.Instance.GetDigitalInput("Interact", false);
                if (mousePressedLastFrame != mousePressedThisFrame)
                {
                    if (mousePressedThisFrame)
                    {
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
                if (Physics.Raycast(mouseRay, out RaycastHit info, 1000f, LevelStudioPlugin.editorInteractableLayerMask))
                {
                    if (info.transform.TryGetComponent(out IEditorInteractable interactable))
                    {
                        if (interactable.OnClicked())
                        {
                            heldInteractable = interactable;
                            return;
                        }
                    }
                }
                if (mouseGridPosition.x >= 0 && mouseGridPosition.z >= 0 && mouseGridPosition.x < levelData.mapSize.x && mouseGridPosition.z < levelData.mapSize.z)
                {
                    SelectTile(mouseGridPosition);
                }
            }
        }

        protected virtual void SelectTile(IntVector2 tileSelected)
        {
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
            workerEc.InitializeLighting();
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
            LevelStudioPlugin.Instance.lightmaps["standard"].Apply(false,false);
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

            workerCgm = GameObject.Instantiate(cgmPrefab);
            workerCgm.gameObject.SetActive(true);
            workerCgm.gameObject.SetActive(false);
            RegenerateGridAndCells();
        }
    }
}
