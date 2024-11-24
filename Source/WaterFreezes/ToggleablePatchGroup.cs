using System.Collections.Generic;

namespace WF;

/// <summary>
///     A group of patches to be considered together for toggling. These ignore the child Enabled setting in favor of the
///     group one.
/// </summary>
public class ToggleablePatchGroup : IToggleablePatch
{
    /// <summary>
    ///     The patches that the patch group consists of.
    /// </summary>
    public List<IToggleablePatch> Patches;

    /// <summary>
    ///     The name of the patch group.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Whether or not the patch group is enabled, this determines what happens when it is processed.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Whether the patch group is presently applied.
    /// </summary>
    public bool Applied { get; protected set; }

    /// <summary>
    ///     Apply the patch group's patches as possible and necessary.
    /// </summary>
    public void Apply()
    {
        if (!Applied)
        {
            ToggleablePatch.MessageLoggingMethod($"[ToggleablePatch] Applying patches in patch group \"{Name}\"..");
            foreach (var patch in Patches)
            {
                patch.Apply();
            }

            Applied = true;
        }
        else
        {
            ToggleablePatch.MessageLoggingMethod(
                $"[ToggleablePatch] Skipping application of patch group \"{Name}\" because it is already applied.");
        }
    }

    /// <summary>
    ///     Remove the patch group's patches as possible and necessary.
    /// </summary>
    public void Remove()
    {
        if (Applied)
        {
            ToggleablePatch.MessageLoggingMethod($"[ToggleablePatch] Removing patches in patch group \"{Name}\"..");
            foreach (var patch in Patches)
            {
                patch.Remove();
            }

            Applied = false;
        }
        else
        {
            ToggleablePatch.MessageLoggingMethod(
                $"[ToggleablePatch] Skipping removal of patch group \"{Name}\" because it is not applied.");
        }
    }
}