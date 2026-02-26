using MTM101BaldAPI;
using PlusLevelStudio.Ingame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        protected virtual SoundObject GetSoundFromTime(int time)
        {
            return timeSounds[time];
        }

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
            return new GameObject[] { priClone.gameObject };
        }

        public virtual bool TimesAreDefault()
        {
            if (times.Count != 11) return false;
            for (int i = 0; i < defaultTimes.Length; i++)
            {
                if (times[i] != defaultTimes[i]) return false;
            }
            return true;
        }

        public override bool IsAtDefaultSettings()
        {
            return TimesAreDefault();
        }

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            int count = reader.ReadInt32();
            times.Clear();
            for (int i = 0; i < count; i++)
            {
                times.Add(reader.ReadByte());
            }
        }

        const byte version = 0;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(times.Count);
            for (int i = 0; i < times.Count; i++)
            {
                writer.Write((byte)times[i]);
            }
        }
    }
}
