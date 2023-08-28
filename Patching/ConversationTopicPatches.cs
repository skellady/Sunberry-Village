using HarmonyLib;
using StardewValley;
using StardewValley.Quests;
using SunberryVillage.Utilities;
using System;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Patching;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class ConversationTopicPatches
{

	/// <summary>
	/// Patches <c>Dialogue.checkForSpecialDialogueAttributes</c> to check for CT addition.<br />
	/// Syntax:  ===CTName=== for a CT with a duration of 1 day, or
	///			 ===CTName/[number]=== for a CT with a duration of [number] days.<br />
	///	Example: ===MyConversationTopic1/20=== would add a CT with name MyConversationTopic1 and duration of 20 days.
	/// </summary>
	[HarmonyPatch(typeof(Dialogue), "checkForSpecialDialogueAttributes")]
	[HarmonyPostfix]
	public static void checkForSpecialDialogueAttributes_Postfix(Dialogue __instance)
	{
		try
		{
			// get current dialogue line
			string currentDialogue = __instance.dialogues[__instance.currentDialogueIndex];

			// define pattern and search for matches in current line
			Regex pattern = new(@"={3}(?<ctName>[A-Za-z0-9_]+)(?:\/(?<duration>\d+))?={3}");
			MatchCollection matches = pattern.Matches(currentDialogue);

			// for each match, add a CT with the corresponding ctName group. If duration is specified, use that duration; otherwise, default to 1.
			foreach (Match match in matches)
			{
				string ctName = match.Groups["ctName"].Value;
				int duration = match.Groups["duration"].Value != "" ? int.Parse(match.Groups["duration"].Value) : 1;

				if (Game1.player.activeDialogueEvents.ContainsKey(ctName))
					Game1.player.activeDialogueEvents[ctName] = duration;
				else
					Game1.player.activeDialogueEvents.Add(ctName, duration);
			}

			// remove any matches from dialogue
			if (matches.Count > 0)
				__instance.dialogues[__instance.currentDialogueIndex] = pattern.Replace(currentDialogue, "");
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(ConversationTopicPatches)}::{nameof(checkForSpecialDialogueAttributes_Postfix)}\" has encountered an error while handling dialogue for {__instance.speaker?.Name ?? "unknown NPC"}: \"{__instance.dialogues[__instance.currentDialogueIndex]}\". \n{e}");
		}
	}

	/// <summary>
	/// Patches <c>Quest.questComplete</c> to add a CT generated from the details of the quest that was just completed.
	/// </summary>
	/// <param name="__instance">The quest that has been completed.</param>
	[HarmonyPatch(typeof(Quest), nameof(Quest.questComplete))]
	[HarmonyPostfix]
	public static void questComplete_Postfix(Quest __instance)
	{
		try
		{
			string title = __instance.questTitle.Replace(" ", "");
			string name = __instance.questType.Value switch
			{
				3 => (__instance as ItemDeliveryQuest)?.target.Value,
				4 => (__instance as SlayMonsterQuest)?.target.Value,
				7 => (__instance as FishingQuest)?.target.Value,
				10 => (__instance as ResourceCollectionQuest)?.target.Value,
				_ => null
			};

			string questKey = "QuestComplete_" + title + (name is not null ? "_" + name : "");

			if (Game1.player.activeDialogueEvents.ContainsKey(questKey))
				return;

			Game1.player.activeDialogueEvents.Add(questKey, __instance.dailyQuest.Value ? 1 : 7);
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(ConversationTopicPatches)}::{nameof(questComplete_Postfix)}\" has encountered an error while handling conversation topic for completion of quest \"{__instance.questTitle}\": \n{e}");
		}
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression