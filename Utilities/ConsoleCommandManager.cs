using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using SunberryVillage.Lighting;
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

		#region Lighting

		Globals.CCHelper.Add("sbv.light.add", "Adds or updates test light in Sunberry. Optional argument: [radius (as a float)]", (string command, string[] args) =>
			{
				if (!IsWorldReady())
					return;

				// was on the verge of making this way more complicated and decided to scale it back. may revisit it if i feel the need to.
				// okay yes i will revisit this later bc im overhauling the lighting stuff :arson:
				switch (args.Length)
				{
					// one parameter provided - assumed to be radius
					case 1:
							if (float.TryParse(args[0], out float value))
								LightingHandler.AddLight(value);

							else
								Log.Warn($"Unrecognized argument \"{args[0]}\"");

							break;

					// no arguments provided - use default parameters
					case 0:
					default:
						LightingHandler.AddLight();
						break;
				}
			}
		);

		Globals.CCHelper.Add("sbv.light.remove", "Removes test light in Sunberry.", (_, _) =>
			{
				if (!IsWorldReady())
					return;

				LightingHandler.RemoveLight();
			}
		);

		#endregion

#endif
	}

	// Helpers

	internal static bool IsWorldReady()
	{
		if (Context.IsWorldReady) return true;

		Log.Warn("This command should only be used in a loaded save.");
		return false;

	}


}
