using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Internal;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewValley.TokenizableStrings;
using xTile.Layers;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace SunberryVillage.Shops;
[HarmonyPatch]
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

		// should never happen lol - usually means the CP portion didn't load properly
		if (marketDailySpecialAsset == null || !marketDailySpecialAsset.Any())
		{
			Log.Warn($"Could not get data from asset \"{MarketSpecialAssetPath}\". Creating generic fallback item \"(O)0\".");
			MarketDailySpecialItem = ItemRegistry.Create("(O)0");
			return;
		}
		
		Random dailyRandom = Utility.CreateDaySaveRandom();
		ItemQueryContext itemQueryContext = new(Game1.currentLocation, Game1.player, null, "skellady.SBVCP_MarketDailySpecialItemQuery");
		List<GenericSpawnItemData> spawnPool = [];

		foreach (MarketDailySpecialData entry in marketDailySpecialAsset.Values.Where(entry => GameStateQuery.CheckConditions(entry.Condition, Game1.currentLocation, Game1.player)))
		{
			spawnPool.AddRange(entry.Items.Where(data => GameStateQuery.CheckConditions(data.Condition)));
		}

		bool itemResolved = false;
		while (!itemResolved && spawnPool.Any())
		{
			GenericSpawnItemData selectedEntry = spawnPool.GetRandomElementFromList(dailyRandom, true);

			// assume success unless logError fires
			itemResolved = true;
			MarketDailySpecialItem = ItemQueryResolver.TryResolveRandomItem(selectedEntry, itemQueryContext,
				logError: (query, message) =>
				{
					itemResolved = false;
					Log.Warn($"Failed parsing item query \"{query}\": {message}");
				});
		}

		if (itemResolved)
		{
			Log.Trace($"Selected item \"{GetOfferItemName()}\" [{MarketDailySpecialItem.QualifiedItemId} '{MarketDailySpecialItem.DisplayName}' x {MarketDailySpecialItem.Stack}] for market daily special.");
		}
		else
		{
			Log.Warn("Unable to select valid item for market daily special. Creating generic fallback item \"(O)0\".");
			MarketDailySpecialItem = ItemRegistry.Create("(O)0");
		}

		
		// add to market
		GameLocation market = Game1.getLocationFromName("Custom_SBV_AriMarket");

		if (market is null)
		{
			Log.Error(
				"Unable to find location matching \"Custom_SBV_AriMarket\", failed to add daily market special sprite to location.");
		}
		else
		{
			DailySpecialSprite =
				new TemporaryAnimatedSprite(null, Rectangle.Empty, Vector2.Zero, flipped: false, 0f, Color.White)
				{
					layerDepth = 0.135f
				};
			DailySpecialSprite.CopyAppearanceFromItemId(MarketDailySpecialItem.QualifiedItemId);

			int heightOffset = DailySpecialSprite.sourceRect.Height / 16;

			// iterate over map to find property in order to determine where to place sprite for special item
			bool found = false;
			Layer layer = market.Map.GetLayer("Buildings");
			for (int x = 0; x < layer.LayerWidth; x++)
			{
				if (found)
					break;

				for (int y = 0; y < layer.LayerHeight; y++)
				{
					if (found)
						break;

					string[] action = market.GetTilePropertySplitBySpaces("Action", "Buildings", x, y);
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
		}
	}

	[HarmonyPatch(nameof(GameLocation), nameof(GameLocation.setUpLocationSpecificFlair))]
	[HarmonyPrefix]
	public static void setUpLocationSpecificFlair_Prefix(GameLocation __instance)
    {
		if (__instance.Name != "Custom_SBV_AriMarket" || Game1.player.modData.ContainsKey(AlreadyPurchasedMailFlag))
			return;

		__instance.TemporarySprites.Add(DailySpecialSprite);
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

		return TokenParser.ParseText($"[ArticleFor {MarketDailySpecialItem.DisplayName}] {MarketDailySpecialItem.DisplayName}{(MarketDailySpecialItem.Stack == 1 ? "" : $" {Utils.GetTranslationWithPlaceholder("MarketDailySpecialQuantityIndicator")} {MarketDailySpecialItem.Stack}")}");
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
		if (MarketDailySpecialItem.modData.TryGetValue("SunberryTeam.SBVSMAPI_MarketSpecialPrice", out string priceString) && int.TryParse(priceString, out int price))
			return price;

		return MarketDailySpecialItem.sellToStorePrice();
	}

	internal static void RemoveDailySpecial()
	{
		Game1.player.modData[AlreadyPurchasedMailFlag] = "true";
		Game1.currentLocation.TemporarySprites.Remove(DailySpecialSprite);
	}
}

internal class MarketDailySpecialData(string id, string condition = "", List<GenericSpawnItemDataWithCondition> items = default)
{
	internal string Id = id;
	internal string Condition = condition;
	internal List<GenericSpawnItemDataWithCondition> Items = items;
}