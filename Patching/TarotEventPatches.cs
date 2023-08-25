using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile.Dimensions;
using SunberryVillage.TarotEvent;
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
internal class TarotEventPatches
{

	/// <summary>
	/// Patches <c>GameLocation.performAction</c> to check for the "DialaTarot" action and handle it accordingly.
	/// </summary>
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction))]
	[HarmonyPrefix]
	public static bool performAction_Prefix(string action, Farmer who, Location tileLocation)
	{
		if (action != "DialaTarot" || !who.IsLocalPlayer)
			return true;

		GameLocation currentLoc = Game1.currentLocation;

		if (currentLoc.characters.Any(npc => npc.Name == "DialaSBV" && Vector2.Distance(npc.getTileLocation(), new Vector2(tileLocation.X, tileLocation.Y)) < 3f))
		{
			if (who.modData.ContainsKey("SunberryTeam.SBV/Tarot/ReadingDoneForToday"))
			{
				Game1.drawObjectDialogue("You've already had a reading done today. Come back another time.");
				return false;
			}

			// if you have seen the necessary event
			if (who.eventsSeen.Contains(20031411))
			{
				currentLoc.createQuestionDialogue("Would you like to have a tarot reading done?",
					currentLoc.createYesNoResponses(), "tarotReading");
				return false;
			}

			// otherwise generic rejection dialogue
			Game1.drawObjectDialogue("Diala is busy.");
			return false;
		}

		// if diala is not on the map or near the tile location
		Game1.drawObjectDialogue("Come back when Diala is here.");
		return false;
	}

	/// <summary>
	/// Patches <c>GameLocation.answerDialogueAction</c> to check for tarotReading_Yes questionAndAnswer string and handle it accordingly.
	/// </summary>
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
	[HarmonyPrefix]
	public static bool answerDialogueAction_Prefix(string questionAndAnswer)
	{
		if (questionAndAnswer != "tarotReading_Yes")
			return true;

		Game1.player.modData["SunberryTeam.SBV/Tarot/ReadingDoneForToday"] = "true";

		Game1.activeClickableMenu = null;
		GameLocation currentLoc = Game1.currentLocation;

		string eventString = Game1.content.Load<Dictionary<string, string>>("SunberryTeam.SBV/Tarot/Event")["Event"];

		currentLoc.startEvent(new Event(eventString));
		return false;
	}

	/// <summary>
	/// Patches <c>Event.command_cutscene</c> to check for DialaTarot cutscene and handle it accordingly.
	/// </summary>
	[HarmonyPatch(typeof(Event), nameof(Event.command_cutscene))]
	[HarmonyPrefix]
	public static bool command_cutscene_Prefix(Event __instance, string[] split)
	{
		// if custom event script is active, skip prefix and run original code
		if (__instance.currentCustomEventScript != null)
		{
			return true;
		}
		if (split[1] == "DialaTarot")
		{
			__instance.currentCustomEventScript = new EventScriptDialaTarot();
		}

		return true;
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression