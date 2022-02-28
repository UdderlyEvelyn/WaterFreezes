using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using System.Runtime.CompilerServices;

namespace WF
{
    public static class TerrainDefExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBridge(this TerrainDef def)
        {
            return def.bridge || def.label.ToLowerInvariant().Contains("bridge") || def.defName.ToLowerInvariant().Contains("bridge");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFreezableWater(this TerrainDef def)
        {
            return WaterFreezesStatCache.FreezableWater.Contains(def);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsThawableIce(this TerrainDef def)
        {
            return WaterFreezesStatCache.ThawableIce.Contains(def);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsShallowDepth(this TerrainDef def)
        {
            return def == TerrainDefOf.WaterShallow || def == TerrainDefOf.WaterMovingShallow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDeepDepth(this TerrainDef def)
        {
            return def == TerrainDefOf.WaterDeep || def == TerrainDefOf.WaterMovingChestDeep;
        }
    }
}
