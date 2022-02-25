using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using System.Runtime.CompilerServices;
using System;

namespace WF
{
    public static class DebugActionsWaterFreezes
    {
		[DebugAction("Water Freezes", "Reinitialize MapComponent", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_ReinitializeMapComponent()
		{
			ReinitializeMapComponent(Find.CurrentMap);
		}

		public static void ReinitializeMapComponent(Map map)
        {
			var comp = WaterFreezesCompCache.GetFor(map);
			//Clear out any existing ice instantly so we aren't left with stuff a reinit can't comprehend.
			for (int i = 0; i < comp.AllWaterTerrainGrid.Length; i++)
			{
				var allWaterTerrain = comp.AllWaterTerrainGrid[i];
				if (allWaterTerrain != null && map.terrainGrid.TerrainAt(i) != allWaterTerrain)
				{
					comp.IceDepthGrid[i] = 0;
					map.terrainGrid.SetTerrain(map.cellIndices.IndexToCell(i), allWaterTerrain);
				}
			}
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
			SetAsNaturalWater(Find.CurrentMap, UI.MouseCell());
		}

		[DebugAction("Water Freezes (Rect)", "Set As Natural Water", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetAsNaturalWater_Rect()
		{
			DoForRect(SetAsNaturalWater);
		}

		public static void SetAsNaturalWater(Map map, IntVec3 cell)
		{
			var comp = WaterFreezesCompCache.GetFor(map);
			var index = map.cellIndices.CellToIndex(cell);
			var currentTerrain = cell.GetTerrain(map);
			if (comp.IsAnyWater(index, currentTerrain))
				comp.NaturalWaterTerrainGrid[index] = comp.AllWaterTerrainGrid[index] = currentTerrain;
			else
				Messages.Message("Attempted to set natural water status for non-water terrain.", MessageTypeDefOf.RejectInput);
		}

		[DebugAction("Water Freezes", "Clear Natural Water Status", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_ClearNaturalWaterStatus()
		{
			ClearNaturalWaterStatus(Find.CurrentMap, UI.MouseCell());
		}

		[DebugAction("Water Freezes (Rect)", "Clear Natural Water Status", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_ClearNaturalWaterStatus_Rect()
		{
			DoForRect(ClearNaturalWaterStatus);
		}

		public static void ClearNaturalWaterStatus(Map map, IntVec3 cell)
		{
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

		[DebugAction("Water Freezes", "Clear Natural Water Status/Depth", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_ClearNaturalWaterStatusAndDepth()
		{
			ClearNaturalWaterStatusAndDepth(Find.CurrentMap, UI.MouseCell());
		}

		[DebugAction("Water Freezes (Rect)", "Clear Natural Water Status/Depth", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_ClearNaturalWaterStatusAndDepth_Rect()
		{
			DoForRect(ClearNaturalWaterStatusAndDepth);
		}

		public static void ClearNaturalWaterStatusAndDepth(Map map, IntVec3 cell)
		{
			var index = map.cellIndices.CellToIndex(cell);
			var comp = WaterFreezesCompCache.GetFor(map);
			if (comp.NaturalWaterTerrainGrid[index] != null)
			{
				comp.NaturalWaterTerrainGrid[index] = null;
				comp.WaterDepthGrid[index] = 0;
				comp.UpdateIceStage(cell);
			}
			else
				Messages.Message("Attempted to set natural water status to null where it was already null.", MessageTypeDefOf.RejectInput);
		}

		[DebugAction("Water Freezes", "Set Water Depth To Max", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetWaterDepthToMax()
		{
			SetWaterDepthToMax(Find.CurrentMap, UI.MouseCell());
		}

		[DebugAction("Water Freezes (Rect)", "Set Water Depth To Max", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetWaterDepthToMax_Rect()
		{
			DoForRect(SetWaterDepthToMax);
		}

		public static void SetWaterDepthToMax(Map map, IntVec3 cell)
		{
			var comp = WaterFreezesCompCache.GetFor(map);
			var index = map.cellIndices.CellToIndex(cell);
			if (comp.AllWaterTerrainGrid[index] == null)
			{
				Messages.Message("Attempted to set water depth to max for non-water terrain.", MessageTypeDefOf.RejectInput);
				return; //Abort.
			}
			var currentTerrain = map.terrainGrid.TerrainAt(index);
			var underTerrain = map.terrainGrid.UnderTerrainAt(index);
			float maxForTerrain = 0;
			if (comp.IsAnyShallowWater(index, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterShallow;
			else if (comp.IsAnyDeepWater(index, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterDeep;
			else if (comp.IsMarsh(index, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterMarsh;
			Messages.Message("Set depth to " + maxForTerrain + " where it was " + comp.WaterDepthGrid[index] + " previously, terrain is \"" + currentTerrain.defName + "\".", MessageTypeDefOf.TaskCompletion);
			comp.WaterDepthGrid[index] = maxForTerrain;
			comp.UpdateIceStage(cell, currentTerrain);
		}

		[DebugAction("Water Freezes", "Set Water Depth To Zero", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetWaterDepthToZero()
		{
			SetWaterDepthToZero(Find.CurrentMap, UI.MouseCell());
		}

		[DebugAction("Water Freezes (Rect)", "Set Water Depth To Zero", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetWaterDepthToZero_Rect()
		{
			DoForRect(SetWaterDepthToZero);
		}

		public static void SetWaterDepthToZero(Map map, IntVec3 cell)
        {
			var comp = WaterFreezesCompCache.GetFor(map);
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
			SetIceDepthToMax(Find.CurrentMap, UI.MouseCell());
		}

		[DebugAction("Water Freezes (Rect)", "Set Ice Depth To Max", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetIceDepthToMax_Rect()
		{
			DoForRect(SetIceDepthToMax);
		}

		public static void SetIceDepthToMax(Map map, IntVec3 cell)
		{
			var comp = WaterFreezesCompCache.GetFor(map);
			var index = map.cellIndices.CellToIndex(cell);
			if (comp.AllWaterTerrainGrid[index] == null)
			{
				Messages.Message("Attempted to set ice depth to max for non-water terrain.", MessageTypeDefOf.RejectInput);
				return; //Abort.
			}
			var currentTerrain = map.terrainGrid.TerrainAt(index);
			var underTerrain = map.terrainGrid.UnderTerrainAt(index);
			float maxForTerrain = 0;
			if (comp.IsAnyShallowWater(index, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxIceShallow;
			else if (comp.IsAnyDeepWater(index, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxIceDeep;
			else if (comp.IsMarsh(index, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxIceMarsh;
			comp.IceDepthGrid[index] = maxForTerrain;
			comp.UpdateIceStage(cell, currentTerrain);
		}

		[DebugAction("Water Freezes", "Set Ice Depth To Zero", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetIceDepthToZero()
		{
			SetIceDepthToZero(Find.CurrentMap, UI.MouseCell());
		}

		[DebugAction("Water Freezes (Rect)", "Set Ice Depth To Zero", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetIceDepthToZero_Rect()
		{
			DoForRect(SetIceDepthToZero);
		}

		public static void SetIceDepthToZero(Map map, IntVec3 cell)
        {
			var comp = WaterFreezesCompCache.GetFor(map);
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
			SetIceAndWaterDepthToZero(Find.CurrentMap, UI.MouseCell());
		}

		public static void SetIceAndWaterDepthToZero(Map map, IntVec3 cell)
		{
			var comp = WaterFreezesCompCache.GetFor(map);
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
			SetNaturalWaterAndWaterDepthToMax(Find.CurrentMap, UI.MouseCell());
		}

		private static void SetNaturalWaterAndWaterDepthToMax(Map map, IntVec3 cell)
        {
			var comp = WaterFreezesCompCache.GetFor(map);
			var index = map.cellIndices.CellToIndex(cell);
			var currentTerrain = map.terrainGrid.TerrainAt(index);
			var underTerrain = map.terrainGrid.UnderTerrainAt(index);
			if (comp.IsAnyWater(index, currentTerrain, underTerrain))
				comp.NaturalWaterTerrainGrid[index] = comp.AllWaterTerrainGrid[index] = currentTerrain;
			else
			{
				Messages.Message("Attempted to set natural water and water depth to max for non-water terrain.", MessageTypeDefOf.RejectInput);
				return; //Abort, not water.
			}
			float maxForTerrain = 0;
			if (comp.IsAnyShallowWater(index, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterShallow;
			else if (comp.IsAnyDeepWater(index, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterDeep;
			else if (comp.IsMarsh(index, currentTerrain, underTerrain))
				maxForTerrain = comp.MaxWaterMarsh;
			comp.WaterDepthGrid[index] = maxForTerrain;
			comp.UpdateIceStage(cell, currentTerrain);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void DoForRect(Action<Map, IntVec3> action)
        {
			Map map = Find.CurrentMap;
			DebugTool tool = null;
			IntVec3 firstCorner;
			tool = new DebugTool("first corner...", delegate
			{
				firstCorner = UI.MouseCell();
				DebugTools.curTool = new DebugTool("second corner...", delegate
				{
					IntVec3 secondCorner = UI.MouseCell();
					foreach (IntVec3 cell in CellRect.FromLimits(firstCorner, secondCorner).ClipInsideMap(map))
						action(map, cell);
					DebugTools.curTool = tool;
				}, firstCorner);
			});
			DebugTools.curTool = tool;
		}
	}
}