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
    class JarvanIV : ChampionPlugin
    {
        static JarvanIV()
        {
            
        }

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "JarvanIVDragonStrike")
            {
                AIHeroClient hero = HeroManager.Enemies.FirstOrDefault(h => h.ChampionName == "JarvanIV");
                if (hero == null)
                {
                    return;
                }

                AIHeroClient.OnProcessSpellCast += ProcessSpell_JarvanIVDemacianStandard;

                SpellDetector.OnProcessSpecialSpell += ProcessSpell_JarvanIVDragonStrike;
                Obj_AI_Minion.OnCreate += OnCreateObj_JarvanIVDragonStrike;
                Obj_AI_Minion.OnDelete += OnDeleteObj_JarvanIVDragonStrike;
            }            
        }

        private static void ProcessSpell_JarvanIVDemacianStandard(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args)
        {
            if (hero.IsEnemy && args.SData.Name == "JarvanIVDemacianStandard")
            {
                ObjectTracker.AddObjTrackerPosition("Beacon", args.End, 1000);
            }
        }

        private static void OnDeleteObj_JarvanIVDragonStrike(GameObject obj, EventArgs args)
        {
            if (obj.Name == "Beacon")
            {
                ObjectTracker.objTracker.Remove(obj.NetworkId);
            }
        }

        private static void OnCreateObj_JarvanIVDragonStrike(GameObject obj, EventArgs args)
        {
            if (obj.Name == "Beacon")
            {
                ObjectTracker.objTracker.Add(obj.NetworkId, new ObjectTrackerInfo(obj));
            }
        }

        private static void ProcessSpell_JarvanIVDragonStrike(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args, SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (args.SData.Name == "JarvanIVDragonStrike")
            {
                if (SpellDetector.onProcessSpells.TryGetValue("JarvanIVDragonStrike2", out spellData))
                {
                    foreach (KeyValuePair<int, ObjectTrackerInfo> entry in ObjectTracker.objTracker)
                    {
                        var info = entry.Value;

                        if (info.Name == "Beacon" || info.obj.Name == "Beacon")
                        {
                            if (info.usePosition == false && (info.obj == null || !info.obj.IsValid || info.obj.IsDead))
                            {
                                DelayAction.Add(1, () => ObjectTracker.objTracker.Remove(info.obj.NetworkId));
                                continue;
                            }

                            var objPosition = info.usePosition ? info.position.LSTo2D() : info.obj.Position.LSTo2D();

                            if (args.End.LSTo2D().LSDistance(objPosition) < 300)
                            {
                                var dir = (objPosition - args.Start.LSTo2D()).LSNormalized();
                                var endPosition = objPosition + dir * 110;

                                SpellDetector.CreateSpellData(hero, args.Start, endPosition.To3D(), spellData);
                                specialSpellArgs.noProcess = true;
                                return;
                            }
                        }
                    }


                }
            }
        }
    }
}
