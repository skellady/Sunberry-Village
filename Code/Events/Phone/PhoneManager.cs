using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using SunberryVillage.Utilities;

namespace SunberryVillage.Events.Phone;

internal static class PhoneManager
{
	internal const string PhoneAssetPath = "SunberryTeam.SBV/Phone";
	
	public static void AddEventHooks()
	{
		Globals.EventHelper.Content.AssetRequested += Phone_AssetRequested;
	}
	
	private static void Phone_AssetRequested(object sender, AssetRequestedEventArgs e)
	{
		if (!e.NameWithoutLocale.StartsWith(PhoneAssetPath))
			return;

		if (e.NameWithoutLocale.IsEquivalentTo(PhoneAssetPath))
		{
			e.LoadFromModFile<Texture2D>("Assets/Phone/blank.png", AssetLoadPriority.Medium);
		}
		else if (e.NameWithoutLocale.IsEquivalentTo($"{PhoneAssetPath}/MessageBox"))
		{
			e.LoadFromModFile<Texture2D>("Assets/Phone/messagebox.png", AssetLoadPriority.Medium);
		}
		else if (e.NameWithoutLocale.IsEquivalentTo($"{PhoneAssetPath}/Overlay"))
		{
			e.LoadFromModFile<Texture2D>("Assets/Phone/overlay.png", AssetLoadPriority.Medium);
		}
	}
	
}