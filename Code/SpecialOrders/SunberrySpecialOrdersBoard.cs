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

		const string tileSheetId = "spring_town";
		GameLocation loc = Game1.currentLocation;

		loc.setMapTile(68, 13, 2045, "Buildings", tileSheetId);
		loc.setMapTile(69, 13, 2046, "Buildings", tileSheetId);
		loc.setMapTile(70, 13, 2047, "Buildings", tileSheetId);
		loc.setTileProperty(68, 13, "Buildings", "Action", "SunberryTeam.SBVSMAPI_SpecialOrdersBoard");
		loc.setTileProperty(69, 13, "Buildings", "Action", "SunberryTeam.SBVSMAPI_SpecialOrdersBoard");
		loc.setTileProperty(70, 13, "Buildings", "Action", "SunberryTeam.SBVSMAPI_SpecialOrdersBoard");
		loc.setMapTile(68, 12, 2013, "Front", tileSheetId);
		loc.setMapTile(69, 12, 2014, "Front", tileSheetId);
		loc.setMapTile(70, 12, 2015, "Front", tileSheetId);
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
