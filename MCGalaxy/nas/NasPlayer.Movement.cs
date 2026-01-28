#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.Util.Imaging;
using Newtonsoft.Json;
using System;
using System.IO;
namespace NotAwesomeSurvival
{
    public partial class NasPlayer
    {
        [JsonIgnore] public DateTime datePositionCheckingIsAllowed = DateTime.MinValue;
        [JsonIgnore] public bool placePortal = false;
        [JsonIgnore] public int round = 0;
        [JsonIgnore] public bool atBorder = true;
        [JsonIgnore] public TransferInfo transferInfo = null;
        [JsonIgnore]
        public bool CanDoStuffBasedOnPosition
        {
            get
            {
                if (DateTime.UtcNow >= datePositionCheckingIsAllowed)
                {
                    return true;
                }
                return false;
            }
            set
            {
                PingList pingList = p.Session.Ping;
                if (!value)
                {
                    datePositionCheckingIsAllowed = DateTime.UtcNow.AddMilliseconds(2000 + pingList.HighestPing());
                }
            }
        }
        public static void Register()
        {
            OnPlayerSpawningEvent.Register(OnPlayerSpawning, Priority.High);
        }
        public static void Unregister()
        {
            OnPlayerSpawningEvent.Unregister(OnPlayerSpawning);
        }
        public static void OnPlayerSpawning(Player p, ref Position pos, ref byte yaw, ref byte pitch, bool respawning)
        {
            NasPlayer np = GetNasPlayer(p);
            np.nl = NasLevel.Get(p.Level.name);
            np.SpawnPlayer(p.Level, ref pos, ref yaw, ref pitch);
        }
        public ushort ConvertBlock(ushort block)
        {
            ushort raw;
            if (block >= 256)
            {
                raw = Nas.ToRaw(block);
            }
            else
            {
                raw = Nas.Convert(block);
                if (raw >= 66)
                {
                    raw = 22;
                }
            }
            if (raw > 767)
            {
                raw = p.Level.GetFallback(block);
            }
            if (!p.Session.hasBlockDefs && raw < 66)
            {
                BlockDefinition def = p.Level.CustomBlockDefs[raw];
                if (def != null)
                {
                    raw = def.FallBack;
                }
            }
            if (!p.Session.hasCustomBlocks)
            {
                raw = fallback[(byte)raw];
            }
            return raw;
        }
        public void SaveStatsTask(SchedulerTask _)
        {
            Save();
        }
        public void Save()
        {
            if (this != null)
            {
                string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
                FileUtils.TryWriteAllText(Nas.GetSavePath(p), jsonString);
                FileUtils.TryWriteAllText(Nas.GetTextPath(p), jsonString);
            }
        }
        public void SpawnPlayer(Level level, ref Position spawnPos, ref byte yaw, ref byte pitch)
        {
            if (!NasLevel.IsNasLevel(level))
            {
                return;
            }
            CanDoStuffBasedOnPosition = false;
            inventory.Setup();
            if (isDead)
            {
                if (!headingToBed)
                {
                    TryDropGravestone();
                    inventory = new(p);
                    exp = 0;
                    levels = 0;
                    inventory.Setup();
                    inventory.DisplayHeldBlock(NasBlock.Default, 0);
                }
                CommandData data = p.DefaultCmdData;
                data.Context = CommandContext.SendCmd;
                Orientation rot = new(Server.mainLevel.rotx, Server.mainLevel.roty);
                SetLocation(this, spawnMap, spawnCoords, rot);
                Log("Teleporting {0} to {1} bed!", p.truename, p.pronouns.Object);
                if (!headingToBed)
                {
                    SendCpeMessage(CpeMessageType.Announcement, "&cY O U  D I E D");
                    Chat.MessageChat(p, reason, null, true);
                    curFogColor = new(0, 0, 0, 255);
                    curRenderDistance = 1;
                    HP = maxHP;
                    Air = maxAir;
                    holdingBreath = false;
                    DisplayHealth();
                }
                headingToBed = false;
                isDead = false;
            }
            if (!hasBeenSpawned)
            {
                SpawnPlayerFirstTime(level, ref spawnPos, ref yaw, ref pitch);
                return;
            }
            if (transferInfo != null)
            {
                if (transferInfo.travelX == -1)
                {
                    transferInfo.CalcNewPos();
                    spawnPos = transferInfo.posBeforeMapChange;
                    spawnPos.X = spawnPos.BlockX * 32 + 16;
                    spawnPos.Z = spawnPos.BlockZ * 32 + 16;
                }
                else
                {
                    spawnPos = transferInfo.posBeforeMapChange;
                    spawnPos.X = transferInfo.travelX * 32 + 16;
                    spawnPos.Y = transferInfo.travelY * 32 + 51;
                    spawnPos.Z = transferInfo.travelZ * 32 + 16;
                    if (placePortal)
                    {
                        int orX = transferInfo.travelX,
                            orY = transferInfo.travelY,
                            orZ = transferInfo.travelZ;
                        SetSafetyBlock(orX, orY - 1, orZ, Nas.FromRaw(162));
                        SetSafetyBlock(orX, orY + 2, orZ, Nas.FromRaw(162));
                        ushort temp = nl.GetBlock(orX, orY + 1, orZ);
                        if (temp != 0 && !nl.blockEntities.ContainsKey(orX + " " + (orY + 1) + " " + orZ))
                        {
                            nl.SetBlock(orX, orY + 1, orZ, 0);
                            nl.lvl.BlockDB.Cache.Add(p, (ushort)orX, (ushort)orY, (ushort)orZ, 1 << 2, temp, 0);
                        }
                        temp = nl.GetBlock(orX, orY, orZ);
                        if (temp != Nas.FromRaw(457) && !nl.blockEntities.ContainsKey(orX + " " + orY + " " + orZ))
                        {
                            nl.SetBlock(orX, orY, orZ, Nas.FromRaw(457));
                            nl.lvl.BlockDB.Cache.Add(p, (ushort)orX, (ushort)orY, (ushort)orZ, 1 << 2, temp, Nas.FromRaw(457));
                        }
                        placePortal = false;
                    }
                }
                yaw = transferInfo.yawBeforeMapChange;
                pitch = transferInfo.pitchBeforeMapChange;
                atBorder = true;
                transferInfo = null;
            }
        }
        public void SetSafetyBlock(int x, int y, int z, ushort block)
        {
            ushort oldBlock = nl.GetBlock(x, y, z);
            if (nl.blockEntities.ContainsKey(x + " " + y + " " + z))
            {
                return;
            }
            if (NasBlock.Get(Collision.ConvertToClientushort(oldBlock)).collideAction != NasBlock.DefaultSolidCollideAction())
            {
                nl.SetBlock(x, y, z, block);
                nl.lvl.BlockDB.Cache.Add(p, (ushort)x, (ushort)y, (ushort)z, 1 << 2, oldBlock, block);
            }
        }
        public void SetModel()
        {
            p.UpdateModel("human|0.93023255813953488372093023255814");
            Server.models.Update(p.name, "human|0.93023255813953488372093023255814");
            Server.models.Save();
        }
        public void SpawnPlayerFirstTime(Level level, ref Position spawnPos, ref byte yaw, ref byte pitch)
        {
            if (hasBeenSpawned)
            {
                return;
            }
            atBorder = true;
            if (!p.Model.Contains("|0.93023255813953488372093023255814"))
            {
                SetModel();
            }
            spawnPos = new(location.X, location.Y, location.Z);
            yaw = this.yaw;
            pitch = this.pitch;
            Log("Teleporting {0}!", p.truename);
            if (level.name != levelName)
            {
                Log("{0}: trying to use /goto to move to the map they logged out in", p.truename);
                CommandData data = p.DefaultCmdData;
                data.Context = CommandContext.SendCmd;
                p.HandleCommand("goto", levelName, data);
                return;
            }
            hasBeenSpawned = true;
            Log("{0}: hasBeenSpawned set to {1}", p.truename, hasBeenSpawned);
        }
        public void DoMovement(Position next, byte _, byte __)
        {
            UpdateHeldBlock();
            if (CanDoStuffBasedOnPosition)
            {
                UpdateAir();
            }
            CheckMapCrossing(p.Pos);
            if (CanDoStuffBasedOnPosition)
            {
                DoNasBlockCollideActions(next);
            }
            if (CanDoStuffBasedOnPosition)
            {
                UpdatePosition(p.Pos, p.Level.name);
            }
            CheckGround(p.Pos);
            UpdateCaveFog(next);
            round++;
        }
        public void UpdatePosition(Position pos, string level)
        {
            location = new(pos.X, pos.Y, pos.Z);
            levelName = level;
        }
        public void CheckGround(Position next)
        {
            if (p.invincible)
            {
                lastGroundedLocation = new(next.X, next.Y, next.Z);
                return;
            }
            Position below = next;
            below.Y -= 2;
            if (Collision.TouchesGround(p.Level, bounds, below, out float fallDamageMultiplier))
            {
                float fallHeight = lastGroundedLocation.Y - next.Y;
                if (!CanDoStuffBasedOnPosition && fallHeight > 0 && !hasBeenSpawned)
                {
                    Message("&WTrying to take fall damage but can't.");
                }
                if (fallHeight > 0 && CanDoStuffBasedOnPosition)
                {
                    fallHeight /= 32f;
                    fallHeight -= 4;
                    if (fallHeight > 0)
                    {
                        float damage = (int)fallHeight * 2;
                        damage /= 4;
                        TakeDamage(damage * fallDamageMultiplier, DamageSource.Falling);
                    }
                }
                lastGroundedLocation = new(next.X, next.Y, next.Z);
            }
        }
        public void CheckMapCrossing(Position next)
        {
            if (next.BlockX <= 0)
            {
                TryGoMapAt(-1, 0);
                return;
            }
            if (next.BlockX >= p.Level.Width - 1)
            {
                TryGoMapAt(1, 0);
                return;
            }
            if (next.BlockZ <= 0)
            {
                TryGoMapAt(0, -1);
                return;
            }
            if (next.BlockZ >= p.Level.Length - 1)
            {
                TryGoMapAt(0, 1);
                return;
            }
            atBorder = false;
        }
        public bool TryGoMapAt(int dirX, int dirZ)
        {
            if (atBorder)
            {
                return false;
            }
            atBorder = true;
            int chunkOffsetX = 0, chunkOffsetZ = 0;
            string seed = "DEFAULT";
            if (!NasGen.GetSeedAndChunkOffset(p.Level.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ))
            {
                return false;
            }
            string mapName;
            chunkOffsetX += dirX;
            chunkOffsetZ += dirZ;
            mapName = seed + "_" + chunkOffsetX + "," + chunkOffsetZ;
            if (File.Exists(NasLevel.GetFileName(mapName)))
            {
                transferInfo = new(p, dirX, dirZ, -1, -1, -1);
                CommandData data = p.DefaultCmdData;
                data.Context = CommandContext.SendCmd;
                p.HandleCommand("goto", mapName, data);
                return true;
            }
            else
            {
                if (NasGen.currentlyGenerating)
                {
                    Message("&cA map is already generating!");
                    return false;
                }
                GenInfo info = new()
                {
                    p = p,
                    mapName = mapName,
                    seed = seed
                };
                SchedulerTask taskGenMap;
                taskGenMap = NasGen.genScheduler.QueueOnce(GenTask, info, new TimeSpan(0, 0, 5));
                return false;
            }
        }
        public bool NetherTravel(string map, TransferInfo trans)
        {
            if (atBorder)
            {
                return false;
            }
            atBorder = true;
            int chunkOffsetX = 0, chunkOffsetZ = 0;
            string seed = "DEFAULT";
            if (!NasGen.GetSeedAndChunkOffset(p.Level.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ)) 
            { 
                return false; 
            }
            string mapName;
            mapName = map;
            NasGen.GetSeedAndChunkOffset(map, ref seed, ref chunkOffsetX, ref chunkOffsetZ);
            if (File.Exists(NasLevel.GetFileName(mapName)))
            {
                transferInfo = trans;
                placePortal = true;
                CommandData data = p.DefaultCmdData;
                data.Context = CommandContext.SendCmd;
                p.HandleCommand("goto", mapName, data);
                return true;
            }
            else
            {
                if (NasGen.currentlyGenerating)
                {
                    Message("&cA map is already generating!");
                    return false;
                }
                GenInfo info = new()
                {
                    p = p,
                    mapName = mapName,
                    seed = seed
                };
                SchedulerTask taskGenMap;
                taskGenMap = NasGen.genScheduler.QueueOnce(GenTask, info, new TimeSpan(0, 0, 5));
                return false;
            }
        }
        public class GenInfo
        {
            public Player p;
            public string mapName, seed;
        }
        public static void GenTask(SchedulerTask task)
        {
            GenInfo info = (GenInfo)task.State;
            info.p.Message("Seed is {0}", info.seed);
            GenMap(info);
        }
        public static void GenMap(GenInfo info)
        {
            Level lvl = null;
            try
            {
                string width = NasGen.mapWideness.ToString(),
                    height = NasGen.mapTallness.ToString(),
                    length = NasGen.mapWideness.ToString();
                lvl = NasLevel.GenerateMap(info.p, info.mapName, width, height, length, info.seed);
                if (lvl == null)
                {
                    return;
                }
                lvl.Save(true);
            }
            finally
            {
                lvl?.Dispose();
                Server.DoGC();
            }
        }
        public class TransferInfo
        {
            [JsonIgnore] public Position posBeforeMapChange;
            [JsonIgnore] public byte yawBeforeMapChange, pitchBeforeMapChange;
            [JsonIgnore]
            public int chunkOffsetX, chunkOffsetZ,
                travelX = -1, travelY = -1, travelZ = -1;
            public TransferInfo(Player p, int chunkOffsetX, int chunkOffsetZ, int x, int y, int z)
            {
                posBeforeMapChange = p.Pos;
                yawBeforeMapChange = p.Rot.RotY;
                pitchBeforeMapChange = p.Rot.HeadX;
                this.chunkOffsetX = chunkOffsetX;
                this.chunkOffsetZ = chunkOffsetZ;
                travelX = x;
                travelZ = z;
                travelY = y;
            }
            public void CalcNewPos()
            {
                int xOffset = chunkOffsetX * NasGen.mapWideness * 32,
                    zOffset = chunkOffsetZ * NasGen.mapWideness * 32;
                posBeforeMapChange.X -= xOffset;
                posBeforeMapChange.Z -= zOffset;
            }
        }
        public void UpdateCaveFog(Position next)
        {
            if (!NasLevel.all.ContainsKey(p.Level.name))
            {
                return;
            }
            if (curRenderDistance > targetRenderDistance)
            {
                curRenderDistance *= 1 - 0.03125f;
                if (curRenderDistance < targetRenderDistance)
                {
                    curRenderDistance = targetRenderDistance;
                }
            }
            else if (curRenderDistance < targetRenderDistance)
            {
                curRenderDistance *= 1 + 0.03125f;
                if (curRenderDistance > targetRenderDistance)
                {
                    curRenderDistance = targetRenderDistance;
                }
            }
            curFogColor = ScaleColor(curFogColor, targetFogColor);
            Send(Packet.EnvMapProperty(EnvProp.MaxFog, (int)curRenderDistance));
            Send(Packet.EnvColor(2, curFogColor.R, curFogColor.G, curFogColor.B));
            NasLevel nl = NasLevel.all[p.Level.name];
            int z = next.BlockZ;
            z = Utils.Clamp(z, 0, (ushort)(p.Level.Length - 1));
            ushort height = (ushort)Utils.Clamp(z, 0, (ushort)(p.Level.Height - 1));
            if (next.BlockCoords == p.Pos.BlockCoords)
            {
                return;
            }
            if (height < NasGen.oceanHeight)
            {
                height = NasGen.oceanHeight;
            }
            int distanceBelow = nl.biome < 0 ? 0 : height - next.BlockY, expFog;
            if (distanceBelow >= NasGen.diamondDepth)
            {
                targetRenderDistance = 128;
                targetFogColor = NasGen.diamondFogColor;
                expFog = 1;
            }
            else if (distanceBelow >= NasGen.goldDepth)
            {
                targetRenderDistance = 192;
                targetFogColor = NasGen.goldFogColor;
                expFog = 1;
            }
            else if (distanceBelow >= NasGen.ironDepth)
            {
                targetRenderDistance = 192;
                targetFogColor = NasGen.ironFogColor;
                expFog = 1;
            }
            else if (distanceBelow >= NasGen.coalDepth)
            {
                targetRenderDistance = 256;
                targetFogColor = NasGen.coalFogColor;
                expFog = 1;
            }
            else
            {
                targetRenderDistance = Server.Config.MaxFogDistance;
                targetFogColor = new(255, 255, 255, 255);
                expFog = 0;
            }
            Send(Packet.EnvMapProperty(EnvProp.ExpFog, expFog));
        }
        public static Pixel ScaleColor(Pixel cur, Pixel goal)
        {
            byte R = ScaleChannel(cur.R, goal.R),
                G = ScaleChannel(cur.G, goal.G),
                B = ScaleChannel(cur.B, goal.B);
            return new(R, G, B, 255);
        }
        public static byte ScaleChannel(byte curChannel, byte goalChannel)
        {
            if (curChannel > goalChannel)
            {
                curChannel--;
            }
            else if (curChannel < goalChannel)
            {
                curChannel++;
            }
            return curChannel;
        }
    }
}
#endif