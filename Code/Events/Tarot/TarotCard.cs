using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buffs;
using SunberryVillage.Utilities;
using System;

namespace SunberryVillage.Tarot;

internal class TarotCard
{
	// hardcoded magic number to hide duration from buff hover text
	private const int AllDayDuration = -2;

	internal string Id;
	internal string Name;
	internal string Description;
	internal Lazy<Texture2D> Texture;
	internal string BuffId;
	internal BuffEffects BuffEffects;
	internal bool IsDebuff;
	internal Func<bool> Condition;

	internal TarotCard(string id, Func<bool> condition, string buffId = null, BuffEffects buffEffects = null, bool isDebuff = false)
	{
		Id = id;
		Name = Utils.GetTranslationWithPlaceholder($"{id}.name");
		Description = Utils.GetTranslationWithPlaceholder($"{id}.desc");
		Texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>($"SunberryTeam.SBV/Tarot/Texture/{id}"));
		BuffId = buffId;
		BuffEffects = buffEffects;
		IsDebuff = isDebuff;
		Condition = condition;
	}

	internal void ApplyBuff(string id)
	{
		Buff buff = new(
			id: BuffId ?? Id,
			source: id,
			displaySource: Utils.GetTranslationWithPlaceholder($"TarotBuffDisplaySource"),
			duration: AllDayDuration,
			iconTexture: TarotManager.TarotBuffIcons,
			iconSheetIndex: 0,
			effects: BuffEffects,
			isDebuff: IsDebuff,
			displayName: Name,
			description: null
		);
		Game1.player.applyBuff(buff);
	}
}