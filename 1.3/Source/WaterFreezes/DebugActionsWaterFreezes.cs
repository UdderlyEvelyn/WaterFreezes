using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using System.Runtime.CompilerServices;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WF
{
    public static class DebugActionsWaterFreezes
    {
		[DebugAction("Water Freezes", "Reinitialize MapComponent", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_ReinitializeMapComponent()
		{
			var map = Find.CurrentMap;
			var comp = WaterFreezesCompCache.GetFor(map);
			comp.AllWaterTerrainGrid = null;
			comp.NaturalWaterTerrainGrid = null;
			comp.WaterDepthGrid = null;
			comp.IceDepthGrid = null;
			comp.PseudoWaterElevationGrid = null;
			comp.Initialize();
			Messages.Message("Water Freezes MapComponent was reinitialized.", MessageTypeDefOf.TaskCompletion);
		}

		[DebugAction("Water Freezes", "Set As Natural Water", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetAsNaturalWater()
		{
			var map = Find.CurrentMap;
			var comp = WaterFreezesCompCache.GetFor(map);
			IntVec3 cell = UI.MouseCell();
			var index = map.cellIndices.CellToIndex(cell);
			var currentTerrain = cell.GetTerrain(map);
			if (comp.IsAnyWater(index, map, currentTerrain))
				comp.NaturalWaterTerrainGrid[index] = comp.AllWaterTerrainGrid[index] = currentTerrain;
			else
				Messages.Message("Attempted to set natural water status for non-water terrain.", MessageTypeDefOf.RejectInput);
		}

		[DebugAction("Water Freezes", "Clear Natural Water Status", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_ClearNaturalWaterStatus()
		{
			var map = Find.CurrentMap;
			IntVec3 cell = UI.MouseCell();
			var index = map.cellIndices.CellToIndex(cell);
			var comp = WaterFreezesCompCache.GetFor(map);
			if (comp.NaturalWaterTerrainGrid[index] != null)
			{
				comp.NaturalWaterTerrainGrid[index] = null;
				comp.UpdateIceStage(cell);
			}
			else
				Messages.Message("Attempted to set natural water status to null where it was already null.", MessageTypeDefOf.RejectInput);
		}

		[DebugAction("Water Freezes", "Set Water Depth To Max", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetWaterDepthToMax()
		{
			var map = Find.CurrentMap;
			var comp = WaterFreezesCompCache.GetFor(map);
			var cell = UI.MouseCell();
			var index = map.cellIndices.CellToIndex(cell);
			if (comp.AllWaterTerrainGrid[index] == null)
			{
				Messages.Message("Attempted to set water depth to max for non-water terrain.", MessageTypeDefOf.RejectInput);
				return; //Abort.
			}
			var currentTerrain = map.terrainGrid.TerrainAt(index);
			var underTerrain = map.terrainGrid.UnderTerrainAt(index);
			float maxForTerrain = 0;
			if (comp.IsAnyShallowWater(index, map, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterShallow;
			else if (comp.IsAnyDeepWater(index, map, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterDeep;
			else if (comp.IsMarsh(index, map, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterMarsh;
			Messages.Message("Set depth to " + maxForTerrain + " where it was " + comp.WaterDepthGrid[index] + " previously, terrain is \"" + currentTerrain.defName + "\".", MessageTypeDefOf.TaskCompletion);
			comp.WaterDepthGrid[index] = maxForTerrain;
			comp.UpdateIceStage(cell, currentTerrain);
		}

		[DebugAction("Water Freezes", "Set Water Depth To Zero", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetWaterDepthToZero()
		{
			var map = Find.CurrentMap;
			var comp = WaterFreezesCompCache.GetFor(map);
			var cell = UI.MouseCell();
			var index = map.cellIndices.CellToIndex(cell);
			if (comp.AllWaterTerrainGrid[index] != null)
			{
				comp.WaterDepthGrid[index] = 0;
				comp.UpdateIceStage(cell);
			}
			else
				Messages.Message("Attempted to set water depth to zero for non-water terrain.", MessageTypeDefOf.RejectInput);
		}

		[DebugAction("Water Freezes", "Set Ice Depth To Max", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetIceDepthToMax()
		{
			var map = Find.CurrentMap;
			var comp = WaterFreezesCompCache.GetFor(map);
			var cell = UI.MouseCell();
			var index = map.cellIndices.CellToIndex(cell);
			if (comp.AllWaterTerrainGrid[index] == null)
			{
				Messages.Message("Attempted to set ice depth to max for non-water terrain.", MessageTypeDefOf.RejectInput);
				return; //Abort.
			}
			var currentTerrain = map.terrainGrid.TerrainAt(index);
			var underTerrain = map.terrainGrid.UnderTerrainAt(index);
			float maxForTerrain = 0;
			if (comp.IsAnyShallowWater(index, map, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxIceShallow;
			else if (comp.IsAnyDeepWater(index, map, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxIceDeep;
			else if (comp.IsMarsh(index, map, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxIceMarsh;
			comp.IceDepthGrid[index] = maxForTerrain;
			comp.UpdateIceStage(cell, currentTerrain);
		}

		[DebugAction("Water Freezes", "Set Ice Depth To Zero", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetIceDepthToZero()
		{
			var map = Find.CurrentMap;
			var comp = WaterFreezesCompCache.GetFor(map);
			var cell = UI.MouseCell();
			var index = map.cellIndices.CellToIndex(cell);
			if (comp.AllWaterTerrainGrid[index] != null)
			{
				comp.IceDepthGrid[index] = 0;
				comp.UpdateIceStage(cell);
			}
			else
				Messages.Message("Attempted to set ice depth to zero for non-water terrain.", MessageTypeDefOf.RejectInput);
		}

		[DebugAction("Water Freezes", "Set Ice/Water Depth To Zero", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetIceAndWaterDepthToZero()
		{
			var map = Find.CurrentMap;
			var comp = WaterFreezesCompCache.GetFor(map);
			var cell = UI.MouseCell();
			var index = map.cellIndices.CellToIndex(cell);
			if (comp.AllWaterTerrainGrid[index] != null)
			{
				comp.IceDepthGrid[index] = 0;
				comp.WaterDepthGrid[index] = 0;
				comp.UpdateIceStage(cell);
			}
			else
				Messages.Message("Attempted to set ice & water depth to zero for non-water terrain.", MessageTypeDefOf.RejectInput);
		}

		[DebugAction("Water Freezes", "Set Natural Water/Depth To Max", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetNaturalWaterAndWaterDepthToMax()
		{
			var map = Find.CurrentMap;
			var comp = WaterFreezesCompCache.GetFor(map);
			var cell = UI.MouseCell();
			var index = map.cellIndices.CellToIndex(cell);
			var currentTerrain = map.terrainGrid.TerrainAt(index);
			var underTerrain = map.terrainGrid.UnderTerrainAt(index);
			if (comp.IsAnyWater(index, map, currentTerrain, underTerrain))
				comp.NaturalWaterTerrainGrid[index] = comp.AllWaterTerrainGrid[index] = currentTerrain;
			else
			{
				Messages.Message("Attempted to set natural water and water depth to max for non-water terrain.", MessageTypeDefOf.RejectInput);
				return; //Abort, not water.
			}
			float maxForTerrain = 0;
			if (comp.IsAnyShallowWater(index, map, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterShallow;
			else if (comp.IsAnyDeepWater(index, map, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterDeep;
			else if (comp.IsMarsh(index, map, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterMarsh;
			comp.WaterDepthGrid[index] = maxForTerrain;
			comp.UpdateIceStage(cell, currentTerrain);
		}
	}
}