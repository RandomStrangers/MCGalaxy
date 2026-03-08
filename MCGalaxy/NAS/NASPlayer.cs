using MCGalaxy.Commands;
using MCGalaxy.DB;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.Util.Imaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace MCGalaxy
{
    public abstract class NASCommand : Command
    {
        public override string Type => "NAS";
        public override bool SuperUseable => false;
        public override bool MuseumUsable => false;
        public override bool MessageBlockRestricted => true;
        public override void Use(Player p, string message) => Use(NASPlayer.GetPlayer(p), message);
        public abstract void Use(NASPlayer np, string message);
    }
    public abstract class NASCommand2 : NASCommand
    {
        public override void Use(NASPlayer np, string message) => Execute(np.p, message);
        public abstract void Execute(Player p, string message);
    }
    public class CmdGravestones : NASCommand2
    {
        public override string Name => "Gravestones";
        public override LevelPermission DefaultRank => LevelPermission.Operator;
        public override bool SuperUseable => true;
        public override void Execute(Player p, string message)
        {
            string PlayerName;
            if (CheckSuper(p, message, "player name"))
                return;
            if (string.IsNullOrEmpty(message))
                PlayerName = p.name;
            else
            {
                PlayerData target = PlayerDB.Match(p, message);
                if (target != null)
                    PlayerName = target.Name;
                else
                    return;
            }
            string file = NAS.GetDeathPath(PlayerName);
            if (!File.Exists(file))
            {
                p.Message("{0}&S has no gravestones recorded!", PlayerName);
                return;
            }
            string[] deaths = FileIO.TryReadAllLines(file),
                deaths2 = FileIO.TryReadAllLines(file);
            long count = deaths2.LongLength;
            for (long i = 0; i < deaths2.LongLength; i++)
            {
                if (!deaths2[i].IsNullOrWhiteSpace())
                    foreach (char c in deaths2[i])
                    {
                        string cString = c.ToString();
                        if (char.IsWhiteSpace(c))
                            deaths2[i] = deaths2[i].Replace(cString, "");
                    }
                if (deaths2[i].IsNullOrWhiteSpace())
                {
                    deaths2[i] = null;
                    count--;
                }
            }
            if (count <= 0)
            {
                p.Message("{0}&S has no gravestones recorded!", PlayerName);
                return;
            }
            p.Message("{0}&S's gravestones:", PlayerName);
            p.MessageLines(deaths);
        }
        public override void Help(Player p) => p.Message("&T/Gravestones [name] &H- Views the location of the player's gravestones");
    }
    public class CmdMyGravestones : NASCommand
    {
        public override string Name => "MyGravestones";
        public override void Use(NASPlayer np, string message)
        {
            string file = NAS.GetDeathPath(np.p.name);
            if (!File.Exists(file))
            {
                np.Message("You have no gravestones recorded!");
                return;
            }
            string[] deaths = FileIO.TryReadAllLines(file),
                deaths2 = FileIO.TryReadAllLines(file);
            long count = deaths2.LongLength;
            for (long i = 0; i < deaths2.LongLength; i++)
            {
                if (!deaths2[i].IsNullOrWhiteSpace())
                    foreach (char c in deaths2[i])
                    {
                        string cString = c.ToString();
                        if (char.IsWhiteSpace(c))
                            deaths2[i] = deaths2[i].Replace(cString, "");
                    }
                if (deaths2[i].IsNullOrWhiteSpace())
                {
                    deaths2[i] = null;
                    count--;
                }
            }
            if (count <= 0)
            {
                np.Message("&SYou have no gravestones recorded!");
                return;
            }
            np.Message("Your gravestones:");
            np.MessageLines(deaths);
        }
        public override void Help(Player p) => p.Message("&T/MyGravestones &H- Views the location of your own gravestones");
    }
    public class CmdPVP : NASCommand
    {
        public override string Name => "PvP";
        public override void Use(NASPlayer np, string message)
        {
            if (message.CaselessEq("on") || message.CaselessEq("enable"))
            {
                if (!np.pvpEnabled)
                {
                    np.Message("You can now attack and be attacked by other players.");
                    np.pvpEnabled = true;
                    np.pvpCooldown = DateTime.UtcNow + new TimeSpan(1, 0, 0);
                    return;
                }
                else
                {
                    np.Message("PvP is already enabled");
                    return;
                }
            }
            if (message.CaselessEq("off") || message.CaselessEq("disable"))
            {
                if (!np.pvpEnabled)
                {
                    np.Message("PvP is already disabled");
                    return;
                }
                if (np.pvpCooldown > DateTime.UtcNow)
                {
                    TimeSpan remaining = np.pvpCooldown - DateTime.UtcNow;
                    np.Message("Please wait " + remaining.Minutes + " minutes and " + remaining.Seconds + " seconds before using this command");
                    return;
                }
                np.Message("You can no longer attack and be attacked by other players.");
                np.pvpEnabled = false;
                return;
            }
            Help(np.p);
            return;
        }
        public override void Help(Player p)
        {
            p.Message("&T/PvP [on/off]");
            p.Message("&HToggles PvP, but once you turn it on, you can't turn it off for an hour.");
        }
    }
    public class CmdNASSpawn : NASCommand
    {
        public override string Name => "NASSpawn";
        public override LevelPermission DefaultRank => LevelPermission.Admin;
        public override void Use(NASPlayer np, string message)
        {
            if ((!message.CaselessContains("confirm") || string.IsNullOrEmpty(message)) && np.hasBeenSpawned)
            {
                np.Message("&HHasBeenSpawned is currently true. If you want to turn off HasBeenSpawned, type &T/NASSpawn &Hconfirm");
                return;
            }
            else if (message.CaselessContains("confirm") && np.hasBeenSpawned)
            {
                np.Message("HasBeenSpawned set to false. Use &T/NASSpawn &Hto turn it back on.");
                np.hasBeenSpawned = false;
                return;
            }
            else if (!np.hasBeenSpawned)
            {
                np.hasBeenSpawned = true;
                np.Message("HasBeenSpawned set to true.");
                return;
            }
        }
        public override void Help(Player p) => p.Message("&T/NASSpawn &H- Toggles HasBeenSpawned");
    }
    public class CmdSpawnDungeon : NASCommand
    {
        public override string Name => "SpawnDungeon";
        public override string Shortcut => "GenerateDungeon";
        public override LevelPermission DefaultRank => LevelPermission.Admin;
        public override void Use(NASPlayer np, string message)
        {
            np.Message("Generating dungeon.");
            NASGen.GenerateDungeon(np, np.p.Pos.BlockCoords.X, np.p.Pos.BlockCoords.Y, np.p.Pos.BlockCoords.Z, np.p.Level, NASLevel.Get(np.p.Level.name));
        }
        public override void Help(Player p)
        {
            p.Message("&T/SpawnDungeon");
            p.Message("&HGenerates a dungeon.");
        }
    }
    public class CmdBarrelMode : NASCommand
    {
        public override string Name => "BarrelMode";
        public override void Use(NASPlayer np, string message)
        {
            if (np.oldBarrel)
            {
                np.Message("You now have to input the amount of items you want to put into the barrel manually.");
                np.oldBarrel = false;
            }
            else
            {
                np.Message("Half of the items you're holding will now be put into the barrel.");
                np.oldBarrel = true;
            }
        }
        public override void Help(Player p)
        {
            p.Message("&T/BarrelMode");
            p.Message("&HToggles how you enter items into barrels.");
        }
    }
    public enum NASDamageSource
    {
        Falling, Suffocating, Drowning, Entity, None, Murder
    }
    public class NASEntity
    {
        [JsonIgnore] public NASLevel nl;
        public bool holdingBreath = false;
        public virtual bool CanTakeDamage(NASDamageSource source) => true;
        public virtual bool TakeDamage(float damage, NASDamageSource source, string customDeathReason = "") => !CanTakeDamage(source) && false;
    }
    public partial class NASPlayer : NASEntity
    {
        public static byte[] fallback = new byte[256];
        [JsonIgnore] public Player p;
        [JsonIgnore] public NASBlock heldNasBlock = NASBlock.Default;
        [JsonIgnore] public ushort breakX = ushort.MaxValue, breakY = ushort.MaxValue, breakZ = ushort.MaxValue;
        [JsonIgnore] public int breakAttempt = 0, round = 0;
        [JsonIgnore] public DateTime? lastAirClickDate = null;
        [JsonIgnore] public DateTime lastLeftClickReleaseDate = DateTime.MinValue,
            lastSuffocationDate = DateTime.MinValue,
            datePositionCheckingIsAllowed = DateTime.MinValue;
        [JsonIgnore] public bool justBrokeOrPlaced = false, 
            SendingMap = false, hasBeenSpawned = false, isDead = false, 
            headingToBed = false, isChewing = false, isInserting = false,
            placePortal = false, atBorder = true;
        [JsonIgnore] public byte craftingAreaID = 0;
        [JsonIgnore] public int[] interactCoords;
        [JsonIgnore] public const string Path = NAS.Path + "PlayerData/",
            DeathsPath = Path + "Deaths/";
        [JsonIgnore] public Scheduler PlayerSavingScheduler;
        [JsonIgnore] public SchedulerTask PlayerSaveTask;
        [JsonIgnore] public Pixel targetFogColor = new(255, 255, 255, 255),
            curFogColor = new(255, 255, 255, 255);
        [JsonIgnore] public float AirPrev,
            targetRenderDistance = Server.Config.MaxFogDistance,
            curRenderDistance = Server.Config.MaxFogDistance;
        [JsonIgnore] public Player lastAttackedPlayer = null;
        [JsonIgnore] public string reason = null;
        [JsonIgnore] public CpeMessageType whereHealthIsDisplayed = CpeMessageType.BottomRight2;
        [JsonIgnore]
        public AABB bounds = AABB.Make(new(0, 0, 0), new(16, 26 * 2, 16)),
            eyeBounds = AABB.Make(new(0, 24 * 2 - 2, 0), new(4, 4, 4));
        [JsonIgnore] public NASTransferInfo transferInfo = null;
        public bool bigUpdate = false, oldBarrel = true, pvpEnabled = false;
        public NASInventory inventory;
        public Position spawnCoords;
        public int[] bedCoords;
        public string spawnMap, levelName;
        public DateTime pvpCooldown;
        public int kills = 0, exp = 0, levels = 0, resetCount = 0;
        public byte yaw, pitch;
        public Vec3S32 location, lastGroundedLocation;
        public float HP, Air;
        public static Dictionary<string, DateTime> cooldowns = new();
        [JsonIgnore]
        public bool CanDoStuffBasedOnPosition
        {
            get
            {
                return DateTime.UtcNow >= datePositionCheckingIsAllowed;
            }
            set
            {
                if (!value)
                {
                    datePositionCheckingIsAllowed = DateTime.UtcNow.AddMilliseconds(2000 + p.Session.Ping.HighestPing());
                }
            }
        }
        public void Message(string message, params object[] args) => p.Message(string.Format(message, args));
        public void MessageLines(IEnumerable<string> lines) => p.MessageLines(lines);
        public void Send(byte[] buffer) => p.Socket.Send(buffer, SendFlags.None);
        public void ResetBreaking()
        {
            breakX = breakY = breakZ = ushort.MaxValue;
            if (p.Extras.Contains("NAS_taskDisplayMeter"))
                NASBlockChange.breakScheduler.Cancel((SchedulerTask)p.Extras["NAS_taskDisplayMeter"]);
        }
        public void SaveStatsTask(SchedulerTask _) => Save();
        public void Save()
        {
            if (this != null)
            {
                string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
                FileIO.TryWriteAllText(NAS.GetSavePath(p), jsonString);
                FileIO.TryWriteAllText(NAS.GetTextPath(p), jsonString);
            }
        }
        public void SetLocation(string levelName, Position pos, Orientation rot)
        {
            this.levelName = levelName;
            location.X = pos.X;
            location.Y = pos.Y;
            location.Z = pos.Z;
            yaw = rot.RotY;
            pitch = rot.HeadX;
        }
        public static NASPlayer GetPlayer(Player p)
        {
            if (!p.Extras.Contains(NAS.PlayerKey))
            {
                NASPlayer np = new(p);
                np.SetLocation(Server.mainLevel.name, Server.mainLevel.SpawnPos, new(Server.mainLevel.rotx, Server.mainLevel.roty));
                p.Extras[NAS.PlayerKey] = np;
                return np;
            }
            return (NASPlayer)p.Extras[NAS.PlayerKey];
        }
        public bool CanDamage() => !p.invincible && !p.Game.Referee && pvpEnabled;
        public NASPlayer(Player p)
        {
            this.p = p;
            HP = 10;
            Air = 10;
            inventory = new()
            {
                p = p
            };
            spawnCoords = Server.mainLevel.SpawnPos;
            bedCoords = new int[] 
            {
                238, 94, 179 
            };
            spawnMap = Server.mainLevel.name;
        }
        public void SetPlayer(Player p)
        {
            this.p = p;
            inventory.p = p;
        }
        public void HandleInteraction(MouseButton button, MouseAction action, ushort x, ushort y, ushort z, byte _, TargetBlockFace face)
        {
            if (button == MouseButton.Right && p.ClientHeldBlock != 0)
            {
                ushort xPlacing = x,
                    yPlacing = y,
                    zPlacing = z;
                if (face == TargetBlockFace.AwayX)
                    xPlacing++;
                if (face == TargetBlockFace.TowardsX)
                    xPlacing--;
                if (face == TargetBlockFace.AwayY)
                    yPlacing++;
                if (face == TargetBlockFace.TowardsY)
                    yPlacing--;
                if (face == TargetBlockFace.AwayZ)
                    zPlacing++;
                if (face == TargetBlockFace.TowardsZ)
                    zPlacing--;
                if (p.Level.GetBlock(xPlacing, yPlacing, zPlacing) == 0)
                {
                    AABB worldAABB = bounds.OffsetPosition(p.Pos),
                        blockAABB = new(0, 0, 0, 32, 32, 32);
                    blockAABB = blockAABB.Offset(xPlacing * 32, yPlacing * 32, zPlacing * 32);
                    if (!AABB.Intersects(ref worldAABB, ref blockAABB))
                        return;
                }
            }
            ushort serverushort = p.Level.GetBlock(x, y, z),
                clientushort = ConvertBlock(serverushort);
            NASBlock nasBlock = NASBlock.Get(clientushort);
            if (nasBlock.interaction != null)
            {
                if (!CanDoStuffBasedOnPosition)
                {
                    if (action == MouseAction.Released)
                        Message("&cPlease wait a moment before interacting with blocks");
                    return;
                }
                nasBlock.interaction(this, button, action, nasBlock, x, y, z);
            }
        }
        public static void StartCooldown(Player p, int milli)
        {
            int milliseconds = milli % 1000,
                seconds = milli / 1000;
            DateTime expires = DateTime.UtcNow.AddMilliseconds(milliseconds);
            expires = expires.AddSeconds(seconds);
            cooldowns[p.name] = expires;
        }
        public void UpdateValues()
        {
            spawnCoords = Server.mainLevel.SpawnPos;
            bedCoords = new int[] { 238, 94, 179 };
            spawnMap = Server.mainLevel.name;
            levelName = Server.mainLevel.name;
            location = new(spawnCoords.X, spawnCoords.Y, spawnCoords.Z);
            lastGroundedLocation = location;
            yaw = 128;
            pitch = 0;
            bigUpdate = true;
            resetCount += 1;
        }
        public static bool CooledDown(Player p) => DateTime.UtcNow.CompareTo(cooldowns[p.name]) != -1;
        public void GiveExp(int amount)
        {
            if (inventory.HeldItem.Enchant("Mending") == 1 && inventory.HeldItem.HP < inventory.HeldItem.Prop.baseHP)
            {
                int given = Math.Min(amount * 15, (int)inventory.HeldItem.Prop.baseHP - (int)inventory.HeldItem.HP);
                amount -= given / 15;
                inventory.HeldItem.HP += given;
            }
            int expRequired;
            if (levels <= 16)
                expRequired = 2 * levels + 7;
            else
            {
                if (levels <= 31)
                    expRequired = 5 * levels - 38;
                else
                    expRequired = 9 * levels - 158;
            }
            exp += amount;
            while (exp >= expRequired)
            {
                if (exp >= expRequired && levels == 99)
                    exp = expRequired - 1;
                if (exp >= expRequired)
                {
                    exp -= expRequired;
                    levels += 1;
                }
                if (levels <= 16)
                    expRequired = 2 * levels + 7;
                else
                {
                    if (levels <= 31)
                        expRequired = 5 * levels - 38;
                    else
                        expRequired = 9 * levels - 158;
                }
            }
        }
        public void GiveLevels(int amount)
        {
            levels += amount;
            if (levels > 99)
                levels = 99;
        }
        public int GetExp() => levels <= 16
                ? exp + (int)Math.Pow(levels, 2) + 6 * levels
                : levels <= 31
                ? exp + (int)(2.5 * Math.Pow(levels, 2) - 40.5 * levels + 360)
                : exp + (int)(4.5 * Math.Pow(levels, 2) - 162.5 * levels + 2220);
        public static void ClickOnPlayer(Player p, byte entity, MouseButton button, MouseAction action)
        {
            NASPlayer np = GetPlayer(p);
            if (entity == 0xFF || (button == MouseButton.Right && np.inventory.HeldItem.Prop.knockback >= 0) || (button == MouseButton.Left && np.inventory.HeldItem.Prop.knockback < 0) || button == MouseButton.Middle || action == MouseAction.Pressed)
                return;
            if (!cooldowns.ContainsKey(p.name))
                cooldowns.Add(p.name, DateTime.UtcNow);
            TimeSpan kbdelay = new(0);
            Player[] players = PlayerInfo.Online.Items;
            for (int i = 0; i < players.Length; i++)
            {
                if (!p.EntityList.GetID(players[i], out byte ID) || ID != entity)
                    continue;
                Player who = players[i];
                if (!np.CanDamage())
                {
                    string reason = "";
                    if (np.p.Game.Referee)
                        reason = "are a referee";
                    if (np.p.invincible)
                        reason = "are invincible";
                    if (!np.pvpEnabled)
                        reason = "do not have PVP enabled";
                    if (!np.hasBeenSpawned)
                        reason = "are not spawned";
                    np.Message("&SYou cannot damage {0} &Sbecause you currently {1}.", who.DisplayName, reason);
                    return;
                }
                Vec3F32 delta = p.Pos.ToVec3F32() - who.Pos.ToVec3F32();
                float reachSq = p.ReachDistance * p.ReachDistance;
                if (np.inventory.HeldItem.Prop.knockback < 0)
                {
                    reachSq *= 16;
                    kbdelay = new(TimeSpan.TicksPerMillisecond * (int)delta.LengthSquared * 20);
                }
                if (!CooledDown(p))
                {
                    StartCooldown(p, np.inventory.HeldItem.Prop.recharge + (int)(kbdelay.Ticks / TimeSpan.TicksPerMillisecond));
                    return;
                }
                StartCooldown(p, np.inventory.HeldItem.Prop.recharge + (int)(kbdelay.Ticks / TimeSpan.TicksPerMillisecond));
                if (delta.LengthSquared > (reachSq + 1))
                    return;
                NASItem Held = np.inventory.HeldItem;
                if (Held.Prop.damage > 0.5f)
                {
                    if (Held.TakeDamage(7))
                        np.inventory.BreakItem(ref Held);
                }
                else
                {
                    if (Held.TakeDamage(1))
                        np.inventory.BreakItem(ref Held);
                }
                np.inventory.UpdateItemDisplay();
                NASPlayer w = GetPlayer(who);
                w.lastAttackedPlayer = p;
                if (!w.CanTakeDamage(NASDamageSource.Murder) || !w.CanTakeDamage(NASDamageSource.Entity))
                {
                    string reason = "";
                    if (w.p.Game.Referee)
                        reason = who.Pronouns.PresentVerb + " a referee";
                    if (w.p.invincible)
                        reason = who.Pronouns.PresentVerb + " invincible";
                    if (w.headingToBed)
                        reason = who.Pronouns.PresentVerb + " heading to bed";
                    if (who.Pos.FeetBlockCoords.X == w.bedCoords[0] && who.Pos.FeetBlockCoords.Y == w.bedCoords[1] && who.Pos.FeetBlockCoords.Z == w.bedCoords[2])
                        reason = who.Pronouns.PresentVerb + " in bed";
                    if (!w.pvpEnabled)
                    {
                        if (who.Pronouns.Plural)
                            reason = "do not have PVP enabled";
                        else
                            reason = "does not have PVP enabled";
                    }
                    if (!w.hasBeenSpawned)
                        reason = w.p.Pronouns.PresentVerb + " not spawned";
                    np.Message("&SYou cannot damage {0} &Sbecause " + who.Pronouns.Subject + " currently {1}.", who.DisplayName, reason);
                    return;
                }
                if (w.pvpEnabled)
                {
                    float added = 0;
                    if (np.inventory.HeldItem.Enchant("Sharpness") != 0)
                        added += 1;
                    added += np.inventory.HeldItem.Enchant("Sharpness") * 0.5f;
                    w.TakeDamage(np.inventory.HeldItem.Prop.damage + added, NASDamageSource.Entity, "@p %f" + who.Pronouns.PastVerb + " slain by " + p.ColoredName + " %fusing " + np.inventory.HeldItem.displayName);
                    NASFishingInfo info = new()
                    {
                        p = p,
                        who = who
                    };
                    SchedulerTask kbtask = NASBlockChange.fishingScheduler.QueueOnce(np.Knockback, info, kbdelay);
                }
            }
        }
        public void Knockback(SchedulerTask task)
        {
            Player who = ((NASFishingInfo)task.State).who,
                p = ((NASFishingInfo)task.State).p;
            int srcHeight = ModelInfo.CalcEyeHeight(p),
                dstHeight = ModelInfo.CalcEyeHeight(who),
                dx = p.Pos.X - who.Pos.X,
                dy = p.Pos.Y + srcHeight - (who.Pos.Y + dstHeight),
                dz = p.Pos.Z - who.Pos.Z;
            Vec3F32 dir = new(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);
            float mult = inventory.HeldItem.Prop.knockback + 0.25f * inventory.HeldItem.Enchant("Knockback"),
                yChange = 0f;
            if (mult < 0)
            {
                mult = (p.Pos.ToVec3F32() - who.Pos.ToVec3F32()).Length / -2f;
                yChange = -dir.Y;
            }
            if (who.Supports(CpeExt.VelocityControl) && p.Supports(CpeExt.VelocityControl))
                who.Send(Packet.VelocityControl(-dir.X * mult, 0.5f + (yChange * mult), -dir.Z * mult, 0, 1, 0));
            else
                p.Message("Please update to the latest ClassiCube build to hit with knockback.");
        }
        public void ChangeHealth(float diff)
        {
            HP += diff;
            if (HP < 0)
                HP = 0;
            if (HP > 10)
                HP = 10;
            DisplayHealth();
        }
        public float DamageSaved(bool takeDamage = false)
        {
            float armor = 0f;
            armor += SearchItem("chestplate", takeDamage).Prop.armor;
            armor += SearchItem("helmet", takeDamage).Prop.armor;
            armor += SearchItem("leggings", takeDamage).Prop.armor;
            armor += SearchItem("boots", takeDamage).Prop.armor;
            if (armor > 20f)
                armor = 20f;
            return armor;
        }
        public int EnchantLevels(string ench)
        {
            int armor = 0;
            armor += SearchItem("chestplate").Enchant(ench);
            armor += SearchItem("helmet").Enchant(ench);
            armor += SearchItem("leggings").Enchant(ench);
            armor += SearchItem("boots").Enchant(ench);
            return armor;
        }
        public NASItem SearchItem(string type, bool takeDamage = false)
        {
            float armor = 0f;
            int i, index = 0;
            bool done = false;
            for (i = 0; i < inventory.items.Length; i++)
                if (inventory.items[i] != null && inventory.items[i].armor > armor &&
                    inventory.items[i].name.CaselessContains(type))
                {
                    armor = inventory.items[i].Prop.armor;
                    index = i;
                    done = true;
                    break;
                }
            NASItem saved = new("Key");
            if (done)
                saved = inventory.items[index];
            if (done)
                saved.enchants = inventory.items[index].enchants;
            if (armor > 0f && takeDamage)
            {
                double toolDamageChance = 60 + (40.0 / inventory.items[index].Enchant("Unbreaking") + 1);
                Random r = new();
                if (r.NextDouble() < toolDamageChance && inventory.items[index].TakeDamage(1))
                    inventory.BreakItem(ref inventory.items[index]);
                inventory.UpdateItemDisplay();
            }
            return saved;
        }
        public override bool CanTakeDamage(NASDamageSource source)
        {
            if (!pvpEnabled && (source == NASDamageSource.Murder || source == NASDamageSource.Entity) || p.invincible || p.Game.Referee || headingToBed || p.Pos.FeetBlockCoords.X == bedCoords[0] && p.Pos.FeetBlockCoords.Y == bedCoords[1] && p.Pos.FeetBlockCoords.Z == bedCoords[2])
                return false;
            if (!hasBeenSpawned)
            {
                Message("If you get this message for more than 5 seconds, rejoin.");
                return false;
            }
            if (source == NASDamageSource.Suffocating)
            {
                TimeSpan timeSinceSuffocation = DateTime.UtcNow.Subtract(lastSuffocationDate);
                if (timeSinceSuffocation.TotalMilliseconds < 500)
                    return false;
                lastSuffocationDate = DateTime.UtcNow;
            }
            return true;
        }
        static string DeathReason(NASDamageSource source, Player p) => source switch
        {
            NASDamageSource.Entity => "@p &cdied.",
            NASDamageSource.Falling => "@p &cfell to " + p.Pronouns.Object + " death.",
            NASDamageSource.Suffocating => "@p &esuffocated.",
            NASDamageSource.Drowning => "@p &rdrowned.",
            NASDamageSource.None => "@p &adied from unknown causes.",
            NASDamageSource.Murder => "@p &8" + p.Pronouns.PastVerb + "&8 murdered by &S@s",
            _ => Enum.GetName(typeof(NASDamageSource), source).ToLower(),
        };
        public override bool TakeDamage(float damage, NASDamageSource source, string customDeathReason = "")
        {
            if (HP > 10)
                HP = 10;
            if (!CanTakeDamage(source) || damage == 0)
                return false;
            if (source != NASDamageSource.Drowning)
            {
                damage *= 1 - ((1 - (damage / 50)) * (DamageSaved(true) * 0.04f));
                damage *= 1 - (EnchantLevels("Protection") * 0.04f);
                if (damage > 1f)
                    damage = (float)Math.Round(damage * 2f) / 2f;
            }
            if (source == NASDamageSource.Falling && EnchantLevels("Feather Falling") > 0)
                damage = Math.Max(0, damage - EnchantLevels("Feather Falling"));
            if (damage == 0)
                return false;
            ChangeHealth(-damage);
            curFogColor = new(255, 255, 0, 0);
            Position next = p.Pos;
            int x = Utils.Clamp(next.BlockX, 0, (ushort)(p.Level.Width - 1)),
                z = Utils.Clamp(next.BlockZ, 0, (ushort)(p.Level.Length - 1));
            ushort y = (ushort)Utils.Clamp(next.BlockY, 0, (ushort)(p.Level.Height - 1));
            if (y < NASGen.oceanHeight)
                y = NASGen.oceanHeight;
            float fogMultiplier = 1f + (damage * damage * 0.08f);
            int distanceBelow = nl.biome < 0 ? 0 : y - next.BlockY;
            curRenderDistance = distanceBelow >= NASGen.diamondDepth
                ? 128 * fogMultiplier
                : distanceBelow >= NASGen.goldDepth || distanceBelow >= NASGen.ironDepth
                    ? 192 * fogMultiplier
                    : distanceBelow >= NASGen.coalDepth || nl.biome < 0 ? 256 * fogMultiplier : Server.Config.MaxFogDistance * 0.7f;
            DisplayHealth("f", "&7[", "&7]");
            if (HP <= 0)
            {
                if (customDeathReason.Length == 0)
                    customDeathReason = DeathReason(source, p);
                if (source == NASDamageSource.Entity)
                {
                    GetPlayer(lastAttackedPlayer).kills++;
                    GetPlayer(lastAttackedPlayer).GiveLevels(levels / 2);
                    levels -= levels / 2;
                }
                Die(customDeathReason);
                return true;
            }
            SchedulerTask taskDisplayRed = Server.MainScheduler.QueueOnce(FinishTakeDamage, this, TimeSpan.FromMilliseconds(100));
            return false;
        }
        public static void FinishTakeDamage(SchedulerTask task) => (task.State as NASPlayer).DisplayHealth();
        public void Die(string thing)
        {
            hasBeenSpawned = false;
            isDead = true;
            lastAttackedPlayer = null;
            reason = thing.Replace("@p", p.ColoredName);
            PlayerActions.Respawn(p);
        }
        public void TryDropGravestone()
        {
            NASDrop deathDrop = new(inventory);
            if (deathDrop.blockStacks == null && deathDrop.items == null)
                return;
            Vec3S32 gravePos = p.Pos.FeetBlockCoords;
            p.Level.ClampPos(gravePos);
            int x = gravePos.X,
                y = gravePos.Y,
                z = gravePos.Z;
            if (x < 0)
                x = 0;
            if (z < 0)
                z = 0;
            if (x > 383)
                x = 383;
            if (z > 383)
                z = 383;
            while (!CanPlaceGraveStone(x, y, z))
            {
                y++;
                if (y >= p.Level.Height - 1)
                {
                    Message("Something weird happened. You died in a location such that we can't place a grave.");
                    Message("Sorry, you've lost all your stuff.");
                }
            }
            nl.SetBlock(x, y, z, Block.FromRaw(647));
            NASBlockEntity blockEntity = new()
            {
                drop = deathDrop
            };
            nl.blockEntities.Add(x + " " + y + " " + z, blockEntity);
            Message("You dropped a gravestone at {0} {1} {2} in {3}", x, y, z, p.Level.name);
            FileIO.TryAppendAllText(NAS.GetDeathPath(p.name), x + " " + y + " " + z + " in " + p.Level.name);
            nl.blockEntities[x + " " + y + " " + z].lockedBy = p.name;
            nl.blockEntities[x + " " + y + " " + z].drop.exp = GetExp();
        }
        public string OxygenString()
        {
            if (Air == 10)
                return "";
            if (Air == 0)
                return "&r?";
            StringBuilder builder = new("", 16);
            for (int i = 0; i < Air; ++i)
                builder.Append('°');
            return builder.ToString();
        }
        public bool CanPlaceGraveStone(int x, int y, int z) => NASBlock.CanPhysicsKillThis(nl.GetBlock(x, y, z)) || NASBlock.IsThisLiquid(nl.GetBlock(x, y, z));
        public void SendCpeMessage(CpeMessageType type, string message) => p.SendCpeMessage(type, message);
        public void DisplayHealth(string healthColor = "p", string prefix = "&7[", string suffix = "&7]¼") => SendCpeMessage(whereHealthIsDisplayed, OxygenString() + " " + prefix + HealthString(healthColor) + suffix + ArmorDisplay() + " " + ExpDisplay() + " " + AttackRecharge());
        public string HealthString(string healthColor)
        {
            if (HP > 10)
                HP = 10;
            StringBuilder builder = new("&8", 16);
            float totalLostHealth = 10 - HP,
                lostHealthRemaining = totalLostHealth;
            for (int i = 0; i < totalLostHealth; ++i)
            {
                switch (lostHealthRemaining)
                {
                    case < 1:
                        builder.Append("&" + healthColor + "╝");
                        break;
                    default:
                        builder.Append("♥");
                        break;
                }
                lostHealthRemaining--;
            }
            builder.Append("&" + healthColor);
            for (int i = 0; i < (int)HP; ++i)
                builder.Append("♥");
            return builder.ToString();
        }
        public string AttackRecharge()
        {
            if (!cooldowns.ContainsKey(p.name))
                return "&aα";
            double cooldownPercent = (cooldowns[p.name] - DateTime.UtcNow).TotalMilliseconds / inventory.HeldItem.Prop.recharge;
            return cooldownPercent >= 0.66
                ? "&4α"
                : cooldownPercent >= 0.33 ? "&oα" : cooldownPercent > 0 ? "&eα" : cooldownPercent <= 0 ? "&aα" : "&hα";
        }
        public string ArmorDisplay()
        {
            StringBuilder builder = new(5);
            builder.Append("&fΦ ");
            builder.Append(DamageSaved().ToString());
            return builder.ToString();
        }
        public string ExpDisplay()
        {
            StringBuilder builder = new(5);
            builder.Append("&a☼ ");
            builder.Append(levels.ToString());
            return builder.ToString();
        }
        public void UpdateAir()
        {
            AirPrev = Air;
            if (holdingBreath)
            {
                Air -= 0.03125f;
                if (Air < 0)
                    Air = 0;
            }
            else
            {
                Air += 0.03125f;
                if (Air > 10)
                    Air = 10;
            }
            if (Air == 0)
                TakeDamage(0.125f, NASDamageSource.Drowning);
            if (Air != AirPrev && Air == Math.Floor(Air))
                DisplayHealth();
        }
        public void UpdateHeldBlock()
        {
            ushort clientushort = ConvertBlock(p.ClientHeldBlock);
            NASBlock nasBlock = NASBlock.Get(clientushort);
            if (nasBlock.parentID != heldNasBlock.parentID)
                inventory.DisplayHeldBlock(nasBlock);
            heldNasBlock = nasBlock;
        }
        public void Teleport(string message)
        {
            bool preciseTP = message.CaselessStarts("-precise ");
            if (preciseTP)
                message = message.Substring("-precise ".Length);
            string[] args = message.SplitSpaces();
            TeleportCoords(args, preciseTP);
        }
        public bool GetTeleportCoords(Entity ori, string[] args, bool precise,
                                               out Position pos, out byte yaw, out byte pitch)
        {
            Vec3S32 P;
            pos = p.Pos;
            yaw = ori.Rot.RotY;
            pitch = ori.Rot.HeadX;
            if (!precise)
            {
                P = p.Pos.FeetBlockCoords;
                if (!CommandParser.GetCoords(p, args, 0, ref P))
                    return false;
                pos = Position.FromFeetBlockCoords(P.X, P.Y, P.Z);
            }
            else
            {
                P = new(p.Pos.X, p.Pos.Y - 51, p.Pos.Z);
                if (!CommandParser.GetCoords(p, args, 0, ref P))
                    return false;
                pos = new(P.X, P.Y + 51, P.Z);
            }
            int angle = 0;
            if (args.Length > 3)
            {
                if (!CommandParser.GetInt(p, args[3], "Yaw angle", ref angle, -360, 360))
                    return false;
                yaw = Orientation.DegreesToPacked(angle);
            }
            if (args.Length > 4)
            {
                if (!CommandParser.GetInt(p, args[4], "Pitch angle", ref angle, -360, 360))
                    return false;
                pitch = Orientation.DegreesToPacked(angle);
            }
            return true;
        }
        public void TeleportCoords(string[] args, bool precise)
        {
            if (!GetTeleportCoords(p, args, precise, out Position pos, out byte yaw, out byte pitch))
                return;
            PlayerOperations.TeleportToCoords(p, pos, new(yaw, pitch));
        }
        public void SendToMain()
        {
            if (p.Level == Server.mainLevel)
                PlayerActions.Respawn(p);
            else
                PlayerActions.ChangeMap(p, Server.mainLevel);
        }
    }
}
