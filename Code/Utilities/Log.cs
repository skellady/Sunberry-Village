using StardewModdingAPI;
// ReSharper disable UnusedMember.Global

namespace SunberryVillage.Utilities;

// Credit to kittycatcasey for initial version. i've iterated on it a bit :3
internal class Log
{
	public static IMonitor Monitor;

	public static void Verbose(object obj) => Monitor.VerboseLog(obj.ToString() ?? string.Empty);

	public static void Trace(object obj) => Monitor.Log(obj.ToString() ?? string.Empty);

	// Only log Debug messages if compiled in Debug mode.
	public static void Debug(object obj)
	{
		#if DEBUG

		Monitor.Log(obj.ToString() ?? string.Empty, LogLevel.Debug);

		#endif
	}

	public static void Info(object obj) => Monitor.Log(obj.ToString() ?? string.Empty, LogLevel.Info);

	public static void Warn(object obj) => Monitor.Log(obj.ToString() ?? string.Empty, LogLevel.Warn);

	public static void Error(object obj) => Monitor.Log(obj.ToString() ?? string.Empty, LogLevel.Error);
}
