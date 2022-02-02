using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace WF
{
    public class WaterFreezesSettings : ModSettings
    {
        public static int IceRate = 1000;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref IceRate, "IceRate");

            base.ExposeData();
        }
    }
}