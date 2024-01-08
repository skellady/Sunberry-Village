using SunberryVillage.Animations;
using SunberryVillage.Audio;
using SunberryVillage.Integration;
using SunberryVillage.Lighting;
using SunberryVillage.Maps;
using SunberryVillage.Portrait;
using SunberryVillage.Tarot;
using SunberryVillage.TextEmojis;

namespace SunberryVillage.Utilities;

internal class EventHookManager
{
	internal static void InitializeEventHooks()
	{
		IntegrationHandler.AddEventHooks();
		TarotHandler.AddEventHooks();
		AnimationsHandler.AddEventHooks();
		PortraitHandler.AddEventHooks();
		AudioHandler.AddEventHooks();
		LightingHandler.AddEventHooks();
		EmojiHandler.AddEventHooks();
		ActionManager.AddEventHooks();
	}
}
