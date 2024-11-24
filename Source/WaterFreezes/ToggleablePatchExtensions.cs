namespace WF;

public static class ToggleablePatchExtensions
{
    /// <summary>
    ///     Apply or remove the patch as dictated by the current status.
    /// </summary>
    /// <param name="patch">the patch to process</param>
    public static void Process(this IToggleablePatch patch)
    {
        if (patch.Enabled)
        {
            patch.Apply();
        }
        else if (patch.Applied) //If it's not enabled, but it is applied.
        {
            patch.Remove();
        }
        else
        {
            ToggleablePatch.MessageLoggingMethod(
                $"[ToggleablePatch] Skipping patch {(patch is ToggleablePatchGroup ? "group" : "")}\"{patch.Name}\" because it is disabled.");
        }
    }
}