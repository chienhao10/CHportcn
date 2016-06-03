﻿// Copyright 2014 - 2014 Esk0r
// Collision.cs is part of Evade.
// 
// Evade is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Evade is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Evade. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using EloBuddy;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

#endregion

namespace EvadeSharp
{
    public enum CollisionObjectTypes
    {
        Minion,
        Champions,
        YasuoWall,
    }

    internal class FastPredResult
    {
        public Vector2 CurrentPos;
        public bool IsMoving;
        public Vector2 PredictedPos;
    }

    internal class DetectedCollision
    {
        public float Diff;
        public float Distance;
        public Vector2 Position;
        public CollisionObjectTypes Type;
        public Obj_AI_Base Unit;
    }

    internal static class Collision
    {
        private static int WallCastT;
        private static Vector2 YasuoWallCastedPos;

        public static void Init()
        {
            Obj_AI_Base.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
        }


        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsValid && sender.Team == ObjectManager.Player.Team && args.SData.Name == "YasuoWMovingWall")

            {
                WallCastT = Utils.TickCount;
                YasuoWallCastedPos = sender.ServerPosition.LSTo2D();
            }
        }

        public static FastPredResult FastPrediction(Vector2 from, Obj_AI_Base unit, int delay, int speed)
        {
            var tDelay = delay / 1000f + (from.LSDistance(unit) / speed);
            var d = tDelay * unit.MoveSpeed;
            var path = unit.GetWaypoints();

            if (path.PathLength() > d)
            {
                return new FastPredResult
                {
                    IsMoving = true,
                    CurrentPos = unit.ServerPosition.LSTo2D(),
                    PredictedPos = path.CutPath((int) d)[0],
                };
            }
            return new FastPredResult
            {
                IsMoving = false,
                CurrentPos = path[path.Count - 1],
                PredictedPos = path[path.Count - 1],
            };
        }

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

        public static Vector2 GetCollisionPoint(Skillshot skillshot)
        {
            var collisions = new List<DetectedCollision>();
            var from = skillshot.GetMissilePosition(0);
            skillshot.ForceDisabled = false;
            foreach (var cObject in skillshot.SpellData.CollisionObjects)
            {
                switch (cObject)
                {
                    case CollisionObjectTypes.Minion:

                        if (!getCheckBoxItem(Config.collision, "MinionCollision"))
                        {
                            break;
                        }
                        foreach (var minion in
                            MinionManager.GetMinions(
                                from.To3D(), 1200, MinionTypes.All,
                                skillshot.Unit.Team == ObjectManager.Player.Team
                                    ? MinionTeam.NotAlly
                                    : MinionTeam.NotAllyForEnemy))
                        {
                            var pred = FastPrediction(
                                from, minion,
                                Math.Max(0, skillshot.SpellData.Delay - (Utils.TickCount - skillshot.StartTick)),
                                skillshot.SpellData.MissileSpeed);
                            var pos = pred.PredictedPos;
                            var w = skillshot.SpellData.RawRadius + (!pred.IsMoving ? (minion.BoundingRadius - 15) : 0) -
                                    pos.LSDistance(from, skillshot.End, true);
                            if (w > 0)
                            {
                                collisions.Add(
                                    new DetectedCollision
                                    {
                                        Position =
                                            pos.LSProjectOn(skillshot.End, skillshot.Start).LinePoint +
                                            skillshot.Direction * 30,
                                        Unit = minion,
                                        Type = CollisionObjectTypes.Minion,
                                        Distance = pos.LSDistance(from),
                                        Diff = w,
                                    });
                            }
                        }

                        break;

                    case CollisionObjectTypes.Champions:
                        if (!getCheckBoxItem(Config.collision, "HeroCollision"))
                        {
                            break;
                        }
                        foreach (var hero in
                            ObjectManager.Get<AIHeroClient>()
                                .Where(
                                    h =>
                                        (h.LSIsValidTarget(1200, false) && h.Team == ObjectManager.Player.Team && !h.IsMe ||
                                         Config.TestOnAllies && h.Team != ObjectManager.Player.Team)))
                        {
                            var pred = FastPrediction(
                                from, hero,
                                Math.Max(0, skillshot.SpellData.Delay - (Utils.TickCount - skillshot.StartTick)),
                                skillshot.SpellData.MissileSpeed);
                            var pos = pred.PredictedPos;

                            var w = skillshot.SpellData.RawRadius + 30 - pos.LSDistance(from, skillshot.End, true);
                            if (w > 0)
                            {
                                collisions.Add(
                                    new DetectedCollision
                                    {
                                        Position =
                                            pos.LSProjectOn(skillshot.End, skillshot.Start).LinePoint +
                                            skillshot.Direction * 30,
                                        Unit = hero,
                                        Type = CollisionObjectTypes.Minion,
                                        Distance = pos.LSDistance(from),
                                        Diff = w,
                                    });
                            }
                        }
                        break;

                    case CollisionObjectTypes.YasuoWall:
                        if (!getCheckBoxItem(Config.collision, "YasuoCollision"))
                        {
                            break;
                        }
                        if (
                            !ObjectManager.Get<AIHeroClient>()
                                .Any(
                                    hero =>
                                        hero.LSIsValidTarget(float.MaxValue, false) &&
                                        hero.Team == ObjectManager.Player.Team && hero.ChampionName == "Yasuo"))
                        {
                            break;
                        }
                        GameObject wall = null;
                        foreach (var gameObject in ObjectManager.Get<GameObject>())
                        {
                            if (gameObject.IsValid &&
                                System.Text.RegularExpressions.Regex.IsMatch(
                                    gameObject.Name, "_w_windwall.\\.troy",
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                            {
                                wall = gameObject;
                            }
                        }
                        if (wall == null)
                        {
                            break;
                        }
                        var level = wall.Name.Substring(wall.Name.Length - 6, 1);
                        var wallWidth = (300 + 50 * Convert.ToInt32(level));


                        var wallDirection = (wall.Position.LSTo2D() - YasuoWallCastedPos).LSNormalized().LSPerpendicular();
                        var wallStart = wall.Position.LSTo2D() + wallWidth / 2 * wallDirection;
                        var wallEnd = wallStart - wallWidth * wallDirection;
                        var wallPolygon = new Geometry.Rectangle(wallStart, wallEnd, 75).ToPolygon();
                        var intersection = new Vector2();
                        var intersections = new List<Vector2>();

                        for (var i = 0; i < wallPolygon.Points.Count; i++)
                        {
                            var inter =
                                wallPolygon.Points[i].LSIntersection(
                                    wallPolygon.Points[i != wallPolygon.Points.Count - 1 ? i + 1 : 0], from,
                                    skillshot.End);
                            if (inter.Intersects)
                            {
                                intersections.Add(inter.Point);
                            }
                        }

                        if (intersections.Count > 0)
                        {
                            intersection = intersections.OrderBy(item => item.LSDistance(from)).ToList()[0];
                            var collisionT = Utils.TickCount +
                                             Math.Max(
                                                 0,
                                                 skillshot.SpellData.Delay -
                                                 (Utils.TickCount - skillshot.StartTick)) + 100 +
                                             (1000 * intersection.LSDistance(from)) / skillshot.SpellData.MissileSpeed;
                            if (collisionT - WallCastT < 4000)
                            {
                                if (skillshot.SpellData.Type != SkillShotType.SkillshotMissileLine)
                                {
                                    skillshot.ForceDisabled = true;
                                }
                                return intersection;
                            }
                        }

                        break;
                }
            }

            Vector2 result;
            if (collisions.Count > 0)
            {
                result = collisions.OrderBy(c => c.Distance).ToList()[0].Position;
            }
            else
            {
                result = new Vector2();
            }

            return result;
        }
    }
}