using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using SunberryVillage.TarotEvent;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;

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
internal class TarotEventPatches
{

	/// <summary>
	/// Patches <c>GameLocation.performAction</c> to check for the "DialaTarot" action and handle it accordingly.
	/// </summary>
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction))]
	[HarmonyPrefix]
	public static bool performAction_Prefix(string action, Farmer who, Location tileLocation)
	{
		try
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
				if (who.eventsSeen.Contains(TarotHandler.TarotRequiredEventId))
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
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(performAction_Prefix)}\" has encountered an error while handling \"{action}\" at ({tileLocation.X}, {tileLocation.Y}) on {Game1.currentLocation}: \n{e}");
			return true;
		}
	}

	/// <summary>
	/// Patches <c>GameLocation.answerDialogueAction</c> to check for tarotReading_Yes questionAndAnswer string and handle it accordingly.
	/// </summary>
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
	[HarmonyPrefix]
	public static bool answerDialogueAction_Prefix(string questionAndAnswer)
	{
		try
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
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(TarotEventPatches)}::{nameof(answerDialogueAction_Prefix)}\" has encountered an error while handling \"{questionAndAnswer}\": \n{e}");
			return true;
		}
	}

	/// <summary>
	/// Patches <c>Event.command_cutscene</c> to check for DialaTarot cutscene and handle it accordingly.
	/// </summary>
	/// <param name="__instance">The currently running Event.</param>
	/// <param name="split">The command that was just parsed. split[0] is always the name of the command, and any additional values are parameters passed along with it.</param>
	/// <returns><c>True</c> in order to run the original code following the execution of this patch.</returns>
	[HarmonyPatch(typeof(Event), nameof(Event.command_cutscene))]
	[HarmonyPrefix]
	public static bool command_cutscene_Prefix(Event __instance, string[] split)
	{
		try
		{
			if (__instance.currentCustomEventScript != null)
				return true;

			if (split.Length <= 1)
				return true;

			if (split[1] == "DialaTarot")
				__instance.currentCustomEventScript = new EventScriptDialaTarot();

			return true;
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(TarotEventPatches)}::{nameof(command_cutscene_Prefix)}\" has encountered an error during event \"{__instance.id}\" at command {string.Join(" ", split)}: \n{e}");
			return true;
		}
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression