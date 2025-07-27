using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public class VisualsAndLightsUIExchangeHandler : GlobalSettingsUIExchangeHandler
    {

        StandardMenuButton[] skyboxButtons;
        Image skyboxImage;
        int skyboxViewOffset = 0;
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
        }

        public override void Refresh()
        {
            RefreshSkyboxView();
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
            }
        }
    }
}
