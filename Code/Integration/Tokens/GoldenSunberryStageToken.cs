﻿using StardewValley;
using StardewValley.Mods;
using System.Collections.Generic;
using System.Linq;

namespace SunberryVillage.Integration.Tokens;

internal class GoldenSunberryStageToken
{
	internal static bool Growing;
	internal static bool ForceGrow;

	internal static int Stage = -1;
	internal static int NightsInStage;
	internal static int HeartTotal;

	internal const string TriggerMailId = "skellady.SBVCP_GSBTriggerLetter";
	internal const int MaxStage = 6;
	internal const int MinNightsInStage = 7;

	internal const string GrowingModDataKey = "SunberryTeam.SBVSMAPI/GoldenSunberry/Growing";
	internal const string StageModDataKey = "SunberryTeam.SBVSMAPI/GoldenSunberry/Stage";
	internal const string DaysInStageModDataKey = "SunberryTeam.SBVSMAPI/GoldenSunberry/DaysInStage";

	internal static readonly List<string> ResidentNames =
	[
		"AichaSBV",
		"AminaSBV",
		"AriSBV",
		"BlakeSBV",
		"DialaSBV",
		"DeryaSBV",
		"EliasSBV",
		"EzraSBV",
		"ImanSBV",
		"JumanaSBV",
		"LyenneSBV",
		"MaiaSBV",
		"MiyoungSBV",
		"MoonSBV",
		"OpheliaSBV",
		"ReihanaSBV",
		"SilasSBV",
		"NadiaSBV",
		"PanSBV",
		"WrenSong",
		"Jonghyuk",
		"Spanner",
		"AirynDao",
		"PS.Thysania",
		"PS.Michelle",
		"PS.Trevor",
		"PS.Greyscale",
		"TG.Valli",
		"LaniSBV",
		"WildflourSASS",
		"RoseSASS",
		"Yri",
		"Charles",
		"Sierra",
		"Bijou",
		"Clara.Ripley"
	];

	internal static readonly int[] HeartsNeededToAdvanceStage = [10, 25, 50, 80, 120, 120];

	internal static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.SaveLoaded += LoadOrInitStageVars;
		Globals.EventHelper.GameLoop.DayEnding += UpdateAndSaveGoldenSunberryStageVars;
	}

	private static void LoadOrInitStageVars(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
	{
		ModDataDictionary modData = Game1.MasterPlayer.modData;

		// cannot find GoldenSunberryGrowing moddata, assume no golden sunberry vars have been saved to moddata
		if (!modData.TryGetValue(GrowingModDataKey, out string growingModDataValue))
		{
			Growing = false;
			Stage = -1;
			NightsInStage = 0;
		}
		else
		{
			Growing = bool.Parse(growingModDataValue);
			Stage = modData.ContainsKey(StageModDataKey) ? int.Parse(modData[StageModDataKey]) : -1;
			NightsInStage = modData.ContainsKey(DaysInStageModDataKey) ? int.Parse(modData[DaysInStageModDataKey]) : 0;
		}

	}

	private static void UpdateAndSaveGoldenSunberryStageVars(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
	{
		// start growing on trigger mail being received
		if (!Growing && Game1.MasterPlayer.hasOrWillReceiveMail(TriggerMailId))
		{
			Stage = 0;
			Growing = true;
		}

		// short circuit if not currently growing or if maxed out
		if (!Growing || Stage >= MaxStage)
			return;

		// IMPORTANT only advance nights in stage after event for stage has been seen
		switch (Stage)
		{
			case 0 when Game1.MasterPlayer.eventsSeen.Contains("skellady.SBVCP_20031448"):
                NightsInStage++;
				break;

            case 1 when Game1.MasterPlayer.eventsSeen.Contains("skellady.SBVCP_20031451") || Game1.MasterPlayer.eventsSeen.Contains("skellady.SBVCP_20031452"):
                NightsInStage++;
                break;

			case 2 when Game1.MasterPlayer.eventsSeen.Contains("skellady.SBVCP_20031453"):
                NightsInStage++;
                break;

			case 3 when Game1.MasterPlayer.eventsSeen.Contains("skellady.SBVCP_20031454"):
                NightsInStage++;
                break;

			case 4 when Game1.MasterPlayer.eventsSeen.Contains("skellady.SBVCP_20031455"):
                NightsInStage++;
                break;

			case 5 when Game1.MasterPlayer.eventsSeen.Contains("skellady.SBVCP_20031456"):
                NightsInStage++;
                break;
		}

		UpdateHeartTotal();

		// has been in Stage for minimum length and has the required number of hearts to advance
		if (ReadyToAdvanceStage())
		{
			NightsInStage = 0;
			Stage++;
		}

		Game1.MasterPlayer.modData[GrowingModDataKey] = Growing.ToString();
		Game1.MasterPlayer.modData[StageModDataKey] = Stage.ToString();
		Game1.MasterPlayer.modData[DaysInStageModDataKey] = NightsInStage.ToString();
	}

	internal static bool ReadyToAdvanceStage(bool probe = false)
	{
		// cannot grow past max Stage
		if (Stage >= MaxStage)
			return false;

        // probe means test whether the conditions will be true when the day ends, not whether they're true at this moment - also ignores forcegrow for accurate diagnostics
        // only used in sbv.gs.info debug command
        return probe
            ? NightsInStage + 1 >= MinNightsInStage && HeartTotal >= HeartsNeededToAdvanceStage[Stage]
            : NightsInStage >= MinNightsInStage && HeartTotal >= HeartsNeededToAdvanceStage[Stage] || ForceGrow;
    }

    internal static void UpdateHeartTotal()
	{
		HeartTotal = Utility.getAllCharacters().Where(npc => ResidentNames.Contains(npc.Name)).Sum(npc => Game1.MasterPlayer.getFriendshipHeartLevelForNPC(npc.Name));
	}
}
