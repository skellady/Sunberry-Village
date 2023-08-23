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

	private static void CheckForPortraitShake(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
	{
		if (e.NewMenu is DialogueBox dialogue)
			PortraitShakeHandler.SetShake(dialogue.characterDialogue);
	}

	private static void ReloadCachedAssets(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
	{
		PortraitShakeHandler.ReloadAsset();
	}

	private static void ClearTarotFlag(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
	{
		Game1.player.modData.Remove("sophie.DialaTarot/ReadingDoneForToday");
	}
}
