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
    class Yasuo : ChampionPlugin
    {
        static Yasuo()
        {

        }

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "YasuoQW" || spellData.spellName == "YasuoQ3W")
            {
                var hero = HeroManager.Enemies.FirstOrDefault(h => h.ChampionName == "Yasuo");
                if (hero != null)
                {
                    AIHeroClient.OnProcessSpellCast += (sender, args) => ProcessSpell_YasuoQW(sender, args, spellData);
                }
            }
        }

        private static void ProcessSpell_YasuoQW(Obj_AI_Base hero, GameObjectProcessSpellCastEventArgs args, SpellData spellData)
        {
            if (hero.IsEnemy && args.SData.Name == "YasuoQ")
            {
                var castTime = (hero.Spellbook.CastTime - Game.Time) * 1000;

                if (castTime > 0)
                {
                    spellData.spellDelay = castTime;
                }
            }
        }
    }
}
