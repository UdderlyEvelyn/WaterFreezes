using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace LCF
{
    public static class IceDefs
    {
        public static TerrainDef LCF_LakeIceThin = DefDatabase<TerrainDef>.GetNamed("LCF_LakeIceThin");
        public static TerrainDef LCF_LakeIce = DefDatabase<TerrainDef>.GetNamed("LCF_LakeIce");
        public static TerrainDef LCF_LakeIceThick = DefDatabase<TerrainDef>.GetNamed("LCF_LakeIceThick");
    }
}
