﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewValley;
using SunberryVillage.Tarot;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SunberryVillage.Maps;

internal class ActionManager
{
	internal static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.GameLaunched += RegisterTileActions;
	}

	private static void InitializeToggles(object sender, WarpedEventArgs e)
	{
		throw new NotImplementedException();
	}

	private static void RegisterTileActions(object sender, GameLaunchedEventArgs e)
	{
		GameLocation.RegisterTileAction("SunberryTeam.SBVSMAPI_Book", HandleBookAction);
		GameLocation.RegisterTileAction("SunberryTeam.SBVSMAPI_ChooseDestination", HandleChooseDestinationAction);
		GameLocation.RegisterTileAction("SunberryTeam.SBVSMAPI_DialaTarot", HandleTarotAction);
		GameLocation.RegisterTileAction("SunberryTeam.SBVSMAPI_ToggleTiles", TileToggleManager.HandleToggleTilesAction);
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
			if (player.modData.ContainsKey("SunberryTeam.SBV/Tarot/ReadingDoneForToday"))
			{
				Game1.drawObjectDialogue(Utils.GetTranslationWithPlaceholder("TarotAlreadyReadToday"));
				return false;
			}

			// if you have seen the necessary event
			if (player.eventsSeen.Contains(TarotHandler.TarotRequiredEventId))
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
}
