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
        public static List<IToggleablePatch> Patches = new List<IToggleablePatch>();

        public static ToggleablePatch<ThingDef> WatermillIsFlickablePatch = new ToggleablePatch<ThingDef>
        {
            Name = "Watermill Is Flickable",
            Enabled = true,
            TargetDefName = "WatermillGenerator",
            Patch = def =>
            {
                def.comps.Add(new CompProperties_Flickable());
            },
            Unpatch = def =>
            {
                var comps = def.comps.Where(c => c is CompProperties_Flickable);
                foreach (var comp in comps)
                    def.comps.Remove(comp);                
            },
        }; 
        public static ToggleablePatch<ThingDef> VPEAdvancedWatermillIsFlickablePatch = new ToggleablePatch<ThingDef>
        {
            Name = "VPE Advanced Watermill Is Flickable",
            Enabled = true,
            TargetModID = "VanillaExpanded.VFEPower",
            TargetDefName = "VFE_AdvancedWatermillGenerator",
            Patch = def =>
            {
                def.comps.Add(new CompProperties_Flickable());
            },
            Unpatch = def =>
            {
                var comps = def.comps.Where(c => c is CompProperties_Flickable);
                foreach (var comp in comps)
                    def.comps.Remove(comp);
            },
        };
        public static ToggleablePatch<ThingDef> VPETidalGeneratorIsFlickablePatch = new ToggleablePatch<ThingDef>
        {
            Name = "VPE Tidal Generator Is Flickable",
            Enabled = true,
            TargetModID = "VanillaExpanded.VFEPower",
            TargetDefName = "VFE_TidalGenerator",
            Patch = def =>
            {
                def.comps.Add(new CompProperties_Flickable());
            },
            Unpatch = def =>
            {
                var comps = def.comps.Where(c => c is CompProperties_Flickable);
                foreach (var comp in comps)
                    def.comps.Remove(comp);
            },
        };

        static WaterFreezes()
        {
            var harmony = new Harmony("UdderlyEvelyn.LakesCanFreeze");
            harmony.PatchAll(); 
            Log.Message("[Water Freezes] Initializing..");
            Patches.Add(WatermillIsFlickablePatch);
            Patches.Add(VPEAdvancedWatermillIsFlickablePatch);
            Patches.Add(VPETidalGeneratorIsFlickablePatch);
            ProcessPatches();
        }

        /// <summary>
        /// Process the patches stored in SoilRelocation.Patches.
        /// </summary>
        /// <param name="reason">the reason to process them, optional, shown in logging</param>
        public static void ProcessPatches(string reason = null)
        {
            Log.Message("[Water Freezes] Processing patches" + (reason != null ? " (" + reason + ")" : "") + "..");
            foreach (var patch in Patches)
                patch.Process();
        }
    }
}