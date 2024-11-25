using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Reflection;

namespace WF
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ToggleablePatch : Attribute
    {
        public static bool AutoScan = true;
        protected static bool _performedPatchScan = false;
        public static Action<string> MessageLoggingMethod = Log.Message;
        public static Action<string> WarningLoggingMethod = Log.Warning;
        public static Action<string> ErrorLoggingMethod = Log.Error;
        public static List<IToggleablePatch> Patches = new List<IToggleablePatch>();

        public static void ScanForPatches()
        {
            if (!_performedPatchScan)
            {
                var types = Assembly.GetExecutingAssembly().GetTypes();
                var members = types.SelectMany(type => type.GetMembers(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static
                        )
                );
                var iToggleablePatchType = typeof(IToggleablePatch);
                foreach (var member in members)
                    if (member.HasAttribute<ToggleablePatch>())
                    {
                        if (member is FieldInfo field)
                        {
                            if (field.FieldType.GetInterfaces().Contains(iToggleablePatchType))
                                Patches.Add((IToggleablePatch)field.GetValue(null));
                            else ErrorLoggingMethod("[ToggleablePatch] Field \"" + field.Name + "\" is marked with ToggleablePatch attribute but does not implement IToggleablePatch.");
                        }
                        else if (member is PropertyInfo property)
                        {
                            if (property.PropertyType.GetInterfaces().Contains(iToggleablePatchType))
                                Patches.Add((IToggleablePatch)property.GetValue(null));
                            else ErrorLoggingMethod("[ToggleablePatch] Property \"" + property.Name + "\" is marked with ToggleablePatch attribute but does not implement IToggleablePatch.");
                        }
                    }
                _performedPatchScan = true;
            }
        }

        public static void AddPatches(params IToggleablePatch[] patches)
        {
            Patches.AddRange(patches);
        }

        /// <summary>
        /// Process the patches stored in ToggleablePatch.Patches.
        /// </summary>
        /// <param name="reason">the reason to process them, optional, shown in logging</param>
        public static void ProcessPatches(string modID, string reason = null)
        {
            if (AutoScan)
                ScanForPatches();
            MessageLoggingMethod("[ToggleablePatch] Processing " + Patches.Count + " patches" + (reason != null ? " because " + reason : "") + " for \"" + modID + "\"..");
            foreach (var patch in Patches)
                patch.Process();
        }
    }

    public static class ToggleablePatchExtensions
    {
        /// <summary>
        /// Apply or remove the patch as dictated by the current status.
        /// </summary>
        /// <param name="patch">the patch to process</param>
        public static void Process(this IToggleablePatch patch)
        {
            if (patch.Enabled)
                patch.Apply();
            else if (patch.Applied) //If it's not enabled but it is applied..
                patch.Remove();
            else
                ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] Skipping patch " + (patch is ToggleablePatchGroup ? "group" : "") + "\"" + patch.Name + "\" because it is disabled.");
        }
    }

    public interface IToggleablePatch
    {
        /// <summary>
        /// The name of the patch.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Whether the patch is enabled, dictates what happens during processing.
        /// </summary>
        bool Enabled { get; }
        /// <summary>
        /// Whether the patch is presently applied.
        /// </summary>
        bool Applied { get; }
        /// <summary>
        /// Apply the patch if possible and necessary.
        /// </summary>
        void Apply();
        /// <summary>
        /// Remove the patch if possible and necessary.
        /// </summary>
        void Remove();
    }

    public class ToggleablePatch<T> : IToggleablePatch where T : Def
    {
        /// <summary>
        /// Cache variable for the target def.
        /// </summary>
        protected T targetDef;
        /// <summary>
        /// Cache variable for whether the mod that is targeted is installed.
        /// </summary>
        protected bool? modInstalled;
        /// <summary>
        /// Name of the patch.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The mod ID of the mod that is targeted by this patch.
        /// </summary>
        public string TargetModID;
        /// <summary>
        /// The def name of the def targeted by this patch.
        /// </summary>
        public string TargetDefName;
        /// <summary>
        /// Whether the patch is enabled or not, dictates where it will be applied or removed when processed.
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Whether the patch is presently applied or not.
        /// </summary>
        public bool Applied { get; protected set; }
        /// <summary>
        /// List of conflicting mod IDs that this patch will not be applied if are present.
        /// </summary>
        public List<string> ConflictingModIDs = new();
        /// <summary>
        /// The patch code.
        /// </summary>
        public Action<ToggleablePatch<T>, T> Patch;
        /// <summary>
        /// The unpatch code - this should undo the patch code completely.
        /// </summary>
        public Action<ToggleablePatch<T>, T> Unpatch;
        /// <summary>
        /// A space to save data from the patching process to be used by the unpatching process, primarily for restoring non-vanilla data in destructive patches.
        /// </summary>
        public object State;

        /// <summary>
        /// Returns the target as a string in the form of ModID.DefName (DefType.FullName) with the "ModID." missing if no Mod ID is assigned (e.g., it's Vanilla or unconditional).
        /// </summary>
        public string TargetDescriptionString
        {
            get
            {
                return (TargetModID != null ? TargetModID + "." : "") + TargetDefName + " (" + typeof(T).FullName + ")";
            }
        }

        /// <summary>
        /// Apply the patch if possible and necessary.
        /// </summary>
        public void Apply()
        {
            if (CanPatch)
            {
                if (!Applied)
                {
                    ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] " + (Name != null ? ("Applying patch \"" + Name + "\", patching ") : "Patching ") + TargetDescriptionString + "..");
                    if (targetDef == null)
                        targetDef = DefDatabase<T>.GetNamed(TargetDefName);
                    try
                    {
                        Patch(this, targetDef);
                    }
                    catch (Exception ex)
                    {
                        ToggleablePatch.ErrorLoggingMethod("[ToggleablePatch] Error " + (Name != null ? ("applying patch \"" + Name + "\"") : "patching ") + ". Most likely you have another mod that already patches " + TargetDescriptionString + ". Remove that mod or disable this patch in the mod options.\n\n" + ex.ToString());
                    }
                    Applied = true; //Set it as applied.
                }
                else
                    ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] Skipping application of patch \"" + Name + "\" because it is already applied.");
            }
            else
                ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] Skipping application of patch \"" + Name + "\" because it cannot be applied.");
        }

        /// <summary>
        /// Remove the patch if possible and necessary.
        /// </summary>
        public void Remove()
        {
            if (Applied) //If it's been applied already.
            {
                ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] " + (Name != null ? ("Removing patch \"" + Name + "\", unpatching ") : "Unpatching ") + TargetDescriptionString + "..");
                if (targetDef == null)
                    targetDef = DefDatabase<T>.GetNamed(TargetDefName);
                try
                {
                    Unpatch(this, targetDef);
                }
                catch (Exception ex)
                {
                    ToggleablePatch.ErrorLoggingMethod("[ToggleablePatch] Error " + (Name != null ? ("removing patch \"" + Name + "\"") : "unpatching ") + ". Most likely you have another mod that already patches " + TargetDescriptionString + ", and it failed to patch in the first place. Remove that mod or disable this patch in the mod options.\n\n" + ex.ToString());
                }
                Applied = false; //Set it as not applied anymore.
            }
            else
                ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] Skipping removal of patch \"" + Name + "\" because it is not applied.");
        }

        /// <summary>
        /// Whether we can patch this, depends on whether the mod it comes from is installed (always true if it's from vanilla).
        /// </summary>
        public bool CanPatch
        {
            get
            {
                foreach (var modID in ConflictingModIDs)
                    if (ModLister.GetActiveModWithIdentifier(modID) != null) //If mod present..
                    {
                        ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] Skipping patch \"" + Name + "\" because conflicting mod with ID \"" + modID + "\" was found.");
                        return false; //Can't patch.
                    }
                if (TargetModID != null) //If it has a target mod..
                {
                    if (!modInstalled.HasValue) //If mod presence is unknown..
                        modInstalled = ModLister.GetActiveModWithIdentifier(TargetModID) != null; //Check presence..
                    return modInstalled.Value; //Return true if it is installed, false if not..
                }
                return true; //Return true if it had no target mod ID and no conflicting mods stopped it before now.
            }
        }
    }

    /// <summary>
    /// A group of patches to be considered together for toggling. These ignore the child Enabled setting in favor of the group one.
    /// </summary>
    public class ToggleablePatchGroup : IToggleablePatch
    {
        /// <summary>
        /// The name of the patch group.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Whether or not the patch group is enabled, this determines what happens when it is processed.
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Whether or not the patch group is presently applied.
        /// </summary>
        public bool Applied { get; protected set; }
        /// <summary>
        /// The patches that the patch group consists of.
        /// </summary>
        public List<IToggleablePatch> Patches;

        /// <summary>
        /// Apply the patch group's patches as possible and necessary.
        /// </summary>
        public void Apply()
        {
            if (!Applied)
            {
                ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] Applying patches in patch group \"" + Name + "\"..");
                foreach (var patch in Patches)
                    patch.Apply();
                Applied = true;
            }
            else
                ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] Skipping application of patch group \"" + Name + "\" because it is already applied.");
        }

        /// <summary>
        /// Remove the patch group's patches as possible and necessary.
        /// </summary>
        public void Remove()
        {
            if (Applied)
            {
                ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] Removing patches in patch group \"" + Name + "\"..");
                foreach (var patch in Patches)
                    patch.Remove();
                Applied = false;
            }
            else
                ToggleablePatch.MessageLoggingMethod("[ToggleablePatch] Skipping removal of patch group \"" + Name + "\" because it is not applied.");
        }
    }
}