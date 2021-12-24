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
        public TerrainDef[] WaterGrid;
		public float[] IceGrid;
		//MapGenFloatGrid elevation;
		//MapGenFloatGrid fertility;
		float thresholdThinIce = 25;
		float thresholdIce = 125;
		float thresholdThickIce = 250;
		float maximumIce = 400;
		int freezingMultiplier = 4;
		int thawingMultiplier = 2;
		float iceRate = 2500;

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
			if (WaterGrid == null) //If we haven't got a waterGrid loaded from the save file, make one.
			{
				Log.Message("[LakesCanFreeze] Generating Water Grid..");
				WaterGrid = new TerrainDef[map.cellIndices.NumGridCells];
				for (int i = 0; i < map.cellIndices.NumGridCells; i++)
				{
					var c = map.cellIndices.IndexToCell(i);
					var t = c.GetTerrain(map);
					if (t == TerrainDefOf.WaterDeep || t == TerrainDefOf.WaterShallow)
						WaterGrid[i] = t;
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
			int cellsFreezing = 0;
			int cellsThawing = 0;
			int cellsRefilled = 0;
			int cellsThawed = 0;
			for (int i = 0; i < WaterGrid.Length; i++) //Thread this later probably.
			{
				var cell = map.cellIndices.IndexToCell(i);
				var currentTerrain = cell.GetTerrain(map); //Get current terrain.
				//If it's lake ice or it's water..
				if (currentTerrain == IceDefs.LCF_LakeIceThin || 
					currentTerrain == IceDefs.LCF_LakeIce || 
					currentTerrain == IceDefs.LCF_LakeIceThick || 
					currentTerrain == TerrainDefOf.WaterShallow || 
					currentTerrain == TerrainDefOf.WaterDeep)
				{
					var temperature = GenTemperature.GetTemperatureForCell(cell, map);
					bool cellIsFreezing = false;
					if (temperature < 0) //Temperature is below zero..
					{
						cellsFreezing++;
						IceGrid[i] += -temperature * freezingMultiplier * //Based on temperature but sped up by a multiplier.
							(currentTerrain == TerrainDefOf.WaterDeep ? .5f : 1f) * //If it's deep water right now, slow it down more.
							(.75f + (.25f * Rand.Value)) //25% of the rate is variable for flavor.
							/ 2500 * iceRate; //Adjust to iceRate based on the 2500 we tuned it to originally.
						if (IceGrid[i] > maximumIce)
							IceGrid[i] = maximumIce;
						cellIsFreezing = true;
					}
					else if (temperature > 0) //Temperature is above zero..
					{
						TerrainDef underTerrain = map.terrainGrid.UnderTerrainAt(cell); //Get under-terrain
						{
							cellsThawing++;
							IceGrid[i] -= temperature / thawingMultiplier * //Based on temperature but slowed down by a multiplier.
								(currentTerrain == IceDefs.LCF_LakeIceThick ? .5f : 1f) *  //If it's thick ice right now, slow it down more.
								(currentTerrain == IceDefs.LCF_LakeIce ? .75f : 1f) * //If it's regular ice right now, slow it down a little less than thick.
								(.75f + (.25f * Rand.Value)) //25% of the rate is variable for flavor.
								/ 2500 * iceRate; //Adjust to iceRate based on the 2500 we tuned it to originally.
							if (IceGrid[i] < 0)
								IceGrid[i] = 0;
							if (IceGrid[i] == 0)
							{
								if (underTerrain != null && (underTerrain == TerrainDefOf.WaterShallow || underTerrain == TerrainDefOf.WaterDeep)) //If there was under-terrain and it's water.
								{
									cellsThawed++;
									cellsThawing--; //Not thawing if it thawed!
									map.terrainGrid.SetTerrain(cell, underTerrain); //Set the top layer to the under-terrain
									map.terrainGrid.SetUnderTerrain(cell, null); //Clear the under-terrain
								}
								else if (WaterGrid[i] != null && currentTerrain != TerrainDefOf.WaterDeep && currentTerrain != TerrainDefOf.WaterShallow) //If it's natural water but is missing..
								{
									var season = GenLocalDate.Season(map);
									if (season == Season.Spring || season == Season.Summer || season == Season.PermanentSummer)
									{
										cellsRefilled++;
										map.terrainGrid.SetTerrain(cell, WaterGrid[i]); //Restore it.
									}
								}
								DestroyBuildingsInCell(cell);
							}
						}
					}
					//Update ice stage..
					if (IceGrid[i] > thresholdThinIce && IceGrid[i] < thresholdIce)
					{
						map.terrainGrid.SetTerrain(cell, IceDefs.LCF_LakeIceThin); //Switch to thin ice.
						if (cellIsFreezing) //If it's going down, *not* up..
							map.terrainGrid.SetUnderTerrain(cell, currentTerrain); //Store the original in the under-terrain
					}
					else if (IceGrid[i] > thresholdIce && IceGrid[i] < thresholdThickIce)
					{
						map.terrainGrid.SetTerrain(cell, IceDefs.LCF_LakeIce); //Switch to regular ice.
					}
					else  if (IceGrid[i] > thresholdThickIce)
					{
						map.terrainGrid.SetTerrain(cell, IceDefs.LCF_LakeIceThick); //Switch to thick ice.
					}
				}
			}
			Log.Message("[LakesCanFreeze] Freezing " + cellsFreezing + ", thawing " + cellsThawing + ", thawed " + cellsThawed + ", and refilling " + cellsRefilled + ".");
		}

		public float TakeCellIce(IntVec3 cell)
        {
			Log.Message("[LakesCanFreeze] GetCellIce executing!");
			int i = map.cellIndices.CellToIndex(cell);
			float ice = IceGrid[i];
			IceGrid[i] = 0;
			return ice;
        }

		//public void ClearCellIce(IntVec3 cell)
		//{
		//	Log.Message("[LakesCanFreeze] ClearCellIce executing!");
		//	IceGrid[map.cellIndices.CellToIndex(cell)] = 0;
  //      }

		public void DestroyBuildingsInCell(IntVec3 cell)
        {
			var things = cell.GetThingList(map);
			for (int i = 0; i < things.Count; i++)
			{
				var thing = things[i];
				if (thing.def is BuildableDef && thing.def.destroyable)
					thing.Destroy(DestroyMode.Deconstruct);
			}
        }

        public override void ExposeData()
		{
			Scribe_Values.Look(ref WaterGrid, "WaterGrid");
			Scribe_Values.Look(ref IceGrid, "IceGrid");
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