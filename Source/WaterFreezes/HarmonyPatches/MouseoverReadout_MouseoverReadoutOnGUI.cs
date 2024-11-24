using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace WF;

[HarmonyPatch(typeof(MouseoverReadout), nameof(MouseoverReadout.MouseoverReadoutOnGUI))]
public class MouseoverReadout_MouseoverReadoutOnGUI
{
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var labelMaker = AccessTools.Method(typeof(MouseoverReadout_MouseoverReadoutOnGUI), "MakeLabelIfRequired");
        var BotLeft = AccessTools.Field(typeof(MouseoverReadout), "BotLeft");
        var codes = new List<CodeInstruction>(instructions);
        var num = 0;
        var skip = true;
        foreach (var codeInstruction in codes)
        {
            if (num == 7 && skip)
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0).WithLabels(codeInstruction.labels);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldfld, BotLeft);
                yield return new CodeInstruction(OpCodes.Ldloc_1);

                yield return new CodeInstruction(OpCodes.Call, labelMaker);
                yield return new CodeInstruction(OpCodes.Stloc_1);
                skip = false;
                yield return codeInstruction.WithLabels();
                continue;
            }

            yield return codeInstruction;
            if (codeInstruction.opcode == OpCodes.Stloc_1)
            {
                num++;
            }
        }
    }

    public static float MakeLabelIfRequired(IntVec3 cell, Vector2 BotLeft, float num)
    {
        var comp = Find.CurrentMap.GetComponent<MapComponent_WaterFreezes>();
        if (comp == null)
        {
            return num;
        }

        var rectY = num;
        var ind = comp.map.cellIndices.CellToIndex(cell);

        var ice = comp.IceDepthGrid[ind];
        var water = comp.WaterDepthGrid[ind];
        var naturalWater = comp.NaturalWaterTerrainGrid[ind] != null;
        //string allWater = comp.AllWaterTerrainGrid[ind]?.defName ?? "null";
        //float pseudoElevation = comp.PseudoWaterElevationGrid[ind];
        //string underTerrain = comp.map.terrainGrid.UnderTerrainAt(cell)?.defName ?? "null";
        if (ice > 0)
        {
            Widgets.Label(new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - rectY, 999f, 999f),
                $"Ice depth {Math.Round(ice, 4)}");
            rectY += 19f;
        }

        if (water > 0)
        {
            Widgets.Label(new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - rectY, 999f, 999f),
                $"Water depth {Math.Round(water, 4)}");
            rectY += 19f;
        }

        if (!naturalWater)
        {
            return rectY;
        }

        Widgets.Label(new Rect(BotLeft.x, UI.screenHeight - BotLeft.y - rectY, 999f, 999f), "Natural Water");
        rectY += 19f;

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
}