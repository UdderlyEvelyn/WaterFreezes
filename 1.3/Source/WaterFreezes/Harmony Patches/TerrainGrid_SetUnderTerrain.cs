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
    //[HarmonyPatch(typeof(TerrainGrid), "SetUnderTerrain")]
    //public class TerrainGrid_SetUnderTerrain
    //{
    //    internal static void Prefix(IntVec3 c, TerrainDef newTerr, Map ___map)
    //    {
    //        if (WaterFreezesCompCache.GetFor(___map).NaturalWaterTerrainGrid[___map.cellIndices.CellToIndex(c)] == null)
    //        {
    //            var oldTerr = ___map.terrainGrid.UnderTerrainAt(c);
    //            if (oldTerr != newTerr)
    //                WaterFreezes.Log("SetUnderTerrain Prefix: Old terrain was \"" + (oldTerr?.defName ?? "null") + "\", new terrain will be \"" + newTerr.defName + "\"..");
    //        }
    //    }
    //}
}