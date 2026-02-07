using MCGalaxy.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NASBlockAction = MCGalaxy.NASAction<MCGalaxy.NASLevel, MCGalaxy.NASBlock, int, int, int>;
namespace MCGalaxy
{
    public partial class NASLevel
    {
        [JsonIgnore] public static Dictionary<string, NASLevel> all = new();
        [JsonIgnore] public Level lvl;
        [JsonIgnore] public ushort[,] heightmap = new ushort[0, 0];
        [JsonIgnore] public SimplePriorityQueue<NASQueuedBlockUpdate, DateTime> tickQueue = new();
        [JsonIgnore] public SchedulerTask schedulerTask;
        public int biome;
        public bool dungeons = false,
            deepslateGenerated = true;
        public static Scheduler TickScheduler;
        public static TimeSpan tickDelay = TimeSpan.FromMilliseconds(100);
        public static Random r = new();
        public ushort height;
        public List<NASBlockLocation> blocksThatMustBeDisturbed = new();
        public Dictionary<string, NASBlock.NASBlockEntity> blockEntities = new();
        public ushort[] observers =
        {
            NASPlugin.FromRaw(415),
            NASPlugin.FromRaw(416),
            NASPlugin.FromRaw(417),
            NASPlugin.FromRaw(418),
            NASPlugin.FromRaw(419),
            NASPlugin.FromRaw(420),
        },
        repeatersOff =
        {
            NASPlugin.FromRaw(176),
            NASPlugin.FromRaw(177),
            NASPlugin.FromRaw(174),
            NASPlugin.FromRaw(175),
            NASPlugin.FromRaw(172),
            NASPlugin.FromRaw(173),
        },
        repeatersOn =
        {
            NASPlugin.FromRaw(617),
            NASPlugin.FromRaw(618),
            NASPlugin.FromRaw(615),
            NASPlugin.FromRaw(616),
            NASPlugin.FromRaw(613),
            NASPlugin.FromRaw(614),
        };
        public class NASBlockLocation
        {
            public int X, Y, Z;
            public NASBlockLocation() { }
            public NASBlockLocation(NASQueuedBlockUpdate qb)
            {
                X = qb.x;
                Y = qb.y;
                Z = qb.z;
            }
            public NASBlockLocation(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }
        public struct NASQueuedBlockUpdate
        {
            public int x, y, z;
            public NASBlock nb;
            public DateTime date;
            public NASBlockAction da;
        }
        public static void Log(string format, params object[] args) => Logger.Log(LogType.Debug, string.Format(format, args));
        public void BeginTickTask()
        {
            TickScheduler ??= new("NASLevelTickScheduler");
            Log("Re-disturbing {0} blocks.", blocksThatMustBeDisturbed.Count);
            foreach (NASBlockLocation blockLoc in blocksThatMustBeDisturbed)
            {
                DisturbBlock(blockLoc.X, blockLoc.Y, blockLoc.Z);
            }
            blocksThatMustBeDisturbed.Clear();
            schedulerTask = TickScheduler.QueueRepeat(TickLevelCallback, this, tickDelay);
        }
        public void EndTickTask()
        {
            TickScheduler ??= new("NASLevelTickScheduler");
            TickScheduler.Cancel(schedulerTask);
            Log("Saving {0} blocks to re-disturb later.", tickQueue.Count);
            if (tickQueue.Count == 0)
            {
                return;
            }
            blocksThatMustBeDisturbed = new();
            foreach (NASQueuedBlockUpdate qb in tickQueue)
            {
                NASBlockLocation blockLoc = new(qb);
                if (blocksThatMustBeDisturbed.Contains(blockLoc))
                {
                    continue;
                }
                blocksThatMustBeDisturbed.Add(blockLoc);
            }
            tickQueue.Clear();
        }
        public static void TickLevelCallback(SchedulerTask task)
        {
            object state = task.State;
            if (state != null)
            {
                NASLevel nl = (NASLevel)state;
                if (nl != null)
                {
                    nl.Tick();
                }
                else
                {
                    Log("NASLevel tick task was null, skipping tick.");
                }
            }
            else
            {
                Log("NASLevel tick task was null, skipping tick.");
            }
        }
        public void Tick()
        {
            if (tickQueue.Count < 1)
            {
                return;
            }
            int actions = 0;
            while (tickQueue.First.date < DateTime.UtcNow)
            {
                if (actions > 64)
                {
                    break;
                }
                NASQueuedBlockUpdate qb = tickQueue.First;
                if (lvl != null)
                {
                    ushort block = lvl.GetBlock((ushort)qb.x, (ushort)qb.y, (ushort)qb.z);
                    if (block > 0)
                    {
                        ushort u = NASBlock.blocksIndexedByServerushort[block].selfID;
                        if (u > 0)
                        {
                            if (u == qb.nb.selfID)
                            {
                                if (this != null)
                                {
                                    if (qb.nb != null)
                                    {
                                        qb.da(this, qb.nb, qb.x, qb.y, qb.z);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Log("NASLevel tick called on a null level, skipping tick");
                    return;
                }
                tickQueue.Dequeue();
                actions++;
                if (tickQueue.Count < 1)
                {
                    break;
                }
            }
        }
        public void SetBlock(int x, int y, int z, ushort serverushort, bool disturbDiagonals = false)
        {
            if (
                x >= lvl.Width ||
                x < 0 ||
                y >= lvl.Height ||
                y < 0 ||
                z >= lvl.Length ||
                z < 0 ||
                serverushort == 255)
            {
                return;
            }
            lvl.Blockchange((ushort)x, (ushort)y, (ushort)z, serverushort);
            DisturbBlocks(x, y, z, disturbDiagonals);
        }
        public void FastSetBlock(int x, int y, int z, ushort serverushort, bool disturbDiagonals = false)
        {
            lvl.Blockchange((ushort)x, (ushort)y, (ushort)z, serverushort);
            DisturbBlocks(x, y, z, disturbDiagonals);
        }
        public void SimulateSetBlock(int x, int y, int z, bool disturbDiagonals = false)
        {
            if (
                x >= lvl.Width ||
                x < 0 ||
                y >= lvl.Height ||
                y < 0 ||
                z >= lvl.Length ||
                z < 0)
            {
                return;
            }
            DisturbBlocks(x, y, z, disturbDiagonals);
        }
        public void DisturbBlocks(int x, int y, int z, bool diagonals = false)
        {
            if (diagonals)
            {
                for (int xOff = -1; xOff <= 1; xOff++)
                {
                    for (int yOff = -1; yOff <= 1; yOff++)
                    {
                        for (int zOff = -1; zOff <= 1; zOff++)
                        {
                            DisturbBlock(x, y, z, xOff, yOff, zOff);
                        }
                    }
                }
                return;
            }
            if (NASBlock.IsPartOfSet(observers, lvl.GetBlock((ushort)x, (ushort)y, (ushort)z)) == -1)
            {
                DisturbBlock(x, y, z);
            }
            DisturbBlock(x, y, z, 1, 0, 0);
            DisturbBlock(x, y, z, -1, 0, 0);
            DisturbBlock(x, y, z, 0, 1, 0);
            DisturbBlock(x, y, z, 0, -1, 0);
            DisturbBlock(x, y, z, 0, 0, 1);
            DisturbBlock(x, y, z, 0, 0, -1);
        }
        public void DisturbBlock(int x, int y, int z, int changeX = 0, int changeY = 0, int changeZ = 0)
        {
            if (
                x + changeX >= lvl.Width ||
                x + changeX < 0 ||
                y + changeY >= lvl.Height ||
                y + changeY < 0 ||
                z + changeZ >= lvl.Length ||
                z + changeZ < 0)
            {
                return;
            }
            ushort block = lvl.FastGetBlock((ushort)(x + changeX), (ushort)(y + changeY), (ushort)(z + changeZ));
            int index = NASBlock.IsPartOfSet(observers, block);
            if (index == -1)
            {
                index = NASBlock.IsPartOfSet(repeatersOff, block);
            }
            if (index == -1)
            {
                index = NASBlock.IsPartOfSet(repeatersOn, block);
            }
            if (index != -1 && Math.Abs(changeX) + Math.Abs(changeY) + Math.Abs(changeZ) == 1)
            {
                bool cancel = true;
                if (index == 0 && changeZ == 1)
                {
                    cancel = false;
                }
                if (index == 1 && changeX == -1)
                {
                    cancel = false;
                }
                if (index == 2 && changeZ == -1)
                {
                    cancel = false;
                }
                if (index == 3 && changeX == 1)
                {
                    cancel = false;
                }
                if (index == 4 && changeY == -1)
                {
                    cancel = false;
                }
                if (index == 5 && changeY == 1)
                {
                    cancel = false;
                }
                if (cancel)
                {
                    return;
                }
            }
            x += changeX;
            y += changeY;
            z += changeZ;
            NASBlock nb = NASBlock.blocksIndexedByServerushort[block];
            if (nb.disturbedAction == null)
            {
                return;
            }
            NASQueuedBlockUpdate qb = new()
            {
                x = x,
                y = y,
                z = z
            };
            float seconds = (float)(r.NextDouble() * (nb.disturbDelayMax - nb.disturbDelayMin) + nb.disturbDelayMin);
            qb.date = DateTime.UtcNow + TimeSpan.FromSeconds(seconds);
            qb.date = qb.date.Floor(tickDelay);
            qb.nb = nb;
            qb.da = nb.disturbedAction;
            tickQueue.Enqueue(qb, qb.date);
            if (nb.instantAction == null)
            {
                return;
            }
            qb = new()
            {
                x = x,
                y = y,
                z = z,
                nb = nb,
                date = DateTime.UtcNow
            };
            qb.date = qb.date.Floor(tickDelay);
            qb.da = nb.instantAction;
            tickQueue.Enqueue(qb, DateTime.UtcNow.Floor(tickDelay));
        }
        public ushort GetBlock(int x, int y, int z) => lvl.GetBlock((ushort)x, (ushort)y, (ushort)z);
    }
}
