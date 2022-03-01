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
	////This is used to log why something can't build on a certain spot.
 //   [HarmonyPatch(typeof(GenConstruct), "CanBuildOnTerrain")]
 //   public class GenConstruct_CanBuildOnTerrain
 //   {
 //       internal static void Postfix(BuildableDef entDef, Map map, IntVec3 c, bool __result, Rot4 rot, Thing thingToIgnore = null, ThingDef stuffDef = null)
 //       {
 //           if (!__result)
 //           {
 //               var terrain = c.GetTerrain(map);
 //               if (entDef is TerrainDef && !terrain.changeable)
 //                   Log.Message(entDef.defName + " was determined to not be buildable at its location because the terrain \"" + terrain.defName + "\" was not \"changeable\"..");
 //               else
 //               {
	//				TerrainAffordanceDef terrainAffordanceNeed = entDef.GetTerrainAffordanceNeed(stuffDef);
	//				if (terrainAffordanceNeed != null)
	//				{
	//					CellRect cellRect = GenAdj.OccupiedRect(c, rot, entDef.Size);
	//					cellRect.ClipInsideMap(map);
	//					foreach (IntVec3 item in cellRect)
	//					{
	//						var terrainAtItem = map.terrainGrid.TerrainAt(item);
	//						if (!terrainAtItem.affordances.Contains(terrainAffordanceNeed))
	//						{
	//							Log.Message(entDef.defName + " was determined to not be buildable at its location because the terrain \"" + terrainAtItem.defName + "\" at one of the occupied cells for the blueprint is lacking the affordance \"" + terrainAffordanceNeed.defName + "\", it has:");
	//							if (terrainAtItem.affordances != null)
	//							{
	//								foreach (var aff in terrainAtItem.affordances)
	//									Log.Message("-" + aff.defName);
	//							}
	//							else
	//								Log.Message("(It has no affordances.)");
	//						}
	//						List<Thing> thingList = item.GetThingList(map);
	//						for (int i = 0; i < thingList.Count; ++i)
	//							if (thingList[i] != thingToIgnore &&  //The thing isn't the magical thing to ignore..
	//								thingList[i].def.entityDefToBuild is TerrainDef terrainDef && //And it's a terraindef..
	//								!terrainDef.affordances.Contains(terrainAffordanceNeed)) //And that terraindef can support the need of.. itself..
	//								Log.Message(entDef.defName + " was determined to not be buildable at its location because it would not provide the necessary affordance \"" + terrainAffordanceNeed.defName + "\" for the stuff assigned, \"" + (stuffDef == null ? "NULL" : stuffDef.defName) + "\".");
	//					}
	//				}
	//			}
 //           }
	//	}
 //   }
}