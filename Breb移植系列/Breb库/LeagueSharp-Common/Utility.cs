﻿#region LICENSE

/*
 Copyright 2014 - 2015 LeagueSharp
 Utility.cs is part of LeagueSharp.Common.
 
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
using EloBuddy.SDK;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace LeagueSharp.Common
{

    /// <summary>
    ///     Game functions related utilities.
    /// </summary>
    public static class Utility
    {
        public enum FountainType
        {
            OwnFountain,
            EnemyFountain
        }

        /// <summary>
        ///     Returns if the source is facing the target.
        /// </summary>
        public static bool LSIsFacing(this Obj_AI_Base source, Obj_AI_Base target)
        {
            if (source == null || target == null)
            {
                return false;
            }

            const float angle = 90;
            return source.Direction.LSTo2D().LSPerpendicular().LSAngleBetween((target.Position - source.Position).LSTo2D()) <
                   angle;
        }

        /// <summary>
        ///     Will return real time spell cooldown
        /// </summary>
        /// <param name="hero"></param>
        /// <param name="spell"></param>
        /// <returns></returns>
        public static float GetSpellCooldownEx(this AIHeroClient hero, SpellSlot spell)
        {
            var expire = hero.Spellbook.GetSpell(spell).CooldownExpires;
            var cd = expire - (Game.Time - 1);

            return cd <= 0 ? 0 : cd;
        }

        /// <summary>
        ///     Returns if both source and target are Facing Themselves.
        /// </summary>
        public static bool IsBothFacing(Obj_AI_Base source, Obj_AI_Base target)
        {
            return source.IsFacing(target) && target.IsFacing(source);
        }

        /// <summary>
        ///     Returns if the target is valid (not dead, targetable, visible...).
        /// </summary>
        public static bool LSIsValidTarget(this AttackableUnit unit,
            float range = float.MaxValue,
            bool checkTeam = true,
            Vector3 from = new Vector3())
        {
            if (unit == null || !unit.IsValid || unit.IsDead || !unit.IsVisible || !unit.IsTargetable ||
                unit.IsInvulnerable || unit.Name == "WardCorpse")
            {
                return false;
            }

            var @base = unit as Obj_AI_Base;
            if (@base != null)
            {
                if (@base.HasBuff("kindredrnodeathbuff") && @base.HealthPercent <= 10)
                {
                    return false;
                }
            }

            if (checkTeam && unit.Team == HeroManager.Player.Team)
            {
                return false;
            }

            var unitPosition = @base != null ? @base.ServerPosition : unit.Position;

            return !(range < float.MaxValue) ||
                   !(Vector2.DistanceSquared(
                       (@from.LSTo2D().IsValid() ? @from : HeroManager.Player.ServerPosition).LSTo2D(),
                       unitPosition.LSTo2D()) > range * range);
        }

        public static SpellDataInst GetSpell(this AIHeroClient hero, SpellSlot slot)
        {
            return hero.Spellbook.GetSpell(slot);
        }

        /// <summary>
        ///     Returns if the unit's movement is impaired (Slows, Taunts, Charms, Taunts, Snares, Fear)
        /// </summary>
        public static bool IsMovementImpaired(this AIHeroClient hero)
        {
            return hero.HasBuffOfType(BuffType.Flee) || hero.HasBuffOfType(BuffType.Charm) ||
                   hero.HasBuffOfType(BuffType.Slow)
                   || hero.HasBuffOfType(BuffType.Snare) || hero.HasBuffOfType(BuffType.Stun)
                   || hero.HasBuffOfType(BuffType.Taunt);
        }

        /// <summary>
        ///     Returns if the spell is ready to use.
        /// </summary>
        public static bool IsReady(this SpellDataInst spell, int t = 0)
        {
            return spell != null && spell.Slot != SpellSlot.Unknown && t == 0
                ? spell.State == SpellState.Ready
                : spell.State == SpellState.Ready
                  || (spell.State == SpellState.Cooldown && spell.CooldownExpires - Game.Time <= t / 1000f);
        }

        /// <summary>
        ///     Returns if the spell is ready to use.
        /// </summary>
        public static bool IsReady(this Spell spell, int t = 0)
        {
            return IsReady(spell.Instance, t);
        }

        /// <summary>
        ///     Returns if the spell is ready to use.
        /// </summary>
        public static bool IsReady(this SpellSlot slot, int t = 0)
        {
            var s = HeroManager.Player.Spellbook.GetSpell(slot);
            return s != null && IsReady(s, t);
        }

        /// <summary>
        ///     Returns if the GameObject is valid
        /// </summary>
        public static bool IsValid<T>(this GameObject obj) where T : GameObject
        {
            return obj is T && obj.IsValid;
        }

        /// <summary>
        ///     Returns if the SpellSlot of the InventorySlot is valid
        /// </summary>
        public static bool IsValidSlot(this InventorySlot slot)
        {
            return slot != null && slot.SpellSlot != SpellSlot.Unknown;
        }

        /// <summary>
        ///     Returns the unit's ability power
        /// </summary>
        [Obsolete("Use TotalMagicalDamage attribute.", false)]
        public static float AbilityPower(this Obj_AI_Base @base)
        {
            return @base.FlatMagicDamageMod + @base.PercentMagicDamageMod * @base.FlatMagicDamageMod;
        }

        /// <summary>
        ///     Returns the unit's health percentage (From 0 to 100).
        /// </summary>
        [Obsolete("Use HealthPercent attribute.", false)]
        public static float HealthPercentage(this Obj_AI_Base unit)
        {
            return unit.HealthPercent;
        }

        /// <summary>
        ///     Returns the unit's mana percentage (From 0 to 100).
        /// </summary>
        [Obsolete("Use ManaPercent attribute.", false)]
        public static float ManaPercentage(this Obj_AI_Base unit)
        {
            return unit.ManaPercent;
        }

        [Obsolete("Use TotalMagicalDamage from Leaguesharp.Core.", false)]
        public static float TotalMagicalDamage(this AIHeroClient target)
        {
            return target.TotalMagicalDamage;
        }

        [Obsolete("Use TotalAttackDamage attribute from LeagueSharp.Core", false)]
        public static float TotalAttackDamage(this AIHeroClient target)
        {
            return target.TotalAttackDamage;
        }

        /// <summary>
        ///     Checks if the unit is a Hero or Champion
        /// </summary>
        public static bool IsChampion(this Obj_AI_Base unit)
        {
            var hero = unit as AIHeroClient;
            return hero != null && hero.IsValid;
        }

        /// <summary>
        ///     Checks if this unit is the same as the given champion name
        /// </summary>
        public static bool IsChampion(this Obj_AI_Base unit, string championName)
        {
            var hero = unit as AIHeroClient;
            return hero != null && hero.IsValid && hero.ChampionName.Equals(championName);
        }

        /// <summary>
        ///     Checks if the unit casting recall
        /// </summary>
        public static bool LSIsRecalling(this AIHeroClient unit)
        {
            return unit.Buffs.Any(buff => buff.Name.ToLower().Contains("recall") && buff.Type == BuffType.Aura);
        }

        /// <summary>
        ///     Checks if the unit position is on screen
        /// </summary>
        public static bool LSIsOnScreen(this Vector3 position)
        {
            var pos = Drawing.WorldToScreen(position);
            return pos.X > 0 && pos.X <= Drawing.Width && pos.Y > 0 && pos.Y <= Drawing.Height;
        }

        /// <summary>
        ///     Checks if the unit position is on screen
        /// </summary>
        public static bool LSIsOnScreen(this Vector2 position)
        {
            return position.To3D().LSIsOnScreen();
        }

        /// <summary>
        ///     Randomizes the position with the supplied min/max
        /// </summary>
        public static Vector3 Randomize(this Vector3 position, int min, int max)
        {
            var ran = new Random(Utils.TickCount);
            return position + new Vector2(ran.Next(min, max), ran.Next(min, max)).To3D();
        }

        /// <summary>
        ///     Randomizes the position with the supplied min/max
        /// </summary>
        public static Vector2 Randomize(this Vector2 position, int min, int max)
        {
            return position.To3D().Randomize(min, max).LSTo2D();
        }

        /// <summary>
        ///     Checks if this spell is an autoattack
        /// </summary>
        public static bool IsAutoAttack(this SpellData spellData)
        {
            return Orbwalking.IsAutoAttack(spellData.Name);
        }

        /// <summary>
        ///     Checks if this spell is an autoattack
        /// </summary>
        public static bool IsAutoAttack(this SpellDataInst spellData)
        {
            return Orbwalking.IsAutoAttack(spellData.Name);
        }

        /// <summary>
        ///     Checks if this position is a wall using NavMesh
        /// </summary>
        public static bool LSIsWall(this Vector3 position)
        {
            return NavMesh.GetCollisionFlags(position).HasFlag(CollisionFlags.Wall);
        }

        /// <summary>
        ///     Checks if this position is a wall using NavMesh
        /// </summary>
        public static bool LSIsWall(this Vector2 position)
        {
            return position.To3D().LSIsWall();
        }

        /// <summary>
        ///     Checks if CastState is SuccessfullyCasted
        /// </summary>
        public static bool IsCasted(this Spell.CastStates state)
        {
            return state == Spell.CastStates.SuccessfullyCasted;
        }

        /// <summary>
        ///     Returns the recall duration
        /// </summary>
        public static int GetRecallTime(AIHeroClient obj)
        {
            return GetRecallTime(obj.Spellbook.GetSpell(SpellSlot.Recall).Name);
        }

        public static int GetRecallTime(string recallName)
        {
            var duration = 0;

            switch (recallName.ToLower())
            {
                case "recall":
                    duration = 8000;
                    break;
                case "recallimproved":
                    duration = 7000;
                    break;
                case "odinrecall":
                    duration = 4500;
                    break;
                case "odinrecallimproved":
                    duration = 4000;
                    break;
                case "superrecall":
                    duration = 4000;
                    break;
                case "superrecallimproved":
                    duration = 4000;
                    break;
            }
            return duration;
        }

        /// <summary>
        ///     Levels up a spell
        /// </summary>
        public static void LevelUpSpell(this Spellbook book, SpellSlot slot, bool evolve = false)
        {
            book.LevelSpell(slot);
        }

        public static List<Vector2> CutPath(this List<Vector2> path, float distance)
        {
            var result = new List<Vector2>();
            var Distance = distance;
            if (distance < 0)
            {
                path[0] = path[0] + distance * (path[1] - path[0]).LSNormalized();
                return path;
            }

            for (var i = 0; i < path.Count - 1; i++)
            {
                var dist = path[i].LSDistance(path[i + 1]);
                if (dist > Distance)
                {
                    result.Add(path[i] + Distance * (path[i + 1] - path[i]).LSNormalized());
                    for (var j = i + 1; j < path.Count; j++)
                    {
                        result.Add(path[j]);
                    }

                    break;
                }
                Distance -= dist;
            }
            return result.Count > 0 ? result : new List<Vector2> { path.Last() };
        }

        /// <summary>
        ///     Returns the path of the unit appending the ServerPosition at the start, works even if the unit just entered fow.
        /// </summary>
        public static List<Vector2> GetWaypoints(this Obj_AI_Base unit)
        {
            var result = new List<Vector2>();

            if (unit.IsVisible)
            {
                result.Add(unit.ServerPosition.LSTo2D());
                var path = unit.Path;
                if (path.Length > 0)
                {
                    var first = path[0].LSTo2D();
                    if (first.LSDistance(result[0], true) > 40)
                    {
                        result.Add(first);
                    }

                    for (var i = 1; i < path.Length; i++)
                    {
                        result.Add(path[i].LSTo2D());
                    }
                }
            }
            else if (WaypointTracker.StoredPaths.ContainsKey(unit.NetworkId))
            {
                var path = WaypointTracker.StoredPaths[unit.NetworkId];
                var timePassed = (Utils.TickCount - WaypointTracker.StoredTick[unit.NetworkId]) / 1000f;
                if (path.PathLength() >= unit.MoveSpeed * timePassed)
                {
                    result = CutPath(path, (int)(unit.MoveSpeed * timePassed));
                }
            }

            return result;
        }

        public static List<Vector2Time> GetWaypointsWithTime(this Obj_AI_Base unit)
        {
            var wp = unit.GetWaypoints();

            if (wp.Count < 1)
            {
                return null;
            }

            var result = new List<Vector2Time>();
            var speed = unit.MoveSpeed;
            var lastPoint = wp[0];
            var time = 0f;

            foreach (var point in wp)
            {
                time += point.LSDistance(lastPoint) / speed;
                result.Add(new Vector2Time(point, time));
                lastPoint = point;
            }

            return result;
        }

        /// <summary>
        ///     Returns true if the buff is active and didn't expire.
        /// </summary>
        public static bool IsValidBuff(this BuffInstance buff)
        {
            return buff.IsActive && buff.EndTime - Game.Time > 0;
        }

        /// <summary>
        ///     Returns if the unit has the specified buff in the indicated amount of time
        /// </summary>
        public static bool HasBuffIn(this Obj_AI_Base unit, string displayName, float tickCount, bool includePing = true)
        {
            return
                unit.Buffs.Any(
                    buff =>
                        buff.IsValid && buff.DisplayName == displayName &&
                        buff.EndTime - Game.Time > tickCount - (includePing ? Game.Ping / 2000f : 0));
        }

        /// <summary>
        ///     Returns if the unit has the buff and it is active
        /// </summary>
        [Obsolete("Use Obj_AI_Base.HasBuff")]
        public static bool LSHasBuff(this Obj_AI_Base unit,
            string buffName,
            bool dontUseDisplayName = false,
            bool kappa = true)
        {
            return
                unit.Buffs.Any(
                    buff =>
                        ((dontUseDisplayName &&
                          string.Equals(buff.Name, buffName, StringComparison.CurrentCultureIgnoreCase)) ||
                         (!dontUseDisplayName &&
                          string.Equals(buff.DisplayName, buffName, StringComparison.CurrentCultureIgnoreCase))) &&
                        buff.IsValidBuff());
        }

        /// <summary>
        ///     Returns the spell slot with the name.
        /// </summary>
        public static SpellSlot GetSpellSlot(this AIHeroClient unit, string name)
        {
            foreach (var spell in
                unit.Spellbook.Spells.Where(
                    spell => string.Equals(spell.Name, name, StringComparison.CurrentCultureIgnoreCase)))
            {
                return spell.Slot;
            }

            return SpellSlot.Unknown;
        }

        /// <summary>
        ///     Returns true if the unit is under tower range.
        /// </summary>
        public static bool UnderTurret(this Obj_AI_Base unit)
        {
            return UnderTurret(unit.Position, true);
        }

        /// <summary>
        ///     Returns true if the unit is under turret range.
        /// </summary>
        public static bool UnderTurret(this Obj_AI_Base unit, bool enemyTurretsOnly)
        {
            return UnderTurret(unit.Position, enemyTurretsOnly);
        }

        public static bool UnderTurret(this Vector3 position, bool enemyTurretsOnly)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.LSIsValidTarget(950, enemyTurretsOnly, position));
        }

        /// <summary>
        ///     Return true if unit is under ally turret range.
        ///     <returns></returns>
        public static bool UnderAllyTurret(this Obj_AI_Base unit)
        {
            return UnderAllyTurret(unit.Position);
        }

        public static bool UnderAllyTurret(this Vector3 position)
        {
            return
                ObjectManager.Get<Obj_AI_Turret>()
                    .Any(turret => turret.LSIsValidTarget(950, false, position) && turret.IsAlly);
        }

        public static NavMeshCell ToNavMeshCell(this Vector3 position)
        {
            var nav = NavMesh.WorldToGrid(position.X, position.Y);
            return NavMesh.GetCell((short)nav.X, (short)nav.Y);
        }

        [Obsolete("Use CountEnemiesInRange", false)]
        public static int LSCountEnemysInRange(this Obj_AI_Base unit, float range)
        {
            return unit.ServerPosition.LSCountEnemiesInRange(range);
        }

        [Obsolete("Use CountEnemiesInRange", false)]
        public static int LSCountEnemysInRange(this Vector3 point, float range)
        {
            return point.LSCountEnemiesInRange(range);
        }

        /// <summary>
        ///     Counts the enemies in range of Player.
        /// </summary>
        public static int LSCountEnemiesInRange(float range)
        {
            return HeroManager.Player.LSCountEnemiesInRange(range);
        }

        /// <summary>
        ///     Counts the enemies in range of Unit.
        /// </summary>
        public static int LSCountEnemiesInRange(this Obj_AI_Base unit, float range)
        {
            return unit.ServerPosition.LSCountEnemiesInRange(range);
        }

        /// <summary>
        ///     Counts the enemies in range of point.
        /// </summary>
        public static int LSCountEnemiesInRange(this Vector3 point, float range)
        {
            return HeroManager.Enemies.Count(h => h.LSIsValidTarget(range, true, point));
        }

        // Use same interface as CountEnemiesInRange
        /// <summary>
        ///     Count the allies in range of the Player.
        /// </summary>
        public static int CountAlliesInRange(float range)
        {
            return HeroManager.Player.CountAlliesInRange(range);
        }

        /// <summary>
        ///     Counts the allies in range of the Unit.
        /// </summary>
        public static int CountAlliesInRange(this Obj_AI_Base unit, float range)
        {
            return unit.ServerPosition.CountAlliesInRange(range, unit);
        }

        /// <summary>
        ///     Counts the allies in the range of the Point.
        /// </summary>
        public static int CountAlliesInRange(this Vector3 point, float range, Obj_AI_Base originalunit = null)
        {
            if (originalunit != null)
            {
                return HeroManager.Allies
                    .Count(x => x.NetworkId != originalunit.NetworkId && x.LSIsValidTarget(range, false, point));
            }
            return HeroManager.Allies
                .Count(x => x.LSIsValidTarget(range, false, point));
        }

        public static List<AIHeroClient> GetAlliesInRange(this Obj_AI_Base unit, float range)
        {
            return GetAlliesInRange(unit.ServerPosition, range, unit);
        }

        public static List<AIHeroClient> GetAlliesInRange(this Vector3 point, float range,
            Obj_AI_Base originalunit = null)
        {
            if (originalunit != null)
            {
                return
                    HeroManager.Allies
                        .FindAll(
                            x =>
                                x.NetworkId != originalunit.NetworkId &&
                                point.LSDistance(x.ServerPosition, true) <= range * range);
            }
            return
                HeroManager.Allies
                    .FindAll(x => point.LSDistance(x.ServerPosition, true) <= range * range);
        }

        public static List<AIHeroClient> GetEnemiesInRange(this Obj_AI_Base unit, float range)
        {
            return GetEnemiesInRange(unit.ServerPosition, range);
        }

        public static List<AIHeroClient> GetEnemiesInRange(this Vector3 point, float range)
        {
            return
                HeroManager.Enemies
                    .FindAll(x => point.LSDistance(x.ServerPosition, true) <= range * range);
        }

        public static List<T> GetObjects<T>(this Vector3 position, float range) where T : GameObject, new()
        {
            return ObjectManager.Get<T>().Where(x => position.LSDistance(x.Position, true) < range * range).ToList();
        }

        public static List<T> GetObjects<T>(string objectName, float range, Vector3 rangeCheckFrom = new Vector3())
            where T : GameObject, new()
        {
            if (rangeCheckFrom.Equals(Vector3.Zero))
            {
                rangeCheckFrom = HeroManager.Player.ServerPosition;
            }

            return ObjectManager.Get<T>().Where(x => rangeCheckFrom.LSDistance(x.Position, true) < range * range).ToList();
        }


        /// <summary>
        ///     Draws a "lag-free" circle
        /// </summary>
        public static void DrawCircle(Vector3 center,
            float radius,
            Color color,
            int thickness = 5,
            int quality = 30,
            bool onMinimap = false)
        {
            if (!onMinimap)
            {
                Render.Circle.DrawCircle(center, radius, color, thickness);
                return;
            }

            var pointList = new List<Vector3>();
            for (var i = 0; i < quality; i++)
            {
                var angle = i * Math.PI * 2 / quality;
                pointList.Add(
                    new Vector3(
                        center.X + radius * (float)Math.Cos(angle), center.Y + radius * (float)Math.Sin(angle),
                        center.Z));
            }

            for (var i = 0; i < pointList.Count; i++)
            {
                var a = pointList[i];
                var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];

                var aonScreen = Drawing.WorldToMinimap(a);
                var bonScreen = Drawing.WorldToMinimap(b);

                Drawing.DrawLine(aonScreen.X, aonScreen.Y, bonScreen.X, bonScreen.Y, thickness, color);
            }
        }

        /// <summary>
        ///     Returns true if unit is in shop range (range in which the shopping is allowed).
        /// </summary>
        /// <returns></returns>
        public static bool InShop(this Obj_AI_Base unit)
        {
            float fountainRange = 562500; //750 * 750
            var map = Map.GetMap();
            if (map != null && map.Type == Map.MapType.SummonersRift)
            {
                fountainRange = 1000000; //1000 * 1000
            }
            var fpos = unit.Team == HeroManager.Player.Team ? MiniCache.AllyFountain : MiniCache.EnemyFountain;
            return unit.IsVisible && unit.LSDistance(fpos, true) <= fountainRange;
        }

        /// <summary>
        ///     Returns true if unit is in fountain range (range in which fountain heals).
        ///     The second optional parameter allows you to indicate which fountain you want to check against.
        /// </summary>
        public static bool InFountain(this Obj_AI_Base unit, FountainType ftype = FountainType.OwnFountain)
        {
            float fountainRange = 562500; //750 * 750
            var map = Map.GetMap();
            if (map != null && map.Type == Map.MapType.SummonersRift)
            {
                fountainRange = 1210000; //1100 * 1100
            }

            var fpos = new Vector3();

            if (ftype == FountainType.OwnFountain)
            {
                fpos = unit.Team == HeroManager.Player.Team ? MiniCache.AllyFountain : MiniCache.EnemyFountain;
            }
            if (ftype == FountainType.EnemyFountain)
            {
                fpos = unit.Team == HeroManager.Player.Team ? MiniCache.EnemyFountain : MiniCache.AllyFountain;
            }

            return unit.IsVisible && unit.LSDistance(fpos, true) <= fountainRange;
        }

        /// <summary>
        ///     Checks a point to see if it is in an ally or enemy fountain
        /// </summary>
        public static bool InFountain(this Vector3 position, FountainType fountain)
        {
            return position.LSTo2D().InFountain(fountain);
        }

        /// <summary>
        ///     Checks a point to see if it is in an ally or enemy fountain
        /// </summary>
        public static bool InFountain(this Vector2 position, FountainType fountain)
        {
            float fountainRange = 562500; //750 * 750
            var map = Map.GetMap();
            if (map != null && map.Type == Map.MapType.SummonersRift)
            {
                fountainRange = 1210000; //1100 * 1100
            }
            var fpos = fountain == FountainType.OwnFountain ? MiniCache.AllyFountain : MiniCache.EnemyFountain;
            return position.LSDistance(fpos, true) <= fountainRange;
        }


        public static short GetPacketId(this GamePacketEventArgs gamePacketEventArgs)
        {
            var packetData = gamePacketEventArgs.PacketData;
            if (packetData.Length < 2)
            {
                return 0;
            }
            return (short)(packetData[0] + packetData[1] * 256);
        }

        public static void SendAsPacket(this byte[] packetData,
            PacketChannel channel = PacketChannel.C2S,
            PacketProtocolFlags protocolFlags = PacketProtocolFlags.Reliable)
        {
            Game.SendPacket(packetData, channel, protocolFlags);
        }

        public static void ProcessAsPacket(this byte[] packetData, PacketChannel channel = PacketChannel.S2C)
        {
            Game.ProcessPacket(packetData, channel);
        }

        public static class DelayAction
        {
            public delegate void Callback();

            public static List<Action> ActionList = new List<Action>();

            static DelayAction()
            {
                Game.OnUpdate += GameOnOnGameUpdate;
            }

            private static void GameOnOnGameUpdate(EventArgs args)
            {
                for (var i = ActionList.Count - 1; i >= 0; i--)
                {
                    if (ActionList[i].Time <= Utils.GameTimeTickCount)
                    {
                        try
                        {
                            if (ActionList[i].CallbackObject != null)
                            {
                                ActionList[i].CallbackObject();
                                //Will somehow result in calling ALL non-internal marked classes of the called assembly and causes NullReferenceExceptions.
                            }
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        ActionList.RemoveAt(i);
                    }
                }
            }

            public static void Add(int time, Callback func)
            {
                var action = new Action(time, func);
                ActionList.Add(action);
            }

            public struct Action
            {
                public Callback CallbackObject;
                public int Time;

                public Action(int time, Callback callback)
                {
                    Time = time + Utils.GameTimeTickCount;
                    CallbackObject = callback;
                }
            }
        }

        public static class HpBarDamageIndicator
        {
            public delegate float DamageToUnitDelegate(AIHeroClient hero);

            private const int XOffset = 10;
            private const int YOffset = 20;
            private const int Width = 103;
            private const int Height = 8;
            public static Color Color = Color.Lime;
            public static bool Enabled = true;
            private static DamageToUnitDelegate _damageToUnit;

            private static readonly Render.Text Text = new Render.Text(
                0, 0, string.Empty, 11, new ColorBGRA(255, 0, 0, 255), "monospace");

            public static DamageToUnitDelegate DamageToUnit
            {
                get { return _damageToUnit; }

                set
                {
                    if (_damageToUnit == null)
                    {
                        Drawing.OnDraw += Drawing_OnDraw;
                    }
                    _damageToUnit = value;
                }
            }

            private static void Drawing_OnDraw(EventArgs args)
            {
                if (!Enabled || _damageToUnit == null)
                {
                    return;
                }

                var height = Drawing.Height;
                var width = Drawing.Width;

                foreach (var unit in
                    HeroManager.Enemies.FindAll(h => h.IsValid && h.IsHPBarRendered))
                {
                    var barPos = unit.HPBarPosition;

                    if (barPos.X < -200 || barPos.X > width + 200)
                        continue;
                    if (barPos.Y < -200 || barPos.X > height + 200)
                        continue;

                    var damage = _damageToUnit(unit);
                    var percentHealthAfterDamage = Math.Max(0, unit.Health - damage) / unit.MaxHealth;
                    var xPos = barPos.X + XOffset + Width * percentHealthAfterDamage;

                    //if (damage > unit.Health)
                    {
                        Text.X = (int)barPos.X + XOffset;
                        Text.Y = (int)barPos.Y + YOffset - 13;
                        Text.text = ((int)(unit.Health - damage)).ToString();
                        Text.OnEndScene();
                    }

                    Drawing.DrawLine(xPos, barPos.Y + YOffset, xPos, barPos.Y + YOffset + Height, 2, Color);
                }
            }
        }

        public class MiniCache
        {
            private static VectorHolder _allySpawn, _enemySpawn;

            public static Vector3 AllyFountain
            {
                get
                {
                    if (_allySpawn != null) return _allySpawn.position;
                    _allySpawn = new VectorHolder(ObjectManager.Get<Obj_SpawnPoint>().Find(x => x.IsAlly).Position);
                    return _allySpawn.position;
                }
            }

            public static Vector3 EnemyFountain
            {
                get
                {
                    if (_enemySpawn != null) return _enemySpawn.position;
                    _enemySpawn = new VectorHolder(ObjectManager.Get<Obj_SpawnPoint>().Find(x => x.IsEnemy).Position);
                    return _enemySpawn.position;
                }
            }

            private class VectorHolder
            {
                public readonly Vector3 position;

                public VectorHolder(Vector3 position)
                {
                    this.position = position;
                }
            }
        }

        public class Map
        {
            public enum MapType
            {
                Unknown,
                SummonersRift,
                CrystalScar,
                TwistedTreeline,
                HowlingAbyss
            }

            private static readonly IDictionary<int, Map> MapById = new Dictionary<int, Map>
            {
                {
                    8,
                    new Map
                    {
                        Name = "The Crystal Scar",
                        ShortName = "crystalScar",
                        Type = MapType.CrystalScar,
                        Grid = new Vector2(13894f/2, 13218f/2),
                        StartingLevel = 3
                    }
                },
                {
                    10,
                    new Map
                    {
                        Name = "The Twisted Treeline",
                        ShortName = "twistedTreeline",
                        Type = MapType.TwistedTreeline,
                        Grid = new Vector2(15436f/2, 14474f/2),
                        StartingLevel = 1
                    }
                },
                {
                    11,
                    new Map
                    {
                        Name = "Summoner's Rift",
                        ShortName = "summonerRift",
                        Type = MapType.SummonersRift,
                        Grid = new Vector2(13982f/2, 14446f/2),
                        StartingLevel = 1
                    }
                },
                {
                    12,
                    new Map
                    {
                        Name = "Howling Abyss",
                        ShortName = "howlingAbyss",
                        Type = MapType.HowlingAbyss,
                        Grid = new Vector2(13120f/2, 12618f/2),
                        StartingLevel = 3
                    }
                }
            };

            private static Map _currentMap { get; set; }

            public MapType Type { get; private set; }
            public Vector2 Grid { get; private set; }
            public string Name { get; private set; }
            public string ShortName { get; private set; }
            public int StartingLevel { get; private set; }

            /// <summary>
            ///     Returns the current map.
            /// </summary>
            public static Map GetMap()
            {
                if (_currentMap != null)
                {
                    return _currentMap;
                }
                if (MapById.ContainsKey((int)Game.MapId))
                {
                    _currentMap = MapById[(int)Game.MapId];
                    return _currentMap;
                }

                return new Map
                {
                    Name = "Unknown",
                    ShortName = "unknown",
                    Type = MapType.Unknown,
                    Grid = new Vector2(0, 0),
                    StartingLevel = 1
                };
            }
        }

        /// <summary>
        ///     Internal class used to get the waypoints even when the enemy enters the fow of war.
        /// </summary>
        internal static class WaypointTracker
        {
            public static readonly Dictionary<int, List<Vector2>> StoredPaths = new Dictionary<int, List<Vector2>>();
            public static readonly Dictionary<int, int> StoredTick = new Dictionary<int, int>();
        }
    }

    public static class Version
    {
        public static int MajorVersion;
        public static int MinorVersion;
        public static int Build;
        public static int Revision;
        private static readonly int[] VersionArray;

        static Version()
        {
            var d = Game.Version.Split('.');
            MajorVersion = Convert.ToInt32(d[0]);
            MinorVersion = Convert.ToInt32(d[1]);
            Build = Convert.ToInt32(d[2]);
            Revision = Convert.ToInt32(d[3]);

            VersionArray = new[] { MajorVersion, MinorVersion, Build, Revision };
        }

        public static bool IsOlder(string version)
        {
            var d = version.Split('.');
            return MinorVersion < Convert.ToInt32(d[1]);
        }

        public static bool IsNewer(string version)
        {
            var d = version.Split('.');
            return MinorVersion > Convert.ToInt32(d[1]);
        }

        public static bool IsEqual(string version)
        {
            var d = version.Split('.');
            for (var i = 0; i <= d.Length; i++)
            {
                if (d[i] == null || Convert.ToInt32(d[i]) != VersionArray[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class Vector2Time
    {
        public Vector2 Position;
        public float Time;

        public Vector2Time(Vector2 pos, float time)
        {
            Position = pos;
            Time = time;
        }
    }
}