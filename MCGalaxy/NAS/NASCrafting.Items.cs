namespace MCGalaxy
{
    public partial class NASCrafting
    {
        public static void SetupItems()
        {
            NASRecipe woodPickaxe = new(new("Wood Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5,  5, 5 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            NASRecipe stonePickaxe = new(new("Stone Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1,  1, 1 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            NASRecipe stoneShovel = new(new("Stone Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {   1 },
                    {  78 },
                    {  78 }
                }
            };
            NASRecipe stoneAxe = new(new("Stone Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1,  1 },
                    {  1, 78 },
                    {  0, 78 }
                }
            };
            NASRecipe stoneSword = new(new("Stone Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1 },
                    {  1 },
                    { 78 }
                }
            };
            NASRecipe ironPickaxe = new(new("Iron Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42, 42,42 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            NASRecipe ironShovel = new(new("Iron Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  42 },
                    {  78 },
                    {  78 }
                }
            };
            NASRecipe ironAxe = new(new("Iron Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42, 42 },
                    { 42, 78 },
                    {  0, 78 }
                }
            };
            NASRecipe ironSword = new(new("Iron Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42 },
                    { 42 },
                    { 78 }
                }
            };
            NASRecipe ironhelm = new(new("Iron Helmet"))
            {
                pattern = new ushort[,] {
                    { 42, 42, 42 },
                    { 42, 0, 42 },
                }
            };
            NASRecipe ironchest = new(new("Iron Chestplate"))
            {
                pattern = new ushort[,] {
                    { 42, 0, 42 },
                    { 42, 42, 42 },
                    { 42, 42, 42 },
                }
            };
            NASRecipe ironlegs = new(new("Iron Leggings"))
            {
                pattern = new ushort[,] {
                    { 42, 42, 42 },
                    { 42, 0, 42 },
                    { 42, 0, 42 },
                }
            };
            NASRecipe ironboots = new(new("Iron Boots"))
            {
                pattern = new ushort[,] {
                    { 42, 0, 42 },
                    { 42, 0, 42 },
                }
            };
            NASRecipe goldPickaxe = new(new("Gold Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 41, 41,41 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            NASRecipe goldShovel = new(new("Gold Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  41 },
                    {  78 },
                    {  78 }
                }
            };
            NASRecipe goldAxe = new(new("Gold Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 41, 41 },
                    { 41, 78 },
                    {  0, 78 }
                }
            };
            NASRecipe goldSword = new(new("Gold Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 41 },
                    { 41 },
                    { 78 }
                }
            };
            NASRecipe goldhelm = new(new("Gold Helmet"))
            {
                pattern = new ushort[,] {
                    { 41, 41, 41 },
                    { 41, 0, 41 },
                }
            };
            NASRecipe goldchest = new(new("Gold Chestplate"))
            {
                pattern = new ushort[,] {
                    { 41, 0, 41 },
                    { 41, 41, 41 },
                    { 41, 41, 41 },
                }
            };
            NASRecipe goldlegs = new(new("Gold Leggings"))
            {
                pattern = new ushort[,] {
                    { 41, 41, 41 },
                    { 41, 0, 41 },
                    { 41, 0, 41 },
                }
            };
            NASRecipe goldboots = new(new("Gold Boots"))
            {
                pattern = new ushort[,] {
                    { 41, 0, 41 },
                    { 41, 0, 41 },
                }
            };
            NASRecipe diamondPickaxe = new(new("Diamond Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 631, 631,631 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            NASRecipe diamondShovel = new(new("Diamond Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  631 },
                    {  78 },
                    {  78 }
                }
            };
            NASRecipe diamondAxe = new(new("Diamond Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 631, 631 },
                    { 631, 78 },
                    {  0, 78 }
                }
            };
            NASRecipe diamondSword = new(new("Diamond Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 631 },
                    { 631 },
                    { 78 }
                }
            };
            NASRecipe diamondhelm = new(new("Diamond Helmet"))
            {
                pattern = new ushort[,] {
                    { 631, 631, 631 },
                    { 631, 0, 631 },
                }
            };
            NASRecipe diamondchest = new(new("Diamond Chestplate"))
            {
                pattern = new ushort[,] {
                    { 631, 0, 631 },
                    { 631, 631, 631 },
                    { 631, 631, 631 },
                }
            };
            NASRecipe diamondlegs = new(new("Diamond Leggings"))
            {
                pattern = new ushort[,] {
                    { 631, 631, 631 },
                    { 631, 0, 631 },
                    { 631, 0, 631 },
                }
            };
            NASRecipe diamondboots = new(new("Diamond Boots"))
            {
                pattern = new ushort[,] {
                    { 631, 0, 631 },
                    { 631, 0, 631 },
                }
            },
            emeraldPickaxe = new(new("Emerald Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 650, 650,650 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            },
            emeraldShovel = new(new("Emerald Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  650 },
                    {  78 },
                    {  78 }
                }
            },
            emeraldAxe = new(new("Emerald Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 650, 650 },
                    { 650, 78 },
                    {  0, 78 }
                }
            },
            emeraldSword = new(new("Emerald Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 650 },
                    { 650 },
                    { 78 }
                }
            },
            emeraldhelm = new(new("Emerald Helmet"))
            {
                pattern = new ushort[,] {
                    { 650, 650, 650 },
                    { 650, 0, 650 },
                }
            },
            emeraldchest = new(new("Emerald Chestplate"))
            {
                pattern = new ushort[,] {
                    { 650, 0, 650 },
                    { 650, 650, 650 },
                    { 650, 650, 650 },
                }
            },
            emeraldlegs = new(new("Emerald Leggings"))
            {
                pattern = new ushort[,] {
                    { 650, 650, 650 },
                    { 650, 0, 650 },
                    { 650, 0, 650 },
                }
            },
            emeraldboots = new(new("Emerald Boots"))
            {
                pattern = new ushort[,] {
                    { 650, 0, 650 },
                    { 650, 0, 650 },
                }
            },
            key = new(new("Key"))
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  294, 149, 294 },
                    {  149, 148,  0  },
                    {  149, 148,  0  }
                }
            },
            shears = new(new("Shears"))
            {
                pattern = new ushort[,] {
                    {  0, 42 },
                    {  42, 0 }
                }
            },
            fishing = new(new("Fishing Rod"))
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
