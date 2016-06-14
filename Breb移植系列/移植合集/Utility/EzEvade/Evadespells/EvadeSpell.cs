﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using EloBuddy.SDK.Menu;
using EloBuddy;
using EloBuddy.SDK.Menu.Values;

namespace ezEvade
{
    class EvadeSpell
    {
        private static AIHeroClient myHero { get { return ObjectManager.Player; } }

        public delegate void Callback();

        public static List<EvadeSpellData> evadeSpells = new List<EvadeSpellData>();
        public static List<EvadeSpellData> itemSpells = new List<EvadeSpellData>();
        public static EvadeCommand lastSpellEvadeCommand = new EvadeCommand { isProcessed = true, timestamp = EvadeUtils.TickCount };

        public static Menu menu;
        public static Menu evadeSpellMenu;

        public EvadeSpell(Menu mainMenu)
        {
            menu = mainMenu;

            //Game.OnUpdate += Game_OnGameUpdate;

            evadeSpellMenu = menu.AddSubMenu("Evade Spells", "EvadeSpells");
            ObjectCache.menuCache.AddMenuToCache(evadeSpellMenu);

            LoadEvadeSpellList();
            DelayAction.Add(100, () => CheckForItems());
        }

        private void Game_OnGameUpdate(EventArgs args)
        {
            //CheckDashing();
        }

        public static void CheckDashing()
        {
            if (EvadeUtils.TickCount - lastSpellEvadeCommand.timestamp < 250 && myHero.LSIsDashing()
                && lastSpellEvadeCommand.evadeSpellData.evadeType == EvadeType.Dash)
            {
                var dashInfo = myHero.LSGetDashInfo();

                //Console.WriteLine("" + dashInfo.EndPos.Distance(lastSpellEvadeCommand.targetPosition));
                lastSpellEvadeCommand.targetPosition = dashInfo.EndPos;
            }
        }

        private static void CheckForItems()
        {
            foreach (var spell in itemSpells)
            {
                var hasItem = Items.HasItem((int)spell.itemID);

                if (hasItem && !evadeSpells.Exists(s => s.spellName == spell.spellName))
                {
                    evadeSpells.Add(spell);

                    var newSpellMenu = CreateEvadeSpellMenu(spell);
                    ObjectCache.menuCache.AddMenuToCache(newSpellMenu);
                }
            }

            DelayAction.Add(5000, () => CheckForItems());
        }

        private static Menu CreateEvadeSpellMenu(EvadeSpellData spell)
        {

            string menuName = spell.name + " (" + spell.spellKey.ToString() + ") Settings";

            if (spell.isItem)
            {
                menuName = spell.name + " Settings";
            }

            evadeSpellMenu.AddGroupLabel(menuName);
            evadeSpellMenu.Add(spell.name + "UseEvadeSpell", new CheckBox("Use Spell"));
            evadeSpellMenu.Add(spell.name + "EvadeSpellDangerLevel", new ComboBox("Danger Level", spell.dangerlevel - 1, "Low", "Normal", "High", "Extreme"));
            evadeSpellMenu.Add(spell.name + "EvadeSpellMode", new ComboBox("Spell Mode", GetDefaultSpellMode(spell), "Undodgeable", "Activation Time", "Always"));

            return evadeSpellMenu;
        }

        public static int GetDefaultSpellMode(EvadeSpellData spell)
        {
            if (spell.dangerlevel > 3)
            {
                return 0;
            }

            return 1;
        }

        public static bool PreferEvadeSpell()
        {
            if (!Situation.ShouldUseEvadeSpell())
                return false;

            foreach (KeyValuePair<int, Spell> entry in SpellDetector.spells)
            {
                Spell spell = entry.Value;

                if (!ObjectCache.myHeroCache.serverPos2D.InSkillShot(spell, ObjectCache.myHeroCache.boundingRadius))
                    continue;

                if (ActivateEvadeSpell(spell, true))
                {
                    return true;
                }
            }

            return false;
        }

        public static void UseEvadeSpell()
        {
            if (!Situation.ShouldUseEvadeSpell())
            {
                return;
            }

            //int posDangerlevel = EvadeHelper.CheckPosDangerLevel(ObjectCache.myHeroCache.serverPos2D, 0);

            if (EvadeUtils.TickCount - lastSpellEvadeCommand.timestamp < 1000)
            {
                return;
            }

            foreach (KeyValuePair<int, Spell> entry in SpellDetector.spells)
            {
                Spell spell = entry.Value;

                if (ShouldActivateEvadeSpell(spell))
                {
                    if (ActivateEvadeSpell(spell))
                    {
                        Evade.SetAllUndodgeable();
                        return;
                    }
                }
            }

        }

        public static bool ActivateEvadeSpell(Spell spell, bool checkSpell = false)
        {
            var sortedEvadeSpells = evadeSpells.OrderBy(s => s.dangerlevel);

            var extraDelayBuffer = ObjectCache.menuCache.cache["ExtraPingBuffer"].Cast<Slider>().CurrentValue;
            float spellActivationTime = ObjectCache.menuCache.cache["SpellActivationTime"].Cast<Slider>().CurrentValue + ObjectCache.gamePing + extraDelayBuffer;

            if (ObjectCache.menuCache.cache["CalculateWindupDelay"].Cast<CheckBox>().CurrentValue)
            {
                var extraWindupDelay = Evade.lastWindupTime - EvadeUtils.TickCount;
                if (extraWindupDelay > 0)
                {
                    return false;
                }
            }

            foreach (var evadeSpell in sortedEvadeSpells)
            {
                var processSpell = true;

                if (ObjectCache.menuCache.cache[evadeSpell.name + "UseEvadeSpell"].Cast<CheckBox>().CurrentValue == false
                    || GetSpellDangerLevel(evadeSpell) > spell.GetSpellDangerLevel()
                    || (evadeSpell.isItem == false && myHero.Spellbook.CanUseSpell(evadeSpell.spellKey) != SpellState.Ready)
                    || (evadeSpell.isItem && !Items.CanUseItem((int)evadeSpell.itemID))
                    || (evadeSpell.checkSpellName && myHero.Spellbook.GetSpell(evadeSpell.spellKey).Name != evadeSpell.spellName))
           
                {
                    continue; //can't use spell right now               
                }

                float evadeTime, spellHitTime;
                spell.CanHeroEvade(myHero, out evadeTime, out spellHitTime);

                float finalEvadeTime = (spellHitTime - evadeTime);

                if (checkSpell)
                {
                    var mode = ObjectCache.menuCache.cache[evadeSpell.name + "EvadeSpellMode"].Cast<ComboBox>().CurrentValue;

                    if (mode == 0)
                    {
                        continue;
                    }
                    else if (mode == 1)
                    {
                        if (spellActivationTime < finalEvadeTime)
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    //if (ObjectCache.menuCache.cache[evadeSpell.name + "LastResort"].Cast<CheckBox>().CurrentValue)
                    if (evadeSpell.spellDelay <= 50 && evadeSpell.evadeType != EvadeType.Dash)
                    {
                        var path = myHero.Path;
                        if (path.Length > 0)
                        {
                            var movePos = path[path.Length - 1].LSTo2D();
                            var posInfo = EvadeHelper.CanHeroWalkToPos(movePos, ObjectCache.myHeroCache.moveSpeed, 0, 0);

                            if (GetSpellDangerLevel(evadeSpell) > posInfo.posDangerLevel)
                            {
                                continue;
                            }
                        }
                    }
                }

                if (evadeSpell.evadeType != EvadeType.Dash && spellHitTime > evadeSpell.spellDelay + 100 + Game.Ping +
                    ObjectCache.menuCache.cache["ExtraPingBuffer"].Cast<Slider>().CurrentValue)
                {
                    processSpell = false;

                    if (checkSpell == false)
                    {
                        continue;
                    }
                }

                if (evadeSpell.isSpecial)
                {
                    if (evadeSpell.useSpellFunc != null)
                    {
                        if (evadeSpell.useSpellFunc(evadeSpell, processSpell))
                        {
                            return true;
                        }
                    }

                    continue;
                }
                else if (evadeSpell.evadeType == EvadeType.Blink)
                {
                    if (evadeSpell.castType == CastType.Position)
                    {
                        var posInfo = EvadeHelper.GetBestPositionBlink();
                        if (posInfo != null)
                        {
                            CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, posInfo.position), processSpell);
                            //DelayAction.Add(50, () => myHero.IssueOrder(GameObjectOrder.MoveTo, posInfo.position.To3D()));
                            return true;
                        }
                    }
                    else if (evadeSpell.castType == CastType.Target)
                    {
                        var posInfo = EvadeHelper.GetBestPositionTargetedDash(evadeSpell);
                        if (posInfo != null && posInfo.target != null && posInfo.posDangerLevel == 0)
                        {
                            CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, posInfo.target), processSpell);
                            //DelayAction.Add(50, () => myHero.IssueOrder(GameObjectOrder.MoveTo, posInfo.position.To3D()));
                            return true;
                        }
                    }
                }
                else if (evadeSpell.evadeType == EvadeType.Dash)
                {
                    if (evadeSpell.castType == CastType.Position)
                    {
                        var posInfo = EvadeHelper.GetBestPositionDash(evadeSpell);
                        if (posInfo != null && CompareEvadeOption(posInfo, checkSpell))
                        {
                            if (evadeSpell.isReversed)
                            {
                                var dir = (posInfo.position - ObjectCache.myHeroCache.serverPos2D).LSNormalized();
                                var range = ObjectCache.myHeroCache.serverPos2D.LSDistance(posInfo.position);
                                var pos = ObjectCache.myHeroCache.serverPos2D - dir * range;

                                posInfo.position = pos;
                            }

                            CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, posInfo.position), processSpell);
                            //DelayAction.Add(50, () => myHero.IssueOrder(GameObjectOrder.MoveTo, posInfo.position.To3D()));
                            return true;
                        }
                    }
                    else if (evadeSpell.castType == CastType.Target)
                    {
                        var posInfo = EvadeHelper.GetBestPositionTargetedDash(evadeSpell);
                        if (posInfo != null && posInfo.target != null && posInfo.posDangerLevel == 0)
                        {
                            CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, posInfo.target), processSpell);
                            //DelayAction.Add(50, () => myHero.IssueOrder(GameObjectOrder.MoveTo, posInfo.position.To3D()));
                            return true;
                        }
                    }
                }
                else if (evadeSpell.evadeType == EvadeType.WindWall)
                {
                    if (spell.hasProjectile() || evadeSpell.spellName == "FioraW") //temp fix, don't have fiora :'(
                    {
                        var dir = (spell.startPos - ObjectCache.myHeroCache.serverPos2D).LSNormalized();
                        var pos = ObjectCache.myHeroCache.serverPos2D + dir * 100;

                        CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, pos), processSpell);
                        return true;
                    }
                }
                else if (evadeSpell.evadeType == EvadeType.SpellShield)
                {
                    if (evadeSpell.isItem)
                    {
                        CastEvadeSpell(() => Items.UseItem((int)evadeSpell.itemID), processSpell);
                        return true;
                    }
                    else
                    {
                        if (evadeSpell.castType == CastType.Target)
                        {
                            CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell, myHero), processSpell);
                            return true;
                        }
                        else if (evadeSpell.castType == CastType.Self)
                        {
                            CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell), processSpell);
                            return true;
                        }
                    }
                }
                else if (evadeSpell.evadeType == EvadeType.MovementSpeedBuff)
                {
                    
                }
            }

            return false;
        }

        public static void CastEvadeSpell(Callback func, bool process = true)
        {
            if (process)
            {
                func();
            }
        }

        public static bool CompareEvadeOption(PositionInfo posInfo, bool checkSpell = false)
        {
            if (checkSpell)
            {
                if (posInfo.posDangerLevel == 0)
                {
                    return true;
                }
            }

            return posInfo.isBetterMovePos();
        }

        private static bool ShouldActivateEvadeSpell(Spell spell)
        {
            if (Evade.lastPosInfo == null)
                return false;

            if (ObjectCache.menuCache.cache["DodgeSkillShots"].Cast<KeyBind>().CurrentValue)
            {
                if (Evade.lastPosInfo.undodgeableSpells.Contains(spell.spellID)
                && ObjectCache.myHeroCache.serverPos2D.InSkillShot(spell, ObjectCache.myHeroCache.boundingRadius))
                {
                    return true;
                }
            }
            else
            {
                if (ObjectCache.myHeroCache.serverPos2D.InSkillShot(spell, ObjectCache.myHeroCache.boundingRadius))
                {
                    return true;
                }
            }


            /*float activationTime = Evade.menu.SubMenu("MiscSettings").SubMenu("EvadeSpellMisc").Item("EvadeSpellActivationTime")
                .GetValue<Slider>().Value + ObjectCache.gamePing;

            if (spell.spellHitTime != float.MinValue && activationTime > spell.spellHitTime - spell.evadeTime)
            {
                return true;
            }*/

            return false;
        }

        public static int GetSpellDangerLevel(EvadeSpellData spell)
        {
            var dangerStr = ObjectCache.menuCache.cache[spell.name + "EvadeSpellDangerLevel"].Cast<ComboBox>().CurrentValue;

            var dangerlevel = 1;

            switch (dangerStr)
            {
                case 0:
                    dangerlevel = 1;
                    break;
                case 2:
                    dangerlevel = 3;
                    break;
                case 3:
                    dangerlevel = 4;
                    break;
                default:
                    dangerlevel = 2;
                    break;
            }

            return dangerlevel;
        }

        private SpellSlot GetSummonerSlot(string spellName)
        {
            if (myHero.Spellbook.GetSpell(SpellSlot.Summoner1).SData.Name == spellName)
            {
                return SpellSlot.Summoner1;
            }
            else if (myHero.Spellbook.GetSpell(SpellSlot.Summoner2).SData.Name == spellName)
            {
                return SpellSlot.Summoner2;
            }

            return SpellSlot.Unknown;
        }

        private void LoadEvadeSpellList()
        {

            foreach (var spell in EvadeSpellDatabase.Spells.Where(
                s => (s.charName == myHero.ChampionName || s.charName == "AllChampions")))
            {

                if (spell.isSummonerSpell)
                {
                    SpellSlot spellKey = GetSummonerSlot(spell.spellName);
                    if (spellKey == SpellSlot.Unknown)
                    {
                        continue;
                    }
                    else
                    {
                        spell.spellKey = spellKey;
                    }
                }

                if (spell.isItem)
                {
                    itemSpells.Add(spell);
                    continue;
                }

                if (spell.isSpecial)
                {
                    SpecialEvadeSpell.LoadSpecialSpell(spell);
                }

                evadeSpells.Add(spell);

                var newSpellMenu = CreateEvadeSpellMenu(spell);
            }

            evadeSpells.Sort((a, b) => a.dangerlevel.CompareTo(b.dangerlevel));
        }
    }
}
