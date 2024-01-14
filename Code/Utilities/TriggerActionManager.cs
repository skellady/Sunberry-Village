using Microsoft.Xna.Framework;
using StardewValley;
using System;

namespace SunberryVillage.Utilities;

internal class TriggerActionManager
{
	internal static bool HandleDoTrigger(GameLocation location, string[] args, Farmer player, Point tile)
	{
		StardewValley.Triggers.TriggerActionManager.Raise("");
		return true;
	}
}