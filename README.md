# [Water Freezes (Continued)](https://steamcommunity.com/sharedfiles/filedetails/?id=3278128973)

![Image](https://i.imgur.com/buuPQel.png)

Update of UdderlyEvelyns mod https://steamcommunity.com/sharedfiles/filedetails/?id=2728000958



-  Watermills should now just stop when frozen and not break down



![Image](https://i.imgur.com/pufA0kM.png)
	
![Image](https://i.imgur.com/Z4GOv8H.png)

# Features



- Lakes, marshes, and rivers can now freeze when it is sufficiently cold out!
- Water and ice depth tracking system, as water freezes it turns to ice, and as ice melts it turns to water.
- Relatively realistic thermodynamic simulation yet still very fast!
- Thicker ice melts slower than thinner ice, and lake depth is estimated based on distance from land and things freeze from the outside in.
- Buildings that can't exist on ice but can exist on water have breakdowns or are destroyed when water freezes under them. Buildings that can't exist on water but you build on the ice will be destroyed when the terrain supporting them thaws. Buildings on bridges (including modded ones) are immune, naturally.



# Interoperates With [Soil Relocation Framework](https://steamcommunity.com/sharedfiles/filedetails/?id=2654088143)!

[![Image](https://steamuserimages-a.akamaihd.net/ugc/2477620729410521986/0E7C57C6A526D7284A0734A7F2B1C82A28F2BF66/?imw=5000&imh=5000&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=false)](https://steamcommunity.com/sharedfiles/filedetails/?id=3248607572)


- Dig up ice, more ice is given if the cell has more ice in it, but it takes longer!
- When you dig out ice, it leaves behind any water that wasn't frozen underneath (if any), allowing you to fish if you have a mod that allows it.
- Use the ice to cool rooms! Tribal freezers! Solar flare backup cooling!
- Natural lakes refill in the spring if their ice is removed. Artificial lakes do not (if you have the means to make them)!



# Coming Soon



- Ice skating recreation type.
- Use ice as a material for sculptures that can cool rooms and melt away depending on where you keep it (with Soil Relocation Framework).
- Oceans can freeze, even, if it gets cold enough!
- More!



# FAQ

Q: Can this be added to an existing save?
A: Yes! Any existing water on the map will be considered "natural", though, even if it didn't generate with the map (you can use debug tools to correct anything you wish to, though).

Q: Can this be safely removed from a save?
A: If no ice from this mod is present, yes - no promises otherwise! A quick way to remove all ice is to use the "Reinitialize" debug action.

Q: I used debug tools to change water to something else and it reverted, why?
A: Use the "Clear Natural Water Status" tool (or a tool that includes this function) first, natural water reverts during spring/summer and SetTerrain doesn't clear it on its own.

# Compatibility



- Designed to work with [Soil Relocation Framework](https://steamcommunity.com/sharedfiles/filedetails/?id=3248607572)
- Proxy Heat: Should work just fine, designed with this in mind.
- Celsius: Disable their freezing/melting system or they will conflict, should work just fine otherwise.
- Tested and working with the Multiplayer mod!
- Compatible with Terrain System Overhaul.
- If you use a terraforming mod that allows messing with water to change a *natural* bit of water terrain *it will revert*. I will be adding the ability to fill water through Soil Relocation Framework soon, or you can use debug options to set things as not natural water anymore.
- If any mods buildings that go on water don't seem to act right with freezing/unfreezing feel free to ask about it.



Huge thanks to ZenthWolf, my official tester/troubleshooter, without whom it would take me longer to get patches out.

water freeze ice lake river ocean salt fresh thermodynamic simulation depth thickness dig gather freezer refrigerator

![Image](https://i.imgur.com/PwoNOj4.png)



-  See if the the error persists if you just have this mod and its requirements active.
-  If not, try adding your other mods until it happens again.
-  Post your error-log using [HugsLib](https://steamcommunity.com/workshop/filedetails/?id=818773962) or the standalone [Uploader](https://steamcommunity.com/sharedfiles/filedetails/?id=2873415404) and command Ctrl+F12
-  For best support, please use the Discord-channel for error-reporting.
-  Do not report errors by making a discussion-thread, I get no notification of that.
-  If you have the solution for a problem, please post it to the GitHub repository.
-  Use [RimSort](https://github.com/RimSort/RimSort/releases/latest) to sort your mods



[![Image](https://img.shields.io/github/v/release/emipa606/WaterFreezes?label=latest%20version&style=plastic&color=9f1111&labelColor=black)](https://steamcommunity.com/sharedfiles/filedetails/changelog/3278128973)
