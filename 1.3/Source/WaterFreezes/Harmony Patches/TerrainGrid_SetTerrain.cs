using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;

namespace WF
{
    [HarmonyPatch(typeof(TerrainGrid), "SetTerrain")]
    public class TerrainGrid_SetTerrain
    {
        //This updates the AllWaterTerrainGrid when terrain is changed to be which water type it is, or null, depending on the old/new terrain (with exception for if it's natural water preventing nulling).
        internal static void Prefix(IntVec3 c, TerrainDef newTerr, Map ___map, ref TerrainDef __state)
        {
            __state = ___map.terrainGrid.TerrainAt(c);
            //if (__state != newTerr)
            //    WaterFreezes.Log("SetTerrain Prefix: Old terrain was \"" + (__state?.defName ?? "null") + "\", new terrain will be \"" + (newTerr?.defName ?? "null") + "\"..");
        }

        internal static FieldInfo terrainDef_layerable = AccessTools.Field(typeof(TerrainDef), "layerable");
        internal static FieldInfo terrainGrid_topGrid = AccessTools.Field(typeof(TerrainGrid), "topGrid");

        //This fixes failures when attempting to place layerable terrain during worldgen (technically any time that the terrain at a place is null and it tries to place layerable terrain).
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionList = instructions.ToList();
            for (int i = 0; i < instructionList.Count; ++i)
            {
                var instruction = instructionList[i];
                yield return instruction; //Send this one out.
                if (instruction.opcode == OpCodes.Ldfld && instruction.OperandIs(terrainDef_layerable)) //TerrainDef.layerable
                {
                    yield return instructionList[i + 1]; //Send the next one out too.
                    yield return new CodeInstruction(OpCodes.Ldarg_0); //this.
                    yield return new CodeInstruction(OpCodes.Ldfld, terrainGrid_topGrid); //TerrainGrid.topGrid
                    yield return new CodeInstruction(OpCodes.Ldloc_0); //num (indexer)
                    yield return new CodeInstruction(OpCodes.Ldelem_Ref); //[] (index int topGrid with indexer num)
                    yield return new CodeInstruction(OpCodes.Brfalse_S, instructionList[i + 1].operand); //If it's not null, then..
                    ++i; //Make sure we don't process that extra one we sent out.
                }
            }
        }

        internal static void Postfix(IntVec3 c, TerrainDef newTerr, Map ___map, ref TerrainDef __state)
        {
            //if (__state != newTerr)
            //    WaterFreezes.Log("SetTerrain Postfix: Old terrain was \"" + (__state?.defName ?? "null") + "\", new terrain will be \"" + (newTerr?.defName ?? "null") + "\"..");
            var oldTerrain = __state;
            if (oldTerrain == newTerr || oldTerrain == null) //If we're not actually changing anything or it had no terrain previously..
                return; //Who cares?
            int i = ___map.cellIndices.CellToIndex(c);
            if (oldTerrain.IsFreezableWater()) //If old terrain is freezable water..
            {
                if (!newTerr.IsFreezableWater()) //It's water and becoming NOT water..
                {
                    var comp = WaterFreezesCompCache.GetFor(___map);
                    if (comp == null || !comp.Initialized) //If comp is null or uninitialized..
                        return; //Don't try.
                    var naturalWater = comp.NaturalWaterTerrainGrid[i] != null;
                    if (!naturalWater && !newTerr.IsThawableIce()) //It's not natural water, freezable water, or thawable ice.
                    {
                        //WaterFreezes.Log("SetTerrain Postfix: Considered water going to non-water, nulling AllTerrain and zeroing water depth, then updating pseudo water elevation grid.");
                        comp.AllWaterTerrainGrid[i] = null; //Stop tracking it.
                        comp.WaterDepthGrid[i] = 0; //Make sure there's no water here now or else it'll be restored (in case a mod besides us is doing this).
                    }
                    comp.UpdatePseudoWaterElevationGridAtAndAroundCell(c);
                }
            }
            else //It wasn't water to begin with..
            {
                var comp = WaterFreezesCompCache.GetFor(___map);
                if (comp == null || !comp.Initialized) //If comp is null or uninitialized..
                    return; //Don't try.
                if (!oldTerrain.IsThawableIce() && newTerr.IsFreezableWater()) //But it's becoming water now..
                {
                    var naturalWater = comp.NaturalWaterTerrainGrid[i] != null;
                    if (!naturalWater && !newTerr.IsThawableIce()) //It's not natural water..
                    {
                        //WaterFreezes.Log("SetTerrain Postfix: Non-water going to unnatural water, setting AllTerrain and maxing water depth, then updating pseudo water elevation grid.");
                        comp.AllWaterTerrainGrid[i] = newTerr; //Track it.
                        comp.SetMaxWaterByDef(i, newTerr, updateIceStage: false);
                    }
                    comp.UpdatePseudoWaterElevationGridAtAndAroundCell(c);
                }
            }
        }
    }
}