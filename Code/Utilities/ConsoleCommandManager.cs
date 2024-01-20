using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using SunberryVillage.Animations;
using SunberryVillage.Integration.Tokens;
using SunberryVillage.Lighting;
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

		Globals.CCHelper.Add("sbv.lights.add", "Adds or updates test light on the current map." +
			"\nFormat: \"sbv.lights.add\" OR \"sbv.lights.add [id] [xPos] [yPos] [intensity]\". If no arguments provided, will use random id, current position and default intensity of 1.0." +
			"\nArguments:" +
			"\n\tid (string): The id of the light to add. This will allow you to edit or remove the light later." +
			"\n\txPos (float): X position (in tiles) where the light should be added. May use decimals (i.e., 58.5 is a valid argument)." +
			"\n\tyPos (float): Y position (in tiles) where the light should be added. May use decimals (i.e., 67.1 is a valid argument)." +
			"\n\tintensity (float): Intensity of the light. May use decimals (i.e., 3.92 is a valid argument).", (string command, string[] args) =>
			{
				if (!IsWorldReady())
					return;

				// if no args provided, fill in default values
				if (args.Length == 0)
				{
					string tempId = Utils.GenerateRandomString(6);
					string xPos = Game1.player.Tile.X.ToString();
					string yPos = Game1.player.Tile.Y.ToString();
					string intensity = "1";

					args = new string[] { tempId, xPos, yPos, intensity };
				}

				if (LightingHandler.TryAddOrUpdateTempLight(args, out string error))
				{
					LightingHandler.AddLightsToCurrentLocation();

					Log.Info($"Temporary light created with parameters {{{string.Join(", ", args)}}}.");
					Log.Warn($"NOTE: THIS LIGHT WILL NOT PERSIST AFTER YOU EXIT THE GAME. IT IS SOLELY FOR TESTING PURPOSES. " +
						$"In order to define a persistent light, you need to edit the \"{LightingHandler.LightsAssetPath}\" asset via Content Patcher. " +
						$"See the example pack for details.");
				}
				else
				{
					Log.Error($"Error encountered while attempting to add light: {error}");
				}

			}
		);

		Globals.CCHelper.Add("sbv.lights.remove", "Removes temp light with given id.\nFormat: \"sbv.lights.remove [id]\".\nArguments:" +
			"\n\tid (string): The id of the light to remove.", (_, args) =>
			{
				if (!IsWorldReady())
					return;

				if (LightingHandler.RemoveLight(args[0]))
					Log.Info($"Light with id \"{args[0]}\" removed.");
				else
					Log.Warn($"Light with id \"{args[0]}\" not removed. (Does it exist?)");

			}
		);

		Globals.CCHelper.Add("sbv.lights.listhere", "Prints list of all custom lights in the current location.", (_, _) =>
			{
				if (!IsWorldReady())
					return;

				GameLocation loc = Game1.currentLocation;

				Log.Info($"\nData asset lights in the current location: \n\t{string.Join(",\n\t", LightingHandler.Lights.Where(kvp => kvp.Value.GameLocation.Equals(loc)).Select(kvp => $"{kvp.Key}: {kvp.Value}"))}" +
					$"\n\nTemporary lights in the current location: \n\t{string.Join(",\n\t", LightingHandler.TempLights.Where(kvp => kvp.Value.GameLocation.Equals(loc)).Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
			}
		);

		Globals.CCHelper.Add("sbv.lights.listall", "Prints list of all custom lights.", (_, _) =>
			{
				if (!IsWorldReady())
					return;

				Log.Info($"\nData asset lights: \n\t{string.Join(",\n\t", LightingHandler.Lights.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}" +
					$"\n\nTemporary lights: \n\t{string.Join(",\n\t", LightingHandler.TempLights.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
			}
		);

		Globals.CCHelper.Add("sbv.lights.infodump", "Prints list of all light sources in the current location.", (_, _) =>
			{
				if (!IsWorldReady())
					return;

				Log.Info($"Light sources in the current location:\n\t{string.Join(",\n\t", Game1.currentLightSources.Select(l => $"{l.Identifier}: {{{l.textureIndex} | {l.position} | {l.radius} | {l.lightContext} | {l.PlayerID}}}"))}");
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
			AnimationsHandler.Init();
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
				if (!IsWorldReady())
					return;

				GoldenSunberryStageToken.UpdateHeartTotal();
				StringBuilder infoBuilder = new("\nGolden Sunberry Stage Token Info:");

				if (!GoldenSunberryStageToken.Growing)
				{
					infoBuilder.Append("\n\tGolden Sunberry is not currently growing.");
				}
				else if (GoldenSunberryStageToken.Stage >= GoldenSunberryStageToken.MAX_STAGE)
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

					infoBuilder.Append($"\n\tNumber of Nights Needed to Advance Stage: {GoldenSunberryStageToken.MIN_NIGHTS_IN_STAGE}");
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

		#region Misc

		Globals.CCHelper.Add("sbv.misc.getlostbooks", "Gets all lost books.", (_, _) =>
			{
				if (IsWorldReady())
					Game1.netWorldState.Value.LostBooksFound = 21;
			}
		);

		Globals.CCHelper.Add("sbv.misc.testbook", "Loads string in book format for testing. Arguments: at least one path+key string, without quotes around it. Multiple may be provided and should be separated by a space." +
			"\nExample: \"sbv.misc.testbook Strings\\\\StringsFromCSFiles:summer\"", (_, args) =>
			{
				if (!args.Any())
					return;

				string book = string.Join("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^", args.Select(s => Game1.content.LoadString(s)));

				Log.Info($"Displaying the following in book format:\n\n{book}\n");
				Game1.drawLetterMessage(book);
			}
		);

		#endregion

#endif
	}

	private static void PrintWalkableTileCount()
	{
		if (!IsWorldReady())
			return;

		Log.Info($"Walkable tiles in {Game1.currentLocation.Name}: {DoWalkableFloodFill()}");
	}

	private static void PrintFishingDistribution(int numToCatch)
	{
		if (!IsWorldReady())
			return;

		Dictionary<string, int> catches = DoFishingDistribution(numToCatch);
		Log.Info($"Fishing distribution in {Game1.currentLocation.Name} over {numToCatch} attempts:\n\t{string.Join("\n\t", catches.OrderBy(kvp => kvp.Value).Select(kvp => $"{kvp.Value} x \"{kvp.Key}\""))}"); // sorry this line is ugly as fuck i don't care
	}


	// Helpers

	internal static bool IsWorldReady()
	{
		if (Context.IsWorldReady) return true;

		Log.Warn("This command should only be used in a loaded save.");
		return false;

	}


	private static Dictionary<string, int> DoFishingDistribution(int numToCatch)
	{
		Dictionary<string, int> catchQuantities = new();

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
