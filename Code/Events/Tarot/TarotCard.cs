using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buffs;
using System;

namespace SunberryVillage.Events.Tarot;

internal class TarotCard
{
	internal string Id;
	internal string Name;
	internal string Description;
	internal int IconIndex;
	internal Lazy<Texture2D> Texture;
	internal string BuffId;
	internal BuffEffects BuffEffects;
	internal bool IsDebuff;
	internal Func<bool> Condition;

	internal TarotCard(string id, Func<bool> condition, string buffId = null, int iconIndex = 0, BuffEffects buffEffects = null, bool isDebuff = false)
	{
		Id = id;
		Name = Utils.GetTranslationWithPlaceholder($"{id}.name");
		Description = Utils.GetTranslationWithPlaceholder($"{id}.desc");
		IconIndex = iconIndex;
		Texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>($"SunberryTeam.SBV/Tarot/Texture/{id}"));
		BuffId = buffId;
		BuffEffects = buffEffects;
		IsDebuff = isDebuff;
		Condition = condition;
	}

	internal void ApplyBuff(string id)
	{
		if (Id.Equals("Temperance"))
		{
			TemperanceBuff temperanceBuff = new(
				id: Id,
				source: id,
				displaySource: Utils.GetTranslationWithPlaceholder($"TarotBuffDisplaySource"),
				duration: Buff.ENDLESS,
				iconTexture: TarotManager.TarotBuffIcons,
				iconSheetIndex: IconIndex,
				effects: BuffEffects,
				isDebuff: IsDebuff,
				displayName: Name,
				description: null
			);

			Game1.player.applyBuff(temperanceBuff);
		}
		else
		{
			Buff buff = new(
				id: BuffId ?? Id,
				source: id,
				displaySource: Utils.GetTranslationWithPlaceholder($"TarotBuffDisplaySource"),
				duration: Buff.ENDLESS,
				iconTexture: TarotManager.TarotBuffIcons,
				iconSheetIndex: IconIndex,
				effects: BuffEffects,
				isDebuff: IsDebuff,
				displayName: Name,
				description: null
			);
			Game1.player.applyBuff(buff);
		}
	}
}