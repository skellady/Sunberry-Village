﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using SunberryVillage.Events.Tarot;
using SunberryVillage.Shops;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Layers;

namespace SunberryVillage.Maps;

internal class MapManager
{
	internal const string TraveledSunberryRoadToday = "SunberryTeam.SBVSMAPI_TraveledSunberryRoadToday";

	internal static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.GameLaunched += RegisterTileActions;
		Globals.EventHelper.Player.Warped += CheckForMinesChanges;
		Globals.EventHelper.Player.Warped += CheckTravelingSunberryRoad;
		Globals.EventHelper.GameLoop.DayEnding += ModifyTravelingSunberryRoadStat;
	}

	private static void CheckTravelingSunberryRoad(object sender, WarpedEventArgs e)
	{
		if (!e.OldLocation.Name.Equals("Town") || !e.NewLocation.Name.Equals("Custom_SBV_SunberryRoad") || !e.IsLocalPlayer || e.Player.modData.ContainsKey(TraveledSunberryRoadToday))
			return;

		e.Player.modData[TraveledSunberryRoadToday] = "true";
	}
	
	private static void ModifyTravelingSunberryRoadStat(object sender, DayEndingEventArgs e)
	{
		if (Game1.player.modData[TraveledSunberryRoadToday] != "true")
			return;

		Game1.player.modData.Remove(TraveledSunberryRoadToday);
		Game1.player.stats.Increment("SunberryTeam.SBVSMAPI_DaysTraveledSunberryRoad");
	}

	private static void CheckForMinesChanges(object sender, WarpedEventArgs e)
	{
		if (!e.NewLocation.Name.Contains("Custom_SBV_Mines") || !e.IsLocalPlayer)
			return;

		Farmer player = e.Player;
		GameLocation mine = e.NewLocation;
		Layer buildingsLayer = mine.Map.GetLayer("Buildings");
		Layer frontLayer = mine.Map.GetLayer("Front");

		if (buildingsLayer is null || frontLayer is null)
			return;

		for (int x = 0; x < buildingsLayer.LayerWidth; x++)
		{
			for (int y = 0; y < buildingsLayer.LayerHeight; y++)
			{
				if (buildingsLayer.Tiles[x, y]?.TileIndex != 194)
					continue;

				if (player.hasOrWillReceiveMail($"Coal_{mine.Name}_{x}_{y}_Collected"))
				{
					buildingsLayer.Tiles[x, y].TileIndex++;
					frontLayer.Tiles[x, y - 1].TileIndex++;
					buildingsLayer.Tiles[x, y].Properties.Remove("Action");
				}
				else
				{
					buildingsLayer.Tiles[x, y].Properties["Action"] = "None";
				}
			}
		}
	}

	private static void RegisterTileActions(object sender, GameLaunchedEventArgs e)
	{
		GameLocation.RegisterTileAction("SunberryTeam.SBVSMAPI_Book", HandleBookAction);
		GameLocation.RegisterTileAction("SunberryTeam.SBVSMAPI_ChooseDestination", HandleChooseDestinationAction);
		GameLocation.RegisterTileAction("SunberryTeam.SBVSMAPI_DialaTarot", HandleTarotAction);
		GameLocation.RegisterTileAction("SunberryTeam.SBVSMAPI_MarketDailySpecial", HandleMarketDailySpecialAction);
	}

	private static bool HandleBookAction(GameLocation location, string[] args, Farmer player, Point tile)
	{
		if (args.Length < 2)
			throw new Exception("Incorrect number of arguments provided to SunberryTeam.SBVSMAPI_Book action." +
				"\nProper syntax for SunberryTeam.SBVSMAPI_Book is as follows: SunberryTeam.SBVSMAPI_Book MandatoryStringPathAndKey [OptionalStringPathAndKey] ... [OptionalStringPathAndKey] (note the lack of quotes around each parameter)" +
				"\nString path and key formatted like so: Path\\To\\File:StringKey" +
				"\nExample: \"SunberryTeam.SBVSMAPI_Book Strings\\StringsFromCSFiles:summer\"");

		// separate individual pages with a bunch of newlines - ensures pages don't bleed over onto the previous page
		// remove 0th arg as it is the command name
		string book = string.Join("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^", args[1..].Select(s => Game1.content.LoadString(s)));

		// display letter menu with contents of pages
		Game1.drawLetterMessage(book);
		return true;
	}

	private static bool HandleChooseDestinationAction(GameLocation location, string[] args, Farmer player, Point tile)
	{
		// strip out 0th entry because it just contains the action name
		string[] actionParams = args[1..];

		// number of parameters should be a multiple of 4 greater than 0
		if (actionParams.Length < 1 || actionParams.Length % 4 != 0)
			throw new Exception("Incorrect number of arguments provided to SunberryTeam.SBVSMAPI_ChooseDestination action." +
				"\nProper syntax for SunberryTeam.SBVSMAPI_ChooseDestination is as follows: SunberryTeam.SBVSMAPI_ChooseDestination \"[Option1StringKey]\" [x1] [y1] [LocationName1] \"[Option2StringKey]\" [x2] [y2] [LocationName2] ... \"[OptionNStringKey]\" [xN] [yN] [LocationNameN]");

		List<Response> responses = new();

		for (int index = 0; index < actionParams.Length; index += 4)
		{
			responses.Add(new Response(string.Join('¦', actionParams[(index + 1)..(index + 4)]), Game1.content.LoadString(actionParams[index].Replace("\"", ""))));
		}
		responses.Add(new Response("Cancel", Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10993")).SetHotKey(Keys.Escape));

		// logging
		//Log.Trace($"ChooseDestination dialogue created.\nOptions:\n\t{string.Join("\n\t", responses.Select(r => r.responseKey))}");

		location.createQuestionDialogue(" ", responses.ToArray(), "SunberryTeam.SBVSMAPI_ChooseDestination");
		return true;
	}

	private static bool HandleTarotAction(GameLocation location, string[] args, Farmer player, Point tile)
	{
		if (location.characters.Any(npc => npc.Name == "DialaSBV" && Vector2.Distance(npc.Tile, tile.ToVector2()) < 3f))
		{
			if (player.modData.ContainsKey("SunberryTeam.SBVSMAPI_TarotReadingDoneForToday"))
			{
				Game1.drawObjectDialogue(Utils.GetTranslationWithPlaceholder("TarotAlreadyReadToday"));
				return false;
			}

			// if you have seen the necessary event
			if (player.eventsSeen.Contains(TarotManager.TarotRequiredEventId))
			{
				location.createQuestionDialogue(Utils.GetTranslationWithPlaceholder("TarotPrompt"), location.createYesNoResponses(), "tarotReading");
				return true;
			}

			// otherwise generic rejection dialogue
			Game1.drawObjectDialogue(Utils.GetTranslationWithPlaceholder("TarotDialaBusy"));
			return false;
		}

		// if diala is not on the map or near the tile location
		Game1.drawObjectDialogue(Utils.GetTranslationWithPlaceholder("TarotDialaAway"));
		return false;
	}

	private static bool HandleMarketDailySpecialAction(GameLocation location, string[] args, Farmer player, Point tile)
	{
		NPC ari = Game1.currentLocation.getCharacterFromName("AriSBV");
		if (ari is null)
			return false;

		// if Ari is on the map and close enough to the daily special tile
		if (location.characters.Contains(ari) && Vector2.Distance(ari.Tile, tile.ToVector2()) < 10f)
		{
			if (player.modData.ContainsKey("SunberryTeam.SBVSMAPI_AlreadyPurchasedMarketDailySpecial"))
			{
				Dialogue alreadyPurchasedDialogue = new(ari, null, Utils.GetTranslationWithPlaceholder("MarketDailySpecialAlreadyPurchasedToday").Replace("{0}", Game1.player.Name));
				ari.setNewDialogue(alreadyPurchasedDialogue, true);
				Game1.drawDialogue(ari);

				return false;
			}

			Dialogue offerDialogue = new(ari, null, MarketDailySpecialManager.GetOfferDialogue());

			ari.setNewDialogue(offerDialogue, true);
			Game1.afterDialogues = () => location.createQuestionDialogue(MarketDailySpecialManager.GetPurchaseConfirmationDialogue(),
				location.createYesNoResponses(), "SunberryTeam.SBVSMAPI_MarketDailySpecialResponses");
			Game1.drawDialogue(ari);

			return false;
		}

		// if Ari is not around
		Game1.drawObjectDialogue(Utils.GetTranslationWithPlaceholder("MarketDailySpecialAriNotAround").Replace("{0}", MarketDailySpecialManager.GetOfferItemName()));
		return false;
	}
}
