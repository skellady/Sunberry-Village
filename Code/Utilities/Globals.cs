using StardewModdingAPI;
using StardewModdingAPI.Events;
// ReSharper disable UnusedMember.Global

namespace SunberryVillage.Utilities;

internal class Globals
{
	public static IManifest Manifest { get; set; }
	public static IModHelper Helper { get; set; }
	// ReSharper disable once InconsistentNaming
	public static ICommandHelper CCHelper => Helper.ConsoleCommands;
	public static IGameContentHelper GameContent => Helper.GameContent;
	public static IModContentHelper ModContent => Helper.ModContent;
	public static IContentPackHelper ContentPackHelper => Helper.ContentPacks;
	public static IDataHelper DataHelper => Helper.Data;
	public static IInputHelper InputHelper => Helper.Input;
	public static IModEvents EventHelper => Helper.Events;
	public static IMultiplayerHelper MultiplayerHelper => Helper.Multiplayer;
	public static IReflectionHelper ReflectionHelper => Helper.Reflection;
	public static ITranslationHelper TranslationHelper => Helper.Translation;
	// ReSharper disable once InconsistentNaming
	public static string UUID => Manifest.UniqueID;

	internal static void InitializeGlobals(ModEntry modEntry)
	{
		Manifest = modEntry.ModManifest;
		Helper = modEntry.Helper;
		Log.Monitor = modEntry.Monitor;
	}
}
