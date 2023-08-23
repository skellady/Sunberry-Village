using StardewModdingAPI;

namespace SunberryVillage.Utilities;

// Credit to kittycatcasey
internal class Log
{
	public static IMonitor Monitor;

	public static void Verbose(object obj) => Monitor.VerboseLog(obj.ToString());

	public static void Trace(object obj) => Monitor.Log(obj.ToString());

	public static void Debug(object obj) => Monitor.Log(obj.ToString(), LogLevel.Debug);

	public static void Info(object obj) => Monitor.Log(obj.ToString(), LogLevel.Info);

	public static void Warn(object obj) => Monitor.Log(obj.ToString(), LogLevel.Warn);

	public static void Error(object obj) => Monitor.Log(obj.ToString(), LogLevel.Error);
}
