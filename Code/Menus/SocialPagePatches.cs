using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunberryVillage.Code.Menus
{
    [HarmonyPatch]
    internal class SocialPagePatches
    {
        /// <summary>
        /// Dictionary of the unlockable NPCs and the eventID of the event that unlocks them
        /// </summary>
        public static Dictionary<string, string> unlock_rules = new() {
                { "EliasSBV", "skellady.SBVCP_20031483" }
            };

        /// <summary>
        /// Patches <c>SocialPage.drawNPCSlot</c> to draw "(Locked) below NPCs that arent available for marriage yet".
        /// </summary>
        [HarmonyPatch(typeof(SocialPage), nameof(SocialPage.drawNPCSlot))]
        [HarmonyPostfix]
        public static void SocialPage_drawNPCSlot_Postfix(SocialPage __instance, SpriteBatch b, int i)
        {
            var socialEntry = __instance.SocialEntries[i];
            if (!socialEntry.IsDatable && unlock_rules.TryGetValue(socialEntry.InternalName, out var eventCondition))
            {
                if (Game1.player.eventsSeen.Contains(eventCondition))
                    return;
                int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
                string text = Game1.parseText(Globals.TranslationHelper.Get("RelationshipStatus.Locked"), Game1.smallFont, width);
                Vector2 textSize = Game1.smallFont.MeasureString(text);
                float lineHeight = Game1.smallFont.MeasureString("W").Y;
                b.DrawString(Game1.smallFont, text, new Vector2((__instance.xPositionOnScreen + 192 + 8) - textSize.X / 2f, __instance.sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
            }
        }
    }
}
