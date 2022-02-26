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
            return def == TerrainDefOf.WaterShallow || 
                   def == TerrainDefOf.WaterDeep || 
                   def == TerrainDefOf.WaterMovingShallow || 
                   def == TerrainDefOf.WaterMovingChestDeep || 
                   def == WaterDefs.Marsh;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsThawableIce(this TerrainDef def)
        {
            return def == IceDefs.WF_LakeIceThin ||
                   def == IceDefs.WF_LakeIce ||
                   def == IceDefs.WF_LakeIceThick ||
                   def == IceDefs.WF_MarshIceThin ||
                   def == IceDefs.WF_MarshIce ||
                   def == IceDefs.WF_RiverIceThin ||
                   def == IceDefs.WF_RiverIce ||
                   def == IceDefs.WF_RiverIceThick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLakeWater(this TerrainDef def)
        {
            return def == TerrainDefOf.WaterShallow ||
                   def == TerrainDefOf.WaterDeep;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRiverWater(this TerrainDef def)
        {
            return def == TerrainDefOf.WaterMovingShallow ||
                   def == TerrainDefOf.WaterMovingChestDeep;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOceanWater(this TerrainDef def)
        {
            return def == TerrainDefOf.WaterOceanShallow ||
                   def == TerrainDefOf.WaterOceanDeep;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLakeIce(this TerrainDef def)
        {
            return def == IceDefs.WF_LakeIceThin ||
                   def == IceDefs.WF_LakeIce ||
                   def == IceDefs.WF_LakeIceThick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMarshIce(this TerrainDef def)
        {
            return def == IceDefs.WF_MarshIceThin ||
                   def == IceDefs.WF_MarshIce;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRiverIce(this TerrainDef def)
        {
            return def == IceDefs.WF_RiverIceThin ||
                   def == IceDefs.WF_RiverIce ||
                   def == IceDefs.WF_RiverIceThick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLake(this TerrainDef def)
        {
            return def.IsLakeWater() || def.IsLakeIce();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRiver(this TerrainDef def)
        {
            return def.IsRiverWater() || def.IsRiverIce();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMarsh(this TerrainDef def)
        {
            return def == WaterDefs.Marsh || def.IsMarshIce();
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
