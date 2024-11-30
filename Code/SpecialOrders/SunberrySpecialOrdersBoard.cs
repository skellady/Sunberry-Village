using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
	}

	private static void UpdateSpecialOrdersOnWeekStart(object sender, DayStartedEventArgs e)
	{
		if (Game1.IsMasterGame && (Game1.dayOfMonth == 1 || Game1.dayOfMonth == 8 || Game1.dayOfMonth == 15 || Game1.dayOfMonth == 22))
		{
			SpecialOrder.UpdateAvailableSpecialOrders("SunberryBoard", forceRefresh: true);
		}
	}

	private static void RegisterTileAction(object sender, GameLaunchedEventArgs e)
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
