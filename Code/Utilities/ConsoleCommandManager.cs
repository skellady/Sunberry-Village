using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.Network;
using StardewValley.Quests;
using SunberryVillage.Animations;
using SunberryVillage.Events.Tarot;
using SunberryVillage.Integration.Tokens;
using SunberryVillage.Lighting;
using SunberryVillage.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xTile;
using xTile.Layers;

namespace SunberryVillage.Utilities;

internal class ConsoleCommandManager
{
	private static bool[,] walkable;
	private static bool drawWalkable;

	private static readonly Random Random = new();

	internal static void InitializeConsoleCommands()
	{
#if DEBUG

		#region Tarot

		Globals.CCHelper.Add("sbv.tarot.forceevent", "Test Diala's tarot event", (_, _) =>
			{
				if (!PrintWorldReady())
					return;

				string eventString = Game1.content.Load<Dictionary<string, string>>("SunberryTeam.SBV/Tarot/Event")["Event"];

				GameLocation currentLoc = Game1.currentLocation;
				currentLoc.startEvent(new Event(eventString));
			}
		);

		Globals.CCHelper.Add("sbv.tarot.clearflag", "Clears the flag that blocks you from having multiple tarot readings done in a single day", (_, _) =>
			{
				if (!PrintWorldReady())
					return;

				Game1.player.modData.Remove("SunberryTeam.SBVSMAPI_TarotReadingDoneForToday");
			}
		);

		Globals.CCHelper.Add("sbv.tarot.temperance", "Gives Temperance buff", (_, _) =>
			{
				if (!PrintWorldReady())
					return;

				TarotCard card = new(
					id: "Temperance",
					buffEffects: null,	// custom buff for Temperance
					iconIndex: 13,
					condition: () => Game1.player.eventsSeen.Contains("JonghyukCoffee")
					);

				card.ApplyBuff("debug");
			}
				
		);

		#endregion

		#region Quests

		Globals.CCHelper.Add("sbv.quests.completeall", "Marks all quests in your log as completed.", (_, _) =>
			{
				if (!PrintWorldReady())
					return;

				foreach (Quest quest in Game1.player.questLog)
				{
					quest.questComplete();
				}
			}
		);

		#endregion

		#region Lighting

		Globals.CCHelper.Add("sbv.lights.listhere", "Prints list of all custom lights in the current location.", (_, _) =>
			{
				if (!PrintWorldReady())
					return;

				GameLocation loc = Game1.currentLocation;

				Log.Info($"\nData asset lights in the current location: \n\t{string.Join(",\n\t", LightingManager.Lights.Where(kvp => kvp.Value.GameLocation.Equals(loc)).Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
			}
		);

		Globals.CCHelper.Add("sbv.lights.listall", "Prints list of all custom lights.", (_, _) =>
			{
				if (!PrintWorldReady())
					return;

				Log.Info($"\nData asset lights: \n\t{string.Join(",\n\t", LightingManager.Lights.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
			}
		);

		Globals.CCHelper.Add("sbv.lights.infodump", "Prints list of all light sources in the current location.", (_, _) =>
			{
				if (!PrintWorldReady())
					return;

				Log.Info($"Light sources in the current location:\n\t{string.Join(",\n\t", Game1.currentLightSources.Select(l => $"{l.Value.Id}: {{{l.Value.textureIndex} | {l.Value.position} | {l.Value.radius} | {l.Value.lightContext} | {l.Value.PlayerID}}}"))}");
			}
		);

		#endregion

		#region Animations


		Globals.CCHelper.Add("sbv.anim.test", "Tests the specified animation for the character." +
			"\nFormat: \"sbv.anim.test [npcName] [animationName]\"" +
			"\nArguments:" +
			"\n\tnpcName (string): The internal name of the NPC who is performing the animation." +
			"\n\tanimationName (string): The name of the animation to perform.", (command, args) =>
		{
			AnimationsManager.Init();
			if (args.Length < 2)
			{
				return;
			}
			NPC npc = Game1.getCharacterFromName(args[0]);
			if (npc != null)
			{
				npc.StartActivityRouteEndBehavior(args[1], "test");
			}
		});

		#endregion

		#region Mines

		Globals.CCHelper.Add("sbv.mines.walkablearea", "Counts walkable tiles on the current map.", (_, _) => PrintWalkableTileCount());
		Globals.CCHelper.Add("sbv.mines.toggledraw", "Toggles debug drawing of walkable tiles.", (_, _) =>
			{
				drawWalkable = !drawWalkable;
				if (drawWalkable)
					PrintWalkableTileCount();
			}
		);

		Globals.EventHelper.Display.RenderedWorld += DrawWalkableTileOverlay;
		Globals.EventHelper.Player.Warped += ClearWalkableTileOverlay;

		#endregion

		#region Golden Sunberry Stage Token

		Globals.CCHelper.Add("sbv.gs.stage", "Prints the current expected output of the Golden Sunberry Stage CP token.", (_, _) =>
			{
				Log.Info($"Current expected output of the Golden Sunberry Stage CP token is: {GoldenSunberryStageToken.Stage}");
			});

		Globals.CCHelper.Add("sbv.gs.info", "Displays info about the golden sunberry stage token. " +
			"If any arguments are supplied, will provide verbose output.", (_, args) =>
			{
				if (!PrintWorldReady())
					return;

				GoldenSunberryStageToken.UpdateHeartTotal();
				StringBuilder infoBuilder = new("\nGolden Sunberry Stage Token Info:");

				if (!GoldenSunberryStageToken.Growing)
				{
					infoBuilder.Append("\n\tGolden Sunberry is not currently growing.");
				}
				else if (GoldenSunberryStageToken.Stage >= GoldenSunberryStageToken.MaxStage)
				{
					infoBuilder.Append("\n\tGolden Sunberry has reached its final growth stage and will not grow any further.");
				}
				else
				{
					infoBuilder.Append("\n\tGolden Sunberry is currently growing.");
					infoBuilder.Append($"\n\tCurrent Golden Sunberry Stage: {GoldenSunberryStageToken.Stage}");

					if (GoldenSunberryStageToken.ForceGrow)
						infoBuilder.Append("\n\tForce Grow is toggled on, so the stage will advance overnight whether the conditions are met or not.");
					else
						infoBuilder.Append($"\n\tStage will {(GoldenSunberryStageToken.ReadyToAdvanceStage(probe: true) ? "advance overnight" : "not advance overnight")}.");

					infoBuilder.Append($"\n\tNumber of Nights Needed to Advance Stage: {GoldenSunberryStageToken.MinNightsInStage}");
					infoBuilder.Append($"\n\tNumber of Nights in Current Stage: {GoldenSunberryStageToken.NightsInStage}");
					infoBuilder.Append($"\n\tSunberry Village Heart Level Needed to Advance Stage: {GoldenSunberryStageToken.HeartsNeededToAdvanceStage[GoldenSunberryStageToken.Stage]}");
					infoBuilder.Append($"\n\tCurrent Sunberry Village Heart Level: {GoldenSunberryStageToken.HeartTotal}");

					if (args.Any())
					{
						infoBuilder.Append("\n\tCurrent Sunberry Village Heart Level Breakdown:");
						foreach (NPC npc in Utility.getAllCharacters())
						{
							if (GoldenSunberryStageToken.ResidentNames.Contains(npc.Name))
								infoBuilder.Append($"\n\t\t{npc.Name}: {Game1.MasterPlayer.getFriendshipHeartLevelForNPC(npc.Name)}");
						}
					}

				}

				Log.Info(infoBuilder);

			});

		Globals.CCHelper.Add("sbv.gs.startgrowing", "Forces the token to act as if the golden sunberry is currently growing.", (_, _) =>
			{
				GoldenSunberryStageToken.Stage = 0;
				GoldenSunberryStageToken.Growing = true;
				Log.Info("Golden Sunberry will start growing.");
			});

		Globals.CCHelper.Add("sbv.gs.toggleforcegrow", "Toggles a bypass that will force the stage to advance overnight. ", (_, _) =>
			{
				if (!GoldenSunberryStageToken.Growing)
				{
					GoldenSunberryStageToken.Stage = 0;
					GoldenSunberryStageToken.Growing = true;
				}

				GoldenSunberryStageToken.ForceGrow = !GoldenSunberryStageToken.ForceGrow;
				Log.Info($"ForceGrow toggled {(GoldenSunberryStageToken.ForceGrow ? "on" : "off")}.");
			});

		#endregion

		#region Fishing

		Globals.CCHelper.Add("sbv.fish.dist", "Gets an estimate of the distribution of fish for the current location. Uses the player's fishing level.", (_, args) => {
			int numToCatch = 100;
			if (args.Length > 0 && int.TryParse(args[0], out int inputNum))
				numToCatch = inputNum;
			
			PrintFishingDistribution(numToCatch);
		});

		#endregion

		#region Perfection

		Globals.CCHelper.Add("sbv.perf.countnpcs", "Counts NPCs needed for perfection.", (_, _) =>
			{
				if (PrintWorldReady())
				{
					int maxedFriends = 0;
					int totalFriends = 0;
					foreach ((string npcName, CharacterData data) in Game1.characterData)
					{
						if (!data.PerfectionScore || GameStateQuery.IsImmutablyFalse(data.CanSocialize))
						{
							continue;
						}

						totalFriends++;

						if (!Game1.player.friendshipData.TryGetValue(npcName, out var friendship))
							continue;

						int maxPoints = (data.CanBeRomanced ? 8 : 10) * 250;
						if (friendship != null && friendship.Points >= maxPoints)
						{
							maxedFriends++;
						}
					}

					float percentMaxed = (float)maxedFriends / totalFriends * 100f;

					Log.Info($"{totalFriends} total NPCs needed for perfection, {maxedFriends} currently maxed out ({percentMaxed:n2}%)");
				}
			}
		);

		#endregion

		#region Misc

		Globals.CCHelper.Add("sbv.misc.getlostbooks", "Gets all lost books.", (_, _) =>
			{
				if (PrintWorldReady())
					Game1.netWorldState.Value.LostBooksFound = 21;
			}
		);

		Globals.CCHelper.Add("sbv.misc.testbook", "Loads string in book format for testing. Arguments: at least one path+key string, without quotes around it. Multiple may be provided and should be separated by a space." +
			"\nExample: \"sbv.misc.testbook Strings\\\\StringsFromCSFiles:summer\"", (_, args) =>
			{
				if (!args.Any())
					return;

				List<string> text = args.Select(s => Game1.content.LoadString(s)).ToList();
				string book = string.Join("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^", text);

				Log.Info($"Displaying the following in book format:\n\n{book}\n");
				Game1.activeClickableMenu = BookViewerMenu.GetMenu(text, "");
			}
		);

		Globals.CCHelper.Add("sbv.misc.checkintro", "Checks to see who is counted in Introductions quest and what their status is", (_, args) =>
			{
				if (!PrintWorldReady())
					return;

				List<string> npcsMet = [];
				List<string> npcsNotMet = [];
				int introNum = 0;
				foreach ((string npcName, CharacterData data) in Game1.characterData)
				{
					if (!data.IntroductionsQuest ?? data.HomeRegion != "Town")
						continue;

					introNum++;

					if (!Game1.player.friendshipData.TryGetValue(npcName, out Friendship friendship))
					{
						npcsNotMet.Add(npcName);
					}
					else
					{
						npcsMet.Add(npcName);
					}
				}

				StringBuilder sb = new();
				sb.AppendLine($"Introductions quest progress: {npcsMet.Count} / {introNum}");
				sb.AppendLine($"NPCs met:\n\t{string.Join("\n\t", npcsMet)}");
				sb.AppendLine($"NPCs not met:\n\t{string.Join("\n\t", npcsNotMet)}");

				if (args.Any())
				{
					Log.Info(sb);
				}
				else
				{
					Log.Trace(sb);
				}
			});

		Globals.CCHelper.Add("sbv.misc.testimageviewer", "Creates test image viewer menu", (_, _) =>
			{
				Game1.activeClickableMenu = new ImageMenu("TestId");
			});

		Globals.CCHelper.Add("sbv.misc.mailflag", "Lists all mailflags current player has matching specified text, or all mailflags if no text is provided.", (_, args) =>
			{
				if (!PrintWorldReady())
					return;

				StringBuilder sb = new();

				int flagCount = 0;

				if (!args.Any())
				{
					string flags = string.Join(", ", Game1.player.mailReceived);

					flagCount = Game1.player.mailReceived.Count;

					sb.Append($"Player has {flagCount} {(flagCount == 1 ? "mailflag" : "mailflags")}:\n\n");
					sb.Append(flags);
				}
				else
				{
					string matchText = args[0];

					List<string> matchingFlags = Game1.player.mailReceived.Where(str => str.Contains(matchText, StringComparison.OrdinalIgnoreCase)).ToList();

					flagCount = matchingFlags.Count;
					string flags = string.Join(", ", matchingFlags);

					sb.Append($"Player has {flagCount} {(flagCount == 1 ? "mailflag" : "mailflags")} matching specified text \"{args[0]}\":\n\n");
					sb.Append(flags);
				}

				sb.Append('\n');
				Log.Info(sb);
			});

		Globals.CCHelper.Add("sbv.so.flag", "Sets the mail flag for the Sunberry special order board to show up regardless of unlock conditions.", (_, _) =>
			{
				if (!PrintWorldReady())
					return;

				Game1.MasterPlayer.mailReceived.Add("skellady.SBVCP_SpecialOrderBoardReady");
			});

		Globals.CCHelper.Add("sbv.setworldstate", "Adds or removes the given worldstate ID.", (_, args) =>
		{
			if (!PrintWorldReady() || !args.Any())
				return;

			if (!ArgUtility.TryGet(args, 0, out string worldState, out string error, allowBlank: false, name: "worldstate flag"))
			{
                Log.Error($"Failed to set world state: {error}");
                return;
            }

			if (!ArgUtility.TryGetOptionalBool(args, 1, out bool add, out error, defaultValue: true, name: "value"))
			{
				Log.Error($"Failed to set world state: {error}");
				return;
			}

			if (add)
			{
				NetWorldState.addWorldStateIDEverywhere(worldState);

				Log.Info($"Added world state ID {worldState}.");
			}
			else
			{
				Game1.netWorldState.Value.removeWorldStateID(worldState);
				Game1.worldStateIDs.Remove(worldState);

                Log.Info($"Removed world state ID {worldState}.");
            }

        });

        Globals.CCHelper.Add("sbv.listworldstate", "Lists out current worldstate IDs.", (_, _) =>
        {
            if (!PrintWorldReady())
                return;

			Log.Info($"Current world state IDs:\n\t\t{string.Join("\n\t\t", Game1.worldStateIDs)}");
        });

        #endregion

#endif
    }

	private static void PrintWalkableTileCount()
	{
		if (!PrintWorldReady())
			return;

		Log.Info($"Walkable tiles in {Game1.currentLocation.Name}: {DoWalkableFloodFill()}");
	}

	private static void PrintFishingDistribution(int numToCatch)
	{
		if (!PrintWorldReady())
			return;

		Dictionary<string, int> catches = DoFishingDistribution(numToCatch);
		Log.Info($"Fishing distribution in {Game1.currentLocation.Name} over {numToCatch} attempts:\n\t{string.Join("\n\t", catches.OrderBy(kvp => kvp.Value).Select(kvp => $"{kvp.Value} x \"{kvp.Key}\""))}"); // sorry this line is ugly as fuck i don't care
	}


	// Helpers

	internal static bool PrintWorldReady()
	{
		if (Context.IsWorldReady) return true;

		Log.Warn("This command should only be used in a loaded save.");
		return false;

	}


	private static Dictionary<string, int> DoFishingDistribution(int numToCatch)
	{
		Dictionary<string, int> catchQuantities = [];

		for (int i = 0; i < numToCatch; i++)
		{
			Item item = Game1.currentLocation.getFish(
				millisecondsAfterNibble: 0f,
				bait: "",
				waterDepth: Random.Next(1, 6),
				who: Game1.player,
				baitPotency: 0f,
				bobberTile: Game1.player.Tile
			);

			if (catchQuantities.ContainsKey(item.DisplayName))
				catchQuantities[item.DisplayName] += item.Stack;
			else
				catchQuantities.Add(item.DisplayName, item.Stack);
		}

		return catchQuantities;
	}

	private static int DoWalkableFloodFill()
	{
		int tileCount = 0;
		GameLocation curLoc = Game1.currentLocation;
		Map map = curLoc.Map;
		Layer back = map.GetLayer("Back");
		Layer buildings = map.GetLayer("Buildings");
		Rectangle bounds = new(0, 0, back.LayerWidth, back.LayerHeight);

		bool[,] alreadyChecked = new bool[back.LayerWidth, back.LayerHeight];
		walkable = new bool[back.LayerWidth, back.LayerHeight];

		Point currentTile = Game1.player.TilePoint;
		WalkableFloodFill(curLoc, currentTile, bounds, buildings, alreadyChecked, ref tileCount);

		return tileCount;
	}

	private static void WalkableFloodFill(GameLocation currentLocation, Point currentTile, Rectangle bounds, Layer buildings, bool[,] alreadyChecked, ref int tileCount)
	{
		if (!bounds.Contains(currentTile))
			return;

		int x = currentTile.X;
		int y = currentTile.Y;

		if (alreadyChecked[x, y])
			return;

		alreadyChecked[x, y] = true;

		if (!IsTilePassable(currentLocation, buildings, x, y))
			return;

		tileCount++;
		walkable[x, y] = true;
		WalkableFloodFill(currentLocation, new Point(x + 1, y), bounds, buildings, alreadyChecked, ref tileCount);
		WalkableFloodFill(currentLocation, new Point(x - 1, y), bounds, buildings, alreadyChecked, ref tileCount);
		WalkableFloodFill(currentLocation, new Point(x, y + 1), bounds, buildings, alreadyChecked, ref tileCount);
		WalkableFloodFill(currentLocation, new Point(x, y - 1), bounds, buildings, alreadyChecked, ref tileCount);
	}

	private static bool IsTilePassable(GameLocation currentLocation, Layer buildings, int x, int y)
	{
		bool backPassable = !currentLocation.isWaterTile(x, y) && currentLocation.doesTileHavePropertyNoNull(x, y, "Passable", "Back") != "F";
		bool buildingsNull = buildings.Tiles[x, y] == null;
		bool buildingsPassable = !buildingsNull && ((currentLocation.doesTileHaveProperty(x, y, "Passable", "Buildings") ?? "F") != "F" || currentLocation.doesTileHavePropertyNoNull(x, y, "Shadow", "Buildings") == "T");
		bool terrainFeature = currentLocation.isTerrainFeatureAt(x, y);

		return !terrainFeature && ((backPassable && buildingsNull) || buildingsPassable);
	}

	private static void DrawWalkableTileOverlay(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
	{
		if (!drawWalkable || walkable is null)
			return;

		for (int x = 0; x < walkable.GetLength(0); x++)
		{
			for (int y = 0; y < walkable.GetLength(1); y++)
			{
				if (walkable[x, y])
					e.SpriteBatch.Draw(
						texture: Game1.staminaRect,
						destinationRectangle: new Rectangle(Game1.GlobalToLocal(new Vector2(x, y) * 64f).ToPoint(), new Point(64, 64)),
						sourceRectangle: Game1.staminaRect.Bounds,
						color: Color.Red * 0.5f,
						rotation: 0f,
						origin: Vector2.Zero,
						effects: Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
						layerDepth: 0f
					);
			}
		}
	}

	private static void ClearWalkableTileOverlay(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
	{
		if (drawWalkable)
			PrintWalkableTileCount();

		else
			walkable = null;
	}

}
