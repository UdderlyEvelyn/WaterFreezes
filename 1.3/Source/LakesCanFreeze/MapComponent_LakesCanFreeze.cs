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
        TerrainDef[] waterGrid;
		MapGenFloatGrid elevation;
		MapGenFloatGrid fertility;

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
			Map fakeMap = new Map() { uniqueID = map.uniqueID, info = map.info, cellIndices = map.cellIndices }; //Super secret fake map.
			new GenStep_ElevationFertility().Generate(fakeMap, new GenStepParams()); //Parms aren't used.
			//Preserve these, when another map is gen'd they'll be discarded!
			elevation = MapGenerator.Elevation;
			fertility = MapGenerator.Fertility;

			waterGrid = new TerrainDef[map.cellIndices.NumGridCells];
			for (int i = 0; i < map.cellIndices.NumGridCells; i++)
			{
				var c = map.cellIndices.IndexToCell(i);
				var t = TerrainFrom(c, map, elevation[c], fertility[c], null, true);
				if (t == TerrainDefOf.WaterDeep || t == TerrainDefOf.WaterShallow)
					waterGrid[i] = t;
			}
			init = true;
		}

        public override void MapComponentTick()
        {
			if (!init) //If we aren't initialized..
				Initialize(); //Initialize it!
			if (Find.TickManager.TicksGame % 2500 != 0) //If it's not once per hour..
				return; //Don't execute the rest, throttling measure.
			Log.Message("[LakesCanFreeze] Ticky tocky..");
			for (int i = 0; i < waterGrid.Length; i++) //Thread this later probably.
			{
				var c = map.cellIndices.IndexToCell(i);
				var ot = c.GetTerrain(map); //Get original terrain.
				//If it's natural water or it's shallow/deep right now.
				if (waterGrid[i] != null || ot == TerrainDefOf.WaterShallow || ot == TerrainDefOf.WaterDeep)
						{ 
				var temp = GenTemperature.GetTemperatureForCell(c, map);
					if (temp < 0) //Temperature is below zero..
					{
						map.terrainGrid.SetUnderTerrain(c, ot); //Store it in under-terrain
						map.terrainGrid.SetTerrain(c, TerrainDefOf.Ice); //Put ice on top
					}
					else if (temp > 0) //Temperature is above zero..
					{
						TerrainDef ut = map.terrainGrid.UnderTerrainAt(c); //Get under-terrain
						if (ut != null) //If there was under-terrain
						{
							map.terrainGrid.SetTerrain(c, ut); //Set the top layer to the under-terrain
							map.terrainGrid.SetUnderTerrain(c, null); //Clear the under-terrain
						}
						else if (waterGrid[i] != null) //If it's natural water..
							map.terrainGrid.SetTerrain(c, waterGrid[i]); //Restore it.
						//else it wasn't natural water, and we didn't freeze it, so leave it alone..?
					}
				}
			}
        }

		//Copypasta from GenStep_Terrain due to PRIVATE.
		private TerrainDef TerrainFrom(IntVec3 c, Map map, float elevation, float fertility, RiverMaker river, bool preferSolid)
		{
			TerrainDef terrainDef = null;
			if (river != null)
			{
				terrainDef = river.TerrainAt(c, recordForValidation: true);
			}
			if (terrainDef == null && preferSolid)
			{
				return GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
			}
			TerrainDef terrainDef2 = BeachMaker.BeachTerrainAt(c, map.Biome);
			if (terrainDef2 == TerrainDefOf.WaterOceanDeep)
			{
				return terrainDef2;
			}
			if (terrainDef != null && terrainDef.IsRiver)
			{
				return terrainDef;
			}
			if (terrainDef2 != null)
			{
				return terrainDef2;
			}
			if (terrainDef != null)
			{
				return terrainDef;
			}
			for (int i = 0; i < map.Biome.terrainPatchMakers.Count; i++)
			{
				terrainDef2 = map.Biome.terrainPatchMakers[i].TerrainAt(c, map, fertility);
				if (terrainDef2 != null)
				{
					return terrainDef2;
				}
			}
			if (elevation > 0.55f && elevation < 0.61f)
			{
				return TerrainDefOf.Gravel;
			}
			if (elevation >= 0.61f)
			{
				return GenStep_RocksFromGrid.RockDefAt(c).building.naturalTerrain;
			}
			terrainDef2 = TerrainThreshold.TerrainAtValue(map.Biome.terrainsByFertility, fertility);
			if (terrainDef2 != null)
			{
				return terrainDef2;
			}
			return TerrainDefOf.Sand;
		}
	}
}