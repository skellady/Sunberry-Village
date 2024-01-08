using HarmonyLib;
using StardewValley;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable RedundantAssignment
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Maps;

// Boilerplate suppression for Harmony patch files
#pragma warning disable IDE0079 // Remove unnecessary suppression
// Method names reflect the original methods that they are patching, hence the naming rule violations
#pragma warning disable IDE1006 // Naming Styles
// Certain parameters have special meanings to Harmony
#pragma warning disable IDE0060 // Remove unused parameter

[HarmonyPatch]
internal class GameLocationPatches
{

	/*
	 *  Patches
	 */

	/// <summary>
	/// Patches <c>GameLocation.answerDialogueAction</c> to check for response to ChooseDestination question and handle it accordingly.
	/// </summary>
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
	[HarmonyPrefix]
	public static bool answerDialogueAction_Prefix(string questionAndAnswer)
	{
		try
		{
			if (questionAndAnswer.StartsWith("SunberryTeam.SBVSMAPI_ChooseDestination"))
				return HandleChooseDestinationDialogueAction(questionAndAnswer);

			if (questionAndAnswer.Equals("tarotReading_Yes"))
				return HandleTarotDialogueAction();

			return true;
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(GameLocationPatches)}::{nameof(answerDialogueAction_Prefix)}\" has encountered an error while handling \"{questionAndAnswer}\": \n{e}");
			return true;
		}
	}

	/*
	 *  Helpers
	 */

	private static bool HandleChooseDestinationDialogueAction(string questionAndAnswer)
	{
		// if ChooseDestination answer dialogue and cancel selected, no further action needed
		if (questionAndAnswer.Equals("SunberryTeam.SBVSMAPI_ChooseDestination_Cancel"))
			return false;

		// otherwise - perform warp
		string[] warpParams = questionAndAnswer.Remove(0, "SunberryTeam.SBVSMAPI_ChooseDestination_".Length).Split('¦');

		//Log.Trace("Warp params: " + string.Join(" ", warpParams));
		Game1.warpFarmer(warpParams[2], int.Parse(warpParams[0]), int.Parse(warpParams[1]), false);

		//Log.Trace($"{questionAndAnswer} chosen.");

		return false;
	}

	private static bool HandleTarotDialogueAction()
	{
		Game1.player.modData["SunberryTeam.SBV/Tarot/ReadingDoneForToday"] = "true";

		Game1.activeClickableMenu?.emergencyShutDown();
		Game1.activeClickableMenu?.exitThisMenuNoSound();

		string eventString = Game1.content.Load<Dictionary<string, string>>("SunberryTeam.SBV/Tarot/Event")["Event"];

		Game1.currentLocation.startEvent(new Event(eventString));
		return false;
	}

	/*
	 *  Deprecated
	 */

	///// <summary>
	///// Patches <c>GameLocation.performAction</c> to check for the "SBVBook" tile property
	///// </summary>
	//[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction))]
	//[HarmonyPrefix]
	//public static bool performAction_Prefix(string action, Farmer who, Location tileLocation)
	//{
	//	try
	//	{
	//		if (action.Trim().StartsWith("SBVBook") && who.IsLocalPlayer)
	//			return HandleSBVBookAction(action);

	//		if (action.Trim().StartsWith("ChooseDestination") && who.IsLocalPlayer)
	//			return HandleChooseDestinationAction(action);

	//		if (action.Equals("DialaTarot") && who.IsLocalPlayer)
	//			return HandleTarotAction(action, who, tileLocation);

	//		return true;
	//	}
	//	catch (Exception e)
	//	{
	//		Log.Error($"Harmony patch \"{nameof(GameLocationPatches)}::{nameof(performAction_Prefix)}\" has encountered an error while handling action \"{action}\" at ({tileLocation.X}, {tileLocation.Y}) on map \"{Game1.currentLocation.Name}\": \n{e}");
	//		return true;
	//	}
	//}

	//private static bool HandleSBVBookAction(string action)
	//{
	//	// split the string on spaces and then strip out the first entry because it just contains the action name
	//	string[] actionParams = action.Split(' ')[1..];

	//	// number of parameters should be greater than 0
	//	if (actionParams.Length < 2)
	//		throw new Exception("Incorrect number of arguments provided to SBVBook action." +
	//			"\nProper syntax for SBVBook is as follows: SBVBook MandatoryStringPathAndKey [OptionalStringPathAndKey] ... [OptionalStringPathAndKey] (note the lack of quotes around each parameter)" +
	//			"\nString path and key formatted like so: Path\\To\\File:StringKey" +
	//			"\nExample: \"SBVBook Strings\\StringsFromCSFiles:summer\"");

	//	// separate individual pages with a bunch of newlines - ensures pages don't bleed over onto the previous page
	//	string book = string.Join("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^", actionParams.Select(s => Game1.content.LoadString(s)));

	//	// display letter menu with contents of 
	//	Game1.drawLetterMessage(book);

	//	return false;
	//}

	//private static bool HandleChooseDestinationAction(string action)
	//{
	//	// split the string on spaces and then strip out the first entry because it just contains the action name
	//	string[] actionParams = action.Split(' ')[1..];

	//	// number of parameters should be a multiple of 4 greater than 0
	//	if (actionParams.Length < 1 || actionParams.Length % 4 != 0)
	//		throw new Exception("Incorrect number of arguments provided to ChooseDestination action." +
	//			"\nProper syntax for ChooseDestination is as follows: ChooseDestination \"[Option1StringKey]\" [x1] [y1] [LocationName1] \"[Option2StringKey]\" [x2] [y2] [LocationName2] ... \"[OptionNStringKey]\" [xN] [yN] [LocationNameN]");

	//	List<Response> responses = new();

	//	for (int index = 0; index < actionParams.Length; index += 4)
	//	{
	//		responses.Add(new Response(string.Join('¦', actionParams[(index + 1)..(index + 4)]), Game1.content.LoadString(actionParams[index].Replace("\"", ""))));
	//	}
	//	responses.Add(new Response("Cancel", Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10993")).SetHotKey(Keys.Escape));

	//	// logging
	//	//Log.Trace($"ChooseDestination dialogue created.\nOptions:\n\t{string.Join("\n\t", responses.Select(r => r.responseKey))}");

	//	Game1.currentLocation.createQuestionDialogue(" ", responses.ToArray(), "ChooseDestination");
	//	return false;
	//}

	//private static bool HandleTarotAction(string action, Farmer who, Location tileLocation)
	//{
	//	GameLocation currentLoc = Game1.currentLocation;

	//	if (currentLoc.characters.Any(npc => npc.Name == "DialaSBV" && Vector2.Distance(npc.Tile, new Vector2(tileLocation.X, tileLocation.Y)) < 3f))
	//	{
	//		if (who.modData.ContainsKey("SunberryTeam.SBV/Tarot/ReadingDoneForToday"))
	//		{
	//			Game1.drawObjectDialogue("You've already had a reading done today. Come back another time.");
	//			return false;
	//		}

	//		// if you have seen the necessary event
	//		if (who.eventsSeen.Contains(TarotHandler.TarotRequiredEventId))
	//		{
	//			currentLoc.createQuestionDialogue("Would you like to have a tarot reading done?", currentLoc.createYesNoResponses(), "tarotReading");
	//			return false;
	//		}

	//		// otherwise generic rejection dialogue
	//		Game1.drawObjectDialogue("Diala is busy.");
	//		return false;
	//	}

	//	// if diala is not on the map or near the tile location
	//	Game1.drawObjectDialogue("Come back when Diala is here.");
	//	return false;
	//}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression