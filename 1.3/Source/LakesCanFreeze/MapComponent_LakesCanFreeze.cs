using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using System.Runtime.CompilerServices;

namespace LCF
{
    public class MapComponent_LakesCanFreeze : MapComponent
    {
		bool init;
        public TerrainDef[] NaturalWaterTerrainGrid;
		public TerrainDef[] AllWaterTerrainGrid;
		public float[] IceDepthGrid;
		public float[] WaterDepthGrid;
		public float[] PseudoWaterElevationGrid;
		//MapGenFloatGrid elevation;
		//MapGenFloatGrid fertility;
		float thresholdThinIce = 25;
		float thresholdIce = 100;
		float thresholdThickIce = 200;
		int freezingMultiplier = 4;
		int thawingMultiplier = 2;
		float iceRate = 2500;
		float maxWaterDeep = 400;
		float maxWaterShallow = 100;
		float maxIceDeep = 400;
		float maxIceShallow = 100;

		public MapComponent_LakesCanFreeze(Map map) : base(map)
		{

		}

        public override void MapGenerated()
		{
			Initialize();
		}

		public void Initialize()
        {
			Log.Message("[LakesCanFreeze] Initializing..");
			//if (map.AgeInDays == 0)
			//{
			//	Map fakeMap = new Map() { uniqueID = map.uniqueID, info = map.info, cellIndices = map.cellIndices }; //Super secret fake map.
			//	MapGenerator.mapBeingGenerated = fakeMap;
			//	RockNoises.Init(fakeMap);
			//	new GenStep_ElevationFertility().Generate(fakeMap, new GenStepParams()); //Parms aren't used.
			//	RockNoises.Reset();
			//	//Preserve these, when another map is gen'd they'll be discarded!
			//	elevation = MapGenerator.Elevation;
			//	fertility = MapGenerator.Fertility;
			//	MapGenerator.mapBeingGenerated = map; //Put it back, don't be rude!
			//}
			if (WaterDepthGrid == null) //If we have no water depth grid..
			{
				Log.Message("[LakesCanFreeze] Instantiating water depth grid..");
				WaterDepthGrid = new float[map.cellIndices.NumGridCells]; //Instantiate it.
			}
			if (NaturalWaterTerrainGrid == null) //If we haven't got a waterGrid loaded from the save file, make one.
			{
				Log.Message("[LakesCanFreeze] Generating natural water grid and populating water depth grid..");
				NaturalWaterTerrainGrid = new TerrainDef[map.cellIndices.NumGridCells];
				for (int i = 0; i < map.cellIndices.NumGridCells; i++)
				{
					var c = map.cellIndices.IndexToCell(i);
					var t = c.GetTerrain(map);
					if (t == TerrainDefOf.WaterDeep)
					{
						NaturalWaterTerrainGrid[i] = t;
						WaterDepthGrid[i] = maxWaterDeep;
					}
					else if (t == TerrainDefOf.WaterShallow)
                    {
						NaturalWaterTerrainGrid[i] = t;
						WaterDepthGrid[i] = maxWaterShallow;
                    }
				}
			}
			if (AllWaterTerrainGrid == null) //If we have no all-water terrain grid..
			{
				Log.Message("[LakesCanFreeze] Cloning natural water grid into all water grid..");
				AllWaterTerrainGrid = (TerrainDef[])NaturalWaterTerrainGrid.Clone(); //Instantiate it to content of the natural water array for starters.
			}
			if (IceDepthGrid == null)
			{
				Log.Message("[LakesCanFreeze] Instantiating ice depth grid..");
				IceDepthGrid = new float[map.cellIndices.NumGridCells];
			}
			if (PseudoWaterElevationGrid == null)
			{
				Log.Message("[LakesCanFreeze] Generating pseudo water elevation grid..");
				PseudoWaterElevationGrid = new float[map.cellIndices.NumGridCells];
				UpdatePseudoWaterElevationGrid();
			}
			init = true;
		}

		public override void MapComponentTick()
		{
			if (!init) //If we aren't initialized..
				Initialize(); //Initialize it!
			if (Find.TickManager.TicksGame % iceRate != 0) //If it's not once per hour..
				return; //Don't execute the rest, throttling measure.
			for (int i = 0; i < NaturalWaterTerrainGrid.Length; i++) //Thread this later probably.
			{
				var cell = map.cellIndices.IndexToCell(i);
				var currentTerrain = cell.GetTerrain(map); //Get current terrain.
				//If it's lake ice or it's water, or it's a natural water spot..
				if (currentTerrain == IceDefs.LCF_LakeIceThin || 
					currentTerrain == IceDefs.LCF_LakeIce || 
					currentTerrain == IceDefs.LCF_LakeIceThick || 
					currentTerrain == TerrainDefOf.WaterShallow || 
					currentTerrain == TerrainDefOf.WaterDeep ||
					AllWaterTerrainGrid[i] != null)
				{
					UpdateIceForTemperature(cell, currentTerrain);
					UpdateIceStage(cell, currentTerrain);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdatePseudoWaterElevationGrid()
        {
			Log.Message("[LakesCanFreeze] Updating pseudo water elevation grid..");
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
				if (AllWaterTerrainGrid[adjacentCellIndex] == null) //If it's land..
					pseudoElevationScore += 1; //+4 for each land
			}
			PseudoWaterElevationGrid[i] = pseudoElevationScore;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsTerrainDef(int i, Map map, TerrainDef def, TerrainDef currentTerrain = null, TerrainDef underTerrain = null)
		{
			if (currentTerrain == null) //If it wasn't passed in..
				currentTerrain = map.terrainGrid.TerrainAt(i); //Get it.
			if (underTerrain == null) //If it wasn't passed in..
				underTerrain = map.terrainGrid.UnderTerrainAt(i); //Get it.
			return currentTerrain == def || underTerrain == def || AllWaterTerrainGrid[i] == def;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsShallowWater(int i, Map map, TerrainDef currentTerrain = null, TerrainDef underTerrain = null)
        {
			return IsTerrainDef(i, map, TerrainDefOf.WaterShallow, currentTerrain, underTerrain);
        }


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsDeepWater(int i, Map map, TerrainDef currentTerrain = null, TerrainDef underTerrain = null)
        {
			return IsTerrainDef(i, map, TerrainDefOf.WaterDeep, currentTerrain, underTerrain);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsLakeIce(int i, Map map, TerrainDef currentTerrain = null, TerrainDef underTerrain = null)
		{
			if (currentTerrain == null) //If it wasn't passed in..
				currentTerrain = map.terrainGrid.TerrainAt(i); //Get it.
			if (underTerrain == null) //If it wasn't passed in..
				underTerrain = map.terrainGrid.UnderTerrainAt(i); //Get it.
			return IsTerrainDef(i, map, IceDefs.LCF_LakeIceThin, currentTerrain, underTerrain) || 
				   IsTerrainDef(i, map, IceDefs.LCF_LakeIce, currentTerrain, underTerrain) ||
				   IsTerrainDef(i, map, IceDefs.LCF_LakeIceThick, currentTerrain, underTerrain);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsWater(int i, Map map, TerrainDef currentTerrain = null, TerrainDef underTerrain = null)
		{
			if (currentTerrain == null) //If it wasn't passed in..
				currentTerrain = map.terrainGrid.TerrainAt(i); //Get it.
			if (underTerrain == null) //If it wasn't passed in..
				underTerrain = map.terrainGrid.UnderTerrainAt(i); //Get it.
			return IsShallowWater(i, map, currentTerrain, underTerrain) ||
				   IsDeepWater(i, map, currentTerrain, underTerrain);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateIceForTemperature(IntVec3 cell, TerrainDef currentTerrain = null, TerrainDef underTerrain = null)
		{
			var temperature = GenTemperature.GetTemperatureForCell(cell, map);
			if (temperature == 0) //If it's 0degC..
				return; //We don't update when ambiguous.
			int i = map.cellIndices.CellToIndex(cell);
			if (temperature < 0) //Temperature is below zero..
			{
				if (WaterDepthGrid[i] > 0)
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
							return adjacentCellTerrain == IceDefs.LCF_LakeIceThin ||
								   adjacentCellTerrain == IceDefs.LCF_LakeIce ||
								   adjacentCellTerrain == IceDefs.LCF_LakeIceThick;
						}))
							return; //We aren't going to freeze before there's ice adjacent to us.
					}
					var change = -temperature * (freezingMultiplier + PseudoWaterElevationGrid[i]) * // //Based on temperature but sped up by a multiplier which takes into account surrounding terrain.
						//(WaterDepthGrid[i] / 100) //* //Slow freezing by water depth per 100 water.
						(currentTerrain == TerrainDefOf.WaterDeep ? .5f : 1f) * //If it's deep water right now, slow it down more.
						(.9f + (.1f * Rand.Value)) //10% of the rate is variable for flavor.
						/ 2500 * iceRate; //Adjust to iceRate based on the 2500 we tuned it to originally.
					IceDepthGrid[i] += change; //Ice goes up..
					if (IsShallowWater(i, map, currentTerrain, underTerrain))
						if (IceDepthGrid[i] > maxIceShallow)
							IceDepthGrid[i] = maxIceShallow;
					if (IsDeepWater(i, map, currentTerrain, underTerrain))
						if (IceDepthGrid[i] > maxIceDeep)
							IceDepthGrid[i] = maxIceDeep;
					WaterDepthGrid[i] -= change; //Water depth goes down..
					if (WaterDepthGrid[i] < 0)
						WaterDepthGrid[i] = 0;
				}
			}
			else if (temperature > 0) //Temperature is above zero..
			{
				if (IceDepthGrid[i] > 0)
				{
					var change = temperature / (thawingMultiplier + PseudoWaterElevationGrid[i]) / // //Based on temperature but slowed down by a multiplier which takes into account surrounding terrain.
						(IceDepthGrid[i] / 100) * //Slow thawing further by ice thickness per 100 ice.
						(currentTerrain == IceDefs.LCF_LakeIceThick ? .5f : 1f) *  //If it's thick ice right now, slow it down more.
						(currentTerrain == IceDefs.LCF_LakeIce ? .75f : 1f) * //If it's regular ice right now, slow it down a little less than thick.
						(.9f + (.1f * Rand.Value)) //10% of the rate is variable for flavor.
						/ 2500 * iceRate; //Adjust to iceRate based on the 2500 we tuned it to originally.
					IceDepthGrid[i] -= change; //Ice goes down..
					if (IceDepthGrid[i] < 0)
						IceDepthGrid[i] = 0;
					else //Only mess with water grid if ice grid had ice to melt.
					{
						WaterDepthGrid[i] += change; //Water depth goes up..
						if (IsShallowWater(i, map, currentTerrain, underTerrain) && WaterDepthGrid[i] > maxWaterShallow) //If shallow underneath and too much water,
							WaterDepthGrid[i] = maxWaterShallow; //Cap it.
						if (IsDeepWater(i, map, currentTerrain, underTerrain) && WaterDepthGrid[i] > maxWaterDeep) //If deep underneath and too much water,
							WaterDepthGrid[i] = maxWaterDeep; //Cap it.
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdateIceStage(IntVec3 cell, TerrainDef currentTerrain = null)
        {
			int i = map.cellIndices.CellToIndex(cell);
			float ice = IceDepthGrid[i];
			if (ice < thresholdThinIce)
				ThawCell(cell);
			else if (ice < thresholdIce)
			{
				if (currentTerrain == null) //If it wasn't passed in..
					currentTerrain = map.terrainGrid.topGrid[i]; //Get it.
				if (currentTerrain == TerrainDefOf.WaterDeep || currentTerrain == TerrainDefOf.WaterShallow) //If it's water then it's freezing now..
					map.terrainGrid.SetUnderTerrain(cell, currentTerrain); //Store the water in under-terrain.
				map.terrainGrid.SetTerrain(cell, IceDefs.LCF_LakeIceThin);
			}
			else if (ice < thresholdThickIce)
				map.terrainGrid.SetTerrain(cell, IceDefs.LCF_LakeIce);
			else
				map.terrainGrid.SetTerrain(cell, IceDefs.LCF_LakeIceThick);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ThawCell(IntVec3 cell, TerrainDef currentTerrain = null, TerrainDef underTerrain = null)
        {
			int i = map.cellIndices.CellToIndex(cell);
			if (currentTerrain == null) //If it wasn't passed in..
				currentTerrain = cell.GetTerrain(map); //Get it.
			if (underTerrain == null) //If it wasn't passed in..
				underTerrain = map.terrainGrid.UnderTerrainAt(i);
			if (NaturalWaterTerrainGrid[i] != null) //If it's natural water..
			{
				map.terrainGrid.SetTerrain(cell, NaturalWaterTerrainGrid[i]); //Make sure terrain is set to the right thing.
				DestroyBuildingsInCell(cell);
				var season = GenLocalDate.Season(map);
				if (season == Season.Spring || season == Season.Summer || season == Season.PermanentSummer) //If it's the right season..
				{
					//Refill the cell..
					if (NaturalWaterTerrainGrid[i] == TerrainDefOf.WaterDeep) //It's deep water when present..
					{
						if (WaterDepthGrid[i] < maxWaterDeep) //If it's not over-full..
							WaterDepthGrid[i] += 1 / 2500 * iceRate; //Fill
						if (WaterDepthGrid[i] > maxWaterDeep) //If it's too full..
							WaterDepthGrid[i] = maxWaterDeep; //Cap it.
					}
					else if (NaturalWaterTerrainGrid[i] == TerrainDefOf.WaterShallow) //It's shallow water when present..
					{
						if (WaterDepthGrid[i] < maxWaterShallow) //If it's not over-full..
							WaterDepthGrid[i] += 1 / 2500 * iceRate; //Fill
						if (WaterDepthGrid[i] > maxWaterShallow) //If it's too full..
							WaterDepthGrid[i] = maxWaterShallow; //Cap it.
					}
				}
			}
			else if (underTerrain != null && (underTerrain == TerrainDefOf.WaterShallow || underTerrain == TerrainDefOf.WaterDeep)) //If there was under-terrain and it's water.
			{
				if (WaterDepthGrid[i] > 0) //If there's water there..
				{
					map.terrainGrid.SetTerrain(cell, underTerrain); //Set the top layer to the under-terrain
					map.terrainGrid.SetUnderTerrain(cell, null); //Clear the under-terrain
					DestroyBuildingsInCell(cell);
				}
			}
		}

		/// <summary>
		/// Removes ice from a cell, updates its ice stage, and reports the amount of ice removed (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public float TakeCellIce(IntVec3 cell)
		{
			int i = map.cellIndices.CellToIndex(cell);
			var ice = IceDepthGrid[i];
			IceDepthGrid[i] = 0;
			UpdateIceStage(cell);
			return ice;
        }

		/// <summary>
		/// Returns the amount of ice in a cell (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public float QueryCellIce(IntVec3 cell)
        {
			return IceDepthGrid[map.cellIndices.CellToIndex(cell)];
		}

		/// <summary>
		/// Returns the amount of water in a cell (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public float QueryCellWater(IntVec3 cell)
		{
			return WaterDepthGrid[map.cellIndices.CellToIndex(cell)];
		}

		/// <summary>
		/// Returns the natural water def of a cell or null if not present (called from outside as part of the API).
		/// </summary>
		/// <param name="cell"></param>
		/// <returns></returns>
		public TerrainDef QueryCellNaturalWater(IntVec3 cell)
        {
			return NaturalWaterTerrainGrid[map.cellIndices.CellToIndex(cell)];
        }

		public void DestroyBuildingsInCell(IntVec3 cell)
        {
			var things = cell.GetThingList(map);
			for (int i = 0; i < things.Count; i++)
			{
				var thing = things[i];
				if (thing is Building && thing.def.destroyable)
					thing.Destroy(DestroyMode.Deconstruct);
			}
        }

        public override void ExposeData()
		{
			List<TerrainDef> naturalWaterTerrainGridList = new List<TerrainDef>();
			List<TerrainDef> allWaterTerrainGridList = new List<TerrainDef>();
			List<float> iceDepthGridList = new List<float>();
			List<float> waterDepthGridList = new List<float>();
			List<float> pseudoElevationGridList = new List<float>();
			if (Scribe.mode == LoadSaveMode.Saving)
            {
				if (AllWaterTerrainGrid != null)
					allWaterTerrainGridList = AllWaterTerrainGrid.ToList();
				if (NaturalWaterTerrainGrid != null)
					naturalWaterTerrainGridList = NaturalWaterTerrainGrid.ToList();
				if (IceDepthGrid != null)
					iceDepthGridList = IceDepthGrid.ToList();
				if (WaterDepthGrid != null)
					waterDepthGridList = WaterDepthGrid.ToList();
				if (PseudoWaterElevationGrid != null)
					pseudoElevationGridList = PseudoWaterElevationGrid.ToList();
            }
			Scribe_Collections.Look(ref naturalWaterTerrainGridList, "NaturalWaterTerrainGrid");
			Scribe_Collections.Look(ref allWaterTerrainGridList, "AllWaterTerrainGrid");
			Scribe_Collections.Look(ref iceDepthGridList, "IceDepthGrid");
			Scribe_Collections.Look(ref waterDepthGridList, "WaterDepthGrid");
			Scribe_Collections.Look(ref pseudoElevationGridList, "PseudoElevationGrid");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				NaturalWaterTerrainGrid = naturalWaterTerrainGridList.ToArray();
				AllWaterTerrainGrid = allWaterTerrainGridList.ToArray();
				IceDepthGrid = iceDepthGridList.ToArray();
				WaterDepthGrid = waterDepthGridList.ToArray();
				PseudoWaterElevationGrid = pseudoElevationGridList.ToArray();
			}
			base.ExposeData();
        }

        ////Copypasta from GenStep_Terrain due to PRIVATE.
        //private TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, float fertility, RiverMaker river, bool preferSolid)
        //{
        //	TerrainDef terrainDef = null;
        //	if (river != null)
        //	{
        //		terrainDef = river.TerrainAt(c, recordForValidation: true);
        //	}
        //	if (terrainDef == null && preferSolid)
        //	{
        //		return null; //Changed from RockDefAt, we don't care about rocks here.
        //	}
        //	TerrainDef terrainDef2 = BeachMaker.BeachTerrainAt(c, map.Biome);
        //	if (terrainDef2 == TerrainDefOf.WaterOceanDeep)
        //	{
        //		return terrainDef2;
        //	}
        //	if (terrainDef != null && terrainDef.IsRiver)
        //	{
        //		return terrainDef;
        //	}
        //	if (terrainDef2 != null)
        //	{
        //		return terrainDef2;
        //	}
        //	if (terrainDef != null)
        //	{
        //		return terrainDef;
        //	}
        //	for (int i = 0; i < map.Biome.terrainPatchMakers.Count; i++)
        //	{
        //		terrainDef2 = map.Biome.terrainPatchMakers[i].TerrainAt(c, map, fertility);
        //		if (terrainDef2 != null)
        //		{
        //			return terrainDef2;
        //		}
        //	}
        //	if (elevation > 0.55f && elevation < 0.61f)
        //	{
        //		return null; //We don't care about gravel.
        //	}
        //	if (elevation >= 0.61f)
        //	{
        //		return null; //We don't care about your rocks.
        //	}
        //	terrainDef2 = TerrainThreshold.TerrainAtValue(map.Biome.terrainsByFertility, fertility);
        //	if (terrainDef2 != null)
        //	{
        //		return terrainDef2;
        //	}
        //	return TerrainDefOf.Sand;
        //}
    }
}