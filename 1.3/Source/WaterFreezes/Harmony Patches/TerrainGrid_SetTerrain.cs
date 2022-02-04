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
        internal static void Prefix(IntVec3 c, TerrainDef newTerr, Map ___map)
        {
            int i = ___map.cellIndices.CellToIndex(c);
            var oldTerrain = ___map.terrainGrid.TerrainAt(i);
            if (oldTerrain == newTerr) //If we're not actually changing anything..
                return; //Who cares?
            if (oldTerrain == TerrainDefOf.WaterDeep ||
                oldTerrain == TerrainDefOf.WaterShallow ||
                oldTerrain == WaterDefs.Marsh ||
                oldTerrain == TerrainDefOf.WaterMovingShallow ||
                oldTerrain == TerrainDefOf.WaterMovingChestDeep) //If it's the freezable type of water..
            {
                var comp = HarmonyPatchSharedData.GetCompForMap(___map);
                if (newTerr == TerrainDefOf.WaterDeep ||
                    newTerr == TerrainDefOf.WaterShallow ||
                    newTerr == WaterDefs.Marsh ||
                    newTerr == TerrainDefOf.WaterMovingShallow ||
                    newTerr == TerrainDefOf.WaterMovingChestDeep) //If it's becoming water..
                    comp.AllWaterTerrainGrid[i] = newTerr;
                else //It's water and becoming not water..
                {
                    var naturalWater = comp.NaturalWaterTerrainGrid[i];
                    if (!(naturalWater == TerrainDefOf.WaterDeep ||
                          naturalWater == TerrainDefOf.WaterShallow ||
                          naturalWater == WaterDefs.Marsh ||
                          naturalWater == TerrainDefOf.WaterMovingShallow ||
                          naturalWater == TerrainDefOf.WaterMovingChestDeep)) //It's not natural water..
                    {
                        comp.AllWaterTerrainGrid[i] = null; //Mark it as not water.
                        comp.WaterDepthGrid[i] = 0; //Make sure there's no water here now or else it'll be restored.
                    }
                }
            }
        }
    }
}