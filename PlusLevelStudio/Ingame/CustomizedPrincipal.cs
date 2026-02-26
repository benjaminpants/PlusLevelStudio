using HarmonyLib;
using PlusLevelStudio.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Ingame
{
    public class EditorPrincipalCustomization : MonoBehaviour
    {
        public int[] detentionTimes;
        public SoundObject[] detentionSounds;

        public void GetValues(ref int detentionLevel, out float detentionInit, out SoundObject[] sounds)
        {
            if (detentionLevel >= detentionTimes.Length)
            {
                detentionLevel--;
            }
            detentionInit = detentionTimes[detentionLevel];
            SoundObject targetSound = detentionSounds[detentionLevel];
            sounds = new SoundObject[detentionTimes.Length + 1];
            for (int i = 0; i < detentionTimes.Length; i++)
            {
                sounds[i] = targetSound;
            }
        }
    }
}

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(Principal))]
    [HarmonyPatch("SendToDetention")]
    class PrincipalDetentionPatch
    {
        static void Prefix(Principal __instance, ref float ___detentionInc, ref float ___detentionInit, ref int ___detentionLevel, ref SoundObject[] ___audTimes)
        {
            if (!__instance.TryGetComponent<EditorPrincipalCustomization>(out EditorPrincipalCustomization custom))
            {
                return;
            }
            ___detentionInc = 0f;
            custom.GetValues(ref ___detentionLevel, out ___detentionInit, out ___audTimes);
        }
    }
}