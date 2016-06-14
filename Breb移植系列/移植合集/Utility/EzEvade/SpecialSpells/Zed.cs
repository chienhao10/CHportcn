﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using EloBuddy;

namespace ezEvade.SpecialSpells
{
    class Zed : ChampionPlugin
    {
        static Zed()
        {

        }

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "ZedQ")
            {
                SpellDetector.OnProcessSpecialSpell += ProcessSpell_ZedShuriken;
                MissileClient.OnCreate += SpellMissile_ZedShadowDash;
                Obj_AI_Minion.OnCreate += OnCreateObj_ZedShuriken;
                Obj_AI_Minion.OnDelete += OnDeleteObj_ZedShuriken;
            }
        }

        private static void OnCreateObj_ZedShuriken(GameObject obj, EventArgs args)
        {
            if (obj.Name == "Shadow" && obj.IsEnemy)
            {
                if (!ObjectTracker.objTracker.ContainsKey(obj.NetworkId))
                {
                    ObjectTracker.objTracker.Add(obj.NetworkId, new ObjectTrackerInfo(obj));

                    foreach (KeyValuePair<int, ObjectTrackerInfo> entry in ObjectTracker.objTracker)
                    {
                        var info = entry.Value;

                        if (info.Name == "Shadow" && info.usePosition && info.position.LSDistance(obj.Position) < 5)
                        {
                            info.Name = "Shadow";
                            info.usePosition = false;
                            info.obj = obj;
                        }
                    }
                }
            }
        }

        private static void OnDeleteObj_ZedShuriken(GameObject obj, EventArgs args)
        {
            if (obj != null && obj.Name == "Shadow")
            {
                ObjectTracker.objTracker.Remove(obj.NetworkId);
            }
        }

        private static void ProcessSpell_ZedShuriken(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args, SpellData spellData,
            SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "ZedQ")
            {
                foreach (KeyValuePair<int, ObjectTrackerInfo> entry in ObjectTracker.objTracker)
                {
                    var info = entry.Value;

                    if (info.Name == "Shadow")
                    {
                        if (info.usePosition == false && (info.obj == null || !info.obj.IsValid || info.obj.IsDead))
                        {
                            DelayAction.Add(1, () => ObjectTracker.objTracker.Remove(info.obj.NetworkId));
                            continue;
                        }
                        else
                        {
                            Vector3 endPos2;
                            if (info.usePosition == false)
                            {
                                endPos2 = info.obj.Position.LSExtend(args.End, spellData.range);
                                SpellDetector.CreateSpellData(hero, info.obj.Position, endPos2, spellData, null, 0, false);
                            }
                            else
                            {
                                endPos2 = info.position.LSExtend(args.End, spellData.range);
                                SpellDetector.CreateSpellData(hero, info.position, endPos2, spellData, null, 0, false);
                            }

                        }
                    }
                }
            }
        }

        private static void SpellMissile_ZedShadowDash(GameObject obj, EventArgs args)
        {
            if (!obj.IsValid<MissileClient>())
                return;

            MissileClient missile = (MissileClient)obj;

            if (missile.SpellCaster.IsEnemy && missile.SData.Name == "ZedWMissile")
            {
                if (!ObjectTracker.objTracker.ContainsKey(obj.NetworkId))
                {
                    ObjectTrackerInfo info = new ObjectTrackerInfo(obj);
                    info.Name = "Shadow";
                    info.OwnerNetworkID = missile.SpellCaster.NetworkId;
                    info.usePosition = true;
                    info.position = missile.EndPosition;

                    ObjectTracker.objTracker.Add(obj.NetworkId, info);

                    DelayAction.Add(1000, () => ObjectTracker.objTracker.Remove(obj.NetworkId));
                }
            }
        }
    }
}
