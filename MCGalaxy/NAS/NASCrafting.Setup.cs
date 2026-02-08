namespace MCGalaxy
{
    public partial class NASCrafting
    {
        public static void Setup()
        {
            NASRecipe wood = new(5, 4)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  17 }
                }
            };
            NASRecipe woodFall = new(657, 4)
            {
                usesParentID = true,
                shapeless = true,
                pattern = new ushort[,] {
                    {  17, 12, 12 },
                    {  12, 12, 12 },
                    {  12, 12, 12 }
                }
            };
            NASRecipe fakeDirt = new(685, 4)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 3, 55, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe fakeDirt2 = new(685, 4)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 3, 470, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe woodFall2 = new(656, 2)
            {
                usesParentID = true,
                shapeless = true,
                pattern = new ushort[,] {
                    {  242, 12, 12 },
                    {  12, 12, 12 },
                    {  12, 12, 12 }
                }
            };
            NASRecipe glassFall = new(655, 4)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    {  20, 12, 12 },
                    {  12, 12, 12 },
                    {  12, 12, 12 }
                }
            };
            NASRecipe trapdoor = new(659, 3)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5, 5, 5 },
                    {  5, 5, 5 }
                }
            };
            NASRecipe woodSlab = new(56, 6)
            {
                pattern = new ushort[,] {
                    {  5, 5, 5 }
                }
            };
            NASRecipe woodWall = new(182, 6)
            {
                pattern = new ushort[,] {
                    {  5 },
                    {  5 },
                    {  5 }
                }
            };
            NASRecipe woodStair = new(66, 6)
            {
                pattern = new ushort[,] {
                    {  5, 0, 0 },
                    {  5, 5, 0 },
                    {  5, 5, 5 }
                }
            };
            NASRecipe woodPole = new(78, 4)
            {
                pattern = new ushort[,] {
                    {  5 },
                    {  5 }
                }
            };
            NASRecipe fenceWE = new(94, 4)
            {
                pattern = new ushort[,] {
                    {  78, 79, 78 },
                    {  78, 79, 78 }
                }
            };
            NASRecipe fenceNS = new(94, 4)
            {
                pattern = new ushort[,] {
                    {  78, 80, 78 },
                    {  78, 80, 78 }
                }
            };
            NASRecipe darkDoor = new(55, 2)
            {
                pattern = new ushort[,] {
                    { 5, 5 },
                    { 5, 5 },
                    { 5, 5 }
                }
            };
            NASRecipe board = new(168, 6)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  56, 56, 56 }
                }
            };
            NASRecipe boardSideways = new(524, 6)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  182 },
                    {  182 },
                    {  182 }
                }
            };
            NASRecipe sprucewood = new(97, 4)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  250 }
                }
            };
            NASRecipe sprucewoodSlab = new(99, 6)
            {
                pattern = new ushort[,] {
                    {  97, 97, 97 }
                }
            };
            NASRecipe sprucewoodWall = new(190, 6)
            {
                pattern = new ushort[,] {
                    {  97 },
                    {  97 },
                    {  97 }
                }
            };
            NASRecipe sprucewoodStair = new(266, 6)
            {
                pattern = new ushort[,] {
                    {  97, 0, 0 },
                    {  97, 97, 0 },
                    {  97, 97, 97 }
                }
            };
            NASRecipe sprucewoodPole = new(252, 4)
            {
                pattern = new ushort[,] {
                    {  97 },
                    {  97 }
                }
            };
            NASRecipe sprucefenceWE = new(258, 4)
            {
                pattern = new ushort[,] {
                    {  252, 253, 252 },
                    {  252, 253, 252 }
                }
            };
            NASRecipe sprucefenceNS = new(258, 4)
            {
                pattern = new ushort[,] {
                    {  252, 254, 252 },
                    {  252, 254, 252 }
                }
            };
            NASRecipe birchwood = new(98, 4)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  242 }
                }
            };
            NASRecipe birchwoodSlab = new(101, 6)
            {
                pattern = new ushort[,] {
                    {  98, 98, 98 }
                }
            };
            NASRecipe birchwoodWall = new(186, 6)
            {
                pattern = new ushort[,] {
                    {  98 },
                    {  98 },
                    {  98 }
                }
            };
            NASRecipe birchwoodStair = new(262, 6)
            {
                pattern = new ushort[,] {
                    {  98, 0, 0 },
                    {  98, 98, 0 },
                    {  98, 98, 98 }
                }
            };
            NASRecipe birchwoodPole = new(255, 4)
            {
                pattern = new ushort[,] {
                    {  98 },
                    {  98 }
                }
            };
            NASRecipe birchfenceWE = new(260, 4)
            {
                pattern = new ushort[,] {
                    {  255, 256, 255 },
                    {  255, 256, 255 }
                }
            };
            NASRecipe birchfenceNS = new(260, 4)
            {
                pattern = new ushort[,] {
                    {  255, 257, 255 },
                    {  255, 257, 255 }
                }
            };
            NASRecipe lightDoor = new(470, 2)
            {
                pattern = new ushort[,] {
                    { 98, 98 },
                    { 98, 98 },
                    { 98, 98 }
                }
            };
            NASRecipe chest = new(216, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5,  5,  5 },
                    {  5, 0, 5 },
                    {  5,  5,  5 }
                }
            };
            NASRecipe barrel = new(143, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 5, 56, 5 },
                    { 5, 148, 5 },
                    { 5, 57, 5 },
                }
            };
            NASRecipe barrel2 = new(143, 1);
            barrel.usesAlternateID = true;
            barrel2.pattern = new ushort[,] {
                { 150 },
                {  17 },
                { 149 }
            };
            NASRecipe auto = new(413, 1)
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
            NASRecipe bedbeacon = new(612, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 20, 23, 20 },
                    { 23, 42, 23 },
                    { 20, 23, 20 }
                }
            };
            NASRecipe smith = new(676, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 42, 42 },
                    { 5, 5 },
                    { 5, 5 }
                }
            };
            NASRecipe tank = new(697, 1)
            {
                pattern = new ushort[,] {
                    { 690, 149, 690 },
                    { 690, 0, 690 },
                    { 690, 690, 690 },
                }
            };
            NASRecipe crate = new(142, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 5, 5 },
                    { 5, 5 }
                }
            };
            NASRecipe cryingObs = new(457, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 690, 690, 690 },
                    { 690, 239, 690 },
                    { 690, 690, 690 },
                }
            };
            NASRecipe stoneSlab = new(596, 6)
            {
                pattern = new ushort[,] {
                    {  1, 1, 1 }
                }
            };
            NASRecipe stoneWall = new(598, 6)
            {
                pattern = new ushort[,] {
                    {  1 },
                    {  1 },
                    {  1 }
                }
            };
            NASRecipe stoneStair = new(70, 6)
            {
                pattern = new ushort[,] {
                    {  1, 0, 0 },
                    {  1, 1, 0 },
                    {  1, 1, 1 }
                }
            };
            NASRecipe marker = new(64, 1)
            {
                pattern = new ushort[,] {
                    {  65, 65, 65 },
                    {  65,  0, 65 },
                    {  65, 65, 65 }
                }
            };
            NASRecipe stoneBrick = new(65, 6)
            {
                pattern = new ushort[,] {
                    {  1, 1, 0 },
                    {  0, 1, 1 },
                    {  1, 1, 0 }
                }
            };
            NASRecipe stoneBrickSlab = new(86, 6)
            {
                pattern = new ushort[,] {
                    {  65, 65, 65 }
                }
            };
            NASRecipe stoneBrickWall = new(278, 6)
            {
                pattern = new ushort[,] {
                    {  65 },
                    {  65 },
                    {  65 }
                }
            };
            NASRecipe stonePole = new(75, 4)
            {
                pattern = new ushort[,] {
                    {  65 },
                    {  65 }
                }
            };
            NASRecipe thinPole = new(211, 4)
            {
                pattern = new ushort[,] {
                    {  75 },
                    {  75 }
                }
            };
            NASRecipe linedStone = new(477, 4)
            {
                pattern = new ushort[,] {
                    {  65, 65 },
                    {  65, 65 }
                }
            };
            NASRecipe mossyCobble = new(181, 4)
            {
                pattern = new ushort[,] {
                    {  18, 162 },
                    {  162, 18 }
                }
            };
            NASRecipe mossyBricks = new(180, 4)
            {
                pattern = new ushort[,] {
                    {  181, 181 },
                    {  181, 181 }
                }
            };
            NASRecipe boulder = new(214, 4)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1 }
                }
            };
            NASRecipe nub = new(194, 4)
            {
                pattern = new ushort[,] {
                    {  214 }
                }
            };
            NASRecipe cobbleBrick = new(4, 4)
            {
                pattern = new ushort[,] {
                    {  162, 162 },
                    {  162, 162 }
                }
            };
            NASRecipe cobbleBrickSlab = new(50, 6)
            {
                pattern = new ushort[,] {
                    {  4, 4, 4 }
                }
            };
            NASRecipe cobbleBrickWall = new(133, 6)
            {
                pattern = new ushort[,] {
                    {  4, 4, 4 },
                    {  4, 4, 4 }
                }
            };
            NASRecipe cobblestone = new(162, 4)
            {
                pattern = new ushort[,] {
                    {  1, 1 },
                    {  1, 1 }
                }
            };
            NASRecipe cobblestoneSlab = new(163, 6)
            {
                pattern = new ushort[,] {
                    {  162, 162, 162 }
                }
            };
            NASRecipe sandstoneSlab = new(299, 6)
            {
                pattern = new ushort[,] {
                    {  52, 52, 52 }
                }
            };
            NASRecipe furnace = new(625, 1)
            {
                usesAlternateID = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    {  1,  1, 1 },
                    {  1,  0, 1 },
                    {  1,  1, 1 }
                }
            };
            NASRecipe concreteBlock = new(45, 4)
            {
                pattern = new ushort[,] {
                    {  4, 4 },
                    {  4, 4 }
                }
            };
            NASRecipe concreteSlab = new(44, 6)
            {
                pattern = new ushort[,] {
                    {  45, 45, 45 }
                }
            };
            NASRecipe concreteWall = new(282, 6)
            {
                pattern = new ushort[,] {
                    { 45 },
                    { 45 },
                    { 45 }
                }
            };
            NASRecipe concreteBrick = new(549, 4)
            {
                pattern = new ushort[,] {
                    {  45, 45 },
                    {  45, 45 }
                }
            };
            NASRecipe sandstone = new(52, 1)
            {
                pattern = new ushort[,] {
                    {  12, 12 },
                    {  12, 12 }
                }
            };
            NASRecipe stonePlate = new(135, 6)
            {
                pattern = new ushort[,] {
                    {  44, 44, 44 }
                }
            };
            NASRecipe quartzPillar = new(63, 2)
            {
                pattern = new ushort[,] {
                    {  61 },
                    {  61 }
                }
            };
            NASRecipe quartzWall = new(286, 6)
            {
                pattern = new ushort[,] {
                    {  61 },
                    {  61 },
                    {  61 }
                }
            };
            NASRecipe quartzSlab = new(84, 6)
            {
                pattern = new ushort[,] {
                    {  61, 61, 61 },
                }
            };
            NASRecipe quartzChis = new(235, 1)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                     {  84 },
                     {  84 }
                }
            };
            NASRecipe quartzStair = new(274, 6)
            {
                pattern = new ushort[,] {
                    {  61,  0,  0 },
                    {  61, 61,  0 },
                    {  61, 61, 61 }
                }
            };
            NASRecipe stonePlate2 = new(135, 6)
            {
                pattern = new ushort[,] {
                    {  58, 58, 58 }
                }
            };
            NASRecipe concreteStair = new(270, 6)
            {
                pattern = new ushort[,] {
                    {  45,  0,  0 },
                    {  45, 45,  0 },
                    {  45, 45, 45 }
                }
            };
            NASRecipe concreteCorner = new(480, 4)
            {
                pattern = new ushort[,] {
                    { 45 },
                    { 45 }
                }
            };
            NASRecipe charcoal = new(49, 2)
            {
                stationType = NASStationType.Furnace,
                shapeless = true,
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  17, 17, 17 },
                    {  17, 197, 17 },
                    {  17, 17, 17 }
                }
            };
            NASRecipe coalBlock = new(49, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    {  197, 197, 0 },
                    {  0, 0, 0 },
                    {  0, 0, 0 },
                }
            };
            NASRecipe hotCoals = new(239, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    {  49, 49, 0 },
                    {  0, 0, 0 },
                    {  0, 0, 0 },
                }
            };
            NASRecipe iron = new(42, 1)
            {
                stationType = NASStationType.Furnace,
                expGiven = 4,
                shapeless = true,
                pattern = new ushort[,] {
                    {  628, 197, 197 },
                    {  197, 197, 197 },
                    {  197, 197, 197 },
                }
            };
            NASRecipe nugIron = new(42, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    {  624, 624, 624 },
                    {  624, 624, 624 },
                    {  624, 624, 624 },
                }
            };
            NASRecipe ironNug = new(624, 9)
            {
                pattern = new ushort[,] {
                    {  42 },
                }
            };
            NASRecipe ironRefine = new(42, 1)
            {
                stationType = NASStationType.Furnace,
                expGiven = 2,
                shapeless = true,
                pattern = new ushort[,] {
                    {  148, 148, 197 },
                    {  148, 197, 197 },
                    {  197, 197, 197 },
                }
            };
            NASRecipe goldRefine = new(41, 1)
            {
                stationType = NASStationType.Furnace,
                expGiven = 3,
                shapeless = true,
                pattern = new ushort[,] {
                    {  672, 672, 49 },
                    {  672, 49, 49 },
                    {  49, 49, 49 },
                }
            };
            NASRecipe oldIron = new(148, 3)
            {
                stationType = NASStationType.Furnace,
                expGiven = 2,
                shapeless = true,
                pattern = new ushort[,] {
                    {  628, 197, 0 },
                    {  0, 0, 0 },
                    {  0, 0, 0 },
                }
            };
            NASRecipe oldgold = new(672, 3)
            {
                stationType = NASStationType.Furnace,
                expGiven = 3,
                shapeless = true,
                pattern = new ushort[,] {
                    {  629, 49, 0 },
                    {  0, 0, 0 },
                    {  0, 0, 0 },
                }
            };
            NASRecipe goldWire = new(550, 32)
            {
                pattern = new ushort[,] {
                    { 672, 672, 672 }
                }
            };
            NASRecipe oldIronSlab = new(149, 6)
            {
                pattern = new ushort[,] {
                    {  148, 148, 148 }
                }
            };
            NASRecipe oldIronWall = new(294, 6)
            {
                pattern = new ushort[,] {
                    {  148 },
                    {  148 },
                    {  148 }
                }
            };
            NASRecipe tile = new(208, 4)
            {
                pattern = new ushort[,] {
                    { 21, 148 },
                    { 148, 21 }
                }
            };
            NASRecipe ironFence = new(159, 12)
            {
                pattern = new ushort[,] {
                    {  148, 148, 148 },
                    {  148, 148, 148 }
                }
            };
            NASRecipe ironCage = new(161, 4)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {    0, 159,   0 },
                    {  159,   0, 159 },
                    {    0, 159,   0 }
                }
            };
            NASRecipe gold = new(41, 1)
            {
                stationType = NASStationType.Furnace,
                expGiven = 6,
                shapeless = true,
                pattern = new ushort[,] {
                    {  629, 49, 49 },
                    {   49, 49, 49 },
                    {   49, 49, 49 },
                }
            };
            NASRecipe diamond = new(631, 1)
            {
                stationType = NASStationType.Furnace,
                expGiven = 10,
                shapeless = true,
                pattern = new ushort[,] {
                    {  630, 49, 49 },
                    {  49, 49, 49 },
                    {  49, 49, 49 },
                }
            };
            NASRecipe emerald = new(650, 1)
            {
                stationType = NASStationType.Furnace,
                expGiven = 15,
                shapeless = true,
                pattern = new ushort[,] {
                    {  649, 49, 49 },
                    {  49, 49, 49 },
                    {  49, 49, 49 },
                }
            };
            NASRecipe glass = new(20, 1)
            {
                stationType = NASStationType.Furnace,
                pattern = new ushort[,] {
                    { 12 }
                }
            };
            NASRecipe glassPane = new(136, 6)
            {
                pattern = new ushort[,] {
                    {  20, 20, 20 },
                    {  20, 20, 20 }
                }
            };
            NASRecipe oldGlass = new(203, 1)
            {
                pattern = new ushort[,] {
                    { 57 },
                    { 20 }
                }
            };
            NASRecipe oldGlassPane = new(209, 6)
            {
                pattern = new ushort[,] {
                    {  203, 203, 203 },
                    {  203, 203, 203 }
                }
            };
            NASRecipe newGlass = new(471, 1)
            {
                pattern = new ushort[,] {
                    { 150 },
                    {  20 }
                }
            };
            NASRecipe newGlassPane = new(472, 6)
            {
                pattern = new ushort[,] {
                    {  471, 471, 471 },
                    {  471, 471, 471 }
                }
            };
            NASRecipe rope = new(51, 3)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 78 },
                    { 78 },
                    { 78 }
                }
            };
            NASRecipe bread = new(640, 1)
            {
                stationType = NASStationType.Furnace,
                expGiven = 4,
                shapeless = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    { 145, 145, 145 },
                    {  0, 0, 0 },
                    {  0, 0, 0 },
                }
            };
            NASRecipe waffle = new(542, 1)
            {
                stationType = NASStationType.Furnace,
                expGiven = 4,
                shapeless = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    { 0, 0, 0 },
                    { 667, 0, 0 },
                    { 145, 145, 145 },
                }
            };
            NASRecipe leavesDense = new(666, 2)
            {
                pattern = new ushort[,] {
                    {  18, 18 },
                    {  18, 18 },
                }
            };
            NASRecipe pinkLeavesDense = new(686, 2)
            {
                pattern = new ushort[,] {
                    {  103, 103 },
                    {  103, 103 }
                }
            };
            NASRecipe leavesDry = new(104, 12)
            {
                pattern = new ushort[,] {
                    {  18, 18, 18 },
                    {  18, 25, 18 },
                    {  18, 18, 18 }
                }
            };
            NASRecipe leavesSlab = new(105, 6)
            {
                pattern = new ushort[,] {
                    {  18, 18, 18 }
                }
            };
            NASRecipe pinkLeavesSlab = new(246, 6)
            {
                pattern = new ushort[,] {
                    {  103, 103, 103 }
                }
            };
            NASRecipe orange = new(30, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 27, 35, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe pink = new(138, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 27, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe green = new(26, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 23, 35, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe lime = new(32, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 26, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe cyan = new(29, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 23, 26, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe lightblue = new(34, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 23, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe purple = new(22, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 23, 27, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe brown = new(25, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 3, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe black = new(21, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 36, 197, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe magenta = new(200, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 138, 22, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe gray = new(28, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 21, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe lightgray = new(31, 2)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 28, 36, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe poisonBread = new(652, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 131, 640, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe poisonMushroom = new(653, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 131, 604, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe poisonPie = new(654, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 131, 668, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe fire = new(54, 32)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 42, 13, 0 },
                    { 0, 0, 0 },
                    { 0, 0, 0 },
                }
            };
            NASRecipe die = new(236, 4)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 0, 0, 0 },
                    { 485, 486, 487 },
                    { 488, 489, 490 }
                }
            };
            NASRecipe zero = new(484, 8)
            {
                pattern = new ushort[,] {
                    { 45, 27, 45 },
                    { 27, 45, 27 },
                    { 45, 27, 45 }
                }
            };
            NASRecipe one = new(485, 8)
            {
                pattern = new ushort[,] {
                    { 27, 27, 45 },
                    { 45, 27, 45 },
                    { 27, 27, 27 }
                }
            };
            NASRecipe two = new(486, 8)
            {
                pattern = new ushort[,] {
                    { 27, 27, 45 },
                    { 45, 27, 27 },
                    { 27, 27, 45 }
                }
            };
            NASRecipe three = new(487, 8)
            {
                pattern = new ushort[,] {
                    { 27, 27, 27 },
                    { 45, 27, 27 },
                    { 27, 27, 27 }
                }
            };
            NASRecipe four = new(488, 8)
            {
                pattern = new ushort[,] {
                    { 27, 45, 27 },
                    { 27, 27, 27 },
                    { 45, 45, 27 }
                }
            };
            NASRecipe five = new(489, 8)
            {
                pattern = new ushort[,] {
                    { 45, 27, 27 },
                    { 45, 27, 45 },
                    { 27, 27, 45 }
                }
            };
            NASRecipe six = new(490, 8)
            {
                pattern = new ushort[,] {
                    { 27, 45, 45 },
                    { 27, 27, 27 },
                    { 27, 27, 27 }
                }
            };
            NASRecipe seven = new(491, 8)
            {
                pattern = new ushort[,] {
                    { 27, 27, 27 },
                    { 45, 45, 27 },
                    { 45, 45, 27 }
                }
            };
            NASRecipe eight = new(492, 8)
            {
                pattern = new ushort[,] {
                    { 45, 27, 27 },
                    { 27, 27, 27 },
                    { 27, 27, 45 }
                }
            };
            NASRecipe nine = new(493, 8)
            {
                pattern = new ushort[,] {
                    { 27, 27, 27 },
                    { 27, 27, 27 },
                    { 45, 45, 27 }
                }
            };
            NASRecipe a = new(494, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 21 },
                    { 21, 45, 21 }
                }
            };
            NASRecipe b = new(495, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 45 },
                    { 21, 21, 21 },
                    { 21, 21, 45 }
                }
            };
            NASRecipe c = new(496, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 45, 45 },
                    { 21, 21, 21 }
                }
            };
            NASRecipe d = new(497, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 45 },
                    { 21, 45, 21 },
                    { 21, 21, 45 }
                }
            };
            NASRecipe e = new(498, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 45 },
                    { 21, 21, 21 }
                }
            };
            NASRecipe f = new(499, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 45 },
                    { 21, 45, 45 }
                }
            };
            NASRecipe g = new(500, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 45 },
                    { 21, 21, 45 }
                }
            };
            NASRecipe h = new(501, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 21, 21 },
                    { 21, 45, 21 }
                }
            };
            NASRecipe i = new(502, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 45, 21, 45 },
                    { 21, 21, 21 }
                }
            };
            NASRecipe j = new(503, 8)
            {
                pattern = new ushort[,] {
                    { 45, 45, 21 },
                    { 21, 45, 21 },
                    { 21, 21, 21 }
                }
            };
            NASRecipe k = new(504, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 21, 45 },
                    { 21, 45, 21 }
                }
            };
            NASRecipe l = new(505, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 45 },
                    { 21, 45, 45 },
                    { 21, 21, 21 }
                }
            };
            NASRecipe m = new(506, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 21, 21 },
                    { 21, 21, 21 }
                }
            };
            NASRecipe n = new(507, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 45, 21 },
                    { 21, 45, 21 }
                }
            };
            NASRecipe o = new(508, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 45, 21 },
                    { 21, 21, 21 }
                }
            };
            NASRecipe p = new(509, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 21 },
                    { 21, 45, 45 }
                }
            };
            NASRecipe q = new(510, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 21 },
                    { 21, 21, 45 }
                }
            };
            NASRecipe r = new(511, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 21, 21, 45 },
                    { 21, 45, 21 }
                }
            };
            NASRecipe s = new(512, 8)
            {
                pattern = new ushort[,] {
                    { 45, 21, 21 },
                    { 21, 21, 45 },
                    { 45, 21, 21 }
                }
            };
            NASRecipe t = new(513, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 21 },
                    { 45, 21, 45 },
                    { 45, 21, 45 }
                }
            };
            NASRecipe u = new(514, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 45, 21 },
                    { 21, 21, 21 }
                }
            };
            NASRecipe v = new(515, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 45, 21 },
                    { 45, 21, 45 }
                }
            };
            NASRecipe w = new(516, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 21, 21, 21 },
                    { 45, 21, 45 }
                }
            };
            NASRecipe x = new(517, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 45, 21, 45 },
                    { 21, 45, 21 }
                }
            };
            NASRecipe y = new(518, 8)
            {
                pattern = new ushort[,] {
                    { 21, 45, 21 },
                    { 45, 21, 45 },
                    { 45, 21, 45 }
                }
            };
            NASRecipe z = new(519, 8)
            {
                pattern = new ushort[,] {
                    { 21, 21, 45 },
                    { 45, 21, 45 },
                    { 45, 21, 21 }
                }
            };
            NASRecipe period = new(520, 8)
            {
                pattern = new ushort[,] {
                    { 45, 45, 45 },
                    { 45, 21, 45 },
                    { 45, 45, 45 }
                }
            };
            NASRecipe exc = new(521, 8)
            {
                pattern = new ushort[,] {
                    { 45, 21, 45 },
                    { 45, 45, 45 },
                    { 45, 21, 45 }
                }
            };
            NASRecipe slash = new(522, 8)
            {
                pattern = new ushort[,] {
                    { 45, 45, 21 },
                    { 45, 21, 45 },
                    { 21, 45, 45 }
                }
            };
            NASRecipe que = new(523, 8)
            {
                pattern = new ushort[,] {
                    { 45, 21, 21 },
                    { 45, 45, 21 },
                    { 45, 21, 45 }
                }
            };
            NASRecipe sign = new(171, 3)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5, 5, 5 },
                    {  5, 5, 5 },
                    {  0, 78, 0 }
                }
            };
            NASRecipe bookshelf = new(132, 3)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5, 5, 5 },
                    {  667, 667, 667 },
                    {  5, 5, 5 }
                }
            };
            NASRecipe bed = new(703, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  36, 36, 36 },
                    {  5, 5, 5 }
                }
            };
            NASRecipe pie = new(668, 1)
            {
                shapeless = true,
                expGiven = 8,
                usesParentID = true,
                stationType = NASStationType.Furnace,
                pattern = new ushort[,] {
                    {  648, 648, 0 },
                    {  667, 667, 0 },
                    {  145, 145, 0 }
                }
            };
            NASRecipe peachPie = new(698, 1)
            {
                shapeless = true,
                expGiven = 8,
                usesParentID = true,
                stationType = NASStationType.Furnace,
                pattern = new ushort[,] {
                    {  702, 702, 0 },
                    {  667, 667, 0 },
                    {  145, 145, 0 }
                }
            };
            NASRecipe powerSource = new(74, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1, 1 },
                    {  1, 672, 1 },
                    {  1, 1, 1 },
                }
            };
            NASRecipe lever = new(674, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1, 1 },
                    {  1, 74, 1 },
                    {  1, 1, 1 },
                }
            };
            NASRecipe pressure = new(610, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1 }
                }
            };
            NASRecipe spikes = new(178, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  0, 1, 0 },
                    {  1, 1, 1 },
                }
            };
            NASRecipe obSpikes = new(476, 1)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    { 690, 178 },
                }
            };
            NASRecipe lamp = new(687, 4)
            {
                usesParentID = true,
                pattern = new ushort[,] {
                    {  20, 20, 20 },
                    {  20, 550, 20 },
                    {  20, 20, 20 },
                }
            };
            NASRecipe button = new(195, 1)
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    { 550, 550, 550 },
                    { 550, 1, 550 },
                    { 550, 550, 550 },
                }
            };
            NASRecipe piston = new(704, 1)
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  5, 5, 5 },
                    {  1, 42, 1 },
                    {  1, 550, 1 },
                }
            };
            NASRecipe dispenser = new(439, 1)
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1, 1 },
                    {  1, 0, 1 },
                    {  1, 550, 1 },
                }
            };
            NASRecipe observer = new(415, 1)
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1, 1 },
                    {  550, 550, 61 },
                    {  1, 1, 1 },
                }
            };
            NASRecipe repeater = new(172, 1)
            {
                usesParentID = true,
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  1, 1, 1 },
                    {  550, 550, 550 },
                    {  1, 1, 1 },
                }
            };
            NASRecipe strongWire = new(732, 5)
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
            NASRecipe stickyPiston = new(678, 1)
            {
                pattern = new ushort[,] {
                    {  677 },
                    {  704 },
                }
            };
            NASRecipe sticky = new(677, 1)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  6 },
                }
            };
            NASRecipe packed = new(681, 1)
            {
                pattern = new ushort[,] {
                    {  60, 60 },
                    {  60, 60 }
                }
            };
            NASRecipe redCarpet = new(713, 3)
            {
                pattern = new ushort[,] {
                    {  27, 27 }
                }
            };
            NASRecipe orangeCarpet = new(714, 3)
            {
                pattern = new ushort[,] {
                    {  30, 30 }
                }
            };
            NASRecipe yellowCarpet = new(715, 3)
            {
                pattern = new ushort[,] {
                    {  35, 35 }
                }
            };
            NASRecipe limeCarpet = new(716, 3)
            {
                pattern = new ushort[,] {
                    {  32, 32 }
                }
            };
            NASRecipe greenCarpet = new(717, 3)
            {
                pattern = new ushort[,] {
                    {  26, 26 }
                }
            };
            NASRecipe lightblueCarpet = new(718, 3)
            {
                pattern = new ushort[,] {
                    {  34, 34 }
                }
            };
            NASRecipe cyanCarpet = new(719, 3)
            {
                pattern = new ushort[,] {
                    {  29, 29 }
                }
            };
            NASRecipe blueCarpet = new(720, 3)
            {
                pattern = new ushort[,] {
                    {  23, 23 }
                }
            };
            NASRecipe magentaCarpet = new(721, 3)
            {
                pattern = new ushort[,] {
                    {  200, 200 }
                }
            };
            NASRecipe pinkCarpet = new(722, 3)
            {
                pattern = new ushort[,] {
                    {  138, 138 }
                }
            };
            NASRecipe blackCarpet = new(723, 3)
            {
                pattern = new ushort[,] {
                    {  21, 21 }
                }
            };
            NASRecipe purpleCarpet = new(724, 3)
            {
                pattern = new ushort[,] {
                    {  22, 22 }
                }
            };
            NASRecipe grayCarpet = new(725, 3)
            {
                pattern = new ushort[,] {
                    {  28, 28 }
                }
            };
            NASRecipe lightgrayCarpet = new(726, 3)
            {
                pattern = new ushort[,] {
                    {  31, 31 }
                }
            };
            NASRecipe whiteCarpet = new(727, 3)
            {
                pattern = new ushort[,] {
                    {  36, 36 }
                }
            };
            NASRecipe brownCarpet = new(728, 3)
            {
                pattern = new ushort[,] {
                    {  25, 25 }
                }
            };
            NASRecipe snowBlock = new(140, 1)
            {
                pattern = new ushort[,] {
                    {  53, 53 }
                }
            };
            NASRecipe sponge = new(427, 3)
            {
                usesAlternateID = true,
                pattern = new ushort[,] {
                    {  36, 36, 36 },
                    {  36, 36, 36 },
                    {  36, 36, 36 },
                }
            };
            NASRecipe drysponge = new(427, 1)
            {
                stationType = NASStationType.Furnace,
                shapeless = true,
                pattern = new ushort[,] {
                    {  428, 49 }
                }
            };
            NASRecipe cobbledDeep = new(429, 1)
            {
                stationType = NASStationType.Furnace,
                pattern = new ushort[,] {
                    {  430 }
                }
            };
            NASRecipe polishedDeep = new(433, 4)
            {
                pattern = new ushort[,] {
                    {  430, 430 },
                    {  430, 430 }
                }
            };
            NASRecipe bricksDeep = new(436, 4)
            {
                pattern = new ushort[,] {
                    {  433, 433 },
                    {  433, 433 }
                }
            };
            NASRecipe tilesDeep = new(435, 4)
            {
                pattern = new ushort[,] {
                    {  436, 436 },
                    {  436, 436 }
                }
            };
            NASRecipe slabCobbleDeep = new(431, 6)
            {
                pattern = new ushort[,] {
                    {  430, 430, 430 },
                }
            };
            NASRecipe slabBrickDeep = new(437, 6)
            {
                pattern = new ushort[,] {
                    {  436, 436, 436 },
                }
            };
            NASRecipe chiseledDeep = new(434, 1)
            {
                shapeless = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    {  431, 431 },
                }
            };
            NASRecipe bricksNether = new(155, 4)
            {
                pattern = new ushort[,] {
                    {  48, 48 },
                    {  48, 48 },
                }
            };
            NASRecipe bricksNetherSlab = new(157, 6)
            {
                pattern = new ushort[,] {
                    {  155, 155, 155 },
                }
            };
            NASRecipe polishedBlack = new(458, 4)
            {
                pattern = new ushort[,] {
                    {  452, 452 },
                    {  452, 452 },
                }
            };
            NASRecipe polishedBlackSlab = new(460, 6)
            {
                pattern = new ushort[,] {
                    {  458, 458, 458 },
                }
            };
            NASRecipe brickBlack = new(466, 4)
            {
                pattern = new ushort[,] {
                    {  458, 458 },
                    {  458, 458 },
                }
            };
            NASRecipe brickBlackSlab = new(468, 6)
            {
                pattern = new ushort[,] {
                    {  466, 466, 466 },
                }
            };
            NASRecipe gilded = new(469, 8)
            {
                shapeless = true,
                pattern = new ushort[,] {
                    {  452, 452, 452 },
                    {  452, 452, 452 },
                    {  452, 452, 672 },
                }
            };
            NASRecipe crackedBlack = new(474, 1)
            {
                stationType = NASStationType.Furnace,
                pattern = new ushort[,] {
                    { 466 },
                }
            };
            NASRecipe chiseledBlack = new(475, 1)
            {
                shapeless = true,
                usesParentID = true,
                pattern = new ushort[,] {
                    {  460, 460 },
                }
            };
            NASRecipe barrier = new(767, 9)
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
