using System.Collections.Generic;
using StardewValley;

namespace SunberryVillage.Utilities;

internal class ConsoleCommandManager
{
	internal static void InitializeConsoleCommands()
	{
		#if DEBUG
		Globals.CCHelper.Add("sophie.dt.testevent", "Test Diala's tarot event", (command, args) =>
			{
				string eventString = Game1.content.Load<Dictionary<string, string>>("SunberryTeam.SBV/Tarot/Event")["Event"];

				GameLocation currentLoc = Game1.currentLocation;
				currentLoc.startEvent(new Event(eventString));
			}
		);

		Globals.CCHelper.Add("sophie.dt.clearmoddata", "Clears the flag that blocks you from having multiple tarot readings done in a single day", (command, args) =>
			{
				Game1.player.modData.Remove("SunberryTeam.SBV/Tarot/ReadingDoneForToday");
			}
		);
		#endif
	}
}
