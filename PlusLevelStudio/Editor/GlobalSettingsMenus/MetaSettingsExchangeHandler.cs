using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;

namespace PlusLevelStudio.Editor.GlobalSettingsMenus
{
    public class MetaSettingsExchangeHandler : GlobalSettingsUIExchangeHandler
    {
        TextMeshProUGUI titleText;
        TextMeshProUGUI authorText;
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            titleText = transform.Find("LevelTitle").GetComponent<TextMeshProUGUI>();
            authorText = transform.Find("AuthorName").GetComponent<TextMeshProUGUI>();
        }

        public override void Refresh()
        {
            titleText.text = EditorController.Instance.levelData.meta.name;
            authorText.text = EditorController.Instance.levelData.meta.author;
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "titleChanged":
                    EditorController.Instance.levelData.meta.name = (string)data;
                    handler.somethingChanged = true;
                    break;
                case "authorChanged":
                    EditorController.Instance.levelData.meta.author = (string)data;
                    handler.somethingChanged = true;
                    break;
            }
        }
    }
}
