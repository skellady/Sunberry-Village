using StardewValley;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace SunberryVillage.TarotEvent;

internal class TarotCardPool
{
	public static List<TarotCard> Cards = new()
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

	// todo: weighted probability?

	public static List<TarotCard> GetAllTarotCards()
	{
		return Cards.ToList();
	}

	public static List<TarotCard> GetAllTarotCardsWithConditionsMet()
	{
		return Cards.Where(card => card.Condition is null || card.Condition()).ToList();
	}

	public static TarotCard GetTarotCardById(string id)
	{
		return Cards.FirstOrDefault(card => card.Id == id);
	}
}