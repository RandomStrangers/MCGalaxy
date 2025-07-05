#if NAS && TEN_BIT_BLOCKS
using System;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.DB;
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
        /*
        [JsonIgnore] public SchedulerTask SetPrefixTask;
        [JsonIgnore] public Scheduler SetPrefixScheduler;
        public void SetPrefix(SchedulerTask task)
        {
            SetPrefix(p);
        }
        public static void SetPrefix(Player p)
        {
            System.Collections.Generic.List<string> prefixes = new System.Collections.Generic.List<string>();
            MCGalaxy.Games.Team team = p.Game.Team;
            MCGalaxy.Games.IGame game = MCGalaxy.Games.IGame.GameOn(p.level);
            prefixes.Add(p.Game.Referee ? "&2[Ref] " : "");
            prefixes.Add(p.GroupPrefix.Length > 0 ? p.GroupPrefix + p.color : "");
            prefixes.Add(team == null ? "" : "<" + team.Color + team.Name + p.color + "> ");
            prefixes.Add(game == null ? "" : game.GetPrefix(p));
            bool devPrefix = Server.Config.SoftwareStaffPrefixes &&
                             Server.Devs.CaselessContains(p.truename);
            prefixes.Add(devPrefix ? MakeTitle(p, "Dev", "&9") : "");
            prefixes.Add(p.title.Length > 0 ? MakeTitle(p, p.title, p.titlecolor) : "");
            bool NASDevPrefix = Server.Config.SoftwareStaffPrefixes &&
                             Nas.Devs.CaselessContains(p.truename);
            prefixes.Add(NASDevPrefix ? MakeTitle(p, "NASDev", "&a") : "");
            p.prefix = prefixes.Join("");
            OnSettingPrefixEvent.Call(p, prefixes);
        }
        public static string MakeTitle(Player p, string title, string titleCol)
        {
            return p.color + "[" + titleCol + title + p.color + "] ";
        }
        public static Scheduler savingScheduler;
        public static SchedulerTask SaveTask;
        public static void Setup()
        {
            if (savingScheduler == null)
            {
                savingScheduler = new Scheduler("SavingScheduler");
            }
            SaveTask = savingScheduler.QueueRepeat(SaveAll, null, TimeSpan.FromSeconds(5));
        }
        public static void TakeDown()
        {
            savingScheduler.Cancel(SaveTask);
        }
        public static void SaveAll(SchedulerTask task)
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player player in players)
            {
                SaveAction(player);
            }
        }
        public static void SaveAction(Player p)
        {
            NasPlayer np = GetNasPlayer(p);
            if (np != null)
            {
                string jsonString = JsonConvert.SerializeObject(np, Formatting.Indented);
                File.WriteAllText(Nas.GetSavePath(p), jsonString);
                File.WriteAllText(Nas.GetTextPath(p), jsonString);
            }
        }*/
        public static void Register()
        {
            //Setup();
            OnPlayerSpawningEvent.Register(OnPlayerSpawning, Priority.High);
        }
        public static void Unregister()
        {
            //TakeDown();
            OnPlayerSpawningEvent.Unregister(OnPlayerSpawning);
        }
        public static void OnPlayerSpawning(Player p, ref Position pos, ref byte yaw, ref byte pitch, bool respawning)
        {
            NasPlayer np = GetNasPlayer(p);
            np.nl = NasLevel.Get(p.level.name);
            np.SpawnPlayer(p.level, ref pos, ref yaw, ref pitch);
        }
        /// <summary> Converts the given block ID into a raw block ID that can be sent to this player </summary>
        public ushort ConvertBlock(ushort block)
        {
            ushort raw;
            if (block >= Block.Extended)
            {
                raw = Block.ToRaw(block);
            }
            else
            {
                raw = Block.Convert(block);
                // show invalid physics blocks as Orange
                if (raw >= Block.CPE_COUNT)
                {
                    raw = Block.Orange;
                }
            }
            if (raw > Block.MaxRaw)
            {
                raw = p.level.GetFallback(block);
            }
            // Check if a custom block replaced a core block
            //  If so, assume fallback is the better block to display
            if (!p.Session.hasBlockDefs && raw < Block.CPE_COUNT)
            {
                BlockDefinition def = p.level.CustomBlockDefs[raw];
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
        public void SaveStatsTask(SchedulerTask task)
        {
            Save();
        }
        public void Save()
        {
            if (this != null)
            {
                string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(Nas.GetSavePath(p), jsonString);
                File.WriteAllText(Nas.GetTextPath(p), jsonString);
            }
        }
        public void SpawnPlayer(Level level, ref Position spawnPos, ref byte yaw, ref byte pitch)
        {
            if (!NasLevel.IsNasLevel(level)) 
            {
                return; 
            } //not a nas map
            CanDoStuffBasedOnPosition = false;
            inventory.Setup();
            if (isDead)
            {
                if (!headingToBed)
                {
                    TryDropGravestone();
                    inventory = new Inventory(p);
                    exp = 0;
                    levels = 0;
                    inventory.Setup();
                    inventory.DisplayHeldBlock(NasBlock.Default, 0);
                }
                CommandData data = p.DefaultCmdData;
                data.Context = CommandContext.SendCmd;
                Orientation rot = new Orientation(Server.mainLevel.rotx, Server.mainLevel.roty);
                SetLocation(this, spawnMap, spawnCoords, rot);
                Log("Teleporting {0} to {1} bed!", p.truename, p.pronouns.Object);
                if (!headingToBed)
                {
                    SendCpeMessage(CpeMessageType.Announcement, "&cY O U  D I E D");
                    Chat.MessageChat(p, reason, null, true);
                    curFogColor = Color.Black;
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
                        int orX = transferInfo.travelX;
                        int orY = transferInfo.travelY;
                        int orZ = transferInfo.travelZ;
                        SetSafetyBlock(orX, orY - 1, orZ, Block.FromRaw(162));
                        SetSafetyBlock(orX, orY + 2, orZ, Block.FromRaw(162));
                        ushort temp = nl.GetBlock(orX, orY + 1, orZ);
                        if (temp != Block.Air && !nl.blockEntities.ContainsKey(orX + " " + (orY + 1) + " " + orZ))
                        {
                            nl.SetBlock(orX, orY + 1, orZ, Block.Air);
                            nl.lvl.BlockDB.Cache.Add(p, (ushort)orX, (ushort)orY, (ushort)orZ, BlockDBFlags.Drawn, temp, Block.Air);
                        }
                        temp = nl.GetBlock(orX, orY, orZ);
                        if (temp != Block.FromRaw(457) && !nl.blockEntities.ContainsKey(orX + " " + orY + " " + orZ))
                        {
                            nl.SetBlock(orX, orY, orZ, Block.FromRaw(457));
                            nl.lvl.BlockDB.Cache.Add(p, (ushort)orX, (ushort)orY, (ushort)orZ, BlockDBFlags.Drawn, temp, Block.FromRaw(457));
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
                nl.lvl.BlockDB.Cache.Add(p, (ushort)x, (ushort)y, (ushort)z, BlockDBFlags.Drawn, oldBlock, block);
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
            spawnPos = new Position(location.X, location.Y, location.Z);
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
        public void UpdateEnv()
        {
            p.level.Config.SkyColor = NasTimeCycle.globalSkyColor;
            p.level.Config.CloudColor = NasTimeCycle.globalCloudColor;
            p.level.Config.LightColor = NasTimeCycle.globalSunColor;
        }
        public void DoMovement(Position next, byte yaw, byte pitch)
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
                UpdatePosition(p.Pos, p.level.name); 
            }
            CheckGround(p.Pos);
            UpdateCaveFog(next);
            round++;
        }
        public void UpdatePosition(Position pos, string level)
        {
            location = new MCGalaxy.Maths.Vec3S32(pos.X, pos.Y, pos.Z);
            levelName = level;
        }
        public void CheckGround(Position next)
        {
            if (p.invincible) 
            { 
                lastGroundedLocation = new MCGalaxy.Maths.Vec3S32(next.X, next.Y, next.Z); 
                return;
            }
            Position below = next;
            below.Y -= 2;
            if (Collision.TouchesGround(p.level, bounds, below, out float fallDamageMultiplier))
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
                lastGroundedLocation = new MCGalaxy.Maths.Vec3S32(next.X, next.Y, next.Z);
            }
        }
        public void CheckMapCrossing(Position next)
        {
            if (next.BlockX <= 0)
            {
                TryGoMapAt(-1, 0);
                return;
            }
            if (next.BlockX >= p.level.Width - 1)
            {
                TryGoMapAt(1, 0);
                return;
            }
            if (next.BlockZ <= 0)
            {
                TryGoMapAt(0, -1);
                return;
            }
            if (next.BlockZ >= p.level.Length - 1)
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
            if (!NasGen.GetSeedAndChunkOffset(p.level.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ)) 
            { 
                return false;
            }
            string mapName;
            chunkOffsetX += dirX;
            chunkOffsetZ += dirZ;
            mapName = seed + "_" + chunkOffsetX + "," + chunkOffsetZ;
            if (File.Exists(NasLevel.GetFileName(mapName)))
            {
                transferInfo = new TransferInfo(p, dirX, dirZ, -1, -1, -1);
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
                GenInfo info = new GenInfo
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
            if (!NasGen.GetSeedAndChunkOffset(p.level.name, ref seed, ref chunkOffsetX, ref chunkOffsetZ)) { return false; }
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
                GenInfo info = new GenInfo
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
            public string mapName;
            public string seed;
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
                string width = NasGen.mapWideness.ToString();
                string height = NasGen.mapTallness.ToString();
                string length = NasGen.mapWideness.ToString();
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
            [JsonIgnore] public byte yawBeforeMapChange;
            [JsonIgnore] public byte pitchBeforeMapChange;
            [JsonIgnore] public int chunkOffsetX, chunkOffsetZ;
            [JsonIgnore] public int travelX = -1, travelY = -1, travelZ = -1;
            public TransferInfo(Player p, int chunkOffsetX, int chunkOffsetZ)
            {
                posBeforeMapChange = p.Pos;
                yawBeforeMapChange = p.Rot.RotY;
                pitchBeforeMapChange = p.Rot.HeadX;
                this.chunkOffsetX = chunkOffsetX;
                this.chunkOffsetZ = chunkOffsetZ;
                travelX = -1;
                travelY = -1;
                travelZ = -1;
            }
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
                //* 32 because its in player units
                int xOffset = chunkOffsetX * NasGen.mapWideness * 32;
                int zOffset = chunkOffsetZ * NasGen.mapWideness * 32;
                posBeforeMapChange.X -= xOffset;
                posBeforeMapChange.Z -= zOffset;
            }
        }
        public void UpdateCaveFog(Position next)
        {
            if (!NasLevel.all.ContainsKey(p.level.name)) 
            { 
                return; 
            }
            const float change = 0.03125f;//0.03125f;
            if (curRenderDistance > targetRenderDistance)
            {
                curRenderDistance *= 1 - change;
                if (curRenderDistance < targetRenderDistance) 
                {
                    curRenderDistance = targetRenderDistance; 
                }
            }
            else if (curRenderDistance < targetRenderDistance)
            {
                curRenderDistance *= 1 + change;
                if (curRenderDistance > targetRenderDistance) 
                { 
                    curRenderDistance = targetRenderDistance; 
                }
            }
            curFogColor = ScaleColor(curFogColor, targetFogColor);
            Send(Packet.EnvMapProperty(EnvProp.MaxFog, (int)curRenderDistance));
            Send(Packet.EnvColor(2, curFogColor.R, curFogColor.G, curFogColor.B));
            NasLevel nl = NasLevel.all[p.level.name];
            int z = next.BlockZ;
            z = Utils.Clamp(z, 0, (ushort)(p.level.Length - 1));
            ushort height = (ushort)Utils.Clamp(z, 0, (ushort)(p.level.Height - 1));
            if (next.BlockCoords == p.Pos.BlockCoords) 
            { 
                return; 
            }
            if (height < NasGen.oceanHeight) 
            { 
                height = NasGen.oceanHeight;
            }
            int distanceBelow = nl.biome < 0 ? 0 : height - next.BlockY;
            int expFog;
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
                targetFogColor = Color.White;
                expFog = 0;
            }
            Send(Packet.EnvMapProperty(EnvProp.ExpFog, expFog));
        }
        public static Color ScaleColor(Color cur, Color goal)
        {
            byte R = ScaleChannel(cur.R, goal.R);
            byte G = ScaleChannel(cur.G, goal.G);
            byte B = ScaleChannel(cur.B, goal.B);
            return Color.FromArgb(R, G, B);
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