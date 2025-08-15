using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor.SettingsUI
{
    public class SteamValveSettingsExchangeHandler : EditorOverlayUIExchangeHandler
    {
        TextMeshProUGUI strengthText;
        TextMeshProUGUI chanceText;
        public SteamValveLocation myValve;

        bool somethingChanged = false;

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            strengthText = transform.Find("StrengthBox").GetComponent<TextMeshProUGUI>();
            chanceText = transform.Find("ChanceBox").GetComponent<TextMeshProUGUI>();
        }

        public void Refresh()
        {
            strengthText.text = myValve.strength.ToString();
            chanceText.text = GetStructure().startOnChance.ToString();
        }

        public SteamValveStructureLocation GetStructure()
        {
            return (SteamValveStructureLocation)EditorController.Instance.GetStructureData("steamvalves");
        }

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

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "setStrength":
                    int dat = int.Parse((string)data);
                    myValve.strength = (byte)Mathf.Clamp(dat,0,255);
                    somethingChanged = true;
                    EditorController.Instance.UpdateVisual(myValve);
                    Refresh();
                    break;
                case "setChance":
                    int chanceDat = int.Parse((string)data);
                    GetStructure().startOnChance = (byte)Mathf.Clamp(chanceDat, 0, 100);
                    somethingChanged = true;
                    Refresh();
                    break;
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
