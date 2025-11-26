#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.Drawing;
namespace NotAwesomeSurvival
{
    public partial class Crafting
    {
        public static object locker = new();
        public static void ClearCraftingArea(NasLevel nl, ushort startX, ushort startY, ushort startZ, Station.Orientation ori)
        {
            bool WE = ori == Station.Orientation.WE;
            if (WE)
            {
                startX--;
            }
            else
            {
                startZ--;
            }
            startY += 3;
            if (WE)
            {
                for (ushort y = startY; y > startY - 3; y--)
                {
                    for (ushort x = startX; x < startX + 3; x++)
                    {
                        nl.SetBlock(x, y, startZ, 0);
                        if (nl.blockEntities.ContainsKey(x + " " + y + " " + startZ))
                        {
                            nl.blockEntities.Remove(x + " " + y + " " + startZ);
                        }
                    }
                }
            }
            else
            {
                for (ushort y = startY; y > startY - 3; y--)
                {
                    for (ushort z = startZ; z < startZ + 3; z++)
                    {
                        nl.SetBlock(startX, y, z, 0);
                        if (nl.blockEntities.ContainsKey(startX + " " + y + " " + z))
                        {
                            nl.blockEntities.Remove(startX + " " + y + " " + z);
                        }
                    }
                }
            }
        }
        public class Station
        {
            public string name;
            public enum Type { None, Normal, Furnace, Anvil }
            public Type type = Type.None;
            /// <summary>
            /// WE goes along X axis, NS goes along Z axis
            /// </summary>
            public enum Orientation { None, WE, NS }
            public Orientation ori = Orientation.None;
            public Station() { }
            public Station(Station parent)
            {
                name = parent.name;
                type = parent.type;
                ori = parent.ori;
            }
            public void ShowArea(NasPlayer np, ushort x, ushort y, ushort z, Color color, int millisecs = 2000, byte A = 127)
            {
                ushort startX = x, startY = y, startZ = z,
                    endX = x, endY = y, endZ = z;
                bool WE = ori == Orientation.WE;
                if (WE)
                {
                    endX += 2;
                    startX--;
                    endZ++;
                }
                else
                {
                    endZ += 2;
                    startZ--;
                    endX++;
                }
                startY += 4;
                endY++;
                AreaInfo info = new()
                {
                    np = np,
                    start = new Vec3U16(startX, startY, startZ),
                    end = new Vec3U16(endX, endY, endZ),
                    totalRounds = 16
                };
                info.curRound = info.totalRounds;
                info.delay = TimeSpan.FromMilliseconds(millisecs / info.totalRounds);
                info.R = color.R;
                info.G = color.G;
                info.B = color.B;
                info.A = A;
                info.ID = np.craftingAreaID++;
                SchedulerTask showAreaTask = Server.MainScheduler.QueueRepeat(ShowAreaTask, info, TimeSpan.Zero);
            }
            public class AreaInfo
            {
                public NasPlayer np;
                public Vec3U16 start, end;
                public int totalRounds, curRound;
                public TimeSpan delay;
                public short R, G, B;
                public byte A, ID;
            }
            public static void ShowAreaTask(SchedulerTask task)
            {
                AreaInfo info = (AreaInfo)task.State;
                if (info.curRound <= 0)
                {
                    info.np.Send(Packet.DeleteSelection(info.ID));
                    task.Repeating = false;
                    return;
                }
                task.Delay = info.delay;
                short alpha = (short)(info.A / info.totalRounds * info.curRound);
                info.np.Send(Packet.MakeSelection(info.ID, "Crafting Zone", info.start, info.end, info.R, info.G, info.B, alpha, true));
                info.curRound--;
            }
        }
        public static List<Recipe> recipes = new();
        public static Recipe GetRecipe(NasLevel nl, ushort x, ushort y, ushort z, Station station)
        {
            NasBlock[,] area = GetArea(nl, x, y, z, station.ori);
            foreach (Recipe recipe in recipes)
            {
                if (recipe.stationType != station.type)
                {
                    continue;
                }
                if (recipe.shapeless)
                {
                    if (recipe.MatchesShapeless(area))
                    {
                        return recipe;
                    }
                }
                else if (recipe.Matches(area))
                {
                    return recipe;
                }
            }
            return null;
        }
        public static NasBlock[,] GetArea(NasLevel nl, ushort startX, ushort startY, ushort startZ, Station.Orientation ori)
        {
            NasBlock[,] area = new NasBlock[3, 3];
            bool WE = ori == Station.Orientation.WE;
            if (WE)
            {
                startX--;
            }
            else
            {
                startZ--;
            }
            startY += 3;
            int indexX = 0, indexY = 0;
            if (WE)
            {
                for (ushort y = startY; y > startY - 3; y--)
                {
                    for (ushort x = startX; x < startX + 3; x++)
                    {
                        ushort blockID = nl.lvl.GetBlock(x, y, startZ);
                        if (blockID == 0xff)
                        {
                            blockID = 0;
                        }
                        ushort num;
                        if (blockID >= 256)
                        {
                            num = Nas.ToRaw(blockID);
                        }
                        else
                        {
                            num = Nas.Convert(blockID);
                            if (num >= 66)
                            {
                                num = 22;
                            }
                        }
                        NasBlock nb = NasBlock.Get(num);
                        area[indexX, indexY] = nb;
                        indexX++;
                    }
                    indexX = 0;
                    indexY++;
                }
            }
            else
            {
                for (ushort y = startY; y > startY - 3; y--)
                {
                    for (ushort z = startZ; z < startZ + 3; z++)
                    {
                        ushort blockID = nl.lvl.GetBlock(startX, y, z);
                        if (blockID == 0xff)
                        {
                            blockID = 0;
                        }
                        ushort num;
                        if (blockID >= 256)
                        {
                            num = Nas.ToRaw(blockID);
                        }
                        else
                        {
                            num = Nas.Convert(blockID);
                            if (num >= 66)
                            {
                                num = 22;
                            }
                        }
                        NasBlock nb = NasBlock.Get(num);
                        area[indexX, indexY] = nb;
                        indexX++;
                    }
                    indexX = 0;
                    indexY++;
                }
            }
            return area;
        }
        public class Recipe
        {
            public int expGiven = 0;
            public string name;
            public ushort[,] pattern;
            public Dictionary<ushort, int> PatternCost
            {
                get
                {
                    Dictionary<ushort, int> patternCost = new();
                    for (int x = 0; x < pattern.GetLength(1); x++)
                    {
                        for (int y = 0; y < pattern.GetLength(0); y++)
                        {
                            ushort curPatternID = NasBlock.Get(pattern[y, x]).parentID;
                            FillDict(curPatternID, ref patternCost);
                        }
                    }
                    return patternCost;
                }
            }
            public static void FillDict(ushort ID, ref Dictionary<ushort, int> stacks)
            {
                if (stacks.ContainsKey(ID))
                {
                    stacks[ID]++;
                }
                else
                {
                    stacks.Add(ID, 1);
                }
            }
            public Station.Type stationType = Station.Type.Normal;
            /// <summary>
            /// false to make this recipe strict (e.g. sideways stick doesn't work, only upright)
            /// and true to make this recipe work with any rotation of a block group (e.g. all four monitors work)
            /// </summary>
            public bool usesParentID = false,
                usesAlternateID = false,
                shapeless = false;
            public Drop drop;
            public Recipe()
            {
                recipes.Add(this);
            }
            public Recipe(Item item) : this()
            {
                name = item.name;
                drop = new Drop(item);
            }
            public Recipe(ushort blockID, int amount) : this()
            {
                name = blockID.ToString();
                drop = new Drop(blockID, amount);
            }
            public bool MatchesShapeless(NasBlock[,] area)
            {
                int patternWidth = pattern.GetLength(1),
                    patternHeight = pattern.GetLength(0);
                Dictionary<ushort, int> patternStacks = new(),
                    areaStacks = new();
                for (int patternX = 0; patternX < patternWidth; patternX++)
                {
                    for (int patternY = 0; patternY < patternHeight; patternY++)
                    {
                        ushort required = pattern[patternY, patternX];
                        FillDict(required, ref patternStacks);
                    }
                }
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        NasBlock suppliedNB = area[x, y];
                        if (usesAlternateID)
                        {
                            suppliedNB = NasBlock.Get(suppliedNB.alternateID);
                        }
                        ushort supplied = usesParentID ? suppliedNB.parentID : suppliedNB.selfID;
                        FillDict(supplied, ref areaStacks);
                    }
                }
                bool matches = true;
                foreach (KeyValuePair<ushort, int> pair in patternStacks)
                {
                    if (!(areaStacks.ContainsKey(pair.Key) && areaStacks[pair.Key] == pair.Value))
                    {
                        matches = false;
                    }
                }
                return matches;
            }
            public bool Matches(NasBlock[,] area)
            {
                int patternWidth = pattern.GetLength(1),
                    patternHeight = pattern.GetLength(0);
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        if (TestRecipe(area, x, y, true) || TestRecipe(area, x, y, false))
                        {
                            // Check to make sure there aren't any sneaky unused items in the grid
                            //bounds of the current recipe
                            int minX = x, maxX = x + patternWidth,
                                minY = y, maxY = y + patternHeight;
                            //for the entire crafting grid,
                            for (int _x = 0; _x < 3; _x++)
                            {
                                for (int _y = 0; _y < 3; _y++)
                                {
                                    //if a spot is outside the bounds of the current recipe,
                                    if (_x < minX || _x >= maxX || _y < minY || _y >= maxY)
                                    {
                                        if (area[_x, _y].selfID != 0)
                                        { //and the block at this spot is not air, there was an unused item and the recipe is invalid
                                            return false;
                                        }
                                    }
                                }
                            }
                            return true;
                        }
                    }
                }
                return false;
            }
            public bool TestRecipe(NasBlock[,] area, int offsetX, int offsetY, bool mirrored)
            {
                //the offset describes an offset down-right into the crafting grid
                int patternWidth = pattern.GetLength(1),
                    patternHeight = pattern.GetLength(0);
                //if we're out of bounds on lower-right side, it definitely doesn't match
                if (offsetX + patternWidth > 3 || offsetY + patternHeight > 3)
                {
                    return false;
                }
                for (int x = 0; x < patternWidth; x++)
                {
                    for (int y = 0; y < patternHeight; y++)
                    {
                        int xPattern = mirrored ? patternWidth - 1 - x : x;
                        NasBlock suppliedNB = area[x + offsetX, y + offsetY];
                        if (usesAlternateID)
                        {
                            suppliedNB = NasBlock.Get(suppliedNB.alternateID);
                        }
                        ushort supplied = usesParentID ? suppliedNB.parentID : suppliedNB.selfID,
                            required = pattern[y, xPattern];
                        if (supplied != required)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        } //Recipe
    }
}
#endif