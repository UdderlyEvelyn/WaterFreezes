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
        public static TerrainDef WF_MarshIceThin = DefDatabase<TerrainDef>.GetNamed("WF_MarshIceThin");
        public static TerrainDef WF_MarshIce = DefDatabase<TerrainDef>.GetNamed("WF_MarshIce");
        public static TerrainDef WF_RiverIceThin = DefDatabase<TerrainDef>.GetNamed("WF_RiverIceThin");
        public static TerrainDef WF_RiverIce = DefDatabase<TerrainDef>.GetNamed("WF_RiverIce");
        public static TerrainDef WF_RiverIceThick = DefDatabase<TerrainDef>.GetNamed("WF_RiverIceThick");
    }
}
