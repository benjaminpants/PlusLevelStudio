using System;
using System.Collections.Generic;
using System.Text;
using PlusLevelStudio.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.Editor.SettingsUI
{
    public class LightSettingsExchangeHandler : EditorOverlayUIExchangeHandler
    {
        public LightPlacement myPlacement;
        public bool somethingChanged = false;
        public TextMeshProUGUI groupText;
        public TextMeshProUGUI strengthText;

        public float redTickStartY = 0f;
        public float greenTickStartY = 0f;
        public float blueTickStartY = 0f;
        public RectTransform redTick;
        public RectTransform greenTick;
        public RectTransform blueTick;

        public TextMeshProUGUI redText;
        public TextMeshProUGUI greenText;
        public TextMeshProUGUI blueText;
        public Image hexDisplay;
        public TextMeshProUGUI hexText;

        public void Refresh(bool refreshHexText = true)
        {
            LightGroup currentGroup = EditorController.Instance.levelData.lightGroups[myPlacement.lightGroup];
            groupText.text = (myPlacement.lightGroup + 1).ToString();
            strengthText.text = currentGroup.strength.ToString();
            redTick.anchoredPosition = new Vector2(redTick.anchoredPosition.x,redTickStartY + Mathf.Round(currentGroup.color.r * 128));
            greenTick.anchoredPosition = new Vector2(greenTick.anchoredPosition.x, greenTickStartY + Mathf.Round(currentGroup.color.g * 128));
            blueTick.anchoredPosition = new Vector2(blueTick.anchoredPosition.x, blueTickStartY + Mathf.Round(currentGroup.color.b * 128));
            redText.text = Mathf.RoundToInt(currentGroup.color.r * 255f).ToString();
            greenText.text = Mathf.RoundToInt(currentGroup.color.g * 255f).ToString();
            blueText.text = Mathf.RoundToInt(currentGroup.color.b * 255f).ToString();
            hexDisplay.color = currentGroup.color;
            if (refreshHexText)
            {
                hexText.text = ColorUtility.ToHtmlStringRGB(currentGroup.color);
            }
        }

        public void ChangeRed(float newVal)
        {
            LightGroup currentGroup = EditorController.Instance.levelData.lightGroups[myPlacement.lightGroup];
            currentGroup.color = new Color(newVal, currentGroup.color.g, currentGroup.color.b);
        }

        public void ChangeGreen(float newVal)
        {
            LightGroup currentGroup = EditorController.Instance.levelData.lightGroups[myPlacement.lightGroup];
            currentGroup.color = new Color(currentGroup.color.r, newVal, currentGroup.color.b);
        }

        public void ChangeBlue(float newVal)
        {
            LightGroup currentGroup = EditorController.Instance.levelData.lightGroups[myPlacement.lightGroup];
            currentGroup.color = new Color(currentGroup.color.r, currentGroup.color.g, newVal);
        }

        public override void OnElementsCreated()
        {
            groupText = transform.Find("LightGroupCounter").GetComponent<TextMeshProUGUI>();
            strengthText = transform.Find("StrengthCount").GetComponent<TextMeshProUGUI>();
            redTick = transform.Find("RedSliderTick").GetComponent<RectTransform>();
            greenTick = transform.Find("GreenSliderTick").GetComponent<RectTransform>();
            blueTick = transform.Find("BlueSliderTick").GetComponent<RectTransform>();
            redTickStartY = redTick.anchoredPosition.y;
            blueTickStartY = blueTick.anchoredPosition.y;
            greenTickStartY = greenTick.anchoredPosition.y;
            redText = transform.Find("RedText").GetComponent<TextMeshProUGUI>();
            greenText = transform.Find("GreenText").GetComponent<TextMeshProUGUI>();
            blueText = transform.Find("BlueText").GetComponent<TextMeshProUGUI>();
            hexDisplay = transform.Find("HexDisplay").GetComponent<Image>();
            hexText = transform.Find("HexCodeDisplay").GetComponent<TextMeshProUGUI>();
            base.OnElementsCreated();
        }

        public override bool OnExit()
        {
            if (somethingChanged)
            {
                EditorController.Instance.AddHeldUndo();
            }
            return base.OnExit();
        }

        public void ChangeLightStrength(int amount)
        {
            EditorController.Instance.levelData.lightGroups[myPlacement.lightGroup].strength = (byte)Mathf.Clamp(EditorController.Instance.levelData.lightGroups[myPlacement.lightGroup].strength + amount, 0, 255);
        }

        public void ChangeLightGroup(int amount)
        {
            int resultingGroup = myPlacement.lightGroup + amount;
            if (resultingGroup < 0)
            {
                myPlacement.lightGroup = (ushort)(EditorController.Instance.levelData.lightGroups.Count - 1);
                return;
            }
            if (resultingGroup >= EditorController.Instance.levelData.lightGroups.Count)
            {
                myPlacement.lightGroup = 0;
                return;
            }
            myPlacement.lightGroup = (ushort)resultingGroup;
        }

        public void AddLightGroup()
        {
            LightGroup newGroup = new LightGroup(EditorController.Instance.levelData.lightGroups[myPlacement.lightGroup]);
            EditorController.Instance.levelData.lightGroups.Add(newGroup);
            myPlacement.lightGroup = (ushort)(EditorController.Instance.levelData.lightGroups.Count - 1);
        }

        public void UpdateRefreshMark()
        {
            EditorController.Instance.RefreshLights();
            somethingChanged = true;
            Refresh();
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "changeLightGroupPrev":
                    ChangeLightGroup(-1);
                    UpdateRefreshMark();
                    break;
                case "changeLightGroupNext":
                    ChangeLightGroup(1);
                    UpdateRefreshMark();
                    break;
                case "strengthAdd":
                    ChangeLightStrength(1);
                    UpdateRefreshMark();
                    break;
                case "strengthSub":
                    ChangeLightStrength(-1);
                    UpdateRefreshMark();
                    break;
                case "addGroup":
                    AddLightGroup();
                    UpdateRefreshMark();
                    break;
                case "red":
                    ChangeRed(Mathf.Clamp(((Vector3)data).y / 128f,0f,1f));
                    UpdateRefreshMark();
                    break;
                case "green":
                    ChangeGreen(Mathf.Clamp(((Vector3)data).y / 128f, 0f, 1f));
                    UpdateRefreshMark();
                    break;
                case "blue":
                    ChangeBlue(Mathf.Clamp(((Vector3)data).y / 128f, 0f, 1f));
                    UpdateRefreshMark();
                    break;
                case "hexDone":
                case "hex":
                    if (ColorUtility.TryParseHtmlString("#" + (hexText.text.PadRight(6,'0')), out Color c))
                    {
                        EditorController.Instance.levelData.lightGroups[myPlacement.lightGroup].color = c;
                        EditorController.Instance.RefreshLights();
                        somethingChanged = true;
                        Refresh(message == "hexDone");
                    }
                    break;
            }
            base.SendInteractionMessage(message, data);
        }
    }
}
