using StardewValley;

namespace SunberryVillage.Shops;
internal class PipPepShopNotification
{
	public static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.DayStarted += PopNotification;
	}

	private static void PopNotification(object? sender, StardewModdingAPI.Events.DayStartedEventArgs e)
	{
		if (!GameStateQuery.CheckConditions("DAY_OF_MONTH 6 10 22, PLAYER_HAS_SEEN_EVENT Host skellady.SBVCP_20031448") || Game1.MasterPlayer.DailyLuck < 0)
			return;

		Game1.addMorningFluffFunction(() =>
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromMaps:SBV.PipPepShopNotification"));
		});
	}
}