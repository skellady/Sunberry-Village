using StardewValley;
using StardewValley.Menus;
using SunberryVillage.PortraitShake;

namespace SunberryVillage.Utilities;

internal class EventHookManager
{
	internal static void InitializeEventHooks()
	{
		// General
		Globals.EventHelper.Content.AssetRequested += AssetManager.LoadOrEditAssets;
		Globals.EventHelper.GameLoop.DayStarted += ReloadCachedAssets;

		// Tarot
		Globals.EventHelper.GameLoop.DayEnding += ClearTarotFlag;

		// Portrait shake
		Globals.EventHelper.Display.MenuChanged += CheckForPortraitShake;
	}

	/// <summary>
	/// Reloads any cached assets at the start of each day if they have been modified.
	/// </summary>
	private static void ReloadCachedAssets(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
	{
		PortraitShakeHandler.ReloadAsset();
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
		if (e.NewMenu is DialogueBox dialogue && dialogue.characterDialogue is not null)
			PortraitShakeHandler.SetShake(dialogue.characterDialogue);
	}
}
