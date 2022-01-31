using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace WF
{
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
            else if (patch.Applied)
                patch.Remove();
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
        /// The patch code.
        /// </summary>
        public Action<T> Patch;
        /// <summary>
        /// The unpatch code - this should undo the patch code completely.
        /// </summary>
        public Action<T> Unpatch;

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
            if (CanPatch && !Applied)
            {
                Log.Message("[Water Freezes] " + (Name != null ? ("Applying patch \"" + Name + "\", patching ") : "Patching ") + TargetDescriptionString + "..");
                if (targetDef == null)
                    targetDef = DefDatabase<T>.GetNamed(TargetDefName);
                try
                {
                    Patch(targetDef);
                }
                catch (Exception ex)
                {
                    Log.Warning("[Water Freezes] Error " + (Name != null ? ("applying patch \"" + Name + "\"") : "patching ") + ". Most likely you have another mod that already patches " + TargetDescriptionString + ". Remove that mod or disable this patch in the mod options.\n\n" + ex.ToString());
                }
                Applied = true; //Set it as applied.
            }
        }

        /// <summary>
        /// Remove the patch if possible and necessary.
        /// </summary>
        public void Remove()
        {
            if (Applied) //If it's been applied already.
            {
                Log.Message("[Water Freezes] " + (Name != null ? ("Removing patch \"" + Name + "\", unpatching ") : "Unpatching ") + TargetDescriptionString + "..");
                if (targetDef == null)
                    targetDef = DefDatabase<T>.GetNamed(TargetDefName);
                try
                {
                    Unpatch(targetDef);
                }
                catch (Exception ex)
                {
                    Log.Warning("[Water Freezes] Error " + (Name != null ? ("removing patch \"" + Name + "\"") : "unpatching ") + ". Most likely you have another mod that already patches " + TargetDescriptionString + ", and it failed to patch in the first place. Remove that mod or disable this patch in the mod options.\n\n" + ex.ToString());
                }
                Applied = false; //Set it as not applied anymore.
            }
        }

        /// <summary>
        /// Whether we can patch this, depends on whether the mod it comes from is installed (always true if it's from vanilla).
        /// </summary>
        public bool CanPatch
        {
            get
            {
                if (TargetModID != null)
                {
                    if (!modInstalled.HasValue)
                    {
                        modInstalled = ModLister.GetActiveModWithIdentifier(TargetModID) != null;
                    }
                    return modInstalled.Value;
                }
                return true;
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
                Log.Message("[Water Freezes] Applying patches in patch group \"" + Name + "\"..");
                foreach (var patch in Patches)
                    patch.Apply();
                Applied = true;
            }
        }

        /// <summary>
        /// Remove the patch group's patches as possible and necessary.
        /// </summary>
        public void Remove()
        {
            if (Applied)
            {
                Log.Message("[Water Freezes] Removing patches in patch group \"" + Name + "\"..");
                foreach (var patch in Patches)
                    patch.Remove();
                Applied = false;
            }
        }
    }
}