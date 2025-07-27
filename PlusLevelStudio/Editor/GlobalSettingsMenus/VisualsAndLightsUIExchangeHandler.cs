using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public class VisualsAndLightsUIExchangeHandler : GlobalSettingsUIExchangeHandler
    {
        public static float minHideLevel = 0.05f;
        // color stuff
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
        public void ChangeRed(float newVal)
        {
            EditorController.Instance.levelData.minLightColor = new Color(newVal, EditorController.Instance.levelData.minLightColor.g, EditorController.Instance.levelData.minLightColor.b);
        }
        public void ChangeGreen(float newVal)
        {
            EditorController.Instance.levelData.minLightColor = new Color(EditorController.Instance.levelData.minLightColor.r, newVal, EditorController.Instance.levelData.minLightColor.b);
        }
        public void ChangeBlue(float newVal)
        {
            EditorController.Instance.levelData.minLightColor = new Color(EditorController.Instance.levelData.minLightColor.r, EditorController.Instance.levelData.minLightColor.g, newVal);
        }

        StandardMenuButton[] skyboxButtons;
        GameObject warningButton;
        Image skyboxImage;
        int skyboxViewOffset = 0;

        public TextMeshProUGUI additiveButton;
        public TextMeshProUGUI greatestButton;
        public TextMeshProUGUI cumulativeButton;
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            skyboxButtons = new StandardMenuButton[]
            {
                transform.Find("Skybox0").GetComponent<StandardMenuButton>(),
                transform.Find("Skybox1").GetComponent<StandardMenuButton>(),
                transform.Find("Skybox2").GetComponent<StandardMenuButton>(),
            };
            skyboxImage = transform.Find("CurrentSkybox").GetComponent<Image>();
            cumulativeButton = transform.Find("LightModeCumulative").GetComponent<TextMeshProUGUI>();
            additiveButton = transform.Find("LightModeAdditive").GetComponent<TextMeshProUGUI>();
            greatestButton = transform.Find("LightModeGreatest").GetComponent<TextMeshProUGUI>();

            // color stuff
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
            warningButton = transform.Find("UnhideableWarning").gameObject;
        }

        public override void Refresh()
        {
            RefreshSkyboxView();
            RefreshColorStuff();
            RefreshLightmode();
        }

        public void RefreshLightmode()
        {
            cumulativeButton.text = (EditorController.Instance.levelData.lightMode == LightMode.Cumulative ? "<b>" : "") + LocalizationManager.Instance.GetLocalizedText("Ed_LightMode_Cumulative");
            additiveButton.text = (EditorController.Instance.levelData.lightMode == LightMode.Additive ? "<b>" : "") + LocalizationManager.Instance.GetLocalizedText("Ed_LightMode_Additive");
            greatestButton.text = (EditorController.Instance.levelData.lightMode == LightMode.Greatest ? "<b>" : "") + LocalizationManager.Instance.GetLocalizedText("Ed_LightMode_Greatest");
        }

        public void RefreshColorStuff(bool updateHex = true)
        {
            Color currentColor = EditorController.Instance.levelData.minLightColor;
            redTick.anchoredPosition = new Vector2(redTick.anchoredPosition.x, redTickStartY + Mathf.Round(currentColor.r * 128));
            greenTick.anchoredPosition = new Vector2(greenTick.anchoredPosition.x, greenTickStartY + Mathf.Round(currentColor.g * 128));
            blueTick.anchoredPosition = new Vector2(blueTick.anchoredPosition.x, blueTickStartY + Mathf.Round(currentColor.b * 128));
            redText.text = Mathf.RoundToInt(currentColor.r * 255f).ToString();
            greenText.text = Mathf.RoundToInt(currentColor.g * 255f).ToString();
            blueText.text = Mathf.RoundToInt(currentColor.b * 255f).ToString();
            hexDisplay.color = currentColor;
            if (updateHex)
            {
                hexText.text = ColorUtility.ToHtmlStringRGB(currentColor);
            }
            warningButton.gameObject.SetActive(Mathf.Max(currentColor.r, currentColor.g, currentColor.b) > minHideLevel);
        }

        public void RefreshSkyboxView()
        {
            for (int i = 0; i < skyboxButtons.Length; i++)
            {
                if ((i + skyboxViewOffset) >= LevelStudioPlugin.Instance.selectableSkyboxes.Count)
                {
                    skyboxButtons[i].image.color = Color.clear;
                    continue;
                }
                skyboxButtons[i].image.color = Color.white;
                skyboxButtons[i].image.sprite = LevelStudioPlugin.Instance.skyboxSprites[LevelStudioPlugin.Instance.selectableSkyboxes[i + skyboxViewOffset]];
            }
            skyboxImage.sprite = LevelStudioPlugin.Instance.skyboxSprites[EditorController.Instance.levelData.skybox];
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            if (message.StartsWith("skybox"))
            {
                int index = int.Parse(message.Replace("skybox", ""));
                int selectableSkyboxIndex = index + skyboxViewOffset;
                if (selectableSkyboxIndex >= LevelStudioPlugin.Instance.selectableSkyboxes.Count)
                {
                    return;
                }
                EditorController.Instance.levelData.skybox = LevelStudioPlugin.Instance.selectableSkyboxes[index + skyboxViewOffset];
                EditorController.Instance.UpdateSkybox();
                RefreshSkyboxView();
            }
            switch (message)
            {
                case "nextSkybox":
                    skyboxViewOffset = Mathf.Clamp(skyboxViewOffset + 1, 0, LevelStudioPlugin.Instance.selectableSkyboxes.Count - skyboxButtons.Length);
                    RefreshSkyboxView();
                    break;
                case "prevSkybox":
                    skyboxViewOffset = Mathf.Clamp(skyboxViewOffset - 1, 0, LevelStudioPlugin.Instance.selectableSkyboxes.Count - skyboxButtons.Length);
                    RefreshSkyboxView();
                    break;
                case "red":
                    ChangeRed(Mathf.Clamp(((Vector3)data).y / 128f, 0f, 1f));
                    EditorController.Instance.RefreshLights();
                    handler.somethingChanged = true;
                    RefreshColorStuff();
                    break;
                case "green":
                    ChangeGreen(Mathf.Clamp(((Vector3)data).y / 128f, 0f, 1f));
                    EditorController.Instance.RefreshLights();
                    handler.somethingChanged = true;
                    RefreshColorStuff();
                    break;
                case "blue":
                    ChangeBlue(Mathf.Clamp(((Vector3)data).y / 128f, 0f, 1f));
                    EditorController.Instance.RefreshLights();
                    handler.somethingChanged = true;
                    RefreshColorStuff();
                    break;
                case "hexDone":
                case "hex":
                    if (ColorUtility.TryParseHtmlString("#" + (hexText.text.PadRight(6, '0')), out Color c))
                    {
                        EditorController.Instance.levelData.minLightColor = c;
                        EditorController.Instance.RefreshLights();
                        handler.somethingChanged = true;
                        RefreshColorStuff(message == "hexDone");
                    }
                    break;
                case "fixLight":
                    EditorController.Instance.levelData.minLightColor = new Color(Mathf.Min(EditorController.Instance.levelData.minLightColor.r, minHideLevel), Mathf.Min(EditorController.Instance.levelData.minLightColor.g, minHideLevel), Mathf.Min(EditorController.Instance.levelData.minLightColor.b, minHideLevel), EditorController.Instance.levelData.minLightColor.a);
                    // clamp it back to 255x, not technically necessary but if we dont some weirdness occurs with the UI since the UI only supports 8 bit color
                    EditorController.Instance.levelData.minLightColor = new Color(Mathf.Floor((EditorController.Instance.levelData.minLightColor.r * 255f)) / 255f, Mathf.Floor((EditorController.Instance.levelData.minLightColor.g * 255f)) / 255f, Mathf.Floor((EditorController.Instance.levelData.minLightColor.b * 255f)) / 255f, EditorController.Instance.levelData.minLightColor.a);
                    EditorController.Instance.RefreshLights();
                    handler.somethingChanged = true;
                    RefreshColorStuff(true);
                    break;
                case "modeAdditive":
                    EditorController.Instance.levelData.lightMode = LightMode.Additive;
                    handler.somethingChanged = true;
                    EditorController.Instance.RefreshLights();
                    RefreshLightmode();
                    break;
                case "modeGreatest":
                    EditorController.Instance.levelData.lightMode = LightMode.Greatest;
                    handler.somethingChanged = true;
                    EditorController.Instance.RefreshLights();
                    RefreshLightmode();
                    break;
                case "modeCumulative":
                    EditorController.Instance.levelData.lightMode = LightMode.Cumulative;
                    handler.somethingChanged = true;
                    EditorController.Instance.RefreshLights();
                    RefreshLightmode();
                    break;
            }
        }
    }
}
