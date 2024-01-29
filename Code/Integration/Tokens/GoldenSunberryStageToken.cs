using StardewValley;
using StardewValley.Mods;
using SunberryVillage.Utilities;
using System.Collections.Generic;

namespace SunberryVillage.Integration.Tokens;

internal class GoldenSunberryStageToken
{
	internal static bool Growing = false;
	internal static bool ForceGrow = false;

	internal static int Stage = -1;
	internal static int NightsInStage = 0;
	internal static int HeartTotal = 0;

	internal const string TRIGGER_MAIL_ID = "skellady.SBVCP_GSBTriggerLetter";
	internal const int MAX_STAGE = 7;
	internal const int MIN_NIGHTS_IN_STAGE = 7;

	internal const string GrowingModDataKey = "SunberryTeam.SBVSMAPI/GoldenSunberry/Growing";
	internal const string StageModDataKey = "SunberryTeam.SBVSMAPI/GoldenSunberry/Stage";
	internal const string DaysInStageModDataKey = "SunberryTeam.SBVSMAPI/GoldenSunberry/DaysInStage";

	internal static readonly List<string> ResidentNames = new()
	{
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
		"WrenSong",
		"Jonghyuk",
		"AirynDao",
		"PS.Thysania",
		"PS.Michelle",
		"PS.Trevor",
		"PS.Greyscale",
		"TG.Valli",
		"Lani",
		"WildflourSASS",
		"RoseSASS",
		"Yri",
		"Charles",
		"Sierra",
		"Bijou",
		"Clara.Ripley"
	};

	//todo: put actual thresholds in
	internal static readonly int[] HeartsNeededToAdvanceStage = new int[] { 10, 25, 50, 80, 120 };

	internal static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.SaveLoaded += LoadOrInitStageVars;
		Globals.EventHelper.GameLoop.DayEnding += UpdateAndSaveGoldenSunberryStageVars;
	}

	private static void LoadOrInitStageVars(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
	{
		ModDataDictionary modData = Game1.MasterPlayer.modData;

		// cannot find GoldenSunberryGrowing moddata, assume no golden sunberry vars have been saved to moddata
		if (!modData.TryGetValue(GrowingModDataKey, out string GrowingModDataValue))
		{
			Growing = false;
			Stage = -1;
			NightsInStage = 0;
		}
		else
		{
			Growing = bool.Parse(GrowingModDataValue);
			Stage = modData.ContainsKey(StageModDataKey) ? int.Parse(modData[StageModDataKey]) : -1;
			NightsInStage = modData.ContainsKey(DaysInStageModDataKey) ? int.Parse(modData[DaysInStageModDataKey]) : 0;
		}

	}

	private static void UpdateAndSaveGoldenSunberryStageVars(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
	{
		// "is true" is needed bc this statement returns a nullable bool, so we need to make sure it's 1. not null and 2. not false
		if (!Growing && Game1.MasterPlayer.hasOrWillReceiveMail(TRIGGER_MAIL_ID))
		{
			Stage = 0;
			Growing = true;
		}

		// short circuit if not currently growing or if maxed out
		if (!Growing || Stage >= MAX_STAGE)
			return;

		NightsInStage++;
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
		if (Stage >= MAX_STAGE)
			return false;

		// probe means test whether the conditions will be true when the day ends, not whether they're true at this moment
		// only used in sbv.gs.info debug command
		if (probe)
			return NightsInStage + 1 >= MIN_NIGHTS_IN_STAGE && HeartTotal >= HeartsNeededToAdvanceStage[Stage];

		return NightsInStage >= MIN_NIGHTS_IN_STAGE && HeartTotal >= HeartsNeededToAdvanceStage[Stage] || ForceGrow;
	}

	internal static void UpdateHeartTotal()
	{
		int heartTotal = 0;

		foreach (NPC npc in Utility.getAllCharacters())
		{
			if (ResidentNames.Contains(npc.Name))
				heartTotal += Game1.MasterPlayer.getFriendshipHeartLevelForNPC(npc.Name);
		}

		HeartTotal = heartTotal;
	}
}
