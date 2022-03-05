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

        internal static void Postfix(IntVec3 c, TerrainDef newTerr, Map ___map, ref TerrainDef __state)
        {
            //if (__state != newTerr)
            //    WaterFreezes.Log("SetTerrain Postfix: Old terrain was \"" + (__state?.defName ?? "null") + "\", new terrain will be \"" + (newTerr?.defName ?? "null") + "\"..");
            if (__state == newTerr || __state == null) //If we're not actually changing anything or it had no terrain previously..
                return; //Who cares?
            int i = ___map.cellIndices.CellToIndex(c);
            if (__state.IsFreezableWater()) //If old terrain is freezable water..
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
                    ___map.snowGrid.SetDepth(c, 0f);
                    comp.UpdatePseudoWaterElevationGridAtAndAroundCell(c);
                }
            }
            else //It wasn't water to begin with..
            {
                var comp = WaterFreezesCompCache.GetFor(___map);
                if (comp == null || !comp.Initialized) //If comp is null or uninitialized..
                    return; //Don't try.
                if (!__state.IsThawableIce() && newTerr.IsFreezableWater()) //But it's becoming water now..
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