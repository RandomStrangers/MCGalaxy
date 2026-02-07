using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Modules.Warps;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NASBlockExistAction =
    MCGalaxy.NASAction<MCGalaxy.NASPlayer,
    MCGalaxy.NASBlock, bool, ushort, ushort, ushort>;
using NASBlockInteraction =
    MCGalaxy.NASAction<MCGalaxy.NASPlayer, MCGalaxy.Events.PlayerEvents.MouseButton, MCGalaxy.Events.PlayerEvents.MouseAction,
    MCGalaxy.NASBlock, ushort, ushort, ushort>;
namespace MCGalaxy
{
    public partial class NASBlock
    {
        public const string Path = NASPlugin.SavePath + "Blocks/";
        public static List<string> books = ReadAllLinesList("text/BookTitles.txt"),
            authors = ReadAllLinesList("text/BookAuthors.txt");
        public static ushort[] waffleSet = { 256 | 542, 256 | 543 },
            breadSet = new ushort[] { 256 | 640, 256 | 641, 256 | 642 },
            pieSet = new ushort[] { 256 | 668, 256 | 669, 256 | 670, 256 | 671 },
            peachPieSet = new ushort[] { 256 | 698, 256 | 699, 256 | 700, 256 | 701 };
        public static List<string> ReadAllLinesList(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            List<string> lines = new();
            using (StreamReader r = new(path, Encoding.UTF8))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }
        public static string GetTextPath(Player p) => Path + p.name + ".txt";
        public class NASBlockEntity
        {
            public string lockedBy = "",
                blockText = "";
            public int strength = 0, 
                type = 0,
                direction = 0;
            public NASDrop drop = null;
            [JsonIgnore]
            public string FormattedNameOfLocker
            {
                get
                {
                    if (lockedBy.Length == 0)
                    {
                        return "no one";
                    }
                    Player locker = PlayerInfo.FindExact(lockedBy);
                    return locker == null ? lockedBy : locker.ColoredName;
                }
            }
            public bool CanAccess(NASPlayer np) => lockedBy.Length == 0 || lockedBy == np.p.name;
        }
        public class NASContainer
        {
            public const int ToolLimit = 27, BlockStackLimit = 27;
            public enum NASContainerType 
            {
                Chest, Barrel, Crate, 
                Gravestone, AutoCraft, Dispenser 
            }
            public NASContainerType type;
            public string Name => Enum.GetName(typeof(NASContainerType), type);
            public string Description
            {
                get
                {
                    string desc = "&s";
                    desc += type switch
                    {
                        NASContainerType.Chest => Name + "s&S can store &btools&S, with a limit of " + ToolLimit + ".",
                        NASContainerType.Barrel or NASContainerType.Crate or NASContainerType.Dispenser => Name + "s&S can store &bblock&S stacks, with a limit of " + BlockStackLimit + ".",
                        _ => throw new Exception("Invalid value for Type"),
                    };
                    return desc;
                }
            }
            public NASContainer() { }
            public NASContainer(NASContainer parent) => type = parent.type;
        }
        public static NASBlockExistAction WaterExistAction() => (np, nasBlock, exists, x, y, z) =>
                                                                         {
                                                                             if (exists)
                                                                             {
                                                                                 np.inventory.SetAmount(143, 1, false, false);
                                                                             }
                                                                         };
        public static NASBlockExistAction LavaExistAction() => (np, nasBlock, exists, x, y, z) =>
                                                                        {
                                                                            if (exists)
                                                                            {
                                                                                np.inventory.SetAmount(697, 1, false, false);
                                                                            }
                                                                        };
        public static NASBlockInteraction BookshelfInteraction() => (np, button, action, nasBlock, x, y, z) =>
                                                                             {
                                                                                 if (action == MouseAction.Pressed)
                                                                                 {
                                                                                     return;
                                                                                 }
                                                                                 np.Message(books[r.Next(0, 99)] + " by " + authors[r.Next(0, 74)]);
                                                                             };
        public static NASBlockInteraction CrateInteraction(string text) => (np, button, action, nasBlock, x, y, z) =>
                                                                                    {
                                                                                        if (action == MouseAction.Pressed)
                                                                                        {
                                                                                            return;
                                                                                        }
                                                                                        np.Message(text);
                                                                                    };
        public static NASBlockInteraction BedInteraction() => (np, button, action, nasBlock, x, y, z) =>
                                                                       {
                                                                           if (action == MouseAction.Pressed || button != MouseButton.Right)
                                                                           {
                                                                               return;
                                                                           }
                                                                           np.Message("Spawnpoint set");
                                                                           Position coords = np.p.Pos;
                                                                           np.Teleport(x + " " + y + " " + z);
                                                                           np.spawnCoords = np.p.Pos;
                                                                           WarpList list = new()
                                                                           {
                                                                               Filename = "extra/Waypoints/" + np.p.name + "_nas.txt"
                                                                           };
                                                                           if (!np.p.Extras.Contains("NAS_WAYPOINTS"))
                                                                           {
                                                                               np.p.Extras["NAS_WAYPOINTS"] = list;
                                                                           }
                                                                           WarpList waypoints = (WarpList)np.p.Extras["NAS_WAYPOINTS"];
                                                                           if (!waypoints.Exists("Bed"))
                                                                           {
                                                                               waypoints.Create("Bed", np.p);
                                                                           }
                                                                           waypoints.Update(waypoints.Find("Bed"), np.p);
                                                                           np.Teleport("-precise " + coords.X + " " + (coords.Y - 50) + " " + coords.Z);
                                                                           np.spawnMap = np.p.Level.name;
                                                                           np.bedCoords = new int[]
                                                                           {
                    x, y, z
                                                                           };
                                                                           if (!(NASTimeCycle.cycleCurrentTime < 20 * NASTimeCycle.hourMinutes &&
                                                                               NASTimeCycle.cycleCurrentTime >= 7 * NASTimeCycle.hourMinutes))
                                                                           {
                                                                               NASTimeCycle.cycleCurrentTime = 4200;
                                                                               Chat.MessageChat(np.p, "&fSay thanks to " + np.p.ColoredName + " &ffor skipping the night!", null, true);
                                                                           }
                                                                       };
        public static NASBlockInteraction SmithingTableAction() => (np, button, action, nasBlock, x, y, z) =>
                                                                            {
                                                                                if (action == MouseAction.Pressed || button != MouseButton.Right)
                                                                                {
                                                                                    return;
                                                                                }
                                                                                ushort aboveHere = np.nl.GetBlock(x, y + 1, z);
                                                                                float maxHP = np.inventory.HeldItem.Prop.baseHP;
                                                                                if (np.inventory.HeldItem.name.CaselessContains("emerald") && aboveHere == NASPlugin.FromRaw(650))
                                                                                {
                                                                                    np.inventory.HeldItem.HP = np.inventory.HeldItem.HP + (0.4f * maxHP);
                                                                                    if (np.inventory.HeldItem.HP > maxHP)
                                                                                    {
                                                                                        np.inventory.HeldItem.HP = maxHP;
                                                                                    }
                                                                                    np.nl.SetBlock(x, y + 1, z, 0);
                                                                                    np.Message("Repaired your {0}!", np.inventory.HeldItem.displayName);
                                                                                    return;
                                                                                }
                                                                                if (np.inventory.HeldItem.name.CaselessContains("diamond") && aboveHere == NASPlugin.FromRaw(631))
                                                                                {
                                                                                    np.inventory.HeldItem.HP = np.inventory.HeldItem.HP + (0.4f * maxHP);
                                                                                    if (np.inventory.HeldItem.HP > maxHP)
                                                                                    {
                                                                                        np.inventory.HeldItem.HP = maxHP;
                                                                                    }
                                                                                    np.nl.SetBlock(x, y + 1, z, 0);
                                                                                    np.Message("Repaired your {0}!", np.inventory.HeldItem.displayName);
                                                                                    return;
                                                                                }
                                                                                if (np.inventory.HeldItem.name.CaselessContains("gold") && aboveHere == NASPlugin.FromRaw(41))
                                                                                {
                                                                                    np.inventory.HeldItem.HP = np.inventory.HeldItem.HP + (0.4f * maxHP);
                                                                                    if (np.inventory.HeldItem.HP > maxHP)
                                                                                    {
                                                                                        np.inventory.HeldItem.HP = maxHP;
                                                                                    }
                                                                                    np.nl.SetBlock(x, y + 1, z, 0);
                                                                                    np.Message("Repaired your {0}!", np.inventory.HeldItem.displayName);
                                                                                    return;
                                                                                }
                                                                                if (np.inventory.HeldItem.name.CaselessContains("iron") && aboveHere == NASPlugin.FromRaw(42))
                                                                                {
                                                                                    np.inventory.HeldItem.HP = np.inventory.HeldItem.HP + (0.4f * maxHP);
                                                                                    if (np.inventory.HeldItem.HP > maxHP)
                                                                                    {
                                                                                        np.inventory.HeldItem.HP = maxHP;
                                                                                    }
                                                                                    np.nl.SetBlock(x, y + 1, z, 0);
                                                                                    np.Message("Repaired your {0}!", np.inventory.HeldItem.displayName);
                                                                                    return;
                                                                                }
                                                                                if (aboveHere == NASPlugin.FromRaw(171) && np.nl.blockEntities[x + " " + (y + 1) + " " + z].CanAccess(np) && np.nl.blockEntities[x + " " + (y + 1) + " " + z].blockText != "")
                                                                                {
                                                                                    string[] words = np.nl.blockEntities[x + " " + (y + 1) + " " + z].blockText.Split(new[] { ':' }, 2);
                                                                                    np.inventory.HeldItem.displayName = words[1].Remove(0, 1);
                                                                                    np.nl.SetBlock(x, y + 1, z, 0);
                                                                                    np.nl.blockEntities.Remove(x + " " + (y + 1) + " " + z);
                                                                                    np.Message("Changed your tool's name to {0}!", np.inventory.HeldItem.displayName);
                                                                                    return;
                                                                                }
                                                                                np.Message("No valid recipes available.");
                                                                            };
        public static NASBlockExistAction BeaconInteractAction() => (np, nasBlock, exists, x, y, z) =>
                                                                             {
                                                                                 if (exists)
                                                                                 {
                                                                                     np.inventory.SetAmount(1, 1, true, true);
                                                                                     np.nl.SetBlock(x, y, z, 0);
                                                                                     bool inv = np.p.invincible;
                                                                                     if (!inv)
                                                                                     {
                                                                                         np.p.invincible = true;
                                                                                     }
                                                                                     np.SendToMain();
                                                                                     np.lastGroundedLocation = new(Server.mainLevel.SpawnPos.X, Server.mainLevel.SpawnPos.Y, Server.mainLevel.SpawnPos.Z);
                                                                                     NASBlockChange.NASInvInfo invInfo = new()
                                                                                     {
                                                                                         np = np,
                                                                                         inv = inv
                                                                                     };
                                                                                     SchedulerTask repeaterTask = NASBlockChange.repeaterScheduler.QueueOnce(InvTask, invInfo, new(0, 0, 0, 1, 0));
                                                                                 }
                                                                             };
        public static void InvTask(SchedulerTask task)
        {
            if (!((NASBlockChange.NASInvInfo)task.State).inv)
            {
                ((NASBlockChange.NASInvInfo)task.State).np.p.invincible = false;
            }
        }
        public static NASBlockExistAction BedBeaconAction() => (np, nasBlock, exists, x, y, z) =>
                                                                        {
                                                                            if (exists)
                                                                            {
                                                                                if (np.isDead)
                                                                                {
                                                                                    return;
                                                                                }
                                                                                np.headingToBed = true;
                                                                                np.isDead = true;
                                                                                np.nl.SetBlock(x, y, z, 0);
                                                                                np.inventory.SetAmount(612, 1, true, true);
                                                                                np.Die("");
                                                                            }
                                                                        };
        public static NASBlockExistAction MessageExistAction() => (np, nasBlock, exists, x, y, z) =>
                                                                           {
                                                                               if (exists)
                                                                               {
                                                                                   if (np.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                                                                                   {
                                                                                       np.nl.blockEntities.Remove(x + " " + y + " " + z);
                                                                                   }
                                                                                   np.nl.blockEntities.Add(x + " " + y + " " + z, new());
                                                                                   np.nl.blockEntities[x + " " + y + " " + z].lockedBy = np.p.name;
                                                                                   np.Message("To read the sign, right click.");
                                                                                   np.Message("To rewrite the sign, use the /sign command then middle click.");
                                                                                   return;
                                                                               }
                                                                               np.nl.blockEntities.Remove(x + " " + y + " " + z);
                                                                           };
        public static NASBlockInteraction StripInteraction(ushort toThis, string checkString = "Axe") => (np, button, action, nasBlock, x, y, z) =>
                                                                                                                  {
                                                                                                                      if (action == MouseAction.Pressed || button != MouseButton.Right)
                                                                                                                      {
                                                                                                                          return;
                                                                                                                      }
                                                                                                                      if (np.inventory.HeldItem.name.Contains(checkString))
                                                                                                                      {
                                                                                                                          NASItem Held = np.inventory.HeldItem;
                                                                                                                          if (np.inventory.HeldItem.TakeDamage(1))
                                                                                                                          {
                                                                                                                              np.inventory.BreakItem(ref Held);
                                                                                                                          }
                                                                                                                          np.inventory.UpdateItemDisplay();
                                                                                                                          np.nl.SetBlock(x, y, z, toThis);
                                                                                                                      }
                                                                                                                  };
        public static NASBlockInteraction MessageInteraction() => (np, button, action, nasBlock, x, y, z) =>
                                                                           {
                                                                               string file = GetTextPath(np.p);
                                                                               if (!File.Exists(file))
                                                                               {
                                                                                   FileIO.TryWriteAllText(file, string.Empty);
                                                                               }
                                                                               string myText = FileIO.TryReadAllText(file);
                                                                               if (!np.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                                                                               {
                                                                                   np.nl.blockEntities.Add(x + " " + y + " " + z, new());
                                                                                   np.Message("This sign's data got deleted. Now, it's a public sign!");
                                                                               }
                                                                               NASBlockEntity bEntity = np.nl.blockEntities[x + " " + y + " " + z];
                                                                               if (action == MouseAction.Pressed | button == MouseButton.Left)
                                                                               {
                                                                                   return;
                                                                               }
                                                                               if (button == MouseButton.Right)
                                                                               {
                                                                                   np.Message(bEntity.blockText);
                                                                               }
                                                                               if ((button == MouseButton.Middle) && (myText != ""))
                                                                               {
                                                                                   if (!bEntity.CanAccess(np))
                                                                                   {
                                                                                       return;
                                                                                   }
                                                                                   bEntity.blockText = np.p.ColoredName + " &Ssays: " + myText;
                                                                                   FileIO.TryWriteAllText(file, string.Empty);
                                                                                   np.Message("Overwritten!");
                                                                               }
                                                                           };
        public static NASBlockExistAction LavaBarrelAction() => (np, nasBlock, exists, x, y, z) =>
                                                                         {
                                                                             if (exists)
                                                                             {
                                                                                 if (np.inventory.GetAmount(696) >= 5)
                                                                                 {
                                                                                     np.Message("&mYou have too many lava barrels!");
                                                                                     return;
                                                                                 }
                                                                                 bool[] isLava = { false, false, false, false, false, false };
                                                                                 if (np.nl.GetBlock(x, y + 1, z) == 10)
                                                                                 {
                                                                                     isLava[0] = true;
                                                                                 }
                                                                                 if (np.nl.GetBlock(x + 1, y, z) == 10)
                                                                                 {
                                                                                     isLava[1] = true;
                                                                                 }
                                                                                 if (np.nl.GetBlock(x - 1, y, z) == 10)
                                                                                 {
                                                                                     isLava[2] = true;
                                                                                 }
                                                                                 if (np.nl.GetBlock(x, y, z - 1) == 10)
                                                                                 {
                                                                                     isLava[3] = true;
                                                                                 }
                                                                                 if (np.nl.GetBlock(x, y, z + 1) == 10)
                                                                                 {
                                                                                     isLava[4] = true;
                                                                                 }
                                                                                 if (isLava[0] || isLava[1] || isLava[2] || isLava[3] || isLava[4] || isLava[5])
                                                                                 {
                                                                                     np.nl.SetBlock(x, y, z, 0);
                                                                                     if ((isLava[0] || isLava[5]) && IsPartOfSet(lavaSet, np.nl.GetBlock(x, y + 1, z)) != -1)
                                                                                     {
                                                                                         np.nl.SetBlock(x, y + 1, z, 0);
                                                                                     }
                                                                                     if ((isLava[1] || isLava[5]) && IsPartOfSet(lavaSet, np.nl.GetBlock(x + 1, y, z)) != -1)
                                                                                     {
                                                                                         np.nl.SetBlock(x + 1, y, z, 0);
                                                                                     }
                                                                                     if ((isLava[2] || isLava[5]) && IsPartOfSet(lavaSet, np.nl.GetBlock(x - 1, y, z)) != -1)
                                                                                     {
                                                                                         np.nl.SetBlock(x - 1, y, z, 0);
                                                                                     }
                                                                                     if ((isLava[3] || isLava[5]) && IsPartOfSet(lavaSet, np.nl.GetBlock(x, y, z - 1)) != -1)
                                                                                     {
                                                                                         np.nl.SetBlock(x, y, z - 1, 0);
                                                                                     }
                                                                                     if ((isLava[4] || isLava[5]) && IsPartOfSet(lavaSet, np.nl.GetBlock(x, y, z + 1)) != -1)
                                                                                     {
                                                                                         np.nl.SetBlock(x, y, z + 1, 0);
                                                                                     }
                                                                                     np.inventory.SetAmount(696, 1, true, true);
                                                                                     np.inventory.DisplayHeldBlock(blocks[697], -1, false);
                                                                                     return;
                                                                                 }
                                                                             }
                                                                         };
        public static NASBlockExistAction SmithingAction() => (np, nasBlock, exists, x, y, z) =>
                                                                       {
                                                                           if (exists)
                                                                           {
                                                                               np.Message("You placed a &bSmithing table&S!");
                                                                               np.Message("Place the block you want to repair with on top.");
                                                                               np.Message("Then right click with the tool you want to repair.");
                                                                           }
                                                                       };
        public static NASBlockInteraction PortalInteraction() => (np, button, action, nasBlock, x, y, z) =>
                                                                          {
                                                                              if (button != MouseButton.Right || action == MouseAction.Pressed)
                                                                              {
                                                                                  return;
                                                                              }
                                                                              int absoluteX, absoluteZ, lvlX = 0, lvlZ = 0;
                                                                              string seed = "";
                                                                              NASGen.GetSeedAndChunkOffset(np.nl.lvl.name, ref seed, ref lvlX, ref lvlZ);
                                                                              absoluteX = lvlX * np.nl.lvl.Width;
                                                                              absoluteZ = lvlZ * np.nl.lvl.Length;
                                                                              absoluteX += x;
                                                                              absoluteZ += z;
                                                                              bool nether = seed.CaselessContains("-nether");
                                                                              double mult = nether ? 8 : (double)1 / 8;
                                                                              absoluteX = (int)Math.Floor(absoluteX * mult);
                                                                              absoluteZ = (int)Math.Floor(absoluteZ * mult);
                                                                              int levelX = (int)Math.Floor((double)absoluteX / np.nl.lvl.Width),
                                                                              levelZ = (int)Math.Floor((double)absoluteZ / np.nl.lvl.Length);
                                                                              string newLevel = seed.Replace("-nether", "") + (!nether ? "-nether" : "") + "_" + levelX + "," + levelZ;
                                                                              int withX = absoluteX - levelX * np.nl.lvl.Width,
                                                                              withZ = absoluteZ - levelZ * np.nl.lvl.Length;
                                                                              withX = Math.Min(np.nl.lvl.Width - 2, Math.Max(1, withX));
                                                                              withZ = Math.Min(np.nl.lvl.Length - 2, Math.Max(1, withZ));
                                                                              int withY = Math.Min(245, Math.Max(32, (int)y));
                                                                              Level lvl = LevelInfo.FindExact(newLevel);
                                                                              if (!LevelInfo.Loaded.Contains(lvl))
                                                                              {
                                                                                  LevelActions.Load(np.p, newLevel, false);
                                                                              }
                                                                              Level grab = Level.Load(newLevel);
                                                                              int dX = 0, dY = 0, dZ = 0;
                                                                              double minDist = 100000;
                                                                              bool worked = false;
                                                                              if (grab != null)
                                                                              {
                                                                                  for (int offX = nether ? -32 : -8; offX <= (nether ? 32 : 8); offX++)
                                                                                  {
                                                                                      for (int offZ = nether ? -32 : -8; offZ <= (nether ? 32 : 8); offZ++)
                                                                                      {
                                                                                          for (int offY = 2 - withY; offY + withY <= 245; offY++)
                                                                                          {
                                                                                              if (grab.GetBlock((ushort)(offX + withX), (ushort)(offY + withY), (ushort)(offZ + withZ)) == NASPlugin.FromRaw(457))
                                                                                              {
                                                                                                  double tempDist = Math.Sqrt(offX * offX + offZ * offZ);
                                                                                                  if (tempDist < minDist)
                                                                                                  {
                                                                                                      minDist = tempDist;
                                                                                                      dX = offX;
                                                                                                      dY = offY;
                                                                                                      dZ = offZ;
                                                                                                      worked = true;
                                                                                                  }
                                                                                              }
                                                                                          }
                                                                                      }
                                                                                  }
                                                                                  withX += dX;
                                                                                  withY += dY;
                                                                                  withZ += dZ;
                                                                                  dY = 0;
                                                                                  if (!worked)
                                                                                  {
                                                                                      for (int offY = 32 - withY; offY + withY <= 245; offY++)
                                                                                      {
                                                                                          ushort block1 = grab.FastGetBlock((ushort)withX, (ushort)(withY + offY), (ushort)withZ),
                                                                                          block2 = grab.FastGetBlock((ushort)withX, (ushort)(withY + offY + 1), (ushort)withZ);
                                                                                          if (block2 == 0 && (block1 == NASPlugin.FromRaw(457) || block1 == 0))
                                                                                          {
                                                                                              dY = offY;
                                                                                              worked = true;
                                                                                              break;
                                                                                          }
                                                                                      }
                                                                                  }
                                                                              }
                                                                              np.NetherTravel(newLevel, new(np.p, levelX - lvlX, levelZ - lvlZ, withX, withY + dY, withZ));
                                                                          };
        public static NASBlockExistAction ContainerExistAction() => (np, nasBlock, exists, x, y, z) =>
                                                                             {
                                                                                 if (exists && nasBlock.container.type == NASContainer.NASContainerType.Barrel)
                                                                                 {
                                                                                     if (
                                                                                         IsPartOfSet(waterSet, np.nl.GetBlock(x, y + 1, z)) != -1 ||
                                                                                         IsPartOfSet(waterSet, np.nl.GetBlock(x + 1, y, z)) != -1 ||
                                                                                         IsPartOfSet(waterSet, np.nl.GetBlock(x - 1, y, z)) != -1 ||
                                                                                         IsPartOfSet(waterSet, np.nl.GetBlock(x, y, z + 1)) != -1 ||
                                                                                         IsPartOfSet(waterSet, np.nl.GetBlock(x, y, z - 1)) != -1
                                                                                        )
                                                                                     {
                                                                                         np.nl.SetBlock(x, y, z, 0);
                                                                                         np.inventory.SetAmount(643, 1, true, true);
                                                                                         np.inventory.DisplayHeldBlock(blocks[143], -1, false);
                                                                                         return;
                                                                                     }
                                                                                 }
                                                                                 if (exists)
                                                                                 {
                                                                                     if (nasBlock.container.type == NASContainer.NASContainerType.Gravestone)
                                                                                     {
                                                                                         return;
                                                                                     }
                                                                                     if (np.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                                                                                     {
                                                                                         np.nl.blockEntities.Remove(x + " " + y + " " + z);
                                                                                     }
                                                                                     np.nl.blockEntities.Add(x + " " + y + " " + z, new());
                                                                                     np.Message(nasBlock.container.Description);
                                                                                     np.Message("To insert, select what you want to store, then left click.");
                                                                                     np.Message("To extract, right click.");
                                                                                     np.Message("To inspect status, middle click.");
                                                                                     return;
                                                                                 }
                                                                                 np.nl.blockEntities.Remove(x + " " + y + " " + z);
                                                                             };
        public static NASBlockInteraction ChangeInteraction(ushort toggle) => (np, button, action, nasBlock, x, y, z) =>
                                                                                       {
                                                                                           if (!np.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                                                                                           {
                                                                                               np.nl.blockEntities.Add(x + " " + y + " " + z, new());
                                                                                           }
                                                                                           if (action == MouseAction.Pressed)
                                                                                           {
                                                                                               return;
                                                                                           }
                                                                                           if (button == MouseButton.Right)
                                                                                           {
                                                                                               if (toggle == NASPlugin.FromRaw(675) || toggle == NASPlugin.FromRaw(196))
                                                                                               {
                                                                                                   np.nl.blockEntities[x + " " + y + " " + z].strength = 15;
                                                                                               }
                                                                                               if (toggle == NASPlugin.FromRaw(674))
                                                                                               {
                                                                                                   np.nl.blockEntities[x + " " + y + " " + z].strength = 0;
                                                                                               }
                                                                                               np.nl.SetBlock(x, y, z, toggle);
                                                                                               return;
                                                                                           }
                                                                                       };
        public static NASBlockInteraction AutoCraftInteraction() => (np, button, action, nasBlock, x, y, z) =>
                                                                             {
                                                                                 if (action == MouseAction.Pressed)
                                                                                 {
                                                                                     return;
                                                                                 }
                                                                                 NASBlockEntity bEntity = np.nl.blockEntities[x + " " + y + " " + z];
                                                                                 if (button == MouseButton.Middle)
                                                                                 {
                                                                                     CheckContents(np, nasBlock, bEntity);
                                                                                     return;
                                                                                 }
                                                                                 if (button == MouseButton.Left)
                                                                                 {
                                                                                     if (np.inventory.HeldItem.name == "Key")
                                                                                     {
                                                                                         np.Message("You cannot lock auto crafters.");
                                                                                         return;
                                                                                     }
                                                                                     np.Message("You can right click to remove items from auto crafters.");
                                                                                 }
                                                                                 if (button == MouseButton.Right)
                                                                                 {
                                                                                     RemoveAll(np, bEntity, false);
                                                                                     return;
                                                                                 }
                                                                             };
        public static NASBlockInteraction ContainerInteraction() => (np, button, action, nasBlock, x, y, z) =>
                                                                             {
                                                                                 if (action == MouseAction.Pressed)
                                                                                 {
                                                                                     return;
                                                                                 }
                                                                                 if (np.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                                                                                 {
                                                                                     NASBlockEntity bEntity = np.nl.blockEntities[x + " " + y + " " + z];
                                                                                     if (!bEntity.CanAccess(np) && button != MouseButton.Middle)
                                                                                     {
                                                                                         np.Message("This {0} is locked by {1}&S.", nasBlock.container.Name.ToLower(), bEntity.FormattedNameOfLocker);
                                                                                         return;
                                                                                     }
                                                                                     if (button == MouseButton.Middle)
                                                                                     {
                                                                                         CheckContents(np, nasBlock, bEntity);
                                                                                         return;
                                                                                     }
                                                                                     if (button == MouseButton.Left)
                                                                                     {
                                                                                         if (np.inventory.HeldItem.name == "Key")
                                                                                         {
                                                                                             if (nasBlock.container.type == NASContainer.NASContainerType.Gravestone || nasBlock.container.type == NASContainer.NASContainerType.Dispenser)
                                                                                             {
                                                                                                 np.Message("You cannot lock gravestones or dispensers.");
                                                                                             }
                                                                                             else if (bEntity.lockedBy.Length == 0)
                                                                                             {
                                                                                                 bEntity.lockedBy = np.p.name;
                                                                                                 np.Message("You &flock&S the {0}. Only you can access it now.", nasBlock.container.Name.ToLower());
                                                                                                 return;
                                                                                             }
                                                                                         }
                                                                                         if (nasBlock.container.type == NASContainer.NASContainerType.Gravestone)
                                                                                         {
                                                                                             np.Message("You can right click to extract from tombstones.");
                                                                                             return;
                                                                                         }
                                                                                         if (nasBlock.container.type == NASContainer.NASContainerType.Chest)
                                                                                         {
                                                                                             AddTool(np, bEntity);
                                                                                         }
                                                                                         else
                                                                                         {
                                                                                             AddBlocks(np, x, y, z);
                                                                                         }
                                                                                         np.nl.SimulateSetBlock(x, y, z);
                                                                                         return;
                                                                                     }
                                                                                     if (button == MouseButton.Right)
                                                                                     {
                                                                                         if (np.inventory.HeldItem.name == "Key")
                                                                                         {
                                                                                             if (bEntity.lockedBy.Length > 0)
                                                                                             {
                                                                                                 bEntity.lockedBy = "";
                                                                                                 np.Message("You &funlock&S the {0}. Anyone can access it now.", nasBlock.container.Name.ToLower());
                                                                                                 return;
                                                                                             }
                                                                                         }
                                                                                         if (nasBlock.container.type == NASContainer.NASContainerType.Chest)
                                                                                         {
                                                                                             RemoveTool(np, bEntity);
                                                                                         }
                                                                                         else if (nasBlock.container.type == NASContainer.NASContainerType.Barrel || nasBlock.container.type == NASContainer.NASContainerType.Dispenser)
                                                                                         {
                                                                                             RemoveBlocks(np, bEntity);
                                                                                         }
                                                                                         else if (nasBlock.container.type == NASContainer.NASContainerType.Gravestone)
                                                                                         {
                                                                                             string file = NASPlugin.GetDeathPath(np.p.name);
                                                                                             if (!File.Exists(file))
                                                                                             {
                                                                                                 File.Create(NASPlugin.GetDeathPath(np.p.name)).Dispose();
                                                                                             }
                                                                                             string[] locations = FileIO.TryReadAllLines(NASPlugin.GetDeathPath(np.p.name)),
                                                                                             newLocations = new string[locations.Length];
                                                                                             for (int i = 0; i < locations.Length; i++)
                                                                                             {
                                                                                                 if (!locations[i].CaselessContains(x + " " + y + " " + z + " in " + np.p.Level.name))
                                                                                                 {
                                                                                                     newLocations[i] = locations[i];
                                                                                                 }
                                                                                             }
                                                                                             FileIO.TryWriteAllLines(NASPlugin.GetDeathPath(np.p.name), newLocations);
                                                                                             RemoveAll(np, bEntity, bEntity.lockedBy.Length == 0);
                                                                                             bEntity.lockedBy = "";
                                                                                         }
                                                                                         np.nl.SimulateSetBlock(x, y, z);
                                                                                         return;
                                                                                     }
                                                                                     return;
                                                                                 }
                                                                                 if (nasBlock.container.type != NASContainer.NASContainerType.Gravestone)
                                                                                 {
                                                                                     np.Message("(BUG) The data inside this {0} was lost, but you can make it functional again by &cdeleting&S then &breplacing&S it.",
                                                                                                  nasBlock.container.Name.ToLower());
                                                                                     np.nl.blockEntities.Add(x + " " + y + " " + z, new());
                                                                                 }
                                                                             };
        public static void AddTool(NASPlayer np, NASBlockEntity bEntity)
        {
            if (bEntity.drop != null && bEntity.drop.items.Count >= NASContainer.ToolLimit)
            {
                np.Message("There can only be {0} tools at most in a chest.", NASContainer.ToolLimit);
                return;
            }
            if (np.inventory.items[np.inventory.selectedItemIndex] == null)
            {
                np.Message("You need to select a tool to insert it.");
                return;
            }
            if (bEntity.drop == null)
            {
                bEntity.drop = new(np.inventory.items[np.inventory.selectedItemIndex]);
            }
            else
            {
                bEntity.drop.items.Add(np.inventory.items[np.inventory.selectedItemIndex]);
            }
            np.Message("You put {0}&S in the chest.", np.inventory.items[np.inventory.selectedItemIndex].ColoredName);
            np.inventory.items[np.inventory.selectedItemIndex] = null;
            np.inventory.UpdateItemDisplay();
        }
        public static void RemoveTool(NASPlayer np, NASBlockEntity bEntity)
        {
            if (bEntity.drop == null)
            {
                np.Message("There's no tools to extract.");
                return;
            }
            NASDrop taken = new(bEntity.drop.items[bEntity.drop.items.Count - 1]);
            bool fullInv = true;
            for (int i = 0; i < 27; i++)
            {
                if (np.inventory.items[i] == null)
                {
                    fullInv = false;
                }
            }
            if (!fullInv) 
            { 
                bEntity.drop.items.RemoveAt(bEntity.drop.items.Count - 1); 
            }
            np.inventory.GetDrop(taken, true);
            if (bEntity.drop.items.Count == 0)
            {
                bEntity.drop = null;
            }
        }
        public static void AddBlocks(NASPlayer np, int x, int y, int z)
        {
            ushort clientushort = np.ConvertBlock(np.p.ClientHeldBlock);
            NASBlock nasBlock = Get(clientushort);
            NASBlockEntity bEntity = np.nl.blockEntities[x + " " + y + " " + z];
            if (nasBlock.parentID == 0)
            {
                np.Message("Select a block to store it.");
                return;
            }
            if (!np.oldBarrel)
            {
                np.isInserting = true;
                np.interactCoords = new int[] { x, y, z };
                np.Send(Packet.Motd(np.p, "-hax +thirdperson horspeed=0"));
                np.Message("&ePlease enter in chat how many items you would like to put in the barrel.");
            }
            else
            {
                int amount = np.inventory.GetAmount(nasBlock.parentID);
                if (amount < 1)
                {
                    np.Message("You don't have any {0} to store.", nasBlock.GetName(np));
                    return;
                }
                if (amount > 3)
                {
                    amount /= 2;
                }
                if (bEntity.drop == null)
                {
                    np.inventory.SetAmount(nasBlock.parentID, -amount, true, true);
                    bEntity.drop = new(nasBlock.parentID, amount);
                    return;
                }
                foreach (NASBlockStack stack in bEntity.drop.blockStacks)
                {
                    if (stack.ID == nasBlock.parentID)
                    {
                        np.inventory.SetAmount(nasBlock.parentID, -amount, true, true);
                        stack.amount += amount;
                        return;
                    }
                }
                if (bEntity.drop.blockStacks.Count >= NASContainer.BlockStackLimit)
                {
                    np.Message("It can't contain more than {0} stacks of blocks.", NASContainer.BlockStackLimit);
                    return;
                }
                np.inventory.SetAmount(nasBlock.parentID, -amount, true, true);
                bEntity.drop.blockStacks.Add(new(nasBlock.parentID, amount));
            }
        }
        public static void RemoveBlocks(NASPlayer np, NASBlockEntity bEntity)
        {
            if (bEntity.drop != null && bEntity.drop.blockStacks != null)
            {
                if (bEntity.drop.blockStacks.Count == 0)
                {
                    np.Message("&cTHERE ARE 0 BLOCK STACKS INSIDE WARNING THIS SHOULD NEVER HAPPEN IT SHOULD BE NULL INSTEAD");
                    bEntity.drop.blockStacks = null;
                }
                NASBlockStack bs = null;
                ushort clientushort = np.ConvertBlock(np.p.ClientHeldBlock);
                NASBlock nasBlock = Get(clientushort);
                foreach (NASBlockStack stack in bEntity.drop.blockStacks)
                {
                    if (stack.ID == nasBlock.parentID)
                    {
                        bs = stack;
                        break;
                    }
                }
                bs ??= bEntity.drop.blockStacks[bEntity.drop.blockStacks.Count - 1];
                int amount = bs.amount;
                if ((np.inventory.GetAmount(696) + amount) > 5 && bs.ID == 696)
                {
                    amount = 5 - np.inventory.GetAmount(696);
                }
                np.inventory.SetAmount(bs.ID, amount, true, true);
                if (amount >= bs.amount)
                {
                    bEntity.drop.blockStacks.Remove(bs);
                }
                else
                {
                    bs.amount -= amount;
                }
                if (bEntity.drop.blockStacks.Count == 0)
                {
                    bEntity.drop = null;
                }
                return;
            }
            np.Message("There's no blocks to extract.");
        }
        public static void RemoveAll(NASPlayer np, NASBlockEntity bEntity, bool message)
        {
            if (bEntity.drop != null)
            {
                bEntity.drop = np.inventory.GetDrop(bEntity.drop, message, true);
                return;
            }
            np.Message("There's nothing to extract.");
        }
        public static void CheckContents(NASPlayer np, NASBlock nb, NASBlockEntity blockEntity)
        {
            if (blockEntity.drop == null)
            {
                np.Message("There's nothing inside.");
            }
            else
            {
                if (blockEntity.drop.items != null)
                {
                    np.Message("There's {0} tool{1} inside.", blockEntity.drop.items.Count, blockEntity.drop.items.Count == 1 ? "" : "s");
                }
                if (blockEntity.drop.blockStacks != null)
                {
                    foreach (NASBlockStack bs in blockEntity.drop.blockStacks)
                    {
                        np.Message("There's &f{0} {1}&S inside.", bs.amount, blocks[bs.ID].GetName(np));
                    }
                }
            }
            if (nb.container.type == NASContainer.NASContainerType.Gravestone)
            {
                return;
            }
            np.Message("&r(&fi&r)&S This {0} is &f{1}&S", nb.container.Name.ToLower(), blockEntity.lockedBy.Length > 0 ? "locked" : "not locked");
        }
        public static NASBlockExistAction CraftingExistAction() => (np, nasBlock, exists, x, y, z) =>
                                                                            {
                                                                                if (exists)
                                                                                {
                                                                                    np.Message("You placed a &b{0}&S!", nasBlock.station.name);
                                                                                    np.Message("Click to craft.");
                                                                                    np.Message("Right click to auto-replace recipe.");
                                                                                    np.Message("Left click for one-and-done.");
                                                                                    nasBlock.station.ShowArea(np, x, y, z, new(255, 255, 255, 255));
                                                                                    return;
                                                                                }
                                                                            };
        public static NASBlockExistAction WireExistAction(int strength, int type) => (np, nasBlock, exists, x, y, z) =>
                                                                                              {
                                                                                                  if (exists)
                                                                                                  {
                                                                                                      if (np.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                                                                                                      {
                                                                                                          np.nl.blockEntities.Remove(x + " " + y + " " + z);
                                                                                                      }
                                                                                                      np.nl.blockEntities.Add(x + " " + y + " " + z, new());
                                                                                                      np.nl.blockEntities[x + " " + y + " " + z].strength = strength;
                                                                                                      np.nl.blockEntities[x + " " + y + " " + z].type = type;
                                                                                                      return;
                                                                                                  }
                                                                                                  np.nl.blockEntities.Remove(x + " " + y + " " + z);
                                                                                                  return;
                                                                                              };
        public static NASBlockExistAction WireBreakAction() => (np, nasBlock, exists, x, y, z) =>
                                                                        {
                                                                            if (!exists)
                                                                            {
                                                                                np.nl.blockEntities.Remove(x + " " + y + " " + z);
                                                                                return;
                                                                            }
                                                                        };
        public static NASBlockExistAction AutoCraftExistAction() => (np, nasBlock, exists, x, y, z) =>
                                                                             {
                                                                                 if (exists)
                                                                                 {
                                                                                     if (np.nl.blockEntities.ContainsKey(x + " " + y + " " + z))
                                                                                     {
                                                                                         np.nl.blockEntities.Remove(x + " " + y + " " + z);
                                                                                     }
                                                                                     np.nl.blockEntities.Add(x + " " + y + " " + z, new());
                                                                                     np.Message("You placed an &bAuto Crafter&S!");
                                                                                     np.Message("It can craft things without user input.");
                                                                                     np.Message("When powered it tries to craft whatever is above it.");
                                                                                     np.Message("Click it to remove all items crafted, like a gravestone.");
                                                                                     nasBlock.station.ShowArea(np, x, y, z, new(255, 255, 255, 255));
                                                                                     return;
                                                                                 }
                                                                                 np.nl.blockEntities.Remove(x + " " + y + " " + z);
                                                                                 return;
                                                                             };
        public static NASBlockInteraction CraftingInteraction() => (np, button, action, nasBlock, x, y, z) =>
                                                                            {
                                                                                if (action == MouseAction.Pressed)
                                                                                {
                                                                                    return;
                                                                                }
                                                                                NASCrafting.NASRecipe recipe = NASCrafting.GetRecipe(np.nl, x, y, z, nasBlock.station);
                                                                                if (recipe == null)
                                                                                {
                                                                                    nasBlock.station.ShowArea(np, x, y, z, new(255, 0, 0, 255), 500, 127);
                                                                                    return;
                                                                                }
                                                                                NASDrop dropClone = new(recipe.drop);
                                                                                if (np.inventory.GetDrop(dropClone, true) != null)
                                                                                {
                                                                                    return;
                                                                                }
                                                                                np.GiveExp(recipe.expGiven);
                                                                                nasBlock.station.ShowArea(np, x, y, z, new(144, 238, 144, 255), 500);
                                                                                bool clearCraftingArea = button == MouseButton.Left;
                                                                                Dictionary<ushort, int> patternCost = recipe.PatternCost;
                                                                                foreach (KeyValuePair<ushort, int> pair in patternCost)
                                                                                {
                                                                                    if (np.inventory.GetAmount(pair.Key) < pair.Value)
                                                                                    {
                                                                                        if (pair.Key == 0)
                                                                                        {
                                                                                            continue;
                                                                                        }
                                                                                        clearCraftingArea = true;
                                                                                        break;
                                                                                    }
                                                                                }
                                                                                if (clearCraftingArea)
                                                                                {
                                                                                    NASCrafting.ClearCraftingArea(np.nl, x, y, z, nasBlock.station.ori);
                                                                                }
                                                                                else
                                                                                {
                                                                                    foreach (KeyValuePair<ushort, int> pair in patternCost)
                                                                                    {
                                                                                        if (pair.Key != 0)
                                                                                        {
                                                                                            np.inventory.SetAmount(pair.Key, -pair.Value, false);
                                                                                        }
                                                                                    }
                                                                                }
                                                                            };
        public static NASBlockInteraction EatInteraction(ushort[] set, int index, float healthRestored, float chewSeconds = 2) => (np, button, action, nasBlock, x, y, z) =>
                                                                                                                                           {
                                                                                                                                               if (action == MouseAction.Pressed)
                                                                                                                                               {
                                                                                                                                                   return;
                                                                                                                                               }
                                                                                                                                               if (np.isChewing)
                                                                                                                                               {
                                                                                                                                                   return;
                                                                                                                                               }
                                                                                                                                               np.isChewing = true;
                                                                                                                                               SchedulerTask taskChew;
                                                                                                                                               NASEatInfo eatInfo = new()
                                                                                                                                               {
                                                                                                                                                   np = np,
                                                                                                                                                   healthRestored = healthRestored
                                                                                                                                               };
                                                                                                                                               taskChew = Server.MainScheduler.QueueOnce(CanEatAgainCallback, eatInfo, TimeSpan.FromSeconds(chewSeconds));
                                                                                                                                               np.Message("*munch*");
                                                                                                                                               np.p.Level.BlockDB.Cache.Add(np.p, x, y, z, 1 << 0, set[index], 0);
                                                                                                                                               if (index == set.Length - 1)
                                                                                                                                               {
                                                                                                                                                   np.nl.SetBlock(x, y, z, 0);
                                                                                                                                                   return;
                                                                                                                                               }
                                                                                                                                               np.nl.SetBlock(x, y, z, set[index + 1]);
                                                                                                                                           };
        public class NASEatInfo
        {
            public NASPlayer np;
            public float healthRestored;
        }
        public static void CanEatAgainCallback(SchedulerTask task)
        {
            NASEatInfo eatInfo = (NASEatInfo)task.State;
            NASPlayer np = eatInfo.np;
            float healthRestored = eatInfo.healthRestored,
                roundAdd = ((float)Math.Floor(np.HP * 2f) / 2f) - np.HP;
            np.ChangeHealth(roundAdd);
            float HPafterHeal = np.HP + healthRestored;
            if (HPafterHeal > NASEntity.maxHP)
            {
                healthRestored = NASEntity.maxHP - np.HP;
            }
            if (healthRestored < 0)
            {
                np.TakeDamage(-healthRestored, NASEntity.NASDamageSource.None);
            }
            else
            {
                np.ChangeHealth(healthRestored);
            }
            np.isChewing = false;
            np.Message("*gulp*");
            if (healthRestored < 0)
            {
                np.Message("Oh no! It was &mPOISON! &7It tastes super good though..");
                np.Message("&c{0} &f[{1}] HP ╝", healthRestored, np.HP);
            }
            else
            {
                np.Message("&a+{0} &f[{1}] HP ♥", healthRestored, np.HP);
            }
        }
    }
}
