using MCGalaxy.Maths;
using System;
using System.Collections.Generic;
using NASBlockAction = MCGalaxy.NASAction<MCGalaxy.NASLevel, MCGalaxy.NASBlock, int, int, int>;
namespace MCGalaxy
{
    public class NASFloodSim
    {
        public NASLevel nl;
        public int xO,
            yO,
            zO,
            totalDistance,
            widthAndHeight,
            distanceHolesWereFoundAt;
        public ushort[] liquidSet;
        public bool[,] waterAtSpot;
        public List<Vec3S32> holes;
        public NASFloodSim(NASLevel nl, int xO, int yO, int zO, int totalDistance, ushort[] set)
        {
            this.nl = nl;
            this.xO = xO;
            this.yO = yO;
            this.zO = zO;
            this.totalDistance = totalDistance;
            liquidSet = set;
            waterAtSpot = new bool[totalDistance * 2 + 1, totalDistance * 2 + 1];
            widthAndHeight = waterAtSpot.GetLength(0);
            holes = new();
            distanceHolesWereFoundAt = totalDistance;
        }
        public List<Vec3S32> GetHoles(out int distance)
        {
            Flood(xO, zO, true);
            TryFlood(xO + 1, yO, zO);
            TryFlood(xO - 1, yO, zO);
            TryFlood(xO, yO, zO + 1);
            TryFlood(xO, yO, zO - 1);
            distance = distanceHolesWereFoundAt;
            return holes;
        }
        public void TryFlood(int x, int y, int z)
        {
            int distanceFromCenter = Math.Abs(x - xO) + Math.Abs(z - zO);
            if (distanceFromCenter > totalDistance || AlreadyFlooded(x, z))
                return;
            ushort here = nl.GetBlock(x, y, z);
            if (!(NASBlock.CanPhysicsKillThis(here) || NASBlock.IsPartOfSet(liquidSet, here) != -1))
                return;
            ushort below = nl.GetBlock(x, y - 1, z);
            if (NASBlock.CanPhysicsKillThis(below) || NASBlock.IsPartOfSet(liquidSet, below) != -1)
            {
                if (distanceFromCenter < distanceHolesWereFoundAt)
                {
                    holes.Clear();
                    holes.Add(new(x, y - 1, z));
                    distanceHolesWereFoundAt = distanceFromCenter;
                }
                else if (distanceFromCenter == distanceHolesWereFoundAt)
                    holes.Add(new(x, y - 1, z));
            }
            Flood(x, z, true);
            TryFlood(x + 1, y, z);
            TryFlood(x - 1, y, z);
            TryFlood(x, y, z + 1);
            TryFlood(x, y, z - 1);
        }
        public bool AlreadyFlooded(int x, int z)
        {
            int xI = x - xO,
                zI = z - zO;
            xI += totalDistance;
            zI += totalDistance;
            return xI < widthAndHeight &&
                zI < widthAndHeight &&
                xI >= 0 &&
                zI >= 0 && waterAtSpot[xI, zI];
        }
        public void Flood(int x, int z, bool value)
        {
            int xI = x - xO,
                zI = z - zO;
            xI += totalDistance;
            zI += totalDistance;
            waterAtSpot[xI, zI] = value;
        }
    }
    public partial class NASBlock
    {
        public static int LiquidInfiniteIndex = 0,
            LiquidSourceIndex = 1,
            LiquidWaterfallIndex = 2;
        public static ushort[] blocksPhysicsCanKill = new ushort[]
        {
            0,
            37,
            38,
            39,
            40,
            54,
            256|96,
            256|130,
            256|651,
            Block.FromRaw(644),
            Block.FromRaw(645),
            Block.FromRaw(646),
            Block.FromRaw(461),
            53
        },
        waterSet = new ushort[]
        {
            8, 9,
            256|639,
            256|632,
            256|633,
            256|634,
            256|635,
            256|636,
            256|637,
            256|638
        },
        lavaSet = new ushort[]
        {
            11, 10,
            256|695,
            256|691,
            256|692,
            256|693,
            256|694
        },
        grassSet = new ushort[]
        {
            2,
            256|119,
            256|129,
            256|139
        },
        tallGrassSet = new ushort[]
        {
            40,
            256|120,
            256|130,
            256|130
        },
        logSet = new ushort[]
        {
            15, 16, 17,
            256|144,
            256|242,
            256|656,
            256|657,
            256|240,
            256|241,
            256|248,
            256|249,
            256|250,
        },
        infinifire = new ushort[]
        {
            256|647,
            256|690,
            48
        },
        pistonUp =
        {
            Block.FromRaw(704),
            Block.FromRaw(705),
            Block.FromRaw(706)
        },
        stickyPistonUp =
        {
            Block.FromRaw(678),
            Block.FromRaw(679),
            Block.FromRaw(680)
        },
        pistonDown =
        {
            Block.FromRaw(707),
            Block.FromRaw(708),
            Block.FromRaw(709)
        },
        stickyPistonDown =
        {
            Block.FromRaw(710),
            Block.FromRaw(711),
            Block.FromRaw(712)
        },
        pistonNorth =
        {
            Block.FromRaw(389),
            Block.FromRaw(390),
            Block.FromRaw(391)
        },
        pistonEast =
        {
            Block.FromRaw(392),
            Block.FromRaw(393),
            Block.FromRaw(394)
        },
        pistonSouth =
        {
            Block.FromRaw(395),
            Block.FromRaw(396),
            Block.FromRaw(397)
        },
        pistonWest =
        {
            Block.FromRaw(398),
            Block.FromRaw(399),
            Block.FromRaw(400)
        },
        stickyPistonNorth =
        {
            Block.FromRaw(401),
            Block.FromRaw(402),
            Block.FromRaw(403)
        },
        stickyPistonEast =
        {
            Block.FromRaw(404),
            Block.FromRaw(405),
            Block.FromRaw(406)
        },
        stickyPistonSouth =
        {
            Block.FromRaw(407),
            Block.FromRaw(408),
            Block.FromRaw(409)
        },
        stickyPistonWest =
        {
            Block.FromRaw(410),
            Block.FromRaw(411),
            Block.FromRaw(412)
        },
        unpushable =
        {
            Block.FromRaw(690),
            Block.FromRaw(647),
            Block.FromRaw(216),
            Block.FromRaw(217),
            Block.FromRaw(218),
            Block.FromRaw(219),
            Block.FromRaw(602),
            Block.FromRaw(603),
            Block.FromRaw(143),
            Block.FromRaw(171),
            Block.FromRaw(54),
            Block.FromRaw(703),
            Block.FromRaw(7),
            Block.FromRaw(767),
            Block.FromRaw(674),
            Block.FromRaw(675),
            Block.FromRaw(195),
            Block.FromRaw(196),
            Block.FromRaw(172),
            Block.FromRaw(173),
            Block.FromRaw(174),
            Block.FromRaw(175),
            Block.FromRaw(176),
            Block.FromRaw(177),
            Block.FromRaw(612),
            Block.FromRaw(613),
            Block.FromRaw(614),
            Block.FromRaw(615),
            Block.FromRaw(616),
            Block.FromRaw(617),
            Block.FromRaw(413),
            Block.FromRaw(414),
            Block.FromRaw(439),
            Block.FromRaw(440),
            Block.FromRaw(441),
            Block.FromRaw(442),
            Block.FromRaw(443),
            Block.FromRaw(444),
            Block.FromRaw(673),
            Block.FromRaw(457),
        },
        wireSetActive =
        {
            256|683,
            256|682,
            256|684
        },
        wireSetInactive =
        {
            256|551,
            256|550,
            256|552
        },
        fixedWireSetInactive =
        {
            256|732,
            256|733,
            256|734
        },
        fixedWireSetActive =
        {
            256|735,
            256|736,
            256|737
        },
        repeaterSetActive =
        {
            256|613,
            256|614,
            256|615,
            256|616,
            256|617,
            256|618,
        },
        repeaterSetInactive =
        {
            256|172,
            256|173,
            256|174,
            256|175,
            256|176,
            256|177,
        },
        leafSet = new ushort[]
        {
            18
        },
        wheatSet = new ushort[]
        {
            Block.FromRaw(644),
            Block.FromRaw(645),
            Block.FromRaw(646),
            Block.FromRaw(461)
        },
        ironSet = new ushort[]
        {
            Block.FromRaw(729),
            Block.FromRaw(730),
            Block.FromRaw(731),
            Block.FromRaw(479)
        },
        soilForPlants = new ushort[]
        {
            3,
            256|144,
            256|685
        },
        soilForIron = new ushort[]
        {
            48,
            256|452,
            256|451
        };
        public static NASBlockAction FloodAction(ushort[] set) => (nl, nasBlock, x, y, z) =>
        {
            if (CanInfiniteFloodKillThis(nl, x, y - 1, z, set))
            {
                nl.SetBlock(x, y - 1, z, set[LiquidInfiniteIndex]);
                return;
            }
            if (CanInfiniteFloodKillThis(nl, x + 1, y, z, set))
                nl.SetBlock(x + 1, y, z, set[LiquidInfiniteIndex]);
            if (CanInfiniteFloodKillThis(nl, x - 1, y, z, set))
                nl.SetBlock(x - 1, y, z, set[LiquidInfiniteIndex]);
            if (CanInfiniteFloodKillThis(nl, x, y, z + 1, set))
                nl.SetBlock(x, y, z + 1, set[LiquidInfiniteIndex]);
            if (CanInfiniteFloodKillThis(nl, x, y, z - 1, set))
                nl.SetBlock(x, y, z - 1, set[LiquidInfiniteIndex]);
        };
        public static bool CanInfiniteFloodKillThis(NASLevel nl, int x, int y, int z, ushort[] set)
        {
            ushort here = nl.GetBlock(x, y, z);
            return CanPhysicsKillThis(here) || IsPartOfSet(set, here) > LiquidInfiniteIndex;
        }
        public static bool CanPhysicsKillThis(ushort block)
        {
            for (int i = 0; i < blocksPhysicsCanKill.Length; i++)
                if (block == blocksPhysicsCanKill[i])
                    return true;
            return false;
        }
        public static bool IsThisLiquid(ushort block) => IsPartOfSet(waterSet, block) != -1;
        public static int IsPartOfSet(ushort[] set, ushort block)
        {
            for (int i = 0; i < set.Length; i++)
                if (set[i] == block)
                    return i;
            return -1;
        }
        public static int CanReplaceBlockAt(NASLevel nl, int x, int y, int z, ushort[] set, int spreadIndex)
        {
            ushort hereBlock = nl.GetBlock(x, y, z);
            return nl.GetBlock(x, y - 1, z) == Block.FromRaw(703)
                ? -1
                : CanPhysicsKillThis(hereBlock) ? spreadIndex + 1 : IsPartOfSet(set, hereBlock);
        }
        public static bool CanLiquidLive(NASLevel nl, ushort[] set, int index, int x, int y, int z)
        {
            ushort neighbor = nl.GetBlock(x, y, z);
            return neighbor == set[index - 1] ||
                neighbor == set[LiquidSourceIndex] ||
                neighbor == set[LiquidWaterfallIndex];
        }
        public static NASBlockAction LimitedFloodAction(ushort[] set, int index) => (nl, nasBlock, x, y, z) =>
        {
            if (y >= 200 && set == waterSet)
            {
                nl.SetBlock(x, y, z, 60);
                return;
            }
            if (nl.biome < 0 && set == waterSet)
            {
                nl.SetBlock(x, y, z, 0);
                return;
            }
            ushort hereBlock = nl.GetBlock(x, y, z);
            ushort[] aboveHere = { nl.GetBlock(x, y + 1, z), nl.GetBlock(x + 1, y, z), nl.GetBlock(x - 1, y, z), nl.GetBlock(x, y, z + 1), nl.GetBlock(x, y, z - 1) };
            if (IsPartOfSet(lavaSet, hereBlock) != -1)
            {
                if ((IsPartOfSet(waterSet, aboveHere[0]) != -1) ||
                    (IsPartOfSet(waterSet, aboveHere[1]) != -1) ||
                    (IsPartOfSet(waterSet, aboveHere[2]) != -1) ||
                    (IsPartOfSet(waterSet, aboveHere[3]) != -1) ||
                    (IsPartOfSet(waterSet, aboveHere[4]) != -1))
                {
                    switch (hereBlock)
                    {
                        case 10:
                        case 11:
                            nl.SetBlock(x, y, z, 256 | 690);
                            return;
                    }
                    nl.SetBlock(x, y, z, 256 | 162);
                    return;
                }
            }
            if (index > LiquidSourceIndex)
            {
                if (index == LiquidWaterfallIndex)
                {
                    if (IsPartOfSet(set, aboveHere[0]) == -1)
                    {
                        nl.SetBlock(x, y, z, 0);
                        return;
                    }
                }
                else
                {
                    if (!(CanLiquidLive(nl, set, index, x + 1, y, z) ||
                        CanLiquidLive(nl, set, index, x - 1, y, z) ||
                        CanLiquidLive(nl, set, index, x, y, z + 1) ||
                        CanLiquidLive(nl, set, index, x, y, z - 1)))
                    {
                        nl.SetBlock(x, y, z, 0);
                        return;
                    }
                }
            }
            if (set == waterSet && hereBlock == set[3])
            {
                int borders = 0;
                if (nl.GetBlock(x + 1, y, z) == set[LiquidSourceIndex])
                    borders++;
                if (nl.GetBlock(x - 1, y, z) == set[LiquidSourceIndex])
                    borders++;
                if (nl.GetBlock(x, y, z + 1) == set[LiquidSourceIndex])
                    borders++;
                if (nl.GetBlock(x, y, z - 1) == set[LiquidSourceIndex])
                    borders++;
                if (borders > 1)
                {
                    nl.SetBlock(x, y, z, set[LiquidSourceIndex]);
                    return;
                }
            }
            ushort below = nl.GetBlock(x, y - 1, z);
            int belowIndex = IsPartOfSet(set, below);
            if (CanPhysicsKillThis(below) || belowIndex != -1)
            {
                if (!CanPhysicsKillThis(below) && belowIndex <= LiquidWaterfallIndex)
                    return;
                nl.SetBlock(x, y - 1, z, set[LiquidWaterfallIndex]);
                return;
            }
            if (index == set.Length - 1)
                return;
            int spreadIndex = (index < LiquidWaterfallIndex + 1) ? LiquidWaterfallIndex + 1 : index + 1;
            ushort spreadBlock = set[spreadIndex];
            CanFlowInDirection(nl, x, y, z, set, spreadIndex,
                               out bool posX,
                               out bool negX,
                               out bool posZ,
                               out bool negZ);
            if (posX)
                nl.SetBlock(x + 1, y, z, spreadBlock);
            if (negX)
                nl.SetBlock(x - 1, y, z, spreadBlock);
            if (posZ)
                nl.SetBlock(x, y, z + 1, spreadBlock);
            if (negZ)
                nl.SetBlock(x, y, z - 1, spreadBlock);
        };
        public static void CanFlowInDirection(NASLevel nl, int x, int y, int z,
                                       ushort[] set, int spreadIndex,
                                       out bool xPos,
                                       out bool xNeg,
                                       out bool zPos,
                                       out bool zNeg
                                      )
        {
            xPos = true;
            xNeg = true;
            zPos = true;
            zNeg = true;
            bool xBlockedPos = false,
                xBlockedNeg = false,
                zBlockedPos = false,
                zBlockedNeg = false;
            List<Vec3S32> holes = HolesInRange(nl, x, y, z, 4, set, out int originalHoleDistance);
            if (holes.Count > 0)
            {
                CloserToAHole(x, y, z, 1, 0, originalHoleDistance, holes, ref xPos);
                CloserToAHole(x, y, z, -1, 0, originalHoleDistance, holes, ref xNeg);
                CloserToAHole(x, y, z, 0, 1, originalHoleDistance, holes, ref zPos);
                CloserToAHole(x, y, z, 0, -1, originalHoleDistance, holes, ref zNeg);
            }
            int neighborIndex1 = CanReplaceBlockAt(nl, x + 1, y, z, set, spreadIndex),
                neighborIndex2 = CanReplaceBlockAt(nl, x - 1, y, z, set, spreadIndex),
                neighborIndex3 = CanReplaceBlockAt(nl, x, y, z + 1, set, spreadIndex),
                neighborIndex4 = CanReplaceBlockAt(nl, x, y, z - 1, set, spreadIndex);
            if (neighborIndex1 == -1)
                xBlockedPos = true;
            if (neighborIndex2 == -1)
                xBlockedNeg = true;
            if (neighborIndex3 == -1)
                zBlockedPos = true;
            if (neighborIndex4 == -1)
                zBlockedNeg = true;
            xPos = xPos && !xBlockedPos;
            xNeg = xNeg && !xBlockedNeg;
            zPos = zPos && !zBlockedPos;
            zNeg = zNeg && !zBlockedNeg;
            if (!(xPos || xNeg || zPos || zNeg))
            {
                xPos = !xBlockedPos;
                xNeg = !xBlockedNeg;
                zPos = !zBlockedPos;
                zNeg = !zBlockedNeg;
            }
            xPos = xPos && neighborIndex1 > spreadIndex;
            xNeg = xNeg && neighborIndex2 > spreadIndex;
            zPos = zPos && neighborIndex3 > spreadIndex;
            zNeg = zNeg && neighborIndex4 > spreadIndex;
        }
        public static void CloserToAHole(int x, int _, int z, int xDiff, int zDiff, int originalHoleDistance, List<Vec3S32> holes, ref bool canFlowDir)
        {
            x += xDiff;
            z += zDiff;
            foreach (Vec3S32 hole in holes)
            {
                int dist = Math.Abs(x - hole.X) + Math.Abs(z - hole.Z);
                if (dist < originalHoleDistance)
                {
                    canFlowDir = true;
                    return;
                }
            }
            canFlowDir = false;
        }
        public static List<Vec3S32> HolesInRange(NASLevel nl, int x, int y, int z, int totalDistance, ushort[] set, out int distance) => new NASFloodSim(nl, x, y, z, totalDistance, set).GetHoles(out distance);
        public static NASBlockAction FallingBlockAction(ushort serverushort) => (nl, nasBlock, x, y, z) =>
        {
            ushort blockUnder = nl.GetBlock(x, y - 1, z);
            if (nl.GetBlock(x, y - 2, z) == Block.FromRaw(703))
                return;
            if (CanPhysicsKillThis(blockUnder) || IsPartOfSet(waterSet, blockUnder) != -1)
            {
                nl.SetBlock(x, y, z, 0);
                nl.SetBlock(x, y - 1, z, serverushort);
            }
        };
        public static NASBlockAction GrassBlockAction(ushort grass, ushort dirt) => (nl, nasBlock, x, y, z) =>
        {
            if (grass == Block.FromRaw(139) && nl.biome != 2)
                nl.SetBlock(x, y, z, Block.FromRaw(129));
            ushort aboveHere = nl.GetBlock(x, y + 1, z);
            if (!nl.lvl.LightPasses(aboveHere))
                nl.SetBlock(x, y, z, dirt);
        };
        public static NASBlockAction DirtBlockAction(ushort[] grassSet, ushort _) => (nl, nasBlock, x, y, z) =>
        {
            ushort aboveHere = nl.GetBlock(x, y + 1, z);
            if (!nl.lvl.LightPasses(aboveHere))
                return;
            for (int xOff = -1; xOff <= 1; xOff++)
                for (int yOff = -1; yOff <= 1; yOff++)
                    for (int zOff = -1; zOff <= 1; zOff++)
                    {
                        if (xOff == 0 && yOff == -1 && zOff == 0)
                            continue;
                        ushort neighbor = nl.GetBlock(x + xOff, y + yOff, z + zOff);
                        int setIndex = IsPartOfSet(grassSet, neighbor);
                        if (setIndex == 3 && nl.biome != 2)
                            setIndex = 2;
                        if (setIndex != 3 && nl.biome == 2 && setIndex != -1)
                            setIndex = 3;
                        if (setIndex == -1)
                            continue;
                        nl.SetBlock(x, y, z, grassSet[setIndex], true);
                        if (nl.GetBlock(x, y + 1, z) == 0)
                            switch (r.Next(0, 20))
                            {
                                case 0:
                                    nl.SetBlock((ushort)x, (ushort)(y + 1), (ushort)z, 256 | 130);
                                    break;
                                default:
                                    {
                                        int flowerChance = r.Next(0, 100);
                                        switch (flowerChance)
                                        {
                                            case 0:
                                                nl.SetBlock(x, y + 1, z, 256 | 96);
                                                break;
                                            default:
                                                switch (flowerChance)
                                                {
                                                    case 1:
                                                        nl.SetBlock(x, y + 1, z, 37);
                                                        break;
                                                    default:
                                                        switch (flowerChance)
                                                        {
                                                            case 2:
                                                                nl.SetBlock(x, y + 1, z, 38);
                                                                break;
                                                            default:
                                                                switch (flowerChance)
                                                                {
                                                                    case 3:
                                                                        nl.SetBlock(x, y + 1, z, 256 | 651);
                                                                        break;
                                                                    default:
                                                                        switch (flowerChance)
                                                                        {
                                                                            case 4:
                                                                                if (r.Next(0, 20) == 0)
                                                                                    nl.SetBlock(x, y + 1, z, 256 | 604);
                                                                                break;
                                                                            default:
                                                                                switch (flowerChance)
                                                                                {
                                                                                    case 5:
                                                                                        nl.SetBlock(x, y + 1, z, 256 | 201);
                                                                                        break;
                                                                                    default:
                                                                                        if (nl.biome == 2)
                                                                                            nl.SetBlock(x, y + 1, z, 53);
                                                                                        break;
                                                                                }
                                                                                break;
                                                                        }
                                                                        break;
                                                                }
                                                                break;
                                                        }
                                                        break;
                                                }
                                                break;
                                        }
                                        break;
                                    }
                            }
                    }
            return;
        };
        public static NASBlockAction ObserverActivateAction(int type) => (nl, nasBlock, x, y, z) =>
        {
            if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
            {
                nl.blockEntities.Add(x + " " + y + " " + z, new());
                nl.blockEntities[x + " " + y + " " + z].type = type;
            }
            nl.blockEntities[x + " " + y + " " + z].strength = 15;
            nl.SetBlock(x, y, z, (ushort)(nl.lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z) + 6));
        };
        public static NASBlockAction ObserverDeactivateAction(int type) => (nl, nasBlock, x, y, z) =>
        {
            if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
            {
                nl.blockEntities.Add(x + " " + y + " " + z, new());
                nl.blockEntities[x + " " + y + " " + z].type = type;
            }
            nl.blockEntities[x + " " + y + " " + z].strength = 0;
            nl.SetBlock(x, y, z, (ushort)(nl.lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z) - 6));
        };
        public static NASBlockAction LeafBlockAction(ushort[] _, ushort leaf) => (nl, nasBlock, x, y, z) =>
        {
            bool canLive = false;
            int iteration = 1;
            IsThereLog(nl, x + 1, y, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y + 1, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y, z + 1, leaf, iteration, ref canLive);
            IsThereLog(nl, x - 1, y, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y - 1, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y, z - 1, leaf, iteration, ref canLive);
            if (canLive)
                return;
            nl.SetBlock(x, y, z, 0);
            if (r.Next(0, 384) == 0 && CanPhysicsKillThis(nl.GetBlock(x, y - 1, z)))
            {
                if (leaf == 18)
                    nl.SetBlock(x, y - 1, z, Block.FromRaw(648));
                if (leaf == Block.FromRaw(103))
                    nl.SetBlock(x, y - 1, z, Block.FromRaw(702));
            }
        };
        public static NASBlockAction GrowAction(ushort grow) => (nl, nasBlock, x, y, z) =>
        {
            if (grow == Block.FromRaw(667))
            {
                switch (nl.GetBlock(x, y - 1, z))
                {
                    case 12:
                        if (!((nl.GetBlock(x - 1, y - 1, z) == 8) ||
                                                                                                               (nl.GetBlock(x + 1, y - 1, z) == 8) ||
                                                                                                               (nl.GetBlock(x, y - 1, z + 1) == 8) ||
                                                                                                               (nl.GetBlock(x, y - 1, z - 1) == 8) ||
                                                                                                               (nl.GetBlock(x - 1, y - 1, z) == 9) ||
                                                                                                               (nl.GetBlock(x + 1, y - 1, z) == 9) ||
                                                                                                               (nl.GetBlock(x, y - 1, z + 1) == 9) ||
                                                                                                               (nl.GetBlock(x, y - 1, z - 1) == 9)))
                            return;
                        break;
                    default:
                        if (!((nl.GetBlock(x - 1, y - 2, z) == 8) ||
                                  (nl.GetBlock(x + 1, y - 2, z) == 8) ||
                                  (nl.GetBlock(x, y - 2, z + 1) == 8) ||
                                  (nl.GetBlock(x, y - 2, z - 1) == 8) ||
                                  (nl.GetBlock(x - 1, y - 2, z) == 9) ||
                                  (nl.GetBlock(x + 1, y - 2, z) == 9) ||
                                  (nl.GetBlock(x, y - 2, z + 1) == 9) ||
                                  (nl.GetBlock(x, y - 2, z - 1) == 9)))
                            return;
                        break;
                }
            }
            if (!((grow == nl.GetBlock(x, y - 1, z) && nl.GetBlock(x, y - 2, z) == 12) || nl.GetBlock(x, y - 1, z) == 12) || ((nl.GetBlock(x, y - 1, z) == grow) && (nl.GetBlock(x, y - 2, z) == grow)) | (nl.GetBlock(x, y + 1, z) != 0))
                return;
            nl.SetBlock(x, y + 1, z, grow);
        };
        public static NASBlockAction VineGrowAction(ushort grow) => (nl, nasBlock, x, y, z) =>
        {
            if (nl.GetBlock(x, y + 1, z) == grow && nl.GetBlock(x, y + 2, z) == grow)
                return;
            if (nl.GetBlock(x, y - 1, z) == 0)
                nl.SetBlock(x, y - 1, z, grow);
        };
        public static NASBlockAction VineDeathAction() => (nl, nasBlock, x, y, z) =>
        {
            if (IsPartOfSet(blocksPhysicsCanKill, nl.GetBlock(x, y + 1, z)) != -1)
                nl.SetBlock(x, y, z, 0);
        };
        public static NASBlockAction LilyAction() => (nl, nasBlock, x, y, z) =>
        {
            if (IsPartOfSet(waterSet, nl.GetBlock(x, y - 1, z)) == -1)
                nl.SetBlock(x, y, z, 0);
        };
        public static void IsThereLog(NASLevel nl, int x, int y, int z, ushort leaf, int iteration, ref bool canLive)
        {
            if (canLive)
                return;
            ushort hereBlock = nl.GetBlock(x, y, z);
            if (IsPartOfSet(logSet, hereBlock) != -1)
            {
                canLive = true;
                return;
            }
            if (hereBlock != leaf || (iteration >= 10 && leaf == Block.FromRaw(104)) || (iteration >= 5 && leaf != Block.FromRaw(104)))
                return;
            iteration++;
            IsThereLog(nl, x + 1, y, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y + 1, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y, z + 1, leaf, iteration, ref canLive);
            IsThereLog(nl, x - 1, y, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y - 1, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y, z - 1, leaf, iteration, ref canLive);
        }
        public static NASBlockAction FireAction() => (nl, nasBlock, x, y, z) =>
        {
            if (IsPartOfSet(infinifire, nl.GetBlock(x, y - 1, z)) == -1)
                switch (r.Next(0, 8))
                {
                    case 0:
                        nl.SetBlock(x, y, z, Block.FromRaw(131));
                        break;
                    default:
                        nl.SetBlock(x, y, z, 0);
                        break;
                }
        };
        public static NASBlockAction LampAction(ushort on, ushort off, ushort me) => (nl, nasBlock, x, y, z) =>
        {
            NASBlockEntity[] b = new NASBlockEntity[6];
            if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                b[0] = nl.blockEntities[x + " " + (y - 1) + " " + z];
            if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
            if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
            if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                b[5] = nl.blockEntities[x + " " + (y + 1) + " " + z];
            bool powered =
                    ((b[5] != null) && b[5].strength > 0 && (b[5].type == 1 || b[5].type == 4 || b[5].type == 5 || b[5].type == 12)) ||
                    ((b[0] != null) && b[0].strength > 0 && (b[0].type == 1 || b[0].type == 4 || b[0].type == 6 || b[0].type == 12)) ||
                    ((b[1] != null) && b[1].strength > 0 && (b[1].type == 0 || b[1].type == 4 || b[1].type == 10 || b[1].type == 11)) ||
                    ((b[2] != null) && b[2].strength > 0 && (b[2].type == 0 || b[2].type == 4 || b[2].type == 8 || b[2].type == 11)) ||
                    ((b[3] != null) && b[3].strength > 0 && (b[3].type == 2 || b[3].type == 4 || b[3].type == 7 || b[3].type == 13)) ||
                    ((b[4] != null) && b[4].strength > 0 && (b[4].type == 2 || b[4].type == 4 || b[4].type == 9 || b[4].type == 13));
            if (powered)
            {
                if (off == me)
                    nl.SetBlock(x, y, z, on);
            }
            else
            {
                if (on == me)
                    nl.SetBlock(x, y, z, off);
            }
        };
        public static NASBlockAction UnrefinedGoldAction(ushort on, ushort off, ushort me) => (nl, nasBlock, x, y, z) =>
        {
            NASBlockEntity[] b = new NASBlockEntity[6];
            if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                b[0] = nl.blockEntities[x + " " + (y - 1) + " " + z];
            if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
            if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
            if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                b[5] = nl.blockEntities[x + " " + (y + 1) + " " + z];
            bool powered =
                    ((b[5] != null) && b[5].strength > 0 && (b[5].type == 5)) ||
                    ((b[0] != null) && b[0].strength > 0 && (b[0].type == 6)) ||
                    ((b[1] != null) && b[1].strength > 0 && (b[1].type == 10)) ||
                    ((b[2] != null) && b[2].strength > 0 && (b[2].type == 8)) ||
                    ((b[3] != null) && b[3].strength > 0 && (b[3].type == 7)) ||
                    ((b[4] != null) && b[4].strength > 0 && (b[4].type == 9));
            if (powered)
            {
                if (off == me)
                {
                    if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                        return;
                    nl.SetBlock(x, y, z, on);
                    nl.blockEntities[x + " " + y + " " + z].strength = 15;
                }
            }
            else
            {
                if (on == me)
                {
                    if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                        return;
                    nl.SetBlock(x, y, z, off);
                    nl.blockEntities[x + " " + y + " " + z].strength = 0;
                }
            }
        };
        public static NASBlockAction SidewaysPistonAction(string type, string axis, int dir, ushort[] pistonSet, bool sticky = false) => (nl, nasBlock, x, y, z) =>
        {
            int changeX = 0,
            changeZ = 0,
            changeY = 0;
            ushort[] dontpush =
            {
                    0, 0, 0, 0,
                    Block.FromRaw(680),
                    Block.FromRaw(706),
                    Block.FromRaw(709),
                    Block.FromRaw(712)
                                                                                                                                                      };
            if (axis.CaselessEq("x"))
            {
                changeX = dir;
                changeZ = 0;
                dontpush[0] = Block.FromRaw(391);
                dontpush[1] = Block.FromRaw(397);
                dontpush[2] = Block.FromRaw(403);
                dontpush[3] = Block.FromRaw(409);
            }
            if (axis.CaselessEq("z"))
            {
                changeZ = dir;
                changeX = 0;
                dontpush[0] = Block.FromRaw(394);
                dontpush[1] = Block.FromRaw(400);
                dontpush[2] = Block.FromRaw(406);
                dontpush[3] = Block.FromRaw(412);
            }
            if (type.CaselessEq("off"))
            {
                NASBlockEntity[] b = new NASBlockEntity[6];
                if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                    b[0] = nl.blockEntities[x + " " + (y + 1) + " " + z];
                if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                    b[1] = nl.blockEntities[x + " " + (y - 1) + " " + z];
                if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z) && (changeX != 1))
                    b[2] = nl.blockEntities[x + 1 + " " + y + " " + z];
                if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z) && (changeX != -1))
                    b[3] = nl.blockEntities[x - 1 + " " + y + " " + z];
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)) && (changeZ != 1))
                    b[4] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)) && (changeZ != -1))
                    b[5] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                if (
                        ((b[0] != null) && b[0].strength > 0 && (b[0].type == 1 || b[0].type == 4 || b[0].type == 5 || b[0].type == 12)) ||
                        ((b[1] != null) && b[1].strength > 0 && (b[1].type == 1 || b[1].type == 4 || b[1].type == 6 || b[1].type == 12)) ||
                        ((b[2] != null) && b[2].strength > 0 && (b[2].type == 0 || b[2].type == 4 || b[2].type == 10 || b[2].type == 11)) ||
                        ((b[3] != null) && b[3].strength > 0 && (b[3].type == 0 || b[3].type == 4 || b[3].type == 8 || b[3].type == 11)) ||
                        ((b[4] != null) && b[4].strength > 0 && (b[4].type == 2 || b[4].type == 4 || b[4].type == 7 || b[4].type == 13)) ||
                        ((b[5] != null) && b[5].strength > 0 && (b[5].type == 2 || b[5].type == 4 || b[5].type == 9 || b[5].type == 13))
                    )
                {
                    ushort[] above =
                    {
                            TurnValid(nl.GetBlock(x+changeX, y+changeY, z+changeZ)),
                            0, 0, 0, 0, 0, 0
                                                                                                                                                              };
                    int push = 0;
                    switch (above[0])
                    {
                        case 0:
                        case 8:
                            push = 0;
                            break;
                        default:
                            if (IsPartOfSet(unpushable, above[0]) != -1 || IsPartOfSet(dontpush, above[0]) != -1)
                                return;
                            above[1] = TurnValid(nl.GetBlock(x + 2 * changeX, y + 2 * changeY, z + 2 * changeZ));
                            if (above[1] == 0 || above[1] == 8)
                                push = 1;
                            else
                            {
                                if (IsPartOfSet(unpushable, above[1]) != -1 || IsPartOfSet(dontpush, above[1]) != -1)
                                    return;
                                above[2] = TurnValid(nl.GetBlock(x + 3 * changeX, y + 3 * changeY, z + 3 * changeZ));
                                switch (above[2])
                                {
                                    case 0:
                                    case 8:
                                        push = 2;
                                        break;
                                    default:
                                        if (IsPartOfSet(unpushable, above[2]) != -1 || IsPartOfSet(dontpush, above[2]) != -1)
                                            return;
                                        above[3] = TurnValid(nl.GetBlock(x + 4 * changeX, y + 4 * changeY, z + 4 * changeZ));
                                        if (above[3] == 0 || above[3] == 8)
                                            push = 3;
                                        else
                                        {
                                            if (IsPartOfSet(unpushable, above[3]) != -1 || IsPartOfSet(dontpush, above[3]) != -1)
                                                return;
                                            above[4] = TurnValid(nl.GetBlock(x + 5 * changeX, y + 5 * changeY, z + 5 * changeZ));
                                            switch (above[4])
                                            {
                                                case 0:
                                                case 8:
                                                    push = 4;
                                                    break;
                                                default:
                                                    if (IsPartOfSet(unpushable, above[4]) != -1 || IsPartOfSet(dontpush, above[4]) != -1)
                                                        return;
                                                    above[5] = TurnValid(nl.GetBlock(x + 6 * changeX, y + 6 * changeY, z + 6 * changeZ));
                                                    if (above[5] == 0 || above[5] == 8)
                                                        push = 5;
                                                    else
                                                    {
                                                        if (IsPartOfSet(unpushable, above[5]) != -1 || IsPartOfSet(dontpush, above[5]) != -1)
                                                            return;
                                                        above[6] = TurnValid(nl.GetBlock(x + 7 * changeX, y + 7 * changeY, z + 7 * changeZ));
                                                        switch (above[6])
                                                        {
                                                            case 0:
                                                            case 8:
                                                                push = 6;
                                                                break;
                                                            default:
                                                                return;
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                    if (push >= 6)
                    {
                        nl.SetBlock(x + (7 * changeX), y + (7 * changeY), z + (7 * changeZ), above[5]);
                        if (nl.blockEntities.ContainsKey(x + (6 * changeX) + " " + (y + (6 * changeY)) + " " + (z + (6 * changeZ))))
                            nl.blockEntities.Remove(x + (6 * changeX) + " " + (y + (6 * changeY)) + " " + (z + (6 * changeZ)));
                    }
                    if (push >= 5)
                    {
                        nl.SetBlock(x + (6 * changeX), y + (6 * changeY), z + (6 * changeZ), above[4]);
                        if (nl.blockEntities.ContainsKey(x + (5 * changeX) + " " + (y + (5 * changeY)) + " " + (z + (5 * changeZ))))
                            nl.blockEntities.Remove(x + (5 * changeX) + " " + (y + (5 * changeY)) + " " + (z + (5 * changeZ)));
                    }
                    if (push >= 4)
                    {
                        nl.SetBlock(x + (5 * changeX), y + (5 * changeY), z + (5 * changeZ), above[3]);
                        if (nl.blockEntities.ContainsKey(x + (4 * changeX) + " " + (y + (4 * changeY)) + " " + (z + (4 * changeZ))))
                            nl.blockEntities.Remove(x + (4 * changeX) + " " + (y + (4 * changeY)) + " " + (z + (4 * changeZ)));
                    }
                    if (push >= 3)
                    {
                        nl.SetBlock(x + (4 * changeX), y + (4 * changeY), z + (4 * changeZ), above[2]);
                        if (nl.blockEntities.ContainsKey(x + (3 * changeX) + " " + (y + (3 * changeY)) + " " + (z + (3 * changeZ))))
                            nl.blockEntities.Remove(x + (3 * changeX) + " " + (y + (3 * changeY)) + " " + (z + (3 * changeZ)));
                    }
                    if (push >= 2)
                    {
                        nl.SetBlock(x + (3 * changeX), y + (3 * changeY), z + (3 * changeZ), above[1]);
                        if (nl.blockEntities.ContainsKey(x + (2 * changeX) + " " + (y + (2 * changeY)) + " " + (z + (2 * changeZ))))
                            nl.blockEntities.Remove(x + (2 * changeX) + " " + (y + (2 * changeY)) + " " + (z + (2 * changeZ)));
                    }
                    if (push >= 1)
                    {
                        nl.SetBlock(x + (2 * changeX), y + (2 * changeY), z + (2 * changeZ), above[0]);
                        if (nl.blockEntities.ContainsKey(x + changeX + " " + (y + changeY) + " " + (z + changeZ)))
                            nl.blockEntities.Remove(x + changeX + " " + (y + changeY) + " " + (z + changeZ));
                    }
                    nl.SetBlock(x + changeX, y + changeY, z + changeZ, pistonSet[2]);
                    nl.SetBlock(x, y, z, pistonSet[1]);
                    Player[] players = PlayerInfo.Online.Items;
                    for (int i = 0; i < players.Length; i++)
                    {
                        Player who = players[i];
                        Vec3F32 posH = who.Pos.BlockCoords;
                        if ((who.Pos.FeetBlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ) || who.Pos.BlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ)) && Get(NASCollision.ConvertToClientushort(nl.GetBlock((int)posH.X + changeX, (int)posH.Y + changeY, (int)posH.Z + changeZ))).collideAction != DefaultSolidCollideAction())
                        {
                            Position posit = who.Pos;
                            posit.X += changeX * 32;
                            posit.Y += changeY * 32;
                            posit.Z += changeZ * 32;
                            who.SendPosition(posit, new(who.Rot.RotY, who.Rot.HeadX));
                        }
                    }
                }
            }
            if (type.CaselessEq("body"))
            {
                NASBlockEntity[] b = new NASBlockEntity[6];
                if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                    b[0] = nl.blockEntities[x + " " + (y + 1) + " " + z];
                if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                    b[1] = nl.blockEntities[x + " " + (y - 1) + " " + z];
                if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z) && (changeX != 1))
                    b[2] = nl.blockEntities[x + 1 + " " + y + " " + z];
                if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z) && (changeX != -1))
                    b[3] = nl.blockEntities[x - 1 + " " + y + " " + z];
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)) && (changeZ != 1))
                    b[4] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)) && (changeZ != -1))
                    b[5] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                if (!(
                        ((b[0] != null) && b[0].strength > 0 && (b[0].type == 1 || b[0].type == 4 || b[0].type == 5 || b[0].type == 12)) ||
                        ((b[1] != null) && b[1].strength > 0 && (b[1].type == 1 || b[1].type == 4 || b[1].type == 6 || b[1].type == 12)) ||
                        ((b[2] != null) && b[2].strength > 0 && (b[2].type == 0 || b[2].type == 4 || b[2].type == 10 || b[2].type == 11)) ||
                        ((b[3] != null) && b[3].strength > 0 && (b[3].type == 0 || b[3].type == 4 || b[3].type == 8 || b[3].type == 11)) ||
                        ((b[4] != null) && b[4].strength > 0 && (b[4].type == 2 || b[4].type == 4 || b[4].type == 7 || b[4].type == 13)) ||
                        ((b[5] != null) && b[5].strength > 0 && (b[5].type == 2 || b[5].type == 4 || b[5].type == 9 || b[5].type == 13))
                    ))
                {
                    if (!sticky)
                    {
                        nl.SetBlock(x, y, z, pistonSet[0]);
                        nl.SetBlock(x + (1 * changeX), y + (1 * changeY), z + (1 * changeZ), 0);
                        return;
                    }
                    else
                    {
                        nl.SetBlock(x, y, z, pistonSet[0]);
                        ushort pullback = TurnValid(nl.GetBlock(x + (2 * changeX), y + (2 * changeY), z + (2 * changeZ)));
                        if (IsPartOfSet(unpushable, pullback) == -1 && IsPartOfSet(dontpush, pullback) == -1 && pullback != 8)
                        {
                            nl.SetBlock(x + (1 * changeX), y + (1 * changeY), z + (1 * changeZ), pullback);
                            nl.SetBlock(x + (2 * changeX), y + (2 * changeY), z + (2 * changeZ), 0);
                            if (nl.blockEntities.ContainsKey(x + (2 * changeX) + " " + (y + (2 * changeY)) + " " + (z + (2 * changeZ))))
                                nl.blockEntities.Remove(x + (2 * changeX) + " " + (y + (2 * changeY)) + " " + (z + (2 * changeZ)));
                            return;
                        }
                        return;
                    }
                }
                if (nl.GetBlock(x + changeX, y + changeY, z + changeZ) != pistonSet[2])
                    nl.SetBlock(x + changeX, y + changeY, z + changeZ, pistonSet[2]);
            }
            if (type.CaselessEq("head") && nl.GetBlock(x - changeX, y - changeY, z - changeZ) != pistonSet[1])
                nl.SetBlock(x, y, z, 0);
        };
        public static NASBlockAction PistonAction(string type, int changeX, int changeY, int changeZ, ushort[] pistonSet) => (nl, nasBlock, x, y, z) =>
        {
            ushort[] dontpush =
            {
                    Block.FromRaw(391),
                    Block.FromRaw(397),
                    Block.FromRaw(403),
                    Block.FromRaw(409),
                    Block.FromRaw(394),
                    Block.FromRaw(400),
                    Block.FromRaw(406),
                    Block.FromRaw(412),
                                                                                                                                          };
            if (type.CaselessEq("off"))
            {
                NASBlockEntity[] b = new NASBlockEntity[5];
                if (nl.blockEntities.ContainsKey(x + " " + (y - changeY) + " " + z))
                    b[0] = nl.blockEntities[x + " " + (y - changeY) + " " + z];
                if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                    b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
                if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                    b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                    b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                    b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                if (
                        ((b[0] != null) && b[0].strength > 0 && (b[0].type == 1 || b[0].type == 4 || b[0].type == 12 || (changeY == 1 && b[0].type == 6) || (changeY == -1 && b[0].type == 5))) ||
                        ((b[1] != null) && b[1].strength > 0 && (b[1].type == 0 || b[1].type == 4 || b[1].type == 10 || b[1].type == 11)) ||
                        ((b[2] != null) && b[2].strength > 0 && (b[2].type == 0 || b[2].type == 4 || b[2].type == 8 || b[2].type == 11)) ||
                        ((b[3] != null) && b[3].strength > 0 && (b[3].type == 2 || b[3].type == 4 || b[3].type == 7 || b[3].type == 13)) ||
                        ((b[4] != null) && b[4].strength > 0 && (b[4].type == 2 || b[4].type == 4 || b[4].type == 9 || b[4].type == 13))
                    )
                {
                    ushort[] above =
                    {
                            TurnValid(nl.GetBlock(x+changeX, y+changeY, z+changeZ)),
                            0, 0, 0, 0, 0, 0
                                                                                                                                                  };
                    int push = 0;
                    switch (above[0])
                    {
                        case 0:
                        case 8:
                            push = 0;
                            break;
                        default:
                            if (IsPartOfSet(unpushable, above[0]) != -1 || IsPartOfSet(dontpush, above[0]) != -1)
                                return;
                            above[1] = TurnValid(nl.GetBlock(x + 2 * changeX, y + 2 * changeY, z + 2 * changeZ));
                            if (above[1] == 0 || above[1] == 8)
                                push = 1;
                            else
                            {
                                if (IsPartOfSet(unpushable, above[1]) != -1 || IsPartOfSet(dontpush, above[1]) != -1)
                                    return;
                                above[2] = TurnValid(nl.GetBlock(x + 3 * changeX, y + 3 * changeY, z + 3 * changeZ));
                                switch (above[2])
                                {
                                    case 0:
                                    case 8:
                                        push = 2;
                                        break;
                                    default:
                                        if (IsPartOfSet(unpushable, above[2]) != -1 || IsPartOfSet(dontpush, above[2]) != -1)
                                            return;
                                        above[3] = TurnValid(nl.GetBlock(x + 4 * changeX, y + 4 * changeY, z + 4 * changeZ));
                                        if (above[3] == 0 || above[3] == 8)
                                            push = 3;
                                        else
                                        {
                                            if (IsPartOfSet(unpushable, above[3]) != -1 || IsPartOfSet(dontpush, above[3]) != -1)
                                                return;
                                            above[4] = TurnValid(nl.GetBlock(x + 5 * changeX, y + 5 * changeY, z + 5 * changeZ));
                                            switch (above[4])
                                            {
                                                case 0:
                                                case 8:
                                                    push = 4;
                                                    break;
                                                default:
                                                    if (IsPartOfSet(unpushable, above[4]) != -1 || IsPartOfSet(dontpush, above[4]) != -1)
                                                        return;
                                                    above[5] = TurnValid(nl.GetBlock(x + 6 * changeX, y + 6 * changeY, z + 6 * changeZ));
                                                    if (above[5] == 0 || above[5] == 8)
                                                        push = 5;
                                                    else
                                                    {
                                                        if (IsPartOfSet(unpushable, above[5]) != -1 || IsPartOfSet(dontpush, above[5]) != -1)
                                                            return;
                                                        above[6] = TurnValid(nl.GetBlock(x + 7 * changeX, y + 7 * changeY, z + 7 * changeZ));
                                                        switch (above[6])
                                                        {
                                                            case 0:
                                                            case 8:
                                                                push = 6;
                                                                break;
                                                            default:
                                                                return;
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                    if (push >= 6)
                    {
                        nl.SetBlock(x, y + (7 * changeY), z, above[5]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (6 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (6 * changeY)) + " " + z);
                    }
                    if (push >= 5)
                    {
                        nl.SetBlock(x, y + (6 * changeY), z, above[4]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (5 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (5 * changeY)) + " " + z);
                    }
                    if (push >= 4)
                    {
                        nl.SetBlock(x, y + (5 * changeY), z, above[3]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (4 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (4 * changeY)) + " " + z);
                    }
                    if (push >= 3)
                    {
                        nl.SetBlock(x, y + (4 * changeY), z, above[2]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (3 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (3 * changeY)) + " " + z);
                    }
                    if (push >= 2)
                    {
                        nl.SetBlock(x, y + (3 * changeY), z, above[1]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (2 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (2 * changeY)) + " " + z);
                    }
                    if (push >= 1)
                    {
                        nl.SetBlock(x, y + (2 * changeY), z, above[0]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (1 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (1 * changeY)) + " " + z);
                    }
                    nl.SetBlock(x, y + changeY, z, pistonSet[2]);
                    nl.SetBlock(x, y, z, pistonSet[1]);
                    Player[] players = PlayerInfo.Online.Items;
                    for (int i = 0; i < players.Length; i++)
                    {
                        Player who = players[i];
                        Vec3F32 posH = who.Pos.BlockCoords;
                        if ((who.Pos.FeetBlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ) || who.Pos.BlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ)) && Get(NASCollision.ConvertToClientushort(nl.GetBlock((int)posH.X + changeX, (int)posH.Y + changeY, (int)posH.Z + changeZ))).collideAction != DefaultSolidCollideAction())
                        {
                            Position posit = who.Pos;
                            posit.X += changeX * 32;
                            posit.Y += changeY * 32;
                            posit.Z += changeZ * 32;
                            who.SendPosition(posit, new(who.Rot.RotY, who.Rot.HeadX));
                        }
                    }
                }
            }
            if (type.CaselessEq("body"))
            {
                NASBlockEntity[] b = new NASBlockEntity[5];
                if (nl.blockEntities.ContainsKey(x + " " + (y - changeY) + " " + z))
                    b[0] = nl.blockEntities[x + " " + (y - changeY) + " " + z];
                if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                    b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
                if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                    b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                    b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                    b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                if (!(
                        ((b[0] != null) && b[0].strength > 0 && (b[0].type == 1 || b[0].type == 4 || b[0].type == 12 || (changeY == 1 && b[0].type == 6) || (changeY == -1 && b[0].type == 5))) ||
                        ((b[1] != null) && b[1].strength > 0 && (b[1].type == 0 || b[1].type == 4 || b[1].type == 10 || b[1].type == 11)) ||
                        ((b[2] != null) && b[2].strength > 0 && (b[2].type == 0 || b[2].type == 4 || b[2].type == 8 || b[2].type == 11)) ||
                        ((b[3] != null) && b[3].strength > 0 && (b[3].type == 2 || b[3].type == 4 || b[3].type == 7 || b[3].type == 13)) ||
                        ((b[4] != null) && b[4].strength > 0 && (b[4].type == 2 || b[4].type == 4 || b[4].type == 9 || b[4].type == 13))
                    ))
                {
                    nl.SetBlock(x, y, z, pistonSet[0]);
                    return;
                }
                if (nl.GetBlock(x, y + changeY, z) != pistonSet[2])
                    nl.SetBlock(x, y + changeY, z, pistonSet[2]);
            }
            if (type.CaselessEq("head") && nl.GetBlock(x, y - changeY, z) != pistonSet[1])
                nl.SetBlock(x, y, z, 0);
        };
        public static NASBlockAction StickyPistonAction(string type, int changeX, int changeY, int changeZ, ushort[] pistonSet) => (nl, nasBlock, x, y, z) =>
        {
            ushort[] dontpush =
            {
                    Block.FromRaw(391),
                    Block.FromRaw(397),
                    Block.FromRaw(403),
                    Block.FromRaw(409),
                    Block.FromRaw(394),
                    Block.FromRaw(400),
                    Block.FromRaw(406),
                    Block.FromRaw(412),
                                                                                                                                                };
            NASBlockEntity[] b = new NASBlockEntity[5];
            if (nl.blockEntities.ContainsKey(x + " " + (y - changeY) + " " + z))
                b[0] = nl.blockEntities[x + " " + (y - changeY) + " " + z];
            if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
            if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
            if (type.CaselessEq("off"))
            {
                if (
                        ((b[0] != null) && b[0].strength > 0 && (b[0].type == 1 || b[0].type == 4 || b[0].type == 12 || (changeY == 1 && b[0].type == 6) || (changeY == -1 && b[0].type == 5))) ||
                        ((b[1] != null) && b[1].strength > 0 && (b[1].type == 0 || b[1].type == 4 || b[1].type == 10 || b[1].type == 11)) ||
                        ((b[2] != null) && b[2].strength > 0 && (b[2].type == 0 || b[2].type == 4 || b[2].type == 8 || b[2].type == 11)) ||
                        ((b[3] != null) && b[3].strength > 0 && (b[3].type == 2 || b[3].type == 4 || b[3].type == 7 || b[3].type == 13)) ||
                        ((b[4] != null) && b[4].strength > 0 && (b[4].type == 2 || b[4].type == 4 || b[4].type == 9 || b[4].type == 13))
                    )
                {
                    ushort[] above =
                    {
                            TurnValid(nl.GetBlock(x+changeX, y+changeY, z+changeZ)),
                            0, 0, 0, 0, 0, 0
                                                                                                                                                        };
                    int push = 0;
                    switch (above[0])
                    {
                        case 0:
                        case 8:
                            push = 0;
                            break;
                        default:
                            if (IsPartOfSet(unpushable, above[0]) != -1 || IsPartOfSet(dontpush, above[0]) != -1)
                                return;
                            above[1] = TurnValid(nl.GetBlock(x + 2 * changeX, y + 2 * changeY, z + 2 * changeZ));
                            if (above[1] == 0 || above[1] == 8)
                                push = 1;
                            else
                            {
                                if (IsPartOfSet(unpushable, above[1]) != -1 || IsPartOfSet(dontpush, above[1]) != -1)
                                    return;
                                above[2] = TurnValid(nl.GetBlock(x + 3 * changeX, y + 3 * changeY, z + 3 * changeZ));
                                switch (above[2])
                                {
                                    case 0:
                                    case 8:
                                        push = 2;
                                        break;
                                    default:
                                        if (IsPartOfSet(unpushable, above[2]) != -1 || IsPartOfSet(dontpush, above[2]) != -1)
                                            return;
                                        above[3] = TurnValid(nl.GetBlock(x + 4 * changeX, y + 4 * changeY, z + 4 * changeZ));
                                        if (above[3] == 0 || above[3] == 8)
                                            push = 3;
                                        else
                                        {
                                            if (IsPartOfSet(unpushable, above[3]) != -1 || IsPartOfSet(dontpush, above[3]) != -1)
                                                return;
                                            above[4] = TurnValid(nl.GetBlock(x + 5 * changeX, y + 5 * changeY, z + 5 * changeZ));
                                            switch (above[4])
                                            {
                                                case 0:
                                                case 8:
                                                    push = 4;
                                                    break;
                                                default:
                                                    if (IsPartOfSet(unpushable, above[4]) != -1 || IsPartOfSet(dontpush, above[4]) != -1)
                                                        return;
                                                    above[5] = TurnValid(nl.GetBlock(x + 6 * changeX, y + 6 * changeY, z + 6 * changeZ));
                                                    if (above[5] == 0 || above[5] == 8)
                                                        push = 5;
                                                    else
                                                    {
                                                        if (IsPartOfSet(unpushable, above[5]) != -1 || IsPartOfSet(dontpush, above[5]) != -1)
                                                            return;
                                                        above[6] = TurnValid(nl.GetBlock(x + 7 * changeX, y + 7 * changeY, z + 7 * changeZ));
                                                        switch (above[6])
                                                        {
                                                            case 0:
                                                            case 8:
                                                                push = 6;
                                                                break;
                                                            default:
                                                                return;
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                    if (push >= 6)
                    {
                        nl.SetBlock(x, y + (7 * changeY), z, above[5]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (6 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (6 * changeY)) + " " + z);
                    }
                    if (push >= 5)
                    {
                        nl.SetBlock(x, y + (6 * changeY), z, above[4]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (5 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (5 * changeY)) + " " + z);
                    }
                    if (push >= 4)
                    {
                        nl.SetBlock(x, y + (5 * changeY), z, above[3]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (4 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (4 * changeY)) + " " + z);
                    }
                    if (push >= 3)
                    {
                        nl.SetBlock(x, y + (4 * changeY), z, above[2]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (3 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (3 * changeY)) + " " + z);
                    }
                    if (push >= 2)
                    {
                        nl.SetBlock(x, y + (3 * changeY), z, above[1]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (2 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (2 * changeY)) + " " + z);
                    }
                    if (push >= 1)
                    {
                        nl.SetBlock(x, y + (2 * changeY), z, above[0]);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (1 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (1 * changeY)) + " " + z);
                    }
                    nl.SetBlock(x, y + changeY, z, pistonSet[2]);
                    nl.SetBlock(x, y, z, pistonSet[1]);
                    Player[] players = PlayerInfo.Online.Items;
                    for (int i = 0; i < players.Length; i++)
                    {
                        Player who = players[i];
                        Vec3F32 posH = who.Pos.BlockCoords;
                        if ((who.Pos.FeetBlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ) || who.Pos.BlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ)) && Get(NASCollision.ConvertToClientushort(nl.GetBlock((int)posH.X + changeX, (int)posH.Y + changeY, (int)posH.Z + changeZ))).collideAction != DefaultSolidCollideAction())
                        {
                            Position posit = who.Pos;
                            posit.X += changeX * 32;
                            posit.Y += changeY * 32;
                            posit.Z += changeZ * 32;
                            who.SendPosition(posit, new(who.Rot.RotY, who.Rot.HeadX));
                        }
                    }
                }
            }
            if (type.CaselessEq("body"))
            {
                if (!(
                        ((b[0] != null) && b[0].strength > 0 && (b[0].type == 1 || b[0].type == 4 || b[0].type == 12 || (changeY == 1 && b[0].type == 6) || (changeY == -1 && b[0].type == 5))) ||
                        ((b[1] != null) && b[1].strength > 0 && (b[1].type == 0 || b[1].type == 4 || b[1].type == 10 || b[1].type == 11)) ||
                        ((b[2] != null) && b[2].strength > 0 && (b[2].type == 0 || b[2].type == 4 || b[2].type == 8 || b[2].type == 11)) ||
                        ((b[3] != null) && b[3].strength > 0 && (b[3].type == 2 || b[3].type == 4 || b[3].type == 7 || b[3].type == 13)) ||
                        ((b[4] != null) && b[4].strength > 0 && (b[4].type == 2 || b[4].type == 4 || b[4].type == 9 || b[4].type == 13))
                    ))
                {
                    nl.SetBlock(x, y, z, pistonSet[0]);
                    ushort pullback = TurnValid(nl.GetBlock(x, y + (2 * changeY), z));
                    if (IsPartOfSet(unpushable, pullback) == -1 && IsPartOfSet(dontpush, pullback) == -1 && pullback != 8)
                    {
                        nl.SetBlock(x, y + (1 * changeY), z, pullback);
                        nl.SetBlock(x, y + (2 * changeY), z, 0);
                        if (nl.blockEntities.ContainsKey(x + " " + (y + (2 * changeY)) + " " + z))
                            nl.blockEntities.Remove(x + " " + (y + (2 * changeY)) + " " + z);
                    }
                    return;
                }
                if (nl.GetBlock(x, y + changeY, z) != pistonSet[2])
                    nl.SetBlock(x, y + changeY, z, pistonSet[2]);
            }
            if (type.CaselessEq("head") && nl.GetBlock(x, y - changeY, z) != pistonSet[1])
                nl.SetBlock(x, y, z, 0);
        };
        public static bool ConvertBody(ushort block, ushort[] set, out ushort returnedBlock)
        {
            returnedBlock = block;
            if (block == set[1])
            {
                returnedBlock = set[0];
                return true;
            }
            return false;
        }
        public static ushort TurnValid(ushort block) => ConvertBody(block, pistonUp, out ushort returnedBlock)
                ? returnedBlock
                : ConvertBody(block, pistonDown, out returnedBlock)
                ? returnedBlock
                : ConvertBody(block, pistonNorth, out returnedBlock)
                ? returnedBlock
                : ConvertBody(block, pistonEast, out returnedBlock)
                ? returnedBlock
                : ConvertBody(block, pistonSouth, out returnedBlock)
                ? returnedBlock
                : ConvertBody(block, pistonWest, out returnedBlock)
                ? returnedBlock
                : ConvertBody(block, stickyPistonUp, out returnedBlock)
                ? returnedBlock
                : ConvertBody(block, stickyPistonDown, out returnedBlock)
                ? returnedBlock
                : ConvertBody(block, stickyPistonNorth, out returnedBlock)
                ? returnedBlock
                : ConvertBody(block, stickyPistonEast, out returnedBlock)
                ? returnedBlock
                : ConvertBody(block, stickyPistonSouth, out returnedBlock)
                ? returnedBlock
                : ConvertBody(block, stickyPistonWest, out returnedBlock) ? returnedBlock : returnedBlock;
        public static NASBlockAction PowerSourceAction(int direction) => (nl, nasBlock, x, y, z) =>
        {
            if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
            {
                nl.blockEntities.Add(x + " " + y + " " + z, new());
                nl.blockEntities[x + " " + y + " " + z].strength = 15;
                nl.blockEntities[x + " " + y + " " + z].type = direction;
                nl.SimulateSetBlock(x, y, z);
            }
        };
        public static NASBlockAction WireAction(ushort[] actSet, ushort[] inactSet, int direction, ushort hereBlock) => (nl, nasBlock, x, y, z) =>
        {
            int type = IsPartOfSet(actSet, hereBlock) != -1 ? IsPartOfSet(actSet, hereBlock) : IsPartOfSet(inactSet, hereBlock);
            if (actSet == fixedWireSetActive)
                type += 11;
            if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
            {
                nl.blockEntities.Add(x + " " + y + " " + z, new());
                nl.blockEntities[x + " " + y + " " + z].strength = 0;
                nl.blockEntities[x + " " + y + " " + z].type = type;
            }
            NASBlockEntity b = nl.blockEntities[x + " " + y + " " + z],
            strength1 = new(),
            strength2 = new();
            int strength0 = b.strength;
            if (direction == 0)
            {
                if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                {
                    NASBlockEntity bEntity = nl.blockEntities[x + 1 + " " + y + " " + z];
                    int checkType = bEntity.type;
                    switch (checkType)
                    {
                        case < 5:
                        case 10:
                        case 11:
                            strength1 = bEntity;
                            break;
                    }
                }
                if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                {
                    NASBlockEntity bEntity = nl.blockEntities[x - 1 + " " + y + " " + z];
                    int checkType = bEntity.type;
                    switch (checkType)
                    {
                        case < 5:
                        case 8:
                        case 11:
                            strength2 = bEntity;
                            break;
                    }
                }
            }
            if (direction == 1)
            {
                if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                {
                    NASBlockEntity bEntity = nl.blockEntities[x + " " + (y + 1) + " " + z];
                    int checkType = bEntity.type;
                    switch (checkType)
                    {
                        case <= 5:
                        case 12:
                            strength1 = bEntity;
                            break;
                    }
                }
                if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                {
                    NASBlockEntity bEntity = nl.blockEntities[x + " " + (y - 1) + " " + z];
                    int checkType = bEntity.type;
                    switch (checkType)
                    {
                        case < 5:
                        case 6:
                        case 12:
                            strength2 = bEntity;
                            break;
                    }
                }
            }
            if (direction == 2)
            {
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                {
                    NASBlockEntity bEntity = nl.blockEntities[x + " " + y + " " + (z + 1)];
                    int checkType = bEntity.type;
                    switch (checkType)
                    {
                        case < 5:
                        case 7:
                        case 13:
                            strength1 = bEntity;
                            break;
                    }
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                {
                    NASBlockEntity bEntity = nl.blockEntities[x + " " + y + " " + (z - 1)];
                    int checkType = bEntity.type;
                    switch (checkType)
                    {
                        case < 5:
                        case 9:
                        case 13:
                            strength2 = bEntity;
                            break;
                    }
                }
            }
            b.strength = strength1.strength >= strength2.strength ? strength1.strength - 1 : strength2.strength - 1;
            switch (b.strength)
            {
                case <= 0:
                    b.strength = 0;
                    b.direction = 0;
                    if (IsPartOfSet(actSet, hereBlock) != -1)
                        nl.FastSetBlock(x, y, z, inactSet[IsPartOfSet(actSet, hereBlock)]);
                    else
                    {
                        if (strength0 != b.strength)
                            nl.DisturbBlocks(x, y, z);
                    }
                    break;
                default:
                    if (IsPartOfSet(actSet, hereBlock) == -1)
                        nl.FastSetBlock(x, y, z, actSet[IsPartOfSet(inactSet, hereBlock)]);
                    else
                    {
                        if (strength0 != b.strength)
                            nl.DisturbBlocks(x, y, z);
                    }
                    break;
            }
        };
        public static NASBlockAction PressurePlateAction() => (nl, nasBlock, x, y, z) =>
        {
            bool stoodOn = false;
            Player[] players = PlayerInfo.Online.Items;
            for (int i = 0; i < players.Length; i++)
            {
                Player who = players[i];
                if ((who.Pos.FeetBlockCoords == new Vec3S32(x, y, z) || who.Pos.FeetBlockCoords == new Vec3S32(x, y + 1, z)) && who.Level == nl.lvl)
                    stoodOn = true;
            }
            if (!stoodOn)
            {
                nl.SetBlock(x, y, z, Block.FromRaw(610));
                nl.blockEntities[x + " " + y + " " + z].strength = 0;
            }
            else
                nl.SimulateSetBlock(x, y, z);
        };
        public static NASBlockAction RepeaterAction(int direction, ushort hereBlock) => (nl, nasBlock, x, y, z) =>
        {
            int type = IsPartOfSet(repeaterSetActive, hereBlock) != -1 ? 1 : 0;
            if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
            {
                nl.blockEntities.Add(x + " " + y + " " + z, new());
                nl.blockEntities[x + " " + y + " " + z].strength = 0;
                nl.blockEntities[x + " " + y + " " + z].type = direction;
            }
            NASBlockEntity b = nl.blockEntities[x + " " + y + " " + z],
            strength1 = new();
            if (direction == 5 && nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                strength1 = nl.blockEntities[x + " " + (y + 1) + " " + z];
            if (direction == 6 && nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                strength1 = nl.blockEntities[x + " " + (y - 1) + " " + z];
            if (direction == 7 && nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                strength1 = nl.blockEntities[x + " " + y + " " + (z + 1)];
            if (direction == 8 && nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                strength1 = nl.blockEntities[x - 1 + " " + y + " " + z];
            if (direction == 9 && nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                strength1 = nl.blockEntities[x + " " + y + " " + (z - 1)];
            if (direction == 10 && nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                strength1 = nl.blockEntities[x + 1 + " " + y + " " + z];
            NASQueuedBlockUpdate qb = new()
            {
                x = x,
                y = y,
                z = z
            };
            float seconds = 0.4f;
            qb.date = DateTime.UtcNow + TimeSpan.FromSeconds(seconds);
            qb.date = qb.date.Floor(TimeSpan.FromMilliseconds(100));
            qb.nb = nasBlock;
            qb.da = ContRepeaterTask(type, strength1, direction);
            nl.tickQueue.Enqueue(qb, qb.date);
        };
        public static NASBlockAction ContRepeaterTask(int type, NASBlockEntity strength1, int direction) => (nl, nasBlock, x, y, z) =>
        {
            NASBlockEntity b = nl.blockEntities[x + " " + y + " " + z];
            if (!(strength1.type < 5 || strength1.type == b.type || (strength1.type == 11 && (b.type == 10 || b.type == 8)) ||
                  (strength1.type == 12 && (b.type == 5 || b.type == 6)) ||
                  (strength1.type == 13 && (b.type == 9 || b.type == 7))))
            {
                strength1.strength = 0;
            }
            if (type == 0 && strength1.strength > 0)
            {
                nl.SetBlock(x, y, z, repeaterSetActive[direction - 5]);
                b.strength = 15;
            }
            if (type == 1 && strength1.strength == 0)
            {
                nl.SetBlock(x, y, z, repeaterSetInactive[direction - 5]);
                b.strength = 0;
            }
        };
        public static NASBlockAction TurnOffAction() => (nl, nasBlock, x, y, z) =>
        {
            if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                return;
            nl.SetBlock(x, y, z, Block.FromRaw(195));
            nl.blockEntities[x + " " + y + " " + z].strength = 0;
        };
        public static NASBlockAction DispenserAction(int changeX, int changeY, int changeZ) => (nl, nasBlock, x, y, z) =>
        {
            NASBlockEntity[] b = new NASBlockEntity[6];
            if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z) && (changeY != 1))
                b[0] = nl.blockEntities[x + " " + (y + 1) + " " + z];
            if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z) && (changeY != -1))
                b[1] = nl.blockEntities[x + " " + (y - 1) + " " + z];
            if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z) && (changeX != 1))
                b[2] = nl.blockEntities[x + 1 + " " + y + " " + z];
            if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z) && (changeX != -1))
                b[3] = nl.blockEntities[x - 1 + " " + y + " " + z];
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)) && (changeZ != 1))
                b[4] = nl.blockEntities[x + " " + y + " " + (z + 1)];
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)) && (changeZ != -1))
                b[5] = nl.blockEntities[x + " " + y + " " + (z - 1)];
            bool powered =
                ((b[0] != null) && b[0].strength > 0 && (b[0].type == 1 || b[0].type == 4 || b[0].type == 5 || b[0].type == 12)) ||
                ((b[1] != null) && b[1].strength > 0 && (b[1].type == 1 || b[1].type == 4 || b[1].type == 6 || b[1].type == 12)) ||
                ((b[2] != null) && b[2].strength > 0 && (b[2].type == 0 || b[2].type == 4 || b[2].type == 10 || b[2].type == 11)) ||
                ((b[3] != null) && b[3].strength > 0 && (b[3].type == 0 || b[3].type == 4 || b[3].type == 8 || b[3].type == 11)) ||
                ((b[4] != null) && b[4].strength > 0 && (b[4].type == 2 || b[4].type == 4 || b[4].type == 7 || b[4].type == 13)) ||
                ((b[5] != null) && b[5].strength > 0 && (b[5].type == 2 || b[5].type == 4 || b[5].type == 9 || b[5].type == 13));
            if (!powered && nl.blockEntities.ContainsKey(x + " " + y + " " + z))
            {
                nl.blockEntities[x + " " + y + " " + z].type = 0;
                return;
            }
            if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z) || nl.blockEntities[x + " " + y + " " + z].type == 1)
                return;
            nl.blockEntities[x + " " + y + " " + z].type = 1;
            {
                ushort checkBlock = nl.GetBlock(x + changeX, y + changeY, z + changeZ);
                if (!CanPhysicsKillThis(checkBlock) && IsPartOfSet(waterSet, checkBlock) == -1 && IsPartOfSet(lavaSet, checkBlock) == -1)
                    return;
                NASBlockEntity bEntity = nl.blockEntities[x + " " + y + " " + z];
                if (bEntity.drop == null || bEntity.drop.blockStacks == null)
                    return;
                NASBlockStack bs = bEntity.drop.blockStacks[bEntity.drop.blockStacks.Count - 1];
                if (bs.ID == 7)
                    return;
                ushort clientushort = bs.ID,
                addedushort = 0;
                switch (clientushort)
                {
                    case 643:
                        clientushort = 9;
                        addedushort = 143;
                        break;
                    case 696:
                        clientushort = 10;
                        addedushort = 697;
                        break;
                    case 143 when IsPartOfSet(waterSet, checkBlock) != -1:
                        clientushort = 0;
                        addedushort = 643;
                        break;
                    case 697 when checkBlock == 10:
                        clientushort = 0;
                        addedushort = 696;
                        break;
                }
                bs.amount -= 1;
                if (bs.amount == 0)
                    bEntity.drop.blockStacks.Remove(bs);
                if (bEntity.drop.blockStacks.Count == 0)
                    bEntity.drop = null;
                if (addedushort == 0)
                {
                    nl.SetBlock(x + changeX, y + changeY, z + changeZ, Block.FromRaw(clientushort));
                    if (Get(bs.ID).container != null)
                        nl.blockEntities.Add(x + changeX + " " + (y + changeY) + " " + (z + changeZ), new());
                    return;
                }
                if (bEntity.drop == null)
                {
                    nl.SetBlock(x + changeX, y + changeY, z + changeZ, Block.FromRaw(clientushort));
                    bEntity.drop = new(addedushort);
                    return;
                }
                foreach (NASBlockStack stack in bEntity.drop.blockStacks)
                {
                    if (stack.ID == addedushort)
                    {
                        if (addedushort != 0)
                            stack.amount += 1;
                        nl.SetBlock(x + changeX, y + changeY, z + changeZ, Block.FromRaw(clientushort));
                        if (Get(bs.ID).container != null)
                            nl.blockEntities.Add(x + changeX + " " + (y + changeY) + " " + (z + changeZ), new());
                        return;
                    }
                }
                if (bEntity.drop.blockStacks.Count >= NASContainer.BlockStackLimit)
                    return;
                if (addedushort != 0)
                    bEntity.drop.blockStacks.Add(new(addedushort));
                nl.SetBlock(x + changeX, y + changeY, z + changeZ, Block.FromRaw(clientushort));
                if (Get(bs.ID).container != null)
                    nl.blockEntities.Add(x + changeX + " " + (y + changeY) + " " + (z + changeZ), new());
            }
        };
        public static NASBlockAction SpongeAction() => (nl, nasBlock, x, y, z) =>
        {
            bool absorbed = false;
            for (int xOff = -3; xOff <= 3; xOff++)
                for (int yOff = -3; yOff <= 3; yOff++)
                    for (int zOff = -3; zOff <= 3; zOff++)
                        if (IsPartOfSet(waterSet, nl.GetBlock(x + xOff, y + yOff, z + zOff)) != -1)
                        {
                            nl.SetBlock(x + xOff, y + yOff, z + zOff, 0);
                            absorbed = true;
                        }
            if (absorbed)
                nl.SetBlock(x, y, z, Block.FromRaw(428));
        };
        public static NASBlockAction NeedsSupportAction() => (nl, nasBlock, x, y, z) =>
        {
            IsSupported(nl, x, y, z);
        };
        public static NASBlockAction GenericPlantAction() => (nl, nasBlock, x, y, z) =>
        {
            GenericPlantSurvived(nl, x, y, z);
        };
        public static NASBlockAction OakSaplingAction() => (nl, nasBlock, x, y, z) =>
        {
            if (!GenericPlantSurvived(nl, x, y, z))
                return;
            nl.SetBlock(x, y, z, 0);
            NASTree.GenOakTree(nl, r, x, y, z, true);
        };
        public static NASBlockAction BirchSaplingAction() => (nl, nasBlock, x, y, z) =>
        {
            if (!GenericPlantSurvived(nl, x, y, z))
                return;
            nl.SetBlock(x, y, z, 0);
            NASTree.GenBirchTree(nl, r, x, y, z, true);
        };
        public static NASBlockAction SwampSaplingAction() => (nl, nasBlock, x, y, z) =>
        {
            if (!GenericPlantSurvived(nl, x, y, z))
                return;
            nl.SetBlock(x, y, z, 0);
            NASTree.GenSwampTree(nl, r, x, y, z, true);
        };
        public static NASBlockAction SpruceSaplingAction() => (nl, nasBlock, x, y, z) =>
        {
            if (!GenericPlantSurvived(nl, x, y, z))
                return;
            nl.SetBlock(x, y, z, 0);
            NASTree.GenSpruceTree(nl, r, x, y, z, true);
        };
        public static NASBlockAction CropAction(ushort[] cropSet, int index) => (nl, nasBlock, x, y, z) =>
        {
            if (!CropSurvived(nl, x, y, z) || index + 1 >= cropSet.Length)
                return;
            nl.SetBlock(x, y, z, cropSet[index + 1]);
        };
        public static NASBlockAction IronCropAction(ushort[] cropSet, int index) => (nl, nasBlock, x, y, z) =>
        {
            if (!IronCropSurvived(nl, x, y, z) || index + 1 >= cropSet.Length)
                return;
            nl.SetBlock(x, y, z, cropSet[index + 1]);
        };
        public static NASBlockAction AutoCraftingAction() => (nl, nasBlock, x, y, z) =>
        {
            NASBlockEntity[] b = new NASBlockEntity[5];
            if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                b[0] = nl.blockEntities[x + " " + (y - 1) + " " + z];
            if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
            if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
            bool powered =
                    ((b[0] != null) && b[0].strength > 0 && (b[0].type == 1 || b[0].type == 4 || b[0].type == 6 || b[0].type == 12)) ||
                    ((b[1] != null) && b[1].strength > 0 && (b[1].type == 0 || b[1].type == 4 || b[1].type == 10 || b[1].type == 11)) ||
                    ((b[2] != null) && b[2].strength > 0 && (b[2].type == 0 || b[2].type == 4 || b[2].type == 8 || b[2].type == 11)) ||
                    ((b[3] != null) && b[3].strength > 0 && (b[3].type == 2 || b[3].type == 4 || b[3].type == 7 || b[3].type == 13)) ||
                    ((b[4] != null) && b[4].strength > 0 && (b[4].type == 2 || b[4].type == 4 || b[4].type == 9 || b[4].type == 13));
            if (!powered)
            {
                nl.blockEntities[x + " " + y + " " + z].type = 0;
                return;
            }
            if (nl.blockEntities[x + " " + y + " " + z].type == 1)
                return;
            nl.blockEntities[x + " " + y + " " + z].type = 1;
            NASRecipe recipe = NASCrafting.GetRecipe(nl, (ushort)x, (ushort)y, (ushort)z, nasBlock.station);
            if (recipe == null)
                return;
            NASDrop dropClone = new(recipe.drop);
            NASCrafting.ClearCraftingArea(nl, (ushort)x, (ushort)y, (ushort)z, nasBlock.station.ori);
            NASBlockEntity bEntity = nl.blockEntities[x + " " + y + " " + z];
            if (bEntity.drop == null)
            {
                bEntity.drop = dropClone;
                return;
            }
            if (dropClone.items != null)
                foreach (NASItem tool in bEntity.drop.items)
                    bEntity.drop.items.Add(tool);
            if (dropClone.blockStacks != null)
            {
                bool exists = false;
                foreach (NASBlockStack stack in dropClone.blockStacks)
                {
                    exists = false;
                    foreach (NASBlockStack otherStack in bEntity.drop.blockStacks)
                        if (stack.ID == otherStack.ID)
                        {
                            otherStack.amount += stack.amount;
                            exists = true;
                        }
                    if (!exists)
                        bEntity.drop.blockStacks.Add(new(stack.ID, stack.amount));
                }
            }
        };
        public static bool IsSupported(NASLevel nl, int x, int y, int z)
        {
            ushort below = nl.GetBlock(x, y - 1, z);
            if (CanPhysicsKillThis(below))
            {
                nl.SetBlock(x, y, z, 0);
                return false;
            }
            return true;
        }
        public static bool GenericPlantSurvived(NASLevel nl, int x, int y, int z)
        {
            if (!IsSupported(nl, x, y, z))
                return false;
            if (!CanPlantsLiveOn(nl.GetBlock(x, y - 1, z)))
            {
                nl.SetBlock(x, y, z, 39);
                return false;
            }
            return true;
        }
        public static bool CropSurvived(NASLevel nl, int x, int y, int z)
        {
            if (!IsSupported(nl, x, y, z) || nl.biome < 0)
                return false;
            switch (IsPartOfSet(soilForPlants, nl.GetBlock(x, y - 1, z)))
            {
                case -1:
                    nl.SetBlock(x, y, z, 39);
                    return false;
                default:
                    return true;
            }
        }
        public static bool IronCropSurvived(NASLevel nl, int x, int y, int z)
        {
            if (!IsSupported(nl, x, y, z) || nl.biome >= 0)
                return false;
            if (IsPartOfSet(soilForIron, nl.GetBlock(x, y - 1, z)) == -1 || IsPartOfSet(lavaSet, nl.GetBlock(x, y - 2, z)) == -1)
            {
                nl.SetBlock(x, y, z, 39);
                return false;
            }
            return true;
        }
        public static bool CanPlantsLiveOn(ushort block) => IsPartOfSet(soilForPlants, block) != -1 || IsPartOfSet(grassSet, block) != -1;
    }
}
