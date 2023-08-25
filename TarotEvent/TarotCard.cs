using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using SunberryVillage.Utilities;

namespace SunberryVillage.TarotEvent
{
	internal class TarotCard
	{
		// Buff should last the entire Stardew day - its fine to overshoot, it gets removed nightly
		private const int BUFF_DURATION_MILLISECONDS = 60 * 60 * 1000;

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
			Description = Globals.TranslationHelper.Get("${id}.desc").UsePlaceholder(true);
			Texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>($"SunberryTeam.SBV/Tarot/{id}.texture"));
			Buff = buff;
			Condition = condition;
		}

		internal void ApplyBuff()
		{
			Buff.source = Id;
			Buff.displaySource = Name;
			Buff.millisecondsDuration = BUFF_DURATION_MILLISECONDS;
			Game1.buffsDisplay.addOtherBuff(Buff);
		}
	}
}
