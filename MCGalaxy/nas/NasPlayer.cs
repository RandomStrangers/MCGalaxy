#if NAS && TEN_BIT_BLOCKS
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using MCGalaxy;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;
using MCGalaxy.Maths;
using MCGalaxy.Tasks;
using MCGalaxy.DB;
using System.IO;
namespace NotAwesomeSurvival
{
    public partial class NasPlayer : NasEntity
    {
        public static byte[] fallback = new byte[256];
        [JsonIgnore] public Player p;
        [JsonIgnore] public NasBlock heldNasBlock = NasBlock.Default;
        [JsonIgnore] public ushort breakX = ushort.MaxValue, breakY = ushort.MaxValue, breakZ = ushort.MaxValue;
        [JsonIgnore] public int breakAttempt = 0;
        [JsonIgnore] public DateTime? lastAirClickDate = null;
        [JsonIgnore] public DateTime lastLeftClickReleaseDate = DateTime.MinValue;
        [JsonIgnore] public bool justBrokeOrPlaced = false;
        [JsonIgnore] public byte craftingAreaID = 0;
        [JsonIgnore] public bool isChewing = false;
        [JsonIgnore] public bool isInserting = false;
        [JsonIgnore] public int[] interactCoords;
        [JsonIgnore] public bool SendingMap = false;
        [JsonIgnore] public const string DeathsPath = Nas.SavePath + "deaths/";
        [JsonIgnore] public Scheduler PlayerSavingScheduler;
        [JsonIgnore] public SchedulerTask PlayerSaveTask;
        [JsonIgnore] public bool hasBeenSpawned = false;
        [JsonIgnore] public bool isDead = false;
        [JsonIgnore] public bool headingToBed = false;
        [JsonIgnore] public Color targetFogColor = Color.White;
        [JsonIgnore] public Color curFogColor = Color.White;
        [JsonIgnore] public float targetRenderDistance = Server.Config.MaxFogDistance;
        [JsonIgnore] public float curRenderDistance = Server.Config.MaxFogDistance;
        [JsonIgnore] public bool SetInventoryNotif = false;
        [JsonIgnore] public Player lastAttackedPlayer = null;
        [JsonIgnore] public string reason = null;
        [JsonIgnore] public CpeMessageType whereHealthIsDisplayed = CpeMessageType.BottomRight2;
        public bool bigUpdate = false;
        public int resetCount = 0;
        public bool oldBarrel = true;
        public Inventory inventory;
        public Position spawnCoords;
        public int[] bedCoords;
        public string spawnMap;
        public DateTime pvpCooldown;
        public bool pvpEnabled = false;
        public bool staff = false;
        public int kills = 0;
        public int exp = 0;
        public int levels = 0;
        public void Message(string message, params object[] args) 
        { 
            p.Message(string.Format(message, args)); 
        }
        public void Send(byte[] buffer)
        {
            p.Socket.Send(buffer, SendFlags.None);
        }
        public void ResetBreaking()
        {
            breakX = breakY = breakZ = ushort.MaxValue;
            if (p.Extras.Contains("nas_taskDisplayMeter"))
            {
                NasBlockChange.breakScheduler.Cancel((SchedulerTask)p.Extras["nas_taskDisplayMeter"]);
            }
        }
        public static NasPlayer GetNasPlayer(Player p)
        {
            if (!p.Extras.Contains(Nas.PlayerKey))
            {
                NasPlayer np = new NasPlayer(p);
                Orientation rot = new Orientation(Server.mainLevel.rotx, Server.mainLevel.roty);
                SetLocation(np, Server.mainLevel.name, Server.mainLevel.SpawnPos, rot);
                p.Extras[Nas.PlayerKey] = np;
                return np; 
            }
            return (NasPlayer)p.Extras[Nas.PlayerKey];
        }
        public bool CanDamage()
        {
            if (p.invincible || p.Game.Referee || !pvpEnabled)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public NasPlayer(Player p)
        {
            this.p = p;
            HP = 10;
            Air = 10;
            inventory = new Inventory(p);
            spawnCoords = Server.mainLevel.SpawnPos;
            bedCoords = new int[] { 238, 94, 179 };
            spawnMap = Server.mainLevel.name;
        }
        public void SetPlayer(Player p, bool log = true)
        {
            if (log)
            {
                Player.Console.Message("setting player in inventory");
            }
            this.p = p;
            inventory.SetPlayer(p);
        }
        public void HandleInteraction(MouseButton button, MouseAction action, ushort x, ushort y, ushort z, byte entityID, TargetBlockFace face)
        {
            if (button == MouseButton.Right && p.ClientHeldBlock != 0)
            {
                ushort xPlacing = x;
                ushort yPlacing = y; 
                ushort zPlacing = z;
                if (face == TargetBlockFace.AwayX) 
                { 
                    xPlacing++; 
                }
                if (face == TargetBlockFace.TowardsX) 
                { 
                    xPlacing--; 
                }
                if (face == TargetBlockFace.AwayY) 
                { 
                    yPlacing++; 
                }
                if (face == TargetBlockFace.TowardsY) 
                { 
                    yPlacing--; 
                }
                if (face == TargetBlockFace.AwayZ) 
                { 
                    zPlacing++; 
                }
                if (face == TargetBlockFace.TowardsZ) 
                { 
                    zPlacing--; 
                }
                if (p.level.GetBlock(xPlacing, yPlacing, zPlacing) == Block.Air)
                {
                    AABB worldAABB = bounds.OffsetPosition(p.Pos);
                    //checking as if its a fully sized block
                    AABB blockAABB = new AABB(0, 0, 0, 32, 32, 32);
                    blockAABB = blockAABB.Offset(xPlacing * 32, yPlacing * 32, zPlacing * 32);
                    if (!AABB.Intersects(ref worldAABB, ref blockAABB))
                    {
                        return;
                    }
                }
            }
            ushort serverushort = p.level.GetBlock(x, y, z);
            ushort clientushort = ConvertBlock(serverushort);
            NasBlock nasBlock = NasBlock.Get(clientushort);
            if (nasBlock.interaction != null)
            {
                if (!CanDoStuffBasedOnPosition)
                {
                    if (action == MouseAction.Released) 
                    { 
                        Message("&cPlease wait a moment before interacting with blocks"); 
                    }
                    return;
                }
                nasBlock.interaction(this, button, action, nasBlock, x, y, z);
            }
        }
        public static Dictionary<string, DateTime> cooldowns = new Dictionary<string, DateTime>();
        public static void StartCooldown(Player p, int milli)
        {
            int milliseconds = milli % 1000;
            int seconds = milli / 1000;
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
            location = new Vec3S32(spawnCoords.X, spawnCoords.Y, spawnCoords.Z);
            lastGroundedLocation = location;
            yaw = 128;
            pitch = 0;
            bigUpdate = true;
            resetCount += 1;//tick this up as well
        }
        public static bool CooledDown(Player p)
        {
            return DateTime.UtcNow.CompareTo(cooldowns[p.name]) != -1;
        }
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
            { 
                expRequired = 2 * levels + 7; 
            }
            else
            {
                if (levels <= 31) 
                { 
                    expRequired = 5 * levels - 38; 
                }
                else 
                {
                    expRequired = 9 * levels - 158; 
                }
            }
            exp += amount;
            while (exp >= expRequired)
            {
                if (exp >= expRequired && levels == 99)
                {
                    exp = expRequired - 1;
                }
                if (exp >= expRequired)
                {
                    exp -= expRequired;
                    levels += 1;
                }
                if (levels <= 16) 
                { 
                    expRequired = 2 * levels + 7; 
                }
                else
                {
                    if (levels <= 31) 
                    { 
                        expRequired = 5 * levels - 38; 
                    }
                    else 
                    { 
                        expRequired = 9 * levels - 158; 
                    }
                }
            }
        }
        public void GiveLevels(int amount)
        {
            levels += amount;
            if (levels > 99)
            {
                levels = 99;
            }
        }
        public int GetExp()
        {
            if (levels <= 16) 
            { 
                return exp + (int)Math.Pow(levels, 2) + 6 * levels; 
            }
            if (levels <= 31) 
            {
                return exp + (int)(2.5 * Math.Pow(levels, 2) - 40.5 * levels + 360); 
            }
            return exp + (int)(4.5 * Math.Pow(levels, 2) - 162.5 * levels + 2220);
        }
        public bool TakeLevels(int amount)
        {
            int expRequired;
            if (levels <= 16) 
            { 
                expRequired = 2 * levels + 7; 
            }
            else
            {
                if (levels <= 31) 
                { 
                    expRequired = 5 * levels - 38; 
                }
                else 
                { 
                    expRequired = 9 * levels - 158; 
                }
            }
            if (amount > levels)
            {
                return false;
            }
            double percentage = (double)exp / expRequired;
            levels -= amount;
            if (levels <= 16) 
            { 
                expRequired = 2 * levels + 7; 
            }
            else
            {
                if (levels <= 31) 
                {
                    expRequired = 5 * levels - 38; 
                }
                else 
                {
                    expRequired = 9 * levels - 158; 
                }
            }
            exp = (int)Math.Floor(percentage * expRequired);
            return true;
        }
        public static void ClickOnPlayer(Player p, byte entity, MouseButton button, MouseAction action)
        {
            NasPlayer np = GetNasPlayer(p);
            if (entity == Entities.SelfID) 
            { 
                return; 
            }
            if ((button == MouseButton.Right && np.inventory.HeldItem.Prop.knockback >= 0) || (button == MouseButton.Left && np.inventory.HeldItem.Prop.knockback < 0) || button == MouseButton.Middle || action == MouseAction.Pressed) 
            { 
                return; 
            }
            if (!cooldowns.ContainsKey(p.name)) 
            { 
                cooldowns.Add(p.name, DateTime.UtcNow); 
            }
            TimeSpan kbdelay = new TimeSpan(0);
            Player[] players = PlayerInfo.Online.Items;
            for (int i = 0; i < players.Length; i++)
            {
                if (!p.EntityList.GetID(players[i], out byte ID))
                {
                    continue;
                }
                if (ID != entity)
                {
                    continue;
                }
                Player who = players[i];
                if (!np.CanDamage())
                {
                    string reason = "";
                    if (np.p.Game.Referee)
                    {
                        reason = "are a referee";
                    }
                    if (np.p.invincible)
                    {
                        reason = "are invincible";
                    }
                    if (!np.pvpEnabled)
                    {
                        reason = "do not have PVP enabled";
                    }
                    np.Message("&SYou cannot damage {0} &Sbecause you currently {1}.", who.DisplayName, reason);
                    return;
                }
                Vec3F32 delta = p.Pos.ToVec3F32() - who.Pos.ToVec3F32();
                float reachSq = p.ReachDistance * p.ReachDistance;
                if (np.inventory.HeldItem.Prop.knockback < 0)
                {
                    reachSq *= 16;
                    kbdelay = new TimeSpan(TimeSpan.TicksPerMillisecond * (int)delta.LengthSquared * 20);
                }
                if (!CooledDown(p))
                {
                    StartCooldown(p, np.inventory.HeldItem.Prop.recharge + (int)(kbdelay.Ticks / TimeSpan.TicksPerMillisecond)); return;
                }
                StartCooldown(p, np.inventory.HeldItem.Prop.recharge + (int)(kbdelay.Ticks / TimeSpan.TicksPerMillisecond));
                if (delta.LengthSquared > (reachSq + 1))
                {
                    return;
                }
                Item Held = np.inventory.HeldItem;
                if (Held.Prop.damage > 0.5f)
                {
                    if (Held.TakeDamage(7))
                    { 
                        np.inventory.BreakItem(ref Held); 
                    }
                }
                else
                {
                    if (Held.TakeDamage(1))
                    { 
                        np.inventory.BreakItem(ref Held); 
                    }
                }
                np.inventory.UpdateItemDisplay();
                NasPlayer w = GetNasPlayer(who);
                w.lastAttackedPlayer = p;
                if (!w.CanTakeDamage(DamageSource.Murder) || !w.CanTakeDamage(DamageSource.Entity))
                {
                    string reason = "";
                    if (w.p.Game.Referee)
                    {
                        reason = who.pronouns.PresentVerb + " a referee";
                    }
                    if (w.p.invincible)
                    {
                        reason = who.pronouns.PresentVerb + " invincible";
                    }
                    if (w.headingToBed)
                    {
                        reason = who.pronouns.PresentVerb + " heading to bed";
                    }
                    if (who.Pos.FeetBlockCoords.X == w.bedCoords[0] && who.Pos.FeetBlockCoords.Y == w.bedCoords[1] && who.Pos.FeetBlockCoords.Z == w.bedCoords[2])
                    {
                        reason = who.pronouns.PresentVerb + " in bed";
                    }
                    if (!w.pvpEnabled)
                    {
                        if (who.pronouns.Plural)
                        {
                            reason = "do not have PVP enabled";
                        }
                        else
                        {
                            reason = "does not have PVP enabled";
                        }
                    }
                    np.Message("&SYou cannot damage {0} &Sbecause " + who.pronouns.Subject + " currently {1}.", who.DisplayName, reason);
                    return;
                }
                if (w.pvpEnabled)
                {
                    float added = 0;
                    if (np.inventory.HeldItem.Enchant("Sharpness") != 0)
                    {
                        added += 1;
                    }
                    added += np.inventory.HeldItem.Enchant("Sharpness") * 0.5f;
                    w.TakeDamage(np.inventory.HeldItem.Prop.damage + added, DamageSource.Entity, "@p %f" + who.pronouns.PastVerb + " slain by " + p.ColoredName + " %fusing " + np.inventory.HeldItem.displayName);
                    NasBlockChange.FishingInfo info = new NasBlockChange.FishingInfo
                    {
                        p = p,
                        who = who
                    };
                    SchedulerTask kbtask = NasBlockChange.fishingScheduler.QueueOnce(np.Knockback, info, kbdelay);
                }
            }
        }
        public void Knockback(SchedulerTask task)
        {
            Player who = ((NasBlockChange.FishingInfo)task.State).who;
            Player p = ((NasBlockChange.FishingInfo)task.State).p;
            int srcHeight = ModelInfo.CalcEyeHeight(p);
            int dstHeight = ModelInfo.CalcEyeHeight(who);
            int dx = p.Pos.X - who.Pos.X, dy = p.Pos.Y + srcHeight - (who.Pos.Y + dstHeight), dz = p.Pos.Z - who.Pos.Z;
            Vec3F32 dir = new Vec3F32(dx, dy, dz);
            dir = Vec3F32.Normalise(dir);
            float mult = inventory.HeldItem.Prop.knockback + 0.25f * inventory.HeldItem.Enchant("Knockback");
            float yChange = 0f;
            if (mult < 0)
            {
                mult = (p.Pos.ToVec3F32() - who.Pos.ToVec3F32()).Length / -2f;
                yChange = -dir.Y;
            }
            if (who.Supports(CpeExt.VelocityControl) && p.Supports(CpeExt.VelocityControl))
            {
                who.Send(Packet.VelocityControl(-dir.X * mult, 0.5f + (yChange * mult), -dir.Z * mult, 0, 1, 0));
            }
            else
            {
                Message("Please update to the latest ClassiCube build to hit with knockback.");
            }
        }
        public override void ChangeHealth(float diff)
        {
            base.ChangeHealth(diff);
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
            { 
                armor = 20f; 
            }
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
        public Item SearchItem(string type, bool takeDamage = false)
        {
            float armor = 0f;
            int i;
            bool done = false;
            int index = 0;
            for (i = 0; i < inventory.items.Length; i++)
            {
                if (inventory.items[i] != null)
                {
                    if (inventory.items[i].armor > armor &&
                        inventory.items[i].name.CaselessContains(type))
                    {
                        armor = inventory.items[i].Prop.armor;
                        index = i;
                        done = true;
                        break;
                    }
                }
            }
            Item saved = new Item("Key");
            if (done)
            {
                saved = inventory.items[index];
            }
            if (done)
            {
                saved.enchants = inventory.items[index].enchants;
            }
            if (armor > 0f && takeDamage)
            {
                double toolDamageChance = 60 + (40.0 / inventory.items[index].Enchant("Unbreaking") + 1);
                Random r = new Random();
                if (r.NextDouble() < toolDamageChance && inventory.items[index].TakeDamage(1))
                {
                    inventory.BreakItem(ref inventory.items[index]);
                }
                inventory.UpdateItemDisplay();
            }
            return saved;
        }
        public override bool CanTakeDamage(DamageSource source)
        {
            if (!pvpEnabled && (source == DamageSource.Murder || source == DamageSource.Entity))
            {
                return false;
            }
            if (p.invincible || p.Game.Referee) 
            { 
                return false; 
            }
            if (headingToBed) 
            { 
                return false; 
            }
            if (p.Pos.FeetBlockCoords.X == bedCoords[0] && p.Pos.FeetBlockCoords.Y == bedCoords[1] && p.Pos.FeetBlockCoords.Z == bedCoords[2])
            { 
                return false; 
            }
            if (!hasBeenSpawned) 
            { 
                Message("If you get this message for more than 5 seconds, rejoin."); 
                return false; 
            }
            if (source == DamageSource.Suffocating)
            {
                TimeSpan timeSinceSuffocation = DateTime.UtcNow.Subtract(lastSuffocationDate);
                if (timeSinceSuffocation.TotalMilliseconds < SuffocationMilliseconds)
                {
                    return false;
                }
                lastSuffocationDate = DateTime.UtcNow;
            }
            return true;
        }
        /// <summary>
        /// returns true if dead
        /// </summary>
        public override bool TakeDamage(float damage, DamageSource source, string customDeathReason = "")
        {
            if (!CanTakeDamage(source)) 
            {
                return false; 
            }
            if (damage == 0) 
            { 
                return false;
            }
            if (source != DamageSource.Drowning)
            {
                damage *= 1 - ((1 - (damage / 50)) * (DamageSaved(true) * 0.04f));
                damage *= 1 - (EnchantLevels("Protection") * 0.04f);
                if (damage > 1f)
                {
                    damage = (float)Math.Round(damage * 2f) / 2f;
                }
            }
            if (source == DamageSource.Falling && EnchantLevels("Feather Falling") > 0)
            {
                damage = Math.Max(0, damage - EnchantLevels("Feather Falling"));
            }
            if (damage == 0) 
            {
                return false; 
            }
            ChangeHealth(-damage);
            curFogColor = Color.FromArgb(255, 255, 0, 0);
            Position next = p.Pos;
            int x = Utils.Clamp(next.BlockX, 0, (ushort)(p.level.Width - 1));
            int z = Utils.Clamp(next.BlockZ, 0, (ushort)(p.level.Length - 1));
            ushort y = (ushort)Utils.Clamp(next.BlockY, 0, (ushort)(p.level.Height - 1));
            if (y < NasGen.oceanHeight) 
            { 
                y = NasGen.oceanHeight;
            }
            float fogMultiplier = 1f + (damage * damage * 0.08f);
            int distanceBelow = nl.biome < 0 ? 0 : y - next.BlockY;
            if (distanceBelow >= NasGen.diamondDepth)
            {
                curRenderDistance = 128 * fogMultiplier;
            }
            else if (distanceBelow >= NasGen.goldDepth)
            {
                curRenderDistance = 192 * fogMultiplier;
            }
            else if (distanceBelow >= NasGen.ironDepth)
            {
                curRenderDistance = 192 * fogMultiplier;
            }
            else if (distanceBelow >= NasGen.coalDepth || nl.biome < 0)
            {
                curRenderDistance = 256 * fogMultiplier;
            }
            else
            {
                curRenderDistance = Server.Config.MaxFogDistance * 0.7f;
            }
            DisplayHealth("f", "&7[", "&7]");
            if (HP <= 0)
            {
                if (customDeathReason.Length == 0)
                {
                    customDeathReason = DeathReason(source, p);
                }
                if (source == DamageSource.Entity)
                {
                    GetNasPlayer(lastAttackedPlayer).kills++;
                    GetNasPlayer(lastAttackedPlayer).GiveLevels(levels / 2);
                    levels -= levels / 2;
                }
                Die(customDeathReason);
                return true;
            }
            SchedulerTask taskDisplayRed;
            taskDisplayRed = Server.MainScheduler.QueueOnce(FinishTakeDamage, this, TimeSpan.FromMilliseconds(100));
            return false;
        }
        public static void FinishTakeDamage(SchedulerTask task)
        {
            NasPlayer np = (NasPlayer)task.State;
            np.DisplayHealth();
        }
        public override void Die(string thing)
        {
            hasBeenSpawned = false;
            isDead = true;
            lastAttackedPlayer = null;
            Player.Console.Message("{0}: hasBeenSpawned set to {1}", p.name, hasBeenSpawned);
            reason = thing.Replace("@p", p.ColoredName);
            PlayerActions.Respawn(p);
        }
        public void TryDropGravestone()
        {
            lock (NasBlock.Container.locker)
            {
                Drop deathDrop = new Drop(inventory);
                if (deathDrop.blockStacks == null && deathDrop.items == null)
                {
                    return;
                }
                Vec3S32 gravePos = p.Pos.FeetBlockCoords;
                p.level.ClampPos(gravePos);
                int x = gravePos.X;
                int y = gravePos.Y;
                int z = gravePos.Z;
                if (x < 0) 
                { 
                    x = 0; 
                }
                if (z < 0) 
                { 
                    z = 0; 
                }
                if (x > 383) 
                { 
                    x = 383; 
                }
                if (z > 383) 
                { 
                    z = 383;
                }
                while (!CanPlaceGraveStone(x, y, z))
                {
                    y++;
                    if (y >= p.level.Height - 1)
                    {
                        Message("Something weird happened. You died in a location such that we can't place a grave.");
                        Message("Sorry, you've lost all your stuff.");
                    }
                }
                //place tombstone
                nl.SetBlock(x, y, z, Block.FromRaw(647));
                NasBlock.Entity blockEntity = new NasBlock.Entity
                {
                    drop = deathDrop
                };
                nl.blockEntities.Add(x + " " + y + " " + z, blockEntity);
                Message("You dropped a gravestone at {0} {1} {2} in {3}", x, y, z, p.level.name);
                File.AppendAllText(Nas.GetDeathPath(p.name), x + " " + y + " " + z + " in " + p.level.name);
                nl.blockEntities[x + " " + y + " " + z].lockedBy = p.name;
                nl.blockEntities[x + " " + y + " " + z].drop.exp = GetExp();
            }
        }
        public bool CanPlaceGraveStone(int x, int y, int z)
        {
            ushort here = nl.GetBlock(x, y, z);
            return NasBlock.CanPhysicsKillThis(here) || NasBlock.IsThisLiquid(here);
        }
        public void SendCpeMessage(CpeMessageType type, string message)
        {
            p.SendCpeMessage(type, message);
        }
        public void DisplayHealth(string healthColor = "p", string prefix = "&7[", string suffix = "&7]¼")
        {
            SendCpeMessage(whereHealthIsDisplayed, OxygenString() + " " + prefix + HealthString(healthColor) + suffix + ArmorDisplay() + " " + ExpDisplay() + " " + AttackRecharge());
        }
        public string HealthString(string healthColor)
        {
            StringBuilder builder = new StringBuilder("&8", (int)maxHP + 6);
            string final;
            float totalLostHealth = maxHP - HP;
            float lostHealthRemaining = totalLostHealth;
            for (int i = 0; i < totalLostHealth; ++i)
            {
                if (lostHealthRemaining < 1)
                {
                    builder.Append("&" + healthColor + "╝"); //broken heart
                }
                else
                {
                    builder.Append("♥"); //empty
                }
                lostHealthRemaining--;
            }
            builder.Append("&" + healthColor);
            for (int i = 0; i < (int)HP; ++i) 
            { 
                builder.Append("♥"); 
            }
            final = builder.ToString();
            return final;
        }
        public string AttackRecharge()
        {
            if (!cooldowns.ContainsKey(p.name)) 
            { 
                return "&aα"; 
            }
            double cooldownPercent = (cooldowns[p.name] - DateTime.UtcNow).TotalMilliseconds / inventory.HeldItem.Prop.recharge;
            if (cooldownPercent >= 0.66) 
            {
                return "&4α"; 
            }
            if (cooldownPercent >= 0.33) 
            { 
                return "&oα"; 
            }
            if (cooldownPercent > 0) 
            { 
                return "&eα"; 
            }
            if (cooldownPercent <= 0) 
            { 
                return "&aα";
            }
            return "&hα";
        }
        public string ArmorDisplay()
        {
            StringBuilder builder = new StringBuilder(5);
            builder.Append("&fΦ ");
            string armor = DamageSaved().ToString();
            builder.Append(armor);
            string final = builder.ToString();
            return final;
        }
        public string ExpDisplay()
        {
            StringBuilder builder = new StringBuilder(5);
            builder.Append("&a☼ ");
            string xp = levels.ToString();
            builder.Append(xp);
            string final = builder.ToString();
            return final;
        }
        public override void UpdateAir()
        {
            base.UpdateAir();
            if (Air != AirPrev && Air == Math.Floor(Air))
            {
                DisplayHealth();
            }
        }
        public string OxygenString()
        {
            if (Air == maxAir) 
            { 
                return ""; 
            }
            if (Air == 0) 
            {
                return "&r┘"; 
            }
            StringBuilder builder = new StringBuilder("", (int)maxAir + 6);
            string final;
            for (int i = 0; i < Air; ++i)
            {
                builder.Append('°');
            }
            final = builder.ToString();
            return final;
        }
        public void UpdateHeldBlock()
        {
            //p.ClientHeldBlock is server block ID
            ushort clientushort = ConvertBlock(p.ClientHeldBlock);
            NasBlock nasBlock = NasBlock.Get(clientushort);
            if (nasBlock.parentID != heldNasBlock.parentID)
            {
                inventory.DisplayHeldBlock(nasBlock);
            }
            heldNasBlock = nasBlock;
        }
        public abstract class NASCommand : Command
        {
            public abstract string Name { get; }
            public override string name { get { return Name; } }
            public override string type { get { return "NAS"; } }
            public override void Use(Player p, string message)
            {
                NasPlayer np = GetNasPlayer(p);
                Use(np, message);
            }
            public abstract void Use(NasPlayer np, string message);
            public override bool SuperUseable { get { return false; } }
            public override bool MessageBlockRestricted { get { return true; } }
        }
        public abstract class NASCommand2 : NASCommand
        {
            public override void Use(NasPlayer np, string message)
            {
                Execute(np.p, message);
            }
            public abstract void Execute(Player p, string message);
        }
        public class CmdGravestones : NASCommand2
        {
            public override string Name { get { return "Gravestones"; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
            public override bool SuperUseable { get { return true; } }
            public override void Execute(Player p, string message)
            {
                string[] args = message.SplitSpaces();
                string name = args[0];
                string PlayerName;
                if (CheckSuper(p, name, "player name"))
                {
                    return;
                }
                if (name.Length == 0)
                {
                    PlayerName = p.name;
                }
                else
                {
                    PlayerData target = PlayerDB.Match(p, name);
                    if (target != null)
                    {
                        PlayerName = target.Name;
                    }
                    else
                    {
                        return;
                    }
                }
                string file = Nas.GetDeathPath(PlayerName);
                if (!File.Exists(file))
                {
                    p.Message("{0}&S has no gravestones recorded!", PlayerName);
                    return;
                }
                //TODO: Simplify this, shouldn't copy deaths
                string[] deaths = File.ReadAllLines(file);
                string[] deaths2 = File.ReadAllLines(file);
                int count = deaths2.Length;
                for (int i = 0; i < deaths2.Length; i++)
                {
                    if (!deaths2[i].IsNullOrWhiteSpace())
                    {
                        foreach (char c in deaths2[i])
                        {
                            string cString = c.ToString();
                            if (char.IsWhiteSpace(c))
                            {
                                deaths2[i] = deaths2[i].Replace(cString, "");
                            }
                        }
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
            public override void Help(Player p)
            {
                p.Message("&T/Gravestones [name] &H- Views the location of the player's gravestones");
            }
        }
        public class CmdMyGravestones : NASCommand2
        {
            public override string Name { get { return "MyGravestones"; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
            public override void Execute(Player p, string message)
            {
                //TODO: Simplify this, shouldn't copy deaths
                string file = Nas.GetDeathPath(p.name);
                string[] deaths = File.ReadAllLines(file);
                string[] deaths2 = File.ReadAllLines(file);
                int count = deaths2.Length;
                for (int i = 0; i < deaths2.Length; i++)
                {
                    if (!deaths2[i].IsNullOrWhiteSpace())
                    {
                        foreach (char c in deaths2[i])
                        {
                            string cString = c.ToString();
                            if (char.IsWhiteSpace(c))
                            {
                                deaths2[i] = deaths2[i].Replace(cString, "");
                            }
                        }
                    }
                    if (deaths2[i].IsNullOrWhiteSpace())
                    {
                        deaths2[i] = null;
                        count--;
                    }
                }
                if (count <= 0)
                {
                    p.Message("&SYou have no gravestones recorded!");
                    return;
                }
                p.Message("Your gravestones:");
                p.MessageLines(deaths);
            }
            public override void Help(Player p)
            {
                p.Message("&T/MyGravestones &H- Views the location of the your own gravestones");
            }
        }
        public class CmdPVP : NASCommand
        {
            public override string Name { get { return "PvP"; } }
            public override void Use(NasPlayer np, string message)
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
            public override string Name { get { return "NASSpawn"; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
            public override void Use(NasPlayer np, string message)
            {
                if (message.IsNullOrEmpty() && np.hasBeenSpawned)
                {
                    Help(np.p);
                    return;
                }
                else if (!message.CaselessContains("confirm") && np.hasBeenSpawned)
                {
                    np.Message("HasBeenSpawned is currently true. If you want to turn off HasBeenSpawned, type &T/NASSpawn confirm");
                    return;
                }
                else if (message.CaselessContains("confirm") && np.hasBeenSpawned)
                {
                    np.hasBeenSpawned = false;
                    np.Message("HasBeenSpawned set to false. Use &T/NASSpawn &Hto turn it back on.");
                    return;
                }
                else if (!np.hasBeenSpawned)
                {
                    np.hasBeenSpawned = true;
                    np.Message("HasBeenSpawned set to true.");
                    return;
                }
            }
            public override void Help(Player p)
            {
                p.Message("&T/NASSpawn &H- Toggles hasBeenSpawned");
            }
        }
        public class CmdSpawnDungeon : NASCommand2
        {
            public override string Name { get { return "SpawnDungeon"; } }
            public override string shortcut {  get { return "GenerateDungeon"; } }
            public override bool museumUsable { get { return false; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
            public override void Execute(Player p, string message)
            {
                p.Message("Generating dungeon.");
                Vec3S32 P = p.Pos.BlockCoords;
                NasGen.GenInstance.GenerateDungeon(p, P.X, P.Y, P.Z, p.level, NasLevel.Get(p.level.name));
            }
            public override void Help(Player p)
            {
                p.Message("&T/SpawnDungeon");
                p.Message("&HGenerates a dungeon.");
            }
        }
        public class CmdBarrelMode : NASCommand
        {
            public override string Name { get { return "BarrelMode"; } }
            public override void Use(NasPlayer np, string message)
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
    }
}
#endif