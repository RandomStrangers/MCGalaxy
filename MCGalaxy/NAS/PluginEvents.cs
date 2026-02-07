using MCGalaxy.Commands;
using MCGalaxy.Commands.Moderation;
using MCGalaxy.Network;
using Newtonsoft.Json;
using System;
namespace MCGalaxy
{
    public partial class NASPlugin
    {
        static void OnPlayerCommand(Player p, string name, string message, CommandData data)
        {
            if (name.CaselessEq("setall"))
            {
                if (p.Rank < 80)
                {
                    return;
                }
                foreach (Command _cmd in Command.allCmds)
                {
                    Group group = Group.Find(80);
                    group ??= Group.Find(p.Rank);
                    new CmdCmdSet().Use(p, _cmd.Name + " " + group.Name);
                }
                p.cancelcommand = true;
                return;
            }
            if (name.CaselessEq("reload"))
            {
                if (p.CanUse(name))
                {
                    NASPlayer npl = NASPlayer.GetPlayer(p);
                    if (message.CaselessEq("all") && HasExtraPerm(npl, name, 1))
                    {
                        Player[] players = PlayerInfo.Online.Items;
                        foreach (Player player in players)
                        {
                            NASPlayer n = NASPlayer.GetPlayer(player);
                            n.SendingMap = true;
                        }
                    }
                    else if (message.Length > 0 && HasExtraPerm(npl, name, 1))
                    {
                        Player[] players = PlayerInfo.Online.Items;
                        foreach (Player player in players)
                        {
                            Level lvl = Matcher.FindLevels(npl.p, message);
                            if (player.Level.name.CaselessEq(lvl.name))
                            {
                                NASPlayer n = NASPlayer.GetPlayer(player);
                                n.SendingMap = true;
                            }
                        }
                    }
                    npl.SendingMap = true;
                }
            }
            if (name.CaselessEq("gentree"))
            {
                p.cancelcommand = true;
                if (p.Rank < 80)
                {
                    return;
                }
                string[] messageString = message.SplitSpaces();
                NASTree.GenSwampTree(NASLevel.Get(p.Level.name), new Random(), int.Parse(messageString[0]), int.Parse(messageString[1]), int.Parse(messageString[2]));
                return;
            }
            if (name.CaselessEq("time") && message.SplitSpaces()[0].CaselessEq("current"))
            {
                p.cancelcommand = true;
                int mins = NASTimeCycle.cyc.minutes;
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
                int day = NASTimeCycle.cyc.day;
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
            if (name.CaselessEq("time") && message.SplitSpaces()[0].CaselessEq("set"))
            {
                p.cancelcommand = true;
                if (message.SplitSpaces().Length > 1)
                {
                    int time = 0;
                    string setTime = message.SplitSpaces()[1];
                    if (setTime.CaselessEq("sunrise"))
                    {
                        time = 8 * NASTimeCycle.hourMinutes;
                    }
                    else if (setTime.CaselessEq("day"))
                    {
                        time = 7 * NASTimeCycle.hourMinutes;
                    }
                    else if (setTime.CaselessEq("sunset"))
                    {
                        time = 19 * NASTimeCycle.hourMinutes;
                    }
                    else if (setTime.CaselessEq("night"))
                    {
                        time = 20 * NASTimeCycle.hourMinutes;
                    }
                    else if (setTime.CaselessEq("midnight"))
                    {
                        time = 0;
                    }
                    else if (!CommandParser.GetInt(p, setTime, "Amount", ref time, 0))
                    {
                        return;
                    }
                    NASTimeCycle.cycleCurrentTime = time % NASTimeCycle.cycleMaxTime;
                }
            }
            if (name.CaselessEq("goto") && p.Rank < 80 && data.Context != 2)
            {
                p.Message("You cannot use /goto manually. It is triggered automatically when you go to map borders.");
                p.cancelcommand = true;
                return;
            }
            if (name.CaselessEq("color"))
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
            if (name.CaselessEq("sign"))
            {
                p.cancelcommand = true;
                if (string.IsNullOrEmpty(message))
                {
                    p.Message("You need to provide text to put in the sign.");
                    return;
                }
                else
                {
                    FileIO.TryWriteAllText(NASBlock.GetTextPath(p), message);
                    return;
                }
            }
            if (name.CaselessEq("smite"))
            {
                p.cancelcommand = true;
                if (p.Rank < 80)
                {
                    return;
                }
                NASPlayer nw = NASPlayer.GetPlayer(PlayerInfo.FindMatches(p, message));
                nw.lastAttackedPlayer = p;
                nw.TakeDamage(50, 3, "@p &fwas smote by " + p.ColoredName);
                return;
            }
            if (!name.CaselessEq("nas"))
            {
                return;
            }
            p.cancelcommand = true;
            NASPlayer np = NASPlayer.GetPlayer(p);
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
        static void OnPlayerMessage(Player p, string message)
        {
            NASPlayer np = NASPlayer.GetPlayer(p);
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
                NASBlock nasBlock = NASBlock.Get(clientushort);
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
                BlockEntity bEntity = np.nl.blockEntities[x + " " + y + " " + z];
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
                if (bEntity.drop.blockStacks.Count >= Container.BlockStackLimit)
                {
                    np.Message("It can't contain more than {0} stacks of blocks.", Container.BlockStackLimit);
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
        static void OnLevelJoined(Player p, Level prevLevel, Level level, ref bool announce)
        {
            level.Config.SkyColor = NASTimeCycle.globalSkyColor;
            level.Config.CloudColor = NASTimeCycle.globalCloudColor;
            level.Config.LightColor = NASTimeCycle.globalSunColor;
        }
        public void HandleSentMap(Player p, Level _, Level __)
        {
            NASPlayer np = NASPlayer.GetPlayer(p);
            if (np.SendingMap)
            {
                np.inventory.p = p;
                np.inventory.Setup(p);
                np.SendingMap = false;
            }
        }
        public static bool Load(Player p, string file, out NASPlayer np)
        {
            try
            {
                np = JsonConvert.DeserializeObject<NASPlayer>(FileIO.TryReadAllText(file));
                np.SetPlayer(p);
                p.Extras[NASPlayer.PlayerKey] = np;
                Log("Loaded save file {0}!", file);
                return true;
            }
            catch
            {
                np = null;
                return false;
            }
        }
        static void OnPlayerConnect(Player p)
        {
            string path = GetSavePath(p),
                pathText = GetTextPath(p);
            if (!Load(p, path, out NASPlayer np) && !Load(p, pathText, out np))
            {
                np = new(p);
                Orientation rot = new(Server.mainLevel.rotx, Server.mainLevel.roty);
                np.SetLocation(Server.mainLevel.name, Server.mainLevel.SpawnPos, rot);
                p.Extras[NASPlayer.PlayerKey] = np;
                Log("Created new save file for {0}!", p.name);
            }
            np.DisplayHealth();
            np.inventory.ClearHotbar();
            np.inventory.DisplayHeldBlock(NASBlock.Default);
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
        static void OnShutdown(bool restarting, string reason) => SaveAll(Player.Console);
        static void OnPlayerDisconnect(Player p, string reason)
        {
            NASPlayer np = NASPlayer.GetPlayer(p);
            if (np != null)
            {
                np.PlayerSavingScheduler ??= new("SavingScheduler" + p.name);
                np.PlayerSavingScheduler.Cancel(np.PlayerSaveTask);
                np.Save();
            }
        }
        static void OnPlayerClick(Player p, int button,
            int action, ushort yaw, ushort pitch, byte entity,
            ushort x, ushort y, ushort z, int face)
        {
            if (p.Level.Config.Deletable && p.Level.Config.Buildable)
            {
                return;
            }
            NASPlayer.ClickOnPlayer(p, entity, button, action);
            if (button == 0)
            {
                NASBlockChange.HandleLeftClick(p, button, action, yaw, pitch, entity, x, y, z, face);
            }
            NASPlayer np = NASPlayer.GetPlayer(p);
            if (button == 2 && action == 0)
            {
            }
            if (button == 1 && action == 0)
            {
            }
            if (!np.justBrokeOrPlaced)
            {
                np.HandleInteraction(button, action, x, y, z, entity, face);
            }
            if (action == 1)
            {
                np.justBrokeOrPlaced = false;
            }
        }
        static void OnPlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel)
        {
            NASPlayer np = NASPlayer.GetPlayer(p);
            np.DisplayHealth();
            np.DoMovement(next, yaw, pitch);
        }
    }
}
