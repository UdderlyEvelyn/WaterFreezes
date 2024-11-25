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
        private static Dictionary<TerrainDef, bool> bridgeCache = new Dictionary<TerrainDef, bool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBridge(this TerrainDef def)
        {
            if (!bridgeCache.ContainsKey(def))
                bridgeCache[def] =  def.bridge || def.label.ToLowerInvariant().Contains("bridge") || def.defName.ToLowerInvariant().Contains("bridge");
            return bridgeCache[def];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFreezableWater(this TerrainDef def)
        {
            return WaterFreezesStatCache.FreezableWater.Contains(def.defName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsThawableIce(this TerrainDef def)
        {
            return WaterFreezesStatCache.ThawableIce.Contains(def.defName);
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
