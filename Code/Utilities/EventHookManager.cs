using SunberryVillage.Animations;
using SunberryVillage.Audio;
using SunberryVillage.Events;
using SunberryVillage.Integration;
using SunberryVillage.Lighting;
using SunberryVillage.Maps;
using SunberryVillage.Portrait;
using SunberryVillage.Shops;
using SunberryVillage.Tarot;
using SunberryVillage.TextEmojis;

namespace SunberryVillage.Utilities;

internal class EventHookManager
{
	internal static void InitializeEventHooks()
	{
		IntegrationManager.AddEventHooks();
		TarotManager.AddEventHooks();
		AnimationsManager.AddEventHooks();
		PortraitManager.AddEventHooks();
		AudioManager.AddEventHooks();
		LightingManager.AddEventHooks();
		EmojiManager.AddEventHooks();
		ActionManager.AddEventHooks();
		EventCommandManager.AddEventHooks();
		MarketDailySpecialManager.AddEventHooks();
	}
}
