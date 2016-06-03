﻿// Copyright 2014 - 2014 Esk0r
// SkillshotDetector.cs is part of Evade.
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
using System.Linq;
using System.Text.RegularExpressions;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using EloBuddy;

#endregion

namespace EvadeSharp
{
    internal static class SkillshotDetector
    {
        public delegate void OnDeleteMissileH(Skillshot skillshot, MissileClient missile);

        public delegate void OnDetectSkillshotH(Skillshot skillshot);

        static SkillshotDetector()
        {
            //Detect when the skillshots are created.
            //Game.OnProcessPacket += GameOnOnGameProcessPacket; // Used only for viktor's Laser :^)
            Obj_AI_Base.OnProcessSpellCast += ObjAiHeroOnOnProcessSpellCast;

            //Detect when projectiles collide.
            GameObject.OnDelete += ObjSpellMissileOnOnDelete;
            GameObject.OnCreate += ObjSpellMissileOnOnCreate;
            GameObject.OnCreate += GameObject_OnCreate; //TODO: Detect lux R and other large skillshots.
            GameObject.OnDelete += GameObject_OnDelete;

            if (Config.TestOnAllies && ObjectManager.Get<AIHeroClient>().Count() == 1)
            {
                Game.OnWndProc += Game_OnWndProc;
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg == (uint)WindowsMessages.WM_KEYUP)
            {
                TriggerOnDetectSkillshot(
                    DetectionType.ProcessSpell, SpellDatabase.GetByName("TestSkillShot"), Utils.TickCount,
                    Program.PlayerPosition, Game.CursorPos.LSTo2D(), Game.CursorPos.LSTo2D(), ObjectManager.Player);
            }
        }

        private static void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            /*if (ObjectManager.Player.LSDistance(sender.Position) < 1000)
            {
                Console.WriteLine(Utils.TickCount + " " + sender.Name + " " + sender.IsAlly + " " + sender.Type);
            }*/
            var spellData = SpellDatabase.GetBySourceObjectName(sender.Name);

            if (spellData == null)
            {
                return;
            }
            
            if (Config.skillShots["Enabled" + spellData.MenuItemName] == null)
            {
                return;
            }

            TriggerOnDetectSkillshot(DetectionType.ProcessSpell, spellData, Utils.TickCount - Game.Ping / 2, sender.Position.LSTo2D(), sender.Position.LSTo2D(), sender.Position.LSTo2D(), HeroManager.AllHeroes.MinOrDefault(h => h.IsAlly ? 1 : 0));
        }

        private static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (!sender.IsValid || !Config.TestOnAllies && sender.Team == ObjectManager.Player.Team)
            {
                return;
            }

            for (var i = Program.DetectedSkillshots.Count - 1; i >= 0; i--)
            {
                var skillshot = Program.DetectedSkillshots[i];
                if (skillshot.SpellData.ToggleParticleName != "" && new Regex(skillshot.SpellData.ToggleParticleName).IsMatch(sender.Name))
                {
                    Program.DetectedSkillshots.RemoveAt(i);
                }
            }
        }

        private static void ObjSpellMissileOnOnCreate(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;

            if (missile == null || !missile.IsValid)
            {
                return;
            }

            var unit = missile.SpellCaster as AIHeroClient;

            if (unit == null || !unit.IsValid || (unit.Team == ObjectManager.Player.Team && !Config.TestOnAllies))
            {
                return;
            }


        /*Console.WriteLine(
                Utils.TickCount + " Projectile Created: " + missile.SData.Name + " distance: " +
                missile.SData.CastRange + "Radius: " +
                missile.SData.LineWidth + " Speed: " + missile.SData.MissileSpeed);  */


            var spellData = SpellDatabase.GetByMissileName(missile.SData.Name);

            if (spellData == null)
            {
                return;
            }

            LeagueSharp.Common.Utility.DelayAction.Add(0, delegate
            {
                ObjSpellMissionOnOnCreateDelayed(sender, args);
            });
        }

        private static void ObjSpellMissionOnOnCreateDelayed(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;

            if (missile == null || !missile.IsValid)
            {
                return;
            }

            var unit = missile.SpellCaster as AIHeroClient;

            if (unit == null || !unit.IsValid || (unit.Team == ObjectManager.Player.Team && !Config.TestOnAllies))
            {
                return;
            }


            /* Console.WriteLine(
                    Utils.TickCount + " Projectile Created: " + missile.SData.Name + " distance: " +
                    missile.SData.CastRange + "Radius: " +
                    missile.SData.LineWidth + " Speed: " + missile.SData.MissileSpeed);  */


            var spellData = SpellDatabase.GetByMissileName(missile.SData.Name);

            if (spellData == null)
            {
                return;
            }

            var missilePosition = missile.Position.LSTo2D();
            var unitPosition = missile.StartPosition.LSTo2D();
            var endPos = missile.EndPosition.LSTo2D();


            //Calculate the real end Point:
            var direction = (endPos - unitPosition).LSNormalized();
            if (unitPosition.LSDistance(endPos) > spellData.Range || spellData.FixedRange)
            {
                endPos = unitPosition + direction * spellData.Range;
            }

            if (spellData.ExtraRange != -1)
            {
                endPos = endPos +
                         Math.Min(spellData.ExtraRange, spellData.Range - endPos.LSDistance(unitPosition)) * direction;
            }

            var castTime = Utils.TickCount - Game.Ping / 2 - (spellData.MissileDelayed ? 0 : spellData.Delay) -
                           (int)(1000f * missilePosition.LSDistance(unitPosition) / spellData.MissileSpeed);

            //Trigger the skillshot detection callbacks.
            TriggerOnDetectSkillshot(DetectionType.RecvPacket, spellData, castTime, unitPosition, endPos, endPos, unit);
        }

        /// <summary>
        /// Delete the missiles that collide.
        /// </summary>
        private static void ObjSpellMissileOnOnDelete(GameObject sender, EventArgs args)
        {
            var missile = sender as MissileClient;

            if (missile == null || !missile.IsValid)
            {
                return;
            }

            var caster = missile.SpellCaster as AIHeroClient;

            if (caster == null || !caster.IsValid || (caster.Team == ObjectManager.Player.Team && !Config.TestOnAllies))
            {
                return;
            }

            var spellName = missile.SData.Name;
            if (OnDeleteMissile != null)
            {
                foreach (var skillshot in Program.DetectedSkillshots)
                {
                    if (skillshot.SpellData.MissileSpellName.Equals(spellName, StringComparison.InvariantCultureIgnoreCase) &&
                        (skillshot.Unit.NetworkId == caster.NetworkId &&
                         (missile.EndPosition.LSTo2D() - missile.StartPosition.LSTo2D()).LSAngleBetween(skillshot.Direction) <
                         10) && skillshot.SpellData.CanBeRemoved)
                    {
                        OnDeleteMissile(skillshot, missile);
                        break;
                    }
                }
            }

#if DEBUG
           /* Console.WriteLine(
                "Missile deleted: " + missile.SData.Name + " D: " + missile.EndPosition.LSDistance(missile.Position)); */
#endif

            Program.DetectedSkillshots.RemoveAll(
                skillshot =>
                    (skillshot.SpellData.MissileSpellName.Equals(spellName, StringComparison.InvariantCultureIgnoreCase) ||
                     skillshot.SpellData.ExtraMissileNames.Contains(spellName, StringComparer.InvariantCultureIgnoreCase)) &&
                    (skillshot.Unit.NetworkId == caster.NetworkId &&
                     ((missile.EndPosition.LSTo2D() - missile.StartPosition.LSTo2D()).LSAngleBetween(skillshot.Direction) < 10) &&
                     skillshot.SpellData.CanBeRemoved || skillshot.SpellData.ForceRemove)); // 
        }

        /// <summary>
        ///     This event is fired after a skillshot is detected.
        /// </summary>
        public static event OnDetectSkillshotH OnDetectSkillshot;

        /// <summary>
        ///     This event is fired after a skillshot missile collides.
        /// </summary>
        public static event OnDeleteMissileH OnDeleteMissile;


        private static void TriggerOnDetectSkillshot(DetectionType detectionType,
            SpellData spellData,
            int startT,
            Vector2 start,
            Vector2 end,
            Vector2 originalEnd,
            Obj_AI_Base unit)
        {
            var skillshot = new Skillshot(detectionType, spellData, startT, start, end, unit)
            {
                OriginalEnd = originalEnd
            };

            if (OnDetectSkillshot != null)
            {
                OnDetectSkillshot(skillshot);
            }
        }

        /// <summary>
        ///     Gets triggered when a unit casts a spell and the unit is visible.
        /// </summary>
        private static void ObjAiHeroOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || !sender.IsValid)
            {
                return;
            }

            if (Config.PrintSpellData && sender is AIHeroClient)
            {
                Chat.Print(Utils.TickCount + " ProcessSpellCast: " + args.SData.Name);
                Console.WriteLine(Utils.TickCount + " ProcessSpellCast: " + args.SData.Name);
            }

            if (args.SData.Name == "dravenrdoublecast")
            {
                Program.DetectedSkillshots.RemoveAll(
                    s => s.Unit.NetworkId == sender.NetworkId && s.SpellData.SpellName == "DravenRCast");
            }

            if (!sender.IsValid || sender.Team == ObjectManager.Player.Team && !Config.TestOnAllies)
            {
                return;
            }
            //Get the skillshot data.
            var spellData = SpellDatabase.GetByName(args.SData.Name);

            //Skillshot not added in the database.
            if (spellData == null)
            {
                return;
            }

            var startPos = new Vector2();

            if (spellData.FromObject != "")
            {
                foreach (var o in ObjectManager.Get<GameObject>())
                {
                    if (o.Name.Contains(spellData.FromObject))
                    {
                        startPos = o.Position.LSTo2D();
                    }
                }
            }
            else
            {
                startPos = sender.ServerPosition.LSTo2D();
            }

            //For now only zed support.
            if (spellData.FromObjects != null && spellData.FromObjects.Length > 0)
            {
                foreach (var obj in ObjectManager.Get<GameObject>())
                {
                    if (obj.IsEnemy && spellData.FromObjects.Contains(obj.Name))
                    {
                        var start = obj.Position.LSTo2D();
                        var end = start + spellData.Range * (args.End.LSTo2D() - obj.Position.LSTo2D()).LSNormalized();
                        TriggerOnDetectSkillshot(
                            DetectionType.ProcessSpell, spellData, Utils.TickCount - Game.Ping / 2, start, end, end,
                            sender);
                    }
                }
            }

            if (!startPos.IsValid())
            {
                return;
            }

            var endPos = args.End.LSTo2D();

            if (spellData.SpellName == "LucianQ" && args.Target != null &&
                args.Target.NetworkId == ObjectManager.Player.NetworkId)
            {
                return;
            }

            //Calculate the real end Point:
            var direction = (endPos - startPos).LSNormalized();
            if (startPos.LSDistance(endPos) > spellData.Range || spellData.FixedRange)
            {
                endPos = startPos + direction * spellData.Range;
            }

            if (spellData.ExtraRange != -1)
            {
                endPos = endPos +
                         Math.Min(spellData.ExtraRange, spellData.Range - endPos.LSDistance(startPos)) * direction;
            }


            //Trigger the skillshot detection callbacks.
            TriggerOnDetectSkillshot(
                DetectionType.ProcessSpell, spellData, Utils.TickCount - Game.Ping / 2, startPos, endPos, args.End.LSTo2D(), sender);
        }

        /// <summary>
        /// Detects the spells that have missile and are casted from fow.
        /// </summary>
        public static void GameOnOnGameProcessPacket(GamePacketEventArgs args)
        {
            //Gets received when a projectile is created.
            if (args.PacketData[0] == 0x3B)
            {
                var packet = new GamePacket(args.PacketData);

                packet.Position = 1;

                packet.ReadFloat(); //Missile network ID

                var missilePosition = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());
                var unitPosition = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());

                packet.Position = packet.Size() - 119;
                var missileSpeed = packet.ReadFloat();

                packet.Position = 65;
                var endPos = new Vector3(packet.ReadFloat(), packet.ReadFloat(), packet.ReadFloat());

                packet.Position = 112;
                var id = packet.ReadByte();


                packet.Position = packet.Size() - 83;

                var unit = ObjectManager.GetUnitByNetworkId<AIHeroClient>((uint)packet.ReadInteger());
                if ((!unit.IsValid || unit.Team == ObjectManager.Player.Team) && !Config.TestOnAllies)
                {
                    return;
                }

                var spellData = SpellDatabase.GetBySpeed(unit.ChampionName, (int)missileSpeed, id);

                if (spellData == null)
                {
                    return;
                }
                if (spellData.SpellName != "Laser")
                {
                    return;
                }
                var castTime = Utils.TickCount - Game.Ping / 2 - spellData.Delay -
                               (int)
                                   (1000 * missilePosition.SwitchYZ().LSTo2D().LSDistance(unitPosition.SwitchYZ()) /
                                    spellData.MissileSpeed);

                //Trigger the skillshot detection callbacks.
                TriggerOnDetectSkillshot(
                    DetectionType.RecvPacket, spellData, castTime, unitPosition.SwitchYZ().LSTo2D(),
                    endPos.SwitchYZ().LSTo2D(), endPos.SwitchYZ().LSTo2D(), unit);
            }
        }
    }
}
