﻿#region LICENSE

/*
 Copyright 2014 - 2014 LeagueSharp
 HealthPrediction.cs is part of LeagueSharp.Common.
 
 LeagueSharp.Common is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 LeagueSharp.Common is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with LeagueSharp.Common. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

#region

using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
//using EloBuddy.SDK;

#endregion

namespace LeagueSharp.Common
{
    /// <summary>
    ///     This class allows you to calculate the health of units after a set time. Only works on minions and only taking into
    ///     account the auto-attack damage.
    /// </summary>
    public class HealthPrediction
    {
        /// <summary>
        ///     The active attacks
        /// </summary>
        private static readonly Dictionary<int, PredictedDamage> ActiveAttacks = new Dictionary<int, PredictedDamage>();

        /// <summary>
        ///     Initializes static members of the <see cref="HealthPrediction" /> class.
        /// </summary>
        static HealthPrediction()
        {
            Obj_AI_Base.OnProcessSpellCast += ObjAiBaseOnOnProcessSpellCast;
            Game.OnUpdate += Game_OnGameUpdate;
            Spellbook.OnStopCast += SpellbookOnStopCast;
            GameObject.OnDelete += MissileClient_OnDelete;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnDoCast;
        }

        /// <summary>
        ///     Fired when a unit does an auto attack.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        private static void Obj_AI_Base_OnDoCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (ActiveAttacks.ContainsKey(sender.NetworkId) && sender.IsMelee)
            {
                ActiveAttacks[sender.NetworkId].Processed = true;
            }
        }

        /// <summary>
        ///     Fired when a <see cref="MissileClient" /> is deleted from the game.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void MissileClient_OnDelete(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;
            if (missile != null && missile.SpellCaster != null)
            {
                var casterNetworkId = missile.SpellCaster.NetworkId;
                foreach (var activeAttack in ActiveAttacks)
                {
                    if (activeAttack.Key == casterNetworkId)
                    {
                        ActiveAttacks[casterNetworkId].Processed = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Fired when the game is updated.
        /// </summary>
        /// <param name="args">The <see cref="EventArgs" /> instance containing the event data.</param>
        private static void Game_OnGameUpdate(EventArgs args)
        {
            ActiveAttacks.ToList()
                .Where(pair => pair.Value.StartTick < Utils.GameTimeTickCount - 3000)
                .ToList()
                .ForEach(pair => ActiveAttacks.Remove(pair.Key));
        }

        /// <summary>
        ///     Fired when the spellbooks stops a cast.
        /// </summary>
        /// <param name="spellbook">The spellbook.</param>
        /// <param name="args">The <see cref="SpellbookStopCastEventArgs" /> instance containing the event data.</param>
        private static void SpellbookOnStopCast(Obj_AI_Base sender, SpellbookStopCastEventArgs args)
        {
            if (sender.IsValid && args.StopAnimation)
            {
                if (ActiveAttacks.ContainsKey(sender.NetworkId))
                {
                    ActiveAttacks.Remove(sender.NetworkId);
                }
            }
        }

        /// <summary>
        ///     Fired when the game processes a spell cast.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="GameObjectProcessSpellCastEventArgs" /> instance containing the event data.</param>
        private static void ObjAiBaseOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.LSIsValidTarget(3000) || sender.Team != ObjectManager.Player.Team || sender is AIHeroClient
                || !Orbwalking.IsAutoAttack(args.SData.Name) || !(args.Target is Obj_AI_Base))
            {
                return;
            }

            var target = (Obj_AI_Base) args.Target;
            ActiveAttacks.Remove(sender.NetworkId);

            var attackData = new PredictedDamage(
                sender,
                target,
                Utils.GameTimeTickCount - Game.Ping/2,
                sender.AttackCastDelay*1000,
                sender.AttackDelay*1000 - (sender is Obj_AI_Turret ? 70 : 0),
                sender.IsMelee() ? int.MaxValue : (int) args.SData.MissileSpeed,
                (float)sender.LSGetAutoAttackDamage(target, true));
            ActiveAttacks.Add(sender.NetworkId, attackData);
        }

        /// <summary>
        ///     Returns the unit health after a set time milliseconds.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="time">The time.</param>
        /// <param name="delay">The delay.</param>
        /// <returns></returns>
        public static float GetHealthPrediction(Obj_AI_Base unit, int time, int delay = 70)
        {
            var predictedDamage = 0f;

            foreach (var attack in ActiveAttacks.Values)
            {
                var attackDamage = 0f;
                if (!attack.Processed && attack.Source.LSIsValidTarget(float.MaxValue) &&
                    attack.Target.LSIsValidTarget(float.MaxValue) && attack.Target.NetworkId == unit.NetworkId)
                {
                    var landTime = attack.StartTick + attack.Delay +
                                   1000*Math.Max(0, unit.LSDistance(attack.Source) - attack.Source.BoundingRadius)/
                                   attack.ProjectileSpeed + delay;

                    if ( /*Utils.GameTimeTickCount < landTime - delay &&*/ landTime < Utils.GameTimeTickCount + time)
                    {
                        attackDamage = attack.Damage;
                    }
                }

                predictedDamage += attackDamage;
            }

            return unit.Health - predictedDamage;
        }

        /// <summary>
        ///     Returns the unit health after time milliseconds assuming that the past auto-attacks are periodic.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="time">The time.</param>
        /// <param name="delay">The delay.</param>
        /// <returns></returns>
        public static float LaneClearHealthPrediction(Obj_AI_Base unit, int time, int delay = 70)
        {
            var predictedDamage = 0f;

            foreach (var attack in ActiveAttacks.Values)
            {
                var n = 0;
                if (Utils.GameTimeTickCount - 100 <= attack.StartTick + attack.AnimationTime &&
                    attack.Target.LSIsValidTarget(float.MaxValue) &&
                    attack.Source.LSIsValidTarget(float.MaxValue) && attack.Target.NetworkId == unit.NetworkId)
                {
                    var fromT = attack.StartTick;
                    var toT = Utils.GameTimeTickCount + time;

                    while (fromT < toT)
                    {
                        if (fromT >= Utils.GameTimeTickCount &&
                            (fromT + attack.Delay +
                             Math.Max(0, unit.LSDistance(attack.Source) - attack.Source.BoundingRadius)/
                             attack.ProjectileSpeed < toT))
                        {
                            n++;
                        }
                        fromT += (int) attack.AnimationTime;
                    }
                }
                predictedDamage += n*attack.Damage;
            }

            return unit.Health - predictedDamage;
        }

        /// <summary>
        ///     Determines whether the specified minion has minion aggro.
        /// </summary>
        /// <param name="minion">The minion.</param>
        /// <returns></returns>
        public static bool HasMinionAggro(Obj_AI_Minion minion)
        {
            return ActiveAttacks.Values.Any(m => m.Source is Obj_AI_Minion && m.Target.NetworkId == minion.NetworkId);
        }

        /// <summary>
        ///     Determines whether the specified minion has turret aggro.
        /// </summary>
        /// <param name="minion">The minion</param>
        /// <returns></returns>
        public static bool HasTurretAggro(Obj_AI_Minion minion)
        {
            return ActiveAttacks.Values.Any(m => m.Source is Obj_AI_Turret && m.Target.NetworkId == minion.NetworkId);
        }

        /// <summary>
        ///     Return the starttick of the attacking turret.
        /// </summary>
        /// <param name="minion"></param>
        /// <returns></returns>
        public static int TurretAggroStartTick(Obj_AI_Minion minion)
        {
            var ActiveTurret = ActiveAttacks.Values
                .FirstOrDefault(m => m.Source is Obj_AI_Turret && m.Target.NetworkId == minion.NetworkId);
            return ActiveTurret != null ? ActiveTurret.StartTick : 0;
        }

        /// <summary>
        ///     Return the Attacking turret.
        /// </summary>
        /// <param name="minion"></param>
        /// <returns></returns>
        public static Obj_AI_Base GetAggroTurret(Obj_AI_Minion minion)
        {
            var ActiveTurret = ActiveAttacks.Values
                .FirstOrDefault(m => m.Source is Obj_AI_Turret && m.Target.NetworkId == minion.NetworkId);
            return ActiveTurret != null ? ActiveTurret.Source : null;
        }

        /// <summary>
        ///     Represetns predicted damage.
        /// </summary>
        private class PredictedDamage
        {
            /// <summary>
            ///     The animation time
            /// </summary>
            public readonly float AnimationTime;

            /// <summary>
            ///     Initializes a new instance of the <see cref="PredictedDamage" /> class.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <param name="target">The target.</param>
            /// <param name="startTick">The start tick.</param>
            /// <param name="delay">The delay.</param>
            /// <param name="animationTime">The animation time.</param>
            /// <param name="projectileSpeed">The projectile speed.</param>
            /// <param name="damage">The damage.</param>
            public PredictedDamage(Obj_AI_Base source,
                Obj_AI_Base target,
                int startTick,
                float delay,
                float animationTime,
                int projectileSpeed,
                float damage)
            {
                Source = source;
                Target = target;
                StartTick = startTick;
                Delay = delay;
                ProjectileSpeed = projectileSpeed;
                Damage = damage;
                AnimationTime = animationTime;
            }

            /// <summary>
            ///     Gets or sets the damage.
            /// </summary>
            /// <value>
            ///     The damage.
            /// </value>
            public float Damage { get; private set; }

            /// <summary>
            ///     Gets or sets the delay.
            /// </summary>
            /// <value>
            ///     The delay.
            /// </value>
            public float Delay { get; private set; }

            /// <summary>
            ///     Gets or sets the projectile speed.
            /// </summary>
            /// <value>
            ///     The projectile speed.
            /// </value>
            public int ProjectileSpeed { get; private set; }

            /// <summary>
            ///     Gets or sets the source.
            /// </summary>
            /// <value>
            ///     The source.
            /// </value>
            public Obj_AI_Base Source { get; private set; }

            /// <summary>
            ///     Gets or sets the start tick.
            /// </summary>
            /// <value>
            ///     The start tick.
            /// </value>
            public int StartTick { get; internal set; }

            /// <summary>
            ///     Gets or sets the target.
            /// </summary>
            /// <value>
            ///     The target.
            /// </value>
            public Obj_AI_Base Target { get; private set; }

            /// <summary>
            ///     Gets or sets a value indicating whether this <see cref="PredictedDamage" /> is processed.
            /// </summary>
            /// <value>
            ///     <c>true</c> if processed; otherwise, <c>false</c>.
            /// </value>
            public bool Processed { get; internal set; }
        }
    }
}