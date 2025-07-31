using PlusLevelStudio.Editor.GlobalSettingsMenus;
using PlusLevelStudio.Ingame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.ModeSettings
{
    public class MainModeSettings : EditorGameModeSettings
    {
        public bool baldiSpawnAtHappy = true;
        const byte version = 0;
        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            baldiSpawnAtHappy = reader.ReadBoolean();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(baldiSpawnAtHappy);
        }

        public override void ApplySettingsToManager(BaseGameManager manager)
        {
            ((EditorMainGameManager)manager).baldiGoesToHappyBaldi = baldiSpawnAtHappy;
        }
    }

    public class MainGameMode : EditorGameMode
    {
        public override EditorGameModeSettings CreateSettings()
        {
            return new MainModeSettings();
        }
    }

    public class MainModeSettingsPageUIExchangeHandler : ModeSettingsPageUIExchangeHandler
    {
        MenuToggle toggle;
        public override bool GetStateBoolean(string key)
        {
            return false;
        }

        public override void OnElementsCreated()
        {
            toggle = transform.Find("Checkbox").GetComponent<MenuToggle>();
        }

        public override void PageLoaded()
        {
            toggle.Set(((MainModeSettings)settings).baldiSpawnAtHappy);
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "toggleBaldi":
                    ((MainModeSettings)settings).baldiSpawnAtHappy = (bool)data;
                    break;
            }
        }
    }
}
