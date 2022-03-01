using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace WF
{
    public class TerrainExtension_WaterStats : DefModExtension
    {
        /// <summary>
        /// The most water a cell with this type of water terrain can hold (used only on water defs).
        /// </summary>
        public float MaxWaterDepth;
        /// <summary>
        /// The most ice a cell with this type of water terrain can hold (used only on water defs).
        /// </summary>
        public float MaxIceDepth;
        /// <summary>
        /// An offset from 0C for the freezing point of this type of water (used only on water defs).
        /// </summary>
        public float FreezingPoint;
        /// <summary>
        /// Whether this water is considered moving - this affects whether ice lasts longer near land (if true) or away from land (if false).
        /// </summary>
        public bool IsMoving;
        /// <summary>
        /// The def used for thin ice (mandatory if you want it to change terrain to ice at all, used only on water defs).
        /// </summary>
        public TerrainDef ThinIceDef;
        /// <summary>
        /// The def used for regular ice (optional, second stage of thickness, used only on water defs).
        /// </summary>
        public TerrainDef IceDef;
        /// <summary>
        /// The def used for thick ice (optional, third stage of thickness, used only on water defs).
        /// </summary>
        public TerrainDef ThickIceDef;
    }
}