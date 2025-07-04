using HarmonyLib;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using SunberryVillage.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;
using StardewValley.Network;

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

	//             if (!Game1.IsBuildingConstructed("Gold Clock") || Game1.netWorldState.Value.goldenClocksTurnedOff.Value)
	// Patches Sunberry farm to not spawn weeds or stones if golden clock purchased and turned on
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.spawnWeedsAndStones))]
	[HarmonyPrefix]
    public static bool GameLocation_spawnWeedsAndStones_Prefix(GameLocation __instance)
    {
		return !__instance.Name.Equals("Custom_SBV_SunberryFarm") || !Game1.IsBuildingConstructed("Gold Clock") || Game1.netWorldState.Value.goldenClocksTurnedOff.Value;
    }

    // This pair of patches (clunkily) avoids the Sunberry minecart warp to the busstop from triggering the bus arrival cutscene by pretending an event is up for the duration of the resetLocalState method if the player is coming from Sunberry
    [HarmonyPatch(typeof(BusStop), "resetLocalState")]
	[HarmonyPrefix]
	public static void BusStop_resetLocalState_Prefix(bool __state)
	{
		if (!Game1.player.previousLocationName.Contains("Custom_SBV"))
			return;

		__state = Game1.eventUp;
		Game1.eventUp = true;
	}

	[HarmonyPatch(typeof(BusStop), "resetLocalState")]
	[HarmonyPostfix]
	public static void BusStop_resetLocalState_Postfix(bool __state)
	{
		if (!Game1.player.previousLocationName.Contains("Custom_SBV"))
			return;

		Game1.eventUp = __state;
	}

	// Another clunky hack - remove the trashBearDone worldstate for the duration of this method if these festivals are active to avoid Sunberry areas being patched over
    [HarmonyPatch(typeof(Town), nameof(Town.MakeMapModifications))]
    [HarmonyPrefix]
    public static void Town_MakeMapModifications_Prefix(Town __instance, bool __state)
    {
        if (!__instance.mapPath.Value.Contains("Town-Fair") && !__instance.mapPath.Value.Contains("Town-EggFestival") && !__instance.mapPath.Value.Contains("Town-Christmas"))
            return;

		__state = NetWorldState.checkAnywhereForWorldStateID("trashBearDone");

        Game1.netWorldState.Value.removeWorldStateID("trashBearDone");
        Game1.worldStateIDs.Remove("trashBearDone");
    }

    [HarmonyPatch(typeof(Town), nameof(Town.MakeMapModifications))]
    [HarmonyPostfix]
    public static void Town_MakeMapModifications_Postfix(Town __instance, bool __state)
    {
        if (__state)
		{
            NetWorldState.addWorldStateIDEverywhere("trashBearDone");
        }
    }

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

			if (questionAndAnswer.StartsWith("SunberryTeam.SBVSMAPI_FarmPurchaseQuestion"))
				return HandleFarmPurchaseAction(questionAndAnswer);

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

	/// <summary>
	/// Patches <c>GameLocation.checkAction</c> to make Sunberry mines coal cart functional.
	/// </summary>
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

	/// <summary>
	/// Patches <c>GameLocation.lockedDoorWarp</c> to make Sunberry doors openable with the town key.
	/// </summary>
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.lockedDoorWarp))]
	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> lockedDoorWarp_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
	{
		List<CodeInstruction> origInstructions = [..instructions]; // store unaltered instructions in case anything goes wrong
		List<CodeInstruction> newInstructions = [..origInstructions];
		
		try
		{
			MethodInfo m_getLocContext = typeof(GameLocation).GetMethod(nameof(GameLocation.GetLocationContextId));
			MethodInfo m_stringEquals = typeof(string).GetMethod(nameof(string.Equals), [typeof(string), typeof(string)
			]);
			MethodInfo m_inValleyContext = typeof(GameLocation).GetMethod(nameof(GameLocation.InValleyContext));
			
			List<CodeInstruction> sequenceToFind =
			[
				new(OpCodes.Brfalse_S, new Label()),
				new(OpCodes.Ldloc_0),
				new(OpCodes.Brfalse_S, new Label()),
				new(OpCodes.Ldarg_0),
				new(OpCodes.Call, m_inValleyContext),
				new(OpCodes.Brtrue_S, new Label())
			];

			int index = HarmonyUtils.FindFirstSequenceMatch(newInstructions, sequenceToFind);

			if (index == -1)
			{
				Log.Trace("Failed to match sequence. Aborting transpiler.");
				return origInstructions;
			}
			
			Label jumpLabel = (Label)newInstructions[index].operand;
			int insertIndex = index + sequenceToFind.Count;

			List<CodeInstruction> sequenceToInsert =
			[
				new(OpCodes.Ldarg_0),
				new(OpCodes.Call, m_getLocContext),
				new(OpCodes.Ldstr, "Custom_SBV_SunberryVillage"),
				new(OpCodes.Call, m_stringEquals),
				new(OpCodes.Brtrue_S, jumpLabel)
			];

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

	/// <summary>
	/// Patches <c>GameLocation.getFootstepSoundReplacement</c> to make Sunberry mines grass not make snowy sounds/footsteps in winter.
	/// </summary>
	[HarmonyPatch(typeof(GameLocation), nameof(GameLocation.getFootstepSoundReplacement))]
	[HarmonyPostfix]
	public static void getFootstepSoundReplacement_Postfix(GameLocation __instance, string footstep, ref string __result)
	{
		if (__instance?.GetLocationContextId() == "Custom_SBV_Mines" && footstep == "snowyStep")
			__result = "grassyStep";
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

	private static bool HandleFarmPurchaseAction(string questionAndAnswer)
	{
		switch (questionAndAnswer.Replace("SunberryTeam.SBVSMAPI_FarmPurchaseQuestion_", ""))
		{
			case "yes":
			{
				// in case player somehow loses money between receiving question and answering (looking at you, multiplayer)
				if (Game1.player.Money < 150_000)
				{
					Game1.drawObjectDialogue(Utils.GetTranslationWithPlaceholder("FarmPurchase_NotEnoughMoney").Replace("{0}", TokenParser.ParseText("[NumberWithSeparators 150000]")));
					break;
				}
				Game1.player.Money -= 150_000;
				Game1.PlayEvent("skellady.SBVCP_PurchasedFarm", false, false);
				break;
			}

			case "no":
			{
				break;
			}

			case "see_event":
			{
				Game1.PlayEvent("skellady.SBVCP_20031479", false, false);
				break;
			}
		}

		return true;
	}

}

#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0079 // Remove unnecessary suppression