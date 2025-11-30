#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Commands;
using MCGalaxy.Commands.Moderation;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;
using Newtonsoft.Json;
using System;
namespace NotAwesomeSurvival
{
    public partial class Nas
    {
        public static void OnPlayerCommand(Player p, string cmd, string message, CommandData data)
        {
            if (cmd.CaselessEq("setall"))
            {
                if (p.Rank < LevelPermission.Operator)
                {
                    return;
                }
                foreach (Command _cmd in Command.allCmds)
                {
                    new CmdCmdSet().Use(p, _cmd.name + " Operator");
                }
                p.cancelcommand = true;
                return;
            }
            if (cmd.CaselessEq("reload"))
            {
                if (p.CanUse(cmd))
                {
                    NasPlayer npl = NasPlayer.GetNasPlayer(p);
                    if (message.CaselessEq("all") && HasExtraPerm(npl, cmd, 1))
                    {
                        Player[] players = PlayerInfo.Online.Items;
                        foreach (Player player in players)
                        {
                            NasPlayer n = NasPlayer.GetNasPlayer(player);
                            n.SendingMap = true;
                        }
                    }
                    else if (message.Length > 0 && HasExtraPerm(npl, cmd, 1))
                    {
                        Player[] players = PlayerInfo.Online.Items;
                        foreach (Player player in players)
                        {
                            Level lvl = Matcher.FindLevels(npl.p, message);
                            if (player.level.name.CaselessEq(lvl.name))
                            {
                                NasPlayer n = NasPlayer.GetNasPlayer(player);
                                n.SendingMap = true;
                            }
                        }
                    }
                    npl.SendingMap = true;
                }
            }
            if (cmd.CaselessEq("gentree"))
            {
                p.cancelcommand = true;
                if (p.Rank < LevelPermission.Operator)
                {
                    return;
                }
                string[] messageString = message.SplitSpaces();
                NasTree.GenSwampTree(NasLevel.Get(p.level.name), new Random(), int.Parse(messageString[0]), int.Parse(messageString[1]), int.Parse(messageString[2]));
                return;
            }
            if (cmd.CaselessEq("time") && message.SplitSpaces()[0].CaselessEq("current"))
            {
                p.cancelcommand = true;
                int mins = NasTimeCycle.cyc.minutes;
                string time = "12am";
                if (mins < 1200 && mins >= 600)
                {
                    time = "1am";
                }
                else if (mins < 1800)
                {
                    time = "2am";
                }
                else if (mins < 2400)
                {
                    time = "3am";
                }
                else if (mins < 3000)
                {
                    time = "4am";
                }
                else if (mins < 3600)
                {
                    time = "5am";
                }
                else if (mins < 4200)
                {
                    time = "6am";
                }
                else if (mins < 4800)
                {
                    time = "7am";
                }
                else if (mins < 5400)
                {
                    time = "8am";
                }
                else if (mins < 6000)
                {
                    time = "9am";
                }
                else if (mins < 6600)
                {
                    time = "10am";
                }
                else if (mins < 7200)
                {
                    time = "11am";
                }
                else if (mins < 7800)
                {
                    time = "12pm";
                }
                else if (mins < 8400)
                {
                    time = "1pm";
                }
                else if (mins < 9000)
                {
                    time = "2pm";
                }
                else if (mins < 9600)
                {
                    time = "3pm";
                }
                else if (mins < 10200)
                {
                    time = "4pm";
                }
                else if (mins < 10800)
                {
                    time = "5pm";
                }
                else if (mins < 11400)
                {
                    time = "6pm";
                }
                else if (mins < 12000)
                {
                    time = "7pm";
                }
                else if (mins < 12600)
                {
                    time = "8pm";
                }
                else if (mins < 13200)
                {
                    time = "9pm";
                }
                else if (mins < 13800)
                {
                    time = "10pm";
                }
                else if (mins < 14400)
                {
                    time = "11pm";
                }
                p.Message("The time is currently {0}.", time);
                int day = NasTimeCycle.cyc.day;
                if (day == 1)
                {
                    p.Message("1 day has passed since the start of the world.");
                }
                else
                {
                    p.Message("{0} days have passed since the start of the world.", day);
                }
                return;
            }
            if (cmd.CaselessEq("time") && message.SplitSpaces()[0].CaselessEq("set"))
            {
                p.cancelcommand = true;
                if (message.SplitSpaces().Length > 1)
                {
                    int time = 0;
                    string setTime = message.SplitSpaces()[1];
                    if (setTime.CaselessEq("sunrise"))
                    {
                        time = 8 * NasTimeCycle.hourMinutes;
                    }
                    else if (setTime.CaselessEq("day"))
                    {
                        time = 7 * NasTimeCycle.hourMinutes;
                    }
                    else if (setTime.CaselessEq("sunset"))
                    {
                        time = 19 * NasTimeCycle.hourMinutes;
                    }
                    else if (setTime.CaselessEq("night"))
                    {
                        time = 20 * NasTimeCycle.hourMinutes;
                    }
                    else if (setTime.CaselessEq("midnight"))
                    {
                        time = 0;
                    }
                    else if (!CommandParser.GetInt(p, setTime, "Amount", ref time, 0))
                    {
                        return;
                    }
                    NasTimeCycle.cycleCurrentTime = time % NasTimeCycle.cycleMaxTime;
                }
            }
            if (cmd.CaselessEq("goto") && p.Rank < LevelPermission.Operator && data.Context != CommandContext.SendCmd)
            {
                p.Message("You cannot use /goto manually. It is triggered automatically when you go to map borders.");
                p.cancelcommand = true;
                return;
            }
            if (cmd.CaselessEq("color"))
            {
                if (message.Length == 0)
                {
                    return;
                }
                string[] args = message.Split(' ');
                string color = args[args.Length - 1];
                if (Matcher.FindColor(p, color).CaselessEq("&h"))
                {
                    p.Message("That color isn't allowed in names.");
                    p.cancelcommand = true;
                    return;
                }
                return;
            }
            if (cmd.CaselessEq("sign"))
            {
                p.cancelcommand = true;
                if (string.IsNullOrEmpty(message))
                {
                    p.Message("You need to provide text to put in the sign.");
                    return;
                }
                else
                {
                    FileUtils.TryWriteAllText(NasBlock.GetTextPath(p), message);
                    return;
                }
            }
            if (cmd.CaselessEq("smite"))
            {
                p.cancelcommand = true;
                if (p.Rank < LevelPermission.Operator)
                {
                    return;
                }
                NasPlayer nw = NasPlayer.GetNasPlayer(PlayerInfo.FindMatches(p, message));
                nw.lastAttackedPlayer = p;
                nw.TakeDamage(50, NasEntity.DamageSource.Entity, "@p &fwas smote by " + p.ColoredName);
                return;
            }
            if (!cmd.CaselessEq("nas"))
            {
                return;
            }
            p.cancelcommand = true;
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            string[] words = message.Split(' ');
            if (words.Length > 1 && words[0].CaselessEq("hotbar"))
            {
                string hotbarFunc = words[1];
                if (words.Length > 2)
                {
                    string func2 = words[2];
                    if (hotbarFunc.CaselessEq("bagopen"))
                    {
                        if (!np.inventory.bagOpen)
                        {
                            return;
                        }
                        if (func2.CaselessEq("left"))
                        {
                            np.inventory.MoveItemBarSelection(-1);
                            return;
                        }
                        if (func2.CaselessEq("right"))
                        {
                            np.inventory.MoveItemBarSelection(1);
                            return;
                        }
                        if (func2.CaselessEq("up"))
                        {
                            np.inventory.MoveItemBarSelection(-9);
                            return;
                        }
                        if (func2.CaselessEq("down"))
                        {
                            np.inventory.MoveItemBarSelection(9);
                            return;
                        }
                    }
                    return;
                }
                if (hotbarFunc.CaselessEq("left"))
                {
                    np.inventory.MoveItemBarSelection(-1);
                    return;
                }
                if (hotbarFunc.CaselessEq("right"))
                {
                    np.inventory.MoveItemBarSelection(1);
                    return;
                }
                if (hotbarFunc.CaselessEq("up"))
                {
                    np.inventory.MoveItemBarSelection(-9);
                    return;
                }
                if (hotbarFunc.CaselessEq("down"))
                {
                    np.inventory.MoveItemBarSelection(9);
                    return;
                }
                if (hotbarFunc.CaselessEq("move"))
                {
                    np.inventory.DoItemMove();
                    return;
                }
                if (hotbarFunc.CaselessEq("inv"))
                {
                    np.inventory.ToggleBagOpen();
                    return;
                }
                if (hotbarFunc.CaselessEq("delete"))
                {
                    np.inventory.DeleteItem();
                    return;
                }
                if (hotbarFunc.CaselessEq("confirmdelete"))
                {
                    np.inventory.DeleteItem(true);
                    return;
                }
                if (hotbarFunc.CaselessEq("toolinfo"))
                {
                    np.inventory.ToolInfo();
                    return;
                }
                return;
            }
        }
        public static void OnPlayerMessage(Player p, string message)
        {
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            if (np.isInserting)
            {
                int items = 0;
                if (!CommandParser.GetInt(p, message, "Amount", ref items, 0))
                {
                    return;
                }
                if (items == 0)
                {
                    p.cancelchat = true;
                    p.SendMapMotd();
                    np.isInserting = false;
                    return;
                }
                ushort clientushort = np.ConvertBlock(p.ClientHeldBlock);
                NasBlock nasBlock = NasBlock.Get(clientushort);
                int amount = np.inventory.GetAmount(nasBlock.parentID);
                if (items > amount)
                {
                    items = amount;
                }
                if (amount < 1)
                {
                    np.Message("You don't have any {0} to store.", nasBlock.GetName(np));
                    return;
                }
                int x = np.interactCoords[0],
                    y = np.interactCoords[1],
                    z = np.interactCoords[2];
                NasBlock.Entity bEntity = np.nl.blockEntities[x + " " + y + " " + z];
                if (bEntity.drop == null)
                {
                    np.inventory.SetAmount(nasBlock.parentID, -items, true, true);
                    bEntity.drop = new(nasBlock.parentID, items);
                    p.cancelchat = true;
                    p.SendMapMotd();
                    np.isInserting = false;
                    return;
                }
                foreach (BlockStack stack in bEntity.drop.blockStacks)
                {
                    if (stack.ID == nasBlock.parentID)
                    {
                        np.inventory.SetAmount(nasBlock.parentID, -items, true, true);
                        stack.amount += items;
                        p.cancelchat = true;
                        p.SendMapMotd();
                        np.isInserting = false;
                        return;
                    }
                }
                if (bEntity.drop.blockStacks.Count >= NasBlock.Container.BlockStackLimit)
                {
                    np.Message("It can't contain more than {0} stacks of blocks.", NasBlock.Container.BlockStackLimit);
                    p.cancelchat = true;
                    p.SendMapMotd();
                    np.isInserting = false;
                    return;
                }
                np.inventory.SetAmount(nasBlock.parentID, -items, true, true);
                bEntity.drop.blockStacks.Add(new(nasBlock.parentID, items));
                p.cancelchat = true;
                p.SendMapMotd();
                np.isInserting = false;
            }
        }
        public static void OnLevelJoined(Player p, Level prevLevel, Level level, ref bool announce)
        {
            level.Config.SkyColor = NasTimeCycle.globalSkyColor;
            level.Config.CloudColor = NasTimeCycle.globalCloudColor;
            level.Config.LightColor = NasTimeCycle.globalSunColor;
        }
        public void HandleSentMap(Player p, Level prevLevel, Level level)
        {
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            if (!np.SendingMap)
            {
                return;
            }
            np.inventory.Setup(p);
            np.SendingMap = false;
        }
        public static bool Load(Player p, string file, out NasPlayer np)
        {
            try
            {
                string jsonString = FileUtils.TryReadAllText(file);
                np = JsonConvert.DeserializeObject<NasPlayer>(jsonString);
                np.SetPlayer(p);
                p.Extras[PlayerKey] = np;
                Log("Loaded save file {0}!", file);
                return true;
            }
            catch
            {
                np = null;
                return false;
            }
        }
        public static void OnPlayerConnect(Player p)
        {
            string path = GetSavePath(p),
                pathText = GetTextPath(p);
            if (!Load(p, path, out NasPlayer np) && !Load(p, pathText, out np))
            {
                np = new NasPlayer(p);
                Orientation rot = new(Server.mainLevel.rotx, Server.mainLevel.roty);
                NasEntity.SetLocation(np, Server.mainLevel.name, Server.mainLevel.SpawnPos, rot);
                p.Extras[PlayerKey] = np;
                Log("Created new save file for {0}!", p.name);
            }
            np.DisplayHealth();
            np.inventory.ClearHotbar();
            np.inventory.DisplayHeldBlock(NasBlock.Default);
            if (!np.bigUpdate || np.resetCount != 1)
            {
                np.UpdateValues();
            }
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar left◙", 16, 0, true));
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar right◙", 18, 0, true));
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar up◙", 200, 0, true));
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar down◙", 208, 0, true));
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar left◙", 203, 0, true));
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar right◙", 205, 0, true));
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar move◙", 50, 0, true));
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar inv◙", 19, 0, true));
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar delete◙", 45, 0, true));
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar confirmdelete◙", 25, 0, true));
            np.Send(Packet.TextHotKey("NasHotkey", "/nas hotbar toolinfo◙", 23, 0, true));
            np.PlayerSavingScheduler ??= new("SavingScheduler" + p.name);
            np.PlayerSaveTask = np.PlayerSavingScheduler.QueueRepeat(np.SaveStatsTask, null, TimeSpan.FromSeconds(5));
        }
        public static void OnShutdown(bool restarting, string reason)
        {
            SaveAll(Player.Console);
        }
        public static void OnPlayerDisconnect(Player p, string reason)
        {
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            if (np != null)
            {
                np.PlayerSavingScheduler ??= new("SavingScheduler" + p.name);
                np.PlayerSavingScheduler.Cancel(np.PlayerSaveTask);
                np.Save();
            }
        }
        public static void OnPlayerClick(Player p, MouseButton button,
            MouseAction action, ushort yaw, ushort pitch, byte entity,
            ushort x, ushort y, ushort z, TargetBlockFace face)
        {
            if (p.level.Config.Deletable && p.level.Config.Buildable)
            {
                return;
            }
            NasPlayer.ClickOnPlayer(p, entity, button, action);
            if (button == MouseButton.Left)
            {
                NasBlockChange.HandleLeftClick(p, button, action, yaw, pitch, entity, x, y, z, face);
            }
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            if (button == MouseButton.Middle && action == MouseAction.Pressed)
            {
            }
            if (button == MouseButton.Right && action == MouseAction.Pressed)
            {
            }
            if (!np.justBrokeOrPlaced)
            {
                np.HandleInteraction(button, action, x, y, z, entity, face);
            }
            if (action == MouseAction.Released)
            {
                np.justBrokeOrPlaced = false;
            }
        }
        public static void OnBlockChanging(Player p, ushort x, ushort y, ushort z, ushort block, bool placing, ref bool cancel)
        {
            NasBlockChange.PlaceBlock(p, x, y, z, block, placing, ref cancel);
        }
        public static void OnBlockChanged(Player p, ushort x, ushort y, ushort z, ChangeResult result)
        {
            NasBlockChange.OnBlockChanged(p, x, y, z, result);
        }
        public static void OnPlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel)
        {
            NasPlayer np = NasPlayer.GetNasPlayer(p);
            np.DisplayHealth();
            np.DoMovement(next, yaw, pitch);
        }
    }
}
#endif