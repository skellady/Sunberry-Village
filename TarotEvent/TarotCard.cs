using System;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using SunberryVillage.Utilities;

namespace SunberryVillage.TarotEvent;

internal class TarotCard
{
	// Buff should last the entire Stardew day - its fine to overshoot, it gets removed nightly
	private const int BuffDurationMilliseconds = 60 * 60 * 1000;

	internal string Id;
	internal string Name;
	internal string Description;
	internal Lazy<Texture2D> Texture;
	internal Buff Buff;
	internal Func<bool> Condition;

	internal TarotCard(string id, Buff buff, Func<bool> condition)
	{
		Id = id;
		Name = Globals.TranslationHelper.Get($"{id}.name").UsePlaceholder(true);
		Description = Globals.TranslationHelper.Get($"{id}.desc").UsePlaceholder(true);
		Texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>($"SunberryTeam.SBV/Tarot/Texture/{id}"));
		Buff = buff;
		Condition = condition;
	}

	internal void ApplyBuff()
	{
		Buff.source = Id;
		Buff.displaySource = Name;
		Buff.millisecondsDuration = BuffDurationMilliseconds;
		Game1.buffsDisplay.addOtherBuff(Buff);
	}
}