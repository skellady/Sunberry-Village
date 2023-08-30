using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewValley;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;

namespace SunberryVillage.PositionalAudio
{
	internal static class PositionalAudioHandler
	{
		internal static NPC Elias;
		internal static readonly Vector2 EliasGuitarTilePos = new(52f, 84f);
		internal static bool IsMusicPlaying = false;
		internal static ICue CurrentSong;
		internal static List<ICue> EliasSongs;
		internal static Vector2 PreviousPlayerTilePos = new(0f, 0f);
		internal static bool SongIntroed = false;
		internal static bool FadingInMusic = false;
		internal static bool FadingOutMusic = false;

		// pulled this out in case i want to do some fade-in/fade-out stuff, but... i dont think i do
		// i lied i ended up doing some fade in/out shit
		private static float VolumeMod = 1f;
		private static float AmbientMod = 1f;
		private static float FadeInMod = 0f;
		private static float FadeOutMod = 0f;
		private const float FadeFactor = 0.01f;

		// Formula stuff
		private const float MaxVolumeCutoffDistance = 15f;
		private const float LogDivider = 15f;
		private const float FalloffFactor = 130f;

		/// <summary>
		/// Initialize the needed ICues and grab a reference to Elias if possible (will be done later if not).
		/// </summary>
		internal static void Init()
		{
			if (Game1.soundBank != null)
			{
				EliasSongs = new()
				{
					Game1.soundBank.GetCue("kindadumbautumn"),
					Game1.soundBank.GetCue("poppy"),
					Game1.soundBank.GetCue("SettlingIn")
				};
			}

			Elias = Game1.getCharacterFromName("EliasSBV");
		}

		/// <summary>
		/// Selects one of the songs to play at random.
		/// </summary>
		internal static void ChooseMusic()
		{
			CurrentSong = EliasSongs.GetRandomElementFromList();
		}

		/// <summary>
		/// Stops any other music currently playing and starts Elias's song.
		/// </summary>
		internal static void StartMusic()
		{
			if (CurrentSong is null)
				ChooseMusic();

			// small delay in case other music tries to start, then kill it
			DelayedAction.functionAfterDelay(() => Game1.stopMusicTrack(Game1.MusicContext.Default), 50);

			CurrentSong.Play();
			IsMusicPlaying = true;

			// only introduce song once, when it first plays for the night
			// you'll basically only ever see this if you're at the hangout when elias starts playing
			// also only fade in song if he's just started playing
			if (!SongIntroed)
			{
				FadeInMod = 0f;
				FadingInMusic = true;
				AddSongIntroSpeechBubble();
			}

		}

		/// <summary>
		/// Creates a speech bubble over Elias's head to introduce the song he's playing.
		/// </summary>
		private static void AddSongIntroSpeechBubble()
		{
			SongIntroed = true;

			string speechBubble1 = Globals.TranslationHelper.Get("SongIntro").UsePlaceholder(true);
			string speechBubble2 = Globals.TranslationHelper.Get($"{CurrentSong.Name}.name").UsePlaceholder(true) +
				Globals.TranslationHelper.Get("SongCredits").UsePlaceholder(true);

			// break up into two separate bubbles since they don't support newlines
			Elias.showTextAboveHead(speechBubble1, duration: 5000);
			DelayedAction.functionAfterDelay(() => Elias.showTextAboveHead(speechBubble2, duration: 5000), 5750);
		}
		
		/// <summary>
		/// Creates a speech bubble over Elias's head to thank the audience for listening after he's done playing.
		/// </summary>
		private static void AddSongOutroSpeechBubble()
		{
			string speechBubble = Globals.TranslationHelper.Get("SongOutro").UsePlaceholder(true);
			Elias.showTextAboveHead(speechBubble, duration: 5000);
		}

		/// <summary>
		/// Halts Elias's song immediately.
		/// </summary>
		internal static void StopMusic()
		{
			CurrentSong.Stop(AudioStopOptions.Immediate);
			IsMusicPlaying = false;
			FadingInMusic = false;
		}

		/// <summary>
		/// Substitutes the volume formula for a simple one which decreases over time to make the music fade away.
		/// A separate method is needed because once the music is asked to stop, UpdateMusicVolume is no longer called.
		/// </summary>
		/// <returns><c>True</c> to indicate that the music has finished fading out, or <c>False</c> to indicate otherwise.</returns>
		internal static bool FadeOut()
		{
			if (!FadingOutMusic)
			{
				FadeOutMod = 1f;
				FadingOutMusic = true;
				AddSongOutroSpeechBubble();
			}

			if (FadeOutMod <= 0f)
			{
				FadeOutMod = 0f;
				FadingOutMusic = false;
				return true;
			}

			else
			{
				FadeOutMod -= FadeFactor;
				CurrentSong.Volume = Game1.options.musicVolumeLevel * VolumeMod * FadeOutMod;
				return false;
			}
		}

		/// <summary>
		/// Updates the volume of the music based on the player's proximity to Elias.
		/// </summary>
		internal static void UpdateMusicVolume()
		{
			// player hasn't moved and not fading in - no need to update volume
			// even if we take the early out, we need to dampen the ambient sound - see below for details
			Vector2 currentTilePos = Game1.player.getTileLocation();
			if (PreviousPlayerTilePos == currentTilePos && !FadingInMusic)
			{
				Game1.ambientCategory.SetVolume(Game1.options.ambientVolumeLevel * AmbientMod);
				return;
			}

			float DistanceToElias = Vector2.Distance(currentTilePos, EliasGuitarTilePos);

			// if within MaxVolumeCutoffDistance tiles of Elias, can skip formula and set volume to max.
			if (DistanceToElias < MaxVolumeCutoffDistance)
			{
				VolumeMod = 1f;
			}
			// if further than MaxVolumeCutoffDistance tiles of Elias, use formula to determine volume percentage.
			else
			{
				// formula i'm using: y = 100 - FalloffFactor * log10(x / LogDivider)
				// gets value from 100 to 0 - divide by 100 to get VolumeMod
				// using some inverse square law shit: https://www.wkcgroup.com/tools-room/inverse-square-law-sound-calculator/

				VolumeMod = (float)(100 - FalloffFactor * Math.Log10(DistanceToElias / LogDivider)) / 100f;
				VolumeMod = Math.Clamp(VolumeMod, 0f, 1f);
			}

			// handle fade in
			if (FadingInMusic)
			{
				FadeInMod += FadeFactor;
				if (FadeInMod >= 1f)
				{
					FadeInMod = 1f;
					FadingInMusic = false;
				}
			}

			// dampen the ambient sound by an amount inversely proportional to the music volume percentage
			// ambient sound volume is set every tick so we need to lower it every tick
			AmbientMod = Math.Clamp(1f - VolumeMod, 0.1f, 1f);
			Game1.ambientCategory.SetVolume(Game1.options.ambientVolumeLevel * AmbientMod);

			CurrentSong.Volume = Game1.options.musicVolumeLevel * VolumeMod * FadeInMod;
			PreviousPlayerTilePos = currentTilePos;
		}

		/// <summary>
		/// Checks whether the conditions are met for the song to be played.
		/// </summary>
		/// <returns><c>True</c> if Elias is in position and doing his guitar animation.</returns>
		internal static bool ShouldMusicPlay()
		{
			if (Elias is null)
				Elias = Game1.getCharacterFromName("EliasSBV");

			if (Elias is null || Elias.currentLocation != Game1.currentLocation)
				return false;

			return (Vector2.Distance(Elias.getTileLocation(), EliasGuitarTilePos) < 1f) && Elias.doingEndOfRouteAnimation.Value;
		}

		/// <summary>
		/// Restarts the non-looping songs if they've ended.
		/// </summary>
		internal static void RestartSongIfNecessary()
		{
			// SettlingIn loops so no need to manually Play it. the others don't loop
			if (CurrentSong is ICue cue && cue.Name != "SettlingIn" && CurrentSong.IsStopped)
				CurrentSong.Play();
		}
	}
}
