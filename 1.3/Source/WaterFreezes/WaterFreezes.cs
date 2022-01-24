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
    [StaticConstructorOnStartup]
    public static class WaterFreezes
    {
        static WaterFreezes()
        {
            var harmony = new Harmony("UdderlyEvelyn.LakesCanFreeze");
            harmony.PatchAll();
        }
    }
}