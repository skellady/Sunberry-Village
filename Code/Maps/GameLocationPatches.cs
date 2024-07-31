using HarmonyLib;
using StardewValley;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using StardewValley.Extensions;
using StardewValley.Menus;
using SunberryVillage.Shops;
using xTile.Dimensions;

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

	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
	[HarmonyPrefix]
	public static bool checkAction_Prefix(GameLocation __instance, Location tileLocation, Farmer who)
	{
		try
		{
			if (!__instance.Name.Contains("Custom_SBV_Mines") || !who.IsLocalPlayer)
				return true;

			switch (__instance.getTileIndexAt(tileLocation, "Buildings"))
			{
				case 194:
					__instance.playSound("openBox");
					__instance.playSound("Ship");
					__instance.map.RequireLayer("Buildings").Tiles[tileLocation].TileIndex++;
					__instance.map.RequireLayer("Front").Tiles[tileLocation.X, tileLocation.Y - 1].TileIndex++;
					Game1.createRadialDebris(__instance, 382, tileLocation.X, tileLocation.Y, 6, resource: false, -1, item: true);
					who.mailReceived.Add($"Coal_{__instance.Name}_{tileLocation.X}_{tileLocation.Y}_Collected");
					__instance.map.RequireLayer("Buildings").Tiles[tileLocation].Properties.Remove("Action");
					return false;

				default:
					return true;
			}
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch \"{nameof(GameLocationPatches)}::{nameof(checkAction_Prefix)}\" has encountered an error while checking for action at \"{__instance.Name}\" ({tileLocation}): \n{e}");
			return true;
		}
	}

	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.lockedDoorWarp))]
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> lockedDoorWarp_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		List<CodeInstruction> origInstructions = new(instructions); // store unaltered instructions in case anything goes wrong
		List<CodeInstruction> newInstructions = new(origInstructions);
		
		try
		{
			MethodInfo m_getLocContext = typeof(GameLocation).GetMethod(nameof(GameLocation.GetLocationContextId));
			MethodInfo m_stringEquals = typeof(string).GetMethod(nameof(string.Equals), new[] { typeof(string), typeof(string)});
			MethodInfo m_inValleyContext = typeof(GameLocation).GetMethod(nameof(GameLocation.InValleyContext));
			
			List<CodeInstruction> sequenceToFind = new()
			{
				new CodeInstruction(OpCodes.Brfalse_S, new Label()),
				new CodeInstruction(OpCodes.Ldloc_0),
				new CodeInstruction(OpCodes.Brfalse_S, new Label()),
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Call, m_inValleyContext),
				new CodeInstruction(OpCodes.Brtrue_S, new Label())
			};

			int index = HarmonyUtils.FindFirstSequenceMatch(newInstructions, sequenceToFind);

			if (index == -1)
			{
				Log.Trace("Failed to match sequence. Aborting transpiler.");
				return origInstructions;
			}
			
			Label jumpLabel = (Label)newInstructions[index].operand;
			int insertIndex = index + sequenceToFind.Count;

			List<CodeInstruction> sequenceToInsert = new()
			{
				new CodeInstruction(OpCodes.Ldarg_0),
				new CodeInstruction(OpCodes.Call, m_getLocContext),
				new CodeInstruction(OpCodes.Ldstr, "Custom_SBV_SunberryVillage"),
				new CodeInstruction(OpCodes.Call, m_stringEquals),
				new CodeInstruction(OpCodes.Brtrue_S, jumpLabel),
			};

			newInstructions.InsertRange(insertIndex, sequenceToInsert);
			return newInstructions;
		}
		catch (Exception e)
		{
			Log.Error($"Harmony patch <{nameof(GameLocationPatches)}::{nameof(lockedDoorWarp_Transpiler)}> has encountered an error while attempting to transpile <{nameof(GameLocation)}::{nameof(GameLocation.lockedDoorWarp)}>: \n{e}");
			Log.Error("Faulty IL:\n\t" + string.Join("\n\t", newInstructions.Select((instruction, i) => $"{i}\t{instruction}")));
			return origInstructions;
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

		int whichVariant = Utils.Random.Choose(1, 2);

		// if ChooseDestination answer dialogue and cancel selected, no further action needed
		if (questionAndAnswer.Equals("SunberryTeam.SBVSMAPI_MarketDailySpecialResponses_No"))
		{
			// handle rejection logic - ari says something disappointed ?
			Dialogue rejectPurchaseDialogue = new(ari, "",
				Utils.GetTranslationWithPlaceholder($"MarketDailySpecialRejectPurchase{whichVariant}"));
			ari.setNewDialogue(rejectPurchaseDialogue, true);
			Game1.drawDialogue(ari);
			return false;
		}

		if (Game1.player.Money < MarketDailySpecialManager.GetOfferPrice())
		{
			Dialogue notEnoughDialogue = new(ari, "",
				Utils.GetTranslationWithPlaceholder($"MarketDailySpecialNotEnoughMoney{whichVariant}").Replace("{0}", Game1.player.Name));
			ari.setNewDialogue(notEnoughDialogue, true);
			Game1.drawDialogue(ari);
			return false;
		}

		Game1.player.Money -= MarketDailySpecialManager.GetOfferPrice();
		Game1.player.addItemByMenuIfNecessary(MarketDailySpecialManager.GetOfferItem());

		MarketDailySpecialManager.RemoveDailySpecial();

		Dialogue purchaseDialogue = new(ari, "",
			Utils.GetTranslationWithPlaceholder($"MarketDailySpecialPurchased{whichVariant}"));
		ari.setNewDialogue(purchaseDialogue, true);
		Game1.drawDialogue(ari);
		return false;
	}
}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression