using StardewValley;
using StardewValley.Menus;
using SunberryVillage.Animations;
using SunberryVillage.Lighting;
using SunberryVillage.Portrait;
using SunberryVillage.Audio;
using System;
using SunberryVillage.Tarot;

namespace SunberryVillage.Utilities;

internal class EventHookManager
{
	internal static void InitializeEventHooks()
	{
		TarotHandler.AddEventHooks();
		AnimationsHandler.AddEventHooks();
		PortraitHandler.AddEventHooks();
		AudioHandler.AddEventHooks();
		LightingHandler.AddEventHooks();
	}
}
