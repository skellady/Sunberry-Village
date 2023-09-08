using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SunberryVillage.Utilities;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace SunberryVillage.Tarot;

internal class TarotHandler
{
	internal const string TarotAssetPath = "SunberryTeam.SBV/Tarot";
	internal const int TarotRequiredEventId = 20031411;

	public static List<TarotCard> CardPool = new()
	{
		new TarotCard(
			id: "Sun",
			buff: new Buff(21),
			condition: () => Game1.player.isMarried()),
		new TarotCard(
			id: "ThreeOfWands",
			buff: new Buff(0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1200, "", ""),
			condition: null),
		new TarotCard(
			id: "AceOfCups",
			buff: new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1200, "", ""),
			condition: null),
		new TarotCard(
			id: "Empress",
			buff: new Buff(0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 1200, "", ""),
			condition: null),
		new TarotCard(
			id: "Moon",
			buff: new Buff(17),
			condition: null),
		new TarotCard(
			id: "Lovers",
			buff: new Buff(0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 0, 0, 1200, "", ""),
			condition: () => Game1.player.isMarried() || Game1.player.isEngaged()),
		new TarotCard(
			id : "TowerReversed",
			buff : new Buff(26),
			condition : null),
		new TarotCard(
			id : "AceOfPentacles",
			buff : new Buff(0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 1200, "", ""),
			condition : null),
		new TarotCard(
			id : "ThreeOfSwordsReversed",
			buff : new Buff(0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 0, 0, 1200, "", ""),
			condition : null),
		new TarotCard(
			id : "WheelOfFortune",
			buff : new Buff(0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 1200, "", ""),
			condition : null)
	};

	#region Logic

	public static List<TarotCard> GetAllTarotCards()
	{
		return CardPool.ToList();
	}

	public static List<TarotCard> GetAllTarotCardsWithConditionsMet()
	{
		return CardPool.Where(card => card.Condition is null || card.Condition()).ToList();
	}

	public static TarotCard GetTarotCardById(string id)
	{
		return CardPool.FirstOrDefault(card => card.Id == id);
	}

	#endregion

	#region Event Hooks

	/// <summary>
	/// Adds Tarot event hooks.
	/// </summary>
	public static void AddEventHooks()
	{
		Globals.EventHelper.Content.AssetRequested += Tarot_AssetRequested;
		// AssetInvalidated hook isn't necessary because nothing is cached, it's all loaded on demand
		// Init hook also isn't necessary for the same reason
		// At the end of each day, clears the mod data flag which prevents getting multiple tarot readings in one day.
		Globals.EventHelper.GameLoop.DayEnding += (_, _) => Game1.player.modData.Remove("SunberryTeam.SBV/Tarot/ReadingDoneForToday");
	}

	/// <summary>
	/// Provides default assets.
	/// </summary>
	private static void Tarot_AssetRequested(object sender, AssetRequestedEventArgs e)
	{
		if (!e.NameWithoutLocale.StartsWith(TarotAssetPath))
			return;

		if (e.NameWithoutLocale.IsEquivalentTo($"{TarotAssetPath}/CardBack"))
		{
			e.LoadFromModFile<Texture2D>("Assets/Tarot/cardBack.png", AssetLoadPriority.Medium);
		}
		else if (e.NameWithoutLocale.IsEquivalentTo($"{TarotAssetPath}/Event"))
		{
			e.LoadFrom(
				() => new Dictionary<string, string>
				{
					["Event"] = "none/-100 -100/farmer -100 -100 0/globalFadeToClear/skippable/pause 1000/cutscene DialaTarot/pause 1000/end"
				}, AssetLoadPriority.Low);
		}
		else if (e.NameWithoutLocale.StartsWith($"{TarotAssetPath}/Texture"))
		{
			string id = e.NameWithoutLocale.ToString()?.Replace($"{TarotAssetPath}/Texture", "");
			e.LoadFromModFile<Texture2D>($"Assets/Tarot/{id}.png", AssetLoadPriority.Medium);
		}
	}

	#endregion
}