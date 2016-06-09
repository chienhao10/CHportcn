﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Leblanc.Common;
using SharpDX;
using EloBuddy;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK;

namespace Leblanc.Champion
{
    public static class PlayerSpells
    {
        public static List<LeagueSharp.Common.Spell> SpellList = new List<LeagueSharp.Common.Spell>();

        public static LeagueSharp.Common.Spell Q, W, E, R;

        public static LeagueSharp.Common.Spell Q2, W2, E2;

        public static void Init()
        {
            Q = new LeagueSharp.Common.Spell(SpellSlot.Q, 720);
            Q.SetTargetted(0.5f, 1500f);

            W = new LeagueSharp.Common.Spell(SpellSlot.W, 600 + 80);
            W.SetSkillshot(0.6f, 140f, 1450f, false, SkillshotType.SkillshotCircle);

            E = new LeagueSharp.Common.Spell(SpellSlot.E, 820);
            E.SetSkillshot(0.3f, 55f, 1650f, true, SkillshotType.SkillshotLine);

            R = new LeagueSharp.Common.Spell(SpellSlot.R);

            Q2 = new LeagueSharp.Common.Spell(SpellSlot.R, 660);
            Q2.SetTargetted(0.5f, 1500f);

            W2 = new LeagueSharp.Common.Spell(SpellSlot.R, 600 + 80);
            W2.SetSkillshot(0.6f, 180f, 1450f, false, SkillshotType.SkillshotCircle);

            E2 = new LeagueSharp.Common.Spell(SpellSlot.R, 820);
            E2.SetSkillshot(0.3f, 55f, 1650f, true, SkillshotType.SkillshotLine);

            SpellList.AddRange(new[] { Q, W, E });

            Spellbook.OnCastSpell += Spellbook_OnCastSpell;
        }

        public static bool IsReadyInt(this LeagueSharp.Common.Spell spell)
        {
            if (!R.IsReady())
            {
                return false;
            }

            return spell.IsReady();
        }

        private static int LastSeen(Obj_AI_Base t)
        {
            return Common.AutoBushHelper.EnemyInfo.Find(x => x.Player.NetworkId == t.NetworkId).LastSeenForE;
        }

        static void Spellbook_OnCastSpell(Spellbook sender, SpellbookCastSpellEventArgs args)
        {
            var hero = args.Target as AIHeroClient;
            if (hero != null)
            {
                var t = hero;
                if (!t.LSIsValidTarget(E.Range))
                {
                    return;
                }
                // args.Process = !t.HaveImmortalBuff();

                args.Process = Environment.TickCount - LastSeen(t) >= (Modes.ModeSettings.MenuLocal["Settings.SpellCast.VisibleDelay"].Cast<ComboBox>().CurrentValue + 1)*250;
            }
        }

        public static void CastQ(Obj_AI_Base t)
        {
            //if (t.HaveImmortalBuff())
            //{
            //    return;
            //}

            if (Q.CanCast(t))
            {
                Q.CastOnUnit(t);
            }
        }

        public static void CastQ2(Obj_AI_Base t)
        {
            //if (t.HaveImmortalBuff())
            //{
            //    return;
            //}

            if (CommonHelper.SpellRStatus == CommonHelper.SpellRName.R2xQ && Q2.CanCast(t))
            {
                Q2.CastOnUnit(t);
            }
        }
        public static void CastW(Vector3 t)
        {
            if (W.IsReady() && !W.StillJumped())
            {
                W.Cast(t);
            }
        }

        public static void CastW2(Vector3 t)
        {
            if (W2.IsReady() && !W2.StillJumped())
            {
                W2.Cast(t);
            }
        }

        public static void CastW(Obj_AI_Base t, bool returnBack = false)
        {
            if (W.CanCast(t) && !W.StillJumped())
            {
                W.Cast(t.Position);
            }

            if (returnBack && W.StillJumped() && !Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                W.Cast();
            }
        }

        public static void CastW2(Obj_AI_Base t)
        {
            //if (t.HaveImmortalBuff())
            //{
            //    return;
            //}

            if (CommonHelper.SpellRStatus == CommonHelper.SpellRName.R2xW && W2.CanCast(t) && !W2.StillJumped())
            {
                W2.Cast(t.Position);
            }
        }

        public static void CastE(Obj_AI_Base t)
        {
            if (E.IsReady() && t.LSIsValidTarget(Modes.ModeSettings.MaxERange))
            {
                HitChance[] hitChances = new[] { HitChance.VeryHigh, HitChance.High, HitChance.Medium, HitChance.Low };
                E.UpdateSourcePosition(ObjectManager.Player.ServerPosition, t.ServerPosition);
                if (E.GetPrediction(t).Hitchance >= hitChances[Modes.ModeSettings.EHitchance])
                {
                    E.Cast(t);
                }
            }
        }

        public static void CastE2(Obj_AI_Base t)
        {
            if (CommonHelper.SpellRStatus == CommonHelper.SpellRName.R2xE && E.IsReady() && t.LSIsValidTarget(Modes.ModeSettings.MaxERange))
            {
                HitChance[] hitChances = new[] { HitChance.VeryHigh, HitChance.High, HitChance.Medium, HitChance.Low};
                E.UpdateSourcePosition(ObjectManager.Player.ServerPosition, t.ServerPosition);
                if (E.GetPrediction(t).Hitchance >= hitChances[Modes.ModeSettings.EHitchance])
                {
                    E2.Cast(t);
                }

                //E2.UpdateSourcePosition(ObjectManager.Player.ServerPosition, t.ServerPosition);
                //if (E2.GetPrediction(t).Hitchance >= HitChance.High)
                //{
                //    E2.Cast(t);
                //}
            }
        }
    }

}
