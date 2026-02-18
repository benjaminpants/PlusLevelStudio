using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using MTM101BaldAPI;
using TMPro;

namespace PlusLevelStudio.Editor
{
    public class PlaytimeProperties : NPCProperties
    {
        public byte jumps = 5;
        static FieldInfo _jumpropePre = AccessTools.Field(typeof(Playtime), "jumpropePre");
        static FieldInfo _maxJumps = AccessTools.Field(typeof(Jumprope), "maxJumps");
        public override GameObject[] GeneratePrefabs(NPC baseNpc)
        {
            Playtime ptClone = GameObject.Instantiate<Playtime>((Playtime)baseNpc, MTM101BaldiDevAPI.prefabTransform);
            ptClone.name = "Playtime_Customized";
            Jumprope jumpropeClone = GameObject.Instantiate<Jumprope>((Jumprope)_jumpropePre.GetValue(ptClone), MTM101BaldiDevAPI.prefabTransform);
            jumpropeClone.name = "Jumprope_Customized";
            _maxJumps.SetValue(jumpropeClone, (int)jumps);
            _jumpropePre.SetValue(ptClone, jumpropeClone);
            return new GameObject[2] { ptClone.gameObject, jumpropeClone.gameObject };
        }

        public override bool IsAtDefaultSettings()
        {
            return jumps == 5;
        }

        const byte version = 0;

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            jumps = reader.ReadByte();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(jumps);
        }
    }

    public class PlaytimePropExchangeHandler : NPCPropertyExchangeHandler
    {
        public PlaytimeProperties properProps => (PlaytimeProperties)properties;
        public TextMeshProUGUI jumpBox;
        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            jumpBox = transform.Find("JumpBox").GetComponent<TextMeshProUGUI>();
        }

        public override void SendInteractionMessage(string message, object data)
        {
            if (message == "setJumps")
            {
                if (byte.TryParse((string)data, out byte jumps))
                {
                    properProps.jumps = (byte)Mathf.Clamp(jumps,1,10);
                    propertiesChanged = true;
                }
                jumpBox.text = properProps.jumps.ToString();
                return;
            }
            base.SendInteractionMessage(message, data);
        }

        public override void OnPropertiesAssigned()
        {
            jumpBox.text = properProps.jumps.ToString();
        }
    }
}
