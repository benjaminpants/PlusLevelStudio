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
    public class ChalklesProperties : NPCProperties
    {
        public float classSpawnPercent = 70f;
        public float facultySpawnPercent = 20f;
        public float lockTime = 15f;

        static FieldInfo _classSpawnPercent = AccessTools.Field(typeof(ChalkFace), "classSpawnPercent");
        static FieldInfo _facultySpawnPercent = AccessTools.Field(typeof(ChalkFace), "facultySpawnPercent");
        static FieldInfo _lockTime = AccessTools.Field(typeof(ChalkFace), "lockTime");
        public override GameObject[] GeneratePrefabs(NPC baseNpc)
        {
            ChalkFace chalklesPre = GameObject.Instantiate<ChalkFace>((ChalkFace)baseNpc, MTM101BaldiDevAPI.prefabTransform);
            chalklesPre.name = "Chalkles_Customized";
            _classSpawnPercent.SetValue(chalklesPre, classSpawnPercent);
            _facultySpawnPercent.SetValue(chalklesPre, facultySpawnPercent);
            _lockTime.SetValue(chalklesPre, lockTime);
            return new GameObject[] { chalklesPre.gameObject };
        }

        public override bool IsAtDefaultSettings()
        {
            return (classSpawnPercent == 70f) && (facultySpawnPercent == 20f) && (lockTime == 15f);
        }

        public const byte version = 0;

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            classSpawnPercent = reader.ReadSingle();
            facultySpawnPercent = reader.ReadSingle();
            lockTime = reader.ReadSingle();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(classSpawnPercent);
            writer.Write(facultySpawnPercent);
            writer.Write(lockTime);
        }
    }

    public class ChalklesPropExchangeHandler : NPCPropertyExchangeHandler
    {
        public ChalklesProperties properProps => (ChalklesProperties)properties;
        public TextMeshProUGUI classChanceBox;
        public TextMeshProUGUI facultyChanceBox;
        public TextMeshProUGUI lockTimeBox;
        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            classChanceBox = transform.Find("ClassChanceBox").GetComponent<TextMeshProUGUI>();
            facultyChanceBox = transform.Find("FacultyChanceBox").GetComponent<TextMeshProUGUI>();
            lockTimeBox = transform.Find("LocktimeBox").GetComponent<TextMeshProUGUI>();
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "setClassChance":
                    if (float.TryParse((string)data, out float classChance))
                    {
                        properProps.classSpawnPercent = Mathf.Clamp(classChance, 0f, 100f);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
                case "setFacultyChance":
                    if (float.TryParse((string)data, out float facultyChance))
                    {
                        properProps.facultySpawnPercent = Mathf.Clamp(facultyChance, 0f, 100f);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
                case "setLockTime":
                    if (float.TryParse((string)data, out float lockTime))
                    {
                        properProps.lockTime = Mathf.Clamp(lockTime, 1f, 99999999f);
                        propertiesChanged = true;
                    }
                    OnPropertiesAssigned();
                    return;
            }
            base.SendInteractionMessage(message, data);
        }

        public override void OnPropertiesAssigned()
        {
            classChanceBox.text = properProps.classSpawnPercent.ToString();
            facultyChanceBox.text = properProps.facultySpawnPercent.ToString();
            lockTimeBox.text = properProps.lockTime.ToString();
        }
    }
}
