#if NAS && TEN_BIT_BLOCKS
namespace NotAwesomeSurvival
{
    public partial class Crafting
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public static void Setup()
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
            Recipe wood = new Recipe(5, 4)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  17 }
                }
            };
            Recipe woodFall = new Recipe(657, 4)
            {
                usesParentID = true,
                shapeless = true,
                pattern = new ushort[,] {
                    {  17, 12, 12 },
                    {  12, 12, 12 },
                    {  12, 12, 12 }
                }
            };
            Recipe fakeDirt = new Recipe(685, 4)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 3, 55, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe fakeDirt2 = new Recipe(685, 4)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 3, 470, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe woodFall2 = new Recipe(656, 2)
            {
                usesParentID = true,
                shapeless = true,
                pattern = new ushort[,] {
                    {  242, 12, 12 },
                    {  12, 12, 12 },
                    {  12, 12, 12 }
                }
            };
            Recipe glassFall = new Recipe(655, 4)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    {  20, 12, 12 },
                    {  12, 12, 12 },
                    {  12, 12, 12 }
                }
            };
            Recipe trapdoor = new Recipe(659, 3)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5, 5, 5 },
                    {  5, 5, 5 }
                }
            };
            Recipe woodSlab = new Recipe(56, 6)
            {
                pattern = new ushort[,] {
                    {  5, 5, 5 }
                }
            };
            Recipe woodWall = new Recipe(182, 6)
            {
                pattern = new ushort[,] {
                    {  5 },
                    {  5 },
                    {  5 }
                }
            };
            Recipe woodStair = new Recipe(66, 6)
            {
                pattern = new ushort[,] {
                    {  5, 0, 0 },
                    {  5, 5, 0 },
                    {  5, 5, 5 }
                }
            };
            Recipe woodPole = new Recipe(78, 4)
            {
                pattern = new ushort[,] {
                    {  5 },
                    {  5 }
                }
            };
            Recipe fenceWE = new Recipe(94, 4)
            {
                pattern = new ushort[,] {
                    {  78, 79, 78 },
                    {  78, 79, 78 }
                }
            };
            Recipe fenceNS = new Recipe(94, 4)
            {
                pattern = new ushort[,] {
                    {  78, 80, 78 },
                    {  78, 80, 78 }
                }
            };
            Recipe darkDoor = new Recipe(55, 2)
            {
                pattern = new ushort[,] {
                    { 5, 5 },
                    { 5, 5 },
                    { 5, 5 }
                }
            };
            Recipe board = new Recipe(168, 6)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  56, 56, 56 }
                }
            };
            Recipe boardSideways = new Recipe(524, 6)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  182 },
                    {  182 },
                    {  182 }
                }
            };
            //spruce wood stuff ------------------------------------------------------
            Recipe sprucewood = new Recipe(97, 4)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  250 }
                }
            };
            Recipe sprucewoodSlab = new Recipe(99, 6)
            {
                pattern = new ushort[,] {
                    {  97, 97, 97 }
                }
            };
            Recipe sprucewoodWall = new Recipe(190, 6)
            {
                pattern = new ushort[,] {
                    {  97 },
                    {  97 },
                    {  97 }
                }
            };
            Recipe sprucewoodStair = new Recipe(266, 6)
            {
                pattern = new ushort[,] {
                    {  97, 0, 0 },
                    {  97, 97, 0 },
                    {  97, 97, 97 }
                }
            };
            Recipe sprucewoodPole = new Recipe(252, 4)
            {
                pattern = new ushort[,] {
                    {  97 },
                    {  97 }
                }
            };
            Recipe sprucefenceWE = new Recipe(258, 4)
            {
                pattern = new ushort[,] {
                    {  252, 253, 252 },
                    {  252, 253, 252 }
                }
            };
            Recipe sprucefenceNS = new Recipe(258, 4)
            {
                pattern = new ushort[,] {
                    {  252, 254, 252 },
                    {  252, 254, 252 }
                }
            };
            //Recipe thirdDoor = new Recipe(470, 2)
            //{
            //  pattern = new ushort[,] {
            //      { 98, 98 },
            //      { 98, 98 },
            //      { 98, 98 }
            //  }
            //};
            //birch wood stuff ------------------------------------------------------
            Recipe birchwood = new Recipe(98, 4)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  242 }
                }
            };
            Recipe birchwoodSlab = new Recipe(101, 6)
            {
                pattern = new ushort[,] {
                    {  98, 98, 98 }
                }
            };
            Recipe birchwoodWall = new Recipe(186, 6)
            {
                pattern = new ushort[,] {
                    {  98 },
                    {  98 },
                    {  98 }
                }
            };
            Recipe birchwoodStair = new Recipe(262, 6)
            {
                pattern = new ushort[,] {
                    {  98, 0, 0 },
                    {  98, 98, 0 },
                    {  98, 98, 98 }
                }
            };
            Recipe birchwoodPole = new Recipe(255, 4)
            {
                pattern = new ushort[,] {
                    {  98 },
                    {  98 }
                }
            };
            Recipe birchfenceWE = new Recipe(260, 4)
            {
                pattern = new ushort[,] {
                    {  255, 256, 255 },
                    {  255, 256, 255 }
                }
            };
            Recipe birchfenceNS = new Recipe(260, 4)
            {
                pattern = new ushort[,] {
                    {  255, 257, 255 },
                    {  255, 257, 255 }
                }
            };
            Recipe lightDoor = new Recipe(470, 2)
            {
                pattern = new ushort[,] {
                    { 98, 98 },
                    { 98, 98 },
                    { 98, 98 }
                }
            };
            //chest
            Recipe chest = new Recipe(216, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5,  5,  5 },
                    {  5, 0, 5 },
                    {  5,  5,  5 }
                }
            };
            Recipe barrel = new Recipe(143, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 5, 56, 5 },
                    { 5, 148, 5 },
                    { 5, 57, 5 },
                }
            };
            Recipe barrel2 = new Recipe(143, 1);
            barrel.usesAlternateID = true;
            barrel2.pattern = new ushort[,] {
                { 150 },
                {  17 },
                { 149 }
            };
            Recipe auto = new Recipe(413, 1)
            {
                shapeless = true,
                usesAlternateID = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    { 672, 1, 672 },
                    { 1, 17, 1 },
                    { 672, 1, 672 }
                }
            };
            Recipe bedbeacon = new Recipe(612, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 20, 23, 20 },
                    { 23, 42, 23 },
                    { 20, 23, 20 }
                }
            };
            Recipe smith = new Recipe(676, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42, 42 },
                    { 5, 5 },
                    { 5, 5 }
                }
            };
            Recipe tank = new Recipe(697, 1)
            {
                pattern = new ushort[,] {
                    { 690, 149, 690 },
                    { 690, 0, 690 },
                    { 690, 690, 690 },
                }
            };
            Recipe crate = new Recipe(142, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 5, 5 },
                    { 5, 5 }
                }
            };
            Recipe cryingObs = new Recipe(457, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 690, 690, 690 },
                    { 690, 239, 690 },
                    { 690, 690, 690 },
                }
            };
            //stone stuff ------------------------------------------------------
            Recipe stoneSlab = new Recipe(596, 6)
            {
                pattern = new ushort[,] {
                    {  1, 1, 1 }
                }
            };
            Recipe stoneWall = new Recipe(598, 6)
            {
                pattern = new ushort[,] {
                    {  1 },
                    {  1 },
                    {  1 }
                }
            };
            Recipe stoneStair = new Recipe(70, 6)
            {
                pattern = new ushort[,] {
                    {  1, 0, 0 },
                    {  1, 1, 0 },
                    {  1, 1, 1 }
                }
            };
            //stonebrick
            Recipe marker = new Recipe(64, 1)
            {
                pattern = new ushort[,] {
                    {  65, 65, 65 },
                    {  65,  0, 65 },
                    {  65, 65, 65 }
                }
            };
            Recipe stoneBrick = new Recipe(65, 6)
            {
                pattern = new ushort[,] {
                    {  1, 1, 0 },
                    {  0, 1, 1 },
                    {  1, 1, 0 }
                }
            };
            Recipe stoneBrickSlab = new Recipe(86, 6)
            {
                pattern = new ushort[,] {
                    {  65, 65, 65 }
                }
            };
            Recipe stoneBrickWall = new Recipe(278, 6)
            {
                pattern = new ushort[,] {
                    {  65 },
                    {  65 },
                    {  65 }
                }
            };
            Recipe stonePole = new Recipe(75, 4)
            {
                pattern = new ushort[,] {
                    {  65 },
                    {  65 }
                }
            };
            Recipe thinPole = new Recipe(211, 4)
            {
                pattern = new ushort[,] {
                    {  75 },
                    {  75 }
                }
            };
            Recipe linedStone = new Recipe(477, 4)
            {
                pattern = new ushort[,] {
                    {  65, 65 },
                    {  65, 65 }
                }
            };
            Recipe mossyCobble = new Recipe(181, 4)
            {
                pattern = new ushort[,] {
                    {  18, 162 },
                    {  162, 18 }
                }
            };
            Recipe mossyBricks = new Recipe(180, 4)
            {
                pattern = new ushort[,] {
                    {  181, 181 },
                    {  181, 181 }
                }
            };
            Recipe boulder = new Recipe(214, 4)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1 }
                }
            };
            Recipe nub = new Recipe(194, 4)
            {
                pattern = new ushort[,] {
                    {  214 }
                }
            };
            Recipe cobbleBrick = new Recipe(4, 4)
            {
                pattern = new ushort[,] {
                    {  162, 162 },
                    {  162, 162 }
                }
            };
            Recipe cobbleBrickSlab = new Recipe(50, 6)
            {
                pattern = new ushort[,] {
                    {  4, 4, 4 }
                }
            };
            Recipe cobbleBrickWall = new Recipe(133, 6)
            {
                pattern = new ushort[,] {
                    {  4, 4, 4 },
                    {  4, 4, 4 }
                }
            };
            Recipe cobblestone = new Recipe(162, 4)
            {
                pattern = new ushort[,] {
                    {  1, 1 },
                    {  1, 1 }
                }
            };
            Recipe cobblestoneSlab = new Recipe(163, 6)
            {
                pattern = new ushort[,] {
                    {  162, 162, 162 }
                }
            };
            Recipe sandstoneSlab = new Recipe(299, 6)
            {
                pattern = new ushort[,] {
                    {  52, 52, 52 }
                }
            };
            Recipe furnace = new Recipe(625, 1)
            {
                usesAlternateID = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    {  1,  1, 1 },
                    {  1,  0, 1 },
                    {  1,  1, 1 }
                }
            };
            Recipe concreteBlock = new Recipe(45, 4)
            {
                pattern = new ushort[,] {
                    {  4, 4 },
                    {  4, 4 }
                }
            };
            Recipe concreteSlab = new Recipe(44, 6)
            {
                pattern = new ushort[,] {
                    {  45, 45, 45 }
                }
            };
            Recipe concreteWall = new Recipe(282, 6)
            {
                pattern = new ushort[,] {
                    { 45 },
                    { 45 },
                    { 45 }
                }
            };
            Recipe concreteBrick = new Recipe(549, 4)
            {
                pattern = new ushort[,] {
                    {  45, 45 },
                    {  45, 45 }
                }
            };
            Recipe sandstone = new Recipe(52, 1)
            {
                pattern = new ushort[,] {
                    {  12, 12 },
                    {  12, 12 }
                }
            };
            Recipe stonePlate = new Recipe(135, 6)
            {
                pattern = new ushort[,] {
                    {  44, 44, 44 }
                }
            };
            Recipe quartzPillar = new Recipe(63, 2)
            {
                pattern = new ushort[,] {
                    {  61 },
                    {  61 }
                }
            };
            Recipe quartzWall = new Recipe(286, 6)
            {
                pattern = new ushort[,] {
                    {  61 },
                    {  61 },
                    {  61 }
                }
            };
            Recipe quartzSlab = new Recipe(84, 6)
            {
                pattern = new ushort[,] {
                    {  61, 61, 61 },
                }
            };
            Recipe quartzChis = new Recipe(235, 1)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                     {  84 },
                     {  84 }
                }
            };
            Recipe quartzStair = new Recipe(274, 6)
            {
                pattern = new ushort[,] {
                    {  61,  0,  0 },
                    {  61, 61,  0 },
                    {  61, 61, 61 }
                }
            };
            //upside down slab recipe
            Recipe stonePlate2 = new Recipe(135, 6)
            {
                pattern = new ushort[,] {
                    {  58, 58, 58 }
                }
            };
            Recipe concreteStair = new Recipe(270, 6)
            {
                pattern = new ushort[,] {
                    {  45,  0,  0 },
                    {  45, 45,  0 },
                    {  45, 45, 45 }
                }
            };
            Recipe concreteCorner = new Recipe(480, 4)
            {
                pattern = new ushort[,] {
                    { 45 },
                    { 45 }
                }
            };
            //ore stuff
            Recipe charcoal = new Recipe(49, 2)
            {
                stationType = Station.Type.Furnace,
                shapeless = true,
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  17, 17, 17 },
                    {  17, 197, 17 },
                    {  17, 17, 17 }
                }
            };
            Recipe coalBlock = new Recipe(49, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    {  197, 197, 0 },
                    {  0, 0, 0 },
                    {  0, 0, 0 },
                }
            };
            Recipe hotCoals = new Recipe(239, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    {  49, 49, 0 },
                    {  0, 0, 0 },
                    {  0, 0, 0 },
                }
            };
            Recipe iron = new Recipe(42, 1)
            {
                stationType = Station.Type.Furnace,
                expGiven = 4,
                shapeless = true,
                pattern = new ushort[,] {
                    {  628, 197, 197 },
                    {  197, 197, 197 },
                    {  197, 197, 197 },
                }
            };
            Recipe nugIron = new Recipe(42, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    {  624, 624, 624 },
                    {  624, 624, 624 },
                    {  624, 624, 624 },
                }
            };
            Recipe ironNug = new Recipe(624, 9)
            {
                pattern = new ushort[,] {
                    {  42 },
                }
            };
            Recipe ironRefine = new Recipe(42, 1)
            {
                stationType = Station.Type.Furnace,
                expGiven = 2,
                shapeless = true,
                pattern = new ushort[,] {
                    {  148, 148, 197 },
                    {  148, 197, 197 },
                    {  197, 197, 197 },
                }
            };
            Recipe goldRefine = new Recipe(41, 1)
            {
                stationType = Station.Type.Furnace,
                expGiven = 3,
                shapeless = true,
                pattern = new ushort[,] {
                    {  672, 672, 49 },
                    {  672, 49, 49 },
                    {  49, 49, 49 },
                }
            };
            //old iron
            Recipe oldIron = new Recipe(148, 3)
            {
                stationType = Station.Type.Furnace,
                expGiven = 2,
                shapeless = true,
                pattern = new ushort[,] {
                    {  628, 197, 0 },
                    {  0, 0, 0 },
                    {  0, 0, 0 },
                }
            };
            Recipe oldgold = new Recipe(672, 3)
            {
                stationType = Station.Type.Furnace,
                expGiven = 3,
                shapeless = true,
                pattern = new ushort[,] {
                    {  629, 49, 0 },
                    {  0, 0, 0 },
                    {  0, 0, 0 },
                }
            };
            Recipe goldWire = new Recipe(550, 32)
            {
                pattern = new ushort[,] {
                    { 672, 672, 672 }
                }
            };
            Recipe oldIronSlab = new Recipe(149, 6)
            {
                pattern = new ushort[,] {
                    {  148, 148, 148 }
                }
            };
            Recipe oldIronWall = new Recipe(294, 6)
            {
                pattern = new ushort[,] {
                    {  148 },
                    {  148 },
                    {  148 }
                }
            };
            Recipe tile = new Recipe(208, 4)
            {
                pattern = new ushort[,] {
                    { 21, 148 },
                    { 148, 21 }
                }
            };
            //i = 159; //Iron fence-WE
            Recipe ironFence = new Recipe(159, 12)
            {
                pattern = new ushort[,] {
                    {  148, 148, 148 },
                    {  148, 148, 148 }
                }
            };
            //i = 161; //Iron cage
            Recipe ironCage = new Recipe(161, 4)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {    0, 159,   0 },
                    {  159,   0, 159 },
                    {    0, 159,   0 }
                }
            };
            Recipe gold = new Recipe(41, 1)
            {
                stationType = Station.Type.Furnace,
                expGiven = 6,
                shapeless = true,
                pattern = new ushort[,] {
                    {  629, 49, 49 },
                    {   49, 49, 49 },
                    {   49, 49, 49 },
                }
            };
            Recipe diamond = new Recipe(631, 1)
            {
                stationType = Station.Type.Furnace,
                expGiven = 10,
                shapeless = true,
                pattern = new ushort[,] {
                    {  630, 49, 49 },
                    {  49, 49, 49 },
                    {  49, 49, 49 },
                }
            };
            Recipe emerald = new Recipe(650, 1)
            {
                stationType = Station.Type.Furnace,
                expGiven = 15,
                shapeless = true,
                pattern = new ushort[,] {
                    {  649, 49, 49 },
                    {  49, 49, 49 },
                    {  49, 49, 49 },
                }
            };
            //glass
            Recipe glass = new Recipe(20, 1)
            {
                stationType = Station.Type.Furnace,
                pattern = new ushort[,] {
                    { 12 }
                }
            };
            Recipe glassPane = new Recipe(136, 6)
            {
                pattern = new ushort[,] {
                    {  20, 20, 20 },
                    {  20, 20, 20 }
                }
            };
            Recipe oldGlass = new Recipe(203, 1)
            {
                pattern = new ushort[,] {
                    { 57 },
                    { 20 }
                }
            };
            Recipe oldGlassPane = new Recipe(209, 6)
            {
                pattern = new ushort[,] {
                    {  203, 203, 203 },
                    {  203, 203, 203 }
                }
            };
            Recipe newGlass = new Recipe(471, 1)
            {
                pattern = new ushort[,] {
                    { 150 },
                    {  20 }
                }
            };
            Recipe newGlassPane = new Recipe(472, 6)
            {
                pattern = new ushort[,] {
                    {  471, 471, 471 },
                    {  471, 471, 471 }
                }
            };
            Recipe rope = new Recipe(51, 3)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 78 },
                    { 78 },
                    { 78 }
                }
            };
            //bread
            Recipe bread = new Recipe(640, 1)
            {
                stationType = Station.Type.Furnace,
                expGiven = 4,
                shapeless = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    { 145, 145, 145 },
                    {  0, 0, 0 },
                    {  0, 0, 0 },
                }
            };
            Recipe waffle = new Recipe(542, 1)
            {
                stationType = Station.Type.Furnace,
                expGiven = 4,
                shapeless = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    { 0, 0, 0 },
                    { 667, 0, 0 },
                    { 145, 145, 145 },
                }
            };
            Recipe leavesDense = new Recipe(666, 2)
            {
                pattern = new ushort[,] {
                    {  18, 18 },
                    {  18, 18 },
                }
            };
            Recipe pinkLeavesDense = new Recipe(686, 2)
            {
                pattern = new ushort[,] {
                    {  103, 103 },
                    {  103, 103 }
                }
            };
            Recipe leavesDry = new Recipe(104, 12)
            {
                pattern = new ushort[,] {
                    {  18, 18, 18 },
                    {  18, 25, 18 },
                    {  18, 18, 18 }
                }
            };
            Recipe leavesSlab = new Recipe(105, 6)
            {
                pattern = new ushort[,] {
                    {  18, 18, 18 }
                }
            };
            Recipe pinkLeavesSlab = new Recipe(246, 6)
            {
                pattern = new ushort[,] {
                    {  103, 103, 103 }
                }
            };
            Recipe orange = new Recipe(30, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 27, 35, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe pink = new Recipe(138, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 27, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe green = new Recipe(26, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 23, 35, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe lime = new Recipe(32, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 26, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe cyan = new Recipe(29, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 23, 26, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe lightblue = new Recipe(34, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 23, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe purple = new Recipe(22, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 23, 27, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe brown = new Recipe(25, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 3, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe black = new Recipe(21, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 36, 197, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe magenta = new Recipe(200, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 138, 22, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe gray = new Recipe(28, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 21, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe lightgray = new Recipe(31, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 28, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe poisonBread = new Recipe(652, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 131, 640, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe poisonMushroom = new Recipe(653, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 131, 604, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe poisonPie = new Recipe(654, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 131, 668, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe fire = new Recipe(54, 32)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 42, 13, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            Recipe die = new Recipe(236, 4)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 0, 0, 0 },
                    { 485, 486, 487 },
                    { 488, 489, 490 }
                }
            };
            Recipe zero = new Recipe(484, 8)
            {
                pattern = new ushort[,] {
                    { 45, 27, 45 },
                    { 27, 45, 27 },
                    { 45, 27, 45 }
                }
            };
            Recipe one = new Recipe(485, 8)
            {
                pattern = new ushort[,] {
                    { 27, 27, 45 },
                    { 45, 27, 45 },
                    { 27, 27, 27 }
                }
            };
            Recipe two = new Recipe(486, 8)
            {
                pattern = new ushort[,] {
                    { 27, 27, 45 },
                    { 45, 27, 27 },
                    { 27, 27, 45 }
                }
            };
            Recipe three = new Recipe(487, 8)
            {
                pattern = new ushort[,] {
                    { 27, 27, 27 },
                    { 45, 27, 27 },
                    { 27, 27, 27 }
                }
            };
            Recipe four = new Recipe(488, 8)
            {
                pattern = new ushort[,] {
                    { 27, 45, 27 },
                    { 27, 27, 27 },
                    { 45, 45, 27 }
                }
            };
            Recipe five = new Recipe(489, 8)
            {
                pattern = new ushort[,] {
                    { 45, 27, 27 },
                    { 45, 27, 45 },
                    { 27, 27, 45 }
                }
            };
            Recipe six = new Recipe(490, 8)
            {
                pattern = new ushort[,] {
                    { 27, 45, 45 },
                    { 27, 27, 27 },
                    { 27, 27, 27 }
                }
            };
            Recipe seven = new Recipe(491, 8)
            {
                pattern = new ushort[,] {
                    { 27, 27, 27 },
                    { 45, 45, 27 },
                    { 45, 45, 27 }
                }
            };
            Recipe eight = new Recipe(492, 8)
            {
                pattern = new ushort[,] {
                    { 45, 27, 27 },
                    { 27, 27, 27 },
                    { 27, 27, 45 }
                }
            };
            Recipe nine = new Recipe(493, 8)
            {
                pattern = new ushort[,] {
                    { 27, 27, 27 },
                    { 27, 27, 27 },
                    { 45, 45, 27 }
                }
            };
            Recipe a = new Recipe(494, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 21 },
                    { 21, 45, 21 }
                }
            };
            Recipe b = new Recipe(495, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 45 },
                    { 21, 21, 21 },
                    { 21, 21, 45 }
                }
            };
            Recipe c = new Recipe(496, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 45, 45 },
                    { 21, 21, 21 }
                }
            };
            Recipe d = new Recipe(497, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 45 },
                    { 21, 45, 21 },
                    { 21, 21, 45 }
                }
            };
            Recipe e = new Recipe(498, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 45 },
                    { 21, 21, 21 }
                }
            };
            Recipe f = new Recipe(499, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 45 },
                    { 21, 45, 45 }
                }
            };
            Recipe g = new Recipe(500, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 45 },
                    { 21, 21, 45 }
                }
            };
            Recipe h = new Recipe(501, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 21, 21 },
                    { 21, 45, 21 }
                }
            };
            Recipe i = new Recipe(502, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 45, 21, 45 },
                    { 21, 21, 21 }
                }
            };
            Recipe j = new Recipe(503, 8)
            {
                pattern = new ushort[,] {
                    { 45, 45, 21 },
                    { 21, 45, 21 },
                    { 21, 21, 21 }
                }
            };
            Recipe k = new Recipe(504, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 21, 45 },
                    { 21, 45, 21 }
                }
            };
            Recipe l = new Recipe(505, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 45 },
                    { 21, 45, 45 },
                    { 21, 21, 21 }
                }
            };
            Recipe m = new Recipe(506, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 21, 21 },
                    { 21, 21, 21 }
                }
            };
            Recipe n = new Recipe(507, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 45, 21 },
                    { 21, 45, 21 }
                }
            };
            Recipe o = new Recipe(508, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 45, 21 },
                    { 21, 21, 21 }
                }
            };
            Recipe p = new Recipe(509, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 21 },
                    { 21, 45, 45 }
                }
            };
            Recipe q = new Recipe(510, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 21 },
                    { 21, 21, 45 }
                }
            };
            Recipe r = new Recipe(511, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 45 },
                    { 21, 45, 21 }
                }
            };
            Recipe s = new Recipe(512, 8)
            {
                pattern = new ushort[,] {
                    { 45, 21, 21 },
                    { 21, 21, 45 },
                    { 45, 21, 21 }
                }
            };
            Recipe t = new Recipe(513, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 45, 21, 45 },
                    { 45, 21, 45 }
                }
            };
            Recipe u = new Recipe(514, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 45, 21 },
                    { 21, 21, 21 }
                }
            };
            Recipe v = new Recipe(515, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 45, 21 },
                    { 45, 21, 45 }
                }
            };
            Recipe w = new Recipe(516, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 21, 21 },
                    { 45, 21, 45 }
                }
            };
            Recipe x = new Recipe(517, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 45, 21, 45 },
                    { 21, 45, 21 }
                }
            };
            Recipe y = new Recipe(518, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 45, 21, 45 },
                    { 45, 21, 45 }
                }
            };
            Recipe z = new Recipe(519, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 45 },
                    { 45, 21, 45 },
                    { 45, 21, 21 }
                }
            };
            Recipe period = new Recipe(520, 8)
            {
                pattern = new ushort[,] {
                    { 45, 45, 45 },
                    { 45, 21, 45 },
                    { 45, 45, 45 }
                }
            };
            Recipe exc = new Recipe(521, 8)
            {
                pattern = new ushort[,] {
                    { 45, 21, 45 },
                    { 45, 45, 45 },
                    { 45, 21, 45 }
                }
            };
            Recipe slash = new Recipe(522, 8)
            {
                pattern = new ushort[,] {
                    { 45, 45, 21 },
                    { 45, 21, 45 },
                    { 21, 45, 45 }
                }
            };
            Recipe que = new Recipe(523, 8)
            {
                pattern = new ushort[,] {
                    { 45, 21, 21 },
                    { 45, 45, 21 },
                    { 45, 21, 45 }
                }
            };
            Recipe sign = new Recipe(171, 3)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5, 5, 5 },
                    {  5, 5, 5 },
                    {  0, 78, 0 }
                }
            };
            Recipe bookshelf = new Recipe(132, 3)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5, 5, 5 },
                    {  667, 667, 667 },
                    {  5, 5, 5 }
                }
            };
            Recipe bed = new Recipe(703, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  36, 36, 36 },
                    {  5, 5, 5 }
                }
            };
            Recipe pie = new Recipe(668, 1)
            {
                shapeless = true,
                expGiven = 8,
                usesParentID = true,
                stationType = Station.Type.Furnace,
                pattern = new ushort[,] {
                    {  648, 648, 0 },
                    {  667, 667, 0 },
                    {  145, 145, 0 }
                }
            };
            Recipe peachPie = new Recipe(698, 1)
            {
                shapeless = true,
                expGiven = 8,
                usesParentID = true,
                stationType = Station.Type.Furnace,
                pattern = new ushort[,] {
                    {  702, 702, 0 },
                    {  667, 667, 0 },
                    {  145, 145, 0 }
                }
            };
            Recipe powerSource = new Recipe(74, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1, 1 },
                    {  1, 672, 1 },
                    {  1, 1, 1 },
                }
            };
            Recipe lever = new Recipe(674, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1, 1 },
                    {  1, 74, 1 },
                    {  1, 1, 1 },
                }
            };
            Recipe pressure = new Recipe(610, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1 }
                }
            };
            Recipe spikes = new Recipe(178, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  0, 1, 0 },
                    {  1, 1, 1 },
                }
            };
            Recipe obSpikes = new Recipe(476, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 690, 178 },
                }
            };
            Recipe lamp = new Recipe(687, 4)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  20, 20, 20 },
                    {  20, 550, 20 },
                    {  20, 20, 20 },
                }
            };
            Recipe button = new Recipe(195, 1)
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 550, 550, 550 },
                    { 550, 1, 550 },
                    { 550, 550, 550 },
                }
            };
            Recipe piston = new Recipe(704, 1)
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5, 5, 5 },
                    {  1, 42, 1 },
                    {  1, 550, 1 },
                }
            };
            Recipe dispenser = new Recipe(439, 1)
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1, 1 },
                    {  1, 0, 1 },
                    {  1, 550, 1 },
                }
            };
            Recipe observer = new Recipe(415, 1)
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1, 1 },
                    {  550, 550, 61 },
                    {  1, 1, 1 },
                }
            };
            Recipe repeater = new Recipe(172, 1)
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1, 1 },
                    {  550, 550, 550 },
                    {  1, 1, 1 },
                }
            };
            Recipe strongWire = new Recipe(732, 5)
            {
                usesParentID = true,
                usesAlternateID = true,
                shapeless = true,
                pattern = new ushort[,] {
                    {  1, 550, 1 },
                    {  550, 550, 550 },
                    {  1, 550, 1 },
                }
            };
            Recipe stickyPiston = new Recipe(678, 1)
            {
                pattern = new ushort[,] {
                    {  677 },
                    {  704 },
                }
            };
            Recipe sticky = new Recipe(677, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  6 },
                }
            };
            Recipe packed = new Recipe(681, 1)
            {
                pattern = new ushort[,] {
                    {  60, 60 },
                    {  60, 60 }
                }
            };
            Recipe redCarpet = new Recipe(713, 3)
            {
                pattern = new ushort[,] {
                    {  27, 27 }
                }
            };
            Recipe orangeCarpet = new Recipe(714, 3)
            {
                pattern = new ushort[,] {
                    {  30, 30 }
                }
            };
            Recipe yellowCarpet = new Recipe(715, 3)
            {
                pattern = new ushort[,] {
                    {  35, 35 }
                }
            };
            Recipe limeCarpet = new Recipe(716, 3)
            {
                pattern = new ushort[,] {
                    {  32, 32 }
                }
            };
            Recipe greenCarpet = new Recipe(717, 3)
            {
                pattern = new ushort[,] {
                    {  26, 26 }
                }
            };
            Recipe lightblueCarpet = new Recipe(718, 3)
            {
                pattern = new ushort[,] {
                    {  34, 34 }
                }
            };
            Recipe cyanCarpet = new Recipe(719, 3)
            {
                pattern = new ushort[,] {
                    {  29, 29 }
                }
            };
            Recipe blueCarpet = new Recipe(720, 3)
            {
                pattern = new ushort[,] {
                    {  23, 23 }
                }
            };
            Recipe magentaCarpet = new Recipe(721, 3)
            {
                pattern = new ushort[,] {
                    {  200, 200 }
                }
            };
            Recipe pinkCarpet = new Recipe(722, 3)
            {
                pattern = new ushort[,] {
                    {  138, 138 }
                }
            };
            Recipe blackCarpet = new Recipe(723, 3)
            {
                pattern = new ushort[,] {
                    {  21, 21 }
                }
            };
            Recipe purpleCarpet = new Recipe(724, 3)
            {
                pattern = new ushort[,] {
                    {  22, 22 }
                }
            };
            Recipe grayCarpet = new Recipe(725, 3)
            {
                pattern = new ushort[,] {
                    {  28, 28 }
                }
            };
            Recipe lightgrayCarpet = new Recipe(726, 3)
            {
                pattern = new ushort[,] {
                    {  31, 31 }
                }
            };
            Recipe whiteCarpet = new Recipe(727, 3)
            {
                pattern = new ushort[,] {
                    {  36, 36 }
                }
            };
            Recipe brownCarpet = new Recipe(728, 3)
            {
                pattern = new ushort[,] {
                    {  25, 25 }
                }
            };
            Recipe snowBlock = new Recipe(140, 1)
            {
                pattern = new ushort[,] {
                    {  53, 53 }
                }
            };
            Recipe sponge = new Recipe(427, 3)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  36, 36, 36 },
                    {  36, 36, 36 },
                    {  36, 36, 36 },
                }
            };
            Recipe drysponge = new Recipe(427, 1)
            {
                stationType = Station.Type.Furnace,
                shapeless = true,
                pattern = new ushort[,] {
                    {  428, 49 }
                }
            };
            Recipe cobbledDeep = new Recipe(429, 1)
            {
                stationType = Station.Type.Furnace,
                pattern = new ushort[,] {
                    {  430 }
                }
            };
            Recipe polishedDeep = new Recipe(433, 4)
            {
                pattern = new ushort[,] {
                    {  430, 430 },
                    {  430, 430 }
                }
            };
            Recipe bricksDeep = new Recipe(436, 4)
            {
                pattern = new ushort[,] {
                    {  433, 433 },
                    {  433, 433 }
                }
            };
            Recipe tilesDeep = new Recipe(435, 4)
            {
                pattern = new ushort[,] {
                    {  436, 436 },
                    {  436, 436 }
                }
            };
            Recipe slabCobbleDeep = new Recipe(431, 6)
            {
                pattern = new ushort[,] {
                    {  430, 430, 430 },
                }
            };
            Recipe slabBrickDeep = new Recipe(437, 6)
            {
                pattern = new ushort[,] {
                    {  436, 436, 436 },
                }
            };
            Recipe chiseledDeep = new Recipe(434, 1)
            {
                shapeless = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    {  431, 431 },
                }
            };
            Recipe bricksNether = new Recipe(155, 4)
            {
                pattern = new ushort[,] {
                    {  48, 48 },
                    {  48, 48 },
                }
            };
            Recipe bricksNetherSlab = new Recipe(157, 6)
            {
                pattern = new ushort[,] {
                    {  155, 155, 155 },
                }
            };
            Recipe polishedBlack = new Recipe(458, 4)
            {
                pattern = new ushort[,] {
                    {  452, 452 },
                    {  452, 452 },
                }
            };
            Recipe polishedBlackSlab = new Recipe(460, 6)
            {
                pattern = new ushort[,] {
                    {  458, 458, 458 },
                }
            };
            Recipe brickBlack = new Recipe(466, 4)
            {
                pattern = new ushort[,] {
                    {  458, 458 },
                    {  458, 458 },
                }
            };
            Recipe brickBlackSlab = new Recipe(468, 6)
            {
                pattern = new ushort[,] {
                    {  466, 466, 466 },
                }
            };
            Recipe gilded = new Recipe(469, 8)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    {  452, 452, 452 },
                    {  452, 452, 452 },
                    {  452, 452, 672 },
                }
            };
            Recipe crackedBlack = new Recipe(474, 1)
            {
                stationType = Station.Type.Furnace,
                pattern = new ushort[,] {
                    { 466 },
                }
            };
            Recipe chiseledBlack = new Recipe(475, 1)
            {
                shapeless = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    {  460, 460 },
                }
            };
            Recipe barrier = new Recipe(767, 9)
            {
                pattern = new ushort[,] {
                    {  7, 7, 7 },
                    {  7, 650, 7 },
                    {  7, 7, 7 },
                }
            };
            SetupItems();
        }
    }
}
#endif