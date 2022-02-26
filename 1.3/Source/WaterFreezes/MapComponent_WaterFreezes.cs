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
    public class MapComponent_WaterFreezes : MapComponent
    {
		public bool Initialized;
        public TerrainDef[] NaturalWaterTerrainGrid;
		public TerrainDef[] AllWaterTerrainGrid;
		public float[] IceDepthGrid;
		public float[] WaterDepthGrid;
		public float[] PseudoWaterElevationGrid;
		//Ice thresholds of type by depth.
		public float ThresholdThinIce = .15f; //This is ratio of ice to water, unlike other thresholds.
		public float ThresholdIce = 50;
		public float ThresholdThickIce = 110;
		//Used for lakes *and* rivers.
		public float MaxWaterDeep = 400;
		public float MaxWaterShallow = 100;
		public float MaxIceDeep = 120;
		public float MaxIceShallow = 100;
		//Used for marshes.
		public float MaxWaterMarsh = 70;
		public float MaxIceMarsh = 70;

		public MapComponent_WaterFreezes(Map map) : base(map)
		{
			WaterFreezes.Log("New MapComponent constructed (for map " + map.uniqueID + ") adding it to the cache.");
			WaterFreezesCompCache.SetFor(map, this);
		}

        public override void MapGenerated()
		{
			Initialize();
		}

        public override void MapRemoved()
        {
			WaterFreezes.Log("Removing MapComponent from cache due to map removal (for map " + map.uniqueID + ").");
			WaterFreezesCompCache.compCachePerMap.Remove(map.uniqueID); //Yeet from cache so it can die in the GC.
        }

        public void Initialize()
		{
			WaterFreezes.Log("MapComponent Initializing (for map uniqueId " + map.uniqueID + "\")..");
			if (WaterDepthGrid == null) //If we have no water depth grid..
			{
				WaterFreezes.Log("Instantiating water depth grid..");
				WaterDepthGrid = new float[map.cellIndices.NumGridCells]; //Instantiate it.
			}
			if (NaturalWaterTerrainGrid == null) //If we haven't got a waterGrid loaded from the save file, make one.
			{
				WaterFreezes.Log("Generating natural water grid and populating water depth grid..");
				NaturalWaterTerrainGrid = new TerrainDef[map.cellIndices.NumGridCells];
				for (int i = 0; i < map.cellIndices.NumGridCells; i++)
				{
					var t = map.terrainGrid.TerrainAt(i);
					if (t == TerrainDefOf.WaterDeep || t == TerrainDefOf.WaterMovingChestDeep)
					{
						NaturalWaterTerrainGrid[i] = t;
						WaterDepthGrid[i] = MaxWaterDeep;
					}
					else if (t == TerrainDefOf.WaterShallow || t == TerrainDefOf.WaterMovingShallow)
                    {
						NaturalWaterTerrainGrid[i] = t;
						WaterDepthGrid[i] = MaxWaterShallow;
                    }
					else if (t == WaterDefs.Marsh)
                    {
						NaturalWaterTerrainGrid[i] = t;
						WaterDepthGrid[i] = MaxWaterMarsh;
                    }
					else if (t.IsBridge())
                    {
						var ut = map.terrainGrid.UnderTerrainAt(i);
						if (ut.IsWater)
						{
							NaturalWaterTerrainGrid[i] = ut; 
							if (ut == TerrainDefOf.WaterDeep || ut == TerrainDefOf.WaterMovingChestDeep)
								WaterDepthGrid[i] = MaxWaterDeep;
							else if (ut == TerrainDefOf.WaterShallow || ut == TerrainDefOf.WaterMovingShallow)
								WaterDepthGrid[i] = MaxWaterShallow;
							else if (ut == WaterDefs.Marsh)
								WaterDepthGrid[i] = MaxWaterMarsh;
						}
                    }
				}
			}
			if (AllWaterTerrainGrid == null) //If we have no all-water terrain grid..
			{
				WaterFreezes.Log("Cloning natural water grid into all water grid..");
				AllWaterTerrainGrid = (TerrainDef[])NaturalWaterTerrainGrid.Clone(); //Instantiate it to content of the natural water array for starters.
			}
			if (IceDepthGrid == null)
			{
				WaterFreezes.Log("Instantiating ice depth grid..");
				IceDepthGrid = new float[map.cellIndices.NumGridCells];
			}
			if (PseudoWaterElevationGrid == null)
			{
				PseudoWaterElevationGrid = new float[map.cellIndices.NumGridCells];
				UpdatePseudoWaterElevationGrid();
			}
			Initialized = true;
		}

		public override void MapComponentTick()
		{
			if (!Initialized) //If we aren't initialized..
				Initialize(); //Initialize it!
			if (Find.TickManager.TicksGame % WaterFreezesSettings.IceRate != 0) //If it's not once per hour..
				return; //Don't execute the rest, throttling measure.
			for (int i = 0; i < NaturalWaterTerrainGrid.Length; i++) //Thread this later probably.
			{
				var cell = map.cellIndices.IndexToCell(i);
				var currentTerrain = cell.GetTerrain(map); //Get current terrain.
				//If it's lake ice or it's water, or it's a natural water spot..
				if (currentTerrain == IceDefs.WF_LakeIceThin ||
					currentTerrain == IceDefs.WF_LakeIce ||
					currentTerrain == IceDefs.WF_LakeIceThick ||
					currentTerrain == IceDefs.WF_MarshIceThin ||
					currentTerrain == IceDefs.WF_MarshIce ||
					currentTerrain == IceDefs.WF_RiverIceThin ||
					currentTerrain == IceDefs.WF_RiverIce ||
					currentTerrain == IceDefs.WF_RiverIceThick ||
					currentTerrain == TerrainDefOf.WaterShallow ||
					currentTerrain == TerrainDefOf.WaterDeep ||
					currentTerrain == TerrainDefOf.WaterMovingShallow ||
					currentTerrain == TerrainDefOf.WaterMovingChestDeep ||
					currentTerrain == WaterDefs.Marsh ||
					AllWaterTerrainGrid[i] != null)
				{
					UpdateIceForTemperature(cell);
					UpdateIceStage(cell, currentTerrain);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdatePseudoWaterElevationGrid()
        {
			WaterFreezes.Log("Updating pseudo water elevation grid..");
			for (int i = 0; i < AllWaterTerrainGrid.Length; i++)
				if (AllWaterTerrainGrid[i] != null)
					UpdatePseudoWaterElevationGridForCell(map.cellIndices.IndexToCell(i));
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdatePseudoWaterElevationGridForCell(IntVec3 cell)
		{
			int i = map.cellIndices.CellToIndex(cell);
			var adjacentCells = GenAdjFast.AdjacentCells8Way(cell);
			float pseudoElevationScore = 0;
			for (int j = 0; j < adjacentCells.Count; j++)
			{
				int adjacentCellIndex = map.cellIndices.CellToIndex(adjacentCells[j]);
				if (adjacentCellIndex < 0 || adjacentCellIndex >= map.terrainGrid.topGrid.Length) //If it's a negative index or it's a larger index than the map's grid length (faster to get topGrid.Length than use property on the cellIndices).
					continue; //Skip it.
				if (AllWaterTerrainGrid[adjacentCellIndex] == null) //If it's land (e.g., not recognized water)..
					pseudoElevationScore += 1; //+1 for each land
			}
			PseudoWaterElevationGrid[i] = pseudoElevationScore;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdatePseudoWaterElevationGridAtAndAroundCell(IntVec3 cell)
		{
			int i = map.cellIndices.CellToIndex(cell);
			var adjacentCells = GenAdjFast.AdjacentCells8Way(cell);
			float pseudoElevationScore = 0;
			for (int j = 0; j < adjacentCells.Count; j++)
			{
				int adjacentCellIndex = map.cellIndices.CellToIndex(adjacentCells[j]);
				if (adjacentCellIndex < 0 || adjacentCellIndex >= map.terrainGrid.topGrid.Length) //If it's a negative index or it's a larger index than the map's grid length (faster to get topGrid.Length than use property on the cellIndices).
					continue; //Skip it.
				if (AllWaterTerrainGrid[adjacentCellIndex] == null) //If it's land (e.g., not recognized water)..
					pseudoElevationScore += 1; //+1 for each land
				else
					UpdatePseudoWaterElevationGridForCell(cell); //Update for this cell as well.
			}
			PseudoWaterElevationGrid[i] = pseudoElevationScore;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetMaxWaterByDef(int i, TerrainDef water = null, bool updateIceStage = true)
		{
			if (water == null) //Was not passed in..
				water = AllWaterTerrainGrid[i]; //Get it.
			float maxForTerrain = 0;
			if (water.IsShallowDepth())
				maxForTerrain = MaxWaterShallow;
			else if (water.IsDeepDepth())
				maxForTerrain = MaxWaterDeep;
			else if (water == WaterDefs.Marsh)
				maxForTerrain = MaxWaterMarsh;
			WaterDepthGrid[i] = maxForTerrain;
			if (updateIceStage)
				UpdateIceStage(map.cellIndices.IndexToCell(i));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateIceForTemperature(IntVec3 cell)
		{
			var temperature = GenTemperature.GetTemperatureForCell(cell, map);
			int i = map.cellIndices.CellToIndex(cell);
			var water = AllWaterTerrainGrid[i];
			if (water.IsRiverWater() || water.IsRiverIce()) //If it's part of a river..
				temperature += 10; //Offset by 10 degrees because the moving water introduces kinetic energy.
			if (temperature == 0) //If it's 0degC..
				return; //We don't update when ambiguous.
			if (temperature < 0) //Temperature is below zero..
			{
				var currentWater = WaterDepthGrid[i];
				if (currentWater > 0)
				{
					if (PseudoWaterElevationGrid[i] == 0) //if this isn't one of the edge cells..
					{
						//If none of the adjacent cells have ice..
						if (!GenAdjFast.AdjacentCells8Way(cell).Any(adjacentCell =>
						{
							int adjacentCellIndex = map.cellIndices.CellToIndex(adjacentCell);
							if (adjacentCellIndex < 0 || adjacentCellIndex >= map.terrainGrid.topGrid.Length) //If it's a negative index or it's a larger index than the map's grid length (faster to get topGrid.Length than use property on the cellIndices).
								return false;
							var adjacentCellTerrain = map.terrainGrid.TerrainAt(adjacentCellIndex);
							return adjacentCellTerrain.IsThawableIce();
						}))
							return; //We aren't going to freeze before there's ice adjacent to us.
					}
					var change = -temperature //Based on negated temperature..
						* (WaterFreezesSettings.FreezingFactor + PseudoWaterElevationGrid[i]) //But sped up by a multiplier which takes into account surrounding terrain.
						/ 2500f * WaterFreezesSettings.IceRate; //Adjust to iceRate based on the 2500 we tuned it to originally.
																//WaterFreezes.Log("Freezing cell " + cell.ToString() + " for " + change + " amount, prior, ice was " + IceDepthGrid[i] + " and water was " + WaterDepthGrid[i]);
					var currentIce = IceDepthGrid[i] += change; //Ice goes up..
					if (water.IsShallowDepth())
						if (currentIce > MaxIceShallow)
							IceDepthGrid[i] = MaxIceShallow;
					else if (water.IsDeepDepth()) 
						if (currentIce > MaxIceDeep)
							IceDepthGrid[i] = MaxIceDeep;
					else if (water.IsMarsh())
						if (currentIce > MaxIceMarsh)
							IceDepthGrid[i] = MaxIceMarsh;
					if ((WaterDepthGrid[i] -= change) < 0) //Water depth goes down.. if that value is less than zero now, then..
						WaterDepthGrid[i] = 0;
					//WaterFreezes.Log("For cell " + cell.ToString() + " after changes (and clamping), ice was " + IceDepthGrid[i] + " and water was " + WaterDepthGrid[i]);
				}
			}
			else if (temperature > 0) //Temperature is above zero..
			{
				var currentIce = IceDepthGrid[i];
				if (currentIce > 0)
				{
					var change = temperature //Based on temperature..
						/ (WaterFreezesSettings.ThawingFactor + PseudoWaterElevationGrid[i]) / //But slowed down by a divisor which takes into account surrounding terrain.
						(currentIce / 100f) //Slow thawing further by ice thickness per 100 ice.
						/ 2500f * WaterFreezesSettings.IceRate; //Adjust to iceRate based on the 2500 we tuned it to originally.
					currentIce = IceDepthGrid[i] -= change; //Ice goes down..
					if (currentIce < 0)
						IceDepthGrid[i] = 0;
					else //Only mess with water grid if ice grid had ice to melt.
					{
						var currentWater = WaterDepthGrid[i] += change; //Water depth goes up..
						if (currentWater > MaxWaterShallow && water.IsShallowDepth()) //If shallow underneath and too much water,
							WaterDepthGrid[i] = MaxWaterShallow; //Cap it.
						else if (currentWater > MaxWaterDeep && water.IsDeepDepth()) //If deep underneath and too much water,
							WaterDepthGrid[i] = MaxWaterDeep; //Cap it.
						else if (currentWater > MaxWaterMarsh && water.IsMarsh()) //If marsh underneath and too much water,
							WaterDepthGrid[i] = MaxWaterMarsh; //Cap it.
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateIceStage(IntVec3 cell, TerrainDef currentTerrain = null, TerrainDef underTerrain = null)
        {
			int i = map.cellIndices.CellToIndex(cell);
			float iceDepth = IceDepthGrid[i];
			float waterDepth = WaterDepthGrid[i];
			var water = AllWaterTerrainGrid[i];
			if (currentTerrain == null) //If it wasn't passed in..
				currentTerrain = map.terrainGrid.TerrainAt(i);// map.terrainGrid.topGrid[i]; //Get it.
			if (underTerrain == null)
				underTerrain = map.terrainGrid.UnderTerrainAt(i);
			var appropriateTerrain = GetAppropriateTerrainFor(cell, water, waterDepth, iceDepth);
			if (currentTerrain.IsBridge() || (TerrainSystemOverhaul_Interop.TerrainSystemOverhaulPresent && TerrainSystemOverhaul_Interop.GetBridge(map.terrainGrid, cell) != null)) //If it's a bridge..
			{
				//We deal with underTerrain.
				if (underTerrain != appropriateTerrain)
					map.terrainGrid.SetUnderTerrain(cell, appropriateTerrain);
				else
					CheckAndRefillCell(cell, currentTerrain, underTerrain);
			}
			else //Not a bridge..
			{ 
				//We deal with regular terrain.
				if (currentTerrain != appropriateTerrain)
					map.terrainGrid.SetTerrain(cell, appropriateTerrain);
				else
					CheckAndRefillCell(cell, currentTerrain, underTerrain);
			}
			BreakdownOrDestroyBuildingsInCellIfInvalid(cell);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public TerrainDef GetAppropriateTerrainFor(IntVec3 cell, TerrainDef waterTerrain, float waterDepth, float iceDepth)
		{
			var isLake = waterTerrain.IsLakeWater();
			var isMarsh = waterTerrain == WaterDefs.Marsh;
			var isRiver = waterTerrain.IsRiverWater();
			var percentIce = iceDepth / (iceDepth + waterDepth);
			if (float.IsNaN(percentIce) || percentIce < ThresholdThinIce) //If there's no meaningful amount of ice.. (the IsNaN is for the case where 0/0)
				return waterTerrain;
			else if (iceDepth < ThresholdIce) //If there's ice, but it's below the regular ice depth threshold..
			{
				if (isLake)
					return IceDefs.WF_LakeIceThin;
				else if (isMarsh)
					return IceDefs.WF_MarshIceThin;
				else if (isRiver)
					return IceDefs.WF_RiverIceThin;
			}
			else if (iceDepth < ThresholdThickIce) //If it's between regular ice and thick ice in depth..
			{
				if (isLake)
					return IceDefs.WF_LakeIce;
				else if (isMarsh)
					return IceDefs.WF_MarshIce;
				else if (isRiver)
					return IceDefs.WF_RiverIce;
			}
			else //Only thick left..
			{
				//Note, there is no thick marsh ice.
				if (isLake)
					return IceDefs.WF_LakeIceThick;
				else if (isRiver)
					return IceDefs.WF_RiverIceThick;
			}
			WaterFreezes.Log("Reached the end of GetAppropriateTerrainFor without finding anything, this should not happen!");
			return waterTerrain;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CheckAndRefillCell(IntVec3 cell, TerrainDef currentTerrain = null, TerrainDef underTerrain = null)
		{
			if (cell.GetTemperature(map) <= 0) //If it's at or below freezing..
				return; //Do not thaw!
			int i = map.cellIndices.CellToIndex(cell);
			if (currentTerrain == null) //If it wasn't passed in..
				currentTerrain = cell.GetTerrain(map); //Get it.
			if (underTerrain == null) //If it wasn't passed in..
				underTerrain = map.terrainGrid.UnderTerrainAt(i); //Get it.
			var naturalWater = NaturalWaterTerrainGrid[i];
			if (naturalWater != null) //If it's natural water..
			{
				var season = GenLocalDate.Season(map);
				if (season == Season.Spring || season == Season.Summer || season == Season.PermanentSummer) //If it's the right season..
				{
					//Refill the cell..
					if (naturalWater == TerrainDefOf.WaterDeep || naturalWater == TerrainDefOf.WaterMovingChestDeep) //It's deep water when present..
					{
						if (WaterDepthGrid[i] < MaxWaterDeep) //If it's not over-full..
							WaterDepthGrid[i] += 1f / 2500f * WaterFreezesSettings.IceRate; //Fill
						if (WaterDepthGrid[i] > MaxWaterDeep) //If it's too full..
							WaterDepthGrid[i] = MaxWaterDeep; //Cap it.
					}
					else if (naturalWater == TerrainDefOf.WaterShallow || naturalWater == TerrainDefOf.WaterMovingShallow) //It's shallow water when present..
					{
						if (WaterDepthGrid[i] < MaxWaterShallow) //If it's not over-full..
							WaterDepthGrid[i] += 1f / 2500f * WaterFreezesSettings.IceRate; //Fill
						if (WaterDepthGrid[i] > MaxWaterShallow) //If it's too full..
							WaterDepthGrid[i] = MaxWaterShallow; //Cap it.
					}
					else if (naturalWater == WaterDefs.Marsh) //It's marsh when present..
					{
						if (WaterDepthGrid[i] < MaxWaterMarsh) //If it's not over-full..
							WaterDepthGrid[i] += 1f / 2500f * WaterFreezesSettings.IceRate; //Fill
						if (WaterDepthGrid[i] > MaxWaterMarsh) //If it's too full..
							WaterDepthGrid[i] = MaxWaterMarsh; //Cap it.
                    }
				}
			}
		}

		public List<string> BreakdownOrDestroyExceptedDefNames = new()
		{
			"Shuttle",
			"ShuttleCrashed",
		};

		public List<string> BreakdownOrDestroyExceptedPlaceWorkerTypeStrings = new()
		{
			"RimWorld.PlaceWorker_Conduit",
		};

		public List<string> BreakdownOrDestroyExceptedPlaceWorkerFailureReasons = new()
		{
			"VPE_NeedsDistance".Translate(), //If it's a tidal generator trying to see if it's too close to itself..
			"WFFT_NeedsDistance".Translate(), //If it's a fish trap or fish net trying to see if it's too close to itself..
		};

		public void BreakdownOrDestroyBuildingsInCellIfInvalid(IntVec3 cell)
		{
			var terrain = cell.GetTerrain(map);
			var things = cell.GetThingList(map);
			for (int i = 0; i < things.Count; i++)
			{
				var thing = things[i];
				if (thing == null)
					continue; //Can't work on a null thing!
				bool dueToAffordances = false;
				bool shouldBreakdownOrDestroy = false;
				if (thing is Building building && thing.def.destroyable)
				{
					if ((thing.questTags != null && thing.questTags.Count > 0) || //If it's marked for a quest..
						(thing.def.defName.StartsWith("Ancient") || thing.def.defName.StartsWith("VFEA_")) || //If it's ancient stuff..
						BreakdownOrDestroyExceptedDefNames.Contains(thing.def.defName)) //Or if it's in the list of things to skip..
						continue; //Skip this one.
					if (thing.def.PlaceWorkers != null)
						foreach (PlaceWorker pw in thing.def.PlaceWorkers)
						{
							if (BreakdownOrDestroyExceptedPlaceWorkerTypeStrings.Contains(pw.ToString())) //If it's in the list to skip..
								continue; //Skip this one.
							var acceptanceReport = pw.AllowsPlacing(thing.def, thing.Position, thing.Rotation, map);
							if (!acceptanceReport) //Failed PlaceWorker
							{
								if (BreakdownOrDestroyExceptedPlaceWorkerFailureReasons.Contains(acceptanceReport.Reason)) //If it's a reason we don't care about.
									continue; //Don't destroy for this particular reason, irrelevant.
								Log.Message("PlaceWorker failed with reason: " + acceptanceReport.Reason);
								shouldBreakdownOrDestroy = true; 
								break; //We don't need to check more if we've found a reason to not be here.
							}
						}
					//Had no PlaceWorkers or it passed all their checks but it has an affordance that isn't being met.
					if (thing.TerrainAffordanceNeeded != null &&
						thing.TerrainAffordanceNeeded.defName != "" &&
						terrain.affordances != null &&
						!terrain.affordances.Contains(thing.TerrainAffordanceNeeded))
					{
						shouldBreakdownOrDestroy = true;
						dueToAffordances = true;
					}
					if (shouldBreakdownOrDestroy)
					{
						if (thing is ThingWithComps twc) //If it has comps..
						{
							var flickable = twc.GetComp<CompFlickable>();
							var breakdown = twc.GetComp<CompBreakdownable>();
							if (flickable != null && breakdown != null) //If it has both comps..
							{
								if (flickable.SwitchIsOn && !breakdown.BrokenDown) //If it's on and it isn't broken down..
								{
									breakdown.DoBreakdown(); //Cause breakdown.
									flickable.DoFlick(); //Turn it off.
								}
							}
							else if (!(dueToAffordances && terrain.IsWater) && breakdown != null) //It has breakdown but not flickable and this is not due to ice->water lacking affordance.
							{
								if (!breakdown.BrokenDown) //If it isn't already broken down..
									breakdown.DoBreakdown(); //Cause breakdown.
							}
							else //It has either only flickable or neither, or it's got only breakdown but it's due to water->ice lacking affordance.
								thing.Destroy(DestroyMode.FailConstruction);
						}
						else //No comps..
							thing.Destroy(DestroyMode.FailConstruction);
					}
				}
			}
        }

		public override void ExposeData()
		{
			List<float> iceDepthGridList = new List<float>();
			List<float> waterDepthGridList = new List<float>();
			List<float> pseudoElevationGridList = new List<float>();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				List<string> naturalWaterTerrainGridStringList = new List<string>();
				List<string> allWaterTerrainGridStringList = new List<string>();
				if (AllWaterTerrainGrid != null)
					allWaterTerrainGridStringList = AllWaterTerrainGrid.Select(def => def == null ? "null" : def.defName).ToList();
				if (NaturalWaterTerrainGrid != null)
					naturalWaterTerrainGridStringList = NaturalWaterTerrainGrid.Select(def => def == null ? "null" : def.defName).ToList();
				if (IceDepthGrid != null)
					iceDepthGridList = IceDepthGrid.ToList();
				if (WaterDepthGrid != null)
					waterDepthGridList = WaterDepthGrid.ToList();
				if (PseudoWaterElevationGrid != null)
					pseudoElevationGridList = PseudoWaterElevationGrid.ToList();
				Scribe_Collections.Look(ref naturalWaterTerrainGridStringList, "NaturalWaterTerrainGrid");
				Scribe_Collections.Look(ref allWaterTerrainGridStringList, "AllWaterTerrainGrid");
            }
			Scribe_Collections.Look(ref iceDepthGridList, "IceDepthGrid");
			Scribe_Collections.Look(ref waterDepthGridList, "WaterDepthGrid");
			Scribe_Collections.Look(ref pseudoElevationGridList, "PseudoElevationGrid");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				List<TerrainDef> naturalWaterTerrainGridList = new List<TerrainDef>();
				List<TerrainDef> allWaterTerrainGridList = new List<TerrainDef>();
				Scribe_Collections.Look(ref naturalWaterTerrainGridList, "NaturalWaterTerrainGrid");
				Scribe_Collections.Look(ref allWaterTerrainGridList, "AllWaterTerrainGrid");
				NaturalWaterTerrainGrid = naturalWaterTerrainGridList.ToArray();
				AllWaterTerrainGrid = allWaterTerrainGridList.ToArray();
				IceDepthGrid = iceDepthGridList.ToArray();
				WaterDepthGrid = waterDepthGridList.ToArray();
				PseudoWaterElevationGrid = pseudoElevationGridList.ToArray();
			}
			base.ExposeData();
        }
	}
}