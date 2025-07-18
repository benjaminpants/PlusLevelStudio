using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;

namespace PlusLevelStudio.Editor.SettingsUI
{
    public class LockdownDoorSettingsExchangeHandler : EditorOverlayUIExchangeHandler
    {
        public LockdownDoorLocation myDoor;
        public LockdownDoorStructureLocation myLocal;
        public TextMeshProUGUI shutText;
        bool somethingChanged = false;

        public override bool OnExit()
        {
            if (somethingChanged)
            {
                EditorController.Instance.AddHeldUndo();
            }
            else
            {
                EditorController.Instance.CancelHeldUndo();
            }
            return base.OnExit();
        }

        public void Assigned()
        {
            shutText.text = myDoor.prefab == myLocal.type ? "Starts Open" : "Starts Shut";
        }

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            EditorController.Instance.HoldUndo();
            shutText = transform.Find("DoorShutText").GetComponent<TextMeshProUGUI>();
        }


        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "toggleShut":
                    if (myDoor.prefab == myLocal.type)
                    {
                        myDoor.prefab = "lockdowndoor_shut";
                        shutText.text = "Starts Shut";
                    }
                    else
                    {
                        myDoor.prefab = myLocal.type;
                        shutText.text = "Starts Open";
                    }
                    somethingChanged = true;
                    // so we can change prefab
                    EditorController.Instance.RemoveVisual(myDoor);
                    EditorController.Instance.AddVisual(myDoor);
                    EditorController.Instance.UpdateVisual(myLocal);
                    break;
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
