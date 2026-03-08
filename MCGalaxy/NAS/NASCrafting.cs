using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.Util.Imaging;
using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    public enum NASOrientation 
    {
        None, WE, NS 
    }
    public enum NASStationType 
    { 
        None, Normal, Furnace, Anvil 
    }
    public class NASRecipe
    {
        public int expGiven = 0;
        public string name;
        public ushort[,] pattern;
        public NASStationType stationType = NASStationType.Normal;
        public bool usesParentID = false, usesAlternateID = false,
            shapeless = false;
        public NASDrop drop;
        public NASRecipe(NASItem item)
        {
            name = item.name;
            drop = new(item);
            NASCrafting.recipes.Add(this);
        }
        public NASRecipe(ushort blockID, int amount)
        {
            name = blockID.ToString();
            drop = new(blockID, amount);
            NASCrafting.recipes.Add(this);
        }
        public Dictionary<ushort, int> PatternCost
        {
            get
            {
                Dictionary<ushort, int> patternCost = new();
                for (int x = 0; x < pattern.GetLength(1); x++)
                    for (int y = 0; y < pattern.GetLength(0); y++)
                        FillDict(NASBlock.Get(pattern[y, x]).parentID, ref patternCost);
                return patternCost;
            }
        }
        public static void FillDict(ushort ID, ref Dictionary<ushort, int> stacks)
        {
            if (stacks.ContainsKey(ID))
                stacks[ID]++;
            else
                stacks.Add(ID, 1);
        }
        public bool MatchesShapeless(NASBlock[,] area)
        {
            int patternWidth = pattern.GetLength(1),
                patternHeight = pattern.GetLength(0);
            Dictionary<ushort, int> patternStacks = new(),
                areaStacks = new();
            for (int patternX = 0; patternX < patternWidth; patternX++)
                for (int patternY = 0; patternY < patternHeight; patternY++)
                    FillDict(pattern[patternY, patternX], ref patternStacks);
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                {
                    NASBlock suppliedNB = area[x, y];
                    if (usesAlternateID)
                        suppliedNB = NASBlock.Get(suppliedNB.alternateID);
                    FillDict(usesParentID ? suppliedNB.parentID : suppliedNB.selfID, ref areaStacks);
                }
            bool matches = true;
            foreach (KeyValuePair<ushort, int> pair in patternStacks)
                if (!(areaStacks.ContainsKey(pair.Key) && areaStacks[pair.Key] == pair.Value))
                    matches = false;
            return matches;
        }
        public bool Matches(NASBlock[,] area)
        {
            int patternWidth = pattern.GetLength(1),
                patternHeight = pattern.GetLength(0);
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    if (TestRecipe(area, x, y, true) || TestRecipe(area, x, y, false))
                    {
                        int minX = x, maxX = x + patternWidth,
                            minY = y, maxY = y + patternHeight;
                        for (int _x = 0; _x < 3; _x++)
                            for (int _y = 0; _y < 3; _y++)
                                if ((_x < minX || _x >= maxX || _y < minY || _y >= maxY) && area[_x, _y].selfID != 0)
                                    return false;
                        return true;
                    }
            return false;
        }
        public bool TestRecipe(NASBlock[,] area, int offsetX, int offsetY, bool mirrored)
        {
            int patternWidth = pattern.GetLength(1),
                patternHeight = pattern.GetLength(0);
            if (offsetX + patternWidth > 3 || offsetY + patternHeight > 3)
                return false;
            for (int x = 0; x < patternWidth; x++)
                for (int y = 0; y < patternHeight; y++)
                {
                    int xPattern = mirrored ? patternWidth - 1 - x : x;
                    NASBlock suppliedNB = area[x + offsetX, y + offsetY];
                    if (usesAlternateID)
                        suppliedNB = NASBlock.Get(suppliedNB.alternateID);
                    if ((usesParentID ? suppliedNB.parentID : suppliedNB.selfID) != pattern[y, xPattern])
                        return false;
                }
            return true;
        }
    }
    public class NASAreaInfo
    {
        public NASPlayer np;
        public Vec3U16 start, end;
        public int totalRounds, curRound;
        public TimeSpan delay;
        public short R, G, B;
        public byte A, ID;
    }
    public class NASStation
    {
        public string name;
        public NASStationType type = NASStationType.None;
        public NASOrientation ori = NASOrientation.None;
        public NASStation() { }
        public NASStation(NASStation parent)
        {
            name = parent.name;
            type = parent.type;
            ori = parent.ori;
        }
        public void ShowArea(NASPlayer np, ushort x, ushort y, ushort z, Pixel color, int millisecs = 2000, byte A = 127)
        {
            ushort startX = x, startY = y, startZ = z,
                endX = x, endY = y, endZ = z;
            bool WE = ori == NASOrientation.WE;
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
            NASAreaInfo info = new()
            {
                np = np,
                start = new(startX, startY, startZ),
                end = new(endX, endY, endZ),
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
        public static void ShowAreaTask(SchedulerTask task)
        {
            NASAreaInfo info = (NASAreaInfo)task.State;
            if (info.curRound <= 0)
            {
                info.np.Send(Packet.DeleteSelection(info.ID));
                task.Repeating = false;
                return;
            }
            task.Delay = info.delay;
            info.np.Send(Packet.MakeSelection(info.ID, "Crafting Zone", info.start, info.end, info.R, info.G, info.B, (short)(info.A / info.totalRounds * info.curRound), true));
            info.curRound--;
        }
    }
    public partial class NASCrafting
    {
        public static List<NASRecipe> recipes = new();
        public static void ClearCraftingArea(NASLevel nl, ushort startX, ushort startY, ushort startZ, NASOrientation ori)
        {
            bool WE = ori == NASOrientation.WE;
            if (WE)
                startX--;
            else
                startZ--;
            startY += 3;
            if (WE)
                for (ushort y = startY; y > startY - 3; y--)
                    for (ushort x = startX; x < startX + 3; x++)
                    {
                        nl.SetBlock(x, y, startZ, 0);
                        if (nl.blockEntities.ContainsKey(x + " " + y + " " + startZ))
                            nl.blockEntities.Remove(x + " " + y + " " + startZ);
                    }
            else
                for (ushort y = startY; y > startY - 3; y--)
                    for (ushort z = startZ; z < startZ + 3; z++)
                    {
                        nl.SetBlock(startX, y, z, 0);
                        if (nl.blockEntities.ContainsKey(startX + " " + y + " " + z))
                            nl.blockEntities.Remove(startX + " " + y + " " + z);
                    }
        }
        public static NASRecipe GetRecipe(NASLevel nl, ushort x, ushort y, ushort z, NASStation station)
        {
            NASBlock[,] area = GetArea(nl, x, y, z, station.ori);
            foreach (NASRecipe recipe in recipes)
            {
                if (recipe.stationType != station.type)
                    continue;
                if (recipe.shapeless)
                {
                    if (recipe.MatchesShapeless(area))
                        return recipe;
                }
                else if (recipe.Matches(area))
                    return recipe;
            }
            return null;
        }
        public static NASBlock[,] GetArea(NASLevel nl, ushort startX, ushort startY, ushort startZ, NASOrientation ori)
        {
            NASBlock[,] area = new NASBlock[3, 3];
            bool WE = ori == NASOrientation.WE;
            if (WE)
                startX--;
            else
                startZ--;
            startY += 3;
            int indexX = 0, indexY = 0;
            if (WE)
                for (ushort y = startY; y > startY - 3; y--)
                {
                    for (ushort x = startX; x < startX + 3; x++)
                    {
                        ushort blockID = nl.lvl.GetBlock(x, y, startZ);
                        if (blockID == 0xff)
                            blockID = 0;
                        ushort num;
                        switch (blockID)
                        {
                            case >= 256:
                                num = Block.ToRaw(blockID);
                                break;
                            default:
                                num = Block.Convert(blockID);
                                if (num >= 66)
                                    num = 22;
                                break;
                        }
                        NASBlock nb = NASBlock.Get(num);
                        area[indexX, indexY] = nb;
                        indexX++;
                    }
                    indexX = 0;
                    indexY++;
                }
            else
                for (ushort y = startY; y > startY - 3; y--)
                {
                    for (ushort z = startZ; z < startZ + 3; z++)
                    {
                        ushort blockID = nl.lvl.GetBlock(startX, y, z);
                        if (blockID == 0xff)
                            blockID = 0;
                        ushort num;
                        switch (blockID)
                        {
                            case >= 256:
                                num = Block.ToRaw(blockID);
                                break;
                            default:
                                num = Block.Convert(blockID);
                                if (num >= 66)
                                    num = 22;
                                break;
                        }
                        NASBlock nb = NASBlock.Get(num);
                        area[indexX, indexY] = nb;
                        indexX++;
                    }
                    indexX = 0;
                    indexY++;
                }
            return area;
        }
    }
}
