using HarmonyLib;
using RimWorld;
using Verse;

namespace WF;

//This has the moisture pump mark terrain as not natural water anymore.
[HarmonyPatch(typeof(CompTerrainPumpDry), nameof(CompTerrainPumpDry.AffectCell), typeof(Map), typeof(IntVec3))]
public class CompTerrainPumpDry_AffectCell
{
    internal static void Prefix(Map map, IntVec3 c, ref TerrainDef __state)
    {
        //Save cell state before drying
        __state = map.terrainGrid.TerrainAt(c);
    }

    internal static void Postfix(Map map, IntVec3 c, ref TerrainDef __state)
    {
        var terrain = __state;

        if (terrain != TerrainDefOf.WaterDeep &&
            terrain != TerrainDefOf.WaterShallow &&
            terrain != WaterDefs.Marsh &&
            terrain != TerrainDefOf.WaterMovingShallow &&
            terrain != TerrainDefOf.WaterMovingChestDeep) //If it's the freezable type of water.
        {
            return;
        }

        var i = map.cellIndices.CellToIndex(c);
        var comp = WaterFreezesCompCache.GetFor(map);
        //Don't mess with water at all unless
        if (!WaterFreezesSettings.MoisturePumpClearsNaturalWater)
        {
            return;
        }

        comp.NaturalWaterTerrainGrid[i] = null;
        comp.AllWaterTerrainGrid[i] = null;
        comp.WaterDepthGrid[i] = 0;
    }
}