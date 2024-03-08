using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using SunberryVillage.Utilities;
using xTile;
using xTile.Layers;

namespace SunberryVillage.Shops;
internal class MarketDailySpecialManager
{
	internal static ParsedItemData DailySpecialItemData;
	internal static Vector2 DailySpecialPosition = Vector2.Zero;
	internal static TemporaryAnimatedSprite DailySpecialSprite;

	internal const string AlreadyPurchasedMailFlag = "SunberryTeam.SBVSMAPI_AlreadyPurchasedMarketDailySpecial";
	internal const string MarketSpecialTileProperty = "SunberryTeam.SBVSMAPI_MarketDailySpecial";

	internal static void AddEventHooks()
	{
		Globals.EventHelper.GameLoop.DayStarted += GetDailySpecial;
		Globals.EventHelper.Player.Warped += OnWarped;

		Globals.EventHelper.GameLoop.DayEnding += (_, _) => Game1.player.modData.Remove(AlreadyPurchasedMailFlag);
	}

	private static void GetDailySpecial(object sender, DayStartedEventArgs e)
	{
		DailySpecialItemData = ItemRegistry.GetDataOrErrorItem("(F)2396");
	}

	private static void OnWarped(object sender, WarpedEventArgs e)
	{
		if (!e.IsLocalPlayer || e.NewLocation.Name != "Custom_SBV_AriMarket" || Game1.MasterPlayer.modData.ContainsKey(AlreadyPurchasedMailFlag))
			return;

		int heightOffset = DailySpecialItemData.GetSourceRect().Height / 16;

		//Layer layer = e.NewLocation.Map.GetLayer("Buildings");
		//for (int x = 0; x < layer.LayerWidth; x++)
		//{
		//	for (int y = 0; y < layer.LayerHeight; y++)
		//	{
		//		if (e.NewLocation.GetTilePropertySplitBySpaces("Action", "Buildings", x, y).Contains(MarketSpecialTileProperty))
		//		{
		//			DailySpecialPosition = new Vector2(x, y) - new Vector2(0, heightOffset);
		//		}
		//	}
		//}

		DailySpecialPosition = new Vector2(8, 20) - new Vector2(0, heightOffset);

		DailySpecialSprite = new TemporaryAnimatedSprite(null, Rectangle.Empty, DailySpecialPosition * 64f, flipped: false, 0f, Color.White)
		{
			layerDepth = 0.135f
		};
		DailySpecialSprite.CopyAppearanceFromItemId(DailySpecialItemData.QualifiedItemId);

		e.NewLocation.TemporarySprites.Add(DailySpecialSprite);
	}
	
	internal static Item GetOfferItem()
	{
		return ItemRegistry.Create(DailySpecialItemData.QualifiedItemId);
	}

	internal static string GetOfferDialogue()
	{
		//return $"want a {DailySpecialItemData.DisplayName}? only {GetOfferPrice()} bucks.";

	}

	internal static int GetOfferPrice()
	{
		return GetOfferItem().sellToStorePrice();
	}

	internal static void RemoveDailySpecial()
	{
		Game1.player.modData[AlreadyPurchasedMailFlag] = "true";
		Game1.currentLocation.TemporarySprites.Remove(DailySpecialSprite);
	}

}
