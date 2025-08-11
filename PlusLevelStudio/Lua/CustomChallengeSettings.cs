using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.GlobalSettingsMenus;
using PlusLevelStudio.Ingame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Lua
{
    public class CustomChallengeGameMode : EditorGameMode
    {
        public override EditorGameModeSettings CreateSettings()
        {
            return new CustomChallengeGameModeSettings();
        }
    }

    public class CustomChallengeSettingsPageUIExchangeHandler : ModeSettingsPageUIExchangeHandler
    {
        TextMeshProUGUI refreshText;
        CustomChallengeGameModeSettings luaSettings => (CustomChallengeGameModeSettings)settings;
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            refreshText = transform.Find("RefreshButton").transform.GetComponent<TextMeshProUGUI>();
        }

        public override void PageLoaded()
        {
            if (!string.IsNullOrEmpty(luaSettings.fileName))
            {
                refreshText.text = String.Format(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_RefreshLua"), luaSettings.fileName + ".lua");
            }
            else
            {
                refreshText.text = "";
            }
        }

        public bool ScriptSelected(string path)
        {
            if (!File.Exists(path)) return false;
            luaSettings.luaScript = File.ReadAllText(path);
            luaSettings.fileName = Path.GetFileNameWithoutExtension(path);
            refreshText.text = String.Format(LocalizationManager.Instance.GetLocalizedText("Ed_Menu_RefreshLua"), luaSettings.fileName + ".lua");
            return true;
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "loadScript":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.luaPath, ((CustomChallengeGameModeSettings)settings).fileName, "lua", ScriptSelected);
                    break;
                case "refreshScript":
                    if (!string.IsNullOrEmpty(luaSettings.fileName))
                    {
                        ScriptSelected(Path.Combine(LevelStudioPlugin.luaPath, luaSettings.fileName + ".lua"));
                    }
                    break;
            }
        }
    }
}
