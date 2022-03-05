using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace WF.Harmony_Patches
{
    internal class SnowGrid_SetDepth
    {
        //This fixes bad performance when setting depth to zero by bypassing a ton of unnecessary checks.
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; ++i)
            {
                var instruction = instructionList[i];
                yield return instruction; //Send this one out.
                if (instruction.opcode == OpCodes.Stloc_0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_2); //newDepth
                    yield return new CodeInstruction(OpCodes.Ldc_R4); //0
                    yield return new CodeInstruction(OpCodes.Ceq); //==
                    yield return new CodeInstruction(OpCodes.Brfalse_S, instructionList[i + 4].operand); //If it equals 0, then.. jump to where the existing condition would.
                }
            }
        }
    }
}