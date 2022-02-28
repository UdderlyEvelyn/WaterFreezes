using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace WF
{
    public static class WaterFreezesStatCache
    {
        private static Dictionary<TerrainDef, TerrainExtension_WaterStats> extensionPerDef = new Dictionary<TerrainDef, TerrainExtension_WaterStats>();
        public static HashSet<TerrainDef> FreezableWater = new HashSet<TerrainDef>();
        public static HashSet<TerrainDef> ThawableIce = new HashSet<TerrainDef>();

        public static void Initialize()
        {
            var sw = Stopwatch.StartNew();
            foreach (var def in DefDatabase<TerrainDef>.AllDefs)
            {
                var extension = def.GetModExtension<TerrainExtension_WaterStats>();
                if (extension != null)
                {
                    extensionPerDef.Add(def, extension);
                    if (extension.ThinIceDef != null)
                    {
                        FreezableWater.Add(def);
                        ThawableIce.Add(extension.ThinIceDef);
                    }
                    if (extension.IceDef != null)
                        ThawableIce.Add(extension.IceDef);
                    if (extension.ThickIceDef != null)
                        ThawableIce.Add(extension.ThickIceDef);
                }
            }
            sw.Stop();
            WaterFreezes.Log("Generated stat cache in " + (sw.ElapsedMilliseconds > 0 ? sw.ElapsedMilliseconds + "ms" : ((double)sw.ElapsedTicks/((double)TimeSpan.TicksPerMillisecond/1000d))  + "μs"));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TerrainExtension_WaterStats GetExtension(TerrainDef def)
        {
            return extensionPerDef[def];
        }
    }
}
