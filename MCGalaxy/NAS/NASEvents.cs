using MCGalaxy.Commands;
using MCGalaxy.Commands.Moderation;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Network;
using Newtonsoft.Json;
using System;
namespace MCGalaxy
{
    public partial class NAS
    {
        public static void OnPlayerCommand(Player p, string name, string message, CommandData data)
        {
            if (name.CaselessEq("setall"))
            {
                if (p.Rank < LevelPermission.Operator)
                    return;
                foreach (Command _cmd in Command.allCmds)
                {
                    Group group = Group.Find(LevelPermission.Operator);
                    group ??= p.group;
                    new CmdCmdSet().Use(p, _cmd.Name + " " + group.Name);
                }
                p.cancelcommand = true;
                return;
            }
            if (name.CaselessEq("reload") && p.CanUse(name))
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
            if (name.CaselessEq("gentree"))
            {
                p.cancelcommand = true;
                if (p.Rank < LevelPermission.Operator)
                    return;
                string[] messageString = message.SplitSpaces();
                NASTree.GenSwampTree(NASLevel.Get(p.Level.name), new Random(), int.Parse(messageString[0]), int.Parse(messageString[1]), int.Parse(messageString[2]));
                return;
            }
            if (name.CaselessEq("time") && message.SplitSpaces()[0].CaselessEq("current"))
            {
                p.cancelcommand = true;
                int mins = NASTimeCycle.cyc.minutes;
                string time = "12am";
                switch (mins)
                {
                    case < 1200 and >= 600:
                        time = "1am";
                        break;
                    case < 1800:
                        time = "2am";
                        break;
                    case < 2400:
                        time = "3am";
                        break;
                    case < 3000:
                        time = "4am";
                        break;
                    case < 3600:
                        time = "5am";
                        break;
                    case < 4200:
                        time = "6am";
                        break;
                    case < 4800:
                        time = "7am";
                        break;
                    case < 5400:
                        time = "8am";
                        break;
                    case < 6000:
                        time = "9am";
                        break;
                    case < 6600:
                        time = "10am";
                        break;
                    case < 7200:
                        time = "11am";
                        break;
                    case < 7800:
                        time = "12pm";
                        break;
                    case < 8400:
                        time = "1pm";
                        break;
                    case < 9000:
                        time = "2pm";
                        break;
                    case < 9600:
                        time = "3pm";
                        break;
                    case < 10200:
                        time = "4pm";
                        break;
                    case < 10800:
                        time = "5pm";
                        break;
                    case < 11400:
                        time = "6pm";
                        break;
                    case < 12000:
                        time = "7pm";
                        break;
                    case < 12600:
                        time = "8pm";
                        break;
                    case < 13200:
                        time = "9pm";
                        break;
                    case < 13800:
                        time = "10pm";
                        break;
                    case < 14400:
                        time = "11pm";
                        break;
                }
                p.Message("The time is currently {0}.", time);
                int day = NASTimeCycle.cyc.day;
                switch (day)
                {
                    case 1:
                        p.Message("1 day has passed since the start of the world.");
                        break;
                    default:
                        p.Message("{0} days have passed since the start of the world.", day);
                        break;
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
                        time = 8 * NASTimeCycle.hourMinutes;
                    else if (setTime.CaselessEq("day"))
                        time = 7 * NASTimeCycle.hourMinutes;
                    else if (setTime.CaselessEq("sunset"))
                        time = 19 * NASTimeCycle.hourMinutes;
                    else if (setTime.CaselessEq("night"))
                        time = 20 * NASTimeCycle.hourMinutes;
                    else if (setTime.CaselessEq("midnight"))
                        time = 0;
                    else if (!CommandParser.GetInt(p, setTime, "Amount", ref time, 0))
                        return;
                    NASTimeCycle.cycleCurrentTime = time % NASTimeCycle.cycleMaxTime;
                }
            }
            if (name.CaselessEq("goto") && p.Rank < LevelPermission.Operator && data.Context != CommandContext.SendCmd)
            {
                p.Message("You cannot use /goto manually. It is triggered automatically when you go to map borders.");
                p.cancelcommand = true;
                return;
            }
            if (name.CaselessEq("color"))
            {
                if (message.Length == 0)
                    return;
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
                if (p.Rank < LevelPermission.Operator)
                    return;
                NASPlayer nw = NASPlayer.GetPlayer(PlayerInfo.FindMatches(p, message));
                nw.lastAttackedPlayer = p;
                nw.TakeDamage(50, NASDamageSource.Entity, "@p &fwas smote by " + p.ColoredName);
                return;
            }
            if (!name.CaselessEq("NAS"))
                return;
            p.cancelcommand = true;
            NASPlayer np = NASPlayer.GetPlayer(p);
            if (message.CaselessEq("save"))
            {
                try
                {
                    np.Save();
                }
                catch (Exception ex)
                {
                    np.Message("There was an error saving your data!");
                    Logger.Log(LogType.Warning, "Error {0} while saving " + p.truename + " NAS data!", ex);
                }
                return;
            }
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
                            return;
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
            NASPlayer np = NASPlayer.GetPlayer(p);
            if (np.isInserting)
            {
                int items = 0;
                if (!CommandParser.GetInt(p, message, "Amount", ref items, 0))
                    return;
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
                    items = amount;
                if (amount < 1)
                {
                    np.Message("You don't have any {0} to store.", nasBlock.GetName(np));
                    return;
                }
                int x = np.interactCoords[0],
                    y = np.interactCoords[1],
                    z = np.interactCoords[2];
                NASBlockEntity bEntity = np.nl.blockEntities[x + " " + y + " " + z];
                if (bEntity.drop == null)
                {
                    np.inventory.SetAmount(nasBlock.parentID, -items, true, true);
                    bEntity.drop = new(nasBlock.parentID, items);
                    p.cancelchat = true;
                    p.SendMapMotd();
                    np.isInserting = false;
                    return;
                }
                foreach (NASBlockStack stack in bEntity.drop.blockStacks)
                    if (stack.ID == nasBlock.parentID)
                    {
                        np.inventory.SetAmount(nasBlock.parentID, -items, true, true);
                        stack.amount += items;
                        p.cancelchat = true;
                        p.SendMapMotd();
                        np.isInserting = false;
                        return;
                    }
                if (bEntity.drop.blockStacks.Count >= NASContainer.BlockStackLimit)
                {
                    np.Message("It can't contain more than {0} stacks of blocks.", NASContainer.BlockStackLimit);
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
            level.Config.SkyColor = NASTimeCycle.globalSkyColor;
            level.Config.CloudColor = NASTimeCycle.globalCloudColor;
            level.Config.LightColor = NASTimeCycle.globalSunColor;
        }
        public static void HandleSentMap(Player p, Level prevLevel, Level level)
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
                string jsonString = FileIO.TryReadAllText(file);
                np = JsonConvert.DeserializeObject<NASPlayer>(jsonString);
                np.SetPlayer(p);
                p.Extras[PlayerKey] = np;
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
            if (!Load(p, path, out NASPlayer np) && !Load(p, pathText, out np))
            {
                np = new(p);
                np.SetLocation(Server.mainLevel.name, Server.mainLevel.SpawnPos, new(Server.mainLevel.rotx, Server.mainLevel.roty));
                p.Extras[PlayerKey] = np;
            }
            np.DisplayHealth();
            np.inventory.ClearHotbar();
            np.inventory.DisplayHeldBlock(NASBlock.Default);
            if (!np.bigUpdate || np.resetCount != 1)
                np.UpdateValues();
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar left◙", 16, 0, true));
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar right◙", 18, 0, true));
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar up◙", 200, 0, true));
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar down◙", 208, 0, true));
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar left◙", 203, 0, true));
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar right◙", 205, 0, true));
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar move◙", 50, 0, true));
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar inv◙", 19, 0, true));
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar delete◙", 45, 0, true));
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar confirmdelete◙", 25, 0, true));
            np.Send(Packet.TextHotKey("NASHotkey", "/NAS hotbar toolinfo◙", 23, 0, true));
            np.PlayerSavingScheduler ??= new("SavingScheduler" + p.name);
            np.PlayerSaveTask = np.PlayerSavingScheduler.QueueRepeat(np.SaveStatsTask, null, TimeSpan.FromSeconds(5));
        }
        public static void OnShutdown(bool restarting, string reason) => SaveAll(Player.Console);
        public static void OnPlayerDisconnect(Player p, string reason)
        {
            NASPlayer np = NASPlayer.GetPlayer(p);
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
            if (p.Level.Config.Deletable && p.Level.Config.Buildable)
                return;
            NASPlayer.ClickOnPlayer(p, entity, button, action);
            if (button == MouseButton.Left)
                NASBlockChange.HandleLeftClick(p, button, action, yaw, pitch, entity, x, y, z, face);
            NASPlayer np = NASPlayer.GetPlayer(p);
            if (button == MouseButton.Middle && action == MouseAction.Pressed)
            {
            }
            if (button == MouseButton.Right && action == MouseAction.Pressed)
            {
            }
            if (!np.justBrokeOrPlaced)
                np.HandleInteraction(button, action, x, y, z, entity, face);
            if (action == MouseAction.Released)
                np.justBrokeOrPlaced = false;
        }
        public static void OnBlockChanging(Player p, ushort x, ushort y, ushort z, ushort block, bool placing, ref bool cancel) => NASBlockChange.PlaceBlock(p, x, y, z, block, placing, ref cancel);
        public static void OnBlockChanged(Player p, ushort x, ushort y, ushort z, ChangeResult result) => NASBlockChange.OnBlockChanged(p, x, y, z, result);
        public static void OnPlayerMove(Player p, Position next, byte yaw, byte pitch, ref bool cancel)
        {
            NASPlayer np = NASPlayer.GetPlayer(p);
            np.DisplayHealth();
            np.DoMovement(next, yaw, pitch);
        }
    }
}
