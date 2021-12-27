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

namespace LCF
{
    //Saving this in case we need it for future projects, might do something with, e.g., soil fertility that dips after enough plantings and then slowly bounces back..

    //[HarmonyPatch(typeof(GenStep_ElevationFertility), "Generate")]

    [HarmonyPatch(typeof(TerrainGrid), "SetTerrain")]
    public class SetTerrainUpdateHook
    {
        private static Dictionary<int, MapComponent_LakesCanFreeze> compCachePerMap;

        internal static void Postfix(IntVec3 c, TerrainDef newTerr, Map ___map)
        {
            bool deep = newTerr == TerrainDefOf.WaterDeep;
            bool shallow = newTerr == TerrainDefOf.WaterShallow;
            if (!deep && !shallow) //If it's not water..
                return; //Don't care.
            MapComponent_LakesCanFreeze comp; //Set up var.
            if (!compCachePerMap.ContainsKey(___map.uniqueID)) //If not cached..
                compCachePerMap.Add(___map.uniqueID, comp = ___map.GetComponent<MapComponent_LakesCanFreeze>()); //Get and cache.
            else
                comp = compCachePerMap[___map.uniqueID]; //Retrieve from cache.
            comp.
        }
    }


    [HarmonyPatch(typeof(MouseoverReadout), "MouseoverReadoutOnGUI")]
    public class MouseoverReadoutOnGUITranspiler
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo labelMaker = AccessTools.Method(typeof(MouseoverReadoutOnGUITranspiler), "MakeLabelIfRequired");
            FieldInfo BotLeft = AccessTools.Field(typeof(MouseoverReadout), "BotLeft");
            var codes = new List<CodeInstruction>(instructions);
            int num = 0;
            bool skip = true;
            for (var i = 0; i < codes.Count; i++)
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
            var comp = Find.CurrentMap.GetComponent<MapComponent_LakesCanFreeze>();
            if (comp != null)
            {
                float rectY = num;
                int ind = comp.map.cellIndices.CellToIndex(cell);

                float ice = comp.IceDepthGrid[ind];
                float water = comp.WaterDepthGrid[ind];
                string naturalWaterLabel = comp.NaturalWaterTerrainGrid[ind] != null ? comp.NaturalWaterTerrainGrid[ind].LabelCap : null;
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
                if (naturalWaterLabel != null)
                { 
                    Widgets.Label(new Rect(BotLeft.x, (float)UI.screenHeight - BotLeft.y - rectY, 999f, 999f), "Natural water tile " + naturalWaterLabel);
                    rectY += 19f;
                }

                return rectY;
            }
            return num;
        }
    }


    //    static bool Prefix(Map map, GenStepParams parms)
    //    {
    //        NoiseRenderer.renderSize = new IntVec2(map.Size.x, map.Size.z);
    //        ModuleBase input = new Perlin(0.020999999716877937, 2.0, 0.5, 6, map.ConstantRandSeed, QualityMode.High);
    //        input = new ScaleBias(0.5, 0.5, input);
    //        NoiseDebugUI.StoreNoiseRender(input, "elev base");
    //        float num = 1f;
    //        switch (map.TileInfo.hilliness)
    //        {
    //            case Hilliness.Flat:
    //                num = MapGenTuning.ElevationFactorFlat;
    //                break;
    //            case Hilliness.SmallHills:
    //                num = MapGenTuning.ElevationFactorSmallHills;
    //                break;
    //            case Hilliness.LargeHills:
    //                num = MapGenTuning.ElevationFactorLargeHills;
    //                break;
    //            case Hilliness.Mountainous:
    //                num = MapGenTuning.ElevationFactorMountains;
    //                break;
    //            case Hilliness.Impassable:
    //                num = MapGenTuning.ElevationFactorImpassableMountains;
    //                break;
    //        }
    //        input = new Multiply(input, new Const(num));
    //        NoiseDebugUI.StoreNoiseRender(input, "elev world-factored");
    //        if (map.TileInfo.hilliness == Hilliness.Mountainous || map.TileInfo.hilliness == Hilliness.Impassable)
    //        {
    //            ModuleBase input2 = new DistFromAxis((float)map.Size.x * 0.42f);
    //            input2 = new Clamp(0.0, 1.0, input2);
    //            input2 = new Invert(input2);
    //            input2 = new ScaleBias(1.0, 1.0, input2);
    //            Rot4 random;
    //            do
    //            {
    //                random = Rot4.Random;
    //            }
    //            while (random == Find.World.CoastDirectionAt(map.Tile));
    //            if (random == Rot4.North)
    //            {
    //                input2 = new Rotate(0.0, 90.0, 0.0, input2);
    //                input2 = new Translate(0.0, 0.0, -map.Size.z, input2);
    //            }
    //            else if (random == Rot4.East)
    //            {
    //                input2 = new Translate(-map.Size.x, 0.0, 0.0, input2);
    //            }
    //            else if (random == Rot4.South)
    //            {
    //                input2 = new Rotate(0.0, 90.0, 0.0, input2);
    //            }
    //            else
    //            {
    //                _ = random == Rot4.West;
    //            }
    //            NoiseDebugUI.StoreNoiseRender(input2, "mountain");
    //            input = new Add(input, input2);
    //            NoiseDebugUI.StoreNoiseRender(input, "elev + mountain");
    //        }
    //        float b = (map.TileInfo.WaterCovered ? 0f : float.MaxValue);
    //        MapGenFloatGrid elevation = MapGenerator.Elevation;
    //        foreach (IntVec3 allCell in map.AllCells)
    //        {
    //            elevation[allCell] = Mathf.Min(input.GetValue(allCell), b);
    //        }
    //        ModuleBase input3 = new Perlin(0.020999999716877937, 2.0, 0.5, 6, map.ConstantRandSeed, QualityMode.High);
    //        input3 = new ScaleBias(0.5, 0.5, input3);
    //        NoiseDebugUI.StoreNoiseRender(input3, "noiseFert base");
    //        MapGenFloatGrid fertility = MapGenerator.Fertility;
    //        foreach (IntVec3 allCell2 in map.AllCells)
    //        {
    //            fertility[allCell2] = input3.GetValue(allCell2);
    //        }
    //        return false;
    //    }
    //}
}