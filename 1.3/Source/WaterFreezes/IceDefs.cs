using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace WF
{
    public static class IceDefs
    {
        public static TerrainDef WF_LakeIceThin = DefDatabase<TerrainDef>.GetNamed("WF_LakeIceThin");
        public static TerrainDef WF_LakeIce = DefDatabase<TerrainDef>.GetNamed("WF_LakeIce");
        public static TerrainDef WF_LakeIceThick = DefDatabase<TerrainDef>.GetNamed("WF_LakeIceThick");
    }
}
