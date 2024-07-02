using StardewModdingAPI.Utilities;
using StardewValley;

namespace SunberryVillage.Queries;
internal class IsPassiveFestivalPrecondition
{
	internal static bool IsPassiveFestivalToday(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetOptional(args, 1, out string locationContext, out string error, location.GetLocationContextId()))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}

		SDate today = SDate.Now();
		return Utility.TryGetPassiveFestivalDataForDay(today.Day, today.Season, locationContext, out _, out _);
	}

	internal static bool IsPassiveFestivalOnGivenDay(GameLocation location, string eventId, string[] args)
	{
		// string season, int day, [string locationContext]

		if (!ArgUtility.TryGet(args, 1, out string season, out string error, false)
			|| !ArgUtility.TryGetInt(args, 2, out int day, out error)
		    || !ArgUtility.TryGetOptional(args, 3, out string locationContext, out error, location.GetLocationContextId())
			)
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}

		SDate date = new(day, season);
		return Utility.TryGetPassiveFestivalDataForDay(date.Day, date.Season, locationContext, out _, out _);
	}
}
