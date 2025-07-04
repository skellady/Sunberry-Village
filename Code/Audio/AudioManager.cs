using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData;
using System;
using System.Collections.Generic;

namespace SunberryVillage.Audio
{
	internal static class AudioManager
	{
		// turning this into a property so i can use fancy getter logic
		internal static ICue CurrentSong
		{
			get { return _currentSong ??= EliasSongs.GetRandomElementFromList(); }
			set => _currentSong = value;
		}
		private static ICue _currentSong;

		internal static NPC Elias;
		internal static readonly Vector2 EliasGuitarTilePos = new(52f, 84f);
		internal static bool IsMusicPlaying = false;

		internal static List<ICue> EliasSongs;
		internal static Vector2 PreviousPlayerTilePos = new(0f, 0f);
		internal static bool SongIntroed = false;
		internal static bool FadingInMusic = false;
		internal static bool FadingOutMusic = false;

		private static float VolumeMod = 1f;
		private static float AmbientMod = 1f;
		private static float FadeInMod = 0f;
		private static float FadeOutMod = 0f;


		private const float FadeFactor = 0.01f;             // used for fadein/fadeout
		private const float MaxVolumeCutoffDistance = 15f;  // used for formula to determine volume
		private const float LogDivider = 15f;               // " "
		private const float FalloffFactor = 130f;           // " "

		#region Logic

		/// <summary>
		/// Initialize the needed ICues and grab a reference to Elias if possible (will be done later if not).
		/// </summary>
		internal static void Init()
		{
			if (Game1.soundBank != null)
			{
				EliasSongs =
				[
					Game1.soundBank.GetCue("kindadumbautumn"),
					Game1.soundBank.GetCue("poppy"),
					Game1.soundBank.GetCue("SettlingIn"),
				];
			}

			Elias = Game1.getCharacterFromName("EliasSBV");
		}

		/// <summary>
		/// Stops any other music currently playing and starts Elias's song.
		/// </summary>
		internal static void StartMusic()
		{
            // small delay in case other music tries to start, then kill it
            //DelayedAction.functionAfterDelay(() => Game1.stopMusicTrack(Game1.MusicContext.Default), 50);

            //Game1.stopMusicTrack(MusicContext.Default);
            Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.SubLocation);

            CurrentSong.Play();
			IsMusicPlaying = true;

			// only introduce song once, when it first plays for the night
			// you'll basically only ever see this if you're at the hangout when elias starts playing
			// also only fade in song if he's just started playing
			if (SongIntroed)
				return;

			FadeInMod = 0f;
			FadingInMusic = true;
			AddSongIntroSpeechBubble();
		}

		/// <summary>
		/// Creates a speech bubble over Elias's head to introduce the song he's playing.
		/// </summary>
		private static void AddSongIntroSpeechBubble()
		{
			SongIntroed = true;

			string speechBubble1 = Utils.GetTranslationWithPlaceholder("SongIntro");
			string speechBubble2 = Utils.GetTranslationWithPlaceholder($"{CurrentSong.Name}.name") +
				Utils.GetTranslationWithPlaceholder($"{CurrentSong.Name}.SongCredits");

			// break up into two separate bubbles since they don't support newlines
			Elias.showTextAboveHead(speechBubble1, duration: 5000);
			DelayedAction.functionAfterDelay(() => Elias.showTextAboveHead(speechBubble2, duration: 5000), 5750);
		}

		/// <summary>
		/// Creates a speech bubble over Elias's head to thank the audience for listening after he's done playing.
		/// </summary>
		private static void AddSongOutroSpeechBubble()
		{
			string speechBubble = Utils.GetTranslationWithPlaceholder("SongOutro");
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

			FadeOutMod -= FadeFactor;
			CurrentSong.Volume = Game1.options.musicVolumeLevel * VolumeMod * FadeOutMod;
			return false;

		}

		/// <summary>
		/// Updates the volume of the music based on the player's proximity to Elias.
		/// </summary>
		internal static void UpdateMusicVolume()
		{
			// player hasn't moved and not fading in - no need to update volume
			// even if we take the early out, we need to dampen the ambient sound - see below for details
			Vector2 currentTilePos = Game1.player.Tile;
			if (PreviousPlayerTilePos == currentTilePos && !FadingInMusic)
			{
				Game1.ambientCategory.SetVolume(Game1.options.ambientVolumeLevel * AmbientMod);
				return;
			}

			float distanceToElias = Vector2.Distance(currentTilePos, EliasGuitarTilePos);

			// if within MaxVolumeCutoffDistance tiles of Elias, can skip formula and set volume to max.
			if (distanceToElias < MaxVolumeCutoffDistance)
			{
				VolumeMod = 1f;
			}
			// if further than MaxVolumeCutoffDistance tiles of Elias, use formula to determine volume percentage.
			else
			{
				// formula i'm using: y = 100 - FalloffFactor * log10(x / LogDivider)
				// gets value from 100 to 0 - divide by 100 to get VolumeMod
				// using some inverse square law shit: https://www.wkcgroup.com/tools-room/inverse-square-law-sound-calculator/

				VolumeMod = (float)(100 - FalloffFactor * Math.Log10(distanceToElias / LogDivider)) / 100f;
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
			Elias ??= Game1.getCharacterFromName("EliasSBV");

			if (Elias is null || Elias.currentLocation != Game1.currentLocation)
				return false;

			return Vector2.Distance(Elias.Tile, EliasGuitarTilePos) < 1f && Elias.doingEndOfRouteAnimation.Value;
		}

		/// <summary>
		/// Restarts the non-looping songs if they've ended.
		/// </summary>
		internal static void RestartSongIfNecessary()
		{
			// SettlingIn loops so no need to manually Play it. the others don't loop
			if (CurrentSong is { } cue && cue.Name != "SettlingIn" && CurrentSong.IsStopped)
				CurrentSong.Play();
		}

		#endregion

		#region Event Hooks

		/// <summary>
		/// Adds Audio event hooks.
		/// </summary>
		internal static void AddEventHooks()
		{
			// named hooks
			Globals.EventHelper.Player.Warped += HandleAudioOnWarp;

			// anonymous hooks - never need to be removed and only take one line of code so ¯\_(ツ)_/¯
			Globals.EventHelper.GameLoop.SaveLoaded += (_, _) => Init();
			Globals.EventHelper.GameLoop.DayEnding += (_, _) => SongIntroed = false;
			Globals.EventHelper.GameLoop.ReturnedToTitle += (_, _) => CleanUpMusicAndHandlers(forceStop: true);

			Globals.EventHelper.GameLoop.DayStarted += AddTownsvilleIfNecessary;
		}

		private static void AddTownsvilleIfNecessary(object? sender, DayStartedEventArgs e)
		{
			if (!Game1.player.eventsSeen.Contains("skellady.SBVCP_20031484") || EliasSongs.Count > 3)
				return;

			EliasSongs.Add(Game1.soundBank.GetCue("skellady.SBVCP_EliasTheme"));
		}

		// because these hooks are being added dynamically, track them so we don't leave behind or duplicate any
		// this should never happen, but out of an abundance of caution let's do it anyways :)
		private static int RecheckConditionsForMusicHooks = 0;
		private static int FadeOutHooks = 0;
		private static int UpdateVolumeHooks = 0;
		private static int RestartSongIfNecessaryHooks = 0;

		// logging to make sure hooks aren't getting left behind or duplicated
		// keeping this in case i need it bc it's annoying to write
		//private static void TraceHooks()
		//{
		//	Log.Trace("Positional audio hooks:");
		//	Log.Trace(RecheckConditionsHooks == 0 ? "No RecheckConditions hooks" : $"{RecheckConditionsHooks} RecheckConditions hook(s)");
		//	Log.Trace(FadeOutHooks == 0 ? "No FadeOut hooks" : $"{FadeOutHooks} FadeOut hook(s)");
		//	Log.Trace(UpdateVolumeHooks == 0 ? "No UpdateVolume hooks" : $"{UpdateVolumeHooks} UpdateVolume hook(s)");
		//	Log.Trace(RestartSongIfNecessaryHooks == 0 ? "No RestartSong hooks" : $"{RestartSongIfNecessaryHooks} RestartSong hook(s)");
		//}

		/// <summary>
		/// Determines if the player is warping in or out of Sunberry Village and updates music status and event hooks if needed.
		/// </summary>
		private static void HandleAudioOnWarp(object sender, WarpedEventArgs e)
		{
			GameLocation newLoc = e.NewLocation;
			GameLocation oldLoc = e.OldLocation;

			// Entering Sunberry - add event hooks and start music if it is the right time
			if (oldLoc.Name != "Custom_SBV_SunberryVillage" && newLoc.Name == "Custom_SBV_SunberryVillage")
			{
				if (ShouldMusicPlay() && !IsMusicPlaying)
				{
					StartMusicAndUpdateEventHooks();
				}
				else if (RecheckConditionsForMusicHooks == 0)
				{
					RecheckConditionsForMusicHooks++;
					Globals.EventHelper.GameLoop.UpdateTicked += RecheckConditionsForMusic;
				}
				//TraceHooks();
			}
			// Leaving Sunberry - stop music and remove event hooks
			else if (oldLoc.Name == "Custom_SBV_SunberryVillage" && newLoc.Name != "Custom_SBV_SunberryVillage")
			{
				CleanUpMusicAndHandlers(forceStop: true);
			}
		}

		/// <summary>
		/// Stops music and cleans up event hooks.
		/// </summary>
		/// <param name="forceStop">If true, ends music immediately.</param>
		private static void CleanUpMusicAndHandlers(bool forceStop = false)
		{
			// make sure there are no stray conditional hooks
			while (RecheckConditionsForMusicHooks > 0)
			{
				RecheckConditionsForMusicHooks--;
				Globals.EventHelper.GameLoop.UpdateTicked -= RecheckConditionsForMusic;
			}
			while (UpdateVolumeHooks > 0)
			{
				UpdateVolumeHooks--;
				Globals.EventHelper.GameLoop.UpdateTicked -= UpdateVolume;
			}
			while (RestartSongIfNecessaryHooks > 0)
			{
				RestartSongIfNecessaryHooks--;
				Globals.EventHelper.GameLoop.OneSecondUpdateTicked -= RestartSongIfNecessary;
			}

			// if forcing stop, immediately kill music and remove any fade out hooks attached
			if (forceStop)
			{
				while (FadeOutHooks > 0)
				{
					FadeOutHooks--;
					Globals.EventHelper.GameLoop.UpdateTicked -= FadeOutMusic;
				}

				StopMusic();
			}
			// otherwise, add a fade out hook which will, when done fading out, call this method again and force a stop
			else if (FadeOutHooks == 0)
			{
				Globals.EventHelper.GameLoop.UpdateTicked += FadeOutMusic;
				FadeOutHooks++;
			}

			// if there are any conditional hooks left at this point, there is a serious problem
			//TraceHooks();
		}

		/// <summary>
		/// Checks to see if conditions for music to play have changed and starts or stops the music if they have.
		/// </summary>
		private static void RecheckConditionsForMusic(object sender, UpdateTickedEventArgs e)
		{
			// shortcut - dont need to check if not near the right time
			if (Game1.timeOfDay is not (> 1700 and < 2400))
				return;

			bool shouldMusicPlay = ShouldMusicPlay();
			bool isMusicPlaying = IsMusicPlaying;

			// if music should be playing and isn't already, start it
			if (shouldMusicPlay && !isMusicPlaying)
				StartMusicAndUpdateEventHooks();

			// if it is playing and shouldn't be, stop it with fade out
			else if (isMusicPlaying && !shouldMusicPlay)
				CleanUpMusicAndHandlers(forceStop: false);
		}

		/// <summary>
		/// Starts music and adds necessary hooks if they're not already added.
		/// </summary>
		private static void StartMusicAndUpdateEventHooks()
		{
			StartMusic();

			// only add hooks if they're not already added - don't want duplicate hooks
			if (UpdateVolumeHooks == 0)
			{
				Globals.EventHelper.GameLoop.UpdateTicked += UpdateVolume;
				UpdateVolumeHooks++;
			}
			if (RestartSongIfNecessaryHooks == 0)
			{
				Globals.EventHelper.GameLoop.OneSecondUpdateTicked += RestartSongIfNecessary;
				RestartSongIfNecessaryHooks++;
			}
			if (RecheckConditionsForMusicHooks == 0)
			{
				Globals.EventHelper.GameLoop.UpdateTicked += RecheckConditionsForMusic;
				RecheckConditionsForMusicHooks++;
			}

			//TraceHooks();
		}

		/// <summary>
		/// Wrapper hook for <see cref="UpdateMusicVolume"/>.
		/// </summary>
		private static void UpdateVolume(object sender, UpdateTickedEventArgs e)
		{
			UpdateMusicVolume();
		}

		/// <summary>
		/// Wrapper hook for <see cref="FadeOut"/>. When fully faded out, calls <see cref="CleanUpMusicAndHandlers"/> to finish the job.
		/// </summary>
		private static void FadeOutMusic(object sender, UpdateTickedEventArgs e)
		{
			if (FadeOut())
			{
				CleanUpMusicAndHandlers(forceStop: true);
			}
		}

		/// <summary>
		/// Wrapper hook for <see cref="RestartSongIfNecessary"/>.
		/// </summary>
		private static void RestartSongIfNecessary(object sender, OneSecondUpdateTickedEventArgs e)
		{
			RestartSongIfNecessary();
		}

		#endregion
	}
}
