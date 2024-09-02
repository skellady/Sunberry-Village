using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.SpecialOrders;
using xTile.Tiles;

namespace SunberryVillage.SpecialOrders;

internal class SunberrySpecialOrdersBoard : SpecialOrdersBoard
{
	internal static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.GameLaunched += RegisterTileAction;
		Globals.EventHelper.GameLoop.DayStarted += UpdateSpecialOrdersOnWeekStart;
		Globals.EventHelper.Player.Warped += UpdateSpecialOrderBoardIfBuilt;
	}

	private static void UpdateSpecialOrderBoardIfBuilt(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
	{
		if (!(Game1.currentLocation.Name.Equals("Custom_SBV_SunberryVillage") || Game1.currentLocation.Name.Equals("Custom_SBV_TwilightFestival")) || !Game1.player.eventsSeen.Contains("skellady.SBVCP_20031434") && Game1.currentLocation.currentEvent?.id != "skellady.SBVCP_20031434")
		{
			return;
		}

		GameLocation loc = Game1.currentLocation;

		int tilesheet_index = 2;
		TileSheet town_tilesheet = loc.map.GetTileSheet("spring_town");
		if (town_tilesheet != null)
		{
			tilesheet_index = loc.map.TileSheets.IndexOf(town_tilesheet);
		}

		loc.setMapTileIndex(68, 13, 2045, "Buildings", tilesheet_index);
		loc.setMapTileIndex(69, 13, 2046, "Buildings", tilesheet_index);
		loc.setMapTileIndex(70, 13, 2047, "Buildings", tilesheet_index);
		loc.setTileProperty(68, 13, "Buildings", "Action", "SunberryTeam.SBVSMAPI_SpecialOrdersBoard");
		loc.setTileProperty(69, 13, "Buildings", "Action", "SunberryTeam.SBVSMAPI_SpecialOrdersBoard");
		loc.setTileProperty(70, 13, "Buildings", "Action", "SunberryTeam.SBVSMAPI_SpecialOrdersBoard");
		loc.setMapTileIndex(68, 12, 2013, "Front", tilesheet_index);
		loc.setMapTileIndex(69, 12, 2014, "Front", tilesheet_index);
		loc.setMapTileIndex(70, 12, 2015, "Front", tilesheet_index);
	}

	private static void UpdateSpecialOrdersOnWeekStart(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
	{
		if (Game1.IsMasterGame && (Game1.dayOfMonth == 1 || Game1.dayOfMonth == 8 || Game1.dayOfMonth == 15 || Game1.dayOfMonth == 22))
		{
			SpecialOrder.UpdateAvailableSpecialOrders("SunberryBoard", forceRefresh: true);
		}
	}

	private static void RegisterTileAction(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
	{
		GameLocation.RegisterTileAction("SunberryTeam.SBVSMAPI_SpecialOrdersBoard", OpenSpecialOrdersMenu);
	}

	private static bool OpenSpecialOrdersMenu(GameLocation loc, string[] args, Farmer who, Point tile)
	{
		Game1.activeClickableMenu = new SunberrySpecialOrdersBoard();
		return true;
	}

	internal SunberrySpecialOrdersBoard() : base("SunberryBoard") { }
}
