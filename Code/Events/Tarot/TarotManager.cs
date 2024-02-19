using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buffs;
using SunberryVillage.Utilities;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable UnusedMember.Global

namespace SunberryVillage.Tarot;

internal static class TarotManager
{
	internal const string TarotAssetPath = "SunberryTeam.SBV/Tarot";
	internal const string TarotRequiredEventId ="skellady.SBVCP_20031411";
	internal static Texture2D TarotBuffIcons = Game1.content.Load<Texture2D>($"SunberryTeam.SBV/Tarot/BuffIcons");

	#region Card Pool

	public static List<TarotCard> CardPool = new()
	{
		new TarotCard(
			id: "Sun",
			buffId: "21",
			condition: () => Game1.player.isMarriedOrRoommates()),
		new TarotCard(
			id: "ThreeOfWands",
			buffEffects: new() { MiningLevel = { 3 } },
			condition: null),
		new TarotCard(
			id: "AceOfCups",
			buffEffects: new() { MagneticRadius = { 75 } },
			condition: null),
		new TarotCard(
			id: "Empress",
			buffEffects : new() { Speed = { 1 } },
			condition: null),
		new TarotCard(
			id: "Moon",
			buffEffects: new() { Speed = { -1 } },
			condition: null),
		new TarotCard(
			id: "Lovers",
			buffEffects: new() { MaxStamina = { 50 } },
			condition: () => Game1.player.isMarriedOrRoommates() || Game1.player.isEngaged()),
		new TarotCard(
			id: "TowerReversed",
			buffId: "26",
			condition: null),
		new TarotCard(
			id: "AceOfPentacles",
			buffEffects: new() { ForagingLevel = { 3 } },
			condition: null),
		new TarotCard(
			id: "ThreeOfSwordsReversed",
			buffEffects: new() { Immunity = { 3 } },
			condition: null),
		new TarotCard(
			id: "WheelOfFortune",
			buffEffects: new() { LuckLevel = { 3 } },
			condition: null)
	};

	#endregion

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
					["Event"] = "none/-100 -100/farmer -100 -100 0/globalFadeToClear/skippable/pause 1000/SBVTarotCutscene/Cutscene SBVTarotCutscene/pause 1000/end"
				}, AssetLoadPriority.Low);
		}
		else if (e.NameWithoutLocale.StartsWith($"{TarotAssetPath}/Texture"))
		{
			string id = e.NameWithoutLocale.ToString()?.Replace($"{TarotAssetPath}/Texture/", "");
			e.LoadFromModFile<Texture2D>($"Assets/Tarot/{id}.png", AssetLoadPriority.Medium);
		}
		else if (e.NameWithoutLocale.StartsWith($"{TarotAssetPath}/BuffIcons"))
		{
			e.LoadFromModFile<Texture2D>($"Assets/Tarot/Buffs.png", AssetLoadPriority.Medium);
		}
	}

	#endregion
}