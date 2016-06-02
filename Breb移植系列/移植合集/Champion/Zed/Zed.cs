﻿namespace Valvrave_Sharp.Plugin
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Data.Enumerations;
    using LeagueSharp.SDK;
    using SharpDX;

    using Valvrave_Sharp.Core;
    using Valvrave_Sharp.Evade;

    using Color = System.Drawing.Color;
    using Skillshot = Valvrave_Sharp.Evade.Skillshot;
    using EloBuddy;
    using LeagueSharp.SDK.Core.Utils;
    using EloBuddy.SDK.Menu;
    using EloBuddy.SDK.Menu.Values;
    using LeagueSharp.SDK.Modes;
    using EloBuddy.SDK;
    #endregion

    internal class Zed : Program
    {
        #region Static Fields

        private static GameObject deathMark;

        private static int lastW;

        public static Menu config = _MainMenu;

        private static bool wCasted, rCasted;

        private static MissileClient wMissile;

        private static Obj_AI_Minion wShadow, rShadow;

        private static int wShadowT, rShadowT;

        #endregion

        #region Constructors and Destructors

        public static Menu comboMenu, hybridMenu, lhMenu, ksMenu, drawMenu, miscMenu;

        public static bool getCheckBoxItem(Menu m, string item)
        {
            return m[item].Cast<CheckBox>().CurrentValue;
        }

        public static int getSliderItem(Menu m, string item)
        {
            return m[item].Cast<Slider>().CurrentValue;
        }

        public static bool getKeyBindItem(Menu m, string item)
        {
            return m[item].Cast<KeyBind>().CurrentValue;
        }

        public static int getBoxItem(Menu m, string item)
        {
            return m[item].Cast<ComboBox>().CurrentValue;
        }

        public Zed()
        {
            Q = new LeagueSharp.SDK.Spell(SpellSlot.Q, 925).SetSkillshot(0.25f, 50, 1700, true, SkillshotType.SkillshotLine);
            Q2 = new LeagueSharp.SDK.Spell(Q.Slot, Q.Range).SetSkillshot(Q.Delay, Q.Width, Q.Speed, true, Q.Type);
            Q3 = new LeagueSharp.SDK.Spell(Q.Slot, Q.Range).SetSkillshot(Q.Delay, Q.Width, Q.Speed, true, Q.Type);
            W = new LeagueSharp.SDK.Spell(SpellSlot.W, 700).SetSkillshot(0, 60, 1750, false, SkillshotType.SkillshotLine);
            E = new LeagueSharp.SDK.Spell(SpellSlot.E, 290).SetTargetted(0.005f, float.MaxValue);
            R = new LeagueSharp.SDK.Spell(SpellSlot.R, 625);
            Q.DamageType = W.DamageType = E.DamageType = R.DamageType = DamageType.Physical;
            Q.MinHitChance = HitChance.VeryHigh;

            comboMenu = config.AddSubMenu("连招", "Combo");
            comboMenu.AddGroupLabel("Q/E: 持续开启");
            comboMenu.Add("Ignite", new CheckBox("使用 点燃"));
            comboMenu.Add("Items", new CheckBox("使用 物品"));
            comboMenu.AddGroupLabel("替换设置");
            comboMenu.Add("SwapIfKill", new CheckBox("替换W/R 如果标记能杀死目标", false));
            comboMenu.Add("SwapIfHpU", new Slider("替换 W/R 如果血量 < (%)", 10));
            comboMenu.Add("SwapGap", new ComboBox("替换 W/R 接近", 1, "关闭", "智能", "一直"));
            comboMenu.AddGroupLabel("W 设置");
            comboMenu.Add("WNormal", new CheckBox("用于 非连招"));
            comboMenu.Add("WAdv", new ComboBox("用于 R 连招", 1, "关闭", "线形", "三角", "鼠标"));
            comboMenu.AddGroupLabel("R 设置");
            comboMenu.Add("R", new KeyBind("使用 R", false, KeyBind.BindTypes.PressToggle, 'X'));
            comboMenu.Add("RMode", new ComboBox("模式", 0, "一直", "等待 Q/E"));
            comboMenu.Add("RStopRange", new Slider("防止 Q/W/E  如果 R 可用并且距离<=", (int)(R.Range + 200), (int)R.Range, (int)(R.Range + W.Range)));
            if (GameObjects.EnemyHeroes.Any())
                {
                comboMenu.AddGroupLabel("额外 R 设置");
                foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(o => o.IsEnemy))
                {
                    comboMenu.Add("RCast" + enemy.ChampionName, new CheckBox("用于 " + enemy.ChampionName, false));
                }
            }

            hybridMenu = config.AddSubMenu("混合", "Hybrid");
            hybridMenu.Add("Mode", new ComboBox("模式", 0, "W-E-Q", "E-Q", "Q"));
            hybridMenu.AddGroupLabel("自动 Q 设置 (英雄)");
            hybridMenu.Add("AutoQ", new KeyBind("按键", false, KeyBind.BindTypes.PressToggle, 'T'));
            hybridMenu.Add("AutoQMpA", new Slider("如果能量 >=", 100, 0, 200));
            hybridMenu.AddGroupLabel("自动 E 设置 (英雄/影子)");
            hybridMenu.Add("AutoE", new CheckBox("自动", false));

            lhMenu = config.AddSubMenu("LastHit", "尾兵");
            lhMenu.Add("Q", new CheckBox("使用 Q"));

            ksMenu = config.AddSubMenu("KillSteal", "抢头");
            ksMenu.Add("Q", new CheckBox("使用 Q"));
            ksMenu.Add("E", new CheckBox("使用 E"));

            if (GameObjects.EnemyHeroes.Any())
            {
                Evade.Init();
            }

            drawMenu = config.AddSubMenu("线圈", "Draw");
            drawMenu.Add("Q", new CheckBox("Q 范围", false));
            drawMenu.Add("W", new CheckBox("W 范围", false));
            drawMenu.Add("E", new CheckBox("E 范围", false));
            drawMenu.Add("R", new CheckBox("R 范围", false));
            drawMenu.Add("RStop", new CheckBox("防止 Q/W/E 范围", false));
            drawMenu.Add("UseR", new CheckBox("连招R 状态"));
            drawMenu.Add("Target", new CheckBox("目标"));
            drawMenu.Add("DMark", new CheckBox("死亡标记"));
            drawMenu.Add("WPos", new CheckBox("W 影子"));
            drawMenu.Add("RPos", new CheckBox("R 影子"));

            miscMenu = config.AddSubMenu("Misc", "杂项");
            miscMenu.Add("FleeW", new KeyBind("使用 W 逃跑", false, KeyBind.BindTypes.HoldActive, 'C'));

            Evade.Evading += Evading;
            Evade.TryEvading += TryEvading;
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
            {
                if (!sender.IsMe)
                {
                    return;
                }
                if (args.Slot == SpellSlot.W && args.SData.Name == "ZedW")
                {
                    rCasted = false;
                    wCasted = true;
                }
                else if (args.Slot == SpellSlot.R && args.SData.Name == "ZedR")
                {
                    wCasted = false;
                    rCasted = true;
                }
            };
            GameObject.OnCreate += (sender, args) =>
            {
                if (sender.IsEnemy)
                {
                    return;
                }
                var shadow = sender as Obj_AI_Minion;
                if (shadow == null || !shadow.IsAlly || shadow.CharData.BaseSkinName != "ZedUltMissile" || shadow.CharData.BaseSkinName != "ZedShadowDashMissile" || shadow.CharData.BaseSkinName != "zedshadow")
                {
                    return;
                }
                if (wCasted)
                {
                    wShadowT = Variables.TickCount;
                    wShadow = shadow;
                    wCasted = rCasted = false;
                }
                else if (rCasted)
                {
                    rShadowT = Variables.TickCount;
                    rShadow = shadow;
                    wCasted = rCasted = false;
                }
            };
            Obj_AI_Base.OnBuffGain += (sender, args) =>
            {
                if (sender.IsEnemy || !args.Buff.Caster.IsMe)
                {
                    return;
                }
                var shadow = sender as Obj_AI_Minion;
                if (shadow != null && shadow.IsAlly && shadow.BaseSkinName == "ZedShadow" && args.Buff.Caster.IsMe)
                {
                    switch (args.Buff.Name)
                    {
                        case "zedwshadowbuff":
                            if (!wShadow.Compare(shadow))
                            {
                                wShadowT = Variables.TickCount;
                                wShadow = shadow;
                            }
                            break;
                        case "zedrshadowbuff":
                            if (!rShadow.Compare(shadow))
                            {
                                rShadowT = Variables.TickCount;
                                rShadow = shadow;
                            }
                            break;
                    }
                }
            };
            Obj_AI_Base.OnPlayAnimation += (sender, args) =>
            {
                if (sender.IsMe || sender.IsEnemy || args.Animation != "Death")
                {
                    return;
                }
                if (sender.Compare(wShadow))
                {
                    wShadow = null;
                }
                else if (sender.Compare(rShadow))
                {
                    rShadow = null;
                }
            };
            GameObject.OnCreate += (sender, args) =>
            {
                var missile = sender as MissileClient;
                if (missile != null)
                {
                    if (missile.SpellCaster.IsMe && missile.SData.Name == "ZedWMissile")
                    {
                        wMissile = missile;
                    }
                    return;
                }
                if (sender.Name != "Zed_Base_R_buf_tell.troy")
                {
                    return;
                }
                var target = EntityManager.Heroes.Enemies.FirstOrDefault(i => i.LSIsValidTarget() && HaveR(i));
                if (target != null && target.Distance(sender) < 150)
                {
                    deathMark = sender;
                }
            };
            GameObject.OnDelete += (sender, args) =>
            {
                if (sender.Compare(wMissile))
                {
                    wMissile = null;
                }
                else if (sender.Compare(deathMark))
                {
                    deathMark = null;
                }
            };

        }

        #endregion

        #region Properties

        private static bool CanR
            =>
                getBoxItem(comboMenu, "RMode") == 0
                || (Q.IsReady(500) && Player.Mana >= Q.Instance.SData.Mana - 10)
                || (E.IsReady(500) && Player.Mana >= E.Instance.SData.Mana - 10);

        private static AIHeroClient GetTarget
        {
            get
            {
                var extraRange = RangeTarget;
                if (Q.IsReady())
                {
                    extraRange += Q.Width / 2;
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo)
                    && getKeyBindItem(comboMenu, "R") && RState == 0)
                {
                    var targetR = EntityManager.Heroes.Enemies.Where(i => i.IsInRange(Player, Q.Range + extraRange) && i.LSIsValidTarget()).OrderByDescending(i => TargetSelector.GetPriority(i)).ThenBy(i => i.DistanceToPlayer()).FirstOrDefault(i => getCheckBoxItem(comboMenu, "RCast" + i.NetworkId));

                    if (targetR != null)
                    {
                        return targetR;
                    }
                }
                var targets = EntityManager.Heroes.Enemies.Where(i => i.IsInRange(Player, Q.Range + extraRange) && i.LSIsValidTarget()).ToList();
                if (targets.Count == 0)
                {
                    return null;
                }
                var target = targets.FirstOrDefault(HaveR);
                return target != null
                           ? (IsKillByMark(target)
                                  ? (targets.FirstOrDefault(i => !i.Compare(target)) ?? target)
                                  : target)
                           : targets.FirstOrDefault();
            }
        }

        private static bool IsCastingW
            => !wShadow.LSIsValid() && wMissile != null && wMissile.Distance(wMissile.EndPosition) < 40;

        private static bool IsROne => R.Instance.SData.Name == "ZedR";

        private static bool IsWOne => W.Instance.SData.Name == "ZedW";

        private static float RangeTarget
        {
            get
            {
                var validW = wShadow.LSIsValid();
                var validR = rShadow.LSIsValid();
                var posW = validW ? wShadow.ServerPosition : new Vector3();
                if (!posW.IsValid() && IsCastingW)
                {
                    validW = true;
                    posW = wMissile.EndPosition;
                }
                return validW && validR
                           ? Math.Max(rShadow.DistanceToPlayer(), posW.DistanceToPlayer())
                           : (WState == 0 && Variables.TickCount - lastW > 150
                                  ? (validR ? Math.Max(rShadow.DistanceToPlayer(), W.Range) : W.Range)
                                  : (validW ? posW.DistanceToPlayer() : (validR ? rShadow.DistanceToPlayer() : 0)));
            }
        }

        private static bool RShadowCanQ
            => rShadow.LSIsValid() && Variables.TickCount - rShadowT <= 7500 - Q.Delay * 1000 + 50;

        private static int RState => R.IsReady() ? (IsROne ? 0 : 1) : (IsROne ? -1 : 2);

        private static bool WShadowCanQ
            => wShadow.LSIsValid() && Variables.TickCount - wShadowT <= 4500 - Q.Delay * 1000 + 50;

        private static int WState => W.IsReady() ? (IsWOne ? 0 : 1) : -1;

        #endregion

        #region Methods

        private static void AutoQ()
        {
            if (!Q.IsReady() || !getKeyBindItem(hybridMenu, "AutoQ") || Player.Mana < getSliderItem(hybridMenu, "AutoQMpA"))
            {
                return;
            }
            Q.CastingBestTarget(true, LeagueSharp.SDK.CollisionableObjects.YasuoWall);
        }

        private static SpellSlot CanW(AIHeroClient target)
        {
            if (E.IsReady() && Player.Mana >= E.Instance.SData.Mana + W.Instance.SData.Mana
                && target.DistanceToPlayer() < W.Range + E.Range - 20)
            {
                return SpellSlot.E;
            }
            if (Q.IsReady() && Player.Mana >= Q.Instance.SData.Mana + W.Instance.SData.Mana
                && target.DistanceToPlayer() < W.Range + Q.Range)
            {
                return SpellSlot.Q;
            }
            return SpellSlot.Unknown;
        }

        private static void CastE(bool onlyKill = false)
        {
            if (!E.IsReady())
            {
                return;
            }
            var targets = EntityManager.Heroes.Enemies.Where(i => i.IsInRange(Player, E.Range + 20 + RangeTarget) && i.LSIsValidTarget()).ToList();
            if (onlyKill)
            {
                targets = targets.Where(i => !IsKillByMark(i) && i.Health + i.AttackShield <= E.GetDamage(i)).ToList();
            }
            if (targets.Count == 0)
            {
                return;
            }
            if (targets.Any(IsInRangeE))
            {
                E.Cast();
            }
        }

        private static void CastQ(AIHeroClient target)
        {
            if (!Q.IsReady())
            {
                return;
            }
            var pred = Q.GetPrediction(target, true, -1, LeagueSharp.SDK.CollisionableObjects.YasuoWall);
            if (pred.Hitchance >= Q.MinHitChance)
            {
                Q.Cast(pred.CastPosition);
            }
            else
            {
                PredictionOutput predShadow = null;
                if (WShadowCanQ)
                {
                    Q2.UpdateSourcePosition(wShadow.ServerPosition, wShadow.ServerPosition);
                    predShadow = Q2.GetPrediction(target, true, -1, LeagueSharp.SDK.CollisionableObjects.YasuoWall);
                }
                else if (IsCastingW)
                {
                    Q2.UpdateSourcePosition(wMissile.EndPosition, wMissile.EndPosition);
                    predShadow = Q2.GetPrediction(target, true, -1, LeagueSharp.SDK.CollisionableObjects.YasuoWall);
                }
                if (predShadow != null && predShadow.Hitchance >= Q.MinHitChance)
                {
                    Q.Cast(predShadow.CastPosition);
                }
                else if (RShadowCanQ)
                {
                    Q2.UpdateSourcePosition(rShadow.ServerPosition, rShadow.ServerPosition);
                    predShadow = Q2.GetPrediction(target, true, -1, LeagueSharp.SDK.CollisionableObjects.YasuoWall);
                    if (predShadow.Hitchance >= Q.MinHitChance)
                    {
                        Q.Cast(predShadow.CastPosition);
                    }
                }
            }
        }

        private static bool CastQKill(LeagueSharp.SDK.Spell spell, Obj_AI_Base target)
        {
            var pred = spell.GetPrediction(target, false, -1, LeagueSharp.SDK.CollisionableObjects.YasuoWall);
            if (pred.Hitchance < Q.MinHitChance)
            {
                return false;
            }
            var col = spell.GetCollision(
                target,
                new List<Vector3> { pred.UnitPosition, target.Position },
                LeagueSharp.SDK.CollisionableObjects.Heroes | LeagueSharp.SDK.CollisionableObjects.Minions);
            if (col.Count == 0)
            {
                return Q.Cast(pred.CastPosition);
            }
            var subDmg = Q.GetDamage(target, DamageStage.SecondForm);
            switch (target.Type)
            {
                case GameObjectType.AIHeroClient:
                    return target.Health + target.AttackShield <= subDmg && Q.Cast(pred.CastPosition);
                case GameObjectType.obj_AI_Minion:
                    return spell.CanLastHit(target, subDmg) && Q.Cast(pred.CastPosition);
            }
            return false;
        }

        private static void CastW(AIHeroClient target, SpellSlot slot, bool isRCombo = false)
        {
            if (slot == SpellSlot.Unknown || Variables.TickCount - lastW <= 300)
            {
                return;
            }
            var posCast = W.GetPrediction(target).UnitPosition;
            var posStart = W.From;
            if (isRCombo)
            {
                var posEnd = rShadow.ServerPosition;
                if (posCast.Distance(posEnd) > Q.Range - 50)
                {
                    posEnd = posStart;
                }
                switch (getBoxItem(comboMenu, "WAdv"))
                {
                    case 1:
                        posCast = posStart + (posCast - posEnd).LSNormalized() * 500;
                        break;
                    case 2:
                        var subPos1 = posStart + (posCast - posEnd).LSNormalized().Perpendicular() * 500;
                        var subPos2 = posStart + (posEnd - posCast).LSNormalized().Perpendicular() * 500;
                        if (!subPos1.LSIsWall() && subPos2.LSIsWall())
                        {
                            posCast = subPos1;
                        }
                        else if (subPos1.LSIsWall() && !subPos2.LSIsWall())
                        {
                            posCast = subPos2;
                        }
                        else
                        {
                            posCast = subPos1.CountEnemyHeroesInRange(350) > subPos2.CountEnemyHeroesInRange(350)
                                          ? subPos1
                                          : subPos2;
                        }
                        break;
                    case 3:
                        posCast = Game.CursorPos;
                        break;
                }
            }
            else if (posCast.Distance(posStart) < E.Range * 2 - target.BoundingRadius)
            {
                posCast = posStart.LSExtend(posCast, 500);
            }
            if (W.Cast(posCast))
            {
                lastW = Variables.TickCount;
            }
        }

        private static void Combo()
        {
            var target = GetTarget;
            if (target != null)
            {
                Swap(target);
                var useR = getKeyBindItem(comboMenu, "R");
                var targetR = getCheckBoxItem(comboMenu, "RCast" + target.NetworkId);
                var stateR = RState;
                var canCast = !useR || !targetR
                              || (stateR == 0 && target.DistanceToPlayer() > getSliderItem(comboMenu, "RStopRange"))
                              || stateR == -1;
                if (stateR == 0 && useR && targetR && R.IsInRange(target) && CanR && R.CastOnUnit(target))
                {
                    return;
                }
                if (getCheckBoxItem(comboMenu, "Ignite") && Ignite.IsReady() && (HaveR(target) || target.HealthPercent < 25)
                    && target.DistanceToPlayer() < IgniteRange)
                {
                    Player.Spellbook.CastSpell(Ignite, target);
                }
                var norW = getCheckBoxItem(comboMenu, "WNormal");
                var advW = getBoxItem(comboMenu, "WAdv");
                if ((norW || advW > 0) && WState == 0)
                {
                    var slot = CanW(target);
                    if (slot != SpellSlot.Unknown)
                    {
                        if (advW > 0 && rShadow.LSIsValid() && useR && targetR && HaveR(target) && !IsKillByMark(target))
                        {
                            CastW(target, slot, true);
                            return;
                        }
                        if (norW)
                        {
                            if (stateR < 1 && canCast)
                            {
                                CastW(target, slot);
                            }
                            if (rShadow.LSIsValid() && useR && targetR && !HaveR(target))
                            {
                                CastW(target, slot);
                            }
                        }
                    }
                    else if (Variables.TickCount - lastW > 500
                             && target.Health + target.AttackShield <= Player.GetAutoAttackDamage(target)
                             && !E.IsInRange(target) && !IsKillByMark(target)
                             && target.DistanceToPlayer() < W.Range + target.GetRealAutoAttackRange() - 100
                             && W.Cast(
                                 target.ServerPosition.LSExtend(Player.ServerPosition, -target.GetRealAutoAttackRange())))
                    {
                        lastW = Variables.TickCount;
                        return;
                    }
                }
                if (canCast || rShadow.LSIsValid())
                {
                    CastE();
                    CastQ(target);
                }
            }
            if (getCheckBoxItem(comboMenu, "Items"))
            {
                UseItem(target);
            }
        }

        private static void Evading(Obj_AI_Base sender)
        {
            var skillshot = Evade.SkillshotAboutToHit(sender, 100).OrderByDescending(i => i.DangerLevel).ToList();
            if (skillshot.Count == 0)
            {
                return;
            }
            var zedW2 = EvadeSpellDatabase.Spells.FirstOrDefault(i => i.Enable && i.IsReady && i.Slot == SpellSlot.W);
            if (zedW2 != null && wShadow.LSIsValid() && !Evade.IsAboutToHit(wShadow, 30)
                && (!wShadow.IsUnderEnemyTurret() || getCheckBoxItem(Config.evadeMenu, zedW2.Name + "Tower"))
                && skillshot.Any(i => i.DangerLevel >= zedW2.DangerLevel) && W.Cast())
            {
                return;
            }
            var zedR2 =
                EvadeSpellDatabase.Spells.FirstOrDefault(
                    i => i.Enable && i.IsReady && i.Slot == SpellSlot.R && i.CheckSpellName == "zedr2");
            if (zedR2 != null && rShadow.LSIsValid() && !Evade.IsAboutToHit(rShadow, 30)
                && (!rShadow.IsUnderEnemyTurret() || getCheckBoxItem(Config.evadeMenu, zedR2.Name + "Tower"))
                && skillshot.Any(i => i.DangerLevel >= zedR2.DangerLevel))
            {
                R.Cast();
            }
        }

        private static List<double> GetCombo(AIHeroClient target, bool useQ, bool useW, bool useE)
        {
            var dmgTotal = 0d;
            var manaTotal = 0f;
            if (getCheckBoxItem(comboMenu, "Items"))
            {
                if (Bilgewater.IsReady())
                {
                    dmgTotal += Player.CalculateDamage(target, DamageType.Magical, 100);
                }
                if (BotRuinedKing.IsReady())
                {
                    dmgTotal += Player.CalculateDamage(
                        target,
                        DamageType.Physical,
                        Math.Max(target.MaxHealth * 0.1, 100));
                }
                if (Tiamat.IsReady() || Hydra.IsReady())
                {
                    dmgTotal += Player.CalculateDamage(target, DamageType.Physical, Player.TotalAttackDamage);
                }
                if (Titanic.IsReady())
                {
                    dmgTotal += Player.CalculateDamage(target, DamageType.Physical, 40 + 0.1f * Player.MaxHealth);
                }
            }
            if (useQ)
            {
                dmgTotal += Q.GetDamage(target);
                manaTotal += Q.Instance.SData.Mana;
            }
            if (useW)
            {
                if (useQ)
                {
                    dmgTotal += Q.GetDamage(target) / 2;
                }
                if (WState == 0)
                {
                    manaTotal += W.Instance.SData.Mana;
                }
            }
            if (useE)
            {
                dmgTotal += E.GetDamage(target);
                manaTotal += E.Instance.SData.Mana;
            }
            dmgTotal += Player.GetAutoAttackDamage(target) * 2;
            if (HaveR(target))
            {
                dmgTotal += Player.CalculateDamage(
                    target,
                    DamageType.Physical,
                    new[] { 0.25, 0.35, 0.45 }[R.Level - 1] * dmgTotal + Player.TotalAttackDamage);
            }
            return new List<double> { dmgTotal, manaTotal };
        }

        private static bool HaveR(AIHeroClient target)
        {
            return target.HasBuff("zedrtargetmark");
        }

        private static void Hybrid()
        {
            var target = GetTarget;
            if (target == null)
            {
                return;
            }
            var mode = getBoxItem(hybridMenu, "Mode");
            if (mode == 0 && WState == 0)
            {
                CastW(target, CanW(target));
            }
            if (mode < 2)
            {
                CastE();
            }
            CastQ(target);
        }

        private static bool IsInRangeE(AIHeroClient target)
        {
            var pos = E.GetPredPosition(target);
            return pos.DistanceToPlayer() < E.Range || (wShadow.LSIsValid() && wShadow.Distance(pos) < E.Range)
                   || (rShadow.LSIsValid() && rShadow.Distance(pos) < E.Range)
                   || (IsCastingW && wMissile.EndPosition.Distance(pos) < E.Range);
        }

        private static bool IsKillByMark(AIHeroClient target)
        {
            return HaveR(target) && deathMark != null && target.Distance(deathMark) < 150;
        }

        private static void KillSteal()
        {
            if (getCheckBoxItem(ksMenu, "Q") && Q.IsReady())
            {
                var targets = EntityManager.Heroes.Enemies.Where(i => i.IsInRange(Player, Q.Range + Q.Width / 2 + RangeTarget) && !IsKillByMark(i) && i.Health + i.AttackShield <= Q.GetDamage(i)).ToList();
                if (targets.Count > 0)
                {
                    foreach (var target in targets)
                    {
                        if (CastQKill(Q, target))
                        {
                            return;
                        }
                        if (WShadowCanQ)
                        {
                            Q3.UpdateSourcePosition(wShadow.ServerPosition, wShadow.ServerPosition);
                            if (CastQKill(Q3, target))
                            {
                                return;
                            }
                        }
                        else if (IsCastingW)
                        {
                            Q3.UpdateSourcePosition(wMissile.EndPosition, wMissile.EndPosition);
                            if (CastQKill(Q3, target))
                            {
                                return;
                            }
                        }
                        if (RShadowCanQ)
                        {
                            Q3.UpdateSourcePosition(rShadow.ServerPosition, rShadow.ServerPosition);
                            CastQKill(Q3, target);
                        }
                    }
                }
            }
            if (getCheckBoxItem(ksMenu, "E") && E.IsReady())
            {
                CastE(true);
            }
        }

        private static void LastHit()
        {
            if (!getCheckBoxItem(lhMenu, "Q") || !Q.IsReady() || Player.Spellbook.IsAutoAttacking)
            {
                return;
            }
            var minions =
                GameObjects.EnemyMinions.Where(
                    i => (i.IsMinion() || i.IsPet(false)) && i.LSIsValidTarget(Q.Range) && Q.CanLastHit(i, Q.GetDamage(i)))
                    .OrderByDescending(i => i.MaxHealth)
                    .ToList();
            if (minions.Count == 0)
            {
                return;
            }
            minions.ForEach(i => CastQKill(Q, i));
        }

        private static void OnDraw(EventArgs args)
        {
            if (Player.IsDead)
            {
                return;
            }
            if (getCheckBoxItem(drawMenu, "Q") && Q.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
            if (getCheckBoxItem(drawMenu, "W") && W.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
            if (getCheckBoxItem(drawMenu, "E") && E.Level > 0)
            {
                Render.Circle.DrawCircle(Player.Position, E.Range, E.IsReady() ? Color.LimeGreen : Color.IndianRed);
            }
            if (R.Level > 0)
            {
                var rMenu = getKeyBindItem(comboMenu, "R");
                if (RState == 0)
                {
                    if (getCheckBoxItem(drawMenu, "R"))
                    {
                        Render.Circle.DrawCircle(Player.Position, R.Range, Color.LimeGreen);
                    }
                    if (getCheckBoxItem(drawMenu, "RStop") && rMenu)
                    {
                        Render.Circle.DrawCircle(Player.Position, getSliderItem(comboMenu, "RStopRange"), Color.Orange);
                    }
                }
                if (getCheckBoxItem(drawMenu, "UseR"))
                {
                    var pos = Drawing.WorldToScreen(Player.Position);
                    var text = $"Use R In Combo: {(rMenu ? "On" : "Off")}";
                    Drawing.DrawText(
                        pos.X - (float)70 / 2,
                        pos.Y + 20,
                        rMenu ? Color.White : Color.Gray,
                        text);
                }
            }
            if (getCheckBoxItem(drawMenu, "Target"))
            {
                var target = GetTarget;
                if (target != null)
                {
                    Render.Circle.DrawCircle(target.Position, target.BoundingRadius * 1.5f, Color.Aqua);
                }
            }
            if (getCheckBoxItem(drawMenu, "DMark") && rShadow.LSIsValid())
            {
                var target = GameObjects.EnemyHeroes.FirstOrDefault(i => i.LSIsValidTarget() && IsKillByMark(i));
                if (target != null)
                {
                    var pos = Drawing.WorldToScreen(Player.Position);
                    var text = "Death Mark: " + target.ChampionName;
                    Drawing.DrawText(pos.X - (float)70 / 2, pos.Y + 40, Color.Red, text);
                }
            }
            if (getCheckBoxItem(drawMenu, "WPos") && wShadow.LSIsValid())
            {
                Render.Circle.DrawCircle(wShadow.Position, wShadow.BoundingRadius, Color.MediumSlateBlue);
                var pos = Drawing.WorldToScreen(wShadow.Position);
                Drawing.DrawText(pos.X - (float)30 / 2, pos.Y, Color.BlueViolet, "W");
            }
            if (getCheckBoxItem(drawMenu, "RPos") && rShadow.LSIsValid())
            {
                Render.Circle.DrawCircle(rShadow.Position, rShadow.BoundingRadius, Color.MediumSlateBlue);
                var pos = Drawing.WorldToScreen(rShadow.Position);
                Drawing.DrawText(pos.X - (float)30 / 2, pos.Y, Color.BlueViolet, "R");
            }
        }

        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || MenuGUI.IsChatOpen || Shop.IsOpen || Player.LSIsRecalling())
            {
                return;
            }
            KillSteal();

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Hybrid();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                if (getKeyBindItem(miscMenu, "FleeW"))
                {
                    Orbwalker.OrbwalkTo(Game.CursorPos);
                    if (WState == 0)
                    {
                        W.Cast(Game.CursorPos);
                    }
                    else if (WState == 1)
                    {
                        W.Cast();
                    }
                }
            }

            if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                if (getCheckBoxItem(hybridMenu, "AutoE"))
                {
                    CastE();
                }
                if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    AutoQ();
                }
            }
        }

        private static void Swap(AIHeroClient target)
        {
            var eCanKill = E.CanCast(target) && E.CanHitCircle(target)
                           && target.Health + target.AttackShield <= E.GetDamage(target);
            var markCanKill = IsKillByMark(target);
            if (getCheckBoxItem(comboMenu, "SwapIfKill") && (markCanKill || eCanKill))
            {
                SwapCountEnemy();
                return;
            }
            if (Player.HealthPercent < getSliderItem(comboMenu, "SwapIfHpU"))
            {
                if (markCanKill || !eCanKill || Player.HealthPercent < target.HealthPercent)
                {
                    SwapCountEnemy();
                }
            }
            else if (getBoxItem(comboMenu, "SwapGap") > 0 && !E.IsInRange(target) && !markCanKill)
            {
                var wDist = WState == 1 && wShadow.LSIsValid() ? wShadow.Distance(target) : float.MaxValue;
                var rDist = RState == 1 && rShadow.LSIsValid() ? rShadow.Distance(target) : float.MaxValue;
                var minDist = Math.Min(wDist, rDist);
                if (minDist.Equals(float.MaxValue) || target.DistanceToPlayer() <= minDist)
                {
                    return;
                }
                var swapByW = Math.Abs(minDist - wDist) < float.Epsilon;
                var swapByR = Math.Abs(minDist - rDist) < float.Epsilon;
                if (swapByW && minDist < R.Range && !R.IsInRange(target)
                    && getKeyBindItem(comboMenu, "R")
                    && getCheckBoxItem(comboMenu, "RCast" + target.NetworkId) && RState == 0 && CanR && W.Cast())
                {
                    return;
                }
                switch (getBoxItem(comboMenu, "SwapGap"))
                {
                    case 1:
                        if (IsInRangeE(target) && target.HealthPercent < 15 && Player.HealthPercent > 30
                            && (Q.IsReady() || E.IsReady()))
                        {
                            if (swapByW)
                            {
                                W.Cast();
                            }
                            else if (swapByR)
                            {
                                R.Cast();
                            }
                            return;
                        }
                        var combo = GetCombo(
                            target,
                            Q.IsReady() && minDist < Q.Range,
                            false,
                            E.IsReady() && minDist < E.Range);
                        if (minDist > target.GetRealAutoAttackRange())
                        {
                            combo[0] -= Player.GetAutoAttackDamage(target);
                        }
                        if (minDist > target.GetRealAutoAttackRange() + 100)
                        {
                            combo[0] -= Player.GetAutoAttackDamage(target);
                        }
                        if (target.Health + target.AttackShield > combo[0] || Player.Mana < combo[1])
                        {
                            return;
                        }
                        if (swapByW)
                        {
                            W.Cast();
                        }
                        else if (swapByR)
                        {
                            R.Cast();
                        }
                        break;
                    case 2:
                        if (minDist > 500)
                        {
                            return;
                        }
                        if (swapByW)
                        {
                            W.Cast();
                        }
                        else if (swapByR)
                        {
                            R.Cast();
                        }
                        break;
                }
            }
        }

        private static void SwapCountEnemy()
        {
            var wCount = WState == 1 && wShadow.LSIsValid() ? wShadow.CountEnemyHeroesInRange(400) : int.MaxValue;
            var rCount = RState == 1 && rShadow.LSIsValid() ? rShadow.CountEnemyHeroesInRange(400) : int.MaxValue;
            var minCount = Math.Min(rCount, wCount);
            if (minCount == int.MaxValue || Player.CountEnemyHeroesInRange(400) <= minCount)
            {
                return;
            }
            if (minCount == wCount)
            {
                W.Cast();
            }
            else if (minCount == rCount)
            {
                R.Cast();
            }
        }

        private static void TryEvading(List<Skillshot> hitBy, Vector2 to)
        {
            var dangerLevel = hitBy.Select(i => i.DangerLevel).Concat(new[] { 0 }).Max();
            var zedR1 =
                EvadeSpellDatabase.Spells.FirstOrDefault(
                    i =>
                    i.Enable && dangerLevel >= i.DangerLevel && i.IsReady && i.Slot == SpellSlot.R
                    && i.CheckSpellName == "zedr");
            var target =
                zedR1?.GetEvadeTargets(false, true)
                    .OrderByDescending(i => new Priority().GetDefaultPriority((AIHeroClient)i))
                    .ThenBy(i => i.CountEnemyHeroesInRange(400))
                    .FirstOrDefault();
            if (target != null)
            {
                R.CastOnUnit(target);
            }
        }

        private static void UseItem(AIHeroClient target)
        {
            if (target != null && (HaveR(target) || target.HealthPercent < 40 || Player.HealthPercent < 50))
            {
                if (Bilgewater.IsReady())
                {
                    Bilgewater.Cast(target);
                }
                if (BotRuinedKing.IsReady())
                {
                    BotRuinedKing.Cast(target);
                }
            }
            if (Youmuu.IsReady() && Player.CountEnemyHeroesInRange(R.Range + E.Range) > 0)
            {
                Youmuu.Cast();
            }
            if (Tiamat.IsReady() && Player.CountEnemyHeroesInRange(Tiamat.Range) > 0)
            {
                Tiamat.Cast();
            }
            if (Hydra.IsReady() && Player.CountEnemyHeroesInRange(Hydra.Range) > 0)
            {
                Hydra.Cast();
            }
            if (Titanic.IsReady() && !Player.Spellbook.IsAutoAttacking && Orbwalker.LastTarget != null)
            {
                Titanic.Cast();
            }
        }

        #endregion
    }
}