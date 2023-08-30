using StardewValley;
using StardewValley.Menus;
using SunberryVillage.Animations;
using SunberryVillage.PortraitShake;
using SunberryVillage.PositionalAudio;
using System;

namespace SunberryVillage.Utilities;

internal class EventHookManager
{
	/// <summary>
	/// Initializes all event hooks which are applied without conditions. Event hooks with no conditional application should be added here.
	/// </summary>
	internal static void InitializeEventHooks()
	{
		// General
		Globals.EventHelper.Content.AssetRequested += AssetManager.LoadOrEditAssets;
		Globals.EventHelper.GameLoop.DayStarted += ReloadCachedAssets;

		// Tarot
		Globals.EventHelper.GameLoop.DayEnding += ClearTarotFlag;

		// Big animations
		Globals.EventHelper.GameLoop.DayEnding += AnimationsHandler.DayEnd;

		// Portrait shake
		Globals.EventHelper.Display.MenuChanged += CheckForPortraitShake;

		// Positional Audio
		Globals.EventHelper.GameLoop.DayStarted += ChooseMusic;
		Globals.EventHelper.GameLoop.DayEnding += ClearSongIntroFlag;
		Globals.EventHelper.GameLoop.SaveLoaded += InitializeSoundCues;
		Globals.EventHelper.Player.Warped += OnWarped;
		Globals.EventHelper.GameLoop.ReturnedToTitle += (_, _) => CleanUpMusicAndHandlers(fadeOut: false);
	}

	#region General

	/// <summary>
	/// Reloads any cached assets at the start of each day if they have been modified.
	/// </summary>
	private static void ReloadCachedAssets(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
	{
		AssetManager.ReloadAssets();
	}

	#endregion

	#region Tarot

	/// <summary>
	/// Clears the mod data flag which prevents getting multiple tarot readings in one day.
	/// </summary>
	private static void ClearTarotFlag(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
	{
		Game1.player.modData.Remove("SunberryTeam.SBV/Tarot/ReadingDoneForToday");
	}

	#endregion

	#region Portrait shake

	/// <summary>
	/// When a DialogueBox menu opens, checks to see if the portrait should shake. 
	/// </summary>
	private static void CheckForPortraitShake(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
	{
		if (e.NewMenu is DialogueBox { characterDialogue: { } } dialogue)
			PortraitShakeHandler.SetShake(dialogue.characterDialogue);
	}

	#endregion

	#region Positional audio

	// some of these hooks only serve to call other methods. i consider them 'wrappers' because i don't want to scatter hooks all over my code
	// also, we can't add them anonymously like we do the ReturnedToTitle hook above because we need to add or remove the dynamically.
	// idk it makes sense to me

	// because these hooks are being added dynamically, track them so we don't leave behind or duplicate any
	private static int RecheckConditionsHooks = 0;
	private static int FadeOutHooks = 0;
	private static int UpdateVolumeHooks = 0;
	private static int RestartSongIfNecessaryHooks = 0;

	// logging to make sure hooks aren't getting left behind or duplicated
	//private static void TraceHooks()
	//{
	//	Log.Trace("Positional audio hooks:");
	//	Log.Trace(RecheckConditionsHooks == 0 ? "No RecheckConditions hooks" : $"{RecheckConditionsHooks} RecheckConditions hook(s)");
	//	Log.Trace(FadeOutHooks == 0 ? "No FadeOut hooks" : $"{FadeOutHooks} FadeOut hook(s)");
	//	Log.Trace(UpdateVolumeHooks == 0 ? "No UpdateVolume hooks" : $"{UpdateVolumeHooks} UpdateVolume hook(s)");
	//	Log.Trace(RestartSongIfNecessaryHooks == 0 ? "No RestartSong hooks" : $"{RestartSongIfNecessaryHooks} RestartSong hook(s)");
	//}

	/// <summary>
	/// Calls <see cref="PositionalAudioHandler.Init"/>.
	/// </summary>
	private static void InitializeSoundCues(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
	{
		PositionalAudioHandler.Init();
	}

	/// <summary>
	/// Determines if the player is warping in or out of Sunberry Village and updates music status and event hooks if needed.
	/// </summary>
	private static void OnWarped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
	{
		GameLocation newLoc = e.NewLocation;
		GameLocation oldLoc = e.OldLocation;

		// Entering Sunberry - add event hooks and start music if it is the right time
		if (oldLoc.Name != "Custom_SBV_SunberryVillage" && newLoc.Name == "Custom_SBV_SunberryVillage")
		{
			if (PositionalAudioHandler.ShouldMusicPlay() && !PositionalAudioHandler.IsMusicPlaying)
			{
				StartMusicAndUpdateEventHooks();
			}
			else
			{
				if (RecheckConditionsHooks == 0)
				{
					RecheckConditionsHooks++;
					Globals.EventHelper.GameLoop.UpdateTicked += RecheckConditionsForMusic;
				}
				//TraceHooks();
			}
		}
		// Leaving Sunberry - stop music and remove event hooks
		else if (oldLoc.Name == "Custom_SBV_SunberryVillage" && newLoc.Name != "Custom_SBV_SunberryVillage")
		{
			CleanUpMusicAndHandlers(fadeOut: false);
		}
	}

	/// <summary>
	/// Stops music and cleans up event hooks.
	/// </summary>
	private static void CleanUpMusicAndHandlers(bool fadeOut = false)
	{
		// make sure there are no stray conditional hooks
		while (RecheckConditionsHooks > 0)
		{
			RecheckConditionsHooks--;
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

		// ...i dont wanna talk about it
		// i made some bad decisions early in and they led to this spaghetti hell
		if (fadeOut)
		{
			if (FadeOutHooks == 0)
			{
				Globals.EventHelper.GameLoop.UpdateTicked += FadeOutMusic;
				FadeOutHooks++;
			}
		}
		else
		{
			while (FadeOutHooks > 0)
			{
				FadeOutHooks--;
				Globals.EventHelper.GameLoop.UpdateTicked -= FadeOutMusic;
			}

			PositionalAudioHandler.StopMusic();
		}
		// if there are any conditional hooks left at this point, there is a serious problem
		//TraceHooks();

	}

	/// <summary>
	/// Checks to see if conditions for music to play have changed and starts or stops the music if they have.
	/// </summary>
	private static void RecheckConditionsForMusic(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
	{
		// shortcut - dont need to check if not near the right time
		if (Game1.timeOfDay is not (> 1700 and < 2400))
			return;

		bool shouldMusicPlay = PositionalAudioHandler.ShouldMusicPlay();
		bool isMusicPlaying = PositionalAudioHandler.IsMusicPlaying;

		// if music should be playing and isn't already, start it
		if (shouldMusicPlay && !isMusicPlaying)
			StartMusicAndUpdateEventHooks();

		// if it is playing and shouldn't be, stop it with fade out
		else if (!shouldMusicPlay && isMusicPlaying)
			CleanUpMusicAndHandlers(fadeOut: true);
	}

	/// <summary>
	/// Starts music and adds necessary hooks if they're not already added.
	/// </summary>
	private static void StartMusicAndUpdateEventHooks()
	{
		PositionalAudioHandler.StartMusic();

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
		if (RecheckConditionsHooks == 0)
		{
			Globals.EventHelper.GameLoop.UpdateTicked += RecheckConditionsForMusic;
			RecheckConditionsHooks++;
		}
		
		//TraceHooks();
	}

	/// <summary>
	/// Resets value of <see cref="PositionalAudioHandler.SongIntroed"/> for the next time it is needed.
	/// </summary>
	private static void ClearSongIntroFlag(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
	{
		PositionalAudioHandler.SongIntroed = false;
	}

	/// <summary>
	/// Wrapper hook for <see cref="PositionalAudioHandler.UpdateMusicVolume"/>.
	/// </summary>
	private static void UpdateVolume(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
	{
		PositionalAudioHandler.UpdateMusicVolume();
	}

	/// <summary>
	/// Pseudo-wrapper hook for <see cref="PositionalAudioHandler.FadeOut"/>. When fully faded out, calls <see cref="CleanUpMusicAndHandlers"/> to finish the job.
	/// </summary>
	private static void FadeOutMusic(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
	{
		if (PositionalAudioHandler.FadeOut())
		{
			CleanUpMusicAndHandlers(fadeOut: false);
		}
	}

	/// <summary>
	/// Wrapper hook for <see cref="PositionalAudioHandler.ChooseMusic"/>.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private static void ChooseMusic(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
	{
		PositionalAudioHandler.ChooseMusic();
	}

	/// <summary>
	/// Wrapper hook for <see cref="PositionalAudioHandler.RestartSongIfNecessary"/>.
	/// </summary>
	private static void RestartSongIfNecessary(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
	{
		PositionalAudioHandler.RestartSongIfNecessary();
	}

	#endregion
}
