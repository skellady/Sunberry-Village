using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Internal;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Utilities;
using StardewValley.TokenizableStrings;
using xTile.Layers;

namespace SunberryVillage.Shops;
internal class MarketDailySpecialManager
{
	internal static Item MarketDailySpecialItem;
	internal static Vector2 DailySpecialTilePosition = Vector2.Zero;
	internal static TemporaryAnimatedSprite DailySpecialSprite;

	internal const string AlreadyPurchasedMailFlag = "SunberryTeam.SBVSMAPI_AlreadyPurchasedMarketDailySpecial";
	internal const string MarketSpecialAssetPath = "SunberryTeam.SBVSMAPI/MarketDailySpecialData";

	internal static void AddEventHooks()
	{
		Globals.EventHelper.Content.AssetRequested += DailySpecial_AssetRequested;

		Globals.EventHelper.GameLoop.DayStarted += GetDailySpecial;
		Globals.EventHelper.Player.Warped += OnWarped;

		Globals.EventHelper.GameLoop.DayEnding += (_, _) => Game1.player.modData.Remove(AlreadyPurchasedMailFlag);
	}

	private static void DailySpecial_AssetRequested(object sender, AssetRequestedEventArgs e)
	{
		if (e.NameWithoutLocale.IsEquivalentTo(MarketSpecialAssetPath))
		{
			e.LoadFromModFile<Dictionary<string, MarketDailySpecialData>>("Assets/blank.json", AssetLoadPriority.Low);
		}
	}

	private static void GetDailySpecial(object sender, DayStartedEventArgs e)
	{
		// not caching this - it only gets pulled once per day so it's probably fine lmao
		Dictionary<string, MarketDailySpecialData> marketDailySpecialAsset =
			Globals.GameContent.Load<Dictionary<string, MarketDailySpecialData>>(MarketSpecialAssetPath);

		// should never happen lol
		if (marketDailySpecialAsset == null || !marketDailySpecialAsset.Any())
		{
			Log.Warn($"Could not get data from asset \"{MarketSpecialAssetPath}\". Creating generic fallback item \"(O)0\".");
			MarketDailySpecialItem = ItemRegistry.Create("(O)0");
			return;
		}

		// create seeded random to sync daily special across all players
		Random seededRandom = new((int)Game1.MasterPlayer.UniqueMultiplayerID + SDate.Now().DaysSinceStart);

		ItemQueryContext itemQueryContext = new(Game1.currentLocation, Game1.player, null);
		KeyValuePair<string, MarketDailySpecialData> marketDailySpecialDataEntry = marketDailySpecialAsset
			.Where(kvp =>
				string.IsNullOrEmpty(kvp.Value.Condition) ||
				GameStateQuery.CheckConditions(kvp.Value.Condition, Game1.currentLocation, Game1.player)).ToList()
			.GetRandomElementFromList(seededRandom);
		
		Log.Trace($"Selected entry {marketDailySpecialDataEntry.Key} for market daily special.");
		
		if (!marketDailySpecialDataEntry.Value.Items.Any())
		{
			Log.Warn($"Could not get contents of Items field. Creating generic fallback item \"(O)0\".");
			MarketDailySpecialItem = ItemRegistry.Create("(O)0");
			return;
		}

		MarketDailySpecialItem = ItemQueryResolver.TryResolveRandomItem(marketDailySpecialDataEntry.Value.Items.GetRandomElementFromList(seededRandom), itemQueryContext, logError: (query, message) => Log.Warn($"Failed parsing item query '{query}': {message}"));

		Log.Trace($"Selected item [{MarketDailySpecialItem.QualifiedItemId} ({MarketDailySpecialItem.DisplayName}) x {MarketDailySpecialItem.Stack}] for market daily special.");
	}

	private static void OnWarped(object sender, WarpedEventArgs e)
	{
		if (!e.IsLocalPlayer || e.NewLocation.Name != "Custom_SBV_AriMarket" || Game1.MasterPlayer.modData.ContainsKey(AlreadyPurchasedMailFlag))
			return;

		// create TAS and assign appearance from selected daily special item
		DailySpecialSprite = new TemporaryAnimatedSprite(null, Rectangle.Empty, Vector2.Zero, flipped: false, 0f, Color.White)
		{
			layerDepth = 0.135f
		};
		DailySpecialSprite.CopyAppearanceFromItemId(MarketDailySpecialItem.QualifiedItemId);

		int heightOffset = DailySpecialSprite.sourceRect.Height / 16;

		// iterate over map to find property in order to determine where to place sprite for special item
		bool found = false;
		Layer layer = e.NewLocation.Map.GetLayer("Buildings");
		for (int x = 0; x < layer.LayerWidth; x++)
		{
			if (found)
				break;

			for (int y = 0; y < layer.LayerHeight; y++)
			{
				if (found)
					break;

				string[] action = e.NewLocation.GetTilePropertySplitBySpaces("Action", "Buildings", x, y);
				// ReSharper disable once InvertIf
				if (ArgUtility.Get(action, 0, "") == "SunberryTeam.SBVSMAPI_MarketDailySpecial")
				{
					DailySpecialTilePosition = new Vector2(x, y) - new Vector2(0, heightOffset);
					found = true;
				}
			}
		}
		
		// convert tile pos to pixel pos and shift up slightly on counter
		DailySpecialSprite.Position = DailySpecialTilePosition * 64f + new Vector2(0f, -16f);

		e.NewLocation.TemporarySprites.Add(DailySpecialSprite);
	}
	
	internal static Item GetOfferItem()
	{
		return MarketDailySpecialItem;
	}

	internal static string GetOfferItemName()
	{
		// override with custom name logic somehow?
		if (MarketDailySpecialItem.modData.TryGetValue("SunberryTeam.SBVSMAPI_MarketSpecialDisplayName", out string overrideName) && !string.IsNullOrEmpty(overrideName))
			return TokenParser.ParseText(overrideName, null, null, Game1.player);

		return MarketDailySpecialItem.Stack == 1 ? MarketDailySpecialItem.DisplayName : $"{MarketDailySpecialItem.DisplayName} x {MarketDailySpecialItem.Stack}";
	}

	internal static string GetOfferDialogue()
	{
		int whichVariant = new Random().Next(2) + 1;
		
		return Utils.GetTranslationWithPlaceholder($"MarketDailySpecialOffer{whichVariant}")
			.Replace("{0}", GetOfferItemName())
			.Replace("{1}", GetOfferPrice().ToString());
	}

	internal static string GetPurchaseConfirmationDialogue()
	{
		return Utils.GetTranslationWithPlaceholder("MarketDailySpecialConfirmPurchase")
			.Replace("{0}", GetOfferItemName())
			.Replace("{1}", GetOfferPrice().ToString());
	}

	internal static int GetOfferPrice()
	{
		return Utility.getSellToStorePriceOfItem(MarketDailySpecialItem);
	}

	internal static void RemoveDailySpecial()
	{
		Game1.player.modData[AlreadyPurchasedMailFlag] = "true";
		Game1.currentLocation.TemporarySprites.Remove(DailySpecialSprite);
	}
}

internal class MarketDailySpecialData
{
	internal string Id;
	internal string Condition;
	internal List<GenericSpawnItemData> Items;

	public MarketDailySpecialData(string id, string condition = "", List<GenericSpawnItemData> items = default)
	{
		Id = id;
		Condition = condition;
		Items = items;
	}
}