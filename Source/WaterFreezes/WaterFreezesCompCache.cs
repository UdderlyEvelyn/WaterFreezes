using System.Collections.Generic;
using Verse;

namespace WF;

//This stores the comp on a per-map basis so that patches don't have to constantly retrieve it.
public static class WaterFreezesCompCache
{
    public static readonly Dictionary<int, MapComponent_WaterFreezes> compCachePerMap =
        new Dictionary<int, MapComponent_WaterFreezes>();

    public static MapComponent_WaterFreezes GetFor(Map map)
    {
        MapComponent_WaterFreezes comp; //Set up var.
        if (!compCachePerMap.TryGetValue(map.uniqueID, out var value)) //If not cached.
        {
            compCachePerMap.Add(map.uniqueID, comp = map.GetComponent<MapComponent_WaterFreezes>()); //Get and cache.
        }
        else
        {
            comp = value; //Retrieve from cache.
        }

        return comp;
    }

    public static void SetFor(Map map, MapComponent_WaterFreezes comp)
    {
        //If not cached.
        //Cache.
        //Reassign cache.
        compCachePerMap[map.uniqueID] = comp;
    }
}