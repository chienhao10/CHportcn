﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using EloBuddy;

namespace ezEvade
{
    public enum EvadeOrderCommand
    {
        None,
        MoveTo,
        Attack,
        CastSpell
    }

    class EvadeCommand
    {
        private static AIHeroClient myHero { get { return ObjectManager.Player; } }

        public EvadeOrderCommand order;
        public Vector2 targetPosition;
        public Obj_AI_Base target;
        public float timestamp;
        public bool isProcessed;
        public EvadeSpellData evadeSpellData;

        public EvadeCommand()
        {
            this.timestamp = EvadeUtils.TickCount;
            this.isProcessed = false;
        }

        public static void MoveTo(Vector2 movePos)
        {
            if (!Situation.ShouldDodge())
            {
                return;
            }

            Evade.lastEvadeCommand = new EvadeCommand
            {
                order = EvadeOrderCommand.MoveTo,
                targetPosition = movePos,
                timestamp = EvadeUtils.TickCount,
                isProcessed = false
            };

            Evade.lastMoveToPosition = movePos;
            Evade.lastMoveToServerPos = myHero.ServerPosition.LSTo2D();

            Player.IssueOrder(GameObjectOrder.MoveTo, movePos.To3D(), false);
        }

        public static void Attack(EvadeSpellData spellData, Obj_AI_Base target)
        {
            EvadeSpell.lastSpellEvadeCommand = new EvadeCommand
            {
                order = EvadeOrderCommand.Attack,
                target = target,
                evadeSpellData = spellData,
                timestamp = EvadeUtils.TickCount,
                isProcessed = false
            };

            Player.IssueOrder(GameObjectOrder.AttackUnit, target, false);
        }

        public static void CastSpell(EvadeSpellData spellData, Obj_AI_Base target)
        {
            EvadeSpell.lastSpellEvadeCommand = new EvadeCommand
            {
                order = EvadeOrderCommand.CastSpell,
                target = target,
                evadeSpellData = spellData,
                timestamp = EvadeUtils.TickCount,
                isProcessed = false
            };

            myHero.Spellbook.CastSpell(spellData.spellKey, target, false);
        }

        public static void CastSpell(EvadeSpellData spellData, Vector2 movePos)
        {
            EvadeSpell.lastSpellEvadeCommand = new EvadeCommand
            {
                order = EvadeOrderCommand.CastSpell,
                targetPosition = movePos,
                evadeSpellData = spellData,
                timestamp = EvadeUtils.TickCount,
                isProcessed = false
            };

            myHero.Spellbook.CastSpell(spellData.spellKey, movePos.To3D(), false);
        }

        public static void CastSpell(EvadeSpellData spellData)
        {
            EvadeSpell.lastSpellEvadeCommand = new EvadeCommand
            {
                order = EvadeOrderCommand.CastSpell,
                evadeSpellData = spellData,
                timestamp = EvadeUtils.TickCount,
                isProcessed = false
            };

            myHero.Spellbook.CastSpell(spellData.spellKey,false);
        }
    }
}
