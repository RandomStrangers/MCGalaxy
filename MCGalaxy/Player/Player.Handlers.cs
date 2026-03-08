/*
Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
Dual-licensed under the Educational Community License, Version 2.0 and
the GNU General Public License, Version 3 (the "Licenses"); you may
not use this file except in compliance with the Licenses. You may
obtain a copy of the Licenses at
https://opensource.org/license/ecl-2-0/
https://www.gnu.org/licenses/gpl-3.0.html
Unless required by applicable law or agreed to in writing,
software distributed under the Licenses are distributed on an "AS IS"
BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
or implied. See the Licenses for the specific language governing
permissions and limitations under the Licenses.
 */
using MCGalaxy.Authentication;
using MCGalaxy.Blocks;
using MCGalaxy.Blocks.Physics;
using MCGalaxy.Commands;
using MCGalaxy.Commands.Chatting;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.SQL;
using MCGalaxy.Util;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
namespace MCGalaxy
{
    public partial class Player : IDisposable
    {
        internal bool HasBlockChange() => Blockchange != null;
        internal bool DoBlockchangeCallback(ushort x, ushort y, ushort z, ushort block)
        {
            lock (blockchangeLock)
            {
                lastClick.X = x;
                lastClick.Y = y;
                lastClick.Z = z;
                if (Blockchange == null) return false;
                Blockchange(this, x, y, z, block);
                return true;
            }
        }
        public void HandleManualChange(ushort x, ushort y, ushort z, bool placing,
                                       ushort block, bool checkPlaceDist)
        {
            ushort old = Level.GetBlock(x, y, z);
            if (old == 0xff) return;
            if (frozen || possessed) 
            {
                RevertBlock(x, y, z); 
                return;
            }
            if (!agreed)
            {
                Message("You must read /rules then agree to them with /agree!");
                RevertBlock(x, y, z); 
                return;
            }
            if (Level.IsMuseum && Blockchange == null) return;
            bool deletingBlock = !painting && !placing;
            if (Unverified)
            {
                ExtraAuthenticator.RequiresVerification(this, "modify blocks");
                RevertBlock(x, y, z); 
                return;
            }
            if (ClickToMark && DoBlockchangeCallback(x, y, z, block)) return;
            bool cancel = false;
            OnBlockChangingEvent.Call(this, x, y, z, block, placing, ref cancel);
            if (cancel) return;
            if (old >= 200 && old <= 217)
            {
                Message("Block is active, you cannot disturb it.");
                RevertBlock(x, y, z);
                return;
            }
            if (!deletingBlock && Level.FoundInfo(x, y, z).HasWait || Rank == LevelPermission.Banned)
                return;
            if (checkPlaceDist)
            {
                int dx = Pos.BlockX - x, dy = Pos.BlockY - y, dz = Pos.BlockZ - z,
                    diff = (int)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                if (diff > ReachDistance + 4)
                {
                    Logger.Log(LogType.Warning, "{0} attempted to build with a {1} distance offset", name, diff);
                    Message("You can't build that far away.");
                    RevertBlock(x, y, z);
                    return;
                }
            }
            if (!CheckManualChange(old, deletingBlock))
            {
                RevertBlock(x, y, z); 
                return;
            }
            ushort raw = placing ? block : (ushort)0;
            block = BlockBindings[block];
            if (ModeBlock != 0xff) block = ModeBlock;
            ushort newB = deletingBlock ? (ushort)0 : block;
            ChangeResult result;
            if (old == newB)
                result = ChangeResult.Unchanged;
            else if (deletingBlock)
                result = DeleteBlock(old, x, y, z);
            else if (!CommandParser.IsBlockAllowed(this, "place", block))
                result = ChangeResult.Unchanged;
            else
                result = PlaceBlock(old, x, y, z, block);
            if (result != ChangeResult.Modified && !Block.VisuallyEquals(raw, old))
                RevertBlock(x, y, z);
            OnBlockChangedEvent.Call(this, x, y, z, result);
        }
        internal bool CheckManualChange(ushort old, bool deleteMode)
        {
            if (!group.CanDelete[old] && !Block.AllowBreak(old))
            {
                BlockPerms.GetDelete(old).MessageCannotUse(this, deleteMode ? "delete" : "replace");
                return false;
            }
            return true;
        }
        ChangeResult DeleteBlock(ushort old, ushort x, ushort y, ushort z)
        {
            if (deleteMode) return ChangeBlock(x, y, z, 0);
            HandleDelete handler = Level.DeleteHandlers[old];
            return handler != null ? handler(this, old, x, y, z) : ChangeBlock(x, y, z, 0);
        }
        ChangeResult PlaceBlock(ushort _, ushort x, ushort y, ushort z, ushort block)
        {
            HandlePlace handler = Level.PlaceHandlers[block];
            return handler != null ? handler(this, block, x, y, z) : ChangeBlock(x, y, z, block);
        }
        /// <summary> Updates the block at the given position, mainly intended for manual changes by the player. </summary>
        /// <remarks> Adds to the BlockDB. Also turns block below to grass/dirt depending on light. </remarks>
        /// <returns> Return code from DoBlockchange </returns>
        public ChangeResult ChangeBlock(ushort x, ushort y, ushort z, ushort block)
        {
            ushort old = Level.GetBlock(x, y, z);
            ChangeResult result = Level.TryChangeBlock(this, x, y, z, block);
            if (result == ChangeResult.Unchanged) return result;
            if (result == ChangeResult.Modified) Level.BroadcastChange(x, y, z, block);
            ushort flags = 1 << 0;
            if (painting && DefaultSet.IsSolid(Level.CollideType(old)))
                flags = 1 << 1;
            Level.BlockDB.Cache.Add(this, x, y, z, flags, old, block);
            y--; 
            bool grow = Level.Config.GrassGrow && (Level.LevelPhysics == 0 || Level.LevelPhysics == 5);
            if (!grow || Level.CanAffect(this, x, y, z) != null) return result;
            ushort below = Level.GetBlock(x, y, z),
                grass = Level.Props[below].GrassBlock;
            if (grass != 0xff && block == 0)
                Level.Blockchange(this, x, y, z, grass);
            ushort dirt = Level.Props[below].DirtBlock;
            if (dirt != 0xff && !Level.LightPasses(block))
                Level.Blockchange(this, x, y, z, dirt);
            return result;
        }
        public void ProcessBlockchange(ushort x, ushort y, ushort z, byte action, ushort held)
        {
            try
            {
                if (spamChecker.CheckBlockSpam()) return;
                LastAction = DateTime.UtcNow;
                if (IsAfk) CmdAfk.ToggleAfk(this, "");
                ClientHeldBlock = held;
                if ((action == 0 || held == 0) && !Level.Config.Deletable)
                {
                    if (!Level.IsAirAt(x, y, z)) Message("Deleting blocks is disabled in this level.");
                    RevertBlock(x, y, z);
                    return;
                }
                else if (action == 1 && !Level.Config.Buildable)
                {
                    Message("Placing blocks is disabled in this level.");
                    RevertBlock(x, y, z); 
                    return;
                }
                if (held >= 256)
                {
                    if (!Session.hasBlockDefs || Level.CustomBlockDefs[held] == null)
                    {
                        Message("Invalid block type: " + Block.ToRaw(held));
                        RevertBlock(x, y, z);
                        return;
                    }
                }
                HandleManualChange(x, y, z, action != 0, held, true);
            }
            catch (Exception e)
            {
                Chat.MessageOps(DisplayName + " has triggered a block change error");
                Chat.MessageOps(e.GetType().ToString() + ": " + e.Message);
                Logger.LogError(e);
            }
        }
        public void ProcessMovement(int x, int y, int z, byte yaw, byte pitch, int held)
        {
            if (held >= 0) ClientHeldBlock = (ushort)held;
            if (Session.Ping.IgnorePosition || Loading)
                return;
            if (trainGrab || following.Length > 0) 
            { 
                CheckBlocks(Pos, Pos);
                return;
            }
            Position next = new(x, y, z);
            ProcessMovementCore(next, yaw, pitch, true);
        }
        /// <summary>
        /// Called to update player's position and check blocks and zones.
        /// If fromClient is true, calls OnPlayerMove event and updates AFK status.
        /// </summary>
        internal void ProcessMovementCore(Position next, byte yaw, byte pitch, bool fromClient)
        {
            CheckBlocks(Pos, next);
            if (fromClient)
            {
                bool cancel = false;
                OnPlayerMoveEvent.Call(this, next, yaw, pitch, ref cancel);
                if (cancel) 
                { 
                    cancel = false; 
                    return; 
                }
            }
            Pos = next;
            SetYawPitch(yaw, pitch);
            CheckZones(next);
            if (fromClient)
            {
                if (!Moved() || DateTime.UtcNow < AFKCooldown) return;
                LastAction = DateTime.UtcNow;
                if (IsAfk) CmdAfk.ToggleAfk(this, "");
            }
        }
        void CheckZones(Position pos)
        {
            Vec3S32 P = pos.BlockCoords;
            Zone zone = ZoneIn;
            if (zone != null && zone.Contains(P.X, P.Y, P.Z)) return;
            Zone[] zones = Level.Zones.Items;
            if (zones.Length == 0) return;
            for (int i = 0; i < zones.Length; i++)
            {
                if (!zones[i].Contains(P.X, P.Y, P.Z)) continue;
                ZoneIn = zones[i];
                OnChangedZoneEvent.Call(this);
                return;
            }
            ZoneIn = null;
            if (zone != null) OnChangedZoneEvent.Call(this);
        }
        int CurrentEnvProp(EnvProp i, Zone zone)
        {
            int value = Server.Config.GetEnvProp(i);
            bool block = i == EnvProp.SidesBlock || i == EnvProp.EdgeBlock;
            int default_ = block ? 0xff : int.MaxValue;
            if (Level.Config.GetEnvProp(i) != default_)
                value = Level.Config.GetEnvProp(i);
            if (zone != null && zone.Config.GetEnvProp(i) != default_)
                value = zone.Config.GetEnvProp(i);
            if (value == default_) value = EnvConfig.DefaultEnvProp(i, Level.Height);
            if (block) value = Session.ConvertBlock((ushort)value);
            return value;
        }
        public void SendCurrentEnv()
        {
            Zone zone = ZoneIn;
            for (int i = 0; i <= 7; i++)
            {
                string col = Server.Config.GetColor(i);
                if (Level.Config.GetColor(i) != "")
                    col = Level.Config.GetColor(i);
                if (zone != null && zone.Config.GetColor(i) != "")
                    col = zone.Config.GetColor(i);
                Session.SendSetEnvColor((byte)i, col);
            }
            if (Supports(CpeExt.EnvMapAspect) || Supports(CpeExt.EnvMapAspect, 2))
                for (EnvProp i = 0; i < EnvProp.Max; i++)
                    Send(Packet.EnvMapProperty(i, CurrentEnvProp(i, zone)));
            if (Supports(CpeExt.LightingMode))
            {
                EnvConfig cfg;
                if (zone != null && zone.Config.LightingMode != LightingMode.None)
                    cfg = zone.Config;
                else
                {
                    cfg = Level.Config.LightingMode switch
                    {
                        LightingMode.None => Server.Config,
                        _ => Level.Config,
                    };
                }
                Send(Packet.SetLightingMode(cfg.LightingMode, cfg.LightingModeLocked));
            }
            int weather = CurrentEnvProp(EnvProp.Weather, zone);
            Session.SendSetWeather((byte)weather);
        }
        void CheckBlocks(Position prev, Position next)
        {
            try
            {
                Vec3U16 P = (Vec3U16)prev.BlockCoords;
                AABB bb = ModelBB.OffsetPosition(next);
                int index = Level.PosToInt(P.X, P.Y, P.Z);
                if (Level.Config.SurvivalDeath)
                {
                    bool movingDown = next.Y < prev.Y;
                    PlayerPhysics.Drown(this, bb);
                    PlayerPhysics.Fall(this, bb, movingDown);
                }
                lastFallY = bb.Min.Y;
                PlayerPhysics.Walkthrough(this, bb);
                oldIndex = index;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
        bool Moved() => _lastRot.RotY != Rot.RotY || _lastRot.HeadX != Rot.HeadX;
        public void AnnounceDeath(string msg)
        {
            if (hidden)
                Message(msg.Replace("@p", "You").Replace("was", "were"));
            else
                Chat.MessageFromLevel(this, msg.Replace("@p", "λNICK"));
        }
        public bool HandleDeath(ushort block, string customMsg = "", bool explode = false, bool immediate = false)
        {
            if (!immediate && DateTime.UtcNow < deathCooldown) return false;
            if (invincible) return false;
            bool cancel = false;
            OnPlayerDyingEvent.Call(this, block, ref cancel);
            if (cancel) 
            { 
                cancel = false; 
                return false;
            }
            onTrain = false; 
            trainInvincible = false; 
            trainGrab = false;
            ushort x = (ushort)Pos.BlockX, y = (ushort)Pos.BlockY, z = (ushort)Pos.BlockZ;
            string deathMsg = Level.Props[block].DeathMessage;
            if (deathMsg != null) AnnounceDeath(deathMsg);
            if (block == 188) Level.MakeExplosion(x, y, z, 0);
            if (block == 231) Level.MakeExplosion(x, y, z, 1);
            if (block == 1 || block == 4)
            {
                if (explode) Level.MakeExplosion(x, y, z, 1);
                if (block == 1)
                    Chat.MessageFrom(this, customMsg.Replace("@p", "λNICK"));
                else
                    AnnounceDeath(customMsg);
            }
            TimeSpan cooldown = Server.Config.DeathCooldown;
            OnPlayerDiedEvent.Call(this, block, ref cooldown);
            PlayerActions.Respawn(this);
            TimesDied++;
            if (Server.Config.AnnounceDeathCount && TimesDied > 0 && TimesDied % 10 == 0)
                AnnounceDeath("@p &Shas died &3" + TimesDied + " times");
            deathCooldown = DateTime.UtcNow.Add(cooldown);
            return true;
        }
        public void ProcessChat(string text, bool continued)
        {
            LastAction = DateTime.UtcNow;
            if (FilterChat(ref text, continued)) return;
            if (text != "/afk" && IsAfk)
                CmdAfk.ToggleAfk(this, "");
            text = Chat.ParseInput(text, out bool isCommand);
            if (isCommand)
            { 
                DoCommand(text); 
                return; 
            }
            if (muted) 
            { 
                Message("You are muted.");
                return; 
            }
            if (Server.voting)
            {
                if (CheckVote(text, this, "y", "yes", ref Server.YesVotes) ||
                    CheckVote(text, this, "n", "no", ref Server.NoVotes)) return;
            }
            if (!CheckCanSpeak("speak")) return;
            if (Ignores.All)
            {
                Message("Your message wasn't sent because you're ignoring all chat.");
                Message("Use &T/ignore all &Sagain to toggle chat back on.");
                return;
            }
            if (ChatModes.Handle(this, text)) return;
            text = HandleJoker(text);
            OnPlayerChatEvent.Call(this, text);
            if (cancelchat) 
            { 
                cancelchat = false;
                return; 
            }
            Chat.MessageChat(this, "λFULL: &f" + text, null, true);
        }
        bool FilterChat(ref string text, bool continued)
        {
            if (text.StartsWith("/womid"))
            {
                UsingWom = true;
                return true;
            }
            if (continued)
            {
                if (text.Length < 64) text += " ";
                partialMessage += text;
                LimitPartialMessage();
                return true;
            }
            if (text.CaselessContains("^detail.user="))
            {
                Message("&WYou cannot use WoM detail strings in a chat message.");
                return true;
            }
            if (IsPartialSpaced(text))
            {
                AppendPartialMessage(text.Substring(0, text.Length - 2) + " ");
                return true;
            }
            else if (IsPartialJoined(text))
            {
                AppendPartialMessage(text.Substring(0, text.Length - 2));
                return true;
            }
            else if (partialMessage.Length > 0)
            {
                text = partialMessage + text;
                partialMessage = "";
            }
            text = Regex.Replace(text, "  +", " ");
            return text.Length == 0;
        }
        static bool IsPartialSpaced(string text) => text.EndsWith(" >") || text.EndsWith(" /");
        static bool IsPartialJoined(string text) => text.EndsWith(" <") || text.EndsWith(" \\");
        void LimitPartialMessage()
        {
            if (partialMessage.Length < 100 * 64) return;
            partialMessage = "";
            Message("&WPartial message cleared due to exceeding 100 lines");
        }
        void AppendPartialMessage(string part)
        {
            if (!partialLog.AddSpamEntry(20, TimeSpan.FromSeconds(1)))
            {
                Message("&WTried to add over 20 partial message in one second, slow down");
                return;
            }
            partialMessage += part;
            SendRawMessage("&3Partial message: &f" + partialMessage);
            LimitPartialMessage();
        }
        void DoCommand(string text)
        {
            if (text.Length == 0)
            {
                text = lastCMD;
                if (text.Length == 0)
                {
                    Message("Cannot repeat command - no commands used yet."); 
                    return;
                }
                Message("Repeating &T/" + text);
            }
            text.Separate(' ', out string cmd, out string args);
            HandleCommand(cmd, args, DefaultCmdData);
        }
        string HandleJoker(string text)
        {
            if (!joker) return text;
            Logger.Log(LogType.PlayerChat, "<JOKER>: {0}: {1}", name, text);
            Chat.MessageFromOps(this, "&S<&aJ&bO&cK&5E&9R&S>: λNICK:&f " + text);
            TextFile jokerFile = TextFile.Files["Joker"];
            jokerFile.EnsureExists();
            string[] lines = jokerFile.GetText();
            Random rnd = new();
            return lines.Length > 0 ? lines[rnd.Next(lines.Length)] : text;
        }
        public void HandleCommand(string cmd, string args, CommandData data)
        {
            cmd = cmd.ToLower();
            if (!Server.Config.CmdSpamCheck && !CheckMBRecursion(data)) return;
            try
            {
                Command command = GetCommand(ref cmd, ref args, data);
                if (command == null) return;
                bool parallel = command.Parallelism == CommandParallelism.Yes
                                    || data.Context == CommandContext.MessageBlock;
                if (!parallel && !EnqueueSerialCommand(command, args, data)) return;
                ThreadStart callback;
                if (parallel)
                    callback = () => UseCommand(command, args, data);
                else
                    callback = ExecuteSerialCommands;
                Utils.StartBackgroundThread("CMD_ " + cmd, callback);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                Message("&WCommand failed");
            }
        }
        public void HandleCommands(List<string> cmds, CommandData data)
        {
            List<string> messages = new(cmds.Count);
            List<Command> commands = new(cmds.Count);
            if (!Server.Config.CmdSpamCheck && !CheckMBRecursion(data)) return;
            try
            {
                foreach (string raw in cmds)
                {
                    string[] parts = raw.SplitSpaces(2);
                    string cmd = parts[0].ToLower(),
                        args = parts.Length > 1 ? parts[1] : "";
                    Command command = GetCommand(ref cmd, ref args, data);
                    if (command == null) return;
                    messages.Add(args); 
                    commands.Add(command);
                }
                Utils.StartBackgroundThread("CMDS_",
                                   () => UseCommands(commands, messages, data));
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                Message("&WCommand failed.");
            }
        }
        bool UseCommands(List<Command> commands, List<string> messages, CommandData data)
        {
            Level startingLevel = Level;
            for (int i = 0; i < messages.Count; i++)
            {
                if (Level != startingLevel)
                {
                    int remaining = messages.Count - i;
                    Message("&WCancelled {0} queued command{1} because you switched levels.", remaining, remaining == 1 ? "" : "s");
                    return false;
                }
                if (!UseCommand(commands[i], messages[i], data)) return false;
                if (leftServer) return false;
            }
            return true;
        }
        bool CheckMBRecursion(CommandData data)
        {
            if (data.Context == CommandContext.MessageBlock)
            {
                mbRecursion++;
                if (mbRecursion >= 100)
                {
                    mbRecursion = 0;
                    Message("&WInfinite message block loop detected, aborting");
                    return false;
                }
            }
            else if (data.Context == CommandContext.Normal)
                mbRecursion = 0;
            return true;
        }
        bool CheckCommand(string cmd)
        {
            if (cmd.Length == 0)
            { 
                Message("No command entered."); 
                return false;
            }
            if (Server.Config.AgreeToRulesOnEntry && !agreed && !(cmd == "agree" || cmd == "rules" || cmd == "disagree" || cmd == "pass" || cmd == "setpass"))
            {
                Message("You must read /rules then agree to them with /agree!"); 
                return false;
            }
            if (Unverified && !(cmd == "pass" || cmd == "setpass"))
            {
                ExtraAuthenticator.RequiresVerification(this, "use /" + cmd);
                return false;
            }
            TimeSpan delta = cmdUnblocked - DateTime.UtcNow;
            if (delta.TotalSeconds > 0)
            {
                Message("Blocked from using commands for another " + ((int)Math.Ceiling(delta.TotalSeconds)) + " seconds");
                return false;
            }
            return true;
        }
        Command GetCommand(ref string cmdName, ref string cmdArgs, CommandData data)
        {
            if (!CheckCommand(cmdName)) return null;
            if (CmdBindings.TryGetValue(cmdName, out string bound))
                bound.Separate(' ', out cmdName, out cmdArgs);
            else if (byte.TryParse(cmdName, out byte bindIndex) && bindIndex < 10)
            {
                Message("No command is bound to: &T/" + cmdName);
                return null;
            }
            Command.Search(ref cmdName, ref cmdArgs);
            OnPlayerCommandEvent.Call(this, cmdName, cmdArgs, data);
            if (cancelcommand) 
            { 
                cancelcommand = false; 
                return null;
            }
            Command command = Command.Find(cmdName);
            if (command == null)
            {
                Command modeCmd = Command.Find("Mode");
                if (modeCmd != null && Block.Parse(this, cmdName) != Block.Invalid)
                {
                    cmdArgs = cmdName;
                    cmdName = "mode";
                    command = modeCmd;
                }
                else
                {
                    Logger.Log(LogType.CommandUsage, "{0} tried to use unknown command: /{1} {2}", name, cmdName, cmdArgs);
                    Message("Unknown command \"{0}\".", cmdName); 
                    return null;
                }
            }
            if (!CanUse(command))
            {
                command.Permissions.MessageCannotUse(this);
                return null;
            }
            if (Level != null && Level.IsMuseum && !command.MuseumUsable)
            {
                Message("Cannot use &T/{0} &Swhile in a museum.", command.Name);
                return null;
            }
            if (frozen && !command.UseableWhenFrozen)
            {
                Message("Cannot use &T/{0} &Swhile frozen.", command.Name);
                return null;
            }
            return command;
        }
        bool UseCommand(Command command, string args, CommandData data)
        {
            string cmd = command.Name;
            if (command.UpdatesLastCmd)
            {
                lastCMD = args.Length == 0 ? cmd : cmd + " " + args;
                lastCmdTime = DateTime.UtcNow;
            }
            if (command.LogUsage) Logger.Log(LogType.CommandUsage, "{0} used /{1} {2}", name, cmd, args);
            try
            {
                if (Server.Opstats.CaselessContains(cmd) || (cmd.CaselessEq("review") && args.CaselessEq("next") && Server.reviewlist.Count > 0))
                    Database.AddRow("Opstats", "Time, Name, Cmd, Cmdmsg",
                                    DateTime.Now.ToInvariantDateString(), name, cmd, args);
            }
            catch 
            { 
            }
            try
            {
                command.Use(this, args, data);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                Message("&WAn error occured when using the command!");
                Message(e.GetType() + ": " + e.Message);
                return false;
            }
            return spamChecker == null || !spamChecker.CheckCommandSpam();
        }
        bool EnqueueSerialCommand(Command cmd, string args, CommandData data)
        {
            SerialCommand head = default, scmd;
            scmd.cmd = cmd;
            scmd.args = args;
            scmd.data = data;
            lock (serialCmdsLock)
            {
                if (serialCmds.Count > 0)
                    head = serialCmds.Peek();
                serialCmds.Enqueue(scmd);
            }
            if (head.cmd == null) return true;
            if (cmd.Parallelism == CommandParallelism.NoAndWarn)
                Message("Waiting for &T/{0} {1} &Sto finish first before running &T/{2} {3}",
                        head.cmd.Name, head.args, cmd.Name, args);
            spamChecker.CheckCommandSpam();
            return false;
        }
        void ExecuteSerialCommands()
        {
            for (; ; )
            {
                SerialCommand scmd;
                lock (serialCmdsLock)
                {
                    if (serialCmds.Count == 0) return;
                    scmd = serialCmds.Peek();
                }
                UseCommand(scmd.cmd, scmd.args, scmd.data);
                lock (serialCmdsLock)
                {
                    if (serialCmds.Count == 0) return;
                    serialCmds.Dequeue();
                }
            }
        }
        void ClearSerialCommands()
        {
            lock (serialCmdsLock)
                serialCmds.Clear();
        }
    }
}
