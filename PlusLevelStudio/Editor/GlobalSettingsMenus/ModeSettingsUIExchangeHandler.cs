using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public class ModeSettingsUIExchangeHandler : GlobalSettingsUIExchangeHandler
    {
        public TextMeshProUGUI titleText;
        public Transform noConfigSettings;
        int currentPage = 0;

        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            titleText = transform.Find("Title").GetComponent<TextMeshProUGUI>();
            noConfigSettings = transform.Find("NoConfigurableSettings");
        }

        public void SwitchToPage(int page)
        {
            currentPage = page;
            string modeId = EditorController.Instance.currentMode.availableGameModes[currentPage];
            EditorGameMode mode = LevelStudioPlugin.Instance.gameModeAliases[modeId];
            titleText.text = LocalizationManager.Instance.GetLocalizedText(mode.nameKey);
            titleText.fontStyle = (modeId == EditorController.Instance.levelData.meta.gameMode) ? FontStyles.Bold : FontStyles.Normal;
            noConfigSettings.gameObject.SetActive(true);
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
