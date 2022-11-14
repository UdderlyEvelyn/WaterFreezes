using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace WF
{
    //This stores the comp on a per-map basis so that patches don't have to constantly retrieve it.
    public static class WaterFreezesCompCache
    {
        public static Dictionary<int, MapComponent_WaterFreezes> compCachePerMap = new Dictionary<int, MapComponent_WaterFreezes>();

        public static MapComponent_WaterFreezes GetFor(Map map)
        {
            MapComponent_WaterFreezes comp; //Set up var.
            if (!compCachePerMap.ContainsKey(map.uniqueID)) //If not cached..
                compCachePerMap.Add(map.uniqueID, comp = map.GetComponent<MapComponent_WaterFreezes>()); //Get and cache.
            else
                comp = compCachePerMap[map.uniqueID]; //Retrieve from cache.
            return comp;
        }

        public static void SetFor(Map map, MapComponent_WaterFreezes comp)
        {
            if (!compCachePerMap.ContainsKey(map.uniqueID)) //If not cached..
                compCachePerMap.Add(map.uniqueID, comp); //Cache.
            else
                compCachePerMap[map.uniqueID] = comp; //Reassign cache.
        }
    }
}
