using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

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
            //    WaterFreezes.Log("SetTerrain Prefix: Old terrain was \"" + (__state?.defName ?? "null") + "\", new terrain will be \"" + newTerr.defName + "\"..");
        }

        internal static void Postfix(IntVec3 c, TerrainDef newTerr, Map ___map, ref TerrainDef __state)
        {
            //if (__state != newTerr)
            //    WaterFreezes.Log("SetTerrain Postfix: Old terrain was \"" + (__state?.defName ?? "null") + "\", new terrain will be \"" + newTerr.defName + "\"..");
            int i = ___map.cellIndices.CellToIndex(c);
            var oldTerrain = __state;
            if (oldTerrain == newTerr || oldTerrain == null) //If we're not actually changing anything or it had no terrain previously..
                return; //Who cares?
            if (oldTerrain == TerrainDefOf.WaterDeep ||
                oldTerrain == TerrainDefOf.WaterShallow ||
                oldTerrain == WaterDefs.Marsh ||
                oldTerrain == TerrainDefOf.WaterMovingShallow ||
                oldTerrain == TerrainDefOf.WaterMovingChestDeep) //If old terrain is freezable water..
            {
                if (!(newTerr == TerrainDefOf.WaterDeep ||
                   newTerr == TerrainDefOf.WaterShallow ||
                   newTerr == WaterDefs.Marsh ||
                   newTerr == TerrainDefOf.WaterMovingShallow ||
                   newTerr == TerrainDefOf.WaterMovingChestDeep)) //It's water and becoming NOT water..
                {
                    var comp = WaterFreezesCompCache.GetFor(___map);
                    if (comp == null || !comp.Initialized) //If comp is null or uninitialized..
                        return; //Don't try.
                    var naturalWater = comp.NaturalWaterTerrainGrid[i];
                    if (!(naturalWater == TerrainDefOf.WaterDeep ||
                          naturalWater == TerrainDefOf.WaterShallow ||
                          naturalWater == WaterDefs.Marsh ||
                          naturalWater == TerrainDefOf.WaterMovingShallow ||
                          naturalWater == TerrainDefOf.WaterMovingChestDeep)) //It's not natural water..
                    {
                        //WaterFreezes.Log("SetTerrain Postfix: Unnatural water going to non-water, nulling AllTerrain and zeroing water depth, then updating pseudo water elevation grid.");
                        comp.AllWaterTerrainGrid[i] = null; //Stop tracking it.
                        comp.WaterDepthGrid[i] = 0; //Make sure there's no water here now or else it'll be restored (in case a mod besides us is doing this).
                        comp.UpdatePseudoWaterElevationGridAtAndAroundCell(c);
                    }
                }
            }
            else //It wasn't water to begin with..
            {
                var comp = WaterFreezesCompCache.GetFor(___map);
                if (comp == null || !comp.Initialized) //If comp is null or uninitialized..
                    return; //Don't try.
                if (newTerr == TerrainDefOf.WaterDeep ||
                    newTerr == TerrainDefOf.WaterShallow ||
                    newTerr == WaterDefs.Marsh ||
                    newTerr == TerrainDefOf.WaterMovingShallow ||
                    newTerr == TerrainDefOf.WaterMovingChestDeep) //But it's becoming water now..
                {
                    var naturalWater = comp.NaturalWaterTerrainGrid[i];
                    if (!(naturalWater == TerrainDefOf.WaterDeep ||
                          naturalWater == TerrainDefOf.WaterShallow ||
                          naturalWater == WaterDefs.Marsh ||
                          naturalWater == TerrainDefOf.WaterMovingShallow ||
                          naturalWater == TerrainDefOf.WaterMovingChestDeep)) //It's not natural water..
                    {
                        comp.AllWaterTerrainGrid[i] = newTerr; //Track it.
                        comp.SetMaxWaterByDef(i, newTerr, updateIceStage: false);
                        comp.UpdatePseudoWaterElevationGridAtAndAroundCell(c);
                    }
                }
            }
        }
    }
}