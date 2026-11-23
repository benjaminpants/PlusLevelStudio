using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public class VisualsAndLightsUIExchangeHandler : GlobalSettingsUIExchangeHandler
    {
        public static float minHideLevel = 0.05f;

        StandardMenuButton[] skyboxButtons;
        GameObject warningButton;
        Image skyboxImage;
        int skyboxViewOffset = 0;
        ColorPicker colorPicker;

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
            colorPicker = transform.Find("ColorPicker").GetComponent<ColorPicker>();
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
            if (updateHex)
            {
                colorPicker.outputColor = currentColor;
            }
            warningButton.gameObject.SetActive(Mathf.Max(currentColor.r, currentColor.g, currentColor.b) > minHideLevel);
        }

        public void RefreshSkyboxView()
        {
            for (int i = 0; i < skyboxButtons.Length; i++)
            {
                if ((i + skyboxViewOffset) >= (LevelStudioPlugin.Instance.selectableSkyboxes.Count + 1))
                {
                    skyboxButtons[i].image.color = Color.clear;
                    continue;
                }
                if ((i + skyboxViewOffset) == LevelStudioPlugin.Instance.selectableSkyboxes.Count)
                {
                    skyboxButtons[i].image.sprite = LevelStudioPlugin.Instance.skyboxSprites["import_custom"];
                    skyboxButtons[i].image.color = EditorController.Instance.levelData.skyboxColor;
                    continue;
                }
                skyboxButtons[i].image.color = EditorController.Instance.levelData.skyboxColor;
                skyboxButtons[i].image.sprite = LevelStudioPlugin.Instance.skyboxSprites[LevelStudioPlugin.Instance.selectableSkyboxes[i + skyboxViewOffset]];
            }
            skyboxImage.sprite = LevelStudioPlugin.Instance.skyboxSprites[EditorController.Instance.levelData.skybox];
            skyboxImage.color = EditorController.Instance.levelData.skyboxColor;
        }


        string lastUsedFile = "skybox";
        bool OnSkyboxSubmit(string path)
        {
            EditorCustomContentHelpers.GetRelativePathsAndID(LevelStudioPlugin.customSkyboxPath, path, out string relativePath, out string relativePathNoExtension, out string idSuff);
            lastUsedFile = relativePathNoExtension;
            string currentId = "cstm_skybox_" + idSuff;
            if (EditorController.Instance.customContentPackage.entries.Find(x => x.id == currentId) != null)
            {
                EditorController.Instance.levelData.skybox = currentId;
                EditorController.Instance.UpdateSkybox();
                RefreshSkyboxView();
                return true;
            }
            if (EditorController.Instance.customContentPackage.entries.Find(x => x.id == EditorController.Instance.levelData.skybox) != null)
            {
                EditorController.Instance.customContent.GetHandlerFor("skybox").EditorRemoveAllUsing(EditorController.Instance, EditorController.Instance.customContentPackage, EditorController.Instance.levelData.skybox);
            }
            EditorCustomContentEntry entry = new EditorCustomContentEntry("skybox", currentId, relativePath);
            if (EditorController.Instance.customContent.GetHandlerFor("skybox").AddElementOfType(entry))
            {
                EditorController.Instance.levelData.skybox = currentId;
                EditorController.Instance.customContentPackage.entries.Add(entry);
                EditorController.Instance.UpdateSkybox();
                RefreshSkyboxView();
                return true;
            }
            return false;
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            if (message.StartsWith("skybox"))
            {
                int index = int.Parse(message.Replace("skybox", ""));
                int selectableSkyboxIndex = index + skyboxViewOffset;
                if (selectableSkyboxIndex >= (LevelStudioPlugin.Instance.selectableSkyboxes.Count + 1))
                {
                    return;
                }
                if (selectableSkyboxIndex == LevelStudioPlugin.Instance.selectableSkyboxes.Count)
                {
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.customSkyboxPath, lastUsedFile, "png", false, OnSkyboxSubmit);
                    EditorController.Instance.UpdateSkybox();
                    RefreshSkyboxView();
                    return;
                }
                EditorController.Instance.levelData.skybox = LevelStudioPlugin.Instance.selectableSkyboxes[index + skyboxViewOffset];
                EditorController.Instance.UpdateSkybox();
                RefreshSkyboxView();
            }
            switch (message)
            {
                case "nextSkybox":
                    skyboxViewOffset = Mathf.Clamp(skyboxViewOffset + 1, 0, (LevelStudioPlugin.Instance.selectableSkyboxes.Count + 1) - skyboxButtons.Length);
                    RefreshSkyboxView();
                    break;
                case "prevSkybox":
                    skyboxViewOffset = Mathf.Clamp(skyboxViewOffset - 1, 0, (LevelStudioPlugin.Instance.selectableSkyboxes.Count + 1) - skyboxButtons.Length);
                    RefreshSkyboxView();
                    break;
                case "openSkyboxColorMenu":
                    Color oldColor = EditorController.Instance.levelData.skyboxColor;
                    EditorController.Instance.CreateUIColorPickerMenu(EditorController.Instance.levelData.skyboxColor, (Color c) =>
                    {
                        EditorController.Instance.levelData.skyboxColor = c;
                        handler.somethingChanged = c != oldColor;
                        RefreshSkyboxView();
                        EditorController.Instance.UpdateSkybox();
                    }, "Ed_ColorPicker_SkyboxColor");
                    break;
                case "setColor":
                case "setColorDone":
                    EditorController.Instance.levelData.minLightColor = (Color)data;
                    EditorController.Instance.RefreshLights();
                    handler.somethingChanged = true;
                    RefreshColorStuff(message == "setColorDone");
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
