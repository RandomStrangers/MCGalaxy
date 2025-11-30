#if NAS && TEN_BIT_BLOCKS
namespace NotAwesomeSurvival
{
    public partial class Crafting
    {
        public static void SetupItems()
        {
            Recipe woodPickaxe = new(new("Wood Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5,  5, 5 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe stonePickaxe = new(new("Stone Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1,  1, 1 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe stoneShovel = new(new("Stone Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {   1 },
                    {  78 },
                    {  78 }
                }
            };
            Recipe stoneAxe = new(new("Stone Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1,  1 },
                    {  1, 78 },
                    {  0, 78 }
                }
            };
            Recipe stoneSword = new(new("Stone Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1 },
                    {  1 },
                    { 78 }
                }
            };
            Recipe ironPickaxe = new(new("Iron Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42, 42,42 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe ironShovel = new(new("Iron Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  42 },
                    {  78 },
                    {  78 }
                }
            };
            Recipe ironAxe = new(new("Iron Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42, 42 },
                    { 42, 78 },
                    {  0, 78 }
                }
            };
            Recipe ironSword = new(new("Iron Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42 },
                    { 42 },
                    { 78 }
                }
            };
            Recipe ironhelm = new(new("Iron Helmet"))
            {
                pattern = new ushort[,] {
                    { 42, 42, 42 },
                    { 42, 0, 42 },
                }
            };
            Recipe ironchest = new(new("Iron Chestplate"))
            {
                pattern = new ushort[,] {
                    { 42, 0, 42 },
                    { 42, 42, 42 },
                    { 42, 42, 42 },
                }
            };
            Recipe ironlegs = new(new("Iron Leggings"))
            {
                pattern = new ushort[,] {
                    { 42, 42, 42 },
                    { 42, 0, 42 },
                    { 42, 0, 42 },
                }
            };
            Recipe ironboots = new(new("Iron Boots"))
            {
                pattern = new ushort[,] {
                    { 42, 0, 42 },
                    { 42, 0, 42 },
                }
            };
            Recipe goldPickaxe = new(new("Gold Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 41, 41,41 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe goldShovel = new(new("Gold Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  41 },
                    {  78 },
                    {  78 }
                }
            };
            Recipe goldAxe = new(new("Gold Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 41, 41 },
                    { 41, 78 },
                    {  0, 78 }
                }
            };
            Recipe goldSword = new(new("Gold Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 41 },
                    { 41 },
                    { 78 }
                }
            };
            Recipe goldhelm = new(new("Gold Helmet"))
            {
                pattern = new ushort[,] {
                    { 41, 41, 41 },
                    { 41, 0, 41 },
                }
            };
            Recipe goldchest = new(new("Gold Chestplate"))
            {
                pattern = new ushort[,] {
                    { 41, 0, 41 },
                    { 41, 41, 41 },
                    { 41, 41, 41 },
                }
            };
            Recipe goldlegs = new(new("Gold Leggings"))
            {
                pattern = new ushort[,] {
                    { 41, 41, 41 },
                    { 41, 0, 41 },
                    { 41, 0, 41 },
                }
            };
            Recipe goldboots = new(new("Gold Boots"))
            {
                pattern = new ushort[,] {
                    { 41, 0, 41 },
                    { 41, 0, 41 },
                }
            };
            Recipe diamondPickaxe = new(new("Diamond Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 631, 631,631 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe diamondShovel = new(new("Diamond Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  631 },
                    {  78 },
                    {  78 }
                }
            };
            Recipe diamondAxe = new(new("Diamond Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 631, 631 },
                    { 631, 78 },
                    {  0, 78 }
                }
            };
            Recipe diamondSword = new(new("Diamond Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 631 },
                    { 631 },
                    { 78 }
                }
            };
            Recipe diamondhelm = new(new("Diamond Helmet"))
            {
                pattern = new ushort[,] {
                    { 631, 631, 631 },
                    { 631, 0, 631 },
                }
            };
            Recipe diamondchest = new(new("Diamond Chestplate"))
            {
                pattern = new ushort[,] {
                    { 631, 0, 631 },
                    { 631, 631, 631 },
                    { 631, 631, 631 },
                }
            };
            Recipe diamondlegs = new(new("Diamond Leggings"))
            {
                pattern = new ushort[,] {
                    { 631, 631, 631 },
                    { 631, 0, 631 },
                    { 631, 0, 631 },
                }
            };
            Recipe diamondboots = new(new("Diamond Boots"))
            {
                pattern = new ushort[,] {
                    { 631, 0, 631 },
                    { 631, 0, 631 },
                }
            };
            Recipe emeraldPickaxe = new(new("Emerald Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 650, 650,650 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe emeraldShovel = new(new("Emerald Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  650 },
                    {  78 },
                    {  78 }
                }
            };
            Recipe emeraldAxe = new(new("Emerald Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 650, 650 },
                    { 650, 78 },
                    {  0, 78 }
                }
            };
            Recipe emeraldSword = new(new("Emerald Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 650 },
                    { 650 },
                    { 78 }
                }
            };
            Recipe emeraldhelm = new(new("Emerald Helmet"))
            {
                pattern = new ushort[,] {
                    { 650, 650, 650 },
                    { 650, 0, 650 },
                }
            };
            Recipe emeraldchest = new(new("Emerald Chestplate"))
            {
                pattern = new ushort[,] {
                    { 650, 0, 650 },
                    { 650, 650, 650 },
                    { 650, 650, 650 },
                }
            };
            Recipe emeraldlegs = new(new("Emerald Leggings"))
            {
                pattern = new ushort[,] {
                    { 650, 650, 650 },
                    { 650, 0, 650 },
                    { 650, 0, 650 },
                }
            };
            Recipe emeraldboots = new(new("Emerald Boots"))
            {
                pattern = new ushort[,] {
                    { 650, 0, 650 },
                    { 650, 0, 650 },
                }
            };
            Recipe key = new(new("Key"))
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  294, 149, 294 },
                    {  149, 148,  0  },
                    {  149, 148,  0  }
                }
            };
            Recipe shears = new(new("Shears"))
            {
                pattern = new ushort[,] {
                    {  0, 42 },
                    {  42, 0 }
                }
            };
            Recipe fishing = new(new("Fishing Rod"))
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  0, 0, 78 },
                    {  0, 78, 36 },
                    {  78, 0, 36 }
                }
            };
        }
    }
}
#endif