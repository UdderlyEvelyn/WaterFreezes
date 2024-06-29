using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace WF;

//This fixes issues with checking a blueprint that will build terrain while it's already placed in the spot causing an issue where it checks the terrain you're placing for affordances to place the terrain itself.
//E.g., "building a bridge. terrain under it changes. bridge isn't bridgeable sorry gotta delete the blueprint!" no longer occurs.
[HarmonyPatch(typeof(TerrainGrid), "DoTerrainChangedEffects")]
public class TerrainGrid_DoTerrainChangedEffects
{
    public static readonly MethodInfo ListThing_get_Item =
        typeof(List<Thing>).GetMethod("get_Item", [typeof(int)]);

    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldnull) //Luckily the only one in the method. (:D)
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return new CodeInstruction(OpCodes.Ldloc_2);
                yield return new CodeInstruction(OpCodes.Callvirt, ListThing_get_Item);
            }
            else
            {
                yield return instruction;
            }
        }
    }
}