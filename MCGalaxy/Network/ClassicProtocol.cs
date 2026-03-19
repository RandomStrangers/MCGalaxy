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
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
using MCGalaxy.Maths;
using System;
using System.Collections.Generic;
using System.Threading;
namespace MCGalaxy.Network
{
    public class ClassicProtocol : IGameSession
    {
        bool hasEmoteFix, hasTwoWayPing, hasExtTexs, hasTextColors,
            hasHeldBlock, hasLongerMessages, finishedCpeLogin;
        int extensionCount;
        CpeExt[] extensions = CpeExtension.Empty;
        public ClassicProtocol(INetSocket s)
        {
            socket = s;
            player = new Player(s, this);
        }
        public override int MaxEntityID => 254;
        protected override int HandlePacket(byte[] buffer, int offset, int left)
        {
            switch (buffer[offset])
            {
                case 1: 
                    return 1;
                case 0:
                    return HandleLogin(buffer, offset, left);
                case 5: 
                    return HandleBlockchange(buffer, offset, left);
                case 8: 
                    return HandleMovement(buffer, offset, left);
                case 13:
                    return HandleChat(buffer, offset, left);
                case 16:
                    return HandleExtInfo(buffer, offset, left);
                case 17: 
                    return HandleExtEntry(buffer, offset, left);
                case 34: 
                    return HandlePlayerClicked(buffer, offset, left);
                case 43: 
                    return HandleTwoWayPing(buffer, offset, left);
                case 53: 
                    return HandlePluginMessage(buffer, offset, left);
                case 57: 
                    return HandleNotifyAction(buffer, offset, left);
                case 58: 
                    return HandleNotifyPositionAction(buffer, offset, left);
                case 19:
                    return left < 2 ? 0 : 2;
                default:
                    player.Leave("Unhandled opcode \"" + buffer[offset] + "\"!", true);
                    return left;
            }
        }
        ushort ReadBlock(byte[] buffer, int offset)
        {
            ushort block = hasExtBlocks ? MemUtils.ReadU16_BE(buffer, offset) : buffer[offset];
            if (block > 767) block = 767;
            return Block.FromRaw(block);
        }
        #region Classic processing
        int HandleLogin(byte[] buffer, int offset, int left)
        {
            int old_size = 1 + 1 + 64 + 64,
                new_size = 1 + 1 + 64 + 64 + 1;
            if (left < old_size) return 0;
            ProtocolVersion = buffer[offset + 1];
            int size = ProtocolVersion >= 6 ? new_size : old_size;
            if (left < size) return 0;
            if (player.loggedIn) return size;
            if (ProtocolVersion >= 7)
                hasCpe = buffer[offset + 130] == 0x42 && Server.Config.EnableCPE;
            string name = NetUtils.ReadString(buffer, offset + 2),
                mppass = NetUtils.ReadString(buffer, offset + 66);
            if (!player.ProcessLogin(name, mppass)) return left;
            UpdateFallbackTable();
            if (hasCpe)
                SendCpeExtensions();
            else
                player.CompleteLoginProcess();
            return size;
        }
        int HandleBlockchange(byte[] buffer, int offset, int left)
        {
            int size = 1 + 6 + 1 + (hasExtBlocks ? 2 : 1);
            if (left < size) return 0;
            if (!player.loggedIn) return size;
            ushort x = MemUtils.ReadU16_BE(buffer, offset + 1),
                y = MemUtils.ReadU16_BE(buffer, offset + 3),
                z = MemUtils.ReadU16_BE(buffer, offset + 5);
            byte action = buffer[offset + 7];
            if (action > 1)
            {
                player.Leave("Unknown block action!", true);
                return left;
            }
            ushort held = ReadBlock(buffer, offset + 8);
            player.ProcessBlockchange(x, y, z, action, held);
            return size;
        }
        int HandleMovement(byte[] buffer, int offset, int left)
        {
            int size = 1 + 6 + 2 + (player.hasExtPositions ? 6 : 0) + (hasExtBlocks ? 2 : 1);
            if (left < size) return 0;
            if (!player.loggedIn) return size;
            int held = -1;
            if (hasHeldBlock)
            {
                held = ReadBlock(buffer, offset + 1);
                if (hasExtBlocks) offset++;
            }
            int x, y, z;
            if (player.hasExtPositions)
            {
                x = MemUtils.ReadI32_BE(buffer, offset + 2);
                y = MemUtils.ReadI32_BE(buffer, offset + 6);
                z = MemUtils.ReadI32_BE(buffer, offset + 10);
                offset += 6;
            }
            else
            {
                x = MemUtils.ReadI16_BE(buffer, offset + 2);
                y = MemUtils.ReadI16_BE(buffer, offset + 4);
                z = MemUtils.ReadI16_BE(buffer, offset + 6);
            }
            byte yaw = buffer[offset + 8],
                pitch = buffer[offset + 9];
            player.ProcessMovement(x, y, z, yaw, pitch, held);
            return size;
        }
        int HandleChat(byte[] buffer, int offset, int left)
        {
            int size = 1 + 1 + 64;
            if (left < size) return 0;
            if (!player.loggedIn) return size;
            bool continued = hasLongerMessages && buffer[offset + 1] != 0;
            string text = NetUtils.ReadString(buffer, offset + 2);
            player.ProcessChat(text, continued);
            return size;
        }
        #endregion
        #region CPE processing
        public override bool Supports(string extName, int version = 1) => FindExtension(extName) != null && FindExtension(extName).ClientVersion == version;
        CpeExt FindExtension(string extName)
        {
            foreach (CpeExt ext in extensions)
                if (ext.Name.CaselessEq(extName)) return ext;
            return null;
        }
        void SendCpeExtensions()
        {
            extensions = CpeExtension.GetAllEnabled();
            Send(Packet.ExtInfo((byte)(extensions.Length + 1)));
            Send(Packet.ExtEntry(CpeExt.EnvMapAppearance, 1));
            foreach (CpeExt ext in extensions)
                Send(Packet.ExtEntry(ext.Name, ext.ServerVersion));
        }
        void CheckReadAllExtensions()
        {
            if (extensionCount <= 0 && !finishedCpeLogin)
            {
                player.CompleteLoginProcess();
                finishedCpeLogin = true;
            }
        }
        int HandleExtInfo(byte[] buffer, int offset, int left)
        {
            int size = 1 + 64 + 2;
            if (left < size) return 0;
            appName = NetUtils.ReadString(buffer, offset + 1);
            extensionCount = buffer[offset + 66];
            CheckReadAllExtensions();
            return size;
        }
        int HandleExtEntry(byte[] buffer, int offset, int left)
        {
            int size = 1 + 64 + 4;
            if (left < size) return 0;
            string extName = NetUtils.ReadString(buffer, offset + 1);
            int extVersion = MemUtils.ReadI32_BE(buffer, offset + 65);
            if (extVersion == 0x03110003)
            {
                player.Leave("Classic+ Client is unsupported");
                return size;
            }
            AddExtension(extName, extVersion);
            extensionCount--;
            CheckReadAllExtensions();
            return size;
        }
        int HandlePlayerClicked(byte[] buffer, int offset, int left)
        {
            if (left < 15) return 0;
            MouseButton Button = (MouseButton)buffer[offset + 1];
            MouseAction Action = (MouseAction)buffer[offset + 2];
            ushort yaw = MemUtils.ReadU16_BE(buffer, offset + 3),
                pitch = MemUtils.ReadU16_BE(buffer, offset + 5);
            byte entityID = buffer[offset + 7];
            ushort x = MemUtils.ReadU16_BE(buffer, offset + 8),
                y = MemUtils.ReadU16_BE(buffer, offset + 10),
                z = MemUtils.ReadU16_BE(buffer, offset + 12);
            TargetBlockFace face = (TargetBlockFace)buffer[offset + 14];
            if (face > TargetBlockFace.None) face = TargetBlockFace.None;
            OnPlayerClickEvent.Call(player, Button, Action, yaw, pitch, entityID, x, y, z, face);
            return 15;
        }
        int HandleNotifyAction(byte[] buffer, int offset, int left)
        {
            if (left < 5) return 0;
            NotifyActionType action = (NotifyActionType)buffer[offset + 2];
            short value = MemUtils.ReadI16_BE(buffer, offset + 3);
            OnNotifyActionEvent.Call(player, action, value);
            return 5;
        }
        int HandleNotifyPositionAction(byte[] buffer, int offset, int left)
        {
            if (left < 9) return 0;
            NotifyActionType action = (NotifyActionType)buffer[offset + 2];
            ushort x = MemUtils.ReadU16_BE(buffer, offset + 3),
                y = MemUtils.ReadU16_BE(buffer, offset + 5),
                z = MemUtils.ReadU16_BE(buffer, offset + 7);
            OnNotifyPositionActionEvent.Call(player, action, x, y, z);
            return 9;
        }
        int HandleTwoWayPing(byte[] buffer, int offset, int left)
        {
            if (left < 4) return 0;
            bool serverToClient = buffer[offset + 1] != 0;
            ushort data = MemUtils.ReadU16_BE(buffer, offset + 2);
            if (!serverToClient)
                Send(Packet.TwoWayPing(false, data));
            else
            {
                Ping.UnIgnorePosition(data);
                Ping.Update(data);
            }
            return 4;
        }
        int HandlePluginMessage(byte[] buffer, int offset, int left)
        {
            if (left < 66) return 0;
            byte channel = buffer[offset + 1];
            byte[] data = new byte[64];
            Array.Copy(buffer, offset + 2, data, 0, 64);
            OnPluginMessageReceivedEvent.Call(player, channel, data);
            return 66;
        }
        void AddExtension(string extName, int version)
        {
            Player p = player;
            CpeExt ext = FindExtension(extName);
            if (ext == null) return;
            ext.ClientVersion = (byte)version;
            switch (ext.Name)
            {
                case CpeExt.CustomBlocks:
                    if (version == 1) Send(Packet.CustomBlockSupportLevel(1));
                    hasCustomBlocks = true;
                    UpdateFallbackTable();
                    if (MaxRawBlock < 65) MaxRawBlock = 65;
                    break;
                case CpeExt.ChangeModel:
                    p.hasChangeModel = true;
                    break;
                case CpeExt.EmoteFix:
                    hasEmoteFix = true;
                    break;
                case CpeExt.FullCP437:
                    p.hasCP437 = true;
                    break;
                case CpeExt.ExtPlayerList:
                    p.hasExtList = true;
                    break;
                case CpeExt.BlockDefinitions:
                    hasBlockDefs = true;
                    if (MaxRawBlock < 255) MaxRawBlock = 255;
                    break;
                case CpeExt.TextColors:
                    hasTextColors = true;
                    SendGlobalColors();
                    break;
                case CpeExt.ExtEntityPositions:
                    p.hasExtPositions = true;
                    break;
                case CpeExt.TwoWayPing:
                    hasTwoWayPing = true;
                    break;
                case CpeExt.BulkBlockUpdate:
                    hasBulkBlockUpdate = true;
                    break;
                case CpeExt.ExtTextures:
                    hasExtTexs = true;
                    break;
                case CpeExt.HeldBlock:
                    hasHeldBlock = true;
                    break;
                case CpeExt.LongerMessages:
                    hasLongerMessages = true;
                    break;
                case CpeExt.ExtBlocks:
                    hasExtBlocks = true;
                    if (MaxRawBlock < 767) MaxRawBlock = 767;
                    break;
            }
        }
        void SendGlobalColors()
        {
            for (int i = 0; i < Colors.List.Length; i++)
            {
                if (!Colors.List[i].IsModified()) continue;
                Send(Packet.SetTextColor(Colors.List[i]));
            }
        }
        #endregion
        #region Classic packet sending
        public override void SendTeleport(byte id, Position pos, Orientation rot)
        {
            if (id == 0xFF && ProtocolVersion < 5)
            {
                SendSpawnEntity(id, player.color + player.truename, player.SkinName, pos, rot);
                return;
            }
            bool self = id == 0xFF;
            if (self) pos.Y -= 22;
            SendTeleportCore(self, Packet.Teleport(id, pos, rot, player.hasExtPositions));
        }
        public override bool SendTeleport(byte id, Position pos, Orientation rot,
                                          TeleportMoveMode moveMode, bool usePos = true, bool interpolateOri = false, bool useOri = true)
        {
            if (!Supports(CpeExt.ExtEntityTeleport))
                return false;
            bool absoluteSelf = (moveMode == TeleportMoveMode.AbsoluteInstant ||
                moveMode == TeleportMoveMode.AbsoluteSmooth) && id == 0xFF;
            if (absoluteSelf) pos.Y -= 22;
            SendTeleportCore(absoluteSelf, Packet.TeleportExt(id, usePos, moveMode, useOri, interpolateOri, pos, rot, player.hasExtPositions));
            return true;
        }
        void SendTeleportCore(bool absoluteSelf, byte[] packet)
        {
            if (!absoluteSelf || !hasTwoWayPing)
            {
                Send(packet);
                return;
            }
            byte[] pingPacket = Packet.TwoWayPing(true, Ping.NextTwoWayPingData(true)),
                merged = new byte[packet.Length + pingPacket.Length];
            Buffer.BlockCopy(packet, 0, merged, 0, packet.Length);
            Buffer.BlockCopy(pingPacket, 0, merged, packet.Length, pingPacket.Length);
            Send(merged);
        }
        public override void SendRemoveEntity(byte id) => Send(Packet.RemoveEntity(id));
        public override void SendChat(string message)
        {
            char[] buffer = LineWrapper.CleanupColors(message, out int bufferLen,
                                                      hasTextColors, hasTextColors);
            List<string> lines = LineWrapper.Wordwrap(buffer, bufferLen, hasEmoteFix);
            for (int i = 0; i < lines.Count;)
            {
                int count = Math.Min(62, lines.Count - i);
                byte[] data = new byte[count * 66];
                for (int j = 0; j < count; i++, j++)
                    Packet.WriteMessage(lines[i], 0, player.hasCP437, data, j * 66);
                Send(data);
            }
        }
        public override void SendMessage(CpeMessageType type, string message) => Send(Packet.Message(CleanupColors(message), type, player.hasCP437));
        public override void SendKick(string reason, bool sync)
        {
            reason = CleanupColors(reason);
            byte[] buffer = Packet.Kick(reason, player.hasCP437);
            socket.Send(buffer, sync ? SendFlags.Synchronous : SendFlags.None);
        }
        public override bool SendSetUserType(byte type)
        {
            if (ProtocolVersion < 7) return false;
            Send(Packet.UserType(type));
            return true;
        }
        #endregion
        #region CPE packet sending
        public override void SendAddTabEntry(byte id, string name, string nick, string group, byte groupRank) => Send(Packet.ExtAddPlayerName(id, name, CleanupColors(nick), CleanupColors(group), groupRank, player.hasCP437));
        public override void SendRemoveTabEntry(byte id) => Send(Packet.ExtRemovePlayerName(id));
        public override bool SendSetReach(float reach)
        {
            if (!Supports(CpeExt.ClickDistance)) return false;
            Send(Packet.ClickDistance((short)(reach * 32)));
            return true;
        }
        public override bool SendHoldThis(ushort block, bool locked)
        {
            if (!hasHeldBlock) return false;
            ushort raw = ConvertBlock(block);
            Send(Packet.HoldThis(raw, locked, hasExtBlocks));
            return true;
        }
        public override bool SendSetEnvColor(byte type, string hex)
        {
            if (!Supports(CpeExt.EnvColors)) return false;
            if (Colors.TryParseHex(hex, out ColorDesc c))
                Send(Packet.EnvColor(type, c.R, c.G, c.B));
            else
                Send(Packet.EnvColor(type, -1, -1, -1));
            return true;
        }
        public override void SendChangeModel(byte id, string model)
        {
            if (ushort.TryParse(model, out ushort raw) && raw > MaxRawBlock)
            {
                ushort block = Block.FromRaw(raw);
                model = block >= 1024 ? "humanoid" : ConvertBlock(block).ToString();
            }
            Send(Packet.ChangeModel(id, model, player.hasCP437));
        }
        public override void SendEntityProperty(byte id, EntityProp prop, int value) => Send(Packet.EntityProperty(id, prop, value));
        public override bool SendSetWeather(byte weather)
        {
            if (!Supports(CpeExt.EnvWeatherType)) return false;
            Send(Packet.EnvWeatherType(weather));
            return true;
        }
        public override bool SendSetTextColor(ColorDesc color)
        {
            if (!hasTextColors) return false;
            Send(Packet.SetTextColor(color));
            return true;
        }
        public override bool SendDefineBlock(BlockDefinition def)
        {
            if (!hasBlockDefs || def.RawID > MaxRawBlock) return false;
            byte[] packet = Supports(CpeExt.BlockDefinitionsExt, 2) && def.Shape != 0
                ? Packet.DefineBlockExt(def, true, player.hasCP437, hasExtBlocks, hasExtTexs)
                : Supports(CpeExt.BlockDefinitionsExt) && def.Shape != 0
                ? Packet.DefineBlockExt(def, false, player.hasCP437, hasExtBlocks, hasExtTexs)
                : Packet.DefineBlock(def, player.hasCP437, hasExtBlocks, hasExtTexs);
            Send(packet);
            return true;
        }
        public override bool SendUndefineBlock(BlockDefinition def)
        {
            if (!hasBlockDefs || def.RawID > MaxRawBlock) return false;
            Send(Packet.UndefineBlock(def, hasExtBlocks));
            return true;
        }
        public override bool SendAddSelection(byte id, string label, Vec3U16 p1, Vec3U16 p2, ColorDesc color)
        {
            if (!Supports(CpeExt.SelectionCuboid)) return false;
            Send(Packet.MakeSelection(id, label, p1, p2,
                                      color.R, color.G, color.B, color.A, player.hasCP437));
            return true;
        }
        public override bool SendRemoveSelection(byte id)
        {
            if (!Supports(CpeExt.SelectionCuboid)) return false;
            Send(Packet.DeleteSelection(id));
            return true;
        }
        #endregion
        #region Higher level sending
        public override void SendMotd(string motd)
        {
            motd = CleanupColors(motd);
            Send(Packet.Motd(player, motd));
            if (!Supports(CpeExt.HackControl)) return;
            Send(Hacks.MakeHackControl(player, motd));
        }
        public override void SendPing()
        {
            if (hasTwoWayPing)
                Send(Packet.TwoWayPing(true, Ping.NextTwoWayPingData()));
            else
                Send(Packet.Ping());
        }
        public override void SendSetSpawnpoint(Position pos, Orientation rot)
        {
            if (Supports(CpeExt.SetSpawnpoint))
                Send(Packet.SetSpawnpoint(pos, rot, player.hasExtPositions));
            else
                Entities.Spawn(player, player, pos, rot);
        }
        public override bool SendCinematicGui(CinematicGui gui)
        {
            if (!Supports(CpeExt.CinematicGui)) return false;
            float barSize = gui.barSize;
            barSize = Math.Max(0, Math.Min(1, barSize));
            Send(Packet.SetCinematicGui(
                gui.hideCrosshair, gui.hideHand, gui.hideHotbar,
                gui.barColor.R, gui.barColor.G, gui.barColor.B, gui.barColor.A,
                (ushort)(barSize * ushort.MaxValue)));
            return true;
        }
        public override void SendSpawnEntity(byte id, string name, string skin, Position pos, Orientation rot)
        {
            name = CleanupColors(name);
            bool self = id == 0xFF;
            if (self) pos.Y -= 22;
            if (self && ProtocolVersion <= 3)
            {
                byte temp = rot.HeadX;
                rot.HeadX = rot.RotY;
                rot.RotY = (byte)(256 - temp);
            }
            byte[] packet;
            if (Supports(CpeExt.ExtPlayerList, 2))
                packet = Packet.ExtAddEntity2(id, skin, name, pos, rot, player.hasCP437, player.hasExtPositions);
            else if (player.hasExtList)
            {
                byte[] addEntity = Packet.ExtAddEntity(id, skin, name, player.hasCP437),
                    teleport = Packet.Teleport(id, pos, rot, player.hasExtPositions);
                packet = new byte[addEntity.Length + teleport.Length];
                Buffer.BlockCopy(addEntity, 0, packet, 0, addEntity.Length);
                Buffer.BlockCopy(teleport, 0, packet, addEntity.Length, teleport.Length);
            }
            else
                packet = Packet.AddEntity(id, name, pos, rot, player.hasCP437, player.hasExtPositions);
            SendTeleportCore(self, packet);
        }
        public override void SendLevel(Level prev, Level level)
        {
            int volume = level.blocks.Length;
            if (Supports(CpeExt.FastMap))
                Send(Packet.LevelInitaliseExt(volume));
            else
                Send(Packet.LevelInitalise());
            if (hasBlockDefs)
            {
                if (prev != null && prev != level)
                    RemoveOldLevelCustomBlocks(prev);
                BlockDefinition.SendLevelCustomBlocks(player);
                if (Supports(CpeExt.InventoryOrder))
                    BlockDefinition.SendLevelInventoryOrder(player);
            }
            LevelChunkStream.SendLevel(this, level, volume);
            if (level.Config.LoadDelay > 0)
                Thread.Sleep(level.Config.LoadDelay);
            Send(Packet.LevelFinalise(level.Width, level.Height, level.Length));
        }
        void RemoveOldLevelCustomBlocks(Level oldLevel)
        {
            BlockDefinition[] defs = oldLevel.CustomBlockDefs;
            for (int i = 0; i < defs.Length; i++)
            {
                BlockDefinition def = defs[i];
                if (def == BlockDefinition.GlobalDefs[i] || def == null) continue;
                SendUndefineBlock(def);
            }
        }
        #endregion
        public override void SendBlockchange(ushort x, ushort y, ushort z, ushort block)
        {
            byte[] buffer = new byte[hasExtBlocks ? 9 : 8];
            buffer[0] = 6;
            NetUtils.WriteU16(x, buffer, 1);
            NetUtils.WriteU16(y, buffer, 3);
            NetUtils.WriteU16(z, buffer, 5);
            ushort raw = ConvertBlock(block);
            NetUtils.WriteBlock(raw, buffer, 7, hasExtBlocks);
            socket.Send(buffer, SendFlags.LowPriority);
        }
        public override byte[] MakeBulkBlockchange(BufferedBlockSender buffer) => buffer.MakeLimited(fallback);
        void UpdateFallbackTable()
        {
            for (byte b = 0; b <= 65; b++)
                fallback[b] = hasCustomBlocks ? b : Block.ConvertClassic(b, ProtocolVersion);
        }
        string CleanupColors(string value) => LineWrapper.CleanupColors(value, hasTextColors, hasTextColors);
        public override string ClientName() => !string.IsNullOrEmpty(appName)
                ? appName
                : ProtocolVersion switch
                {
                    3 => "Classic 0.0.16",
                    4 => "Classic 0.0.17-0.0.18",
                    _ => ProtocolVersion == 5 ? "Classic 0.0.19" : ProtocolVersion == 6 ? "Classic 0.0.20-0.0.23" : "Classic 0.28-0.30",
                };
        public override unsafe void GetPositionPacket(ref byte* ptr, byte id, bool srcExtPos, bool extPos,
                                                    Position pos, Position oldPos, Orientation rot, Orientation oldRot)
        {
            Position delta = GetDelta(pos, oldPos, srcExtPos);
            bool posChanged = delta.X != 0 || delta.Y != 0 || delta.Z != 0,
                oriChanged = rot.RotY != oldRot.RotY || rot.HeadX != oldRot.HeadX,
                absPosUpdate = Math.Abs(delta.X) > 32 || Math.Abs(delta.Y) > 32 || Math.Abs(delta.Z) > 32;
            if (absPosUpdate)
            {
                *ptr = 8; 
                ptr++;
                *ptr = id; 
                ptr++;
                if (extPos)
                {
                    WriteI32(ref ptr, pos.X); 
                    WriteI32(ref ptr, pos.Y);
                    WriteI32(ref ptr, pos.Z);
                }
                else
                {
                    WriteI16(ref ptr, (short)pos.X); 
                    WriteI16(ref ptr, (short)pos.Y); 
                    WriteI16(ref ptr, (short)pos.Z);
                }
            }
            else if (posChanged)
            {
                byte opcode = oriChanged ? (byte)9 : (byte)10;
                *ptr = opcode; 
                ptr++;
                *ptr = id;
                ptr++;
                *ptr = (byte)delta.X; 
                ptr++;
                *ptr = (byte)delta.Y; 
                ptr++;
                *ptr = (byte)delta.Z; 
                ptr++;
            }
            else if (oriChanged)
            {
                *ptr = 11; 
                ptr++;
                *ptr = id;
                ptr++;
            }
            if (absPosUpdate || oriChanged)
            {
                *ptr = rot.RotY; 
                ptr++;
                *ptr = rot.HeadX; 
                ptr++;
            }
        }
        static unsafe void WriteI32(ref byte* ptr, int value)
        {
            *ptr = (byte)(value >> 24); 
            ptr++; 
            *ptr = (byte)(value >> 16); 
            ptr++;
            *ptr = (byte)(value >> 8); 
            ptr++; 
            *ptr = (byte)value;
            ptr++;
        }
        static unsafe void WriteI16(ref byte* ptr, short value)
        {
            *ptr = (byte)(value >> 8); 
            ptr++;
            *ptr = (byte)value; 
            ptr++;
        }
        static Position GetDelta(Position pos, Position old, bool extPositions)
        {
            Position delta = new(pos.X - old.X, pos.Y - old.Y, pos.Z - old.Z);
            if (extPositions) return delta;
            delta.X = (short)delta.X; 
            delta.Y = (short)delta.Y; 
            delta.Z = (short)delta.Z;
            return delta;
        }
    }
}
