using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace WF
{
	public static class TerrainSystemOverhaul_Interop
	{
		public static bool TerrainSystemOverhaulPresent => _terrainSystemOverhaulUtilsType != null;
		private static Type _terrainSystemOverhaulUtilsType = Type.GetType("TSO.Utils, TSO");
		private static Func<TerrainGrid, IntVec3, TerrainDef> _getBridgeDelegate;

		/// <summary>
		/// Call Utils.GetBridge without needing to reference the assembly.
		/// </summary>
		/// <param name="terrGrid"></param>
		/// <param name="cell">cell to get the a bridge def at</param>
		/// <returns>TerrainDef of bridge at cell, null if none</returns>
		public static TerrainDef GetBridge(TerrainGrid terrGrid, IntVec3 c)
		{
			if (_terrainSystemOverhaulUtilsType != null)
			{
				if (_getBridgeDelegate == null) //Everything in here should only execute once if the mod is present.
					_getBridgeDelegate = (Func<TerrainGrid, IntVec3, TerrainDef>)_terrainSystemOverhaulUtilsType.GetMethod("GetBridge").CreateDelegate(typeof(Func<TerrainGrid, IntVec3, TerrainDef>));
				return _getBridgeDelegate(terrGrid, c);
			}
			return null; //Mod not loaded, return null.
		}
	}
}
