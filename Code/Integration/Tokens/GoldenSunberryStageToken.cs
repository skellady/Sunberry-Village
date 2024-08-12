using StardewValley;
using StardewValley.Mods;
using SunberryVillage.Utilities;
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
	internal const int MaxStage = 5;
	internal const int MinNightsInStage = 7;

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
	};
	
	internal static readonly int[] HeartsNeededToAdvanceStage = { 10, 25, 50, 80, 120 };

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
		if (!Growing && Game1.MasterPlayer.hasOrWillReceiveMail(TriggerMailId))
		{
			Stage = 0;
			Growing = true;
		}

		// short circuit if not currently growing or if maxed out
		if (!Growing || Stage >= MaxStage)
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
		if (Stage >= MaxStage)
			return false;

		// probe means test whether the conditions will be true when the day ends, not whether they're true at this moment
		// only used in sbv.gs.info debug command
		if (probe)
			return NightsInStage + 1 >= MinNightsInStage && HeartTotal >= HeartsNeededToAdvanceStage[Stage];

		return NightsInStage >= MinNightsInStage && HeartTotal >= HeartsNeededToAdvanceStage[Stage] || ForceGrow;
	}

	internal static void UpdateHeartTotal()
	{
		HeartTotal = Utility.getAllCharacters().Where(npc => ResidentNames.Contains(npc.Name)).Sum(npc => Game1.MasterPlayer.getFriendshipHeartLevelForNPC(npc.Name));
	}
}
