using Verse;

namespace WF;

public class TerrainExtension_WaterStats : DefModExtension
{
    /// <summary>
    ///     An offset from 0C for the freezing point of this type of water (used only on water defs).
    /// </summary>
    public float FreezingPoint;

    /// <summary>
    ///     The def used for regular ice (optional, second stage of thickness, used only on water defs).
    /// </summary>
    public TerrainDef IceDef;

    /// <summary>
    ///     Whether this water is considered moving - this affects whether ice lasts longer near land (if true) or away from
    ///     land (if false).
    /// </summary>
    public bool IsMoving;

    /// <summary>
    ///     The most ice a cell with this type of water terrain can hold (used only on water defs).
    /// </summary>
    public float MaxIceDepth;

    /// <summary>
    ///     The most water a cell with this type of water terrain can hold (used only on water defs).
    /// </summary>
    public float MaxWaterDepth;

    /// <summary>
    ///     The def used for thick ice (optional, third stage of thickness, used only on water defs).
    /// </summary>
    public TerrainDef ThickIceDef;

    /// <summary>
    ///     The def used for thin ice (mandatory if you want it to change terrain to ice at all, used only on water defs).
    /// </summary>
    public TerrainDef ThinIceDef;
}