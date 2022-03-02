using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace WF
{
    public static class API
    {
		/// <summary>
		/// Returns whether something is thawable ice, which is ice terrain registered with Water Freezes (called from outside as part of the API).
		/// </summary>
		/// <param name="def"></param>
		/// <returns></returns>
		public static bool IsThawableIce(TerrainDef def)
        {
			return def.IsThawableIce();
        }

		/// <summary>
		/// Removes ice from a cell, updates its ice stage, and reports the amount of ice removed (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public static float TakeCellIce(Map map, IntVec3 cell)
		{
			int i = map.cellIndices.CellToIndex(cell);
			var comp = WaterFreezesCompCache.GetFor(map);
			var ice = comp.IceDepthGrid[i];
			comp.IceDepthGrid[i] = 0;
			comp.UpdateIceStage(cell);
			return ice;
		}

		/// <summary>
		/// Returns the amount of ice in a cell (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public static float QueryCellIce(Map map, IntVec3 cell)
		{
			return WaterFreezesCompCache.GetFor(map).IceDepthGrid[map.cellIndices.CellToIndex(cell)];
		}

		/// <summary>
		/// Returns the amount of water in a cell (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public static float QueryCellWater(Map map, IntVec3 cell)
		{
			return WaterFreezesCompCache.GetFor(map).WaterDepthGrid[map.cellIndices.CellToIndex(cell)];
		}

		/// <summary>
		/// Returns the natural water def of a cell or null if not present (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public static TerrainDef QueryCellNaturalWater(Map map, IntVec3 cell)
		{
			return WaterFreezesCompCache.GetFor(map).NaturalWaterTerrainGrid[map.cellIndices.CellToIndex(cell)];
		}

		/// <summary>
		/// Returns the all water def of a cell or null if not present (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public static TerrainDef QueryCellAllWater(Map map, IntVec3 cell)
		{
			return WaterFreezesCompCache.GetFor(map).AllWaterTerrainGrid[map.cellIndices.CellToIndex(cell)];
		}

		/// <summary>
		/// Removes the natural water designation at a cell (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		public static void ClearCellNaturalWater(Map map, IntVec3 cell)
		{
			WaterFreezesCompCache.GetFor(map).NaturalWaterTerrainGrid[map.cellIndices.CellToIndex(cell)] = null;
		}

		/// <summary>
		/// Removes all water at a cell (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		public static void ClearCellWater(Map map, IntVec3 cell)
		{
			WaterFreezesCompCache.GetFor(map).WaterDepthGrid[map.cellIndices.CellToIndex(cell)] = 0;
		}
	}
}
