﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EloBuddy;
using LeagueSharp.Common;
using SharpDX;

namespace ezEvade
{
    class SpecialEvadeSpell
    {
        private static AIHeroClient myHero { get { return ObjectManager.Player; } }

        public static void LoadSpecialSpell(EvadeSpellData spellData)
        {
            if (spellData.spellName == "EkkoEAttack")
            {
                spellData.useSpellFunc = UseEkkoE2;
            }

            if (spellData.spellName == "EkkoR")
            {
                spellData.useSpellFunc = UseEkkoR;
            }

            if (spellData.spellName == "Pounce")
            {
                spellData.useSpellFunc = UsePounce;
            }

            if (spellData.spellName == "RivenTriCleave")
            {
                spellData.useSpellFunc = UseBrokenWings;
            }
        }

        public static bool UsePounce(EvadeSpellData evadeSpell, bool process = true)
        {
            if (myHero.CharData.BaseSkinName != "Nidalee")
            {
                var posInfo = EvadeHelper.GetBestPositionDash(evadeSpell);
                if (posInfo != null)
                {
                    EvadeSpell.CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell), process);
                    return true;
                }
            }

            return false;
        }

        public static bool UseBrokenWings(EvadeSpellData evadeSpell, bool process = false)
        {
            var posInfo = EvadeHelper.GetBestPositionDash(evadeSpell);
            if (posInfo != null)
            {
                EvadeCommand.MoveTo(posInfo.position);
                DelayAction.Add(50, () => EvadeSpell.CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell), process));
                return true;
            }

            return false;
        }


        public static bool UseEkkoE2(EvadeSpellData evadeSpell, bool process = true)
        {
            if (myHero.HasBuff("ekkoeattackbuff"))
            {
                var posInfo = EvadeHelper.GetBestPositionTargetedDash(evadeSpell);
                if (posInfo != null && posInfo.target != null)
                {
                    EvadeSpell.CastEvadeSpell(() => EvadeCommand.Attack(evadeSpell, posInfo.target), process);
                    //DelayAction.Add(50, () => myHero.IssueOrder(GameObjectOrder.MoveTo, posInfo.position.To3D()));
                    return true;
                }
            }

            return false;
        }

        public static bool UseEkkoR(EvadeSpellData evadeSpell, bool process = true)
        {
            foreach (var obj in ObjectManager.Get<Obj_AI_Minion>())
            {
                if (obj != null && obj.IsValid && !obj.IsDead && obj.Name == "Ekko" && obj.IsAlly)
                {
                    Vector2 blinkPos = obj.ServerPosition.LSTo2D();
                    if (!blinkPos.CheckDangerousPos(10))
                    {
                        EvadeSpell.CastEvadeSpell(() => EvadeCommand.CastSpell(evadeSpell), process);
                        //DelayAction.Add(50, () => myHero.IssueOrder(GameObjectOrder.MoveTo, posInfo.position.To3D()));
                        return true;
                    }

                }
            }

            return false;
        }
    }
}
