using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using Verse.Noise;
using RimWorld.Planet;
using UnityEngine;
using System.Reflection;
using System.Reflection.Emit;

namespace WF
{
    [HarmonyPatch(typeof(MouseoverReadout), "MouseoverReadoutOnGUI")]
    public class MouseoverReadout_MouseoverReadoutOnGUI
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo labelMaker = AccessTools.Method(typeof(MouseoverReadout_MouseoverReadoutOnGUI), "MakeLabelIfRequired");
            FieldInfo BotLeft = AccessTools.Field(typeof(MouseoverReadout), "BotLeft");
            var codes = new List<CodeInstruction>(instructions);
            int num = 0;
            bool skip = true;
            for (var i = 0; i < codes.Count; ++i)
            {
                if (num == 7 && skip)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0).WithLabels(codes[i].labels);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, BotLeft);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);

                    yield return new CodeInstruction(OpCodes.Call, labelMaker);
                    yield return new CodeInstruction(OpCodes.Stloc_1);
                    skip = false;
                    yield return codes[i].WithLabels(new Label[] { });
                    continue;
                }
                yield return codes[i];
                if (codes[i].opcode == OpCodes.Stloc_1)
                {
                    num++;
                }
            }
        }

        public static float MakeLabelIfRequired(IntVec3 cell, Vector2 BotLeft, float num)
        {
            var comp = Find.CurrentMap.GetComponent<MapComponent_WaterFreezes>();
            if (comp != null)
            {
                float rectY = num;
                int ind = comp.map.cellIndices.CellToIndex(cell);

                float ice = comp.IceDepthGrid[ind];
                float water = comp.WaterDepthGrid[ind];
                bool naturalWater = comp.NaturalWaterTerrainGrid[ind] != null;
                //string allWater = comp.AllWaterTerrainGrid[ind]?.defName ?? "null";
                //float pseudoElevation = comp.PseudoWaterElevationGrid[ind];
                //string underTerrain = comp.map.terrainGrid.UnderTerrainAt(cell)?.defName ?? "null";
                if (ice > 0)
                {
                    Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - rectY, 999f, 999f), "Ice depth " + Math.Round(ice, 4).ToString());
                    rectY += 19f;
                }
                if (water > 0)
                {
                    Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - rectY, 999f, 999f), "Water depth " + Math.Round(water, 4).ToString());
                    rectY += 19f;
                }
                if (naturalWater)
                {
                    Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - rectY, 999f, 999f), "Natural Water");
                    rectY += 19f;
                }
                //Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - rectY, 999f, 999f), "UnderTerrain: " + underTerrain);
                //rectY += 19f;
                //Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - rectY, 999f, 999f), allWater);
                //rectY += 19f;
                //if (pseudoElevation > 0)
                //{
                //    Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - rectY, 999f, 999f), "Elevation " + pseudoElevation.ToString());
                //    rectY += 19f;
                //}
                return rectY;
            }
            return num;
        }
    }
}