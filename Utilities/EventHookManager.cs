using StardewValley;
using SunberryVillage.Patching;

namespace SunberryVillage.Utilities;

internal class EventHookManager
{
	internal static void InitializeEventHooks()
	{
		Globals.EventHelper.Content.AssetRequested += AssetManager.LoadOrEditAssets;
		Globals.EventHelper.GameLoop.DayEnding += (_, _) =>
		{
			Game1.player.modData.Remove("sophie.DialaTarot/ReadingDoneForToday");
		};
	}
}
