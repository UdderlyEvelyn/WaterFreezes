namespace WF;

public interface IToggleablePatch
{
    /// <summary>
    ///     The name of the patch.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Whether the patch is enabled, dictates what happens during processing.
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    ///     Whether the patch is presently applied.
    /// </summary>
    bool Applied { get; }

    /// <summary>
    ///     Apply the patch if possible and necessary.
    /// </summary>
    void Apply();

    /// <summary>
    ///     Remove the patch if possible and necessary.
    /// </summary>
    void Remove();
}