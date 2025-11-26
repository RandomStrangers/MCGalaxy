#if NAS && TEN_BIT_BLOCKS
using LibNoise;
using MCGalaxy;
using MCGalaxy.Generator;
using MCGalaxy.Tasks;
using System;
using System.Drawing;
using System.IO;
namespace NotAwesomeSurvival
{
    public static class NasGen
    {
        public const int mapWideness = 384,
            mapTallness = 256;
        public const string seed = "a";
        public const ushort oceanHeight = 60,
            coalDepth = 4,
            ironDepth = 16,
            goldDepth = 30,
            diamondDepth = 45,
            emeraldDepth = 60;
        public const float coalChance = 1f / 8f,
            ironChance = 1f / 16f,
            goldChance = 1f / 24f,
            quartzChance = 1f / 24f,
            diamondChance = 1.25f / 48f,
            emeraldChance = 1.25f / 40f;
        public static Color coalFogColor,
            ironFogColor,
            goldFogColor,
            diamondFogColor,
            emeraldFogColor;
        public static Scheduler genScheduler;
        public static ushort[] stoneTypes =
        {
            52,
            1,
            48
        };
        public static bool currentlyGenerating = false;
        public static void Log(string format, params object[] args)
        {
            Logger.Log(LogType.Debug, string.Format(format, args));
        }
        public static void Setup()
        {
            genScheduler ??= new Scheduler("MapGenScheduler");
            MapGen.Register("NASGen", GenType.Advanced, Gen, "hello?");
            coalFogColor = ColorTranslator.FromHtml("#BCC9E8");
            ironFogColor = ColorTranslator.FromHtml("#A1A3A8");
            goldFogColor = ColorTranslator.FromHtml("#7A706A");
            diamondFogColor = ColorTranslator.FromHtml("#605854");
            emeraldFogColor = ColorTranslator.FromHtml("#605854");
        }
        public static void TakeDown()
        {
            MapGen gen = MapGen.Find("NASGen");
            if (gen != null)
            {
                MapGen.Generators.Remove(gen);
            }
        }
        public static int MakeInt(string seed)
        {
            if (seed.Length == 0)
            {
                return new Random().Next();
            }
            if (!int.TryParse(seed, out int value))
            {
                value = seed.GetHashCode();
            }
            return value;
        }
        /// <summary>
        /// Returns true if seed and offsets were succesfully found
        /// </summary>
        public static bool GetSeedAndChunkOffset(string mapName, ref string seed, ref int chunkOffsetX, ref int chunkOffsetZ)
        {
            string[] bits = mapName.Split('_');
            if (bits.Length <= 1)
            {
                return false;
            }
            seed = bits[0];
            string[] chunks = bits[1].Split(',');
            if (chunks.Length <= 1)
            {
                return false;
            }
            if (!int.TryParse(chunks[0], out chunkOffsetX))
            {
                return false;
            }
            if (!int.TryParse(chunks[1], out chunkOffsetZ))
            {
                return false;
            }
            return true;
        }
        public static bool Gen(Player p, Level lvl, MapGenArgs args)
        {
            return Gen(p, lvl, args.Seed.ToString());
        }
        public static bool Gen(Player p, Level lvl, string seed)
        {
            if (File.Exists("levels/" + lvl.name + ".lvl"))
            {
                p.Message("Something weird happened, try going into the map again");
                return false;
            }
            currentlyGenerating = true;
            int offsetX, offsetZ,
                chunkOffsetX = 0, chunkOffsetZ = 0;
            GetSeedAndChunkOffset(lvl.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ);
            offsetX = chunkOffsetX * mapWideness;
            offsetZ = chunkOffsetZ * mapWideness;
            offsetX -= chunkOffsetX;
            offsetZ -= chunkOffsetZ;
            Perlin adjNoise = new()
            {
                Seed = MakeInt(seed)
            };
            Random r = new(adjNoise.Seed);
            DateTime dateStart = DateTime.UtcNow;
            GenInstance instance = new()
            {
                p = p,
                lvl = lvl,
                adjNoise = adjNoise,
                offsetX = offsetX,
                offsetZ = offsetZ,
                r = r,
                seed = seed,
                biome = new Random().Next(0, 7)
            };
            if (lvl.name.CaselessContains("nether"))
            {
                instance.biome = -1;
            }
            if (lvl.name.CaselessContains("test"))
            {
                instance.biome = 0;
            }
            instance.Do();
            lvl.Config.Deletable = false;
            lvl.Config.MOTD = "-hax +thirdperson maxspeed=1.5";
            lvl.Config.GrassGrow = false;
            TimeSpan timeTaken = DateTime.UtcNow.Subtract(dateStart);
            p.Message("Done in {0}", timeTaken.Shorten(true, true));
            currentlyGenerating = false;
            return true;
        }
        public class GenInstance
        {
            public Player p;
            public Level lvl;
            public NasLevel nl;
            public Perlin adjNoise;
            public float[,] temps;
            public int offsetX, offsetZ, biome;
            public Random r;
            public string seed;
            public ushort topSoil, soil;
            public void Do()
            {
                p.Message("Generating with biome " + biome);
                CalcTemps();
                GenTerrain();
                CalcHeightmap();
                if (biome >= 0)
                {
                    GenSoil();
                }
                GenCaves();
                if (biome < 0)
                {
                    GenRandom();
                }
                GenPlants();
                GenOre();
                GenWaterSources();
                if (biome >= 0)
                {
                    GenDungeons();
                }
                nl.dungeons = true;
                NasLevel.Unload(lvl.name, nl);
            }
            public void CalcTemps()
            {
                adjNoise.OctaveCount = biome == -1 ? 7 : 2;
                if (biome == 2)
                {
                    lvl.Config.Weather = 2;
                }
                if (biome < 0)
                {
                    lvl.Config.EdgeLevel = 30;
                    lvl.Config.HorizonBlock = 10;
                    lvl.Config.CloudsHeight = 300;
                }
                lvl.SaveSettings();
                if (biome < 0)
                {
                    return;
                }
                p.Message("Calculating temperatures");
                temps = new float[lvl.Width, lvl.Length];
                for (double z = 0; z < lvl.Length; ++z)
                {
                    for (double x = 0; x < lvl.Width; ++x)
                    {
                        //divide by more for bigger scale
                        double scale = 150,
                            xVal = (x + offsetX) / scale,
                            zVal = (z + offsetZ) / scale;
                        xVal += 1;
                        zVal += 1;
                        float val = (float)adjNoise.GetValue(xVal, 0, zVal);
                        val += 0.1f;
                        val /= 2;
                        temps[(int)x, (int)z] = val;
                    }
                }
            }
            public void GenTerrain()
            {
                p.Message("Generating terrain");
                //more frequency = smaller map scale
                adjNoise.Frequency = 0.75;
                adjNoise.OctaveCount = 5;
                DateTime dateStartLayer;
                double width = lvl.Width, height = lvl.Height, length = lvl.Length;
                //counter = 0;
                dateStartLayer = DateTime.UtcNow;
                for (double y = 0; y < height; y++)
                {
                    for (double z = 0; z < length; ++z)
                    {
                        for (double x = 0; x < width; ++x)
                        {
                            if (y == 0 || (y == height - 1 && biome < 0))
                            {
                                lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 7);
                                continue;
                            }
                            if (y >= height - 4 && r.Next(2) == 0 && biome < 0)
                            {
                                lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 7);
                                continue;
                            }
                            double threshDiv = 0;
                            if (biome >= 0)
                            {
                                threshDiv = temps[(int)x, (int)z];
                                threshDiv *= 1.5;
                                if (threshDiv <= 0)
                                {
                                    threshDiv = 0;
                                }
                                if (threshDiv > 1)
                                {
                                    threshDiv = 1;
                                }
                            }
                            double averageLandHeightAboveSeaLevel = biome == -1 ? 10 : 1,/* - (6*tallRandom);*/
                                minimumFlatness = biome == -1 ? 0 : 5,
                                maxFlatnessAdded = biome == -1 ? 80 : 28,
                            /*multiply by more to more strictly follow halfway under = solid, above = air*/
                                threshold =
                                (((y + (oceanHeight - averageLandHeightAboveSeaLevel)) / height) - 0.5)
                                * (minimumFlatness + (maxFlatnessAdded * threshDiv)); //4.5f
                            if (threshold < -1.5)
                            {
                                if (biome == 1)
                                {
                                    lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 52);
                                }
                                else
                                {
                                    if (biome < 0)
                                    {
                                        lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 48);
                                    }
                                    else
                                    {
                                        lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 1);
                                    }
                                    continue;
                                }
                            }
                            if (threshold > 1.5)
                            {
                                continue;
                            }
                            //divide y by less for more "layers"
                            double xVal = (x + offsetX) / 200, yVal = y / (250 + (biome == -1 ? 40 : 150 * threshDiv)), zVal = (z + offsetZ) / 200;
                            xVal *= 2;
                            yVal *= 2;
                            zVal *= 2;
                            xVal += 1;
                            yVal += 1;
                            zVal += 1;
                            double value = adjNoise.GetValue(xVal, yVal, zVal);
                            if (value > threshold || (biome == 6 && y < oceanHeight - 10))
                            {
                                if (biome == 1)
                                {
                                    lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 52);
                                }
                                else
                                {
                                    if (biome < 0)
                                    {
                                        lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 48);
                                    }
                                    else
                                    {
                                        lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 1);
                                    }
                                }
                            }
                            else if (y < oceanHeight)
                            {
                                if (biome == 1)
                                {
                                    lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 12);
                                }
                                else
                                {
                                    if (y == (oceanHeight - 1) && biome == 2)
                                    {
                                        lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 60);
                                    }
                                    else
                                    {
                                        if (biome < 0 && y < oceanHeight / 2)
                                        {
                                            lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 10);
                                        }
                                        else if (biome >= 0)
                                        {
                                            lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 8);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    TimeSpan span = DateTime.UtcNow.Subtract(dateStartLayer);
                    if (span > TimeSpan.FromSeconds(5))
                    {
                        if (p != Player.Console)
                        {
                            Log("Initial gen {0}% complete.", (int)(y / height * 100));
                        }
                        p.Message("Initial gen {0}% complete.", (int)(y / height * 100));
                        dateStartLayer = DateTime.UtcNow;
                    }
                }
                p.Message("Initial gen 100% complete.");
            }
            public void CalcHeightmap()
            {
                p.Message("Calculating heightmap");
                nl = new NasLevel
                {
                    heightmap = new ushort[lvl.Width, lvl.Length],
                    height = lvl.Height
                };
                for (ushort z = 0; z < lvl.Length; ++z)
                {
                    for (ushort x = 0; x < lvl.Width; ++x)
                    {
                        //         skip bedrock
                        for (ushort y = 1; y < lvl.Height; ++y)
                        {
                            ushort curBlock = lvl.FastGetBlock(x, y, z);
                            if (NasBlock.IsPartOfSet(stoneTypes, curBlock) == -1)
                            {
                                nl.heightmap[x, z] = (ushort)(y - 1);
                                break;
                            }
                        }
                    }
                }
                nl.lvl = lvl;
                nl.biome = biome;
            }
            public void GenSoil()
            {
                int width = lvl.Width, height = lvl.Height, length = lvl.Length;
                p.Message("Now creating soil.");
                adjNoise.Seed = MakeInt(seed + "soil");
                adjNoise.Frequency = 1;
                adjNoise.OctaveCount = 6;
                DateTime dateStartLayer;
                //counter = 0;
                dateStartLayer = DateTime.UtcNow;
                for (int y = 0; y < height - 1; y++)
                {
                    for (int z = 0; z < length; ++z)
                    {
                        for (int x = 0; x < width; ++x)
                        {
                            if (biome == 1)
                            {
                                soil = 12;
                            }
                            else
                            {
                                soil = 3;
                            }
                            if (NasBlock.IsPartOfSet(stoneTypes, lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z)) != -1 && (
                                NasBlock.IsPartOfSet(stoneTypes, lvl.FastGetBlock((ushort)x, (ushort)(y + 1), (ushort)z)) == -1)
                                        && ShouldThereBeSoil(x, y, z))
                            {
                                soil = GetSoilType();
                                if (y <= oceanHeight - 12 && biome != 6)
                                {
                                    soil = 13;
                                }
                                else if (y <= oceanHeight && biome != 6)
                                {
                                    soil = 12;
                                }
                                int startY = y;
                                for (int yCol = startY; yCol > startY - 2 - r.Next(0, 2); yCol--)
                                {
                                    if (yCol < 0)
                                    {
                                        break;
                                    }
                                    if (lvl.FastGetBlock((ushort)x, (ushort)yCol, (ushort)z) == 1 || lvl.FastGetBlock((ushort)x, (ushort)yCol, (ushort)z) == 52)
                                    {
                                        lvl.SetBlock((ushort)x, (ushort)yCol, (ushort)z, soil);
                                    }
                                }
                            }
                        }
                    }
                    TimeSpan span = DateTime.UtcNow.Subtract(dateStartLayer);
                    if (span > TimeSpan.FromSeconds(5))
                    {
                        if (p != Player.Console)
                        {
                            Log("Soil gen {0}% complete.", y / height * 100);
                        }
                        p.Message("Soil gen {0}% complete.", y / height * 100);
                        dateStartLayer = DateTime.UtcNow;
                    }
                }
            }
            public bool ShouldThereBeSoil(int x, int y, int z)
            {
                if (
                    IsNeighborLowEnough(x, y, z, -1, 0) ||
                    IsNeighborLowEnough(x, y, z, 1, 0) ||
                    IsNeighborLowEnough(x, y, z, 0, -1) ||
                    IsNeighborLowEnough(x, y, z, 0, 1))
                {
                    return false;
                }
                return true;
            }
            public bool IsNeighborLowEnough(int x, int y, int z, int offX, int offZ)
            {
                int neighborX = x + offX,
                    neighborZ = z + offZ;
                if (neighborX >= lvl.Width || neighborX < 0 ||
                    neighborZ >= lvl.Length || neighborZ < 0)
                {
                    return false;
                }
                for (int i = 0; i < 4; i++)
                {
                    if (!lvl.IsAirAt((ushort)neighborX, (ushort)(y - i), (ushort)neighborZ))
                    {
                        return false;
                    }
                }
                return true;
            }
            public void GenCaves()
            {
                int width = lvl.Width, height = lvl.Height, length = lvl.Length;
                p.Message("Now creating caves");
                adjNoise.Seed = MakeInt(seed + "cave");
                adjNoise.Frequency = 1; //more frequency = smaller map scale
                adjNoise.OctaveCount = 2;
                int counter = 0;
                DateTime dateStartLayer = DateTime.UtcNow;
                for (double y = 0; y < height; y++)
                {
                    for (double z = 0; z < length; ++z)
                    {
                        for (double x = 0; x < width; ++x)
                        {
                            double threshold = 0.55;
                            int caveHeight = biome < 0 ? height : lvl.Height - 7;
                            if (y > caveHeight)
                            {
                                threshold += 0.05 * (y - caveHeight);
                            }
                            if (threshold > 1.5)
                            {
                                continue;
                            }
                            bool tryCave = false;
                            ushort thisBlock = lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z);
                            if (thisBlock == 1 || thisBlock == 3 || thisBlock == 52 || thisBlock == 48)
                            {
                                tryCave = true;
                            }
                            if (!tryCave)
                            {
                                continue;
                            }
                            //divide y by less for more "layers"
                            double xVal = (x + offsetX) / 15, yVal = y / 7, zVal = (z + offsetZ) / 15;
                            xVal += 1;
                            yVal += 1;
                            zVal += 1;
                            double value = adjNoise.GetValue(xVal, yVal, zVal);
                            counter++;
                            if (value > threshold)
                            {
                                if (y <= 4)
                                {
                                    lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 10);
                                }
                                else
                                {
                                    lvl.SetTile((ushort)x, (ushort)y, (ushort)z, 0);
                                }
                            }
                        }
                    }
                    TimeSpan span = DateTime.UtcNow.Subtract(dateStartLayer);
                    if (span > TimeSpan.FromSeconds(5))
                    {
                        if (p != Player.Console)
                        {
                            Log("Cave gen {0}% complete.", (int)(y / height * 100));
                        }
                        p.Message("Cave gen {0}% complete.", (int)(y / height * 100));
                        dateStartLayer = DateTime.UtcNow;
                    }
                }
                p.Message("Cave gen 100% complete.");
            }
            public void GenRandom()
            {
                int width = lvl.Width, height = lvl.Height, length = lvl.Length;
                p.Message("Now creating random patches");
                adjNoise.Seed = MakeInt(seed + "random");
                adjNoise.Frequency = 1; //more frequency = smaller map scale
                adjNoise.OctaveCount = 2;
                adjNoise.Persistence = 0.25;
                int counter = 0;
                DateTime dateStartLayer = DateTime.UtcNow;
                for (double y = 0; y < height; y++)
                {
                    for (double z = 0; z < length; ++z)
                    {
                        for (double x = 0; x < width; ++x)
                        {
                            double threshold = 0.7;
                            bool tryPlace = false;
                            ushort thisBlock = lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z);
                            if (thisBlock == 1 || thisBlock == 3 || thisBlock == 52 || thisBlock == 48)
                            {
                                tryPlace = true;
                            }
                            if (!tryPlace)
                            {
                                continue;
                            }
                            //divide y by less for more "layers"
                            double xVal = (x + offsetX) / 35, yVal = y / 35, zVal = (z + offsetZ) / 35;
                            xVal += 1;
                            yVal += 1;
                            zVal += 1;
                            double value = adjNoise.GetValue(xVal, yVal, zVal);
                            counter++;
                            if (value > threshold)
                            {
                                lvl.SetBlock((ushort)x, (ushort)y, (ushort)z, Nas.FromRaw(451));
                            }
                        }
                    }
                    TimeSpan span = DateTime.UtcNow.Subtract(dateStartLayer);
                    if (span > TimeSpan.FromSeconds(5))
                    {
                        if (p != Player.Console)
                        {
                            Log("Random gen {0}% complete.", (int)(y / height * 100));
                        }
                        p.Message("Random gen {0}% complete.", (int)(y / height * 100));
                        dateStartLayer = DateTime.UtcNow;
                    }
                }
                p.Message("Random gen 100% complete.");
            }
            public void GenPlants()
            {
                p.Message("Now creating foliage");
                if (biome < 0)
                {
                    for (ushort y = 0; y < (ushort)(lvl.Height - 1); y++)
                    {
                        for (ushort z = 0; z < lvl.Length; ++z)
                        {
                            for (ushort x = 0; x < lvl.Width; ++x)
                            {
                                if (lvl.FastGetBlock(x, (ushort)(y + 1), z) == 0 && lvl.FastGetBlock(x, y, z) == 48)
                                {
                                    if (r.Next(0, 320) == 0)
                                    {
                                        lvl.SetTile(x, (ushort)(y + 1), z, 54);
                                    }
                                    if (r.Next(0, 320) == 0)
                                    {
                                        lvl.SetBlock(x, (ushort)(y + 1), z, Nas.FromRaw(456));
                                    }
                                }
                            }
                        }
                    }
                    return;
                }
                adjNoise.Seed = MakeInt(seed + "tree");
                adjNoise.Frequency = 1;
                adjNoise.OctaveCount = 1;
                DateTime dateStartLayer;
                //counter = 0;
                dateStartLayer = DateTime.UtcNow;
                int height = lvl.Height - 1, width = lvl.Width, length = lvl.Length;
                for (int y = 0; y < (ushort)height; y++)
                {
                    for (int z = 0; z < length; ++z)
                    {
                        for (int x = 0; x < width; ++x)
                        {
                            topSoil = 256 | 129; //Block.Grass;
                            if (biome == 1)
                            {
                                topSoil = 12;
                            }
                            if (biome == 2)
                            {
                                topSoil = Nas.FromRaw(139);
                            }
                            if ((lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z) == 3 || (lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z) == 12 && biome == 1)) &&
                                lvl.FastGetBlock((ushort)x, (ushort)(y + 1), (ushort)z) == 0)
                            {
                                if (((r.Next(0, 50) == 0 && biome != 3 && biome != 4 && biome != 6) || (r.Next(0, 15) == 0 && (biome == 3 || biome == 4 || biome == 6))) && lvl.IsAirAt((ushort)x, (ushort)(y + 10), (ushort)z))
                                {
                                    double xVal = ((double)x + offsetX) / 200, yVal = (double)y / 130, zVal = ((double)z + offsetZ) / 200;
                                    xVal += 1;
                                    yVal += 1;
                                    zVal += 1;
                                    double value = adjNoise.GetValue(xVal, yVal, zVal);
                                    if (value > r.NextDouble() || biome == 3 || biome == 4 || biome == 6)
                                    {
                                        GenTree((ushort)x, (ushort)(y + 1), (ushort)z);
                                    }
                                    else if (r.Next(0, 20) == 0)
                                    {
                                        GenTree((ushort)x, (ushort)(y + 1), (ushort)z);
                                    }
                                }
                                else if (biome != 1)
                                {
                                    if (r.Next(0, 10) == 0)
                                    {
                                        //tallgrass 40 wettallgrass Block.Extended|130
                                        lvl.SetBlock((ushort)x, (ushort)(y + 1), (ushort)z, 256 | 130);
                                    }
                                    else
                                    {
                                        if (biome == 2)
                                        {
                                            lvl.SetBlock((ushort)x, (ushort)(y + 1), (ushort)z, 53);
                                        }
                                        if (biome == 5)
                                        {
                                            if (r.Next(0, 2) == 0)
                                            {
                                                int flowerChance = r.Next(0, 10);
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
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                lvl.SetBlock((ushort)x, (ushort)y, (ushort)z, topSoil);
                            }
                        }
                    }
                    TimeSpan span = DateTime.UtcNow.Subtract(dateStartLayer);
                    if (span > TimeSpan.FromSeconds(5))
                    {
                        if (p != Player.Console)
                        {
                            Log("Foilage gen {0}% complete.", y / height * 100);
                        }
                        p.Message("Foilage gen {0}% complete.", y / height * 100);
                        dateStartLayer = DateTime.UtcNow;
                    }
                }
                if (biome == 6)
                {
                    for (int z = 0; z < length; ++z)
                    {
                        for (int x = 0; x < width; ++x)
                        {
                            if (NasBlock.IsPartOfSet(NasBlock.waterSet, lvl.FastGetBlock((ushort)x, oceanHeight - 1, (ushort)z)) != -1)
                            {
                                if (lvl.FastGetBlock((ushort)x, oceanHeight, (ushort)z) != 0)
                                {
                                    continue;
                                }
                                if (r.NextDouble() <= 0.05)
                                {
                                    lvl.SetBlock((ushort)x, oceanHeight, (ushort)z, Nas.FromRaw(449));
                                }
                            }
                        }
                    }
                }
                p.Message("Foliage gen 100% complete.");
            }
            public void GenTree(ushort x, ushort y, ushort z)
            {
                if (biome == 3)
                {
                    if (r.Next(3) == 0)
                    {
                        return;
                    }
                    NasTree.GenBirchTree(nl, r, x, y, z);
                    return;
                }
                if (biome == 4)
                {
                    if (r.Next(3) == 0)
                    {
                        return;
                    }
                    NasTree.GenOakTree(nl, r, x, y, z);
                    return;
                }
                if (biome == 6)
                {
                    if (r.Next(2) != 0)
                    {
                        return;
                    }
                    if (y > oceanHeight + 6)
                    {
                        return;
                    }
                    NasTree.GenSwampTree(nl, r, x, y, z);
                    return;
                }
                if (biome == 1)
                {
                    if (r.Next(5) == 0)
                    {
                        lvl.SetBlock(x, y, z, 256 | 106);
                        lvl.SetBlock(x, (ushort)(y + 1), z, 256 | 106);
                        lvl.SetBlock(x, (ushort)(y + 2), z, 256 | 106);
                        return;
                    }
                }
                else
                {
                    if (biome == 2)
                    {
                        NasTree.GenSpruceTree(nl, r, x, y, z);
                    }
                    else
                    {
                        topSoil = 3;
                        if (r.Next(5) == 0)
                        {
                            NasTree.GenBirchTree(nl, r, x, y, z);
                        }
                        else
                        {
                            NasTree.GenOakTree(nl, r, x, y, z);
                        }
                    }
                }
            }
            public ushort GetSoilType()
            {
                if (biome == 1)
                {
                    return 12;
                }
                return 3;
            }
            public void GenOre()
            {
                for (int y = 0; y < lvl.Height - 1; y++)
                {
                    for (int z = 0; z < lvl.Length; ++z)
                    {
                        for (int x = 0; x < lvl.Width; ++x)
                        {
                            ushort curBlock = lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z);
                            if (NasBlock.IsPartOfSet(stoneTypes, curBlock) == -1)
                            {
                                continue;
                            }
                            if (biome >= 0)
                            {
                                TryGenOre(x, y, z, ironDepth, ironChance, 628, 3);
                                TryGenOre(x, y, z, goldDepth, goldChance, 629, 3);
                                TryGenDiamond(x, y, z, diamondDepth, diamondChance, 630, 2);
                                TryGenEmerald(x, y, z, emeraldDepth, emeraldChance, 649, 1);
                                TryGenOre(x, y, z, coalDepth, coalChance, 627, r.Next(3, 4), 0.5);
                            }
                            if (biome == 1)
                            {
                                TryGenOre(x, y, z, coalDepth, quartzChance, 586, 3);
                            }
                            if (biome < 0)
                            {
                                TryGenOre(x, y, z, -1000, coalChance, 454, 3);
                                TryGenOre(x, y, z, -1000, goldChance, 455, 2);
                            }
                        }
                    }
                }
                for (ushort xPl = 0; xPl <= 383; xPl++)
                {
                    for (ushort yPl = 0; yPl <= 20; yPl++)
                    {
                        for (ushort zPl = 0; zPl <= 383; zPl++)
                        {
                            if (yPl <= 10 || r.Next(yPl - 9) == 0)
                            {
                                if (lvl.FastGetBlock(xPl, yPl, zPl) == 1 || lvl.FastGetBlock(xPl, yPl, zPl) == 48)
                                {
                                    lvl.SetBlock(xPl, yPl, zPl, biome >= 0 ? Nas.FromRaw(429) : Nas.FromRaw(452));
                                }
                            }
                        }
                    }
                }
            }
            public bool TryGenOre(int x, int y, int z, int oreDepth, float oreChance, ushort oreID, int size = 0, double vsf = 0.4)
            {
                double chance = (double)(oreChance / 100);
                int height = nl.heightmap[x, z];
                if (height < oceanHeight)
                {
                    height = oceanHeight;
                }
                int hmbyhttdfttrh = lvl.Height - height;
                hmbyhttdfttrh += oreDepth;
                if (y <= lvl.Height - hmbyhttdfttrh && r.NextDouble() <= chance)
                {
                    if (r.NextDouble() > 0.5)
                    {
                        if (BlockExposed(x, y, z))
                        {
                            return false;
                        }
                    }
                    GenerateOreCluster(x, y, z, oreID, size, vsf);
                    return true;
                }
                return false;
            }
            public bool TryGenDiamond(int x, int y, int z, int oreDepth, float oreChance, ushort oreID, int size = 0, double vsf = 0.4)
            {
                double chance = (double)(oreChance / 100);
                int height = nl.heightmap[x, z];
                Random rng = new();
                int genY = rng.Next(0, 20);
                if (height < oceanHeight)
                {
                    height = oceanHeight;
                }
                int hmbyhttdfttrh = lvl.Height - height;
                hmbyhttdfttrh += oreDepth;
                if (y <= lvl.Height - hmbyhttdfttrh && r.NextDouble() <= chance)
                {
                    if (r.NextDouble() > 0.5)
                    {
                        if (BlockExposed(x, y, z))
                        {
                            return false;
                        }
                    }
                    GenerateOreCluster(x, genY, z, oreID, size, vsf);
                    return true;
                }
                return false;
            }
            public bool TryGenEmerald(int x, int y, int z, int oreDepth, float oreChance, ushort oreID, int size = 0, double vsf = 0.4)
            {
                double chance = (double)(oreChance / 100);
                int height = nl.heightmap[x, z];
                Random rng = new();
                int genY = rng.Next(0, 15);
                if (height < oceanHeight)
                {
                    height = oceanHeight;
                }
                int hmbyhttdfttrh = lvl.Height - height;
                hmbyhttdfttrh += oreDepth;
                if (y <= lvl.Height - hmbyhttdfttrh && r.NextDouble() <= chance)
                {
                    if (r.NextDouble() > 0.5)
                    {
                        if (BlockExposed(x, y, z))
                        {
                            return false;
                        }
                    }
                    GenerateOreCluster(x, genY, z, oreID, size, vsf);
                    return true;
                }
                return false;
            }
            public void GenerateOreCluster(int x, int y, int z, ushort oreID, int iteration, double chance = 0.4)
            {
                if (x < 0 || y < 0 || z < 0 || x > mapWideness - 1 || z > mapWideness - 1 || y > mapTallness - 1)
                {
                    return;
                }
                ushort hereBlock = lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z);
                if (hereBlock == 1 || hereBlock == 52 || hereBlock == 48)
                {
                    lvl.SetBlock((ushort)x, (ushort)y, (ushort)z, Nas.FromRaw(oreID));
                }
                else
                {
                    return;
                }
                iteration--;
                if (iteration == 0)
                {
                    return;
                }
                Random rng = new();
                if (rng.NextDouble() <= chance)
                {
                    GenerateOreCluster(x + 1, y, z, oreID, iteration);
                }
                if (rng.NextDouble() <= chance)
                {
                    GenerateOreCluster(x - 1, y, z, oreID, iteration);
                }
                if (rng.NextDouble() <= chance)
                {
                    GenerateOreCluster(x, y + 1, z, oreID, iteration);
                }
                if (rng.NextDouble() <= chance)
                {
                    GenerateOreCluster(x, y - 1, z, oreID, iteration);
                }
                if (rng.NextDouble() <= chance)
                {
                    GenerateOreCluster(x, y, z + 1, oreID, iteration);
                }
                if (rng.NextDouble() <= chance)
                {
                    GenerateOreCluster(x, y, z - 1, oreID, iteration);
                }
            }
            public void GenWaterSources()
            {
                if (biome == 6)
                {
                    lvl.CustomBlockDefs[8] = BlockDefinition.GlobalDefs[8].Copy();
                    lvl.CustomBlockDefs[8].Name = "#Water";
                    lvl.CustomBlockDefs[8].FogR = 72;
                    lvl.CustomBlockDefs[8].FogG = 94;
                    lvl.CustomBlockDefs[8].FogB = 24;
                    lvl.CustomBlockDefs[Nas.FromRaw(129)] = BlockDefinition.GlobalDefs[Nas.FromRaw(129)].Copy();
                    lvl.CustomBlockDefs[Nas.FromRaw(129)].Name = "#Grass";
                    lvl.CustomBlockDefs[Nas.FromRaw(129)].FogR = 176;
                    lvl.CustomBlockDefs[Nas.FromRaw(129)].FogG = 191;
                    lvl.CustomBlockDefs[Nas.FromRaw(129)].FogB = 176;
                    lvl.CustomBlockDefs[3] = BlockDefinition.GlobalDefs[3].Copy();
                    lvl.CustomBlockDefs[3].Name = "#Dirt";
                    lvl.CustomBlockDefs[3].FogR = 176;
                    lvl.CustomBlockDefs[3].FogG = 191;
                    lvl.CustomBlockDefs[3].FogB = 176;
                    lvl.CustomBlockDefs[Nas.FromRaw(130)] = BlockDefinition.GlobalDefs[Nas.FromRaw(130)].Copy();
                    lvl.CustomBlockDefs[Nas.FromRaw(130)].Name = "#Tall grass";
                    lvl.CustomBlockDefs[Nas.FromRaw(130)].FogR = 176;
                    lvl.CustomBlockDefs[Nas.FromRaw(130)].FogG = 191;
                    lvl.CustomBlockDefs[Nas.FromRaw(130)].FogB = 176;
                    BlockDefinition.Save(false, lvl);
                }
                for (int y = 0; y < lvl.Height - 1; y++)
                {
                    for (int z = 0; z < lvl.Length; ++z)
                    {
                        for (int x = 0; x < lvl.Width; ++x)
                        {
                            ushort curBlock = lvl.FastGetBlock((ushort)x, (ushort)y, (ushort)z);
                            if (curBlock == 10)
                            {
                                if (BlockExposed2(x, y, z))
                                {
                                    nl.blocksThatMustBeDisturbed.Add(new NasLevel.BlockLocation(x, y, z));
                                }
                            }
                            if (NasBlock.IsPartOfSet(stoneTypes, curBlock) == -1)
                            {
                                continue;
                            }
                            if (r.NextDouble() < 0.00025)
                            {
                                if (BlockExposed(x, y, z))
                                {
                                    if (NasBlock.IsPartOfSet(stoneTypes, lvl.FastGetBlock((ushort)x, (ushort)(y + 1), (ushort)z)) == -1)
                                    {
                                        continue;
                                    }
                                    lvl.SetTile((ushort)x, (ushort)y, (ushort)z, (byte)(biome < 0 ? 10 : 9));
                                    nl.blocksThatMustBeDisturbed.Add(new NasLevel.BlockLocation(x, y, z));
                                }
                            }
                        }
                    }
                }
            }
            public bool BlockExposed(int x, int y, int z)
            {
                if (lvl.IsAirAt((ushort)(x + 1), (ushort)y, (ushort)z))
                {
                    return true;
                }
                if (lvl.IsAirAt((ushort)(x - 1), (ushort)y, (ushort)z))
                {
                    return true;
                }
                if (lvl.IsAirAt((ushort)x, (ushort)(y + 1), (ushort)z))
                {
                    return true;
                }
                if (lvl.IsAirAt((ushort)x, (ushort)(y - 1), (ushort)z))
                {
                    return true;
                }
                if (lvl.IsAirAt((ushort)x, (ushort)y, (ushort)(z + 1)))
                {
                    return true;
                }
                if (lvl.IsAirAt((ushort)x, (ushort)y, (ushort)(z - 1)))
                {
                    return true;
                }
                return false;
            }
            public bool BlockExposed2(int x, int y, int z)
            {
                if (lvl.IsAirAt((ushort)(x + 1), (ushort)y, (ushort)z))
                {
                    return true;
                }
                if (lvl.IsAirAt((ushort)(x - 1), (ushort)y, (ushort)z))
                {
                    return true;
                }
                if (lvl.IsAirAt((ushort)x, (ushort)y, (ushort)(z + 1)))
                {
                    return true;
                }
                if (lvl.IsAirAt((ushort)x, (ushort)y, (ushort)(z - 1)))
                {
                    return true;
                }
                return false;
            }
            public void GenDungeons()
            {
                p.Message("Generating structures");
                int dungeonCount = r.Next(3, 6);
                for (int done = 0; done <= dungeonCount; done++)
                {
                    GenerateDungeon(r, lvl, nl);
                }
            }
            public static void GenerateDungeon(NasPlayer np, int x, int y, int z, Level level, NasLevel nsl)
            {
                GenerateDungeon(np.p, x, y, z, level, nsl);
            }
            public static void GenerateDungeon(Player p, int x, int y, int z, Level level, NasLevel nsl)
            {
                Random rng = new(MakeInt(level.name));
                if (p != null)
                {
                    GenerateDungeon(rng, x + 2, y, z + 2, level, nsl, true, p);
                }
            }
            public static void GenerateDungeon(Random rng, int x, int y, int z, Level level, NasLevel nsl, bool forced, Player p)
            {
                for (int dx = 0; dx < 9; dx++)
                {
                    for (int dy = 0; dy < 7; dy++)
                    {
                        for (int dz = 0; dz < 9; dz++)
                        {
                            if (rng.Next(0, 3) == 0)
                            {
                                level.SetBlock((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), Nas.FromRaw(180));
                            }
                            else
                            {
                                level.SetTile((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), 65);
                            }
                        }
                    }
                }
                for (int dx = 1; dx < 8; dx++)
                {
                    for (int dy = 2; dy < 6; dy++)
                    {
                        for (int dz = 1; dz < 8; dz++)
                        {
                            level.SetTile((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), 0);
                        }
                    }
                }
                int dungeonType = rng.Next(0, 6);
                if (dungeonType == 0)
                {
                    for (int dx = 2; dx < 7; dx++)
                    {
                        for (int dz = 2; dz < 7; dz++)
                        {
                            level.SetBlock((ushort)(x + dx), (ushort)(y + 2), (ushort)(z + dz), Nas.FromRaw(476));
                        }
                    }
                    level.SetTile((ushort)(x + 3), (ushort)(y + 2), (ushort)(z + 4), 0);
                    level.SetTile((ushort)(x + 5), (ushort)(y + 2), (ushort)(z + 4), 0);
                    level.SetTile((ushort)(x + 4), (ushort)(y + 2), (ushort)(z + 3), 0);
                    level.SetTile((ushort)(x + 4), (ushort)(y + 2), (ushort)(z + 5), 0);
                    level.SetTile((ushort)(x + 3), (ushort)(y + 1), (ushort)(z + 4), 0);
                    level.SetTile((ushort)(x + 5), (ushort)(y + 1), (ushort)(z + 4), 0);
                    level.SetTile((ushort)(x + 4), (ushort)(y + 1), (ushort)(z + 3), 0);
                    level.SetTile((ushort)(x + 4), (ushort)(y + 1), (ushort)(z + 5), 0);
                    level.SetTile((ushort)(x + 4), (ushort)(y + 4), (ushort)(z + 4), 65);
                    level.SetTile((ushort)(x + 4), (ushort)(y + 5), (ushort)(z + 4), 10);
                    nsl.blocksThatMustBeDisturbed.Add(new NasLevel.BlockLocation(x + 4, y + 5, z + 4));
                    GenLoot(x + 4, y + 2, z + 4, level, rng, nsl, forced, p);
                    return;
                }
                if (dungeonType == 1)
                {
                    level.SetTile((ushort)(x + 2), (ushort)(y + 1), (ushort)(z + 2), 48);
                    level.SetTile((ushort)(x + 2), (ushort)(y + 1), (ushort)(z + 6), 48);
                    level.SetTile((ushort)(x + 6), (ushort)(y + 1), (ushort)(z + 2), 48);
                    level.SetTile((ushort)(x + 6), (ushort)(y + 1), (ushort)(z + 6), 48);
                    level.SetBlock((ushort)(x + 2), (ushort)(y + 1), (ushort)(z + 4), Nas.FromRaw(469));
                    level.SetBlock((ushort)(x + 6), (ushort)(y + 1), (ushort)(z + 4), Nas.FromRaw(469));
                    level.SetBlock((ushort)(x + 4), (ushort)(y + 1), (ushort)(z + 2), Nas.FromRaw(469));
                    level.SetBlock((ushort)(x + 4), (ushort)(y + 1), (ushort)(z + 6), Nas.FromRaw(469));
                    level.SetBlock((ushort)(x + 4), (ushort)(y + 2), (ushort)(z + 4), Nas.FromRaw(457));
                    GenLoot(x + 1, y + 2, z + 1, level, rng, nsl, forced, p);
                    GenLoot(x + 7, y + 2, z + 7, level, rng, nsl, forced, p);
                    return;
                }
                if (dungeonType == 2)
                {
                    for (int dx = 1; dx < 8; dx++)
                    {
                        for (int dz = 1; dz < 8; dz++)
                        {
                            level.SetTile((ushort)(x + dx), (ushort)(y + 1), (ushort)(z + dz), 10);
                        }
                    }
                    for (int dx = 1; dx < 8; dx++)
                    {
                        for (int dz = 1; dz < 8; dz++)
                        {
                            level.SetBlock((ushort)(x + dx), (ushort)(y + 2), (ushort)(z + dz), (rng.Next(2) == 0) ? (ushort)65 : Nas.FromRaw(685));
                        }
                    }
                    GenLoot(x + 4, y + 3, z + 4, level, rng, nsl, forced, p);
                    return;
                }
                if (dungeonType == 3)
                {
                    for (int count = 0; count < 4; count++)
                    {
                        int dx = rng.Next(1, 8),
                            dz = rng.Next(1, 8);
                        level.SetBlock((ushort)(x + dx), (ushort)(y + 2), (ushort)(z + dz), Nas.FromRaw(604));
                    }
                    for (int count = 0; count < 4; count++)
                    {
                        int dx = rng.Next(1, 8),
                            dz = rng.Next(1, 8);
                        level.SetBlock((ushort)(x + dx), (ushort)(y + 2), (ushort)(z + dz), Nas.FromRaw(653));
                    }
                    GenLoot(x + 4, y + 2, z + 4, level, rng, nsl, forced, p);
                    return;
                }
                if (dungeonType == 4)
                {
                    for (int dx = 1; dx < 8; dx++)
                    {
                        for (int dy = 1; dy < 6; dy++)
                        {
                            for (int dz = 1; dz < 8; dz++)
                            {
                                if (rng.Next(8) == 0)
                                {
                                    level.SetTile((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), 10);
                                }
                                else
                                {
                                    level.SetTile((ushort)(x + dx), (ushort)(y + dy), (ushort)(z + dz), 1);
                                }
                            }
                        }
                    }
                    GenLoot(x + 4, y + 1, z + 4, level, rng, nsl, forced, p);
                    return;
                }
                if (dungeonType == 5)
                {
                    for (int dx = 1; dx < 8; dx++)
                    {
                        for (int dz = 1; dz < 8; dz++)
                        {
                            level.SetBlock((ushort)(x + dx), (ushort)(y + 1), (ushort)(z + dz), Nas.FromRaw(129));
                        }
                    }
                    level.SetBlock((ushort)(x + 4), (ushort)(y + 3), (ushort)(z + 4), Nas.FromRaw(171));
                    NasBlock.Entity bEntity = new()
                    {
                        blockText = "&mCongratulations. You touched grass."
                    };
                    if (!nsl.blockEntities.ContainsKey(x + 4 + " " + (y + 3) + " " + (z + 4)))
                    {
                        nsl.blockEntities.Add(x + 4 + " " + (y + 3) + " " + (z + 4), bEntity);
                    }
                    GenLoot(x + 4, y + 2, z + 4, level, rng, nsl, forced, p);
                    return;
                }
            }
            public static void GenerateDungeon(Random rng, Level level, NasLevel nsl)
            {
                int genX = rng.Next(10, mapWideness - 10),
                    genZ = rng.Next(10, mapWideness - 10),
                    genY = rng.Next(0, 15);
                GenerateDungeon(rng, genX, genY, genZ, level, nsl, false, Player.Console);
            }
            public static void GenLoot(int x, int y, int z, Level level, Random rng, NasLevel nsl, bool forced, Player p)
            {
                level.SetBlock((ushort)x, (ushort)y, (ushort)z, Nas.FromRaw(647));
                NasBlock.Entity bEntity = new()
                {
                    drop = new Drop(41, rng.Next(1, 5)) //gold
                };
                bEntity.drop.blockStacks.Add(new BlockStack(729, rng.Next(0, 3)));
                if (rng.Next(2) == 0)
                {
                    bEntity.drop.blockStacks.Add(new BlockStack(631, rng.Next(1, 3))); //dia
                }
                if (rng.Next(4) == 0)
                {
                    bEntity.drop.blockStacks.Add(new BlockStack(650)); //ems
                }
                if (rng.Next(3) == 0)
                {
                    bEntity.drop.blockStacks.Add(new BlockStack(478)); //gapple
                }
                if (rng.Next(3) == 0)
                {
                    bEntity.drop.blockStacks.Add(new BlockStack(204)); //monitor
                }
                if (nsl.blockEntities.ContainsKey(x + " " + y + " " + z))
                {
                    nsl.blockEntities.Remove(x + " " + y + " " + z);
                }
                nsl.blockEntities.Add(x + " " + y + " " + z, bEntity);
                if (forced)
                {
                    if (!p.IsSuper)
                    {
                        PlayerActions.ReloadMap(p);
                    }
                }
            }
        }
    }
}
#endif