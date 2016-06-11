﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace Marksman.Common
{
    using EloBuddy;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;
    using System.Drawing;

    using Color = SharpDX.Color;

    internal class CommonAutoPink
    {
        static Menu localMenu;
        static int delay = 0; 
        static float vayneBuffEndTime = 0;

        public static void Initialize(Menu nParentMenu)
        {
            localMenu = nParentMenu;
            localMenu.AddGroupLabel("Pink Ward");
            localMenu.Add("Pink.Use", new CheckBox("Enable Pink Ward"));

            var enemyChampions = new[]
                                     {
                                         "Akali", "KhaZix", "Talon", "Shaco", "MonkeyKing", "Vayne", "Rengar", "Ezreal", "Sivir"
                                     };

            List<AIHeroClient> enemies = HeroManager.Enemies;

            foreach (var e in from fenemies in enemies from fBigBoys in enemyChampions where fBigBoys == fenemies.ChampionName select fenemies)
            {
                localMenu.Add("UsePink." + e.ChampionName, new CheckBox("For: " + e.ChampionName));
            }

            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            GameObject.OnCreate += OnCreate;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void OnProcessSpellCast(Obj_AI_Base enemy, GameObjectProcessSpellCastEventArgs args)
        {
            //Game.PrintChat("OnProcessSpellCast: " + args.SData.Name);
            if (!localMenu["Pink.Use"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            try
            {
                if (enemy.Type == GameObjectType.AIHeroClient && enemy.IsEnemy)
                {
                    if (args.SData.Name.ToLower() == "akalismokebomb" 
                        || args.SData.Name.ToLower() == "deceive"
                        || args.SData.Name.ToLower() == "khazixr" 
                        || args.SData.Name.ToLower() == "khazixrlong"
                        || args.SData.Name.ToLower() == "talonshadowassault" 
                        || args.SData.Name.ToLower() == "monkeykingdecoy"
                        || args.SData.Name.ToLower() == "hideinshadows" 
                        || args.SData.Name.ToLower() == "vaynetumble"
                        )
                    {
                        if (CheckSlot() == SpellSlot.Unknown || ObjectManager.Player.LSDistance(enemy.Position) > 800)
                        {
                            return;
                        }

                        if (args.SData.Name.ToLower().Contains("vaynetumble") && Game.Time > vayneBuffEndTime)
                        {
                            return;
                        }

                        if (Environment.TickCount - delay > 1500 || delay == 0)
                        {
                            var pos = ObjectManager.Player.LSDistance(args.End) > 600
                                          ? ObjectManager.Player.Position
                                          : args.End;
                            ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), pos);
                            delay = Environment.TickCount;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error '{0}'", e);
            }
        }

        #region Rengar / LeBlanc
        private static void OnCreate(GameObject sender, EventArgs args)
        {
            if (!localMenu["Pink.Use"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            var Rengar = HeroManager.Enemies.Find(x => x.ChampionName.ToLower() == "rengar");
            if (Rengar != null)
            {
                if (sender.IsEnemy && sender.Name.Contains("Rengar_Base_R_Alert"))
                {
                    if (ObjectManager.Player.HasBuff("rengarralertsound") &&
                        !Rengar.IsVisible &&
                        !Rengar.IsDead &&
                        CheckSlot() != SpellSlot.Unknown)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), ObjectManager.Player.Position);
                    }
                }
            }

            var leBlanc = HeroManager.Enemies.Find(x => x.ChampionName.ToLower() == "leblanc");
            if (leBlanc != null)
            {
                if (ObjectManager.Player.LSDistance(sender.Position) > 600) return;
                if (sender.IsEnemy && sender.Name == "LeBlanc_Base_P_poof.troy")
                {
                    if (!leBlanc.IsVisible && !leBlanc.IsDead && CheckSlot() != SpellSlot.Unknown)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(CheckSlot(), ObjectManager.Player.Position);
                    }
                }
            }
        }
        #endregion

        #region Vayne
        private static void Game_OnUpdate(EventArgs args)
        {
            if (!localMenu["Pink.Use"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            
            var vayne = HeroManager.Enemies.Find(e => e.ChampionName.ToLower() == "vayne");
            if (vayne != null)
            {
                foreach (var hero in ObjectManager.Get<AIHeroClient>().Where(
                    x =>
                        x.IsEnemy && x.ChampionName.ToLower().Contains("vayne") &&
                        x.Buffs.Any(y => y.Name == "VayneInquisition")))
                {
                    vayneBuffEndTime = hero.Buffs.First(x => x.Name == "VayneInquisition").EndTime;
                }
            }
        }
        #endregion
        
        #region Check Spell Slot
        private static SpellSlot CheckSlot()
        {
            SpellSlot slot = SpellSlot.Unknown;
            if (Items.CanUseItem(3362) && Items.HasItem(3362, ObjectManager.Player)) 
            {
                slot = SpellSlot.Trinket;
            }
            else if (Items.CanUseItem(3364) && Items.HasItem(3364, ObjectManager.Player))
            {
                slot = SpellSlot.Trinket;
            }
            else if (Items.CanUseItem(2043) && Items.HasItem(2043, ObjectManager.Player))
            {
                slot = ObjectManager.Player.GetSpellSlot("VisionWard");
            }
            return slot;
        }
        #endregion
    }

}
;