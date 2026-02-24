using HarmonyLib;
using MTM101BaldAPI;
using PlusStudioLevelFormat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class BaldiProperties : NPCProperties
    {
        // remove these indexes when we actually can freely edit the animation curves
        public int speedPreIndex = 0;
        public int slapPreIndex = 0;
        public AnimationCurve speedCurve;
        public AnimationCurve slapCurve;
        static FieldInfo _slapCurve = AccessTools.Field(typeof(Baldi), "slapCurve");
        static FieldInfo _speedCurve = AccessTools.Field(typeof(Baldi), "speedCurve");

        // PLACEHOLDER
        public BaldiProperties()
        {
            speedCurve = CloneAnimationCurve((AnimationCurve)_speedCurve.GetValue(LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis[0]));
            slapCurve = CloneAnimationCurve((AnimationCurve)_slapCurve.GetValue(LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis[0]));
        }

        public void RefreshSpeedPre()
        {
            speedCurve = CloneAnimationCurve((AnimationCurve)_speedCurve.GetValue(LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis[speedPreIndex]));
        }

        public void RefreshSlapPre()
        {
            slapCurve = CloneAnimationCurve((AnimationCurve)_slapCurve.GetValue(LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis[slapPreIndex]));
        }

        public override GameObject[] GeneratePrefabs(NPC baseNpc)
        {
            Baldi baldiPre = GameObject.Instantiate<Baldi>((Baldi)baseNpc, MTM101BaldiDevAPI.prefabTransform);
            baldiPre.name = "Baldi_Customized";
            _slapCurve.SetValue(baldiPre, slapCurve);
            _speedCurve.SetValue(baldiPre, speedCurve);
            return new GameObject[] { baldiPre.gameObject };
        }

        // PLACEHOLDER
        public override bool IsAtDefaultSettings()
        {
            return (speedPreIndex == 0) && (slapPreIndex == 0);
        }

        public override void ReadInto(BinaryReader reader)
        {
            byte version = reader.ReadByte();
            speedPreIndex = reader.ReadInt32();
            slapPreIndex = reader.ReadInt32();
            speedCurve = ReadAnimationCurve(reader);
            slapCurve = ReadAnimationCurve(reader);
        }
        public const byte version = 0;

        public override void Write(BinaryWriter writer)
        {
            writer.Write(version);
            writer.Write(speedPreIndex);
            writer.Write(slapPreIndex);
            WriteAnimationCurve(writer, speedCurve);
            WriteAnimationCurve(writer, slapCurve);
        }

        public AnimationCurve CloneAnimationCurve(AnimationCurve toCopy)
        {
            return new AnimationCurve(toCopy.keys);
        }

        protected void WriteAnimationCurve(BinaryWriter writer, AnimationCurve curve)
        {
            writer.Write((byte)curve.preWrapMode);
            writer.Write((byte)curve.postWrapMode);
            writer.Write(curve.keys.Length);
            for (int i = 0; i < curve.keys.Length; i++)
            {
                writer.Write(curve.keys[i].time);
                writer.Write(curve.keys[i].value);
                writer.Write(curve.keys[i].inWeight);
                writer.Write(curve.keys[i].outWeight);
                writer.Write(curve.keys[i].inTangent);
                writer.Write(curve.keys[i].outTangent);
                writer.Write((byte)curve.keys[i].weightedMode);
            }
        }

        protected AnimationCurve ReadAnimationCurve(BinaryReader reader)
        {
            AnimationCurve curve = new AnimationCurve();
            curve.preWrapMode = (WrapMode)reader.ReadByte();
            curve.postWrapMode = (WrapMode)reader.ReadByte();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                curve.AddKey(new Keyframe()
                {
                    time = reader.ReadSingle(),
                    value = reader.ReadSingle(),
                    inWeight = reader.ReadSingle(),
                    outWeight = reader.ReadSingle(),
                    inTangent = reader.ReadSingle(),
                    outTangent = reader.ReadSingle(),
                    weightedMode = (WeightedMode)reader.ReadByte()
                });
            }
            return curve;
        }
    }

    public class BaldiPropExchangeHandler : NPCPropertyExchangeHandler
    {
        public BaldiProperties properProps => (BaldiProperties)properties;
        public TextMeshProUGUI speedPreText;
        public TextMeshProUGUI slapPreText;
        public override void OnElementsCreated()
        {
            base.OnElementsCreated();
            speedPreText = transform.Find("SpeedCurveFiller").GetComponent<TextMeshProUGUI>();
            slapPreText = transform.Find("SlapCurveFiller").GetComponent<TextMeshProUGUI>();
        }

        public override void SendInteractionMessage(string message, object data)
        {
            switch (message)
            {
                case "speedCurveNext":
                    properProps.speedPreIndex = (properProps.speedPreIndex + 1) % LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis.Count;
                    properProps.RefreshSpeedPre();
                    propertiesChanged = true;
                    OnPropertiesAssigned();
                    return;
                case "speedCurvePrev":
                    properProps.speedPreIndex = (properProps.speedPreIndex - 1);
                    if (properProps.speedPreIndex < 0)
                    {
                        properProps.speedPreIndex = LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis.Count - 1;
                    }
                    properProps.RefreshSpeedPre();
                    propertiesChanged = true;
                    OnPropertiesAssigned();
                    return;
                case "slapCurveNext":
                    properProps.slapPreIndex = (properProps.slapPreIndex + 1) % LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis.Count;
                    properProps.RefreshSlapPre();
                    propertiesChanged = true;
                    OnPropertiesAssigned();
                    return;
                case "slapCurvePrev":
                    properProps.slapPreIndex = (properProps.slapPreIndex - 1);
                    if (properProps.slapPreIndex < 0)
                    {
                        properProps.slapPreIndex = LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis.Count - 1;
                    }
                    properProps.RefreshSlapPre();
                    propertiesChanged = true;
                    OnPropertiesAssigned();
                    return;
            }
            base.SendInteractionMessage(message, data);
        }

        public override void OnPropertiesAssigned()
        {
            speedPreText.text = LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis[properProps.speedPreIndex].name;
            slapPreText.text = LevelStudioPlugin.Instance.animCurvesBaldiPrefabsDoNotAddToThis[properProps.slapPreIndex].name;
        }
    }
}
