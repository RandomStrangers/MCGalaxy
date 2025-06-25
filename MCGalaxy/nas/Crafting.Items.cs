#if NAS && !NET_20 && TEN_BIT_BLOCKS
namespace NotAwesomeSurvival
{
    public partial class Crafting
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public static void SetupItems()
        {
            //EXPLANATION FOR HOW TO READ RECIPES!

            //the first line of the recipe shows what will be produced. if it's an item it will give the 
            //specified item with the name given. If it's a blockID it will give the blockID specified.

            //The recipe.pattern is a 2d list that contains what blocks are needed. Remember that the number is
            //actually the blockID. You can do /hold (blockid) ingame to see what block it is.

            //the recipe.usesAlternateID specifies whether or not alternate versions of the block can be used. 
            //The alternateIDs of 5 (oak planks) are birch and spruce planks, e.t.c. For 36 (white wool) it's 
            //all the other colors of wool.

            //If recipe.stationType = Crafting.Station.Type.Furnace, then it means the recipe must be made in a
            //furnace and NOT a crafting table.

            //recipe.usesParentID specifies whether or not the Parent ID of a block can be used. ParentIDS usually
            //amount to rotated versions of a block. So if a slab is in a recipe that uses parentIDS, then the slab
            //can be in any direction.

            //Finally, recipe.shapeless determines whether or not you can put the blocks in any order.
            //If it's true, it means the blocks shown don't have to be in the same place relative to each other.

            //Recipes that don't fill up all 9 slots and are NOT shapeless just mean that smaller pattern can be
            //anywhere on the crafting table, just in the same spot relative to each other.
            Recipe woodPickaxe = new Recipe(new Item("Wood Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5,  5, 5 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            // stone tools
            Recipe stonePickaxe = new Recipe(new Item("Stone Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1,  1, 1 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe stoneShovel = new Recipe(new Item("Stone Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {   1 },
                    {  78 },
                    {  78 }
                }
            };
            Recipe stoneAxe = new Recipe(new Item("Stone Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1,  1 },
                    {  1, 78 },
                    {  0, 78 }
                }
            };
            Recipe stoneSword = new Recipe(new Item("Stone Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1 },
                    {  1 },
                    { 78 }
                }
            };
            //iron tools
            Recipe ironPickaxe = new Recipe(new Item("Iron Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42, 42,42 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe ironShovel = new Recipe(new Item("Iron Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  42 },
                    {  78 },
                    {  78 }
                }
            };
            Recipe ironAxe = new Recipe(new Item("Iron Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42, 42 },
                    { 42, 78 },
                    {  0, 78 }
                }
            };
            Recipe ironSword = new Recipe(new Item("Iron Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42 },
                    { 42 },
                    { 78 }
                }
            };
            Recipe ironhelm = new Recipe(new Item("Iron Helmet"))
            {
                pattern = new ushort[,] {
                    { 42, 42, 42 },
                    { 42, 0, 42 },
                }
            };
            Recipe ironchest = new Recipe(new Item("Iron Chestplate"))
            {
                pattern = new ushort[,] {
                    { 42, 0, 42 },
                    { 42, 42, 42 },
                    { 42, 42, 42 },
                }
            };
            Recipe ironlegs = new Recipe(new Item("Iron Leggings"))
            {
                pattern = new ushort[,] {
                    { 42, 42, 42 },
                    { 42, 0, 42 },
                    { 42, 0, 42 },
                }
            };
            Recipe ironboots = new Recipe(new Item("Iron Boots"))
            {
                pattern = new ushort[,] {
                    { 42, 0, 42 },
                    { 42, 0, 42 },
                }
            };
            //gold tools
            Recipe goldPickaxe = new Recipe(new Item("Gold Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 41, 41,41 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe goldShovel = new Recipe(new Item("Gold Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  41 },
                    {  78 },
                    {  78 }
                }
            };
            Recipe goldAxe = new Recipe(new Item("Gold Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 41, 41 },
                    { 41, 78 },
                    {  0, 78 }
                }
            };
            Recipe goldSword = new Recipe(new Item("Gold Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 41 },
                    { 41 },
                    { 78 }
                }
            };
            Recipe goldhelm = new Recipe(new Item("Gold Helmet"))
            {
                pattern = new ushort[,] {
                    { 41, 41, 41 },
                    { 41, 0, 41 },
                }
            };
            Recipe goldchest = new Recipe(new Item("Gold Chestplate"))
            {
                pattern = new ushort[,] {
                    { 41, 0, 41 },
                    { 41, 41, 41 },
                    { 41, 41, 41 },
                }
            };
            Recipe goldlegs = new Recipe(new Item("Gold Leggings"))
            {
                pattern = new ushort[,] {
                    { 41, 41, 41 },
                    { 41, 0, 41 },
                    { 41, 0, 41 },
                }
            };
            Recipe goldboots = new Recipe(new Item("Gold Boots"))
            {
                pattern = new ushort[,] {
                    { 41, 0, 41 },
                    { 41, 0, 41 },
                }
            };
            //diamond tools
            Recipe diamondPickaxe = new Recipe(new Item("Diamond Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 631, 631,631 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe diamondShovel = new Recipe(new Item("Diamond Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  631 },
                    {  78 },
                    {  78 }
                }
            };
            Recipe diamondAxe = new Recipe(new Item("Diamond Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 631, 631 },
                    { 631, 78 },
                    {  0, 78 }
                }
            };
            Recipe diamondSword = new Recipe(new Item("Diamond Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 631 },
                    { 631 },
                    { 78 }
                }
            };
            Recipe diamondhelm = new Recipe(new Item("Diamond Helmet"))
            {
                pattern = new ushort[,] {
                    { 631, 631, 631 },
                    { 631, 0, 631 },
                }
            };
            Recipe diamondchest = new Recipe(new Item("Diamond Chestplate"))
            {
                pattern = new ushort[,] {
                    { 631, 0, 631 },
                    { 631, 631, 631 },
                    { 631, 631, 631 },
                }
            };
            Recipe diamondlegs = new Recipe(new Item("Diamond Leggings"))
            {
                pattern = new ushort[,] {
                    { 631, 631, 631 },
                    { 631, 0, 631 },
                    { 631, 0, 631 },
                }
            };
            Recipe diamondboots = new Recipe(new Item("Diamond Boots"))
            {
                pattern = new ushort[,] {
                    { 631, 0, 631 },
                    { 631, 0, 631 },
                }
            };
            //emerald tools
            Recipe emeraldPickaxe = new Recipe(new Item("Emerald Pickaxe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 650, 650,650 },
                    {  0, 78, 0 },
                    {  0, 78, 0 }
                }
            };
            Recipe emeraldShovel = new Recipe(new Item("Emerald Shovel"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  650 },
                    {  78 },
                    {  78 }
                }
            };
            Recipe emeraldAxe = new Recipe(new Item("Emerald Axe"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 650, 650 },
                    { 650, 78 },
                    {  0, 78 }
                }
            };
            Recipe emeraldSword = new Recipe(new Item("Emerald Sword"))
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 650 },
                    { 650 },
                    { 78 }
                }
            };
            Recipe emeraldhelm = new Recipe(new Item("Emerald Helmet"))
            {
                pattern = new ushort[,] {
                    { 650, 650, 650 },
                    { 650, 0, 650 },
                }
            };
            Recipe emeraldchest = new Recipe(new Item("Emerald Chestplate"))
            {
                pattern = new ushort[,] {
                    { 650, 0, 650 },
                    { 650, 650, 650 },
                    { 650, 650, 650 },
                }
            };
            Recipe emeraldlegs = new Recipe(new Item("Emerald Leggings"))
            {
                pattern = new ushort[,] {
                    { 650, 650, 650 },
                    { 650, 0, 650 },
                    { 650, 0, 650 },
                }
            };
            Recipe emeraldboots = new Recipe(new Item("Emerald Boots"))
            {
                pattern = new ushort[,] {
                    { 650, 0, 650 },
                    { 650, 0, 650 },
                }
            };
            Recipe key = new Recipe(new Item("Key"))
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  294, 149, 294 },
                    {  149, 148,  0  },
                    {  149, 148,  0  }
                }
            };
            Recipe shears = new Recipe(new Item("Shears"))
            {
                pattern = new ushort[,] {
                    {  0, 42 },
                    {  42, 0 }
                }
            };
            Recipe fishing = new Recipe(new Item("Fishing Rod"))
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