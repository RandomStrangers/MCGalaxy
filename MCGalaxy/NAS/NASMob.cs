using System;
using System.Collections.Generic;
using System.IO;
using MCGalaxy.Bots;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
namespace MCGalaxy
{
    public struct NASCoords
    {
        public int X, Y, Z;
        public byte RotX, RotY;
    }
    public class NASMob
    {
        public static BotInstruction NAShostile, NASroam;
        public static SchedulerTask mobSpawningTask;
        public static readonly Random rnd = new();
        public static PlayerBot[] GetMobsInLevel(Level lvl)
        {
            List<PlayerBot> players = new();
            foreach (PlayerBot bot in lvl.Bots.Items)
            {
                if (bot.DisplayName != "" || !bot.name.Contains("NASMob"))
                    continue;
                players.Add(bot);
            }
            return players.ToArray();
        }
        public static void CheckDespawn(Level level)
        {
            foreach (PlayerBot bot in level.Bots.Items)
            {
                if (bot.DisplayName != "" || !bot.name.Contains("NASMob"))
                    continue;
                if (GetPlayersInLevel(level).Count < 1)
                {
                    PlayerBot.Remove(bot);
                    continue;
                }
                if (bot.AIName == "NASHostile" && NASTimeCycle.globalCurrentDayCycle == NASDayCycles.Day)
                {
                    mobHealth[bot] = mobHealth[bot] - 10;
                    if (mobHealth[bot] <= 0)
                    {
                        mobHealth.Remove(bot);
                        PlayerBot.Remove(bot);
                    }
                    PlayerBot.Remove(bot);
                    continue;
                }
                ushort gb = level.FastGetBlock((ushort)bot.Pos.BlockX, (ushort)(bot.Pos.BlockY + 1), (ushort)bot.Pos.BlockZ);
                switch (gb)
                {
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                        PlayerBot.Remove(bot);
                        continue;
                }
                int shortestDist = 650;
                foreach (Player p in GetPlayersInLevel(level))
                {
                    int x = bot.Pos.BlockX,
                        y = bot.Pos.BlockY,
                        z = bot.Pos.BlockZ,
                        dx = p.Pos.BlockX - x, dy = p.Pos.BlockY - y, dz = p.Pos.BlockZ - z,
                        playerDist = Math.Abs(dx) + Math.Abs(dz);
                    if (playerDist < shortestDist)
                        shortestDist = playerDist;
                }
                if (shortestDist >= 210)
                    PlayerBot.Remove(bot);
            }
        }
        public static List<Player> GetPlayersInLevel(Level lvl)
        {
            List<Player> players = new();
            foreach (Player p in PlayerInfo.Online.Items)
            {
                if (p.Level == lvl)
                    players.Add(p);
            }
            return players;
        }
        public static void HandleMobSpawning(SchedulerTask task)
        {
            mobSpawningTask = task;
            Level[] levels = LevelInfo.Loaded.Items;
            if (PlayerInfo.Online.Items.Length < 1)
            {
                foreach (Level lvl in levels)
                    CheckDespawn(lvl);
                return;
            }
            foreach (Level lvl in levels)
            {
                CheckDespawn(lvl);
                List<Player> players = GetPlayersInLevel(lvl);
                if (GetMobsInLevel(lvl).Length >= (12 * players.Count))
                    continue;
                Player selectedPlayer = players[rnd.Next(players.Count)];
                if (selectedPlayer == null)
                    continue;
                ushort x = (ushort)(selectedPlayer.Pos.BlockX + (rnd.Next(25, 128) * (rnd.Next(2) == 1 ? 1 : -1))),
                    z = (ushort)(selectedPlayer.Pos.BlockZ + (rnd.Next(25, 128) * (rnd.Next(2) == 1 ? 1 : -1)));
                if (x >= lvl.Width)
                    x = (ushort)(lvl.Width - 1);
                if (z >= lvl.Length)
                    z = (ushort)(lvl.Length - 1);
                if (x < 0)
                    x = 0;
                if (z < 0)
                    z = 0;
                ushort y = FindGround(lvl, x, lvl.Height, z);
                if (y < 0)
                    y = 0;
                if (y > 1)
                {
                    ushort gb = lvl.FastGetBlock(x, (ushort)(y - 1), z);
                    switch (gb)
                    {
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            continue;
                    }
                }
                switch (NASTimeCycle.globalCurrentDayCycle)
                {
                    case NASDayCycles.Night:
                        switch (rnd.Next(7))
                        {
                            case 1:
                            case 2:
                                SpawnEntity(lvl, "zombie", "NASHostile", x, y, z);
                                break;
                            case 3:
                            case 4:
                                SpawnEntity(lvl, "spider", "NASHostile", x, y, z);
                                break;
                            case 5:
                            case 6:
                                SpawnEntity(lvl, "skeleton", "NASHostile", x, y, z);
                                break;
                            default:
                                break;
                        }
                        break;
                    case NASDayCycles.Midnight:
                        switch (rnd.Next(8))
                        {
                            case 1:
                            case 2:
                            case 3:
                                SpawnEntity(lvl, "zombie", "NASHostile", x, y, z);
                                break;
                            case 4:
                                SpawnEntity(lvl, "spider", "NASHostile", x, y, z);
                                break;
                            case 5:
                            case 6:
                            case 7:
                                SpawnEntity(lvl, "skeleton", "NASHostile", x, y, z);
                                break;
                            default:
                                break;
                        }
                        break;
                    case NASDayCycles.Sunrise:
                        switch (rnd.Next(5))
                        {
                            case 1:
                                SpawnEntity(lvl, "sheep", "NASRoam", x, y, z);
                                break;
                            case 2:
                                SpawnEntity(lvl, "chicken", "NASRoam", x, y, z);
                                break;
                            case 3:
                                SpawnEntity(lvl, "spider", "NASHostile", x, y, z);
                                break;
                            case 4:
                                SpawnEntity(lvl, "pig", "NASRoam", x, y, z);
                                break;
                            default:
                                break;
                        }
                        break;
                    case NASDayCycles.Sunset:
                        switch (rnd.Next(4))
                        {
                            case 1:
                                SpawnEntity(lvl, "zombie", "NASHostile", x, y, z);
                                break;
                            case 2:
                                SpawnEntity(lvl, "sheep", "NASRoam", x, y, z);
                                break;
                            case 3:
                                SpawnEntity(lvl, "spider", "NASHostile", x, y, z);
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        switch (rnd.Next(7))
                        {
                            case 1:
                            case 2:
                                SpawnEntity(lvl, "sheep", "NASRoam", x, y, z);
                                break;
                            case 3:
                            case 4:
                                SpawnEntity(lvl, "pig", "NASRoam", x, y, z);
                                break;
                            case 5:
                            case 6:
                                SpawnEntity(lvl, "chicken", "NASRoam", x, y, z);
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
        }
        public static bool CanHitMob(Player p, PlayerBot victim) => (p.Pos.ToVec3F32() - victim.Pos.ToVec3F32()).LengthSquared switch
        {
            > 12f + 1 => false,
            _ => true
        };
        public static readonly Dictionary<PlayerBot, float> mobHealth = new();
        public static void HandleAttackMob(Player p)
        {
            PlayerBot mob = null;
            float bestDist = float.MaxValue;
            foreach (PlayerBot b in p.Level.Bots.Items)
            {
                if (!CanHitMob(p, b)) continue;
                float dist = (p.Pos.ToVec3F32() - b.Pos.ToVec3F32()).LengthSquared;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    mob = b;
                }
            }
            if (mob == null)
                return;
            if (!mobHealth.ContainsKey(mob))
                switch (mob.Model)
                {
                    case "zombie":
                        mobHealth.Add(mob, 10);
                        break;
                    case "skeleton":
                        mobHealth.Add(mob, 15);
                        break;
                    case "spider":
                        mobHealth.Add(mob, 7);
                        break;
                    default:
                        mobHealth.Add(mob, 5);
                        break;
                }
            NASPlayer np = NASPlayer.GetPlayer(p);
            float added = 0;
            if (np.inventory.HeldItem.Enchant("Sharpness") != 0)
                added += 1;
            added += np.inventory.HeldItem.Enchant("Sharpness") * 0.5f;
            mobHealth[mob] = mobHealth[mob] - (np.inventory.HeldItem.Prop.damage + added);
            if (mobHealth[mob] <= 0)
            {
                mobHealth.Remove(mob);
                PlayerBot.Remove(mob);
            }
        }
        public static void HandleBlockClicked(Player p, MouseButton button, MouseAction action, ushort yaw, ushort pitch, byte entity, ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (button == MouseButton.Left)
                try
                {
                    HandleAttackMob(p);
                }
                catch
                {
                }
        }
        public static void AddAi(string ai, string[] args)
        {
            FileIO.TryDelete("bots/" + ai);
            if (!File.Exists("bots/" + ai))
            {
                using StreamWriter w = new("bots/" + ai);
                w.WriteLine("#Version 2");
            }
            ScriptFile.Append(Player.NASConsole, ai, args.Length > 2 ? args[2] : "", args);
        }
        public static void SpawnEntity(Level level, string model, string ai, ushort x, ushort y, ushort z)
        {
            int uniqueMobId = level.Bots.Items.Length + 1;
            string uniqueName = "NASMob" + uniqueMobId;
            PlayerBot bot = new(uniqueName, level)
            {
                DisplayName = "",
                Model = model
            };
            Position pos = Position.FromFeet(x * 32 + 16, y * 32, z * 32 + 16);
            bot.SetInitialPos(pos);
            bot.AIName = ai;
            PlayerBot.Add(bot);
            ScriptFile.Parse(Player.NASConsole, bot, ai);
            BotsFile.Save(level);
        }
        public static ushort FindGround(Level level, int x, int y, int z)
        {
            if (x > level.Width)
                x = level.Width - 1;
            if (z > level.Length)
                z = level.Length - 1;
            if (x < 0)
                x = 0;
            if (z < 0)
                z = 0;
            for (int i = level.Height - 1; i >= 0; i--)
                if (level.FastGetBlock((ushort)x, (ushort)i, (ushort)z) != 0)
                    return (ushort)(i + 1);
            return (ushort)y;
        }
        public static void Load()
        {
            NAShostile = new NASHostileInstruction();
            NASroam = new NASRoamInstruction();
            BotInstruction.Instructions.Add(NAShostile);
            BotInstruction.Instructions.Add(NASroam);
            OnPlayerClickEvent.Register(HandleBlockClicked, Priority.Low);
            Server.MainScheduler.QueueRepeat(HandleMobSpawning, null, TimeSpan.FromSeconds(1));
            AddAi("NASHostile", new string[] { "", "NASHostile", "NASHostile" });
            AddAi("NASRoam", new string[] { "", "NASRoam", "NASRoam" });
            mobHealth.Clear();
        }
        public static void Unload()
        {
            OnPlayerClickEvent.Unregister(HandleBlockClicked);
            Server.MainScheduler.Cancel(mobSpawningTask);
            mobHealth.Clear();
            BotInstruction.Instructions.Remove(NAShostile);
            BotInstruction.Instructions.Remove(NASroam);
        }
        public static string CalculateCardinal(PlayerBot bot) => Orientation.PackedToDegrees(bot.Rot.RotY) switch
        {
            >= 45 and < 90 => "Northeast",
            >= 135 and < 180 => "Southeast",
            >= 225 and < 270 => "Southwest",
            >= 315 and < 361 => "Northwest",
            _ => ""
        };
        public static void SetDirectionalSpeeds(PlayerBot bot) => bot.movementSpeed = CalculateCardinal(bot) switch
        {
            "Northeast" or "Northwest" or "Southeast" or "Southwest" => (int)Math.Round(3m * 60 / 100m),
            _ => (int)Math.Round(3m * 100 / 100m),
        };
        public static Player ClosestPlayer(PlayerBot bot, int search)
        {
            int maxDist = search * 32;
            Player[] players = PlayerInfo.Online.Items;
            Player closest = null;
            foreach (Player p in players)
            {
                NASPlayer np = NASPlayer.GetPlayer(p);
                if (p.Level != bot.Level || !np.CanTakeDamage(NASDamageSource.Entity)) continue;
                int dx = p.Pos.X - bot.Pos.X, dy = p.Pos.Y - bot.Pos.Y, dz = p.Pos.Z - bot.Pos.Z,
                    playerDist = Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz);
                if (playerDist >= maxDist) continue;
                closest = p;
                maxDist = playerDist;
            }
            return closest;
        }
        public static void FaceTowards(PlayerBot bot)
        {
            int dstHeight = ModelInfo.CalcEyeHeight(bot),
                dx = bot.TargetPos.X - bot.Pos.X,
                dy = bot.TargetPos.Y + 16 - (bot.Pos.Y + dstHeight),
                dz = bot.TargetPos.Z - bot.Pos.Z;
            Vec3F32 dir = new(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);
            Orientation rot = bot.Rot;
            DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);
            bot.Rot = rot;
        }
    }
    public class NASMobMetadata
    {
        public int waitTime, walkTime, lookTime, search;
        public Player chasing;
    }
    public class NASHostileInstruction : BotInstruction
    {
        public NASHostileInstruction() => Name = "NASHostile";
        public static bool MoveTowards(PlayerBot bot, Player p)
        {
            if (p == null) return false;
            int dx = p.Pos.X - bot.Pos.X, dy = p.Pos.Y - bot.Pos.Y, dz = p.Pos.Z - bot.Pos.Z;
            bot.TargetPos = p.Pos;
            Vec3F32 dir = new(dx, dy, dz);
            if (dir.Length > 0) dir = Vec3F32.Normalise(dir);
            Orientation rot = bot.Rot;
            DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);
            bot.Rot = rot;
            NASMob.SetDirectionalSpeeds(bot);
            bot.movement = true;
            dx = Math.Abs(dx);
            dy = Math.Abs(dy);
            dz = Math.Abs(dz);
            AABB playerBB = p.ModelBB.OffsetPosition(p.Pos),
                botBB = bot.ModelBB.OffsetPosition(bot.Pos);
            int dist = (int)(0.875f * 32);
            bool inRange = (long)dx * dx + (long)dz * dz <= dist * dist &&
                botBB.Min.Y <= playerBB.Max.Y && playerBB.Min.Y <= botBB.Max.Y;
            if (inRange) HitPlayer(bot, p, rot);
            return dx <= 8 && dy <= 16 && dz <= 8;
        }
        public static void HitPlayer(PlayerBot bot, Player p, Orientation rot)
        {
            rot.RotY = (byte)(p.Rot.RotY + 128);
            bot.Rot = rot;
            int srcHeight = ModelInfo.CalcEyeHeight(bot),
                dstHeight = ModelInfo.CalcEyeHeight(p),
                dx2 = bot.Pos.X - p.Pos.X, dy2 = bot.Pos.Y + srcHeight - (p.Pos.Y + dstHeight), dz2 = bot.Pos.Z - p.Pos.Z;
            Vec3F32 dir2 = new(dx2, dy2, dz2);
            if (dir2.Length > 0) dir2 = Vec3F32.Normalise(dir2);
            float mult = 1 / ModelInfo.GetRawScale(p.Model),
                plScale = ModelInfo.GetRawScale(p.Model),
                VelocityY = 1.0117f * mult;
            if (dir2.Length <= 0) VelocityY = 0;
            if (p.Supports(CpeExt.VelocityControl))
                p.Send(Packet.VelocityControl(-dir2.X * mult * 0.57f, VelocityY, -dir2.Z * mult * 0.57f, 0, 1, 0));
            int damage = 0;
            switch (bot.Model)
            {
                case "spider":
                    damage = 2;
                    break;
                case "zombie":
                    damage = 3;
                    break;
                case "skeleton":
                    damage = 5;
                    break;
            }
            NASPlayer np = NASPlayer.GetPlayer(p);
            np.TakeDamage(damage, NASDamageSource.Entity);
        }
        public readonly Random _random = new();
        public int RandomNumber(int min, int max) => _random.Next(min, max);
        public void DoStuff(PlayerBot bot, NASMobMetadata meta)
        {
            int stillChance = RandomNumber(0, 5),
                walkTime = RandomNumber(4, 8) * 5,
                waitTime = RandomNumber(2, 5) * 5,
                dx = RandomNumber(bot.Pos.X - (8 * 32), bot.Pos.X + (8 * 32)),
                dz = RandomNumber(bot.Pos.Z - (8 * 32), bot.Pos.Z + (8 * 32));
            switch (stillChance)
            {
                case > 2:
                    meta.walkTime = walkTime;
                    break;
                default:
                    {
                        NASCoords target = new()
                        {
                            X = dx,
                            Y = bot.Pos.Y,
                            Z = dz,
                            RotX = bot.Rot.RotX,
                            RotY = bot.Rot.RotY
                        };
                        bot.TargetPos = new(target.X, target.Y, target.Z);
                        bot.movement = true;
                        if (bot.Pos.BlockX == bot.TargetPos.BlockX && bot.Pos.BlockZ == bot.TargetPos.BlockZ)
                        {
                            bot.SetYawPitch(target.RotX, target.RotY);
                            bot.movement = false;
                        }
                        NASMob.FaceTowards(bot);
                        meta.walkTime = walkTime;
                        bot.movement = false;
                        meta.waitTime = waitTime;
                        break;
                    }
            }
        }
        public override bool Execute(PlayerBot bot, InstructionData data)
        {
            NASMobMetadata meta = (NASMobMetadata)data.Metadata;
            switch (bot.Model)
            {
                case "zombie":
                    bot.movementSpeed = (int)Math.Round(3m * 94 / 100m);
                    break;
                case "skeleton":
                    bot.movementSpeed = (int)Math.Round(3m * 97 / 100m);
                    break;
            }
            if (bot.movementSpeed == 0) bot.movementSpeed = 1;
            int search = 16;
            if (meta.search != 16 && meta.search > 0) search = meta.search;
            else
            {
                if (bot.Model == "zombie") search = 35;
            }
            Player closest = NASMob.ClosestPlayer(bot, search);
            switch (closest)
            {
                case null:
                    if (meta.walkTime > 0)
                    {
                        meta.walkTime--;
                        bot.movement = true;
                        return true;
                    }
                    if (meta.waitTime > 0)
                    {
                        meta.waitTime--;
                        return true;
                    }
                    DoStuff(bot, meta);
                    bot.movement = false;
                    bot.NextInstruction();
                    break;
                default:
                    break;
            }
            if (closest != null)
            {
                if (MoveTowards(bot, closest))
                {
                    bot.NextInstruction();
                    return false;
                }
            }
            return true;
        }
        public override InstructionData Parse(string[] args)
        {
            InstructionData data = default;
            data.Metadata = new NASMobMetadata();
            NASMobMetadata meta = (NASMobMetadata)data.Metadata;
            if (args.Length > 1)
                meta.search = int.Parse(args[1]);
            return data;
        }
        public override string[] Help => new string[] { };
    }
    public class NASRoamInstruction : BotInstruction
    {
        public NASRoamInstruction() => Name = "NASRoam";
        public readonly Random _random = new();
        public int RandomNumber(int min, int max) => _random.Next(min, max);
        public void DoStuff(PlayerBot bot, NASMobMetadata meta)
        {
            int stillChance = RandomNumber(0, 5),
                walkTime = RandomNumber(4, 8) * 5,
                waitTime = RandomNumber(2, 4) * 5,
                lookChance = RandomNumber(0, 2),
                lookTime = RandomNumber(2, 5) * 5,
                dx = RandomNumber(bot.Pos.X - (8 * 32), bot.Pos.X + (8 * 32)),
                dz = RandomNumber(bot.Pos.Z - (8 * 32), bot.Pos.Z + (8 * 32));
            if (stillChance > 2)
            {
                meta.walkTime = walkTime;
                Player p = NASMob.ClosestPlayer(bot, 5);
                if (p != null && lookChance == 1)
                {
                    int srcHeight = ModelInfo.CalcEyeHeight(p),
                        dstHeight = ModelInfo.CalcEyeHeight(bot),
                        lx = p.Pos.X - bot.Pos.X, ly = p.Pos.Y + srcHeight - (bot.Pos.Y + dstHeight), lz = p.Pos.Z - bot.Pos.Z;
                    Vec3F32 dir = new(lx, ly, lz);
                    dir = Vec3F32.Normalise(dir);
                    Orientation rot = bot.Rot;
                    DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);
                    bot.Rot = rot;
                    meta.lookTime = lookTime;
                    meta.chasing = p;
                }
                else
                {
                    NASCoords target = new()
                    {
                        X = dx,
                        Y = bot.Pos.Y,
                        Z = dz,
                        RotX = bot.Rot.RotX,
                        RotY = bot.Rot.RotY
                    };
                    bot.TargetPos = new(target.X, target.Y, target.Z);
                    bot.movement = true;
                    if (bot.Pos.BlockX == bot.TargetPos.BlockX && bot.Pos.BlockZ == bot.TargetPos.BlockZ)
                    {
                        bot.SetYawPitch(target.RotX, bot.Rot.RotY);
                        bot.movement = false;
                    }
                    NASMob.FaceTowards(bot);
                    meta.walkTime = walkTime;
                    bot.movement = false;
                    meta.waitTime = waitTime;
                }
            }
        }
        public override bool Execute(PlayerBot bot, InstructionData data)
        {
            NASMobMetadata meta = (NASMobMetadata)data.Metadata;
            if (meta.walkTime > 0)
            {
                meta.walkTime--;
                bot.movement = true;
                return true;
            }
            if (meta.waitTime > 0)
            {
                Player p = NASMob.ClosestPlayer(bot, 5);
                if (p != null)
                {
                    int lookChance = RandomNumber(0, 2),
                        lookTime = RandomNumber(2, 5) * 5;
                    if (lookChance == 1)
                    {
                        int srcHeight = ModelInfo.CalcEyeHeight(p),
                            dstHeight = ModelInfo.CalcEyeHeight(bot),
                            lx = p.Pos.X - bot.Pos.X, ly = p.Pos.Y + srcHeight - (bot.Pos.Y + dstHeight), lz = p.Pos.Z - bot.Pos.Z;
                        Vec3F32 dir = new(lx, ly, lz);
                        dir = Vec3F32.Normalise(dir);
                        Orientation rot = bot.Rot;
                        DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);
                        bot.Rot = rot;
                        meta.lookTime = lookTime;
                        meta.chasing = p;
                    }
                    meta.waitTime = 0;
                    return true;
                }
                meta.waitTime--;
                return true;
            }
            if (meta.lookTime > 0)
            {
                if (meta.chasing != null)
                {
                    Player p = meta.chasing;
                    int srcHeight = ModelInfo.CalcEyeHeight(p),
                        dstHeight = ModelInfo.CalcEyeHeight(bot),
                        lx = p.Pos.X - bot.Pos.X, ly = p.Pos.Y + srcHeight - (bot.Pos.Y + dstHeight), lz = p.Pos.Z - bot.Pos.Z;
                    Vec3F32 dir = new(lx, ly, lz);
                    dir = Vec3F32.Normalise(dir);
                    Orientation rot = bot.Rot;
                    DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);
                    bot.Rot = rot;
                }
                meta.lookTime--;
                switch (meta.lookTime)
                {
                    case <= 0:
                        if (meta.chasing != null)
                            bot.NextInstruction();
                        break;
                    default:
                        return true;
                }
            }
            DoStuff(bot, meta);
            bot.movement = false;
            bot.NextInstruction();
            return true;
        }
        public override InstructionData Parse(string[] _)
        {
            InstructionData data = default;
            data.Metadata = new NASMobMetadata();
            return data;
        }
        public override string[] Help => new string[] { };
    }
}