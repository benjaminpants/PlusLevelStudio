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

        public Canvas canvas;

        //public Material tileAlphaMaterial;
        public Material gridMaterial;
        public Material selectMaterial;

        public GameObject testQuad;

        public GameCamera cameraPrefab;
        public GameCamera camera;
        public Vector3 cameraRotation = new Vector3(0f, 0f, 0f);
        public Vector2 screenSize = Vector2.one;
        public float calculatedScaleFactor = 1f;
        public Plane currentFloorPlane = new Plane(Vector3.up, 0f);
        public Ray mouseRay;


        public GameObject CreateQuad(string name, Material mat, Vector3 position, Vector3 rotation)
        {
            GameObject newQuad = new GameObject(name);
            newQuad.gameObject.AddComponent<MeshFilter>().mesh = LevelStudioPlugin.Instance.assetMan.Get<Mesh>("Quad");
            newQuad.AddComponent<MeshRenderer>().material = mat;
            newQuad.transform.position = position;
            newQuad.transform.eulerAngles = rotation;
            newQuad.transform.localScale *= 10f;
            return newQuad;
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
            Vector3? pos = CastRayToPlane(currentFloorPlane, false);
            if (pos.HasValue)
            {
                testQuad.transform.position = IntVectorToWorld(WorldToIntVector(pos.Value)) + (Vector3.up * 0.01f);
            }
        }

        protected virtual void PlaySongIfNecessary()
        {
            if (!Singleton<MusicManager>.Instance.MidiPlaying)
            {
                Singleton<MusicManager>.Instance.PlayMidi(LevelStudioPlugin.Instance.editorTracks[UnityEngine.Random.Range(0, LevelStudioPlugin.Instance.editorTracks.Count)], false);
            }
        }

        /// <summary>
        /// Converts a given position to an int vector
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public IntVector2 WorldToIntVector(Vector3 position)
        {
            float x = (position.x - 5f) / 10f;
            float y = (position.z - 5f) / 10f;
            return new IntVector2(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        }

        /// <summary>
        /// Converts a given IntVector to a world position
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public Vector3 IntVectorToWorld(IntVector2 vector)
        {
            return new Vector3((vector.x * 10f + 5f), 0f, (vector.z * 10f + 5f));
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

        protected override void AwakeFunction()
        {
            camera = GameObject.Instantiate(cameraPrefab);
            camera.UpdateTargets(transform,0);
            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    CreateQuad("Grid", gridMaterial, IntVectorToWorld(new IntVector2(x,y)), new Vector3(90f, 0f, 0f));
                }
            }
            testQuad = CreateQuad("TestQuad", selectMaterial, Vector3.zero, new Vector3(90f, 0f, 0f));
            canvas.transform.SetParent(null);
            UpdateUI();
        }
    }
}
