using System;
using StardewModdingAPI.Events;
using StardewValley;
using SunberryVillage.Integration.Tokens;

namespace SunberryVillage.Utilities;

internal class SaveFixer
{
    internal const string GSBFix = "SunberryTeam.SBVSMAPI_GSBFix";


    internal static void AddEventHooks()
    {
        Globals.EventHelper.GameLoop.SaveLoaded += ApplyFixes;
    }

    private static void ApplyFixes(object sender, SaveLoadedEventArgs e)
    {
        if (!Game1.player.mailReceived.Contains(GSBFix))
        {
            ApplyGSBFix();
            Game1.player.mailReceived.Add(GSBFix);
        }
    }

    private static void ApplyGSBFix()
    {
        var seen = Game1.player.eventsSeen;

        if (seen.Contains("skellady.SBVCP_20031448"))
        {
            GoldenSunberryStageToken.Stage = 0;
            GoldenSunberryStageToken.NightsInStage = 0;
        }

        if (seen.Contains("skellady.SBVCP_20031451"))
        {
            GoldenSunberryStageToken.Stage = 1;
            GoldenSunberryStageToken.NightsInStage = 0;
        }

        if (seen.Contains("skellady.SBVCP_20031453"))
        {
            GoldenSunberryStageToken.Stage = 2;
            GoldenSunberryStageToken.NightsInStage = 0;
        }

        if (seen.Contains("skellady.SBVCP_20031454"))
        {
            GoldenSunberryStageToken.Stage = 3;
            GoldenSunberryStageToken.NightsInStage = 0;
        }

        if (seen.Contains("skellady.SBVCP_20031455"))
        {
            GoldenSunberryStageToken.Stage = 4;
            GoldenSunberryStageToken.NightsInStage = 0;
        }

        if (seen.Contains("skellady.SBVCP_20031456"))
        {
            GoldenSunberryStageToken.Stage = 5;
            GoldenSunberryStageToken.NightsInStage = 0;
        }

    }
}