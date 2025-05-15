#if NAS && !NET_20
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
        public bool bigUpdate = false;
        public int resetCount = 0;
        public bool oldBarrel = true;

        public void ResetBreaking()
        {
            breakX = breakY = breakZ = ushort.MaxValue;
            //NassEffect.UndefineEffect(p, NasBlockChange.BreakMeterID);
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

        public Inventory inventory;
        public Position spawnCoords;
        public int[] bedCoords;
        public string spawnMap;
        [JsonIgnore] public bool hasBeenSpawned = false;
        [JsonIgnore] public bool isDead = false;
        [JsonIgnore] public bool headingToBed = false;
        public DateTime pvpCooldown;
        public bool pvpEnabled = false;
        public bool staff = false;
        public int kills = 0;
        public int exp = 0;
        public int levels = 0;
        [JsonIgnore] public Color targetFogColor = Color.White;
        [JsonIgnore] public Color curFogColor = Color.White;
        [JsonIgnore] public float targetRenderDistance = Server.Config.MaxFogDistance;
        [JsonIgnore] public float curRenderDistance = Server.Config.MaxFogDistance;
        public NasPlayer(Player p)
        {
            this.p = p;
            HP = 10;
            Air = 10;
            inventory = new Inventory(p);
            spawnCoords = Server.mainLevel.SpawnPos;
            bedCoords = new int[] { 238, 94, 179 };
            spawnMap = Server.mainLevel.name;
            //hasBeenSpawned = false;
        }
        public void SetPlayer(Player p)
        {
            Player.Console.Message("setting player in inventory");
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
                if (face == TargetBlockFace.AwayX) { xPlacing++; }
                if (face == TargetBlockFace.TowardsX) { xPlacing--; }
                if (face == TargetBlockFace.AwayY) { yPlacing++; }
                if (face == TargetBlockFace.TowardsY) { yPlacing--; }
                if (face == TargetBlockFace.AwayZ) { zPlacing++; }
                if (face == TargetBlockFace.TowardsZ) { zPlacing--; }
                if (p.level.GetBlock(xPlacing, yPlacing, zPlacing) == Block.Air)
                {
                    //p.Message("It's air");
                    AABB worldAABB = bounds.OffsetPosition(p.Pos);
                    //p.Message("worldAABB is {0}", worldAABB);
                    //checking as if its a fully sized block
                    AABB blockAABB = new AABB(0, 0, 0, 32, 32, 32);
                    blockAABB = blockAABB.Offset(xPlacing * 32, yPlacing * 32, zPlacing * 32);
                    //p.Message("blockAABB is {0}", blockAABB);

                    if (!AABB.Intersects(ref worldAABB, ref blockAABB))
                    {
                        //p.Message("it dont intersects");
                        return;
                    }
                    //p.Message("it intersects");
                }
            }
            ushort serverushort = p.level.GetBlock(x, y, z);
            ushort clientushort = ConvertBlock(serverushort);
            NasBlock nasBlock = NasBlock.Get(clientushort);
            if (nasBlock.interaction != null)
            {
                if (!CanDoStuffBasedOnPosition)
                {
                    if (action == MouseAction.Released) { p.Message("&cPlease wait a moment before interacting with blocks"); }
                    return;
                }
                nasBlock.interaction(this, button, action, nasBlock, x, y, z);
            }
        }

        [JsonIgnore] public Player lastAttackedPlayer = null;
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
            resetCount = 1;//tick this up as well
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
            if (levels <= 16) { expRequired = 2 * levels + 7; }
            else
            {
                if (levels <= 31) { expRequired = 5 * levels - 38; }
                else { expRequired = 9 * levels - 158; }
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

                if (levels <= 16) { expRequired = 2 * levels + 7; }
                else
                {
                    if (levels <= 31) { expRequired = 5 * levels - 38; }
                    else { expRequired = 9 * levels - 158; }
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
            if (levels <= 16) { return exp + (int)Math.Pow(levels, 2) + 6 * levels; }
            if (levels <= 31) { return exp + (int)(2.5 * Math.Pow(levels, 2) - 40.5 * levels + 360); }
            return exp + (int)(4.5 * Math.Pow(levels, 2) - 162.5 * levels + 2220);
        }


        public bool TakeLevels(int amount)
        {
            int expRequired;
            if (levels <= 16) { expRequired = 2 * levels + 7; }
            else
            {
                if (levels <= 31) { expRequired = 5 * levels - 38; }
                else { expRequired = 9 * levels - 158; }
            }

            if (amount > levels)
            {
                return false;
            }
            double percentage = (double)exp / expRequired;
            levels -= amount;

            if (levels <= 16) { expRequired = 2 * levels + 7; }
            else
            {
                if (levels <= 31) { expRequired = 5 * levels - 38; }
                else { expRequired = 9 * levels - 158; }
            }

            exp = (int)Math.Floor(percentage * expRequired);
            return true;
        }

        public static void ClickOnPlayer(Player p, byte entity, MouseButton button, MouseAction action)
        {
            NasPlayer np = GetNasPlayer(p);
            if (entity == Entities.SelfID) { return; }
            if ((button == MouseButton.Right && np.inventory.HeldItem.Prop.knockback >= 0) || (button == MouseButton.Left && np.inventory.HeldItem.Prop.knockback < 0) || button == MouseButton.Middle || action == MouseAction.Pressed) { return; }
            if (!np.pvpEnabled) { return; }
            if (!cooldowns.ContainsKey(p.name)) { cooldowns.Add(p.name, DateTime.UtcNow); }

            TimeSpan kbdelay = new TimeSpan(0);
            Player[] players = PlayerInfo.Online.Items;
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].EntityID != entity) continue;
                Player who = players[i];
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
                if (delta.LengthSquared > (reachSq + 1)) return;
                Item Held = np.inventory.HeldItem;
                if (Held.Prop.damage > 0.5f)
                {
                    if (Held.TakeDamage(7))
                    { np.inventory.BreakItem(ref Held); }
                }
                else
                {
                    if (Held.TakeDamage(1))
                    { np.inventory.BreakItem(ref Held); }
                }
                np.inventory.UpdateItemDisplay();
                NasPlayer w = GetNasPlayer(who);
                w.lastAttackedPlayer = p;
                if (w.pvpEnabled)
                {
                    float added = 0;
                    if (np.inventory.HeldItem.Enchant("Sharpness") != 0) added += 1;
                    added += np.inventory.HeldItem.Enchant("Sharpness") * 0.5f;

                    w.TakeDamage(np.inventory.HeldItem.Prop.damage + added, DamageSource.Entity, "@p %fwas slain by " + p.ColoredName + " %fusing " + np.inventory.HeldItem.displayName);
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
            //float plScale = ModelInfo.GetRawScale(who.Model);
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
                p.Message("Please update to the latest ClassiCube build to hit with knockback.");
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
            if (armor > 20f) { armor = 20f; }
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
            if (done) saved = inventory.items[index];
            if (done) saved.enchants = inventory.items[index].enchants;
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
            //return false;
            if (p.invincible) 
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
                p.Message("if you get this message for more than 5 seconds, rejoin."); 
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

            if (!CanTakeDamage(source)) { return false; }
            if (damage == 0) { return false; }
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
            if (damage == 0) { return false; }
            ChangeHealth(-damage);
            curFogColor = Color.FromArgb(255, 255, 0, 0);

            Position next = p.Pos;
            int x = Utils.Clamp(next.BlockX, 0, (ushort)(p.level.Width - 1));
            int z = Utils.Clamp(next.BlockZ, 0, (ushort)(p.level.Length - 1));
            ushort y = (ushort)Utils.Clamp(next.BlockY, 0, (ushort)(p.level.Height - 1));
            //ushort height = nl.heightmap[x, z];

            if (y < NasGen.oceanHeight) { y = NasGen.oceanHeight; }

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
                    customDeathReason = DeathReason(source);
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
            //p.Send(Packet.VelocityControl(0, 0.25f, 0, 0, 0, 0));
            SchedulerTask taskDisplayRed;
            taskDisplayRed = Server.MainScheduler.QueueOnce(FinishTakeDamage, this, TimeSpan.FromMilliseconds(100));
            return false;
        }
        static void FinishTakeDamage(SchedulerTask task)
        {
            NasPlayer np = (NasPlayer)task.State;
            np.DisplayHealth();
        }
        [JsonIgnore] public string reason = null;
        public override void Die(string thing)
        {
            hasBeenSpawned = false;
            isDead = true;
            lastAttackedPlayer = null;
            //p.Message("hasBeenSpawned set to {0}", hasBeenSpawned);
            Player.Console.Message("{0}: hasBeenSpawned set to {1}", p.name.ToUpper(), hasBeenSpawned);
            reason = thing.Replace("@p", p.ColoredName);
            PlayerActions.Respawn(p);
        }


        public void TryDropGravestone()
        {
            lock (NasBlock.Container.locker)
            {
                //if (inventory.GetAmount(1) == 0) {
                //    p.Message("You need to have at least one stone to drop a gravestone upon death.");
                //    return;
                //}
                //inventory.SetAmount(1, -1, true, true);

                Drop deathDrop = new Drop(inventory);
                if (deathDrop.blockStacks == null && deathDrop.items == null)
                {
                    //p.Message("You didn't drop a gravestone because you had no worldly possessions when you died.");
                    return;
                }

                Vec3S32 gravePos = p.Pos.FeetBlockCoords;
                p.level.ClampPos(gravePos);
                int x = gravePos.X;
                int y = gravePos.Y;
                int z = gravePos.Z;
                if (x < 0) { x = 0; }
                if (z < 0) { z = 0; }
                if (x > 383) { x = 383; }
                if (z > 383) { z = 383; }
                while (!CanPlaceGraveStone(x, y, z))
                {
                    y++;
                    if (y >= p.level.Height - 1)
                    {
                        p.Message("Something weird happened. You died in a location such that we can't place a grave.");
                        p.Message("Sorry, you've lost all your stuff.");
                    }
                }


                //place tombstone
                nl.SetBlock(x, y, z, Block.FromRaw(647));
                NasBlock.Entity blockEntity = new NasBlock.Entity
                {
                    drop = deathDrop
                };
                nl.blockEntities.Add(x + " " + y + " " + z, blockEntity);
                p.Message("You dropped a gravestone at {0} {1} {2} in {3}", x, y, z, p.level.name);
                nl.blockEntities[x + " " + y + " " + z].lockedBy = p.name;
                nl.blockEntities[x + " " + y + " " + z].drop.exp = GetExp();
            }
        }
        public bool CanPlaceGraveStone(int x, int y, int z)
        {
            ushort here = nl.GetBlock(x, y, z);
            return NasBlock.CanPhysicsKillThis(here) || NasBlock.IsThisLiquid(here);
        }

        [JsonIgnore] public CpeMessageType whereHealthIsDisplayed = CpeMessageType.BottomRight2;
        public void DisplayHealth(string healthColor = "p", string prefix = "&7[", string suffix = "&7]¼")
        {
            p.SendCpeMessage(whereHealthIsDisplayed, OxygenString() + " " + prefix + HealthString(healthColor) + suffix + ArmorDisplay() + " " + ExpDisplay() + " " + AttackRecharge());
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
            for (int i = 0; i < (int)HP; ++i) { builder.Append("♥"); }

            final = builder.ToString();
            return final;
        }

        public string AttackRecharge()
        {
            if (!cooldowns.ContainsKey(p.name)) { return "&aα"; }
            double cooldownPercent = (cooldowns[p.name] - DateTime.UtcNow).TotalMilliseconds / inventory.HeldItem.Prop.recharge;
            if (cooldownPercent >= 0.66) { return "&4α"; }
            if (cooldownPercent >= 0.33) { return "&oα"; }
            if (cooldownPercent > 0) { return "&eα"; }
            if (cooldownPercent <= 0) { return "&aα"; }
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
                //p.Message("displaying health");
                DisplayHealth();
            }
        }
        public string OxygenString()
        {
            if (Air == maxAir) { return ""; }
            if (Air == 0) { return "&r┘"; }

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

        public class CmdPVP : Command
        {
            public override string name { get { return "pvp"; } }
            public override string type { get { return "NAS"; } }

            public override void Use(Player p, string message)
            {
                NasPlayer np = GetNasPlayer(p);
                if (message == "on")
                {
                    if (!np.pvpEnabled)
                    {
                        p.Message("You can now attack and be attacked by other players.");
                        np.pvpEnabled = true;
                        np.pvpCooldown = DateTime.UtcNow + new TimeSpan(1, 0, 0);
                        return;
                    }
                    { p.Message("PvP is already enabled"); return; }
                }
                if (message == "off")
                {
                    if (!np.pvpEnabled)
                    {
                        p.Message("PvP is already disabled"); return;
                    }

                    if (np.pvpCooldown > DateTime.UtcNow)
                    {
                        TimeSpan remaining = np.pvpCooldown - DateTime.UtcNow;
                        p.Message("Please wait " + remaining.Minutes + " minutes and " + remaining.Seconds + " seconds before using this command");
                        return;
                    }
                    p.Message("You can no longer attack and be attacked by other players.");
                    np.pvpEnabled = false;
                    return;
                }
                Help(p); return;
            }

            public override void Help(Player p)
            {
                p.Message("&T/pvp [on/off]");
                p.Message("&HToggles pvp, but once you turn it on, you can't turn it off for an hour.");
            }


        }
        public class CmdNASSpawn : Command
        {
            public override string name { get { return "NASSpawn"; } }
            public override string type { get { return "NAS"; } }

            public override void Use(Player p, string message)
            {
                NasPlayer np = GetNasPlayer(p);
                if (message == "true")
                {
                    if (!np.hasBeenSpawned)
                    {
                        p.Message("hasBeenSpawned set to true.");
                        np.hasBeenSpawned = true;
                        return;
                    }
                    else
                    {
                        p.Message("hasBeenSpawned is already true.");
                        return;
                    }
                }
                if (message == "false")
                {
                    if (!np.hasBeenSpawned)
                    {
                        p.Message("hasBeenSpawn is already false.");
                        return;
                    }
                    else
                        p.Message("hasBeenSpawned set to false.");
                    np.hasBeenSpawned = false;
                    return;
                }
                else
                {
                    Help(p);
                    return;
                }
            }

            public override void Help(Player p)
            {
                p.Message("&T/NASSpawn [true/false]");
                p.Message("&HToggles hasBeenSpawned, but once you turn it on, you can't turn it off for 10 seconds.");
            }


        }
        public class CmdSpawnDungeon : Command
        {
            public override string name { get { return "SpawnDungeon"; } }
            public override string type { get { return "NAS"; } }
            public override string shortcut {  get { return "GenerateDungeon"; } }
            public override bool museumUsable { get { return false; } }
            public override LevelPermission defaultRank { get { return LevelPermission.Admin; } }
            public override bool SuperUseable { get { return false; } }
            public override bool MessageBlockRestricted { get { return true; } }
            public override void Use(Player p, string message)
            {
                GenDungeons(p);
            }
            public void GenDungeons(Player p)
            {
                p.Message("Generating dungeon.");
                Vec3S32 P = p.Pos.BlockCoords;
                GenerateDungeon(p, P.X, P.Y, P.Z, p.level, NasLevel.Get(p.level.name));
            }
            public static int MakeInt(string seed)
            {
                if (seed.Length == 0) return new Random().Next();

                if (!int.TryParse(seed, out int value)) value = seed.GetHashCode();
                return value;
            }
            public static void GenerateDungeon(Player p, int x, int y, int z, Level level, NasLevel nsl)
            {
                Random rng = new Random(MakeInt(level.name));
                int genX = x + 2;
                int genZ = z + 2;
                int genY = y;


                for (int dx = 0; dx < 9; dx++)
                    for (int dy = 0; dy < 7; dy++)
                        for (int dz = 0; dz < 9; dz++)
                        {
                            if (rng.Next(0, 3) == 0) level.SetBlock((ushort)(genX + dx), (ushort)(genY + dy), (ushort)(genZ + dz), Block.FromRaw(180));
                            else level.SetTile((ushort)(genX + dx), (ushort)(genY + dy), (ushort)(genZ + dz), Block.StoneBrick);
                        }
                for (int dx = 1; dx < 8; dx++)
                    for (int dy = 2; dy < 6; dy++)
                        for (int dz = 1; dz < 8; dz++)
                        {
                            level.SetTile((ushort)(genX + dx), (ushort)(genY + dy), (ushort)(genZ + dz), Block.Air);
                        }

                int dungeonType = rng.Next(0, 6);



                if (dungeonType == 0)
                {

                    for (int dx = 2; dx < 7; dx++)
                        for (int dz = 2; dz < 7; dz++)
                        {
                            level.SetBlock((ushort)(genX + dx), (ushort)(genY + 2), (ushort)(genZ + dz), Block.FromRaw(476));
                        }
                    level.SetTile((ushort)(genX + 3), (ushort)(genY + 2), (ushort)(genZ + 4), Block.Air);
                    level.SetTile((ushort)(genX + 5), (ushort)(genY + 2), (ushort)(genZ + 4), Block.Air);
                    level.SetTile((ushort)(genX + 4), (ushort)(genY + 2), (ushort)(genZ + 3), Block.Air);
                    level.SetTile((ushort)(genX + 4), (ushort)(genY + 2), (ushort)(genZ + 5), Block.Air);
                    level.SetTile((ushort)(genX + 3), (ushort)(genY + 1), (ushort)(genZ + 4), Block.Air);
                    level.SetTile((ushort)(genX + 5), (ushort)(genY + 1), (ushort)(genZ + 4), Block.Air);
                    level.SetTile((ushort)(genX + 4), (ushort)(genY + 1), (ushort)(genZ + 3), Block.Air);
                    level.SetTile((ushort)(genX + 4), (ushort)(genY + 1), (ushort)(genZ + 5), Block.Air);

                    level.SetTile((ushort)(genX + 4), (ushort)(genY + 4), (ushort)(genZ + 4), Block.StoneBrick);
                    level.SetTile((ushort)(genX + 4), (ushort)(genY + 5), (ushort)(genZ + 4), Block.Lava);
                    nsl.blocksThatMustBeDisturbed.Add(new NasLevel.BlockLocation(genX + 4, genY + 5, genZ + 4));

                    GenLoot(p, genX + 4, genY + 2, genZ + 4, level, rng, nsl);
                    return;
                }

                if (dungeonType == 1)
                {

                    level.SetTile((ushort)(genX + 2), (ushort)(genY + 1), (ushort)(genZ + 2), 48);
                    level.SetTile((ushort)(genX + 2), (ushort)(genY + 1), (ushort)(genZ + 6), 48);
                    level.SetTile((ushort)(genX + 6), (ushort)(genY + 1), (ushort)(genZ + 2), 48);
                    level.SetTile((ushort)(genX + 6), (ushort)(genY + 1), (ushort)(genZ + 6), 48);
                    level.SetBlock((ushort)(genX + 2), (ushort)(genY + 1), (ushort)(genZ + 4), Block.FromRaw(469));
                    level.SetBlock((ushort)(genX + 6), (ushort)(genY + 1), (ushort)(genZ + 4), Block.FromRaw(469));
                    level.SetBlock((ushort)(genX + 4), (ushort)(genY + 1), (ushort)(genZ + 2), Block.FromRaw(469));
                    level.SetBlock((ushort)(genX + 4), (ushort)(genY + 1), (ushort)(genZ + 6), Block.FromRaw(469));

                    level.SetBlock((ushort)(genX + 4), (ushort)(genY + 2), (ushort)(genZ + 4), Block.FromRaw(457));

                    /*for (int dx = 1; dx < 8; dx++)
            			for (int dz = 1; dz < 8; dz++) {
            			if (rng.Next(8) == 0) {
            				level.SetBlock((ushort)(genX+dx), (ushort)(genY+5), (ushort)(genZ+dz), Block.FromRaw(107));
            				if (rng.Next(2) == 0) {
            					level.SetBlock((ushort)(genX+dx), (ushort)(genY+4), (ushort)(genZ+dz), Block.FromRaw(107));
            				}
            			}
            		}*/

                    GenLoot(p, genX + 1, genY + 2, genZ + 1, level, rng, nsl);
                    GenLoot(p, genX + 7, genY + 2, genZ + 7, level, rng, nsl);
                    return;
                }

                if (dungeonType == 2)
                {

                    for (int dx = 1; dx < 8; dx++)
                        for (int dz = 1; dz < 8; dz++)
                        {
                            level.SetTile((ushort)(genX + dx), (ushort)(genY + 1), (ushort)(genZ + dz), Block.Lava);
                        }
                    for (int dx = 1; dx < 8; dx++)
                        for (int dz = 1; dz < 8; dz++)
                        {
                            level.SetBlock((ushort)(genX + dx), (ushort)(genY + 2), (ushort)(genZ + dz), (rng.Next(2) == 0) ? Block.StoneBrick : Block.FromRaw(685));
                        }

                    GenLoot(p, genX + 4, genY + 3, genZ + 4, level, rng, nsl);
                    return;
                }

                if (dungeonType == 3)
                {
                    for (int count = 0; count < 4; count++)
                    {
                        int dx = rng.Next(1, 8);
                        int dz = rng.Next(1, 8);
                        level.SetBlock((ushort)(genX + dx), (ushort)(genY + 2), (ushort)(genZ + dz), Block.FromRaw(604));
                    }

                    for (int count = 0; count < 4; count++)
                    {
                        int dx = rng.Next(1, 8);
                        int dz = rng.Next(1, 8);
                        level.SetBlock((ushort)(genX + dx), (ushort)(genY + 2), (ushort)(genZ + dz), Block.FromRaw(653));
                    }

                    GenLoot(p, genX + 4, genY + 2, genZ + 4, level, rng, nsl);
                    return;
                }

                if (dungeonType == 4)
                {

                    for (int dx = 1; dx < 8; dx++)
                        for (int dy = 1; dy < 6; dy++)
                            for (int dz = 1; dz < 8; dz++)
                            {
                                if (rng.Next(8) == 0) level.SetTile((ushort)(genX + dx), (ushort)(genY + dy), (ushort)(genZ + dz), Block.Lava);
                                else level.SetTile((ushort)(genX + dx), (ushort)(genY + dy), (ushort)(genZ + dz), Block.Stone);
                            }

                    GenLoot(p, genX + 4, genY + 1, genZ + 4, level, rng, nsl);
                    return;
                }

                if (dungeonType == 5)
                {

                    for (int dx = 1; dx < 8; dx++)
                        for (int dz = 1; dz < 8; dz++)
                        {
                            level.SetBlock((ushort)(genX + dx), (ushort)(genY + 1), (ushort)(genZ + dz), Block.FromRaw(129));
                        }

                    level.SetBlock((ushort)(genX + 4), (ushort)(genY + 3), (ushort)(genZ + 4), Block.FromRaw(171));
                    NasBlock.Entity bEntity = new NasBlock.Entity
                    {
                        blockText = "&mCongratulations. You touched grass."
                    };
                    if (!nsl.blockEntities.ContainsKey(genX + 4 + " " + (genY + 3) + " " + (genZ + 4))) nsl.blockEntities.Add(genX + 4 + " " + (genY + 3) + " " + (genZ + 4), bEntity);

                    GenLoot(p, genX + 4, genY + 2, genZ + 4, level, rng, nsl);
                    return;
                }

            }
            public static void GenLoot(Player p, int x, int y, int z, Level level, Random rng, NasLevel nsl)
            {
                level.SetBlock((ushort)x, (ushort)y, (ushort)z, Block.FromRaw(647));
                NasBlock.Entity bEntity = new NasBlock.Entity
                {
                    drop = new Drop(41, rng.Next(1, 5))
                };
                bEntity.drop.blockStacks.Add(new BlockStack(729, rng.Next(0, 3)));
                if (rng.Next(2) == 0)
                {
                    bEntity.drop.blockStacks.Add(new BlockStack(631, rng.Next(1, 3)));
                }
                if (rng.Next(4) == 0)
                { 
                    bEntity.drop.blockStacks.Add(new BlockStack(650)); 
                }
                if (rng.Next(3) == 0)
                {
                    bEntity.drop.blockStacks.Add(new BlockStack(478));
                }
                if (rng.Next(3) == 0)
                {
                    bEntity.drop.blockStacks.Add(new BlockStack(204));
                }
                if (nsl.blockEntities.ContainsKey(x + " " + y + " " + z))
                {
                    nsl.blockEntities.Remove(x + " " + y + " " + z);
                }
                nsl.blockEntities.Add(x + " " + y + " " + z, bEntity);
                p.Message("Generated loot.");
                LevelActions.ReloadAll(level, p, false);
            }
            public override void Help(Player p)
            {
                p.Message("&T/SpawnDungeon");
                p.Message("&HGenerates a dungeon.");
            }
        }
        public class CmdBarrelMode : Command
        {
            public override string name { get { return "barrelmode"; } }
            public override string type { get { return "NAS"; } }

            public override void Use(Player p, string message)
            {
                NasPlayer np = GetNasPlayer(p);
                if (np.oldBarrel)
                {
                    p.Message("You now have to input the amount of items you want to put into the barrel manually.");
                    np.oldBarrel = false;
                }
                else
                {
                    p.Message("Half of the items you're holding will now be put into the barrel.");
                    np.oldBarrel = true;
                }
            }

            public override void Help(Player p)
            {
                p.Message("&T/barrelmode");
                p.Message("&HToggles how you enter items into barrels.");
            }


        }
    }
}
#endif