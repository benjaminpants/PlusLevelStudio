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
        public float jumpDelay = 0.7f;
        public float ropeSpeed = 0.6f;
        static FieldInfo _jumpropePre = AccessTools.Field(typeof(Playtime), "jumpropePre");
        static FieldInfo _maxJumps = AccessTools.Field(typeof(Jumprope), "maxJumps");
        static FieldInfo _ropeDelay = AccessTools.Field(typeof(Jumprope), "ropeDelay");
        static FieldInfo _ropeTime = AccessTools.Field(typeof(Jumprope), "ropeTime");
        static FieldInfo _animator = AccessTools.Field(typeof(Jumprope), "animator");
        public override GameObject[] GeneratePrefabs(NPC baseNpc)
        {
            Playtime ptClone = GameObject.Instantiate<Playtime>((Playtime)baseNpc, MTM101BaldiDevAPI.prefabTransform);
            ptClone.name = "Playtime_Customized";
            Jumprope jumpropeClone = GameObject.Instantiate<Jumprope>((Jumprope)_jumpropePre.GetValue(ptClone), MTM101BaldiDevAPI.prefabTransform);
            jumpropeClone.name = "Jumprope_Customized";
            _maxJumps.SetValue(jumpropeClone, (int)jumps);
            _ropeDelay.SetValue(jumpropeClone, jumpDelay);
            _ropeTime.SetValue(jumpropeClone, ropeSpeed);
            ((Animator)_animator.GetValue(jumpropeClone)).speed = 0.6f / ropeSpeed;
            _jumpropePre.SetValue(ptClone, jumpropeClone);
            return new GameObject[2] { ptClone.gameObject, jumpropeClone.gameObject };
        }

        public override bool IsAtDefaultSettings()
        {
            return (jumps == 5) && (jumpDelay == 0.7f) && (ropeSpeed == 0.6f);
        }

        const byte version = 2;

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            jumps = reader.ReadByte();
            if (version < 1) return;
            jumpDelay = reader.ReadSingle();
            if (version < 2) return;
            ropeSpeed = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(jumps);
            writer.Write(jumpDelay);
            writer.Write(ropeSpeed);
        }
    }

    public class PlaytimePropExchangeHandler : NPCPropertyExchangeHandler
    {
        public PlaytimeProperties properProps => (PlaytimeProperties)properties;
        public TextMeshProUGUI jumpBox;
        public TextMeshProUGUI ropeDelayBox;
        public TextMeshProUGUI ropeSpeedBox;
        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            jumpBox = transform.Find("JumpBox").GetComponent<TextMeshProUGUI>();
            ropeDelayBox = transform.Find("JumpDelayBox").GetComponent<TextMeshProUGUI>();
            ropeSpeedBox = transform.Find("RopeSpeedBox").GetComponent<TextMeshProUGUI>();
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
                OnPropertiesAssigned();
                return;
            }
            if (message == "setRopeDelay")
            {
                if (float.TryParse((string)data, out float ropeDelay))
                {
                    properProps.jumpDelay = Mathf.Clamp(ropeDelay,0f,999999f);
                    propertiesChanged = true;
                }
                OnPropertiesAssigned();
                return;
            }
            if (message == "setRopeSpeed")
            {
                if (float.TryParse((string)data, out float ropeDelay))
                {
                    properProps.ropeSpeed = Mathf.Clamp(ropeDelay, 0.001f, 999999f);
                    propertiesChanged = true;
                }
                OnPropertiesAssigned();
                return;
            }
            base.SendInteractionMessage(message, data);
        }

        public override void OnPropertiesAssigned()
        {
            jumpBox.text = properProps.jumps.ToString();
            ropeDelayBox.text = properProps.jumpDelay.ToString();
            ropeSpeedBox.text = properProps.ropeSpeed.ToString();
        }
    }
}
