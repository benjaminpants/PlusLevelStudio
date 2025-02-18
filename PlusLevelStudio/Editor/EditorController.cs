using HarmonyLib;
using MTM101BaldAPI.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlusLevelStudio.Editor
{
    public class EditorController : Singleton<EditorController>
    {
        protected static FieldInfo _deltaThisFrame = AccessTools.Field(typeof(CursorController), "deltaThisFrame");

        public EditorLevelData levelData;
        public Canvas canvas;

        //public Material tileAlphaMaterial;
        public Material gridMaterial;

        public Selector selector;

        public Selector selectorPrefab;

        public Transform gridTransform;

        public GameCamera cameraPrefab;
        public GameCamera camera;
        public Vector3 cameraRotation = new Vector3(0f, 0f, 0f);
        public Vector2 screenSize = Vector2.one;
        public float calculatedScaleFactor = 1f;
        public Plane currentFloorPlane = new Plane(Vector3.up, 0f);
        public Ray mouseRay;

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
            CursorInitiator init = canvas.GetComponent<CursorInitiator>();
            init.screenSize = screenSize;
            init.Inititate();
        }

        protected void UpdateMouseRay()
        {
            Vector3 pos = new Vector3((CursorController.Instance.LocalPosition.x / screenSize.x) * Screen.width, Screen.height + ((CursorController.Instance.LocalPosition.y / screenSize.y) * Screen.height));
            mouseRay = camera.camCom.ScreenPointToRay(pos);
        }

        void Update()
        {
            canvas.scaleFactor = calculatedScaleFactor;
            UpdateMouseRay();
            PlaySongIfNecessary();
            UpdateCamera();
            if (selector.dragging)
            {
                return;
            }
            if (Singleton<InputManager>.Instance.GetDigitalInput("Interact", true))
            {
                // TODO: Put standard raycast here

                // If no other important collisions have been found, assume we want to click on a tile
                Vector3? pos = CastRayToPlane(currentFloorPlane, false);
                if (pos.HasValue)
                {
                    IntVector2 cellPosition = pos.Value.ToCellVector();
                    if (cellPosition.x >= 0 && cellPosition.z >= 0 && cellPosition.x < levelData.mapSize.x && cellPosition.z < levelData.mapSize.z)
                    {
                        SelectTile(cellPosition);
                    }
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

        private GameObject[] gridObjects = new GameObject[0];


        protected void RegenerateGrid()
        {
            // todo: make it so it doesn't delete the entire grid everytime it needs to be regenerated
            for (int i = 0; i < gridObjects.Length; i++)
            {
                GameObject.Destroy(gridObjects[i]);
            }
            gridObjects = new GameObject[levelData.mapSize.x * levelData.mapSize.z];
            int count = 0;
            for (int x = 0; x < levelData.mapSize.x; x++)
            {
                for (int y = 0; y < levelData.mapSize.z; y++)
                {
                    gridObjects[count] = LevelStudioPlugin.CreateQuad("Grid", gridMaterial, new IntVector2(x, y).ToWorld(), new Vector3(90f, 0f, 0f));
                    gridObjects[count].transform.SetParent(gridTransform, true);
                    count++;
                }
            }
        }

        protected override void AwakeFunction()
        {
            levelData = new EditorLevelData();
            gridTransform = new GameObject("Grid").transform;
            camera = GameObject.Instantiate(cameraPrefab);
            camera.UpdateTargets(transform,0);
            RegenerateGrid();
            canvas.transform.SetParent(null);
            UpdateUI();
            selector = GameObject.Instantiate(selectorPrefab);
        }
    }
}
