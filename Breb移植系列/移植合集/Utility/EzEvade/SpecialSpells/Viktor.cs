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
    class Viktor : ChampionPlugin
    {
        static Viktor()
        {

        }

        public void LoadSpecialSpell(SpellData spellData)
        {
            if (spellData.spellName == "ViktorDeathRay3")
            {
                Obj_AI_Minion.OnCreate += OnCreateObj_ViktorDeathRay3;
            }
        }

        private static void OnCreateObj_ViktorDeathRay3(GameObject obj, EventArgs args)
        {
            if (!obj.IsValid<MissileClient>())
                return;

            MissileClient missile = (MissileClient)obj;

            SpellData spellData;

            if (missile.SpellCaster != null && missile.SpellCaster.Team != ObjectManager.Player.Team &&
                missile.SData.Name != null && missile.SData.Name == "ViktorEAugMissile"
                && SpellDetector.onMissileSpells.TryGetValue("ViktorDeathRay3", out spellData)
                && missile.StartPosition != null && missile.EndPosition != null)
            {
                var missileDist = missile.EndPosition.LSTo2D().LSDistance(missile.StartPosition.LSTo2D());
                var delay = missileDist / 1.5f + 1000;

                spellData.spellDelay = delay;

                SpellDetector.CreateSpellData(missile.SpellCaster, missile.StartPosition, missile.EndPosition, spellData);
            }
        }
    }
}
