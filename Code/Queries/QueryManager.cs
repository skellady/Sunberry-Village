using StardewValley.Internal;
using SunberryVillage.Utilities;

namespace SunberryVillage.Queries;
internal class QueryManager
{
	public static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.GameLaunched += RegisterQueries;
	}

	private static void RegisterQueries(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
	{
		ItemQueryResolver.Register("SunberryTeam.SBVSMAPI_ChooseNFromSet", ChooseNFromSetQuery.HandleChooseNFromSetQuery);
	}
}
