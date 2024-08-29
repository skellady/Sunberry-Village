using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buffs;
using System;

namespace SunberryVillage.Events.Tarot;
internal class TemperanceBuff : Buff
{
	public TemperanceBuff(string id, string source = null, string displaySource = null, int duration = -1, Texture2D iconTexture = null, int iconSheetIndex = -1, BuffEffects effects = null, bool? isDebuff = false, string displayName = null, string description = null) : base(id, source, displaySource, duration, iconTexture, iconSheetIndex, effects, isDebuff, displayName, description)
	{ }

	public override void OnAdded()
	{
		Globals.EventHelper.GameLoop.UpdateTicked += CheckHealthAndStamina;
	}

	private void CheckHealthAndStamina(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
	{
		int health = Game1.player.health;
		int stam = (int)Game1.player.stamina;

		if (!e.IsMultipleOf(15))
			return;

		if (Math.Abs(health - stam) < 2)
			return;

		if (health < Game1.player.maxHealth && health < stam)
		{
			Game1.player.health++;
			Game1.player.stamina--;
		}
		else if (stam < Game1.player.maxStamina.Value && stam < health)
		{
			Game1.player.stamina++;
			Game1.player.health--;
		}
	}

	public override void OnRemoved()
	{
		Globals.EventHelper.GameLoop.UpdateTicked -= CheckHealthAndStamina;
	}
}
