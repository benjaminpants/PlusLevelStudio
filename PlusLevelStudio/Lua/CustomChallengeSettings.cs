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

        public bool ScriptSelected(string fileName)
        {
            string path = Path.Combine(LevelStudioPlugin.luaPath, fileName + ".lua");
            Debug.Log(path);
            if (!File.Exists(path)) return false;
            ((CustomChallengeGameModeSettings)settings).luaScript = File.ReadAllText(path);
            return false;
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "loadScript":
                    EditorController.Instance.CreateUIFileBrowser(LevelStudioPlugin.luaPath, "lua", ScriptSelected);
                    break;
            }
        }
    }
}
