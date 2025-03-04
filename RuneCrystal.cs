using Jotunn.Configs;
using Jotunn.Entities;
using UnityEngine;

namespace RuneTablet
{
    public class RuneCrystal : CustomItem
    {
        internal static Sprite IconSprite;

        public RuneCrystal() : base("RuneCrystal", true, new ItemConfig() {
            Name = "$item_runecrystal",
            Description = "$item_runecrystal_desc",
            StackSize = 10,
            Weight = 1,
            Icon = IconSprite,
        }) { }
    }
}
