using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine;
using PlusLevelStudio.Editor;

namespace PlusLevelStudio.Patches
{
    [HarmonyPatch(typeof(CursorController))]
    [HarmonyPatch("Update")]
    class CursorPatch
    {
        static MethodInfo changeV = AccessTools.Method(typeof(CursorPatch), "ChangeValue");
        static FieldInfo pointerEvent = AccessTools.Field(typeof(CursorController), "pointerEventData");
        static void ChangeValue(PointerEventData data) //change the data because the cursor script assumes the canvas and screen match 1 to 1, which is the case in menus but not in the actual game camera
        {
            if (Singleton<EditorController>.Instance != null)
            {
                data.position = CursorController.Instance.LocalPosition;
                Vector3 pos = new Vector3((data.position.x / Singleton<EditorController>.Instance.screenSize.x) * Screen.width, Screen.height + ((data.position.y / Singleton<EditorController>.Instance.screenSize.y) * Screen.height));
                data.position = pos;
            }
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool didPatch = false;
            CodeInstruction[] codeInstructions = instructions.ToArray();
            for (int i = 0; i < codeInstructions.Length; i++)
            {
                CodeInstruction instruction = codeInstructions[i];
                yield return instruction;
                if (didPatch) continue;
                if (i + 12 > codeInstructions.Length - 1) continue;
                if (
                    (codeInstructions[i + 0].opcode == OpCodes.Ldarg_0) &&
                    (codeInstructions[i + 1].opcode == OpCodes.Ldfld) &&
                    (codeInstructions[i + 2].opcode == OpCodes.Callvirt) &&
                    (codeInstructions[i + 3].opcode == OpCodes.Ldarg_0) &&
                    (codeInstructions[i + 4].opcode == OpCodes.Ldfld) &&
                    (codeInstructions[i + 5].opcode == OpCodes.Ldarg_0) &&
                    (codeInstructions[i + 6].opcode == OpCodes.Ldfld) &&
                    (codeInstructions[i + 7].opcode == OpCodes.Ldarg_0) &&
                    (codeInstructions[i + 8].opcode == OpCodes.Ldfld) &&
                    (codeInstructions[i + 9].opcode == OpCodes.Callvirt) &&
                    (codeInstructions[i + 10].opcode == OpCodes.Ldc_I4_0) &&
                    (codeInstructions[i + 11].opcode == OpCodes.Stloc_0)
                    )
                {
                    didPatch = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0); //this
                    yield return new CodeInstruction(OpCodes.Ldfld, pointerEvent); //pointerEventData
                    yield return new CodeInstruction(OpCodes.Call, changeV); //CursorPatch.ChangeValue
                }
            }
            if (!didPatch) throw new Exception("Unable to patch CursorController.Update!");
            yield break;
        }
    }
}
