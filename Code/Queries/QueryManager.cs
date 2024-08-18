using StardewValley;
using StardewValley.Internal;
using SunberryVillage.Utilities;

namespace SunberryVillage.Queries;
internal class QueryManager
{
	public static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.GameLaunched += RegisterQueriesAndPreconditions;
	}

	private static void RegisterQueriesAndPreconditions(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
	{
		ItemQueryResolver.Register("SunberryTeam.SBVSMAPI_ChooseNFromSet", ChooseNFromSetQuery.HandleChooseNFromSetQuery);
		GameStateQuery.Register("SunberryTeam.SBVSMAPI_WeatherTomorrow", WeatherTomorrowQuery.HandleWeatherTomorrowQuery);
		Event.RegisterPrecondition("SunberryTeam.SBVSMAPI_IsPassiveFestivalToday", IsPassiveFestivalPrecondition.IsPassiveFestivalToday);
		Event.RegisterPrecondition("SunberryTeam.SBVSMAPI_IsPassiveFestivalOnGivenDay", IsPassiveFestivalPrecondition.IsPassiveFestivalOnGivenDay);
	}
}