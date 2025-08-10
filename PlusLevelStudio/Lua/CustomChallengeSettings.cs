using PlusLevelStudio.Editor;
using PlusLevelStudio.Editor.GlobalSettingsMenus;
using PlusLevelStudio.Ingame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            
        }

        public override void PageLoaded()
        {
            
        }

        public bool ScriptSelected(string path)
        {
            if (!File.Exists(path)) return false;
            ((CustomChallengeGameModeSettings)settings).luaScript = File.ReadAllText(path);
            ((CustomChallengeGameModeSettings)settings).fileName = Path.GetFileNameWithoutExtension(path);
            return true;
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "loadScript":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.luaPath, ((CustomChallengeGameModeSettings)settings).fileName, "lua", ScriptSelected);
                    break;
            }
        }
    }
}
