using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using SunberryVillage.PortraitShake;
using SunberryVillage.TarotEvent;
using System.Collections.Generic;

namespace SunberryVillage.Utilities;

internal class AssetManager
{
	internal static void LoadOrEditAssets(object sender, AssetRequestedEventArgs e)
	{
		#region Tarot

		if (e.NameWithoutLocale.IsEquivalentTo("SunberryTeam.SBV/Tarot/CardBack"))
			e.LoadFromModFile<Texture2D>("Assets/cardBack.png", AssetLoadPriority.Medium);
		else if (e.NameWithoutLocale.IsEquivalentTo("SunberryTeam.SBV/Tarot/Event"))
			e.LoadFrom(
				() => new Dictionary<string, string>()
				{
					["Event"] = "none/-100 -100/farmer -100 -100 0/globalFadeToClear/skippable/pause 1000/cutscene DialaTarot/pause 1000/end"
				}, AssetLoadPriority.Medium);

		for (int i = 1; i <= TarotCard.Names.Count; i++)
		{
			if (e.NameWithoutLocale.IsEquivalentTo($"SunberryTeam.SBV/Tarot/Card{i}"))
				e.LoadFromModFile<Texture2D>($"Assets/{TarotCard.Names[i]}.png", AssetLoadPriority.Medium);
		}

		#endregion

		#region Portrait shake

		if (e.NameWithoutLocale.IsEquivalentTo("SunberryTeam.SBV/PortraitsToShake"))
			e.LoadFrom(() => new Dictionary<string, PortraitShakeModel>(), AssetLoadPriority.Low);

		#endregion
	}
}
