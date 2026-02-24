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
    public class BullyProperties : NPCProperties
    {
        public float minDelay = 60f;
        public float maxDelay = 120f;
        public float maxStay = 120f;

        static FieldInfo _minDelay = AccessTools.Field(typeof(Bully), "minDelay");
        static FieldInfo _maxDelay = AccessTools.Field(typeof(Bully), "maxDelay");
        static FieldInfo _maxStay = AccessTools.Field(typeof(Bully), "maxStay");
        public override GameObject[] GeneratePrefabs(NPC baseNpc)
        {
            Bully bullyPre = GameObject.Instantiate<Bully>((Bully)baseNpc, MTM101BaldiDevAPI.prefabTransform);
            bullyPre.name = "Bully_Customized";
            _minDelay.SetValue(bullyPre, minDelay);
            _maxDelay.SetValue(bullyPre, maxDelay);
            _maxStay.SetValue(bullyPre, maxStay);
            return new GameObject[] { bullyPre.gameObject };
        }

        public override bool IsAtDefaultSettings()
        {
            return (minDelay == 60f) && (maxDelay == 120f) && (maxStay == 120f);
        }

        public const byte version = 0;

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            minDelay = reader.ReadSingle();
            maxDelay = reader.ReadSingle();
            maxStay = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(minDelay);
            writer.Write(maxDelay);
            writer.Write(maxStay);
        }
    }

    public class BullyPropExchangeHandler : NPCPropertyExchangeHandler
    {
        public BullyProperties properProps => (BullyProperties)properties;
        public TextMeshProUGUI minDelayBox;
        public TextMeshProUGUI maxDelayBox;
        public TextMeshProUGUI maxStayBox;
        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            minDelayBox = transform.Find("MinDelayBox").GetComponent<TextMeshProUGUI>();
            maxDelayBox = transform.Find("MaxDelayBox").GetComponent<TextMeshProUGUI>();
            maxStayBox = transform.Find("StayBox").GetComponent<TextMeshProUGUI>();
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
                case "setStayTime":
                    if (float.TryParse((string)data, out float maxA))
                    {
                        properProps.maxStay = Mathf.Clamp(maxA, 0.001f, 999999f);
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
            maxStayBox.text = properProps.maxStay.ToString();
        }
    }
}
