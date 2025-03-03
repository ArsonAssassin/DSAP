﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSAP
{
    public class Enums
    {
        public enum DSItemCategory
        {
            Armor = 0x10000000,
            Consumables = 0x40000000,
            KeyItems = 0x40000000,
            MeleeWeapons = 0x00000000,
            RangedWeapons = 0x00000000,
            Rings = 0x20000000,
            Shields = 0x00000000,
            Spells = 0x40000000,
            SpellTools = 0x00000000,
            UpgradeMaterials = 0x40000000,
            UsableItems = 0x40000000,
            MysteryWeapons = 0x000000000,
            MysteryArmor = 0x10000000,
            MysteryGoods = 0x40000000,
            Trap = 0x33333333
        }

        public enum ItemUpgrade
        {
            None = 0,
            Unique = 1,
            Armor = 2,
            Infusable = 3,
            InfusableRestricted = 4,
            PyroFlame = 5,
            PyroFlameAscended = 6
        }
    }
}
