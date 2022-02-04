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
    //This has the moisture pump mark terrain as not natural water anymore.
    [HarmonyPatch(typeof(CompTerrainPumpDry), "AffectCell", new Type[] { typeof(Map), typeof(IntVec3) })]
    public class CompTerrainPumpDry_AffectCell
    {
        internal static void Postfix(Map map, IntVec3 c)
        {
            TerrainDef terrain = c.GetTerrain(map);
            if (terrain == TerrainDefOf.WaterDeep ||
                terrain == TerrainDefOf.WaterShallow ||
                terrain == WaterDefs.Marsh ||
                terrain == TerrainDefOf.WaterMovingShallow ||
                terrain == TerrainDefOf.WaterMovingChestDeep) //If it's the freezable type of water..
            {
                var i = map.cellIndices.CellToIndex(c);
                var comp = HarmonyPatchSharedData.GetCompForMap(map);
                comp.AllWaterTerrainGrid[i] = comp.NaturalWaterTerrainGrid[i] = null; //Null out both all water and natural water for this tile on the grid.
            }
        }
    }
}