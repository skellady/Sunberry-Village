using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace SunberryVillage.Maps;

internal static class TileToggleManager
{
	internal static Dictionary<string, HashSet<TileToggleData>> ToggleSets = new();

	internal static bool HandleToggleTilesAction(GameLocation location, string[] args, Farmer player, Point tile)
	{
		// strip out 0th entry because it just contains the action name
		string[] actionParams = args[1..];

		if (ToggleSets.ContainsKey(actionParams[0]))
		{
			return ToggleTiles(actionParams[0], location);
		}

		// number of parameters should be 1 (the ID) + a multiple of 5 greater than 0 (the tiles to toggle)
		if (actionParams.Length < 1 || (actionParams.Length - 1) % 5 != 0)
			throw new Exception("Incorrect number of arguments provided to SunberryTeam.SBVSMAPI_ToggleTiles action." +
				"\nProper syntax for SunberryTeam.SBVSMAPI_ToggleTiles is as follows: SunberryTeam.SBVSMAPI_ToggleTiles [string:UniqueToggleSetID] [string:Layer1] [int:x1] [int:y1] [int:offTileIndex1] [int:onTileIndex1] [string:Layer2] [int:x2] [int:y2] [int:offTileIndex2] [int:onTileIndex2] ... [string:LayerN] [int:xN] [int:yN] [int:offTileIndexN] [int:onTileIndexN]");



		return true;
	}

	private static bool ToggleTiles(string key, GameLocation location)
	{
		HashSet<TileToggleData> toggleSet = ToggleSets[key];

		foreach (var tile in toggleSet)
		{
			tile.Active = !tile.Active;
			// figure out tilesheet number stuff
			location.setMapTileIndex(tile.X, tile.Y, tile.Active ? tile.OnTileIndex : tile.OffTileIndex, tile.Layer);
		}
	}
}

// this is bad
// to be flexible enough it has to be stupidly complicated to input
internal class TileToggleData
{
	internal readonly int X;
	internal readonly int Y;
	internal readonly int OffTileIndex;
	internal readonly int OnTileIndex;
	internal readonly string Layer;

	internal bool Active;
}