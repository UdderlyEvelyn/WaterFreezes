using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace LCF
{
    public class MapComponent_LakesCanFreeze : MapComponent
    {
		bool init;
        public TerrainDef[] NaturalWaterTerrainGrid;
		public float[] IceGrid;
		public float[] WaterGrid;
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
			Log.Message("[Lakes Can Freeze] Initializing..");
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
			if (NaturalWaterTerrainGrid == null) //If we haven't got a waterGrid loaded from the save file, make one.
			{
				Log.Message("[LakesCanFreeze] Generating Water Grids..");
				NaturalWaterTerrainGrid = new TerrainDef[map.cellIndices.NumGridCells];
				WaterGrid = new float[map.cellIndices.NumGridCells];
				for (int i = 0; i < map.cellIndices.NumGridCells; i++)
				{
					var c = map.cellIndices.IndexToCell(i);
					var t = c.GetTerrain(map);
					if (t == TerrainDefOf.WaterDeep)
					{
						NaturalWaterTerrainGrid[i] = t;
						WaterGrid[i] = maxWaterDeep;
					}
					else if (t == TerrainDefOf.WaterShallow)
                    {
						NaturalWaterTerrainGrid[i] = t;
						WaterGrid[i] = maxWaterShallow;
                    }
				}
			}
			if (IceGrid == null)
				IceGrid = new float[map.cellIndices.NumGridCells];
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
					NaturalWaterTerrainGrid[i] != null)
				{
					UpdateIceForTemperature(cell, currentTerrain);
					UpdateIceStage(cell, currentTerrain);
				}
			}
		}

		public void UpdateIceForTemperature(IntVec3 cell, TerrainDef currentTerrain = null, TerrainDef underTerrain = null)
		{
			var temperature = GenTemperature.GetTemperatureForCell(cell, map);
			if (temperature == 0) //If it's 0degC..
				return; //We don't update when ambiguous.
			int i = map.cellIndices.CellToIndex(cell);
			if (currentTerrain == null) //If it wasn't passed in..
				currentTerrain = map.terrainGrid.topGrid[i]; //Get it.
			if (underTerrain == null) //If it wasn't passed in..
				underTerrain = map.terrainGrid.UnderTerrainAt(i); //Get it.
			if (temperature < 0) //Temperature is below zero..
			{
				if (WaterGrid[i] > 0)
				{
					var change = -temperature * freezingMultiplier * //Based on temperature but sped up by a multiplier.
						(currentTerrain == TerrainDefOf.WaterDeep ? .5f : 1f) * //If it's deep water right now, slow it down more.
						(.75f + (.25f * Rand.Value)) //25% of the rate is variable for flavor.
						/ 2500 * iceRate; //Adjust to iceRate based on the 2500 we tuned it to originally.
					IceGrid[i] += change; //Ice goes up..
					if (NaturalWaterTerrainGrid[i] == TerrainDefOf.WaterShallow || currentTerrain == TerrainDefOf.WaterShallow || underTerrain == TerrainDefOf.WaterShallow)
						if (IceGrid[i] > maxIceShallow)
							IceGrid[i] = maxIceShallow;
					if (NaturalWaterTerrainGrid[i] == TerrainDefOf.WaterDeep || currentTerrain == TerrainDefOf.WaterDeep || underTerrain == TerrainDefOf.WaterShallow)
						if (IceGrid[i] > maxIceDeep)
							IceGrid[i] = maxIceDeep;
					WaterGrid[i] -= change; //Water depth goes down..
					if (WaterGrid[i] < 0)
						WaterGrid[i] = 0;
				}
			}
			else if (temperature > 0) //Temperature is above zero..
			{
				if (IceGrid[i] > 0)
				{
					var change = temperature / thawingMultiplier * //Based on temperature but slowed down by a multiplier.
						(currentTerrain == IceDefs.LCF_LakeIceThick ? .5f : 1f) *  //If it's thick ice right now, slow it down more.
						(currentTerrain == IceDefs.LCF_LakeIce ? .75f : 1f) * //If it's regular ice right now, slow it down a little less than thick.
						(.75f + (.25f * Rand.Value)) //25% of the rate is variable for flavor.
						/ 2500 * iceRate; //Adjust to iceRate based on the 2500 we tuned it to originally.
					IceGrid[i] -= change; //Ice goes down..
					if (IceGrid[i] < 0)
						IceGrid[i] = 0;
					else //Only mess with water grid if ice grid had ice to melt.
					{
						WaterGrid[i] += change; //Water depth goes up..
						if ((NaturalWaterTerrainGrid[i] == TerrainDefOf.WaterShallow || currentTerrain == TerrainDefOf.WaterShallow || underTerrain == TerrainDefOf.WaterShallow) && WaterGrid[i] > maxWaterShallow) //If shallow underneath and too much water,
							WaterGrid[i] = maxWaterShallow; //Cap it.
						if ((NaturalWaterTerrainGrid[i] == TerrainDefOf.WaterDeep || currentTerrain == TerrainDefOf.WaterShallow || underTerrain == TerrainDefOf.WaterDeep) && WaterGrid[i] > maxWaterDeep) //If deep underneath and too much water,
							WaterGrid[i] = maxWaterDeep; //Cap it.
					}
				}
			}
		}

		public void UpdateIceStage(IntVec3 cell, TerrainDef currentTerrain = null)
        {
			int i = map.cellIndices.CellToIndex(cell);
			float ice = IceGrid[i];
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
						if (WaterGrid[i] < maxWaterDeep) //If it's not over-full..
							WaterGrid[i] += 1 / 2500 * iceRate; //Fill
						if (WaterGrid[i] > maxWaterDeep) //If it's too full..
							WaterGrid[i] = maxWaterDeep; //Cap it.
					}
					else if (NaturalWaterTerrainGrid[i] == TerrainDefOf.WaterShallow) //It's shallow water when present..
					{
						if (WaterGrid[i] < maxWaterShallow) //If it's not over-full..
							WaterGrid[i] += 1 / 2500 * iceRate; //Fill
						if (WaterGrid[i] > maxWaterShallow) //If it's too full..
							WaterGrid[i] = maxWaterShallow; //Cap it.
					}
				}
			}
			else if (underTerrain != null && (underTerrain == TerrainDefOf.WaterShallow || underTerrain == TerrainDefOf.WaterDeep)) //If there was under-terrain and it's water.
			{
				if (WaterGrid[i] > 0) //If there's water there..
				{
					map.terrainGrid.SetTerrain(cell, underTerrain); //Set the top layer to the under-terrain
					map.terrainGrid.SetUnderTerrain(cell, null); //Clear the under-terrain
					DestroyBuildingsInCell(cell);
				}
			}
		}

		public float TakeCellIce(IntVec3 cell)
		{
			int i = map.cellIndices.CellToIndex(cell);
			var ice = IceGrid[i];
			IceGrid[i] = 0;
			UpdateIceStage(cell);
			return ice;
        }

		public float QueryCellIce(IntVec3 cell)
        {
			return IceGrid[map.cellIndices.CellToIndex(cell)];
		}

		public float QueryCellWater(IntVec3 cell)
		{
			return WaterGrid[map.cellIndices.CellToIndex(cell)];
		}

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
			List<float> iceGridList = new List<float>();
			List<float> waterGridList = new List<float>();
			if (Scribe.mode == LoadSaveMode.Saving)
            {
				if (NaturalWaterTerrainGrid != null)
					naturalWaterTerrainGridList = NaturalWaterTerrainGrid.ToList();
				if (IceGrid != null)
					iceGridList = IceGrid.ToList();
				if (WaterGrid != null)
					waterGridList = WaterGrid.ToList();
            }
			Scribe_Collections.Look(ref naturalWaterTerrainGridList, "NaturalWaterTerrainGrid");
			Scribe_Collections.Look(ref iceGridList, "IceGrid");
			Scribe_Collections.Look(ref waterGridList, "WaterGrid");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				NaturalWaterTerrainGrid = naturalWaterTerrainGridList.ToArray();
				IceGrid = iceGridList.ToArray();
				WaterGrid = waterGridList.ToArray();
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