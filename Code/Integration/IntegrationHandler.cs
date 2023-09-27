using SunberryVillage.Code.Integration.APIs;
using SunberryVillage.Utilities;

namespace SunberryVillage.Code.Integration;

internal class IntegrationHandler
{
	private static IBetterMixedSeedsApi BetterMixedSeedsApi;

	internal static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.GameLaunched += AddApis;
	}

	private static void AddApis(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
	{
		BetterMixedSeedsApi = Globals.ModRegistry.GetApi<IBetterMixedSeedsApi>("Satozaki.BetterMixedSeeds");

		if (BetterMixedSeedsApi is not null)
		{
			Log.Info("Better Mixed Seeds detected, performing mod integration.");
			PerformBetterMixedSeedsIntegration();
		}
		else
		{
			Log.Info("Better Mixed Seeds not detected, skipping mod integration.");
		}

	}

	private static void PerformBetterMixedSeedsIntegration()
	{

		Globals.EventHelper.GameLoop.SaveLoaded += (_, _) =>
			{
				BetterMixedSeedsApi.ForceExcludeCrop("SBV Golden Sunberry Seeds", "SBV Golden Sunberry");
			};
	}
}