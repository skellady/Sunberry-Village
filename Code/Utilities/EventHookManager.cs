using SunberryVillage.Animations;
using SunberryVillage.Audio;
using SunberryVillage.Events;
using SunberryVillage.Events.Phone;
using SunberryVillage.Events.Tarot;
using SunberryVillage.Integration;
using SunberryVillage.Lighting;
using SunberryVillage.Maps;
using SunberryVillage.Menus;
using SunberryVillage.Queries;
using SunberryVillage.Shops;
using SunberryVillage.SpecialOrders;
using SunberryVillage.TextEmojis;

namespace SunberryVillage.Utilities;

internal class EventHookManager
{
	internal static void InitializeEventHooks()
	{
		IntegrationManager.AddEventHooks();
		PhoneManager.AddEventHooks();
		TarotManager.AddEventHooks();
		AnimationsManager.AddEventHooks();
		AudioManager.AddEventHooks();
		LightingManager.AddEventHooks();
		EmojiManager.AddEventHooks();
		MapManager.AddEventHooks();
		QueryManager.AddEventHooks();
		EventCommandManager.AddEventHooks();
		MarketDailySpecialManager.AddEventHooks();
		SunberrySpecialOrdersBoard.AddEventHooks();
		ImageMenuManager.AddEventHooks();
		PipPepShopNotification.AddEventHooks();
		StringPoolManager.AddEventHooks();
		MenuHooks.AddEventHooks();
	}
}
