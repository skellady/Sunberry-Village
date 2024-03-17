using HarmonyLib;
using StardewValley;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using StardewValley.Menus;
using SunberryVillage.Shops;

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
	/// Patches <c>GameLocation.answerDialogueAction</c> to check for response to custom questions and handle them accordingly.
	/// </summary>
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
	[HarmonyPrefix]
	public static bool answerDialogueAction_Prefix(string questionAndAnswer)
	{
		try
		{
			if (questionAndAnswer.StartsWith("SunberryTeam.SBVSMAPI_ChooseDestination"))
				return HandleChooseDestinationDialogueAction(questionAndAnswer);
			
			if (questionAndAnswer.StartsWith("SunberryTeam.SBVSMAPI_MarketDailySpecialResponses"))
				return HandleMarketDailySpecialPurchaseAction(questionAndAnswer);

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

		return false;
	}

	private static bool HandleTarotDialogueAction()
	{
		Game1.player.modData["SunberryTeam.SBVSMAPI_TarotReadingDoneForToday"] = "true";

		Game1.activeClickableMenu?.emergencyShutDown();
		Game1.activeClickableMenu?.exitThisMenuNoSound();

		string eventString = Game1.content.Load<Dictionary<string, string>>("SunberryTeam.SBV/Tarot/Event")["Event"];

		Game1.currentLocation.startEvent(new Event(eventString));
		return false;
	}

	private static bool HandleMarketDailySpecialPurchaseAction(string questionAndAnswer)
	{
		NPC ari = Game1.currentLocation.getCharacterFromName("AriSBV");
		if (ari is null)
			return false;

		int whichVariant = new Random().Next(2) + 1;

		// if ChooseDestination answer dialogue and cancel selected, no further action needed
		if (questionAndAnswer.Equals("SunberryTeam.SBVSMAPI_MarketDailySpecialResponses_No"))
		{
			// handle rejection logic - ari says something disappointed ?
			Dialogue rejectPurchaseDialogue = new (ari, "",
				Utils.GetTranslationWithPlaceholder($"MarketDailySpecialRejectPurchase{whichVariant}"));
			Game1.activeClickableMenu = new DialogueBox(rejectPurchaseDialogue);
			return false;
		}

		if (Game1.player.Money < MarketDailySpecialManager.GetOfferPrice())
		{
			Dialogue notEnoughDialogue = new (ari, "",
				Utils.GetTranslationWithPlaceholder($"MarketDailySpecialNotEnoughMoney{whichVariant}").Replace("{0}", Game1.player.Name));
			Game1.activeClickableMenu = new DialogueBox(notEnoughDialogue);
			return false;
		}

		Game1.player.Money -= MarketDailySpecialManager.GetOfferPrice();
		Game1.player.addItemByMenuIfNecessary(MarketDailySpecialManager.GetOfferItem());

		MarketDailySpecialManager.RemoveDailySpecial();

		Dialogue purchaseDialogue = new (ari, "",
			Utils.GetTranslationWithPlaceholder($"MarketDailySpecialPurchased{whichVariant}"));
		Game1.activeClickableMenu = new DialogueBox(purchaseDialogue);

		return false;
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression