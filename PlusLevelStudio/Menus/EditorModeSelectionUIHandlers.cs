using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;

namespace PlusLevelStudio.Menus
{
    public class EditorOrPlaySelectionUIHandler : UIExchangeHandler
    {
        public EditorModeSelectionMenu menu;
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "exit":
                    menu.gameObject.SetActive(false);
                    menu.mainMenu.SetActive(true);
                    menu.playScreenManager.SetFileWatcherStatus(false);
                    break;
                case "play":
                    menu.playParent.SetActive(true);
                    menu.playScreenManager.UpdateFromFolder();
                    menu.playScreenManager.SetFileWatcherStatus(true);
                    menu.playOrEditParent.SetActive(false);
                    break;
                case "edit":
                    menu.editorTypeParent.SetActive(true);
                    menu.playOrEditParent.SetActive(false);
                    break;
            }
        }

    }

    public class EditorModeSelectionUIHandler : UIExchangeHandler
    {
        public EditorModeSelectionMenu menu;
        public TextMeshProUGUI descText;
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            descText = transform.Find("DescText").GetComponent<TextMeshProUGUI>();
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            if (message.StartsWith("edHigh_"))
            {
                string targText = message.Substring(7);
                descText.text = LocalizationManager.Instance.GetLocalizedText(targText);
                return;
            }
            if (message.StartsWith("edMode_"))
            {
                string targMode = message.Substring(7);
                LevelStudioPlugin.Instance.GoToEditor(targMode);
                return;
            }
            if (message == "exit")
            {
                menu.playOrEditParent.SetActive(true);
                menu.editorTypeParent.SetActive(false);
            }
        }

    }
}
