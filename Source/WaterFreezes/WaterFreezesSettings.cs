using Verse;

namespace WF;

public class WaterFreezesSettings : ModSettings
{
    public static int IceRate = 1000;
    public static float FreezingFactor = 4f;
    public static float ThawingFactor = 2f;
    public static bool MoisturePumpClearsNaturalWater;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref IceRate, "IceRate", 1000);
        Scribe_Values.Look(ref FreezingFactor, "FreezingFactor", 4f);
        Scribe_Values.Look(ref ThawingFactor, "ThawingFactor", 2f);
        Scribe_Values.Look(ref MoisturePumpClearsNaturalWater, "MoisturePumpClearsNaturalWater");

        base.ExposeData();
    }
}