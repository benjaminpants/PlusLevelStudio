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
    public class PompProperties : NPCProperties
    {
        public byte time = 2;

        static FieldInfo _classTime = AccessTools.Field(typeof(NoLateTeacher), "classTime");
        public override GameObject[] GeneratePrefabs(NPC baseNpc)
        {
            NoLateTeacher pmpClone = GameObject.Instantiate<NoLateTeacher>((NoLateTeacher)baseNpc, MTM101BaldiDevAPI.prefabTransform);
            pmpClone.name = "Pomp_Customized";
            _classTime.SetValue(pmpClone, time * 60f);
            return new GameObject[] { pmpClone.gameObject };
        }

        public override bool IsAtDefaultSettings()
        {
            return time == 2;
        }

        const byte version = 0;

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            time = reader.ReadByte();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(time);
        }
    }

    public class PompPropExchangeHandler : NPCPropertyExchangeHandler
    {
        public PompProperties properProps => (PompProperties)properties;
        public TextMeshProUGUI timeBox;
        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            timeBox = transform.Find("TimeBox").GetComponent<TextMeshProUGUI>();
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "setTime":
                    if (byte.TryParse((string)data, out byte time))
                    {
                        properProps.time = (byte)Mathf.Clamp(time, 1, 9);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
            }
            base.SendInteractionMessage(message, data);
        }

        public override void OnPropertiesAssigned()
        {
            timeBox.text = properProps.time.ToString();
        }
    }
}
