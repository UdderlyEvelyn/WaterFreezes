using Mlie;
using UnityEngine;
using Verse;

namespace WF;

public class WaterFreezesMod : Mod
{
    public static string currentVersion;
    private string _freezingMultiplierBuffer;

    private string _iceRateBuffer;
    private string _thawingDivisorBuffer;
    public WaterFreezesSettings Settings;

    public WaterFreezesMod(ModContentPack content) : base(content)
    {
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
        Settings = GetSettings<WaterFreezesSettings>();
    }

    public override string SettingsCategory()
    {
        return "Water Freezes";
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);
        listingStandard.Label("WFM.tickInfo".Translate());
        listingStandard.TextFieldNumeric(ref WaterFreezesSettings.IceRate, ref _iceRateBuffer, 500, 2500);
        listingStandard.GapLine();
        listingStandard.Label("WFM.multiplierInfo".Translate());
        listingStandard.TextFieldNumericLabeled("WFM.freezingFactor".Translate(),
            ref WaterFreezesSettings.FreezingFactor, ref _freezingMultiplierBuffer, 1);
        listingStandard.Label("WFM.freezingFactorDesc".Translate());
        listingStandard.TextFieldNumericLabeled("WFM.thawingFactor".Translate(), ref WaterFreezesSettings.ThawingFactor,
            ref _thawingDivisorBuffer, 1);
        listingStandard.CheckboxLabeled("WFM.moisturePump".Translate(),
            ref WaterFreezesSettings.MoisturePumpClearsNaturalWater, "WFM.moisturePumpDesc".Translate());
        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("WFM.modVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.End();
        base.DoSettingsWindowContents(inRect);
    }
}