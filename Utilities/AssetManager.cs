using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using SunberryVillage.PortraitShake;
using System.Collections.Generic;

namespace SunberryVillage.Utilities;

internal class AssetManager
{
	internal static void LoadOrEditAssets(object sender, AssetRequestedEventArgs e)
	{
		#region Tarot

		if (e.NameWithoutLocale.IsEquivalentTo("SunberryTeam.SBV/Tarot/CardBack"))
			e.LoadFromModFile<Texture2D>("Assets/Tarot/cardBack.png", AssetLoadPriority.Medium);
		else if (e.NameWithoutLocale.IsEquivalentTo("SunberryTeam.SBV/Tarot/Event"))
			e.LoadFrom(
				() => new Dictionary<string, string>
				{
					["Event"] = "none/-100 -100/farmer -100 -100 0/globalFadeToClear/skippable/pause 1000/cutscene DialaTarot/pause 1000/end"
				}, AssetLoadPriority.Low);
		else if (e.NameWithoutLocale.StartsWith("SunberryTeam.SBV/Tarot/Texture"))
		{
			{
				string id = e.NameWithoutLocale.ToString()?.Replace("SunberryTeam.SBV/Tarot/Texture", "");
				e.LoadFromModFile<Texture2D>($"Assets/Tarot/{id}.png", AssetLoadPriority.Medium);
			}
		}

		#endregion

		#region Portrait shake

		else if (e.NameWithoutLocale.IsEquivalentTo("SunberryTeam.SBV/PortraitsToShake"))
			e.LoadFrom(() => new Dictionary<string, PortraitShakeModel>(), AssetLoadPriority.Low);

		#endregion
	}
}
