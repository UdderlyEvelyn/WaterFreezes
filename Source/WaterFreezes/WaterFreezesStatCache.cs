using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Verse;

namespace WF;

public static class WaterFreezesStatCache
{
    private static readonly Dictionary<TerrainDef, TerrainExtension_WaterStats> extensionPerDef =
        new Dictionary<TerrainDef, TerrainExtension_WaterStats>();

    public static readonly HashSet<string> FreezableWater = [];
    public static readonly HashSet<string> ThawableIce = [];

    public static void Initialize()
    {
        var sw = Stopwatch.StartNew();
        foreach (var def in DefDatabase<TerrainDef>.AllDefs)
        {
            var extension = def.GetModExtension<TerrainExtension_WaterStats>();
            if (extension == null)
            {
                continue;
            }

            extensionPerDef.Add(def, extension);
            if (extension.ThinIceDef != null)
            {
                FreezableWater.Add(def.defName);
                ThawableIce.Add(extension.ThinIceDef.defName);
            }

            if (extension.IceDef != null)
            {
                ThawableIce.Add(extension.IceDef.defName);
            }

            if (extension.ThickIceDef != null)
            {
                ThawableIce.Add(extension.ThickIceDef.defName);
            }
        }

        sw.Stop();
        WaterFreezes.Log(
            $"Generated stat cache in {(sw.ElapsedMilliseconds > 0 ? $"{sw.ElapsedMilliseconds}ms" : $"{sw.ElapsedTicks / (TimeSpan.TicksPerMillisecond / 1000d)}μs")}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TerrainExtension_WaterStats GetExtension(TerrainDef def)
    {
        return extensionPerDef[def];
    }
}