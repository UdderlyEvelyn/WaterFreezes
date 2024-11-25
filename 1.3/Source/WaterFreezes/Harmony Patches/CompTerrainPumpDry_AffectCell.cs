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
        internal static void Prefix(Map map, IntVec3 c, ref TerrainDef __state)
        {
            //Save cell state before drying
            __state = map.terrainGrid.TerrainAt(c);

        }
        internal static void Postfix(Map map, IntVec3 c, ref TerrainDef __state)
        {
            TerrainDef terrain = __state;
            Log.Message(terrain.ToString());
            if (terrain == TerrainDefOf.WaterDeep ||
                terrain == TerrainDefOf.WaterShallow ||
                terrain == WaterDefs.Marsh ||
                terrain == TerrainDefOf.WaterMovingShallow ||
                terrain == TerrainDefOf.WaterMovingChestDeep) //If it's the freezable type of water..
            {
                var i = map.cellIndices.CellToIndex(c);
                var comp = WaterFreezesCompCache.GetFor(map);
                //Don't mess with water at all unless
                if (WaterFreezesSettings.MoisturePumpClearsNaturalWater)
                {
                    comp.NaturalWaterTerrainGrid[i] = null;
                    comp.AllWaterTerrainGrid[i] = null;
                    comp.WaterDepthGrid[i] = 0;
                }
            }
        }
    }
}