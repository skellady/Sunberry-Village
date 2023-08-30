using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using System.Collections.Generic;

namespace SunberryVillage.Utilities;

internal class ConsoleCommandManager
{
	internal static void InitializeConsoleCommands()
	{
#if DEBUG

		#region Tarot

		Globals.CCHelper.Add("sbv.tarot.forceevent", "Test Diala's tarot event", (_, _) =>
			{
				if (!IsWorldReady())
					return;

				string eventString = Game1.content.Load<Dictionary<string, string>>("SunberryTeam.SBV/Tarot/Event")["Event"];

				GameLocation currentLoc = Game1.currentLocation;
				currentLoc.startEvent(new Event(eventString));
			}
		);

		Globals.CCHelper.Add("sbv.tarot.clearflag", "Clears the flag that blocks you from having multiple tarot readings done in a single day", (_, _) =>
			{
				if (!IsWorldReady())
					return;

				Game1.player.modData.Remove("SunberryTeam.SBV/Tarot/ReadingDoneForToday");
			}
		);

		#endregion

		#region Quests

		Globals.CCHelper.Add("sbv.quests.completeall", "Marks all quests in your log as completed.", (_, _) =>
			{
				if (!IsWorldReady())
					return;

				foreach (Quest quest in Game1.player.questLog)
				{
					quest.questComplete();
				}
			}
		);

		#endregion

#endif
	}

	internal static bool IsWorldReady()
	{
		if (Context.IsWorldReady) return true;

		Log.Warn("This command should only be used in a loaded save.");
		return false;

	}
}
