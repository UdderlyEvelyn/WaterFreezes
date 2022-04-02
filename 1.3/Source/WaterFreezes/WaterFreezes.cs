using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace WF
{
    [StaticConstructorOnStartup]
    public static class WaterFreezes
    {
        //[ToggleablePatch]
        //public static ToggleablePatch<ThingDef> RimsentialSpaceportsFuelProcessorIsBreakdownablePatch = new ToggleablePatch<ThingDef>
        //{
        //    Name = "Rimsential Spaceports Fuel Processor Is Breakdownable",
        //    Enabled = true,
        //    TargetDefName = "Spaceports_FuelProcessor",
        //    Patch = (patch, def) =>
        //    {
        //        def.comps.Add(new CompProperties_Breakdownable());
        //    },
        //    Unpatch = (patch, def) =>
        //    {
        //        def.comps.RemoveAll(comp => comp is CompProperties_Breakdownable);
        //    },
        //};

        [ToggleablePatch]
        public static ToggleablePatch<ThingDef> WatermillIsFlickablePatch = new ToggleablePatch<ThingDef>
        {
            Name = "Watermill Is Flickable",
            Enabled = true,
            TargetDefName = "WatermillGenerator",
            Patch = (patch, def) =>
            {
                def.comps.Add(new CompProperties_Flickable());
            },
            Unpatch = (patch, def) =>
            {
                def.comps.RemoveAll(comp => comp is CompProperties_Flickable);          
            },
        }; 

        [ToggleablePatch]
        public static ToggleablePatch<ThingDef> VPEAdvancedWatermillIsFlickablePatch = new ToggleablePatch<ThingDef>
        {
            Name = "VPE Advanced Watermill Is Flickable",
            Enabled = true,
            TargetModID = "VanillaExpanded.VFEPower",
            TargetDefName = "VFE_AdvancedWatermillGenerator",
            Patch = (patch, def) =>
            {
                def.comps.Add(new CompProperties_Flickable());
            },
            Unpatch = (patch, def) =>
            {
                def.comps.RemoveAll(c => c is CompProperties_Flickable);
            },
        };

        [ToggleablePatch]
        public static ToggleablePatch<ThingDef> VPETidalGeneratorIsFlickablePatch = new ToggleablePatch<ThingDef>
        {
            Name = "VPE Tidal Generator Is Flickable",
            Enabled = true,
            TargetModID = "VanillaExpanded.VFEPower",
            TargetDefName = "VFE_TidalGenerator",
            Patch = (patch, def) =>
            {
                def.comps.Add(new CompProperties_Flickable());
            },
            Unpatch = (patch, def) =>
            {
                def.comps.RemoveAll(c => c is CompProperties_Flickable);
            },
        };

        static WaterFreezes()
        {
            Log("Initializing..");
            ToggleablePatch.ProcessPatches("UdderlyEvelyn.WaterFreezes");
            var harmony = new Harmony("UdderlyEvelyn.WaterFreezes");
            harmony.PatchAll(); 
            WaterFreezesStatCache.Initialize();
        }

        private static Version version = Assembly.GetAssembly(typeof(WaterFreezes)).GetName().Version;
        /// <summary>
        /// The assembly version of the mod.
        /// </summary>
        public static string Version = version.Major + "." + version.Minor + "." + version.Build;

        /// <summary>
        /// Logging function for the mod, prints the message with the appropriate method based on errorLevel, optionally ignoring the "stop logging limit".
        /// </summary>
        /// <param name="message">the message to log</param>
        /// <param name="errorLevel">the type of logging method to use</param>
        /// <param name="errorOnceKey">if doing ErrorOnce logging, the unique key to use (defaults to 0)</param>
        /// <param name="ignoreStopLoggingLimit">if true, resets the message count before logging the message</param>
        public static void Log(string message, ErrorLevel errorLevel = ErrorLevel.Message, int errorOnceKey = 0, bool ignoreStopLoggingLimit = false)
        {
            if (ignoreStopLoggingLimit)
                Verse.Log.ResetMessageCount();
            var text = "[Water Freezes " + Version + "] " + message;
            switch (errorLevel)
            {
                case ErrorLevel.Message:
                    Verse.Log.Message(text);
                    break;
                case ErrorLevel.Warning:
                    Verse.Log.Warning(text);
                    break;
                case ErrorLevel.Error:
                    Verse.Log.Error(text);
                    break;
                case ErrorLevel.ErrorOnce:
                    Verse.Log.ErrorOnce(text, errorOnceKey);
                    break;
            }
        }
    }

    /// <summary>
    /// Determines which logging method to use through the WaterFreezes.Log method.
    /// </summary>
    public enum ErrorLevel
    {
        Message,
        Warning,
        Error,
        ErrorOnce,
    }

    public class WaterFreezesMod : Mod
    {
        public WaterFreezesSettings Settings;

        public WaterFreezesMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<WaterFreezesSettings>();
        }

        public override string SettingsCategory()
        {
            return "Water Freezes";
        }

        private string _iceRateBuffer;
        private string _freezingMultiplierBuffer;
        private string _thawingDivisorBuffer;

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.Label("Ticks Per Simulation Update");
            listingStandard.Label("The higher this is the less frequently it will update, the less realistic it will be, and the less it will impact TPS.");
            listingStandard.Label("500 minimum, it's indistinguishable from updating every tick quality-wise but way better performance.");
            listingStandard.Label("Anything higher than 2500 (1 in-game hour) has not been tested. Default is 1000 for a good balance.");
            listingStandard.Label("Versions before this setting was introduced had it set to 2500.");
            listingStandard.TextFieldNumeric<int>(ref WaterFreezesSettings.IceRate, ref _iceRateBuffer, 500, 2500);
            listingStandard.Label("Take care when modifying the multiplier and divisor, relatively small changes will produce big effects!");
            listingStandard.Label("The temperature is multiplied by this value before going into other calculations to produce the amount of freezing that occurs below freezing.");
            listingStandard.TextFieldNumericLabeled<float>("Freezing Factor", ref WaterFreezesSettings.FreezingFactor, ref _freezingMultiplierBuffer, 1);
            listingStandard.Label("The (negated) temperature is divided by this value before going into other calculations to produce the amount of thawing that occurs above freezing.");
            listingStandard.TextFieldNumericLabeled<float>("Thawing Factor", ref WaterFreezesSettings.ThawingFactor, ref _thawingDivisorBuffer, 1);
            listingStandard.CheckboxLabeled("Moisture Pump Clears Natural Water", ref WaterFreezesSettings.MoisturePumpClearsNaturalWater, "If enabled, using a moisture pump will prevent water you pumped from refilling. I recommend using Soil Relocation Framework's water filling instead!");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        //public override void WriteSettings()
        //{
        //    //SoilRelocation.SandbagsUseSandPatch.Enabled = SoilRelocationSettings.SandbagsUseSandEnabled;
        //    //SoilRelocation.DubsSkylightsGlassUsesSandPatch.Enabled = SoilRelocationSettings.DubsSkylightsGlassUsesSandEnabled;
        //    //SoilRelocation.VFEArchitectPackedDirtCostsDirt.Enabled = SoilRelocationSettings.VFEArchitectPackedDirtCostsDirtEnabled;
        //    //SoilRelocation.ProcessPatches("settings were updated");

        //    base.WriteSettings();
        //}
    }
}