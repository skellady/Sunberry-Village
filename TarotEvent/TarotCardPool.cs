using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace SunberryVillage.TarotEvent
{
	internal class TarotCardPool
	{
		public static List<TarotCard> Cards = new()
		{
			new TarotCard(
				id: "Sun",
				buff: new(21),
				condition: () => (bool)(Game1.player?.isMarried())),
			new TarotCard(
				id: "ThreeOfWands",
				buff: new(0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1200, "", ""),
				condition: null),
			new TarotCard(
				id: "AceOfCups",
				buff: new(0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1200, "", ""),
				condition: null),
			new TarotCard(
				id: "Empress",
				buff: new(0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 1200, "", ""),
				condition: null),
			new TarotCard(
				id: "Moon",
				buff: new(17),
				condition: null),
			new TarotCard(
				id: "Lovers",
				buff: new(0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 0, 0, 1200, "", ""),
				condition: () => (bool)(Game1.player?.isMarried()) || (bool)(Game1.player?.isEngaged())),
			new TarotCard(
				id : "TowerReversed",
				buff : new(26),
				condition : null),
			new TarotCard(
				id : "AceOfPentacles",
				buff : new(0, 0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 1200, "", ""),
				condition : null),
			new TarotCard(
				id : "ThreeOfSwordsReversed",
				buff : new(0, 0, 0, 0, 0, 0, 0, 50, 0, 0, 0, 0, 1200, "", ""),
				condition : null),
			new TarotCard(
				id : "WheelOfFortune",
				buff : new(0, 0, 0, 0, 3, 0, 0, 0, 0, 0, 0, 0, 1200, "", ""),
				condition : null)
		};

		// todo: weighted probability?

		public static List<TarotCard> GetAllTarotCards()
		{
			return Cards.ToList();
		}

		public static List<TarotCard> GetAllTarotCardsWithConditionsMet()
		{
			return Cards.Where((card) => card.Condition is null || card.Condition()).ToList();
		}

		public static TarotCard GetTarotCardById(string id)
		{
			return Cards.FirstOrDefault((card) => card.Id == id);
		}
	}
}
