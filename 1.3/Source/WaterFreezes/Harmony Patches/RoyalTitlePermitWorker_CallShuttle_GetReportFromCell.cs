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
    //This has the moisture pump mark terrain as not natural water anymore.
    //[HarmonyPatch(typeof(RoyalTitlePermitWorker_CallShuttle), "GetReportFromCell")]
    //public class RoyalTitlePermitWorker_CallShuttle_GetReportFromCell
    //{
    //    internal static void Postfix(Map map, IntVec3 cell, ref string __result)
    //    {
    //        if (__result == null) //If nothing has stopped it yet (no need to do anything if it already failed, for performance)..
    //            if (IceDefs.IsIce(cell.GetTerrain(map))) //If it's one of our types of ice..
    //                __result = "ShuttleCannotLand_IceDangerous".Translate().CapitalizeFirst(); //Don't let it land.
    //    }
    //}
}