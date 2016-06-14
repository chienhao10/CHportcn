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
    class Ziggs : ChampionPlugin
    {
        static Ziggs()
        {

        }

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "ZiggsQ")
            {
                SpellDetector.OnProcessSpecialSpell += ProcessSpell_ZiggsQ;
            }
        }

        private static void ProcessSpell_ZiggsQ(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args, SpellData spellData, SpecialSpellEventArgs specialSpellArgs)
        {
            if (spellData.spellName == "ZiggsQ")
            {
                var startPos = hero.ServerPosition.LSTo2D();
                var endPos = args.End.LSTo2D();
                var dir = (endPos - startPos).LSNormalized();

                if (endPos.LSDistance(startPos) > 850)
                {
                    endPos = startPos + dir * 850;
                }

                SpellDetector.CreateSpellData(hero, args.Start, endPos.To3D(), spellData, null, 0, false);

                var endPos2 = endPos + dir * 0.4f * startPos.LSDistance(endPos);
                SpellDetector.CreateSpellData(hero, args.Start, endPos2.To3D(), spellData, null, 250, false);

                var endPos3 = endPos2 + dir * 0.6f * endPos.LSDistance(endPos2);
                SpellDetector.CreateSpellData(hero, args.Start, endPos3.To3D(), spellData, null, 800);

                specialSpellArgs.noProcess = true;
            }
        }
    }
}
