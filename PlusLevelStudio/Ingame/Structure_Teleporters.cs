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
    public class DummyTeleporterController : TeleporterController
    {
    }

    public class Structure_Teleporters : StructureBuilder
    {

        public Transform buttonPanelPre;
        public TeleporterController controllerPre;
        public DummyTeleporterRoomFunction dummyTeleporter;
        static FieldInfo _teleporterController = AccessTools.Field(typeof(TeleporterRoomFunction), "teleporterController");
        static MethodInfo _UpdateButtons = AccessTools.Method(typeof(TeleporterController), "UpdateButtons");

        public override void Generate(LevelGenerator lg, System.Random rng)
        {
            throw new NotImplementedException("This is for editor levels only!");
        }

        public override void Load(List<StructureData> data)
        {
            base.Load(data);
            Queue<StructureData> queue = new Queue<StructureData>(data);
            List<TeleporterRoomFunction> toSetup = new List<TeleporterRoomFunction>();
            Dictionary<TeleporterRoomFunction, int> ids = new Dictionary<TeleporterRoomFunction, int>();
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
                StructureData teleporterData = queue.Dequeue();
                TeleporterController controller = GameObject.Instantiate<TeleporterController>(controllerPre, room.objectObject.transform);
                controller.transform.position = new Vector3(teleporterData.position.x.ConvertToFloatNoRecast(), 0f, teleporterData.position.z.ConvertToFloatNoRecast());
                controller.transform.eulerAngles = new Vector3(0f, teleporterData.data.ConvertToFloatNoRecast(), 0f);
                function.AssignTeleporterController(controller);
                function.AssignButtonPanel(buttonPanel);
                toSetup.Add(function);
                ids.Add(function, room.GetComponent<EditorRegionMarker>().region);
            }
            for (int i = 0; i < Mathf.Max(4, toSetup.Count); i++)
            {
                if (!ids.ContainsValue(i + 1))
                {
                    Debug.Log("no teleporter found for: " + (i + 1));
                    DummyTeleporterRoomFunction dummy = GameObject.Instantiate<DummyTeleporterRoomFunction>(dummyTeleporter, ec.transform);
                    dummy.SetUpForDummy();
                    ids.Add(dummy, i + 1);
                    toSetup.Add(dummy);
                }
            }
            toSetup.Sort((a, b) => ids[a].CompareTo(ids[b])); // sort by ascending
            for (int i = 0; i < toSetup.Count; i++)
            {
                if (toSetup[i] is DummyTeleporterRoomFunction) continue;
                int region = ids[toSetup[i]] - 1;
                toSetup[i].Setup(toSetup, region);
                _UpdateButtons.Invoke(_teleporterController.GetValue(toSetup[i]), new object[0]);
            }
        }
    }

    public class DummyTeleporterRoomFunction : TeleporterRoomFunction
    {
        static FieldInfo _teleporterController = AccessTools.Field(typeof(TeleporterRoomFunction), "teleporterController");
        static FieldInfo _cooled = AccessTools.Field(typeof(TeleporterController), "cooled");

        public override void Initialize(RoomController room)
        {
            throw new InvalidOperationException("DummyTeleporterRoomFunction isn't meant to be used as an actual room function!!");
        }

        public void SetUpForDummy()
        {
            _cooled.SetValue(_teleporterController.GetValue(this), false);
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
            _label.SetValue(this, label);
            TeleporterController controller = (TeleporterController)_teleporterController.GetValue(this);
            GameButtonBase[] button = (GameButtonBase[])_button.GetValue(controller);
            for (int i = 0; i < button.Length; i++)
            {
                int index = i; // this hurts me on some level. but if you dont do it then it'll always do the last button
                button[i] = panel.Find("GameButton_" + i).GetComponent<GameButton>();
                // need to re-assign to refer to OUR controller
                UnityEvent unEvent = new UnityEvent();
                unEvent.AddListener(() =>
                {
                    controller.ButtonPressed(index);
                });
                _OnPress.SetValue(button[i], unEvent);
            }
        }
    }
}
