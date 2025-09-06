using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using PlusStudioLevelLoader;
using UnityEngine.Events;

namespace PlusLevelStudio.Ingame
{
    public class Structure_Teleporters : StructureBuilder
    {

        public Transform buttonPanelPre;
        public TeleporterController controllerPre;

        public override void Generate(LevelGenerator lg, System.Random rng)
        {
            throw new NotImplementedException("This is for editor levels only!");
        }

        public override void Load(List<StructureData> data)
        {
            base.Load(data);
            Queue<StructureData> queue = new Queue<StructureData>(data);
            List<TeleporterRoomFunction> toSetup = new List<TeleporterRoomFunction>();
            // teleporters
            while (queue.Count != 0)
            {
                RoomController room = ec.rooms[queue.Dequeue().data];
                if (!room.functionObject.TryGetComponent<EditorTeleporterRoomFunction>(out EditorTeleporterRoomFunction function))
                {
                    throw new InvalidOperationException("Can't add teleporter to non-teleporter room!");
                }
                StructureData buttonPanelData = queue.Dequeue();
                Transform buttonPanel = GameObject.Instantiate<Transform>(buttonPanelPre, room.objectObject.transform);
                buttonPanel.transform.position = new Vector3(buttonPanelData.position.x.ConvertToFloatNoRecast(), 0f, buttonPanelData.position.z.ConvertToFloatNoRecast());
                buttonPanel.transform.eulerAngles = new Vector3(0f, buttonPanelData.data.ConvertToFloatNoRecast(), 0f);
                Debug.Log(buttonPanelData.position.x);
                Debug.Log(buttonPanelData.position.z);
                Debug.Log("button panel done");
                StructureData teleporterData = queue.Dequeue();
                TeleporterController controller = GameObject.Instantiate<TeleporterController>(controllerPre, room.objectObject.transform);
                controller.transform.position = new Vector3(teleporterData.position.x.ConvertToFloatNoRecast(), 0f, teleporterData.position.z.ConvertToFloatNoRecast());
                controller.transform.eulerAngles = new Vector3(0f, teleporterData.data.ConvertToFloatNoRecast(), 0f);
                Debug.Log(teleporterData.position.x);
                Debug.Log(teleporterData.position.z);
                Debug.Log("controller done");
                function.AssignTeleporterController(controller);
                Debug.Log("assign teleporter done");
                function.AssignButtonPanel(buttonPanel);
                Debug.Log("assign button panel done");
                toSetup.Add(function);
            }
            for (int i = 0; i < toSetup.Count; i++)
            {
                Debug.Log("setup: " + i);
                toSetup[i].Setup(toSetup, toSetup[i].Room.GetComponent<EditorRegionMarker>().region - 1);
            }
        }
    }

    public class EditorTeleporterRoomFunction : TeleporterRoomFunction
    {
        static FieldInfo _teleporterController = AccessTools.Field(typeof(TeleporterRoomFunction), "teleporterController");
        static FieldInfo _label = AccessTools.Field(typeof(TeleporterRoomFunction), "label");
        static FieldInfo _button = AccessTools.Field(typeof(TeleporterController), "button");

        static FieldInfo _OnPress = AccessTools.Field(typeof(GameButton), "OnPress");
        protected override void OnLastEntityExit(Entity entity)
        {
            if (_teleporterController.GetValue(this) == null) return;
            base.OnLastEntityExit(entity);
        }

        public void AssignTeleporterController(TeleporterController tc)
        {
            _teleporterController.SetValue(this, tc);
        }

        public void AssignButtonPanel(Transform panel)
        {
            GameObject[] label = (GameObject[])_label.GetValue(this);
            for (int i = 0; i < label.Length; i++)
            {
                label[i] = panel.Find("RoomLabels_" + i).gameObject;
            }
            Debug.Log("array label");
            _label.SetValue(this, label);
            Debug.Log("array set label");
            TeleporterController controller = (TeleporterController)_teleporterController.GetValue(this);
            Debug.Log("got controller");
            GameButtonBase[] button = (GameButtonBase[])_button.GetValue(controller);
            Debug.Log("got buttons");
            for (int i = 0; i < button.Length; i++)
            {
                int index = i; // this hurts me on some level. but if you dont do it then it'll always do the last button
                button[i] = panel.Find("GameButton_" + i).GetComponent<GameButton>();
                Debug.Log("button" + i);
                // need to re-assign to refer to OUR controller
                Debug.Log("unity event. fuck");
                UnityEvent unEvent = new UnityEvent();
                unEvent.AddListener(() =>
                {
                    controller.ButtonPressed(index);
                });
                Debug.Log("setting onpress");
                _OnPress.SetValue(button[i], unEvent);
                Debug.Log("ok");
            }
        }
    }
}
