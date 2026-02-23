using HarmonyLib;
using MTM101BaldAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class GottaSweepProperties : NPCProperties
    {
        public float minDelay = 60f;
        public float maxDelay = 180f;
        public float minActive = 45f;
        public float maxActive = 75f;

        static FieldInfo _minDelay = AccessTools.Field(typeof(GottaSweep), "minDelay");
        static FieldInfo _maxDelay = AccessTools.Field(typeof(GottaSweep), "maxDelay");
        static FieldInfo _minActive = AccessTools.Field(typeof(GottaSweep), "minActive");
        static FieldInfo _maxActive = AccessTools.Field(typeof(GottaSweep), "maxActive");
        public override GameObject[] GeneratePrefabs(NPC baseNpc)
        {
            GottaSweep sweepPre = GameObject.Instantiate<GottaSweep>((GottaSweep)baseNpc, MTM101BaldiDevAPI.prefabTransform);
            sweepPre.name = "GottaSweep_Customized";
            _minDelay.SetValue(sweepPre, minDelay);
            _maxDelay.SetValue(sweepPre, maxDelay);
            _minActive.SetValue(sweepPre, minActive);
            _maxActive.SetValue(sweepPre, maxActive);
            return new GameObject[] { sweepPre.gameObject };
        }

        public override bool IsAtDefaultSettings()
        {
            return (minDelay == 60f) && (maxDelay == 180f) && (minActive == 45f) && (maxActive == 75f);
        }

        public const byte version = 0;

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            minDelay = reader.ReadSingle();
            maxDelay = reader.ReadSingle();
            minActive = reader.ReadSingle();
            maxActive = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(minDelay);
            writer.Write(maxDelay);
            writer.Write(minActive);
            writer.Write(maxActive);
        }
    }

    public class GottaSweepPropExchangeHandler : NPCPropertyExchangeHandler
    {
        public GottaSweepProperties properProps => (GottaSweepProperties)properties;
        public TextMeshProUGUI minDelayBox;
        public TextMeshProUGUI maxDelayBox;
        public TextMeshProUGUI minActiveBox;
        public TextMeshProUGUI maxActiveBox;
        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            minDelayBox = transform.Find("MinDelayBox").GetComponent<TextMeshProUGUI>();
            maxDelayBox = transform.Find("MaxDelayBox").GetComponent<TextMeshProUGUI>();
            minActiveBox = transform.Find("MinActiveBox").GetComponent<TextMeshProUGUI>();
            maxActiveBox = transform.Find("MaxActiveBox").GetComponent<TextMeshProUGUI>();
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "setMinDelay":
                    if (float.TryParse((string)data, out float minD))
                    {
                        properProps.minDelay = Mathf.Clamp(minD, 0f, 999998f);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
                case "setMaxDelay":
                    if (float.TryParse((string)data, out float maxD))
                    {
                        properProps.maxDelay = Mathf.Clamp(maxD, 0.001f, 999999f);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
                case "setMinActive":
                    if (float.TryParse((string)data, out float minA))
                    {
                        properProps.minActive = Mathf.Clamp(minA, 0f, 999998f);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
                case "setMaxActive":
                    if (float.TryParse((string)data, out float maxA))
                    {
                        properProps.maxActive = Mathf.Clamp(maxA, 0.001f, 999999f);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
            }
            base.SendInteractionMessage(message, data);
        }

        public override void OnPropertiesAssigned()
        {
            minDelayBox.text = properProps.minDelay.ToString();
            maxDelayBox.text = properProps.maxDelay.ToString();
            minActiveBox.text = properProps.minActive.ToString();
            maxActiveBox.text = properProps.maxActive.ToString();
        }
    }
}
