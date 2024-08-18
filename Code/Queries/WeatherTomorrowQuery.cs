using System.Collections.Generic;
using StardewValley;
using StardewValley.Delegates;
using GSQHelper = StardewValley.GameStateQuery.Helpers;

namespace SunberryVillage.Queries;

internal class WeatherTomorrowQuery
{
	
	internal static bool HandleWeatherTomorrowQuery(string[] query, GameStateQueryContext context)
	{
		if (!ArgUtility.TryGet(query, 1, out string location, out string error, false))
		{
			return GSQHelper.ErrorResult(query, error);
		}

		GameLocation loc = GSQHelper.GetLocation(location, context.Location);

		if (loc == null)
		{
			return GSQHelper.ErrorResult(query, $"Could not find location matching provided location name \"{location}\"");
		}
		

		if (!ArgUtility.TryGet(query, 2, out string weather, out error, false))
		{
			return GSQHelper.ErrorResult(query, error);
		}

		List<string> weathers = new() { weather };

		ArgUtility.TryGetOptionalRemainder(query, 3, out string additionalWeathers);

		if (additionalWeathers != null)
		{
			weathers.AddRange(ArgUtility.SplitBySpace(additionalWeathers));
		}

		return weathers.Contains(loc.GetWeather().WeatherForTomorrow);
	}
}