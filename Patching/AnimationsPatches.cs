using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SunberryVillage.Utilities;
using StardewValley.Menus;

namespace SunberryVillage.Patching
{
    [HarmonyPatch]
    internal class AnimationsPatches
    {
        const string CP_PATH = "SunberryTeam.SBV/Animations";
        private static Dictionary<string, AnimationData> animationData;

        [HarmonyPatch(typeof(NPC), "startRouteBehavior")]
        [HarmonyPostfix]

        internal static void startRouteBehavior_Postfix(ref NPC __instance, string behaviorName)
        {
            try
            {
                if (behaviorName.Length > 0 && behaviorName[0] == '"')
                {
                    return;
                }

                if (animationData.TryGetValue(behaviorName, out AnimationData animation))
                {
                    if (animation.size != Vector2.Zero)
                    {
                        __instance.extendSourceRect((int)animation.size.X - 16, (int)animation.size.Y - 32);
                        __instance.Sprite.SpriteWidth = (int)animation.size.X;
                        __instance.Sprite.SpriteHeight = (int)animation.size.Y;
                    }
                    if (animation.offset != Vector2.Zero)
                    {
                        __instance.drawOffset.Value = animation.offset;
                    }
                    __instance.Sprite.ignoreSourceRectUpdates = false;
                    __instance.HideShadow = animation.hideShadow;


                }


            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(startRouteBehavior_Postfix)}\" has encountered an error for {__instance.Name} with {behaviorName} \n{e}");
            }
        }


        [HarmonyPatch(typeof(NPC), "finishRouteBehavior")]
        [HarmonyPrefix]
        private static bool finishRouteBehavior_Prefix(ref NPC __instance, string behaviorName)
        {
            try
            {
                if (behaviorName.Length > 0 && behaviorName[0] == '"')
                {
                    return true;
                }
                if (animationData.ContainsKey(behaviorName))
                {
                    __instance.reloadSprite();
                    __instance.Sprite.SpriteWidth = 16;
                    __instance.Sprite.SpriteHeight = 32;
                    __instance.Sprite.UpdateSourceRect();
                    __instance.drawOffset.Value = Vector2.Zero;
                    __instance.Halt();
                    __instance.movementPause = 1;
                    __instance.HideShadow = false;
                }
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Harmony patch \"{nameof(finishRouteBehavior_Prefix)}\" has encountered an error for {__instance.Name} with {behaviorName} \n{e}");
                return true;
            }

        }

        internal static void ReloadCPData()
        {
            animationData = Globals.GameContent.Load<Dictionary<string, AnimationData>>(CP_PATH);
        }


        public static void DayEnd(object sender, DayEndingEventArgs args)
        {// reset sprites in case user ended day while animations were in progress
            try
            {
                foreach (var entry in animationData)
                {
                    NPC npc = Game1.getCharacterFromName(entry.Value.npcName);
                    npc.Sprite.SpriteHeight = 32;
                    npc.Sprite.SpriteWidth = 16;
                    npc.Sprite.ignoreSourceRectUpdates = false;
                    npc.Sprite.UpdateSourceRect();
                    npc.drawOffset.Value = Vector2.Zero;
                    npc.IsInvisible = false;
                    npc.HideShadow = false;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed in SBV Animations Day End reset:\n{e}");
            }
        }

    }

    internal class AnimationData
    {
        public string npcName;
        public Vector2 size;
        public Vector2 offset;
        public bool hideShadow;
    }
}
