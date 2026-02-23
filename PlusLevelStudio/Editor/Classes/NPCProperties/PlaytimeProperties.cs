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
    public class AnimatorChangeSpeedComponent : MonoBehaviour
    {
        public Animator anim;
        public float speedToChange = 1f;

        void Awake()
        {
            anim.speed = speedToChange;
        }
    }

    public class PlaytimeProperties : NPCProperties
    {
        public byte jumps = 5;
        public float jumpDelay = 0.7f;
        public float ropeSpeed = 0.6f;
        public float jumpSpeed = 1f;
        static FieldInfo _jumpropePre = AccessTools.Field(typeof(Playtime), "jumpropePre");
        static FieldInfo _maxJumps = AccessTools.Field(typeof(Jumprope), "maxJumps");
        static FieldInfo _ropeDelay = AccessTools.Field(typeof(Jumprope), "ropeDelay");
        static FieldInfo _ropeTime = AccessTools.Field(typeof(Jumprope), "ropeTime");
        static FieldInfo _animator = AccessTools.Field(typeof(Jumprope), "animator");
        static FieldInfo _initVelocity = AccessTools.Field(typeof(Jumprope), "initVelocity");
        static FieldInfo _accel = AccessTools.Field(typeof(Jumprope), "accel");
        public override GameObject[] GeneratePrefabs(NPC baseNpc)
        {
            Playtime ptClone = GameObject.Instantiate<Playtime>((Playtime)baseNpc, MTM101BaldiDevAPI.prefabTransform);
            ptClone.name = "Playtime_Customized";
            Jumprope jumpropeClone = GameObject.Instantiate<Jumprope>((Jumprope)_jumpropePre.GetValue(ptClone), MTM101BaldiDevAPI.prefabTransform);
            jumpropeClone.name = "Jumprope_Customized";
            _maxJumps.SetValue(jumpropeClone, (int)jumps);
            _ropeDelay.SetValue(jumpropeClone, jumpDelay);
            _ropeTime.SetValue(jumpropeClone, ropeSpeed);
            AnimatorChangeSpeedComponent changeSpeed = jumpropeClone.gameObject.AddComponent<AnimatorChangeSpeedComponent>();
            changeSpeed.speedToChange = 0.6f / ropeSpeed;
            changeSpeed.anim = (Animator)_animator.GetValue(jumpropeClone);

            if (jumpSpeed != 1f)
            {
                float height = 3.0476f;
                float gravity = ((float)_accel.GetValue(jumpropeClone)) * jumpSpeed;
                _initVelocity.SetValue(jumpropeClone, Mathf.Sqrt(2f * Mathf.Abs(gravity) * height));
                _accel.SetValue(jumpropeClone, gravity);
            }

            _jumpropePre.SetValue(ptClone, jumpropeClone);
            return new GameObject[2] { ptClone.gameObject, jumpropeClone.gameObject };
        }

        public override bool IsAtDefaultSettings()
        {
            return (jumps == 5) && (jumpDelay == 0.7f) && (ropeSpeed == 0.6f) && (jumpSpeed == 1f);
        }

        const byte version = 3;

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            jumps = reader.ReadByte();
            if (version < 1) return;
            jumpDelay = reader.ReadSingle();
            if (version < 2) return;
            ropeSpeed = reader.ReadSingle();
            if (version < 3) return;
            jumpSpeed = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(jumps);
            writer.Write(jumpDelay);
            writer.Write(ropeSpeed);
            writer.Write(jumpSpeed);
        }
    }

    public class PlaytimePropExchangeHandler : NPCPropertyExchangeHandler
    {
        public PlaytimeProperties properProps => (PlaytimeProperties)properties;
        public TextMeshProUGUI jumpBox;
        public TextMeshProUGUI ropeDelayBox;
        public TextMeshProUGUI ropeSpeedBox;
        public TextMeshProUGUI jumpSpeedBox;
        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            jumpBox = transform.Find("JumpBox").GetComponent<TextMeshProUGUI>();
            ropeDelayBox = transform.Find("JumpDelayBox").GetComponent<TextMeshProUGUI>();
            ropeSpeedBox = transform.Find("RopeSpeedBox").GetComponent<TextMeshProUGUI>();
            jumpSpeedBox = transform.Find("JumpSpeedBox").GetComponent<TextMeshProUGUI>();
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "setJumps":
                    if (byte.TryParse((string)data, out byte jumps))
                    {
                        properProps.jumps = (byte)Mathf.Clamp(jumps, 1, 10);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
                case "setRopeDelay":
                    if (float.TryParse((string)data, out float ropeDelay))
                    {
                        properProps.jumpDelay = Mathf.Clamp(ropeDelay, 0f, 999999f);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
                case "setRopeSpeed":
                    if (float.TryParse((string)data, out float ropeSpeed))
                    {
                        properProps.ropeSpeed = Mathf.Clamp(ropeSpeed, 0.001f, 999999f);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
                case "setJumpSpeed":
                    if (float.TryParse((string)data, out float jumpSpeed))
                    {
                        properProps.jumpSpeed = Mathf.Clamp(jumpSpeed, 0.001f, 999999f);
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
            jumpSpeedBox.text = properProps.jumpSpeed.ToString();
        }
    }
}
