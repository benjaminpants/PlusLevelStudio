using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace PlusStudioLevelLoader.Patches
{
    [HarmonyPatch(typeof(LevelLoader))]
    [HarmonyPatch("Load")]
    [HarmonyPatch(MethodType.Enumerator)]
    public class LevelLoaderTranspilers
    {

        // THANK YOU PIXELGUY!!
        readonly static Type genEnum = AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(LevelLoader), "Load")).DeclaringType;

        static FieldInfo _level = AccessTools.Field(genEnum, "data");

        static Dictionary<string, string> actions = new Dictionary<string, string>()
        {
            { "Level loader setting up events.", "AddTimeOut" }
        };

        public static void AddTimeOut(LevelLoader loader, LevelData data)
        {
            if (!(data.extraData is ExtendedExtraLevelData)) return;
            Debug.Log("Level loader setting up limit... (Loader Extension)");
            if (((ExtendedExtraLevelData)data.extraData).timeOutEvent == null) return;
            if (((ExtendedExtraLevelData)data.extraData).timeOutTime <= 0f) return;
            RandomEvent timeoutEvent = GameObject.Instantiate<RandomEvent>(((ExtendedExtraLevelData)data.extraData).timeOutEvent, loader.Ec.transform);
            timeoutEvent.Initialize(loader.Ec, loader.controlledRNG);
            timeoutEvent.PremadeSetup();
            loader.Ec.AddEvent(timeoutEvent, ((ExtendedExtraLevelData)data.extraData).timeOutTime);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug.Log("Level Loader Transpile Begin!");
            MethodInfo logMethod = AccessTools.Method(typeof(UnityEngine.Debug), "Log", new Type[] { typeof(object) });
            int offset = -1;
            CodeInstruction[] instructionsArray = instructions.ToArray();
            for (int i = 0; i < instructionsArray.Length; i++)
            {
                if ((i + offset) < 0)
                {
                    yield return instructionsArray[i];
                    continue;
                }
                if (instructionsArray[i + offset].opcode == OpCodes.Ldstr
                    && (instructionsArray[i + offset + 1].opcode == OpCodes.Call && (MethodInfo)instructionsArray[i + offset + 1].operand == logMethod)
                    )
                {
                    if (actions.ContainsKey((string)instructionsArray[i + offset].operand))
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_2); //this
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, _level);
                        string key = actions[(string)instructionsArray[i + offset].operand];
                        MethodInfo method = AccessTools.Method(typeof(LevelLoaderTranspilers), key);
                        yield return new CodeInstruction(OpCodes.Call, method);
                        Debug.Log("Patched " + key + "!");
                    }
                }
                yield return instructionsArray[i];
            }
            yield break;
        }
    }
}
