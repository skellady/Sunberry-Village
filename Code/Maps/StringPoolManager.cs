using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TokenizableStrings;
using System.Collections.Generic;
using System.Linq;

namespace SunberryVillage.Maps;
internal class StringPoolManager
{
	internal const string StringPoolsAssetPath = "SunberryTeam.SBVSMAPI/StringPools";
	internal static IAssetName StringPoolsAssetName = Globals.GameContent.ParseAssetName(StringPoolsAssetPath);

	internal static Dictionary<string, StringPool> StringData = new();
	internal static Dictionary<string, List<string>> AlreadySeen = new();

	internal static void AddEventHooks()
	{
		Globals.EventHelper.Content.AssetRequested += StringPools_AssetRequested;
		Globals.EventHelper.Content.AssetsInvalidated += StringPools_AssetsInvalidated;
		Globals.EventHelper.GameLoop.SaveLoaded += InitAsset;
	}

	private static void InitAsset(object? sender, SaveLoadedEventArgs e)
	{
		StringData = Globals.GameContent.Load<Dictionary<string, StringPool>>(StringPoolsAssetName);
	}

	private static void StringPools_AssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
	{
		if (e.NamesWithoutLocale.Contains(StringPoolsAssetName))
			StringData = Globals.GameContent.Load<Dictionary<string, StringPool>>(StringPoolsAssetPath);
	}

	private static void StringPools_AssetRequested(object sender, AssetRequestedEventArgs e)
	{
		if (e.NameWithoutLocale.IsEquivalentTo(StringPoolsAssetPath))
			e.LoadFrom(() => new Dictionary<string, StringPool>(), AssetLoadPriority.Low);
	}

	public static string GetString(string poolId, out string error)
	{
		if (!StringData.TryGetValue(poolId, out StringPool? pool))
		{
			error = $"No pool found with id \"{poolId}\"";
			return "";
		}

		List<string> validIds = [];

		// ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
		foreach ((string stringId, ConditionalMessage stringWithCondition) in pool.Messages) {
			if (GameStateQuery.CheckConditions(stringWithCondition.Condition))
				validIds.Add(stringId);
		}

		if (!validIds.Any()) {
			error = $"No valid strings in pool \"{poolId}\"";
			return "";
		}
		
		List<string> prunedIds = [..validIds];
		if (!AlreadySeen.TryGetValue(poolId, out List<string>? seenIds))
		{
			AlreadySeen[poolId] = [];
		}
		else
		{
			prunedIds.RemoveAll(str => AlreadySeen[poolId].Contains(str));
		}

		if (!prunedIds.Any())
		{
			AlreadySeen[poolId].Clear();

			prunedIds = validIds;

			if (!string.IsNullOrEmpty(pool.FinalString))
			{
				error = "";
				return TokenParser.ParseText(pool.FinalString);
			}
		}

		error = "";

		string selectedId = pool.ShuffleOrder ? Game1.random.ChooseFrom(prunedIds) : prunedIds[0];
		AlreadySeen[poolId].Add(selectedId);

		return TokenParser.ParseText(pool.Messages[selectedId].Message);
	}
}

internal class StringPool
{
	public bool ShuffleOrder = true;
	public Dictionary<string, ConditionalMessage> Messages = [];
	public string FinalString = "";
}

internal class ConditionalMessage
{
	public string Message = "";
	public string Condition = "";
}