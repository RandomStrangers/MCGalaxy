#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Maths;
using System;
using System.Collections.Generic;
using NasBlockAction = NotAwesomeSurvival.Action<NotAwesomeSurvival.NasLevel, NotAwesomeSurvival.NasBlock, int, int, int>;
namespace NotAwesomeSurvival
{
    public partial class NasBlock
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
            Nas.FromRaw(644),
            Nas.FromRaw(645),
            Nas.FromRaw(646),
            Nas.FromRaw(461),
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
            Nas.FromRaw(704),
            Nas.FromRaw(705),
            Nas.FromRaw(706)
        },
        stickyPistonUp =
        {
            Nas.FromRaw(678),
            Nas.FromRaw(679),
            Nas.FromRaw(680)
        },
        pistonDown =
        {
            Nas.FromRaw(707),
            Nas.FromRaw(708),
            Nas.FromRaw(709)
        },
        stickyPistonDown =
        {
            Nas.FromRaw(710),
            Nas.FromRaw(711),
            Nas.FromRaw(712)
        },
        pistonNorth =
        {
            Nas.FromRaw(389),
            Nas.FromRaw(390),
            Nas.FromRaw(391)
        },
        pistonEast =
        {
            Nas.FromRaw(392),
            Nas.FromRaw(393),
            Nas.FromRaw(394)
        },
        pistonSouth =
        {
            Nas.FromRaw(395),
            Nas.FromRaw(396),
            Nas.FromRaw(397)
        },
        pistonWest =
        {
            Nas.FromRaw(398),
            Nas.FromRaw(399),
            Nas.FromRaw(400)
        },
        stickyPistonNorth =
        {
            Nas.FromRaw(401),
            Nas.FromRaw(402),
            Nas.FromRaw(403)
        },
        stickyPistonEast =
        {
            Nas.FromRaw(404),
            Nas.FromRaw(405),
            Nas.FromRaw(406)
        },
        stickyPistonSouth =
        {
            Nas.FromRaw(407),
            Nas.FromRaw(408),
            Nas.FromRaw(409)
        },
        stickyPistonWest =
        {
            Nas.FromRaw(410),
            Nas.FromRaw(411),
            Nas.FromRaw(412)
        },
        unpushable =
        {
            Nas.FromRaw(690),
            Nas.FromRaw(647),
            Nas.FromRaw(216),
            Nas.FromRaw(217),
            Nas.FromRaw(218),
            Nas.FromRaw(219),
            Nas.FromRaw(602),
            Nas.FromRaw(603),
            Nas.FromRaw(143),
            Nas.FromRaw(171),
            Nas.FromRaw(54),
            Nas.FromRaw(703),
            Nas.FromRaw(7),
            Nas.FromRaw(767),
            Nas.FromRaw(674),
            Nas.FromRaw(675),
            Nas.FromRaw(195),
            Nas.FromRaw(196),
            Nas.FromRaw(172),
            Nas.FromRaw(173),
            Nas.FromRaw(174),
            Nas.FromRaw(175),
            Nas.FromRaw(176),
            Nas.FromRaw(177),
            Nas.FromRaw(612),
            Nas.FromRaw(613),
            Nas.FromRaw(614),
            Nas.FromRaw(615),
            Nas.FromRaw(616),
            Nas.FromRaw(617),
            Nas.FromRaw(413),
            Nas.FromRaw(414),
            Nas.FromRaw(439),
            Nas.FromRaw(440),
            Nas.FromRaw(441),
            Nas.FromRaw(442),
            Nas.FromRaw(443),
            Nas.FromRaw(444),
            Nas.FromRaw(673),
            Nas.FromRaw(457),
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
            Nas.FromRaw(644),
            Nas.FromRaw(645),
            Nas.FromRaw(646),
            Nas.FromRaw(461)
        },
        ironSet = new ushort[]
        {
            Nas.FromRaw(729),
            Nas.FromRaw(730),
            Nas.FromRaw(731),
            Nas.FromRaw(479)
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
        public static NasBlockAction FloodAction(ushort[] set)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (CanInfiniteFloodKillThis(nl, x, y - 1, z, set))
                {
                    nl.SetBlock(x, y - 1, z, set[LiquidInfiniteIndex]);
                    return;
                }
                if (CanInfiniteFloodKillThis(nl, x + 1, y, z, set))
                {
                    nl.SetBlock(x + 1, y, z, set[LiquidInfiniteIndex]);
                }
                if (CanInfiniteFloodKillThis(nl, x - 1, y, z, set))
                {
                    nl.SetBlock(x - 1, y, z, set[LiquidInfiniteIndex]);
                }
                if (CanInfiniteFloodKillThis(nl, x, y, z + 1, set))
                {
                    nl.SetBlock(x, y, z + 1, set[LiquidInfiniteIndex]);
                }
                if (CanInfiniteFloodKillThis(nl, x, y, z - 1, set))
                {
                    nl.SetBlock(x, y, z - 1, set[LiquidInfiniteIndex]);
                }
            };
        }
        public static bool CanInfiniteFloodKillThis(NasLevel nl, int x, int y, int z, ushort[] set)
        {
            ushort here = nl.GetBlock(x, y, z);
            if (CanPhysicsKillThis(here) || IsPartOfSet(set, here) > LiquidInfiniteIndex)
            {
                return true;
            }
            return false;
        }
        public static bool CanPhysicsKillThis(ushort block)
        {
            for (int i = 0; i < blocksPhysicsCanKill.Length; i++)
            {
                if (block == blocksPhysicsCanKill[i])
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsThisLiquid(ushort block)
        {
            if (IsPartOfSet(waterSet, block) != -1)
            {
                return true;
            }
            return false;
        }
        public static int IsPartOfSet(ushort[] set, ushort block)
        {
            for (int i = 0; i < set.Length; i++)
            {
                if (set[i] == block)
                {
                    return i;
                }
            }
            return -1;
        }
        public static int CanReplaceBlockAt(NasLevel nl, int x, int y, int z, ushort[] set, int spreadIndex)
        {
            ushort hereBlock = nl.GetBlock(x, y, z);
            if (nl.GetBlock(x, y - 1, z) == Nas.FromRaw(703))
            {
                return -1;
            }
            if (CanPhysicsKillThis(hereBlock))
            {
                return spreadIndex + 1;
            }
            int hereIndex = IsPartOfSet(set, hereBlock);
            return hereIndex;
        }
        public static bool CanLiquidLive(NasLevel nl, ushort[] set, int index, int x, int y, int z)
        {
            ushort neighbor = nl.GetBlock(x, y, z);
            if (neighbor == set[index - 1] ||
                neighbor == set[LiquidSourceIndex] ||
                neighbor == set[LiquidWaterfallIndex]
               )
            {
                return true;
            }
            return false;
        }
        public static NasBlockAction LimitedFloodAction(ushort[] set, int index)
        {
            return (nl, nasBlock, x, y, z) =>
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
                        if (hereBlock == 10 || hereBlock == 11)
                        {
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
                    {
                        borders++;
                    }
                    if (nl.GetBlock(x - 1, y, z) == set[LiquidSourceIndex])
                    {
                        borders++;
                    }
                    if (nl.GetBlock(x, y, z + 1) == set[LiquidSourceIndex])
                    {
                        borders++;
                    }
                    if (nl.GetBlock(x, y, z - 1) == set[LiquidSourceIndex])
                    {
                        borders++;
                    }
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
                    {
                        return;
                    }
                    nl.SetBlock(x, y - 1, z, set[LiquidWaterfallIndex]);
                    return;
                }
                if (index == set.Length - 1)
                {
                    return;
                }
                int spreadIndex = (index < LiquidWaterfallIndex + 1) ? LiquidWaterfallIndex + 1 : index + 1;
                ushort spreadBlock = set[spreadIndex];
                CanFlowInDirection(nl, x, y, z, set, spreadIndex,
                                   out bool posX,
                                   out bool negX,
                                   out bool posZ,
                                   out bool negZ);
                if (posX)
                {
                    nl.SetBlock(x + 1, y, z, spreadBlock);
                }
                if (negX)
                {
                    nl.SetBlock(x - 1, y, z, spreadBlock);
                }
                if (posZ)
                {
                    nl.SetBlock(x, y, z + 1, spreadBlock);
                }
                if (negZ)
                {
                    nl.SetBlock(x, y, z - 1, spreadBlock);
                }
            };
        }
        public static void CanFlowInDirection(NasLevel nl, int x, int y, int z,
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
            {
                xBlockedPos = true;
            }
            if (neighborIndex2 == -1)
            {
                xBlockedNeg = true;
            }
            if (neighborIndex3 == -1)
            {
                zBlockedPos = true;
            }
            if (neighborIndex4 == -1)
            {
                zBlockedNeg = true;
            }
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
        public class FloodSim
        {
            public NasLevel nl;
            public int xO,
                yO,
                zO,
                totalDistance,
                widthAndHeight,
                distanceHolesWereFoundAt;
            public ushort[] liquidSet;
            public bool[,] waterAtSpot;
            public List<Vec3S32> holes;
            public FloodSim(NasLevel nl, int xO, int yO, int zO, int totalDistance, ushort[] set)
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
                if (distanceFromCenter > totalDistance)
                {
                    return;
                }
                if (AlreadyFlooded(x, z))
                {
                    return;
                }
                ushort here = nl.GetBlock(x, y, z);
                if (!(CanPhysicsKillThis(here) || IsPartOfSet(liquidSet, here) != -1))
                {
                    return;
                }
                ushort below = nl.GetBlock(x, y - 1, z);
                if (CanPhysicsKillThis(below) || IsPartOfSet(liquidSet, below) != -1)
                {
                    if (distanceFromCenter < distanceHolesWereFoundAt)
                    {
                        holes.Clear();
                        holes.Add(new(x, y - 1, z));
                        distanceHolesWereFoundAt = distanceFromCenter;
                    }
                    else if (distanceFromCenter == distanceHolesWereFoundAt)
                    {
                        holes.Add(new(x, y - 1, z));
                    }
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
                if (
                    xI >= widthAndHeight ||
                    zI >= widthAndHeight ||
                    xI < 0 ||
                    zI < 0
                   )
                {
                    return false;
                }
                return waterAtSpot[xI, zI];
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
        public static List<Vec3S32> HolesInRange(NasLevel nl, int x, int y, int z, int totalDistance, ushort[] set, out int distance)
        {
            FloodSim sim = new(nl, x, y, z, totalDistance, set);
            return sim.GetHoles(out distance);
        }
        public static NasBlockAction FallingBlockAction(ushort serverushort)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                ushort blockUnder = nl.GetBlock(x, y - 1, z);
                if (nl.GetBlock(x, y - 2, z) == Nas.FromRaw(703))
                {
                    return;
                }
                if (CanPhysicsKillThis(blockUnder) || IsPartOfSet(waterSet, blockUnder) != -1)
                {
                    nl.SetBlock(x, y, z, 0);
                    nl.SetBlock(x, y - 1, z, serverushort);
                }
            };
        }
        public static NasBlockAction GrassBlockAction(ushort grass, ushort dirt)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (grass == Nas.FromRaw(139) && nl.biome != 2)
                {
                    nl.SetBlock(x, y, z, Nas.FromRaw(129));
                }
                ushort aboveHere = nl.GetBlock(x, y + 1, z);
                if (!nl.lvl.LightPasses(aboveHere))
                {
                    nl.SetBlock(x, y, z, dirt);
                }
            };
        }
        public static NasBlockAction DirtBlockAction(ushort[] grassSet, ushort _)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                ushort aboveHere = nl.GetBlock(x, y + 1, z);
                if (!nl.lvl.LightPasses(aboveHere))
                {
                    return;
                }
                for (int xOff = -1; xOff <= 1; xOff++)
                {
                    for (int yOff = -1; yOff <= 1; yOff++)
                    {
                        for (int zOff = -1; zOff <= 1; zOff++)
                        {
                            if (xOff == 0 && yOff == -1 && zOff == 0)
                            {
                                continue;
                            }
                            ushort neighbor = nl.GetBlock(x + xOff, y + yOff, z + zOff);
                            int setIndex = IsPartOfSet(grassSet, neighbor);
                            if (setIndex == 3 && nl.biome != 2)
                            {
                                setIndex = 2;
                            }
                            if (setIndex != 3 && nl.biome == 2 && setIndex != -1)
                            {
                                setIndex = 3;
                            }
                            if (setIndex == -1)
                            {
                                continue;
                            }
                            nl.SetBlock(x, y, z, grassSet[setIndex], true);
                            if (nl.GetBlock(x, y + 1, z) == 0)
                            {
                                if (r.Next(0, 20) == 0)
                                {
                                    nl.SetBlock((ushort)x, (ushort)(y + 1), (ushort)z, 256 | 130);
                                }
                                else
                                {
                                    int flowerChance = r.Next(0, 100);
                                    if (flowerChance == 0)
                                    {
                                        nl.SetBlock(x, y + 1, z, 256 | 96);
                                    }
                                    else
                                    {
                                        if (flowerChance == 1)
                                        {
                                            nl.SetBlock(x, y + 1, z, 37);
                                        }
                                        else
                                        {
                                            if (flowerChance == 2)
                                            {
                                                nl.SetBlock(x, y + 1, z, 38);
                                            }
                                            else
                                            {
                                                if (flowerChance == 3)
                                                {
                                                    nl.SetBlock(x, y + 1, z, 256 | 651);
                                                }
                                                else
                                                {
                                                    if (flowerChance == 4)
                                                    {
                                                        if (r.Next(0, 20) == 0)
                                                        {
                                                            nl.SetBlock(x, y + 1, z, 256 | 604);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (flowerChance == 5)
                                                        {
                                                            nl.SetBlock(x, y + 1, z, 256 | 201);
                                                        }
                                                        else
                                                        {
                                                            if (nl.biome == 2)
                                                            {
                                                                nl.SetBlock(x, y + 1, z, 53);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return;
            };
        }
        public static NasBlockAction ObserverActivateAction(int type)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                {
                    nl.blockEntities.Add(x + " " + y + " " + z, new());
                    nl.blockEntities[x + " " + y + " " + z].type = type;
                }
                nl.blockEntities[x + " " + y + " " + z].strength = 15;
                nl.SetBlock(x, y, z, (ushort)(nl.lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z) + 6));
            };
        }
        public static NasBlockAction ObserverDeactivateAction(int type)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                {
                    nl.blockEntities.Add(x + " " + y + " " + z, new());
                    nl.blockEntities[x + " " + y + " " + z].type = type;
                }
                nl.blockEntities[x + " " + y + " " + z].strength = 0;
                nl.SetBlock(x, y, z, (ushort)(nl.lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z) - 6));
            };
        }
        public static NasBlockAction LeafBlockAction(ushort[] _, ushort leaf)
        {
            return (nl, nasBlock, x, y, z) =>
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
                {
                    return;
                }
                nl.SetBlock(x, y, z, 0);
                if (r.Next(0, 384) == 0 && CanPhysicsKillThis(nl.GetBlock(x, y - 1, z)))
                {
                    if (leaf == 18)
                    {
                        nl.SetBlock(x, y - 1, z, Nas.FromRaw(648));
                    }
                    if (leaf == Nas.FromRaw(103))
                    {
                        nl.SetBlock(x, y - 1, z, Nas.FromRaw(702));
                    }
                }
            };
        }
        public static NasBlockAction GrowAction(ushort grow)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (grow == Nas.FromRaw(667))
                {
                    if (12 == nl.GetBlock(x, y - 1, z))
                    {
                        if (!((nl.GetBlock(x - 1, y - 1, z) == 8) ||
                              (nl.GetBlock(x + 1, y - 1, z) == 8) ||
                              (nl.GetBlock(x, y - 1, z + 1) == 8) ||
                              (nl.GetBlock(x, y - 1, z - 1) == 8) ||
                              (nl.GetBlock(x - 1, y - 1, z) == 9) ||
                              (nl.GetBlock(x + 1, y - 1, z) == 9) ||
                              (nl.GetBlock(x, y - 1, z + 1) == 9) ||
                              (nl.GetBlock(x, y - 1, z - 1) == 9)))
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (!((nl.GetBlock(x - 1, y - 2, z) == 8) ||
                              (nl.GetBlock(x + 1, y - 2, z) == 8) ||
                              (nl.GetBlock(x, y - 2, z + 1) == 8) ||
                              (nl.GetBlock(x, y - 2, z - 1) == 8) ||
                              (nl.GetBlock(x - 1, y - 2, z) == 9) ||
                              (nl.GetBlock(x + 1, y - 2, z) == 9) ||
                              (nl.GetBlock(x, y - 2, z + 1) == 9) ||
                              (nl.GetBlock(x, y - 2, z - 1) == 9)))
                        {
                            return;
                        }
                    }
                }
                if (!((grow == nl.GetBlock(x, y - 1, z) && nl.GetBlock(x, y - 2, z) == 12) || nl.GetBlock(x, y - 1, z) == 12))
                {
                    return;
                }
                if (((nl.GetBlock(x, y - 1, z) == grow) && (nl.GetBlock(x, y - 2, z) == grow)) | (nl.GetBlock(x, y + 1, z) != 0))
                {
                    return;
                }
                nl.SetBlock(x, y + 1, z, grow);
            };
        }
        public static NasBlockAction VineGrowAction(ushort grow)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (nl.GetBlock(x, y + 1, z) == grow && nl.GetBlock(x, y + 2, z) == grow)
                {
                    return;
                }
                if (nl.GetBlock(x, y - 1, z) == 0)
                {
                    nl.SetBlock(x, y - 1, z, grow);
                }
            };
        }
        public static NasBlockAction VineDeathAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (IsPartOfSet(blocksPhysicsCanKill, nl.GetBlock(x, y + 1, z)) != -1)
                {
                    nl.SetBlock(x, y, z, 0);
                }
            };
        }
        public static NasBlockAction LilyAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (IsPartOfSet(waterSet, nl.GetBlock(x, y - 1, z)) == -1)
                {
                    nl.SetBlock(x, y, z, 0);
                }
            };
        }
        public static void IsThereLog(NasLevel nl, int x, int y, int z, ushort leaf, int iteration, ref bool canLive)
        {
            if (canLive)
            {
                return;
            }
            ushort hereBlock = nl.GetBlock(x, y, z);
            if (IsPartOfSet(logSet, hereBlock) != -1)
            {
                canLive = true;
                return;
            }
            if (hereBlock != leaf)
            {
                return;
            }
            if ((iteration >= 10 && leaf == Nas.FromRaw(104)) || (iteration >= 5 && leaf != Nas.FromRaw(104)))
            {
                return;
            }
            iteration++;
            IsThereLog(nl, x + 1, y, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y + 1, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y, z + 1, leaf, iteration, ref canLive);
            IsThereLog(nl, x - 1, y, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y - 1, z, leaf, iteration, ref canLive);
            IsThereLog(nl, x, y, z - 1, leaf, iteration, ref canLive);
        }
        public static NasBlockAction FireAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (IsPartOfSet(infinifire, nl.GetBlock(x, y - 1, z)) == -1)
                {
                    if (r.Next(0, 8) == 0)
                    {
                        nl.SetBlock(x, y, z, Nas.FromRaw(131));
                    }
                    else
                    {
                        nl.SetBlock(x, y, z, 0);
                    }
                }
            };
        }
        public static NasBlockAction LampAction(ushort on, ushort off, ushort me)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                Entity[] b = new Entity[6];
                if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                {
                    b[0] = nl.blockEntities[x + " " + (y - 1) + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                {
                    b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                {
                    b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                {
                    b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                {
                    b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                }
                if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                {
                    b[5] = nl.blockEntities[x + " " + (y + 1) + " " + z];
                }
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
                    {
                        nl.SetBlock(x, y, z, on);
                    }
                }
                else
                {
                    if (on == me)
                    {
                        nl.SetBlock(x, y, z, off);
                    }
                }
            };
        }
        public static NasBlockAction UnrefinedGoldAction(ushort on, ushort off, ushort me)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                Entity[] b = new Entity[6];
                if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                {
                    b[0] = nl.blockEntities[x + " " + (y - 1) + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                {
                    b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                {
                    b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                {
                    b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                {
                    b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                }
                if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                {
                    b[5] = nl.blockEntities[x + " " + (y + 1) + " " + z];
                }
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
                        {
                            return;
                        }
                        nl.SetBlock(x, y, z, on);
                        nl.blockEntities[x + " " + y + " " + z].strength = 15;
                    }
                }
                else
                {
                    if (on == me)
                    {
                        if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                        {
                            return;
                        }
                        nl.SetBlock(x, y, z, off);
                        nl.blockEntities[x + " " + y + " " + z].strength = 0;
                    }
                }
            };
        }
        public static NasBlockAction SidewaysPistonAction(string type, string axis, int dir, ushort[] pistonSet, bool sticky = false)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                int changeX = 0,
                changeZ = 0,
                changeY = 0;
                ushort[] dontpush =
                {
                    0, 0, 0, 0,
                    Nas.FromRaw(680),
                    Nas.FromRaw(706),
                    Nas.FromRaw(709),
                    Nas.FromRaw(712)
                };
                if (axis.CaselessEq("x"))
                {
                    changeX = dir;
                    changeZ = 0;
                    dontpush[0] = Nas.FromRaw(391);
                    dontpush[1] = Nas.FromRaw(397);
                    dontpush[2] = Nas.FromRaw(403);
                    dontpush[3] = Nas.FromRaw(409);
                }
                if (axis.CaselessEq("z"))
                {
                    changeZ = dir;
                    changeX = 0;
                    dontpush[0] = Nas.FromRaw(394);
                    dontpush[1] = Nas.FromRaw(400);
                    dontpush[2] = Nas.FromRaw(406);
                    dontpush[3] = Nas.FromRaw(412);
                }
                if (type.CaselessEq("off"))
                {
                    Entity[] b = new Entity[6];
                    if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                    {
                        b[0] = nl.blockEntities[x + " " + (y + 1) + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                    {
                        b[1] = nl.blockEntities[x + " " + (y - 1) + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z) && (changeX != 1))
                    {
                        b[2] = nl.blockEntities[x + 1 + " " + y + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z) && (changeX != -1))
                    {
                        b[3] = nl.blockEntities[x - 1 + " " + y + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)) && (changeZ != 1))
                    {
                        b[4] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)) && (changeZ != -1))
                    {
                        b[5] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                    }
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
                        if (above[0] == 0 || above[0] == 8)
                        {
                            push = 0;
                        }
                        else
                        {
                            if (IsPartOfSet(unpushable, above[0]) != -1 || IsPartOfSet(dontpush, above[0]) != -1)
                            {
                                return;
                            }
                            above[1] = TurnValid(nl.GetBlock(x + 2 * changeX, y + 2 * changeY, z + 2 * changeZ));
                            if (above[1] == 0 || above[1] == 8)
                            {
                                push = 1;
                            }
                            else
                            {
                                if (IsPartOfSet(unpushable, above[1]) != -1 || IsPartOfSet(dontpush, above[1]) != -1)
                                {
                                    return;
                                }
                                above[2] = TurnValid(nl.GetBlock(x + 3 * changeX, y + 3 * changeY, z + 3 * changeZ));
                                if (above[2] == 0 || above[2] == 8)
                                {
                                    push = 2;
                                }
                                else
                                {
                                    if (IsPartOfSet(unpushable, above[2]) != -1 || IsPartOfSet(dontpush, above[2]) != -1)
                                    {
                                        return;
                                    }
                                    above[3] = TurnValid(nl.GetBlock(x + 4 * changeX, y + 4 * changeY, z + 4 * changeZ));
                                    if (above[3] == 0 || above[3] == 8)
                                    {
                                        push = 3;
                                    }
                                    else
                                    {
                                        if (IsPartOfSet(unpushable, above[3]) != -1 || IsPartOfSet(dontpush, above[3]) != -1)
                                        {
                                            return;
                                        }
                                        above[4] = TurnValid(nl.GetBlock(x + 5 * changeX, y + 5 * changeY, z + 5 * changeZ));
                                        if (above[4] == 0 || above[4] == 8)
                                        {
                                            push = 4;
                                        }
                                        else
                                        {
                                            if (IsPartOfSet(unpushable, above[4]) != -1 || IsPartOfSet(dontpush, above[4]) != -1)
                                            {
                                                return;
                                            }
                                            above[5] = TurnValid(nl.GetBlock(x + 6 * changeX, y + 6 * changeY, z + 6 * changeZ));
                                            if (above[5] == 0 || above[5] == 8)
                                            {
                                                push = 5;
                                            }
                                            else
                                            {
                                                if (IsPartOfSet(unpushable, above[5]) != -1 || IsPartOfSet(dontpush, above[5]) != -1)
                                                {
                                                    return;
                                                }
                                                above[6] = TurnValid(nl.GetBlock(x + 7 * changeX, y + 7 * changeY, z + 7 * changeZ));
                                                if (above[6] == 0 || above[6] == 8)
                                                {
                                                    push = 6;
                                                }
                                                else
                                                {
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (push >= 6)
                        {
                            nl.SetBlock(x + (7 * changeX), y + (7 * changeY), z + (7 * changeZ), above[5]);
                            if (nl.blockEntities.ContainsKey(x + (6 * changeX) + " " + (y + (6 * changeY)) + " " + (z + (6 * changeZ))))
                            {
                                nl.blockEntities.Remove(x + (6 * changeX) + " " + (y + (6 * changeY)) + " " + (z + (6 * changeZ)));
                            }
                        }
                        if (push >= 5)
                        {
                            nl.SetBlock(x + (6 * changeX), y + (6 * changeY), z + (6 * changeZ), above[4]);
                            if (nl.blockEntities.ContainsKey(x + (5 * changeX) + " " + (y + (5 * changeY)) + " " + (z + (5 * changeZ))))
                            {
                                nl.blockEntities.Remove(x + (5 * changeX) + " " + (y + (5 * changeY)) + " " + (z + (5 * changeZ)));
                            }
                        }
                        if (push >= 4)
                        {
                            nl.SetBlock(x + (5 * changeX), y + (5 * changeY), z + (5 * changeZ), above[3]);
                            if (nl.blockEntities.ContainsKey(x + (4 * changeX) + " " + (y + (4 * changeY)) + " " + (z + (4 * changeZ))))
                            {
                                nl.blockEntities.Remove(x + (4 * changeX) + " " + (y + (4 * changeY)) + " " + (z + (4 * changeZ)));
                            }
                        }
                        if (push >= 3)
                        {
                            nl.SetBlock(x + (4 * changeX), y + (4 * changeY), z + (4 * changeZ), above[2]);
                            if (nl.blockEntities.ContainsKey(x + (3 * changeX) + " " + (y + (3 * changeY)) + " " + (z + (3 * changeZ))))
                            {
                                nl.blockEntities.Remove(x + (3 * changeX) + " " + (y + (3 * changeY)) + " " + (z + (3 * changeZ)));
                            }
                        }
                        if (push >= 2)
                        {
                            nl.SetBlock(x + (3 * changeX), y + (3 * changeY), z + (3 * changeZ), above[1]);
                            if (nl.blockEntities.ContainsKey(x + (2 * changeX) + " " + (y + (2 * changeY)) + " " + (z + (2 * changeZ))))
                            {
                                nl.blockEntities.Remove(x + (2 * changeX) + " " + (y + (2 * changeY)) + " " + (z + (2 * changeZ)));
                            }
                        }
                        if (push >= 1)
                        {
                            nl.SetBlock(x + (2 * changeX), y + (2 * changeY), z + (2 * changeZ), above[0]);
                            if (nl.blockEntities.ContainsKey(x + changeX + " " + (y + changeY) + " " + (z + changeZ)))
                            {
                                nl.blockEntities.Remove(x + changeX + " " + (y + changeY) + " " + (z + changeZ));
                            }
                        }
                        nl.SetBlock(x + changeX, y + changeY, z + changeZ, pistonSet[2]);
                        nl.SetBlock(x, y, z, pistonSet[1]);
                        Player[] players = PlayerInfo.Online.Items;
                        for (int i = 0; i < players.Length; i++)
                        {
                            Player who = players[i];
                            Vec3F32 posH = who.Pos.BlockCoords;
                            if (who.Pos.FeetBlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ) || who.Pos.BlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ))
                            {
                                if (Get(Collision.ConvertToClientushort(nl.GetBlock((int)posH.X + changeX, (int)posH.Y + changeY, (int)posH.Z + changeZ))).collideAction != DefaultSolidCollideAction())
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
                }
                if (type.CaselessEq("body"))
                {
                    Entity[] b = new Entity[6];
                    if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                    {
                        b[0] = nl.blockEntities[x + " " + (y + 1) + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                    {
                        b[1] = nl.blockEntities[x + " " + (y - 1) + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z) && (changeX != 1))
                    {
                        b[2] = nl.blockEntities[x + 1 + " " + y + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z) && (changeX != -1))
                    {
                        b[3] = nl.blockEntities[x - 1 + " " + y + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)) && (changeZ != 1))
                    {
                        b[4] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)) && (changeZ != -1))
                    {
                        b[5] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                    }
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
                                {
                                    nl.blockEntities.Remove(x + (2 * changeX) + " " + (y + (2 * changeY)) + " " + (z + (2 * changeZ)));
                                }
                                return;
                            }
                            return;
                        }
                    }
                    if (nl.GetBlock(x + changeX, y + changeY, z + changeZ) != pistonSet[2])
                    {
                        nl.SetBlock(x + changeX, y + changeY, z + changeZ, pistonSet[2]);
                    }
                }
                if (type.CaselessEq("head"))
                {
                    if (nl.GetBlock(x - changeX, y - changeY, z - changeZ) != pistonSet[1])
                    {
                        nl.SetBlock(x, y, z, 0);
                    }
                }
            };
        }
        public static NasBlockAction PistonAction(string type, int changeX, int changeY, int changeZ, ushort[] pistonSet)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                ushort[] dontpush =
                {
                    Nas.FromRaw(391),
                    Nas.FromRaw(397),
                    Nas.FromRaw(403),
                    Nas.FromRaw(409),
                    Nas.FromRaw(394),
                    Nas.FromRaw(400),
                    Nas.FromRaw(406),
                    Nas.FromRaw(412),
                };
                if (type.CaselessEq("off"))
                {
                    Entity[] b = new Entity[5];
                    if (nl.blockEntities.ContainsKey(x + " " + (y - changeY) + " " + z))
                    {
                        b[0] = nl.blockEntities[x + " " + (y - changeY) + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                    {
                        b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                    {
                        b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                    {
                        b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                    {
                        b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                    }
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
                        if (above[0] == 0 || above[0] == 8)
                        {
                            push = 0;
                        }
                        else
                        {
                            if (IsPartOfSet(unpushable, above[0]) != -1 || IsPartOfSet(dontpush, above[0]) != -1)
                            {
                                return;
                            }
                            above[1] = TurnValid(nl.GetBlock(x + 2 * changeX, y + 2 * changeY, z + 2 * changeZ));
                            if (above[1] == 0 || above[1] == 8)
                            {
                                push = 1;
                            }
                            else
                            {
                                if (IsPartOfSet(unpushable, above[1]) != -1 || IsPartOfSet(dontpush, above[1]) != -1)
                                {
                                    return;
                                }
                                above[2] = TurnValid(nl.GetBlock(x + 3 * changeX, y + 3 * changeY, z + 3 * changeZ));
                                if (above[2] == 0 || above[2] == 8)
                                {
                                    push = 2;
                                }
                                else
                                {
                                    if (IsPartOfSet(unpushable, above[2]) != -1 || IsPartOfSet(dontpush, above[2]) != -1)
                                    {
                                        return;
                                    }
                                    above[3] = TurnValid(nl.GetBlock(x + 4 * changeX, y + 4 * changeY, z + 4 * changeZ));
                                    if (above[3] == 0 || above[3] == 8)
                                    {
                                        push = 3;
                                    }
                                    else
                                    {
                                        if (IsPartOfSet(unpushable, above[3]) != -1 || IsPartOfSet(dontpush, above[3]) != -1)
                                        {
                                            return;
                                        }
                                        above[4] = TurnValid(nl.GetBlock(x + 5 * changeX, y + 5 * changeY, z + 5 * changeZ));
                                        if (above[4] == 0 || above[4] == 8)
                                        {
                                            push = 4;
                                        }
                                        else
                                        {
                                            if (IsPartOfSet(unpushable, above[4]) != -1 || IsPartOfSet(dontpush, above[4]) != -1)
                                            {
                                                return;
                                            }
                                            above[5] = TurnValid(nl.GetBlock(x + 6 * changeX, y + 6 * changeY, z + 6 * changeZ));
                                            if (above[5] == 0 || above[5] == 8)
                                            {
                                                push = 5;
                                            }
                                            else
                                            {
                                                if (IsPartOfSet(unpushable, above[5]) != -1 || IsPartOfSet(dontpush, above[5]) != -1)
                                                {
                                                    return;
                                                }
                                                above[6] = TurnValid(nl.GetBlock(x + 7 * changeX, y + 7 * changeY, z + 7 * changeZ));
                                                if (above[6] == 0 || above[6] == 8)
                                                {
                                                    push = 6;
                                                }
                                                else
                                                {
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (push >= 6)
                        {
                            nl.SetBlock(x, y + (7 * changeY), z, above[5]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (6 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (6 * changeY)) + " " + z);
                            }
                        }
                        if (push >= 5)
                        {
                            nl.SetBlock(x, y + (6 * changeY), z, above[4]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (5 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (5 * changeY)) + " " + z);
                            }
                        }
                        if (push >= 4)
                        {
                            nl.SetBlock(x, y + (5 * changeY), z, above[3]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (4 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (4 * changeY)) + " " + z);
                            }
                        }
                        if (push >= 3)
                        {
                            nl.SetBlock(x, y + (4 * changeY), z, above[2]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (3 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (3 * changeY)) + " " + z);
                            }
                        }
                        if (push >= 2)
                        {
                            nl.SetBlock(x, y + (3 * changeY), z, above[1]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (2 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (2 * changeY)) + " " + z);
                            }
                        }
                        if (push >= 1)
                        {
                            nl.SetBlock(x, y + (2 * changeY), z, above[0]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (1 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (1 * changeY)) + " " + z);
                            }
                        }
                        nl.SetBlock(x, y + changeY, z, pistonSet[2]);
                        nl.SetBlock(x, y, z, pistonSet[1]);
                        Player[] players = PlayerInfo.Online.Items;
                        for (int i = 0; i < players.Length; i++)
                        {
                            Player who = players[i];
                            Vec3F32 posH = who.Pos.BlockCoords;
                            if (who.Pos.FeetBlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ) || who.Pos.BlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ))
                            {
                                if (Get(Collision.ConvertToClientushort(nl.GetBlock((int)posH.X + changeX, (int)posH.Y + changeY, (int)posH.Z + changeZ))).collideAction != DefaultSolidCollideAction())
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
                }
                if (type.CaselessEq("body"))
                {
                    Entity[] b = new Entity[5];
                    if (nl.blockEntities.ContainsKey(x + " " + (y - changeY) + " " + z))
                    {
                        b[0] = nl.blockEntities[x + " " + (y - changeY) + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                    {
                        b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                    {
                        b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                    {
                        b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                    {
                        b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                    }
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
                    {
                        nl.SetBlock(x, y + changeY, z, pistonSet[2]);
                    }
                }
                if (type.CaselessEq("head"))
                {
                    if (nl.GetBlock(x, y - changeY, z) != pistonSet[1])
                    {
                        nl.SetBlock(x, y, z, 0);
                    }
                }
            };
        }
        public static NasBlockAction StickyPistonAction(string type, int changeX, int changeY, int changeZ, ushort[] pistonSet)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                ushort[] dontpush =
                {
                    Nas.FromRaw(391),
                    Nas.FromRaw(397),
                    Nas.FromRaw(403),
                    Nas.FromRaw(409),
                    Nas.FromRaw(394),
                    Nas.FromRaw(400),
                    Nas.FromRaw(406),
                    Nas.FromRaw(412),
                };
                Entity[] b = new Entity[5];
                if (nl.blockEntities.ContainsKey(x + " " + (y - changeY) + " " + z))
                {
                    b[0] = nl.blockEntities[x + " " + (y - changeY) + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                {
                    b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                {
                    b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                {
                    b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                {
                    b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                }
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
                        if (above[0] == 0 || above[0] == 8)
                        {
                            push = 0;
                        }
                        else
                        {
                            if (IsPartOfSet(unpushable, above[0]) != -1 || IsPartOfSet(dontpush, above[0]) != -1)
                            {
                                return;
                            }
                            above[1] = TurnValid(nl.GetBlock(x + 2 * changeX, y + 2 * changeY, z + 2 * changeZ));
                            if (above[1] == 0 || above[1] == 8)
                            {
                                push = 1;
                            }
                            else
                            {
                                if (IsPartOfSet(unpushable, above[1]) != -1 || IsPartOfSet(dontpush, above[1]) != -1)
                                {
                                    return;
                                }
                                above[2] = TurnValid(nl.GetBlock(x + 3 * changeX, y + 3 * changeY, z + 3 * changeZ));
                                if (above[2] == 0 || above[2] == 8)
                                {
                                    push = 2;
                                }
                                else
                                {
                                    if (IsPartOfSet(unpushable, above[2]) != -1 || IsPartOfSet(dontpush, above[2]) != -1)
                                    {
                                        return;
                                    }
                                    above[3] = TurnValid(nl.GetBlock(x + 4 * changeX, y + 4 * changeY, z + 4 * changeZ));
                                    if (above[3] == 0 || above[3] == 8)
                                    {
                                        push = 3;
                                    }
                                    else
                                    {
                                        if (IsPartOfSet(unpushable, above[3]) != -1 || IsPartOfSet(dontpush, above[3]) != -1)
                                        { return; }
                                        above[4] = TurnValid(nl.GetBlock(x + 5 * changeX, y + 5 * changeY, z + 5 * changeZ));
                                        if (above[4] == 0 || above[4] == 8)
                                        {
                                            push = 4;
                                        }
                                        else
                                        {
                                            if (IsPartOfSet(unpushable, above[4]) != -1 || IsPartOfSet(dontpush, above[4]) != -1)
                                            {
                                                return;
                                            }
                                            above[5] = TurnValid(nl.GetBlock(x + 6 * changeX, y + 6 * changeY, z + 6 * changeZ));
                                            if (above[5] == 0 || above[5] == 8)
                                            {
                                                push = 5;
                                            }
                                            else
                                            {
                                                if (IsPartOfSet(unpushable, above[5]) != -1 || IsPartOfSet(dontpush, above[5]) != -1)
                                                {
                                                    return;
                                                }
                                                above[6] = TurnValid(nl.GetBlock(x + 7 * changeX, y + 7 * changeY, z + 7 * changeZ));
                                                if (above[6] == 0 || above[6] == 8)
                                                {
                                                    push = 6;
                                                }
                                                else
                                                {
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (push >= 6)
                        {
                            nl.SetBlock(x, y + (7 * changeY), z, above[5]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (6 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (6 * changeY)) + " " + z);
                            }
                        }
                        if (push >= 5)
                        {
                            nl.SetBlock(x, y + (6 * changeY), z, above[4]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (5 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (5 * changeY)) + " " + z);
                            }
                        }
                        if (push >= 4)
                        {
                            nl.SetBlock(x, y + (5 * changeY), z, above[3]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (4 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (4 * changeY)) + " " + z);
                            }
                        }
                        if (push >= 3)
                        {
                            nl.SetBlock(x, y + (4 * changeY), z, above[2]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (3 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (3 * changeY)) + " " + z);
                            }
                        }
                        if (push >= 2)
                        {
                            nl.SetBlock(x, y + (3 * changeY), z, above[1]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (2 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (2 * changeY)) + " " + z);
                            }
                        }
                        if (push >= 1)
                        {
                            nl.SetBlock(x, y + (2 * changeY), z, above[0]);
                            if (nl.blockEntities.ContainsKey(x + " " + (y + (1 * changeY)) + " " + z))
                            {
                                nl.blockEntities.Remove(x + " " + (y + (1 * changeY)) + " " + z);
                            }
                        }
                        nl.SetBlock(x, y + changeY, z, pistonSet[2]);
                        nl.SetBlock(x, y, z, pistonSet[1]);
                        Player[] players = PlayerInfo.Online.Items;
                        for (int i = 0; i < players.Length; i++)
                        {
                            Player who = players[i];
                            Vec3F32 posH = who.Pos.BlockCoords;
                            if (who.Pos.FeetBlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ) || who.Pos.BlockCoords == new Vec3S32(x + (push + 1) * changeX, y + (push + 1) * changeY, z + (push + 1) * changeZ))
                            {
                                if (Get(Collision.ConvertToClientushort(nl.GetBlock((int)posH.X + changeX, (int)posH.Y + changeY, (int)posH.Z + changeZ))).collideAction != DefaultSolidCollideAction())
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
                            {
                                nl.blockEntities.Remove(x + " " + (y + (2 * changeY)) + " " + z);
                            }
                        }
                        return;
                    }
                    if (nl.GetBlock(x, y + changeY, z) != pistonSet[2])
                    {
                        nl.SetBlock(x, y + changeY, z, pistonSet[2]);
                    }
                }
                if (type.CaselessEq("head"))
                {
                    if (nl.GetBlock(x, y - changeY, z) != pistonSet[1])
                    {
                        nl.SetBlock(x, y, z, 0);
                    }
                }
            };
        }
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
        public static ushort TurnValid(ushort block)
        {
            if (ConvertBody(block, pistonUp, out ushort returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, pistonDown, out returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, pistonNorth, out returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, pistonEast, out returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, pistonSouth, out returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, pistonWest, out returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, stickyPistonUp, out returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, stickyPistonDown, out returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, stickyPistonNorth, out returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, stickyPistonEast, out returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, stickyPistonSouth, out returnedBlock))
            {
                return returnedBlock;
            }
            if (ConvertBody(block, stickyPistonWest, out returnedBlock))
            {
                return returnedBlock;
            }
            return returnedBlock;
        }
        public static NasBlockAction PowerSourceAction(int direction)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                {
                    nl.blockEntities.Add(x + " " + y + " " + z, new());
                    nl.blockEntities[x + " " + y + " " + z].strength = 15;
                    nl.blockEntities[x + " " + y + " " + z].type = direction;
                    nl.SimulateSetBlock(x, y, z);
                }
            };
        }
        public static NasBlockAction WireAction(ushort[] actSet, ushort[] inactSet, int direction, ushort hereBlock)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                int type = 0;
                if (IsPartOfSet(actSet, hereBlock) != -1)
                {
                    type = IsPartOfSet(actSet, hereBlock);
                }
                else
                {
                    type = IsPartOfSet(inactSet, hereBlock);
                }
                if (actSet == fixedWireSetActive)
                {
                    type += 11;
                }
                if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                {
                    nl.blockEntities.Add(x + " " + y + " " + z, new());
                    nl.blockEntities[x + " " + y + " " + z].strength = 0;
                    nl.blockEntities[x + " " + y + " " + z].type = type;
                }
                Entity b = nl.blockEntities[x + " " + y + " " + z],
                strength1 = new(),
                strength2 = new();
                int strength0 = b.strength;
                if (direction == 0)
                {
                    if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                    {
                        Entity bEntity = nl.blockEntities[x + 1 + " " + y + " " + z];
                        int checkType = bEntity.type;
                        if (checkType < 5 || checkType == 10 || checkType == 11)
                        {
                            strength1 = bEntity;
                        }
                    }
                    if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                    {
                        Entity bEntity = nl.blockEntities[x - 1 + " " + y + " " + z];
                        int checkType = bEntity.type;
                        if (checkType < 5 || checkType == 8 || checkType == 11)
                        {
                            strength2 = bEntity;
                        }
                    }
                }
                if (direction == 1)
                {
                    if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                    {
                        Entity bEntity = nl.blockEntities[x + " " + (y + 1) + " " + z];
                        int checkType = bEntity.type;
                        if (checkType <= 5 || checkType == 12)
                        {
                            strength1 = bEntity;
                        }
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                    {
                        Entity bEntity = nl.blockEntities[x + " " + (y - 1) + " " + z];
                        int checkType = bEntity.type;
                        if (checkType < 5 || checkType == 6 || checkType == 12)
                        {
                            strength2 = bEntity;
                        }
                    }
                }
                if (direction == 2)
                {
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                    {
                        Entity bEntity = nl.blockEntities[x + " " + y + " " + (z + 1)];
                        int checkType = bEntity.type;
                        if (checkType < 5 || checkType == 7 || checkType == 13)
                        {
                            strength1 = bEntity;
                        }
                    }
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                    {
                        Entity bEntity = nl.blockEntities[x + " " + y + " " + (z - 1)];
                        int checkType = bEntity.type;
                        if (checkType < 5 || checkType == 9 || checkType == 13)
                        {
                            strength2 = bEntity;
                        }
                    }
                }
                if (strength1.strength >= strength2.strength)
                {
                    b.strength = strength1.strength - 1;
                }
                else
                {
                    b.strength = strength2.strength - 1;
                }
                if (b.strength <= 0)
                {
                    b.strength = 0;
                    b.direction = 0;
                    if (IsPartOfSet(actSet, hereBlock) != -1)
                    {
                        nl.FastSetBlock(x, y, z, inactSet[IsPartOfSet(actSet, hereBlock)]);
                    }
                    else
                    {
                        if (strength0 != b.strength)
                        {
                            nl.DisturbBlocks(x, y, z);
                        }
                    }
                }
                else
                {
                    if (IsPartOfSet(actSet, hereBlock) == -1)
                    {
                        nl.FastSetBlock(x, y, z, actSet[IsPartOfSet(inactSet, hereBlock)]);
                    }
                    else
                    {
                        if (strength0 != b.strength)
                        {
                            nl.DisturbBlocks(x, y, z);
                        }
                    }
                }
            };
        }
        public static NasBlockAction PressurePlateAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                bool stoodOn = false;
                Player[] players = PlayerInfo.Online.Items;
                for (int i = 0; i < players.Length; i++)
                {
                    Player who = players[i];
                    if ((who.Pos.FeetBlockCoords == new Vec3S32(x, y, z) || who.Pos.FeetBlockCoords == new Vec3S32(x, y + 1, z)) && who.Level == nl.lvl)
                    {
                        stoodOn = true;
                    }
                }
                if (!stoodOn)
                {
                    nl.SetBlock(x, y, z, Nas.FromRaw(610));
                    nl.blockEntities[x + " " + y + " " + z].strength = 0;
                }
                else
                {
                    nl.SimulateSetBlock(x, y, z);
                }
            };
        }
        public static NasBlockAction RepeaterAction(int direction, ushort hereBlock)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                int type = 0;
                if (IsPartOfSet(repeaterSetActive, hereBlock) != -1)
                {
                    type = 1;
                }
                else
                {
                    type = 0;
                }
                if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                {
                    nl.blockEntities.Add(x + " " + y + " " + z, new());
                    nl.blockEntities[x + " " + y + " " + z].strength = 0;
                    nl.blockEntities[x + " " + y + " " + z].type = direction;
                }
                Entity b = nl.blockEntities[x + " " + y + " " + z],
                strength1 = new();
                if (direction == 5)
                {
                    if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z))
                    {
                        strength1 = nl.blockEntities[x + " " + (y + 1) + " " + z];
                    }
                }
                if (direction == 6)
                {
                    if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                    {
                        strength1 = nl.blockEntities[x + " " + (y - 1) + " " + z];
                    }
                }
                if (direction == 7)
                {
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                    {
                        strength1 = nl.blockEntities[x + " " + y + " " + (z + 1)];
                    }
                }
                if (direction == 8)
                {
                    if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                    {
                        strength1 = nl.blockEntities[x - 1 + " " + y + " " + z];
                    }
                }
                if (direction == 9)
                {
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                    {
                        strength1 = nl.blockEntities[x + " " + y + " " + (z - 1)];
                    }
                }
                if (direction == 10)
                {
                    if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                    {
                        strength1 = nl.blockEntities[x + 1 + " " + y + " " + z];
                    }
                }
                NasLevel.QueuedBlockUpdate qb = new()
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
        }
        public static NasBlockAction ContRepeaterTask(int type, Entity strength1, int direction)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                Entity b = nl.blockEntities[x + " " + y + " " + z];
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
        }
        public static NasBlockAction TurnOffAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                {
                    return;
                }
                nl.SetBlock(x, y, z, Nas.FromRaw(195));
                nl.blockEntities[x + " " + y + " " + z].strength = 0;
            };
        }
        public static NasBlockAction DispenserAction(int changeX, int changeY, int changeZ)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                Entity[] b = new Entity[6];
                if (nl.blockEntities.ContainsKey(x + " " + (y + 1) + " " + z) && (changeY != 1))
                {
                    b[0] = nl.blockEntities[x + " " + (y + 1) + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z) && (changeY != -1))
                {
                    b[1] = nl.blockEntities[x + " " + (y - 1) + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z) && (changeX != 1))
                {
                    b[2] = nl.blockEntities[x + 1 + " " + y + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z) && (changeX != -1))
                {
                    b[3] = nl.blockEntities[x - 1 + " " + y + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)) && (changeZ != 1))
                {
                    b[4] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)) && (changeZ != -1))
                {
                    b[5] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                }
                bool powered =
                    ((b[0] != null) && b[0].strength > 0 && (b[0].type == 1 || b[0].type == 4 || b[0].type == 5 || b[0].type == 12)) ||
                    ((b[1] != null) && b[1].strength > 0 && (b[1].type == 1 || b[1].type == 4 || b[1].type == 6 || b[1].type == 12)) ||
                    ((b[2] != null) && b[2].strength > 0 && (b[2].type == 0 || b[2].type == 4 || b[2].type == 10 || b[2].type == 11)) ||
                    ((b[3] != null) && b[3].strength > 0 && (b[3].type == 0 || b[3].type == 4 || b[3].type == 8 || b[3].type == 11)) ||
                    ((b[4] != null) && b[4].strength > 0 && (b[4].type == 2 || b[4].type == 4 || b[4].type == 7 || b[4].type == 13)) ||
                    ((b[5] != null) && b[5].strength > 0 && (b[5].type == 2 || b[5].type == 4 || b[5].type == 9 || b[5].type == 13));
                if (!powered)
                {
                    if (nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                    {
                        nl.blockEntities[x + " " + y + " " + z].type = 0;
                        return;
                    }
                }
                if (!nl.blockEntities.ContainsKey(x + " " + y + " " + z) || nl.blockEntities[x + " " + y + " " + z].type == 1)
                {
                    return;
                }
                nl.blockEntities[x + " " + y + " " + z].type = 1;
                {
                    ushort checkBlock = nl.GetBlock(x + changeX, y + changeY, z + changeZ);
                    if (!CanPhysicsKillThis(checkBlock) && IsPartOfSet(waterSet, checkBlock) == -1 && IsPartOfSet(lavaSet, checkBlock) == -1)
                    {
                        return;
                    }
                    Entity bEntity = nl.blockEntities[x + " " + y + " " + z];
                    if (bEntity.drop == null || bEntity.drop.blockStacks == null)
                    {
                        return;
                    }
                    BlockStack bs = bEntity.drop.blockStacks[bEntity.drop.blockStacks.Count - 1];
                    if (bs.ID == 7)
                    {
                        return;
                    }
                    ushort clientushort = bs.ID,
                    addedushort = 0;
                    if (clientushort == 643)
                    {
                        clientushort = 9;
                        addedushort = 143;
                    }
                    else
                    {
                        if (clientushort == 696)
                        {
                            clientushort = 10;
                            addedushort = 697;
                        }
                        else
                        {
                            if (clientushort == 143 && IsPartOfSet(waterSet, checkBlock) != -1)
                            {
                                clientushort = 0;
                                addedushort = 643;
                            }
                            else
                            {
                                if (clientushort == 697 && checkBlock == 10)
                                {
                                    clientushort = 0;
                                    addedushort = 696;
                                }
                            }
                        }
                    }
                    bs.amount -= 1;
                    if (bs.amount == 0)
                    {
                        bEntity.drop.blockStacks.Remove(bs);
                    }
                    if (bEntity.drop.blockStacks.Count == 0)
                    {
                        bEntity.drop = null;
                    }
                    if (addedushort == 0)
                    {
                        nl.SetBlock(x + changeX, y + changeY, z + changeZ, Nas.FromRaw(clientushort));
                        if (Get(bs.ID).container != null)
                        {
                            nl.blockEntities.Add(x + changeX + " " + (y + changeY) + " " + (z + changeZ), new());
                        }
                        return;
                    }
                    if (bEntity.drop == null)
                    {
                        nl.SetBlock(x + changeX, y + changeY, z + changeZ, Nas.FromRaw(clientushort));
                        bEntity.drop = new(addedushort);
                        return;
                    }
                    foreach (BlockStack stack in bEntity.drop.blockStacks)
                    {
                        if (stack.ID == addedushort)
                        {
                            if (addedushort != 0)
                            {
                                stack.amount += 1;
                            }
                            nl.SetBlock(x + changeX, y + changeY, z + changeZ, Nas.FromRaw(clientushort));
                            if (Get(bs.ID).container != null)
                            {
                                nl.blockEntities.Add(x + changeX + " " + (y + changeY) + " " + (z + changeZ), new());
                            }
                            return;
                        }
                    }
                    if (bEntity.drop.blockStacks.Count >= Container.BlockStackLimit)
                    {
                        return;
                    }
                    if (addedushort != 0)
                    {
                        bEntity.drop.blockStacks.Add(new(addedushort));
                    }
                    nl.SetBlock(x + changeX, y + changeY, z + changeZ, Nas.FromRaw(clientushort));
                    if (Get(bs.ID).container != null)
                    {
                        nl.blockEntities.Add(x + changeX + " " + (y + changeY) + " " + (z + changeZ), new());
                    }
                }
            };
        }
        public static NasBlockAction SpongeAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                bool absorbed = false;
                for (int xOff = -3; xOff <= 3; xOff++)
                {
                    for (int yOff = -3; yOff <= 3; yOff++)
                    {
                        for (int zOff = -3; zOff <= 3; zOff++)
                        {
                            if (IsPartOfSet(waterSet, nl.GetBlock(x + xOff, y + yOff, z + zOff)) != -1)
                            {
                                nl.SetBlock(x + xOff, y + yOff, z + zOff, 0);
                                absorbed = true;
                            }
                        }
                    }
                }
                if (absorbed)
                {
                    nl.SetBlock(x, y, z, Nas.FromRaw(428));
                }
            };
        }
        public static NasBlockAction NeedsSupportAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                IsSupported(nl, x, y, z);
            };
        }
        public static NasBlockAction GenericPlantAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                GenericPlantSurvived(nl, x, y, z);
            };
        }
        public static NasBlockAction OakSaplingAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (!GenericPlantSurvived(nl, x, y, z))
                {
                    return;
                }
                nl.SetBlock(x, y, z, 0);
                NasTree.GenOakTree(nl, r, x, y, z, true);
            };
        }
        public static NasBlockAction BirchSaplingAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (!GenericPlantSurvived(nl, x, y, z))
                {
                    return;
                }
                nl.SetBlock(x, y, z, 0);
                NasTree.GenBirchTree(nl, r, x, y, z, true);
            };
        }
        public static NasBlockAction SwampSaplingAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (!GenericPlantSurvived(nl, x, y, z))
                {
                    return;
                }
                nl.SetBlock(x, y, z, 0);
                NasTree.GenSwampTree(nl, r, x, y, z, true);
            };
        }
        public static NasBlockAction SpruceSaplingAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (!GenericPlantSurvived(nl, x, y, z))
                {
                    return;
                }
                nl.SetBlock(x, y, z, 0);
                NasTree.GenSpruceTree(nl, r, x, y, z, true);
            };
        }
        public static NasBlockAction CropAction(ushort[] cropSet, int index)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (!CropSurvived(nl, x, y, z))
                {
                    return;
                }
                if (index + 1 >= cropSet.Length)
                {
                    return;
                }
                nl.SetBlock(x, y, z, cropSet[index + 1]);
            };
        }
        public static NasBlockAction IronCropAction(ushort[] cropSet, int index)
        {
            return (nl, nasBlock, x, y, z) =>
            {
                if (!IronCropSurvived(nl, x, y, z))
                {
                    return;
                }
                if (index + 1 >= cropSet.Length)
                {
                    return;
                }
                nl.SetBlock(x, y, z, cropSet[index + 1]);
            };
        }
        public static NasBlockAction AutoCraftingAction()
        {
            return (nl, nasBlock, x, y, z) =>
            {
                Entity[] b = new Entity[5];
                if (nl.blockEntities.ContainsKey(x + " " + (y - 1) + " " + z))
                {
                    b[0] = nl.blockEntities[x + " " + (y - 1) + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + 1 + " " + y + " " + z))
                {
                    b[1] = nl.blockEntities[x + 1 + " " + y + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x - 1 + " " + y + " " + z))
                {
                    b[2] = nl.blockEntities[x - 1 + " " + y + " " + z];
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z + 1)))
                {
                    b[3] = nl.blockEntities[x + " " + y + " " + (z + 1)];
                }
                if (nl.blockEntities.ContainsKey(x + " " + y + " " + (z - 1)))
                {
                    b[4] = nl.blockEntities[x + " " + y + " " + (z - 1)];
                }
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
                {
                    return;
                }
                nl.blockEntities[x + " " + y + " " + z].type = 1;
                Crafting.Recipe recipe = Crafting.GetRecipe(nl, (ushort)x, (ushort)y, (ushort)z, nasBlock.station);
                if (recipe == null)
                {
                    return;
                }
                Drop dropClone = new(recipe.drop);
                Crafting.ClearCraftingArea(nl, (ushort)x, (ushort)y, (ushort)z, nasBlock.station.ori);
                Entity bEntity = nl.blockEntities[x + " " + y + " " + z];
                if (bEntity.drop == null)
                {
                    bEntity.drop = dropClone;
                    return;
                }
                if (dropClone.items != null)
                {
                    foreach (Item tool in bEntity.drop.items)
                    {
                        bEntity.drop.items.Add(tool);
                    }
                }
                if (dropClone.blockStacks != null)
                {
                    bool exists = false;
                    foreach (BlockStack stack in dropClone.blockStacks)
                    {
                        exists = false;
                        foreach (BlockStack otherStack in bEntity.drop.blockStacks)
                        {
                            if (stack.ID == otherStack.ID)
                            {
                                otherStack.amount += stack.amount;
                                exists = true;
                            }
                        }
                        if (!exists)
                        {
                            bEntity.drop.blockStacks.Add(new(stack.ID, stack.amount));
                        }
                    }
                }
            };
        }
        public static bool IsSupported(NasLevel nl, int x, int y, int z)
        {
            ushort below = nl.GetBlock(x, y - 1, z);
            if (CanPhysicsKillThis(below))
            {
                nl.SetBlock(x, y, z, 0);
                return false;
            }
            return true;
        }
        public static bool GenericPlantSurvived(NasLevel nl, int x, int y, int z)
        {
            if (!IsSupported(nl, x, y, z))
            {
                return false;
            }
            if (!CanPlantsLiveOn(nl.GetBlock(x, y - 1, z)))
            {
                nl.SetBlock(x, y, z, 39);
                return false;
            }
            return true;
        }
        public static bool CropSurvived(NasLevel nl, int x, int y, int z)
        {
            if (!IsSupported(nl, x, y, z))
            {
                return false;
            }
            if (nl.biome < 0)
            {
                return false;
            }
            if (IsPartOfSet(soilForPlants, nl.GetBlock(x, y - 1, z)) == -1)
            {
                nl.SetBlock(x, y, z, 39);
                return false;
            }
            return true;
        }
        public static bool IronCropSurvived(NasLevel nl, int x, int y, int z)
        {
            if (!IsSupported(nl, x, y, z))
            {
                return false;
            }
            if (nl.biome >= 0)
            {
                return false;
            }
            if (IsPartOfSet(soilForIron, nl.GetBlock(x, y - 1, z)) == -1 || IsPartOfSet(lavaSet, nl.GetBlock(x, y - 2, z)) == -1)
            {
                nl.SetBlock(x, y, z, 39);
                return false;
            }
            return true;
        }
        public static bool CanPlantsLiveOn(ushort block)
        {
            if (IsPartOfSet(soilForPlants, block) != -1 || IsPartOfSet(grassSet, block) != -1)
            {
                return true;
            }
            return false;
        }
    }
}
#endif