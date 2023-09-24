using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using SunberryVillage.Animations;
using SunberryVillage.Lighting;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;

namespace SunberryVillage.Utilities;

internal class ConsoleCommandManager
{
	private static bool[,] walkable;
	private static bool drawWalkable;

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
					string xPos = Game1.player.getTileX().ToString();
					string yPos = Game1.player.getTileY().ToString();
					string intensity = "1";

					args = new string[] {tempId, xPos, yPos, intensity};
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

		
		Globals.Helper.ConsoleCommands.Add("sbv.anim.test", "Tests the specified animation for the character." +
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

#endif
	}

	private static void PrintWalkableTileCount()
	{
		if (!IsWorldReady())
			return;

		int walkableTileCount = DoWalkableFloodFill();

		Log.Info($"Walkable tiles in {Game1.currentLocation.Name}: {walkableTileCount}");
	}

	// Helpers

	internal static bool IsWorldReady()
	{
		if (Context.IsWorldReady) return true;

		Log.Warn("This command should only be used in a loaded save.");
		return false;

	}

	private static int DoWalkableFloodFill()
	{
		int tileCount = 0;
		xTile.Map map = Game1.currentLocation.Map;
		xTile.Layers.Layer back = map.GetLayer("Back");
		xTile.Layers.Layer back2 = map.GetLayer("Back2");
		xTile.Layers.Layer buildings = map.GetLayer("Buildings");
		Rectangle bounds = new(0, 0, back.LayerWidth, back.LayerHeight);

		bool[,] alreadyChecked = new bool[back.LayerWidth, back.LayerHeight];
		walkable = new bool[back.LayerWidth, back.LayerHeight];

		Point currentTile = Game1.player.getTileLocationPoint();


		WalkableFloodFill(currentTile, bounds, back, back2, buildings, alreadyChecked, ref tileCount);

		return tileCount;
	}

	private static void WalkableFloodFill(Point currentTile, Rectangle bounds, xTile.Layers.Layer back, xTile.Layers.Layer back2, xTile.Layers.Layer buildings, bool[,] alreadyChecked, ref int tileCount)
	{
		if (!bounds.Contains(currentTile))
			return;

		int x = currentTile.X;
		int y = currentTile.Y;

		if (alreadyChecked[x, y])
			return;

		alreadyChecked[x, y] = true;

		if ((back.Tiles[x, y] == null && back2?.Tiles[x, y] == null) || Game1.currentLocation.isWaterTile(x, y) || (buildings.Tiles[x, y] != null && !IsBuildingTilePassable(buildings.Tiles[x, y])))
			return;

		tileCount++;
		walkable[x, y] = true;
		WalkableFloodFill(new Point(x + 1, y), bounds, back, back2, buildings, alreadyChecked, ref tileCount);
		WalkableFloodFill(new Point(x - 1, y), bounds, back, back2, buildings, alreadyChecked, ref tileCount);
		WalkableFloodFill(new Point(x, y + 1), bounds, back, back2, buildings, alreadyChecked, ref tileCount);
		WalkableFloodFill(new Point(x, y - 1), bounds, back, back2, buildings, alreadyChecked, ref tileCount);
	}

	private static bool IsBuildingTilePassable(Tile tile)
	{
		tile.TileIndexProperties.TryGetValue("Passable", out xTile.ObjectModel.PropertyValue property);
		if (property == null)
		{
			tile.Properties.TryGetValue("Passable", out property);
		}

		return property != null;
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
						Game1.staminaRect,
						new Rectangle(Game1.GlobalToLocal(new Vector2(x, y) * 64f).ToPoint(), new Point(64, 64)),
						Game1.staminaRect.Bounds,
						Color.Red * 0.5f,
						0f,
						Vector2.Zero,
						Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
						0f
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
