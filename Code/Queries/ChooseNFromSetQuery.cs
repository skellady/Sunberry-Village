using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Internal;
using SunberryVillage.Utilities;

namespace SunberryVillage.Queries;
internal class ChooseNFromSetQuery
{
	public static IEnumerable<ItemQueryResult> HandleChooseNFromSetQuery(string key, string arguments, ItemQueryContext context, bool avoidRepeat, HashSet<string> avoidItemIds, Action<string, string> logError)
	{
		if (string.IsNullOrEmpty(arguments))
		{
			return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError,
				"Query must be formatted as followed: \"SunberryTeam.SBVSMAPI_HandleChooseNFromSet <quantityToChoose> <itemId1> <itemId2> ... <itemIdN>\".");
		}

		string[] splitArgs = ArgUtility.SplitBySpace(arguments);
		
		if (splitArgs.Length < 2)
		{
			return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError,
				"Query must be formatted as followed: \"SunberryTeam.SBVSMAPI_HandleChooseNFromSet <quantityToChoose> <itemId1> <itemId2> ... <itemIdN>\".");
		}

		if (!int.TryParse(splitArgs[0], out int quantity))
		{
			return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError,
				"Query must be formatted as followed: \"SunberryTeam.SBVSMAPI_HandleChooseNFromSet <quantityToChoose> <itemId1> <itemId2> ... <itemIdN>\".");
		}

		string[] itemIds = splitArgs[1..];

		if (quantity >= itemIds.Length)
		{
			return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError,
				"Quantity requested is greater than or equal to number of possible options.");
		}

		List<ItemQueryResult> results = new();
		foreach (string itemId in itemIds)
		{
			if (ItemRegistry.Exists(itemId) && !avoidItemIds.Contains(itemId))
			{
				results.Add(new ItemQueryResult(ItemRegistry.Create(itemId)));
			}
			else
			{
				return ItemQueryResolver.Helpers.ErrorResult(key, arguments, logError,
					$"Provided item ID {itemId} is invalid or excluded from query.");
			}
		}

		Random random =
			Utility.CreateDaySaveRandom(Game1.hash.GetDeterministicHashCode("SunberryTeam.SBVSMAPI_ChooseNFromSetRandom"));
		results.Shuffle(random);

		return results.Take(quantity);
	}
}
