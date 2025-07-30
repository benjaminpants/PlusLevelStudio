using PlusLevelStudio.Editor.GlobalSettingsMenus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor.ModeSettings
{
    public class StealthyModeSettings : EditorGameModeSettings
    {
        public bool giveChalkErasers = true;
        const byte version = 0;
        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            giveChalkErasers = reader.ReadBoolean();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(giveChalkErasers);
        }
    }

    public class StealthyGameMode : EditorGameMode
    {
        public override EditorGameModeSettings CreateSettings()
        {
            return new StealthyModeSettings();
        }
    }

    public class StealthyChallengeSettingsPageUIExchangeHandler : ModeSettingsPageUIExchangeHandler
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
            toggle.Set(((StealthyModeSettings)settings).giveChalkErasers);
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            switch (message)
            {
                case "toggleItems":
                    ((StealthyModeSettings)settings).giveChalkErasers = (bool)data;
                    break;
            }
        }
    }
}
