using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace WF
{
    public static class TerrainDefExtensions
    {
        public static bool IsBridge(this TerrainDef def)
        {
            return def.bridge || def.label.ToLowerInvariant().Contains("bridge") || def.defName.ToLowerInvariant().Contains("bridge");
        }
    }
}
