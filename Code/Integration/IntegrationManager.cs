using SunberryVillage.Integration.APIs;
using SunberryVillage.Integration.Tokens;

namespace SunberryVillage.Integration;

internal class IntegrationManager
{
	private static IBetterMixedSeedsAPI BetterMixedSeedsApi;
	private static IContentPatcherAPI ContentPatcherApi;

	internal static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.GameLaunched += AddApis;
	}

	private static void AddApis(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
	{
		ContentPatcherApi = Globals.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
		BetterMixedSeedsApi = Globals.ModRegistry.GetApi<IBetterMixedSeedsAPI>("Satozaki.BetterMixedSeeds");

		if (ContentPatcherApi is not null)
		{
			Log.Info("Content Patcher API loaded, registering tokens.");
			RegisterContentPatcherTokens();
		}
		else
		{
			Log.Warn("Failed to load Content Patcher API, certain Sunberry Village components may be impacted.");
		}
		
		// not needed unless better mixed seeds gets ported to 1.6
		//if (BetterMixedSeedsApi is not null)
		//{
		//	Log.Info("Better Mixed Seeds API loaded, performing mod integration.");
		//	PerformBetterMixedSeedsIntegration();
		//}
		//else
		//{
		//	Log.Info("Better Mixed Seeds API failed to load, skipping mod integration.");
		//}
	}

	private static void RegisterContentPatcherTokens()
	{
		GoldenSunberryStageToken.AddEventHooks();
		ContentPatcherApi.RegisterToken(Globals.Manifest, "GoldenSunberryStage", () =>
			{
				return new[] { GoldenSunberryStageToken.Stage.ToString() };
			});
	}

	private static void PerformBetterMixedSeedsIntegration()
	{

		Globals.EventHelper.GameLoop.SaveLoaded += (_, _) =>
			{
				BetterMixedSeedsApi.ForceExcludeCrop("SBV Golden Sunberry Seeds");
			};
	}
}