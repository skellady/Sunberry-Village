using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using SunberryVillage.PortraitShake;
using System.Collections.Generic;
using System;
using SunberryVillage.Animations;
using SunberryVillage.Patching;

namespace SunberryVillage.Utilities;

internal class EventHookManager
{
	/// <summary>
	/// Initializes all event hooks which are applied without conditions. Event hooks with no conditional application should be added here.
	/// </summary>
	internal static void InitializeEventHooks()
	{
		// General
		Globals.EventHelper.Content.AssetRequested += AssetManager.LoadOrEditAssets;
		Globals.EventHelper.GameLoop.DayStarted += ReloadCachedAssets;

		// Tarot
		Globals.EventHelper.GameLoop.DayEnding += ClearTarotFlag;

		// Big animations
		Globals.EventHelper.GameLoop.DayEnding += AnimationsHandler.DayEnd;

		// Portrait shake
		Globals.EventHelper.Display.MenuChanged += CheckForPortraitShake;
	}

	/// <summary>
	/// Reloads any cached assets at the start of each day if they have been modified.
	/// </summary>
	private static void ReloadCachedAssets(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
    {
		AssetManager.ReloadAssets();
    }
	
	/// <summary>
	/// Clears the mod data flag which prevents getting multiple tarot readings in one day.
	/// </summary>
	private static void ClearTarotFlag(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
	{
		Game1.player.modData.Remove("SunberryTeam.SBV/Tarot/ReadingDoneForToday");
	}

	/// <summary>
	/// When a DialogueBox menu opens, checks to see if the portrait should shake. 
	/// </summary>
	private static void CheckForPortraitShake(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
	{
		if (e.NewMenu is DialogueBox {characterDialogue: { }} dialogue)
			PortraitShakeHandler.SetShake(dialogue.characterDialogue);
	}
}
