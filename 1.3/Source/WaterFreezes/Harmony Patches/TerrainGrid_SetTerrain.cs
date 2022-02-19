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
        internal static void Prefix(IntVec3 c, TerrainDef newTerr, Map ___map, TerrainDef __state)
        {
            __state = ___map.terrainGrid.TerrainAt(c);
        }

        internal static void Postfix(IntVec3 c, TerrainDef newTerr, Map ___map, TerrainDef __state)
        {
            int i = ___map.cellIndices.CellToIndex(c);
            var oldTerrain = __state;
            if (oldTerrain == newTerr) //If we're not actually changing anything..
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