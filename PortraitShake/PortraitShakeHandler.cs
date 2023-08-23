using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using SunberryVillage.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunberryVillage.PortraitShake
{

	internal class PortraitShakeHandler
	{
		internal static Dictionary<string, List<int>> PortraitsToShake = new();

		internal static readonly PerScreen<bool> PortraitShouldShake = new();

		internal static bool ShouldPortraitShake(Dialogue dialogue)
		{
			string name = dialogue.speaker.Name;
			int index = dialogue.getPortraitIndex();

			if (!PortraitsToShake.ContainsKey(name) || !PortraitsToShake[name].Contains(index))
				return false;

			return true;
		}

		internal static void SetShake(Dialogue dialogue)
		{
			PortraitShouldShake.Value = ShouldPortraitShake(dialogue);
		}

		internal static void ReloadAsset()
		{
			PortraitsToShake = Globals.GameContent.Load<Dictionary<string, List<int>>>("SBV.PortraitsToShake");
		}
	}
}
