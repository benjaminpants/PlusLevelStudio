using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor.SettingsUI
{
    public class PowerLeverSettingsExchangeHandler : EditorOverlayUIExchangeHandler
    {
        TextMeshProUGUI percentageOnText;
        TextMeshProUGUI maxLevers;

        bool somethingChanged = false;

        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            Transform percentOn = transform.Find("PercentOnBox");
            if (percentOn != null)
            {
                percentageOnText = percentOn.GetComponent<TextMeshProUGUI>();
            }
            Transform maxLeverT = transform.Find("MaxLeversBox");
            if (maxLeverT != null)
            {
                maxLevers = maxLeverT.GetComponent<TextMeshProUGUI>();
            }
        }

        public void Refresh()
        {
            if (percentageOnText != null)
            {
                percentageOnText.text = GetStructure().poweredRoomChance.ToString();
            }
            if (maxLevers != null)
            {
                maxLevers.text = GetStructure().maxLevers.ToString();
            }
        }

        public PowerLeverStructureLocation GetStructure()
        {
            return (PowerLeverStructureLocation)EditorController.Instance.AddOrGetStructureToData("powerlever", true);
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
                case "setOnPercent":
                    if (int.TryParse((string)data, out int percent))
                    {
                        GetStructure().poweredRoomChance = Mathf.Clamp(percent, 0, 100);
                        somethingChanged = true;
                    }
                    Refresh();
                    break;
                case "setMaxLevers":
                    if (int.TryParse((string)data, out int maxLevers))
                    {
                        GetStructure().maxLevers = Mathf.Max(maxLevers, 1);
                        somethingChanged = true;
                    }
                    Refresh();
                    break;
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
