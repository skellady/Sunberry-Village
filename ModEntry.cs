using StardewModdingAPI;
using SunberryVillage.Patching;
using SunberryVillage.Utilities;

namespace SunberryVillage;

public class ModEntry : Mod
{
	public override void Entry(IModHelper helper)
	{
		Globals.InitializeGlobals(this);
		EventHookManager.InitializeEventHooks();
		ConsoleCommandManager.InitializeConsoleCommands();
		HarmonyPatcher.ApplyPatches();
	}
}
