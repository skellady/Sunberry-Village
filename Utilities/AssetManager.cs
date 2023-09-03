using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using SunberryVillage.Animations;
using SunberryVillage.Lighting;
using SunberryVillage.PortraitShake;
using SunberryVillage.TarotEvent;
using System.Collections.Generic;

namespace SunberryVillage.Utilities;

internal class AssetManager
{
	internal static void LoadOrEditAssets(object sender, AssetRequestedEventArgs e)
	{
		#region Tarot

		if (e.NameWithoutLocale.IsEquivalentTo($"{TarotHandler.TarotAssetPath}/CardBack"))
			e.LoadFromModFile<Texture2D>("Assets/Tarot/cardBack.png", AssetLoadPriority.Medium);
		else if (e.NameWithoutLocale.IsEquivalentTo($"{TarotHandler.TarotAssetPath}/Event"))
			e.LoadFrom(
				() => new Dictionary<string, string>
				{
					["Event"] = "none/-100 -100/farmer -100 -100 0/globalFadeToClear/skippable/pause 1000/cutscene DialaTarot/pause 1000/end"
				}, AssetLoadPriority.Low);
		else if (e.NameWithoutLocale.StartsWith($"{TarotHandler.TarotAssetPath}/Texture"))
		{
			string id = e.NameWithoutLocale.ToString()?.Replace($"{TarotHandler.TarotAssetPath}/Texture", "");
			e.LoadFromModFile<Texture2D>($"Assets/Tarot/{id}.png", AssetLoadPriority.Medium);
		}

		#endregion

		#region PortraitShake

		else if (e.NameWithoutLocale.IsEquivalentTo(PortraitShakeHandler.PortraitShakeAssetPath))
			e.LoadFrom(() => new Dictionary<string, PortraitShakeModel>(), AssetLoadPriority.Low);

		#endregion

		#region BigAnimations

		else if (e.NameWithoutLocale.IsEquivalentTo(AnimationsHandler.AnimationsAssetPath))
			e.LoadFrom(() => new Dictionary<string, AnimationDataModel>(), AssetLoadPriority.Low);

		#endregion

		#region Lighting

		else if (e.NameWithoutLocale.IsEquivalentTo(LightingHandler.LightsAssetPath))
			e.LoadFrom(() => new Dictionary<string, LightDataModel>
			{
				["sophie.SBVSaturdayHangoutLight"] = new LightDataModel(
					Location: "Custom_SBV_SunberryVillage",
					Position: new Vector2(59f, 85.5f),
					Intensity: 4.8f)
			}, AssetLoadPriority.Low);

		#endregion
	}

	internal static void ReloadAssets()
	{
		PortraitShakeHandler.PortraitsDict = Globals.GameContent.Load<Dictionary<string, PortraitShakeModel>>(PortraitShakeHandler.PortraitShakeAssetPath);
		AnimationsHandler.AnimationData = Globals.GameContent.Load<Dictionary<string, AnimationDataModel>>(AnimationsHandler.AnimationsAssetPath);
		LightingHandler.Lights = Globals.GameContent.Load<Dictionary<string, LightDataModel>>(LightingHandler.LightsAssetPath);
	}
}
