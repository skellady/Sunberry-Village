using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace SunberryVillage.Menus;
internal class MenuHooks
{
	internal static readonly List<string> Minerals = [
		"skellady.SBVCP_Annabergite",
		"skellady.SBVCP_BlueAuraQuartz",
		"skellady.SBVCP_Citrine",
		"skellady.SBVCP_Moonberry",
		"skellady.SBVCP_Purpurite",
		"skellady.SBVCP_Serpentine"
	];

	internal const string LunarBean = "skellady.SBVCP_LunarBean";

	internal static readonly List<string> Artifacts1 = [
		"skellady.SBVCP_MushroomStatue",
		"skellady.SBVCP_AncientStaff",
		"skellady.SBVCP_SilverMask",
		"skellady.SBVCP_LapisNecklace",
		"skellady.SBVCP_WickedVase",
		"skellady.SBVCP_GoldenCoins"
	];

	internal static readonly List<string> Artifacts2 = [
		"skellady.SBVCP_ChippedJug",
		"skellady.SBVCP_DwarfKeyI",
		"skellady.SBVCP_DwarfKeyII",
		"skellady.SBVCP_DwarfKeyIII",
		"skellady.SBVCP_DwarfKeyIV"
	];

	public static void AddEventHooks()
	{
		Globals.EventHelper.Display.MenuChanged += AlterCollectionsMenu;
	}

	private static void AlterCollectionsMenu(object? sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
	{
		if (e.NewMenu is not GameMenu gameMenu)
			return;

		CollectionsPage collectionsPage = (CollectionsPage)gameMenu.pages[7];

		// artifacts
		foreach (List<ClickableTextureComponent> cList in collectionsPage.collections[2])
		{
			foreach (ClickableTextureComponent c in cList)
			{
				string[] split = ArgUtility.SplitBySpace(c.name);

				if ( (!Artifacts1.Contains(split[0]) || !Game1.player.team.completedSpecialOrders.Contains("SBV.SpecialOrder.SBVMuseumArtifacts1")) &&
				     (!Artifacts2.Contains(split[0]) || !Game1.player.team.completedSpecialOrders.Contains("SBV.SpecialOrder.SBVMuseumArtifacts2")) )
					continue;

				split[1] = bool.TrueString;
				c.name = string.Join(" ", split);
			}
		}

		// minerals
		foreach (List<ClickableTextureComponent> cList in collectionsPage.collections[3])
		{
			foreach (ClickableTextureComponent c in cList)
			{
				string[] split = ArgUtility.SplitBySpace(c.name);

                if ((!Minerals.Contains(split[0]) || !Game1.player.team.completedSpecialOrders.Contains("SBV.SpecialOrder.SBVMuseumMinerals")) &&
                    (split[0] != LunarBean || (!Game1.player.mailReceived.Contains("skellady.SBVCP_LunarBeanGrown") && !(Game1.player.basicShipped.TryGetValue("skellady.SBVCP_LunarBean", out int timesShipped) && timesShipped > 0) && !(Game1.player.mineralsFound.TryGetValue("skellady.SBVCP_LunarBean", out int timesFound) && timesFound > 0))))
                    continue;

                split[1] = bool.TrueString;
				c.name = string.Join(" ", split);
			}
		}
	}
}
