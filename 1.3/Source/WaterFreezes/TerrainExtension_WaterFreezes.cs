using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace WF
{
    public class TerrainExtension_WaterFreezes
    {
        public float MaxWaterDepth;
        public float MaxIceDepth;
        public float FreezingTemperatureOffset;
        public TerrainDef ThinIceDef;
        public TerrainDef IceDef;
        public TerrainDef ThickIceDef;
    }
}