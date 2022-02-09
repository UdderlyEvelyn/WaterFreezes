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
    ////This has the moisture pump mark terrain as not natural water anymore.
    //[HarmonyPatch(typeof(DropCellFinder), "TryFindSafeLandingSpotCloseToColony")]
    //public class DropCellFinder_SkyfallerCanLandAt
    //{
    //    internal static void Postfix(IntVec3 c, Map map, ref bool __result)
    //    {
    //        if (__result) //If nothing has stopped it yet (no need to do anything if it already failed, for performance)..
    //            if (IceDefs.IsIce(c.GetTerrain(map))) //If it's one of our types of ice..
    //                __result = "ShuttleCannotLand_IceDangerous".Translate().CapitalizeFirst(); //Don't let it land.
    //    }
    //}
}


/*
 * public static bool SkyfallerCanLandAt(IntVec3 c, Map map, IntVec2 size, Faction faction = null)
	{
		if (!IsSafeDropSpot(c, map, faction, size, 5))
		{
			return false;
		}
		foreach (IntVec3 item in GenAdj.OccupiedRect(c, Rot4.North, size))
		{
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing is IActiveDropPod || thing is Skyfaller)
				{
					return false;
				}
				if (thing.def.preventSkyfallersLandingOn)
				{
					return false;
				}
				if (thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Building)
				{
					return false;
				}
			}
		}
		return true;
	}*/