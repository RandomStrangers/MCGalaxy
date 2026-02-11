using MCGalaxy.Blocks;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.Util.Imaging;
using System;
namespace MCGalaxy
{
    public class NASInvInfo
    {
        public NASPlayer np;
        public bool inv;
    }
    public class NASFishingInfo
    {
        public Player p, who;
    }
    public class NASBreakInfo
    {
        public NASPlayer np;
        public ushort x, y, z,
            serverushort;
        public NASBlock nasBlock;
        public int breakAttempt;
        public NASItem toolUsed;
    }
    public class NASMeterInfo
    {
        public Player p;
        public int milliseconds;
        public float x, y, z;
    }
    public static class NASBlockChange
    {
        public static Scheduler breakScheduler,
            repeaterScheduler,
            fishingScheduler;
        public static Pixel[] blockColors = new Pixel[768];
        public static byte BreakEffectID = 255;
        public static bool Setup()
        {
            breakScheduler ??= new("BlockBreakScheduler");
            repeaterScheduler ??= new("RepeaterScheduler");
            fishingScheduler ??= new("FishingScheduler");
            if (!FileIO.TryReadBytes(NAS.Path + "terrain.png", out byte[] data))
            {
                Logger.Log(LogType.Warning, "Could not read {0} (needed for block particle colors)", NAS.Path + "terrain.png");
                return false;
            }
            Bitmap2D terrain = ImageDecoder.DecodeFrom(data);
            terrain.Width /= 16;
            terrain.Height /= 16;
            for (ushort blockID = 0; blockID <= 767; blockID++)
            {
                BlockDefinition def = BlockDefinition.GlobalDefs[Block.FromRaw(blockID)];
                if (def == null && blockID < 66)
                {
                    def = DefaultSet.MakeCustomBlock(Block.FromRaw(blockID));
                }
                if (def == null)
                {
                    blockColors[blockID] = new(255,255,255,255);
                    continue;
                }
                int x = def.BackTex % 16,
                    y = def.BackTex / 16;
                blockColors[blockID] = terrain.Get(x, y);
            }
            return true;
        }
        public static byte GetBreakID() => BreakEffectID;
        public static void SetBreakID(byte value)
        {
            if (value <= 249)
            {
                value = 255;
            }
            BreakEffectID = value;
        }
        public static void BreakBlock(NASPlayer np, ushort x, ushort y, ushort z, ushort serverushort, NASBlock nasBlock)
        {
            if (np.nl == null)
            {
                return;
            }
            ushort here = np.p.Level.GetBlock(x, y, z);
            if (here != serverushort)
            {
                return;
            }
            if (nasBlock.container != null &&
                np.nl.blockEntities.ContainsKey(x + " " + y + " " + z) &&
                (np.nl.blockEntities[x + " " + y + " " + z].drop != null || !np.nl.blockEntities[x + " " + y + " " + z].CanAccess(np))
               )
            {
                return;
            }
            if (np.isInserting)
            {
                np.Message("&ePlease insert items into the container before breaking blocks.");
                return;
            }
            if (nasBlock.parentID != 0)
            {
                NASDrop drop = nasBlock.dropHandler(np, nasBlock.parentID);
                np.inventory.GetDrop(drop);
                Random r = new();
                np.GiveExp(r.Next(nasBlock.expGivenMin, nasBlock.expGivenMax + 1));
            }
            else
            {
                np.Message("Why the hell are you trying to get {0}? It's not even a real block..",
                          Block.GetName(np.p, serverushort));
            }
            nasBlock.existAction?.Invoke(np, nasBlock, false, x, y, z);
            np.p.Level.BlockDB.Cache.Add(np.p, x, y, z, 1 << 0, here, 0);
            np.nl.SetBlock(x, y, z, 0);
            foreach (Player pl in np.p.Level.Players)
            {
                NASEffect.Define(pl, GetBreakID(), NASEffect.breakEffects[(int)nasBlock.material], blockColors[nasBlock.selfID]);
                NASEffect.Spawn(pl, GetBreakID(), NASEffect.breakEffects[(int)nasBlock.material], x, y, z, x, y, z);
            }
            SetBreakID((byte)(GetBreakID() - 1));
            np.justBrokeOrPlaced = true;
            if (!np.hasBeenSpawned)
            {
                np.Message("&chasBeenSpawned is &cfalse&S, this shouldn't happen if you didn't just die.");
                np.Message("&bPlease report to randomstrangers on Discord what you were doing before this happened");
            }
        }
        public static void CancelPlacedBlock(Player p, ushort x, ushort y, ushort z, NASPlayer np, ref bool cancel)
        {
            cancel = true;
            p.RevertBlock(x, y, z);
            if (!np.isDead)
            {
                np.Teleport("-precise ~ ~ ~");
            }
        }
        public static void PlaceBlock(Player p, ushort x, ushort y, ushort z, ushort serverushort, bool placing, ref bool cancel)
        {
            if (p.Level.Config.Deletable && p.Level.Config.Buildable)
            {
                return;
            }
            if (!placing)
            {
                p.Message("&cYou shouldn't be allowed to do this.");
                cancel = true;
                return;
            }
            NASPlayer np = NASPlayer.GetPlayer(p);
            ushort clientushort = np.ConvertBlock(serverushort);
            NASBlock nasBlock = NASBlock.Get(clientushort);
            if (nasBlock.parentID == 0)
            {
                np.Message("You can't place undefined blocks.");
                CancelPlacedBlock(p, x, y, z, np, ref cancel);
                return;
            }
            if ((nasBlock.selfID == 10 || nasBlock.selfID == 476 || nasBlock.selfID == 178) && p.Level.name.Contains("0,0") && !p.Level.name.Contains("nether"))
            {
                if (p.Rank < LevelPermission.Admin)
                {
                    np.Message("&mCan't do that at 0,0.");
                    CancelPlacedBlock(p, x, y, z, np, ref cancel);
                    return;
                }
            }
            if (np.nl.GetBlock(x, y, z + 1) == Block.FromRaw(703) || np.nl.GetBlock(x, y - 1, z) == Block.FromRaw(703))
            {
                np.Message("&mCan't obstruct a bed!");
                CancelPlacedBlock(p, x, y, z, np, ref cancel);
                return;
            }
            if (nasBlock.selfID == 703 && (np.nl.GetBlock(x, y, z - 1) != 0 || np.nl.GetBlock(x, y + 1, z) != 0))
            {
                np.Message("&mCan't place a bed in an obstructed location!");
                CancelPlacedBlock(p, x, y, z, np, ref cancel);
                return;
            }
            int amount = np.inventory.GetAmount(nasBlock.parentID);
            if (amount < 1)
            {
                np.Message("&cYou don't have any {0}.", nasBlock.GetName(np));
                CancelPlacedBlock(p, x, y, z, np, ref cancel);
                return;
            }
            if (amount < nasBlock.resourceCost)
            {
                np.Message("&cYou need at least {0} {1} to place {2}.",
                          nasBlock.resourceCost, nasBlock.GetName(np), nasBlock.GetName(np, clientushort));
                CancelPlacedBlock(p, x, y, z, np, ref cancel);
                return;
            }
            np.inventory.SetAmount(nasBlock.parentID, -nasBlock.resourceCost);
            np.justBrokeOrPlaced = true;
        }
        public static void OnBlockChanged(Player p, ushort x, ushort y, ushort z, ChangeResult _)
        {
            if (p.Level.Config.Deletable && p.Level.Config.Buildable)
            {
                return;
            }
            if (!NASLevel.IsNASLevel(p.Level))
            {
                return;
            }
            NASBlock nasBlock = NASBlock.blocksIndexedByServerushort[p.Level.GetBlock(x, y, z)];
            nasBlock.existAction?.Invoke(NASPlayer.GetPlayer(p), nasBlock, true, x, y, z);
            NASLevel.Get(p.Level.name)?.SimulateSetBlock(x, y, z);
        }
        public static void BreakTask(SchedulerTask task)
        {
            NASBreakInfo breakInfo = (NASBreakInfo)task.State;
            NASPlayer np = breakInfo.np;
            NASBlock nasBlock = breakInfo.nasBlock;
            bool coordsMatch =
                np.breakX == breakInfo.x &&
                np.breakY == breakInfo.y &&
                np.breakZ == breakInfo.z;
            if (coordsMatch &&
                np.breakAttempt == breakInfo.breakAttempt &&
                np.inventory.HeldItem == breakInfo.toolUsed
               )
            {
                if ((np.inventory.GetAmount(696) >= 5) && nasBlock.selfID == 696)
                {
                    np.Message("&mYou have too many lava barrels!");
                    return;
                }
                BreakBlock(np, breakInfo.x, breakInfo.y, breakInfo.z, breakInfo.serverushort, nasBlock);
                double toolDamageChance = 1.0 / (breakInfo.toolUsed.Enchant("Unbreaking") + 1);
                Random r = new();
                if (r.NextDouble() < toolDamageChance && breakInfo.toolUsed.TakeDamage(nasBlock.damageDoneToTool))
                {
                    np.inventory.BreakItem(ref breakInfo.toolUsed);
                }
                np.inventory.UpdateItemDisplay();
                np.lastLeftClickReleaseDate = DateTime.UtcNow;
                np.ResetBreaking();
                return;
            }
        }
        public static void MeterTask(SchedulerTask task)
        {
            NASMeterInfo info = (NASMeterInfo)task.State;
            int millisecs = info.milliseconds;
            millisecs -= 100;
            NASEffect.Define(info.p, 249, NASEffect.breakMeter, new(255,255,255,255), (float)(millisecs / 1000.0f));
            NASEffect.Spawn(info.p, 249, NASEffect.breakMeter, info.x, info.y, info.z, info.x, info.y, info.z);
        }
        public static void HandleLeftClick(Player p, MouseButton _,
            MouseAction action, ushort __, ushort ___, byte ____,
            ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (!p.agreed)
            {
                p.Message("You need to read and agree to the &b/rules&S to play");
                return;
            }
            NASPlayer np = NASPlayer.GetPlayer(p);
            if (action == MouseAction.Released)
            {
                np.ResetBreaking();
                np.lastAirClickDate = null;
                np.lastLeftClickReleaseDate = DateTime.UtcNow;
                NASEffect.UndefineEffect(p, 249);
            }
            if (action == MouseAction.Pressed)
            {
                if (x == ushort.MaxValue ||
                    y == ushort.MaxValue ||
                    z == ushort.MaxValue)
                {
                    np.ResetBreaking();
                    NASEffect.UndefineEffect(p, 249);
                    return;
                }
                ushort serverushort = p.Level.GetBlock(x, y, z),
                    clientushort = np.ConvertBlock(serverushort);
                NASBlock nasBlock = NASBlock.Get(clientushort);
                if (nasBlock.durability == int.MaxValue)
                {
                    np.ResetBreaking();
                    NASEffect.UndefineEffect(p, 249);
                    return;
                }
                if (nasBlock.container != null &&
                    np.nl.blockEntities.ContainsKey(x + " " + y + " " + z) &&
                    (np.nl.blockEntities[x + " " + y + " " + z].drop != null || !np.nl.blockEntities[x + " " + y + " " + z].CanAccess(np))
                   )
                {
                    np.ResetBreaking();
                    NASEffect.UndefineEffect(p, 249);
                    return;
                }
                NASItem heldItem = np.inventory.HeldItem;
                bool toolEffective = false;
                if (heldItem.Prop.materialsEffectiveAgainst != null)
                {
                    foreach (NASMaterial mat in heldItem.Prop.materialsEffectiveAgainst)
                    {
                        if (nasBlock.material == mat)
                        {
                            toolEffective = true;
                            break;
                        }
                    }
                }
                bool canBreakBlock = heldItem.Prop.tier >= nasBlock.tierOfToolNeededToBreak && toolEffective;
                if (nasBlock.tierOfToolNeededToBreak <= 0)
                {
                    canBreakBlock = true;
                }
                if (!canBreakBlock)
                {
                    np.ResetBreaking();
                    NASEffect.UndefineEffect(p, 249);
                    return;
                }
                if (serverushort == 0)
                {
                    if (np.lastAirClickDate == null)
                    {
                        np.lastAirClickDate = DateTime.UtcNow;
                    }
                    np.ResetBreaking();
                    NASEffect.UndefineEffect(p, 249);
                    return;
                }
                if (np.breakX == x && np.breakY == y && np.breakZ == z)
                {
                    return;
                }
                NASEffect.UndefineEffect(p, 249);
                np.breakX = x;
                np.breakY = y;
                np.breakZ = z;
                np.breakAttempt++;
                int millisecs = (nasBlock.durability * 260) - 260;
                if (toolEffective)
                {
                    float multiplier = 1 - heldItem.Prop.percentageOfTimeSaved;
                    switch (heldItem.Enchant("Efficiency"))
                    {
                        case 1:
                            multiplier *= 0.75f;
                            break;
                        case 2:
                            multiplier *= 0.55f;
                            break;
                        case 3:
                            multiplier *= 0.375f;
                            break;
                        case 4:
                            multiplier *= 0.25f;
                            break;
                        case 5:
                            multiplier *= 0.1875f;
                            break;
                    }
                    millisecs = (int)(millisecs * multiplier);
                }
                if (millisecs < 0)
                {
                    millisecs = 0;
                }
                TimeSpan breakTime = TimeSpan.FromMilliseconds(np.SearchItem("helmet").Enchant("Aqua Affinity") == 0 && np.holdingBreath ? millisecs * 8 : millisecs);
                if (np.lastAirClickDate != null)
                {
                    TimeSpan sub = DateTime.UtcNow.Subtract((DateTime)np.lastAirClickDate);
                    breakTime -= sub;
                    millisecs = (int)breakTime.TotalMilliseconds;
                }
                else
                {
                    TimeSpan timeSinceLastBlockBroken = DateTime.UtcNow.Subtract(np.lastLeftClickReleaseDate);
                    PingList pingList = p.Session.Ping;
                    int ping = pingList.AveragePing();
                    if (timeSinceLastBlockBroken.TotalMilliseconds >= ping)
                    {
                        breakTime -= TimeSpan.FromMilliseconds(ping + (ping / 2));
                        millisecs = (int)breakTime.TotalMilliseconds;
                    }
                }
                np.lastAirClickDate = null;
                NASBreakInfo breakInfo = new()
                {
                    np = np,
                    x = x,
                    y = y,
                    z = z,
                    serverushort = serverushort,
                    nasBlock = nasBlock,
                    breakAttempt = np.breakAttempt,
                    toolUsed = heldItem
                };
                SchedulerTask taskBreakBlock;
                taskBreakBlock = breakScheduler.QueueOnce(BreakTask, breakInfo, breakTime);
                NASMeterInfo meterInfo = new()
                {
                    p = p,
                    milliseconds = millisecs,
                    x = x,
                    y = y,
                    z = z
                };
                BlockDefinition def = BlockDefinition.GlobalDefs[Block.FromRaw(clientushort)];
                if (def == null && clientushort < 66)
                {
                    def = DefaultSet.MakeCustomBlock(Block.FromRaw(clientushort));
                }
                if (def != null)
                {
                    if (face == TargetBlockFace.AwayX)
                    {
                        DoOffset(def.MaxX, true, ref meterInfo.x);
                    }
                    if (face == TargetBlockFace.TowardsX)
                    {
                        DoOffset(def.MinX, false, ref meterInfo.x);
                    }
                    if (face == TargetBlockFace.AwayY)
                    {
                        DoOffset(def.MaxZ, true, ref meterInfo.y);
                    }
                    if (face == TargetBlockFace.TowardsY)
                    {
                        DoOffset(def.MinZ, false, ref meterInfo.y);
                    }
                    if (face == TargetBlockFace.AwayZ)
                    {
                        DoOffset(def.MaxY, true, ref meterInfo.z);
                    }
                    if (face == TargetBlockFace.TowardsZ)
                    {
                        DoOffset(def.MinY, false, ref meterInfo.z);
                    }
                }
                p.Extras["NAS_taskDisplayMeter"] = breakScheduler.QueueOnce(MeterTask, meterInfo, TimeSpan.FromMilliseconds(100));
            }
        }
        public static void DoOffset(byte minOrMax, bool positive, ref float coord)
        {
            if (positive)
            {
                coord -= 0.5f;
                coord += (float)(minOrMax / 16.0f);
                coord += 0.125f;
                return;
            }
            coord -= 0.5f;
            coord += (float)(minOrMax / 16.0f);
            coord -= 0.125f;
        }
    }
}
