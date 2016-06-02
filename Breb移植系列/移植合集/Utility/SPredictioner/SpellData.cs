﻿// Copyright 2014 - 2014 Esk0r
// SpellData.cs is part of Evade.
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

using EloBuddy;
using LeagueSharp;
using LeagueSharp.Common;


#endregion

namespace SPredictioner
{
    public class SpellData
    {
        public enum CollisionObjectTypes
        {
            Minion,
            Champions,
            YasuoWall,
        }

        public SpellData.CollisionObjectTypes[] CollisionObjects = { };
        public bool AddHitbox;
        public bool CanBeRemoved = false;
        public bool Centered;
        public string ChampionName;
        public int DangerValue;
        public int Delay;
        public bool DisabledByDefault = false;
        public bool DisableFowDetection = false;
        public bool DontAddExtraDuration;
        public bool DontCheckForDuplicates = false;
        public bool DontCross = false;
        public bool DontRemove = false;
        public int ExtraDuration;
        public string[] ExtraMissileNames = { };
        public int ExtraRange = -1;
        public string[] ExtraSpellNames = { };
        public bool FixedRange;
        public bool ForceRemove = false;
        public bool FollowCaster = false;
        public string FromObject = "";
        public string[] FromObjects = { };
        public int Id = -1;
        public bool Invert;
        public bool IsDangerous = false;
        public int MissileAccel = 0;
        public bool MissileDelayed;
        public bool MissileFollowsUnit;
        public int MissileMaxSpeed;
        public int MissileMinSpeed;
        public int MissileSpeed;
        public string MissileSpellName = "";
        public float MultipleAngle;
        public int MultipleNumber = -1;
        public int RingRadius;
        public SpellSlot Slot;
        public string SpellName;
        public bool TakeClosestPath = false;
        public string ToggleParticleName = "";
        public LeagueSharp.Common.SkillshotType Type;
        private int _radius;
        private int _range;

        public SpellData() { }

        public SpellData(string championName,
            string spellName,
            SpellSlot slot,
            LeagueSharp.Common.SkillshotType type,
            int delay,
            int range,
            int radius,
            int missileSpeed,
            bool addHitbox,
            bool fixedRange,
            int defaultDangerValue)
        {
            ChampionName = championName;
            SpellName = spellName;
            Slot = slot;
            Type = type;
            Delay = delay;
            Range = range;
            _radius = radius;
            MissileSpeed = missileSpeed;
            AddHitbox = addHitbox;
            FixedRange = fixedRange;
            DangerValue = defaultDangerValue;
        }

        public string MenuItemName
        {
            get { return ChampionName + " - " + SpellName; }
        }

        public int Radius
        {
            get
            {
                return _radius;
            }
            set { _radius = value; }
        }

        public int RawRadius
        {
            get { return _radius; }
        }

        public int RawRange
        {
            get { return _range; }
        }

        public int Range
        {
            get
            {
                return _range;
            }
            set
            {
                _range = value;
            }
        }

        public bool Collisionable
        {
            get
            {
                for(int i = 0; i < CollisionObjects.Length; i++)
                {
                    if (CollisionObjects[i] == SpellData.CollisionObjectTypes.Champions || CollisionObjects[i] == SpellData.CollisionObjectTypes.Minion)
                        return true;
                }
                return false;
            }
        }
    }
}