using HarmonyLib;
using MTM101BaldAPI;
using PlusLevelStudio.Ingame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class PrincipalProperties : NPCProperties
    {
        public static Dictionary<int, SoundObject> timeSounds = new Dictionary<int, SoundObject>();

        public List<int> times = new List<int>()
        {
            15,
            20,
            25,
            30,
            35,
            40,
            45,
            50,
            55,
            60,
            99
        };

        private static int[] defaultTimes = new int[]
        {
            15,
            20,
            25,
            30,
            35,
            40,
            45,
            50,
            55,
            60,
            99
        };

        public bool allKnowing = false;

        protected virtual SoundObject GetSoundFromTime(int time)
        {
            return timeSounds[time];
        }


        static FieldInfo _allKnowing = AccessTools.Field(typeof(Principal), "allKnowing");
        public override GameObject[] GeneratePrefabs(NPC baseNpc)
        {
            Principal priClone = GameObject.Instantiate<Principal>((Principal)baseNpc, MTM101BaldiDevAPI.prefabTransform);
            priClone.name = "Principal_Customized";
            if (!TimesAreDefault())
            {
                EditorPrincipalCustomization customizer = priClone.gameObject.AddComponent<EditorPrincipalCustomization>();
                customizer.detentionTimes = times.ToArray();
                customizer.detentionSounds = new SoundObject[times.Count];
                for (int i = 0; i < times.Count; i++)
                {
                    customizer.detentionSounds[i] = GetSoundFromTime(times[i]);
                }
            }
            _allKnowing.SetValue(priClone, allKnowing);
            return new GameObject[] { priClone.gameObject };
        }

        public virtual bool TimesAreDefault()
        {
            if (times.Count != defaultTimes.Length) return false;
            for (int i = 0; i < defaultTimes.Length; i++)
            {
                if (times[i] != defaultTimes[i]) return false;
            }
            return true;
        }

        public override bool IsAtDefaultSettings()
        {
            return TimesAreDefault() && !allKnowing;
        }

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            if (version > 0)
            {
                allKnowing = reader.ReadBoolean();
            }
            int count = reader.ReadInt32();
            times.Clear();
            for (int i = 0; i < count; i++)
            {
                times.Add(reader.ReadByte());
            }
        }

        const byte version = 1;

        public virtual int SnapValue(int time)
        {
            int closestIndex = -1;
            int closestValue = int.MaxValue;
            for (int i = 0; i < defaultTimes.Length; i++)
            {
                if (Mathf.Abs(defaultTimes[i] - time) < closestValue)
                {
                    closestIndex = i;
                    closestValue = Mathf.Abs(defaultTimes[i] - time);
                }
            }
            return defaultTimes[closestIndex];
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(allKnowing);
            writer.Write(times.Count);
            for (int i = 0; i < times.Count; i++)
            {
                writer.Write((byte)times[i]);
            }
        }
    }
    public class PrincipalPropExchangeHandler : NPCPropertyExchangeHandler
    {
        public PrincipalProperties properProps => (PrincipalProperties)properties;
        TextMeshProUGUI[] texts = new TextMeshProUGUI[11];
        MenuToggle toggle;
        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i] = transform.Find("TimeBox" + i).GetComponent<TextMeshProUGUI>();
            }
            toggle = transform.Find("allKnowingCheck").GetComponent<MenuToggle>();
        }

        public override void SendInteractionMessage(string message, object data)
        {
            if (message.StartsWith("setTime"))
            {
                int index = int.Parse(message.Remove(0,7));
                if (int.TryParse((string)data, out int res))
                {
                    propertiesChanged = true;
                    if (index >= properProps.times.Count)
                    {
                        properProps.times.Add(-1);
                        index = properProps.times.Count - 1;
                    }
                    if (res == 0)
                    {
                        properProps.times.RemoveAt(index);
                    }
                    else
                    {
                        res = properProps.SnapValue(res);
                        properProps.times[index] = res;
                    }
                }
                OnPropertiesAssigned();
                return;
            }
            if (message == "toggleAllKnowing")
            {
                properProps.allKnowing = !properProps.allKnowing;
                propertiesChanged = true;
                OnPropertiesAssigned();
            }
            base.SendInteractionMessage(message, data);
        }

        public override void OnPropertiesAssigned()
        {
            for (int i = 0; i < texts.Length; i++)
            {
                if (i >= properProps.times.Count)
                {
                    texts[i].text = "0";
                    continue;
                }
                texts[i].text = properProps.times[i].ToString();
            }
            toggle.Set(properProps.allKnowing);
            //timeBox.text = properProps.time.ToString();
        }
    }
}
