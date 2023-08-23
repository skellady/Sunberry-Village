using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using SunberryVillage.TarotEvent;

namespace SunberryVillage.Utilities;

internal class AssetManager
{
	internal static void LoadOrEditAssets(object sender, AssetRequestedEventArgs e)
	{
		// Load stuff for tarot event
		if (e.NameWithoutLocale.IsEquivalentTo("sophie.DialaTarot/CardBack"))
			e.LoadFromModFile<Texture2D>("Assets/cardBack.png", AssetLoadPriority.Medium);
		else if (e.NameWithoutLocale.IsEquivalentTo("sophie.DialaTarot/Event"))
			e.LoadFrom(
				() => new Dictionary<string, string>()
				{
					["Event"] = "none/-100 -100/farmer -100 -100 0/globalFadeToClear/skippable/pause 1000/cutscene DialaTarot/pause 1000/end"
				}, AssetLoadPriority.Medium);

		for (int i = 1; i <= TarotCard.Names.Count; i++)
		{
			if (e.NameWithoutLocale.IsEquivalentTo($"sophie.DialaTarot/Card{i}"))
				e.LoadFromModFile<Texture2D>($"Assets/{TarotCard.Names[i]}.png", AssetLoadPriority.Medium);
		}

		// Load portrait shake asset
		if (e.NameWithoutLocale.IsEquivalentTo("SBV.PortraitsToShake"))
			e.LoadFrom(() => new Dictionary<string, List<int>>(), AssetLoadPriority.Low);
	}
}
