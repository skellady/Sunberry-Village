using HarmonyLib;
using StardewValley;
using StardewValley.Quests;
using System;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.ConversationTopics;

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