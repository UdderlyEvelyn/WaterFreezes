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
			for (int i = 0; i < comp.AllWaterTerrainGrid.Length; ++i)
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

		public static bool SetAsNaturalWater(Map map, IntVec3 cell, bool sendMessage = true)
		{
			var index = map.cellIndices.CellToIndex(cell);
			var comp = WaterFreezesCompCache.GetFor(map);
			var water = comp.AllWaterTerrainGrid[index];
			if (water != null)
				comp.NaturalWaterTerrainGrid[index] = water;
			else
			{
				if (sendMessage)
					Messages.Message("Attempted to set natural water status for non-water (or unsupported water) terrain.", MessageTypeDefOf.RejectInput);
				return false;
			}
			return true;
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

		public static bool ClearNaturalWaterStatus(Map map, IntVec3 cell, bool sendMessage = true)
		{
			var index = map.cellIndices.CellToIndex(cell);
			var comp = WaterFreezesCompCache.GetFor(map);
			if (comp.NaturalWaterTerrainGrid[index] != null)
			{
				comp.NaturalWaterTerrainGrid[index] = null;
				//comp.UpdateIceStage(cell); //Why was this here? Leaving it commented out just in case it becomes apparent later. -UdderlyEvelyn 3/27/22
			}
			else
			{
				if (sendMessage)
					Messages.Message("Attempted to set natural water status to null where it was already null.", MessageTypeDefOf.RejectInput);
				return false;
			}
			return true;
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

		public static bool ClearNaturalWaterStatusAndDepth(Map map, IntVec3 cell, bool sendMessage = true)
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
			{
				if (sendMessage)
					Messages.Message("Attempted to set natural water status to null where it was already null.", MessageTypeDefOf.RejectInput);
				return false;
			}
			return true;
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

		public static bool SetWaterDepthToMax(Map map, IntVec3 cell, bool sendMessage = true)
		{
			var comp = WaterFreezesCompCache.GetFor(map);
			var index = map.cellIndices.CellToIndex(cell);
			var water = comp.AllWaterTerrainGrid[index];
			if (water == null)
			{
				if (sendMessage)
					Messages.Message("Attempted to set water depth to max for non-water (or unrecognized water) terrain.", MessageTypeDefOf.RejectInput);	
				return false; //Abort.
			}
			comp.SetMaxWaterByDef(index, water);
			return true;
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

		public static bool SetWaterDepthToZero(Map map, IntVec3 cell, bool sendMessage = true)
        {
			var comp = WaterFreezesCompCache.GetFor(map);
			var index = map.cellIndices.CellToIndex(cell);
			if (comp.AllWaterTerrainGrid[index] != null)
			{
				comp.WaterDepthGrid[index] = 0;
				comp.UpdateIceStage(cell);
			}
			else
			{
				if (sendMessage)
					Messages.Message("Attempted to set water depth to zero for non-water (or unsupported water) terrain.", MessageTypeDefOf.RejectInput);
				return false;
			}
			return true;
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

		public static bool SetIceDepthToMax(Map map, IntVec3 cell, bool sendMessage = true)
		{
			var comp = WaterFreezesCompCache.GetFor(map);
			var index = map.cellIndices.CellToIndex(cell);
			var water = comp.AllWaterTerrainGrid[index];
			if (water == null)
			{
				if (sendMessage)
					Messages.Message("Attempted to set ice depth to max for non-water (or unsupported water) terrain.", MessageTypeDefOf.RejectInput);
				return false; //Abort.
			}
			var extension = WaterFreezesStatCache.GetExtension(water);
			comp.IceDepthGrid[index] = extension.MaxIceDepth;
			comp.UpdateIceStage(cell, extension);
			return true;
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

		public static bool SetIceDepthToZero(Map map, IntVec3 cell, bool sendMessage = true)
        {
			var comp = WaterFreezesCompCache.GetFor(map);
			var index = map.cellIndices.CellToIndex(cell);
			if (comp.AllWaterTerrainGrid[index] != null)
			{
				comp.IceDepthGrid[index] = 0;
				comp.UpdateIceStage(cell);
			}
			else
			{
				if (sendMessage)
					Messages.Message("Attempted to set ice depth to zero for non-water terrain.", MessageTypeDefOf.RejectInput);
				return false;
			}
			return true;
		}

		[DebugAction("Water Freezes", "Set Ice/Water Depth To Zero", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetIceAndWaterDepthToZero()
		{
			SetIceAndWaterDepthToZero(Find.CurrentMap, UI.MouseCell());
		}

		[DebugAction("Water Freezes (Rect)", "Set Ice/Water Depth To Zero", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetIceAndWaterDepthToZero_Rect()
		{
			DoForRect(SetIceAndWaterDepthToZero);
		}

		public static bool SetIceAndWaterDepthToZero(Map map, IntVec3 cell, bool sendMessage = true)
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
			{
				if (sendMessage)
					Messages.Message("Attempted to set ice & water depth to zero for non-water terrain.", MessageTypeDefOf.RejectInput);
				return false;
			}
			return true;
		}

		[DebugAction("Water Freezes", "Set Nat. Water/Depth To Max", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetNaturalWaterAndWaterDepthToMax()
		{
			SetNaturalWaterAndWaterDepthToMax(Find.CurrentMap, UI.MouseCell());
		}

		[DebugAction("Water Freezes (Rect)", "Set Nat. Water/Depth To Max", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void DebugAction_SetNaturalWaterAndDepthToMax_Rect()
		{
			DoForRect(SetNaturalWaterAndWaterDepthToMax);
		}

		private static bool SetNaturalWaterAndWaterDepthToMax(Map map, IntVec3 cell, bool sendMessage = true)
		{
			var index = map.cellIndices.CellToIndex(cell);
			var comp = WaterFreezesCompCache.GetFor(map);
			var water = comp.AllWaterTerrainGrid[index];
			if (water != null)
				comp.NaturalWaterTerrainGrid[index] = water;
			else
			{
				if (sendMessage)
					Messages.Message("Attempted to set natural water and water depth to max for non-water (or unsupported water) terrain.", MessageTypeDefOf.RejectInput);
				return false; //Abort, not water.
			}
			var extension = WaterFreezesStatCache.GetExtension(water);
			comp.WaterDepthGrid[index] = extension.MaxWaterDepth;
			comp.UpdateIceStage(cell, extension);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void DoForRect(Func<Map, IntVec3, bool, bool> action)
        {
			Map map = Find.CurrentMap;
			DebugTool tool = null;
			IntVec3 firstCorner;
			tool = new DebugTool("first corner...", delegate
			{
				firstCorner = UI.MouseCell();
				DebugTools.curTool = new DebugTool("second corner...", delegate
				{
					int failures = 0;
					int cellCount = 0;
					IntVec3 secondCorner = UI.MouseCell();
					foreach (IntVec3 cell in CellRect.FromLimits(firstCorner, secondCorner).ClipInsideMap(map))
					{
						if (!action(map, cell, false))
							failures++;
						cellCount++;
					}
					DebugTools.curTool = tool;
					if (failures > 0)
						Messages.Message("There were " + failures + " failures to perform the requested operation on " + cellCount + " cells.", MessageTypeDefOf.TaskCompletion);
					else
						Messages.Message("Successfully performed the operation on " + cellCount + " cells.", MessageTypeDefOf.TaskCompletion);
				}, firstCorner);
			});
			DebugTools.curTool = tool;
		}
	}
}