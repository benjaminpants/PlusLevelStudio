using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public abstract class ModeSettingsPageUIExchangeHandler : UIExchangeHandler
    {
        public EditorGameModeSettings settings;
        public abstract void PageLoaded();
        public virtual void AssignSettings(EditorGameModeSettings settings)
        {
            this.settings = settings;
        }
    }
    public class ModeSettingsUIExchangeHandler : GlobalSettingsUIExchangeHandler
    {
        public TextMeshProUGUI titleText;
        public Transform noConfigSettings;
        public ModeSettingsPageUIExchangeHandler[] pages;
        public EditorGameModeSettings currentFalseSettings;
        int currentPage = 0;

        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            titleText = transform.Find("Title").GetComponent<TextMeshProUGUI>();
            noConfigSettings = transform.Find("NoConfigurableSettings");
            pages = new ModeSettingsPageUIExchangeHandler[EditorController.Instance.currentMode.availableGameModes.Count];
            for (int i = 0; i < EditorController.Instance.currentMode.availableGameModes.Count; i++)
            {
                EditorGameMode mode = LevelStudioPlugin.Instance.gameModeAliases[EditorController.Instance.currentMode.availableGameModes[i]];
                if (!mode.hasSettingsPage)
                {
                    pages[i] = null;
                    continue;
                }
                ModeSettingsPageUIExchangeHandler pageHand = (ModeSettingsPageUIExchangeHandler)UIBuilder.BuildUIFromFile(mode.settingsPageType, transform.GetComponent<RectTransform>(), EditorController.Instance.currentMode.availableGameModes[i] + "_Page", mode.settingsPagePath);
                pages[i] = pageHand;
                pageHand.gameObject.SetActive(false);
            }
        }

        public void SwitchToPage(int page)
        {
            // TODO: revise
            noConfigSettings.gameObject.SetActive(false);
            for (int i = 0; i < pages.Length; i++)
            {
                if (pages[i] == null) continue;
                pages[i].settings = null;
                pages[i].gameObject.SetActive(false);
            }
            currentPage = page;
            string modeId = EditorController.Instance.currentMode.availableGameModes[currentPage];
            EditorGameMode mode = LevelStudioPlugin.Instance.gameModeAliases[modeId];
            titleText.text = LocalizationManager.Instance.GetLocalizedText(mode.nameKey);
            titleText.fontStyle = (modeId == EditorController.Instance.levelData.meta.gameMode) ? FontStyles.Bold : FontStyles.Normal;
            if (pages[currentPage] == null)
            {
                noConfigSettings.gameObject.SetActive(true);
                currentFalseSettings = null;
            }
            else
            {
                if (EditorController.Instance.levelData.meta.gameMode == modeId)
                {
                    currentFalseSettings = EditorController.Instance.levelData.meta.modeSettings.MakeCopy();
                }
                else
                {
                    currentFalseSettings = mode.CreateSettings();
                }
                pages[currentPage].gameObject.SetActive(true);
                pages[currentPage].AssignSettings(currentFalseSettings);
                pages[currentPage].PageLoaded();
            }
        }

        public override void Refresh()
        {
            currentPage = EditorController.Instance.currentMode.availableGameModes.IndexOf(EditorController.Instance.levelData.meta.gameMode);
            SwitchToPage(currentPage);
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "applyMode":
                    EditorController.Instance.levelData.meta.gameMode = EditorController.Instance.currentMode.availableGameModes[currentPage];
                    if (currentFalseSettings != null)
                    {
                        EditorController.Instance.levelData.meta.modeSettings = currentFalseSettings.MakeCopy();
                    }
                    else
                    {
                        EditorController.Instance.levelData.meta.modeSettings = null;
                    }
                    handler.somethingChanged = true;
                    SwitchToPage(currentPage);
                    break;
                case "prevPage":
                    SwitchToPage(Mathf.Max(currentPage - 1, 0));
                    break;
                case "nextPage":
                    SwitchToPage(Mathf.Min(currentPage + 1, EditorController.Instance.currentMode.availableGameModes.Count - 1));
                    break;
            }
        }
    }
}
