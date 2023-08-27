using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using SunberryVillage.Animations;
using SunberryVillage.PortraitShake;
using SunberryVillage.TarotEvent;
using System.Collections.Generic;

namespace SunberryVillage.Utilities;

internal class AssetManager
{
	internal static void LoadOrEditAssets(object sender, AssetRequestedEventArgs e)
	{
		#region Tarot

		if (e.NameWithoutLocale.IsEquivalentTo($"{TarotCardHandler.TarotAssetPath}/CardBack"))
			e.LoadFromModFile<Texture2D>("Assets/Tarot/cardBack.png", AssetLoadPriority.Medium);
		else if (e.NameWithoutLocale.IsEquivalentTo($"{TarotCardHandler.TarotAssetPath}/Event"))
			e.LoadFrom(
				() => new Dictionary<string, string>
				{
					["Event"] = "none/-100 -100/farmer -100 -100 0/globalFadeToClear/skippable/pause 1000/cutscene DialaTarot/pause 1000/end"
				}, AssetLoadPriority.Low);
		else if (e.NameWithoutLocale.StartsWith($"{TarotCardHandler.TarotAssetPath}/Texture"))
		{
			string id = e.NameWithoutLocale.ToString()?.Replace($"{TarotCardHandler.TarotAssetPath}/Texture", "");
			e.LoadFromModFile<Texture2D>($"Assets/Tarot/{id}.png", AssetLoadPriority.Medium);
		}

		#endregion

		#region PortraitShake

		else if (e.NameWithoutLocale.IsEquivalentTo(PortraitShakeHandler.PortraitShakeAssetPath))
			e.LoadFrom(() => new Dictionary<string, PortraitShakeModel>(), AssetLoadPriority.Low);

		#endregion

		#region BigAnimations

		else if (e.Name.IsEquivalentTo(AnimationsHandler.AnimationsAssetPath))
			e.LoadFrom(() => new Dictionary<string, AnimationsHandler.AnimationDataModel>(), AssetLoadPriority.Low);

		#endregion
	}

	internal static void ReloadAssets()
	{
		PortraitShakeHandler.PortraitsDict = Globals.GameContent.Load<Dictionary<string, PortraitShakeModel>>(PortraitShakeHandler.PortraitShakeAssetPath);
		AnimationsHandler.AnimationData = Globals.GameContent.Load<Dictionary<string, AnimationsHandler.AnimationDataModel>>(AnimationsHandler.AnimationsAssetPath);
	}
}
