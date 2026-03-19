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
using MCGalaxy.Maths;
using MCGalaxy.Network;
using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    class VisibleSelection
    {
        public object data;
        public byte ID;
    }
    public partial class Player : IDisposable
    {
        public void Send(byte[] buffer) => Socket.Send(buffer, SendFlags.None);
        public void MessageLines(IEnumerable<string> lines)
        {
            lock (messageLocker)
                foreach (string line in lines)
                    Message(line);
        }
        public void Message(string message, params object[] args) => Message(string.Format(message, args));
        public virtual void Message(string message)
        {
            if (message.Length > 0 && !(message[0] == '&' || message[0] == '%')) message = "&S" + message;
            message = Chat.Format(message, this);
            lock (messageLocker)
                SendRawMessage(message);
        }
        void SendRawMessage(string message)
        {
            bool cancel = false;
            OnMessageRecievedEvent.Call(this, ref message, ref cancel);
            if (cancel) return;
            try
            {
                Session.SendChat(message);
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
        public void SendCpeMessage(CpeMessageType type, string message, PersistentMessagePriority priority = PersistentMessagePriority.Normal)
        {
            if (type != CpeMessageType.Normal && !Supports(CpeExt.MessageTypes))
                switch (type)
                {
                    case CpeMessageType.Announcement:
                        type = CpeMessageType.Normal;
                        break;
                    default:
                        return;
                }
            message = Chat.Format(message, this);
            if (!persistentMessages.Handle(type, ref message, priority)) return;
            Session.SendMessage(type, message);
        }
        public void SendMapMotd()
        {
            string motd = GetMotd();
            motd = Chat.Format(motd, this);
            OnSendingMotdEvent.Call(this, ref motd);
            if (Game.Referee)
            {
                motd = motd
                    .Replace("-hax", "+hax").Replace("-noclip", "+noclip")
                    .Replace("-speed", "+speed").Replace("-respawn", "+respawn")
                    .Replace("-fly", "+fly").Replace("-thirdperson", "+thirdperson");
            }
            Session.SendMotd(motd);
        }
        public bool SendRawMap(Level oldLevel, Level level)
        {
            lock (joinLock)
                return SendRawMapCore(oldLevel, level);
        }
        bool SendRawMapCore(Level prev, Level level)
        {
            bool success = true;
            try
            {
                if (Level.blocks == null)
                    throw new InvalidOperationException("Tried to join unloaded level");
                useCheckpointSpawn = false;
                lastCheckpointIndex = -1;
                AFKCooldown = DateTime.UtcNow.AddSeconds(2);
                ZoneIn = null;
                AllowBuild = Level.BuildAccess.CheckAllowed(this);
                SendMapMotd();
                selections.Clear();
                Session.SendLevel(prev, level);
                Loading = false;
                OnSentMapEvent.Call(this, prev, level);
            }
            catch (Exception ex)
            {
                success = false;
                PlayerActions.ChangeMap(this, Server.mainLevel);
                Message("&WThere was an error sending the map, you have been sent to the main level.");
                Logger.LogError(ex);
            }
            finally
            {
                Server.DoGC();
            }
            return success;
        }
        /// <summary> Like SendPosition, but immediately updates the player's server-side position and orientation. </summary>
        public void SendAndSetPos(Position pos, Orientation rot)
        {
            Pos = pos;
            SetYawPitch(rot.RotY, rot.HeadX);
            Session.SendTeleport(0xFF, pos, rot);
        }
        /// <summary> Sends a packet indicating an absolute position + orientation change for this player. </summary>
        public void SendPosition(Position pos, Orientation rot)
        {
            if (!Session.SendTeleport(0xFF, pos, rot, TeleportMoveMode.AbsoluteInstant))
                Session.SendTeleport(0xFF, pos, rot);
            if (frozen || Session.Ping.IgnorePosition) Pos = pos;
        }
        public void SendBlockchange(ushort x, ushort y, ushort z, ushort block)
        {
            if (x >= Level.Width || y >= Level.Height || z >= Level.Length) return;
            Session.SendBlockchange(x, y, z, block);
        }
        /// <summary> Whether this player's client supports the given CPE extension at the given version </summary>
        public bool Supports(string extName, int version = 1) => Session != null && Session.Supports(extName, version);
        public string GetTextureUrl()
        {
            string url = Level.Config.TexturePack.Length == 0 ? Level.Config.Terrain : Level.Config.TexturePack;
            if (url.Length == 0)
                url = Server.Config.DefaultTexture.Length == 0 ? Server.Config.DefaultTerrain : Server.Config.DefaultTexture;
            return url;
        }
        public void SendCurrentTextures()
        {
            Zone zone = ZoneIn;
            int cloudsHeight = CurrentEnvProp(EnvProp.CloudsLevel, zone),
                edgeHeight = CurrentEnvProp(EnvProp.EdgeLevel, zone),
                maxFogDist = CurrentEnvProp(EnvProp.MaxFog, zone);
            byte side = (byte)CurrentEnvProp(EnvProp.SidesBlock, zone),
                edge = (byte)CurrentEnvProp(EnvProp.EdgeBlock, zone);
            string url = GetTextureUrl();
            if (Supports(CpeExt.EnvMapAspect, 2))
            {
                if (url != lastUrl) Send(Packet.EnvMapUrlV2("", hasCP437));
                Send(Packet.EnvMapUrlV2(url, hasCP437));
            }
            else if (Supports(CpeExt.EnvMapAspect))
            {
                if (url != lastUrl) Send(Packet.EnvMapUrl("", hasCP437));
                Send(Packet.EnvMapUrl(url, hasCP437));
            }
            else if (Supports(CpeExt.EnvMapAppearance, 2))
            {
                if (url != lastUrl)
                    Send(Packet.MapAppearanceV2("", side, edge, edgeHeight, cloudsHeight, maxFogDist, hasCP437));
                Send(Packet.MapAppearanceV2(url, side, edge, edgeHeight, cloudsHeight, maxFogDist, hasCP437));
                lastUrl = url;
            }
            else if (Supports(CpeExt.EnvMapAppearance))
            {
                url = Level.Config.Terrain.Length == 0 ? Server.Config.DefaultTerrain : Level.Config.Terrain;
                Send(Packet.MapAppearance(url, side, edge, edgeHeight, hasCP437));
            }
        }
        public void SendCurrentBlockPermissions()
        {
            if (Supports(CpeExt.BlockPermissions)) SendAllBlockPermissions();
        }
        void SendAllBlockPermissions()
        {
            bool extBlocks = Session.hasExtBlocks;
            int count = Session.MaxRawBlock + 1,
                size = extBlocks ? 5 : 4;
            byte[] bulk = new byte[count * size];
            for (int i = 0; i < count; i++)
            {
                ushort block = Block.FromRaw((ushort)i);
                bool place = group.CanPlace[block] && Level.Config.Buildable,
                    delete = group.CanDelete[block] && (Level.Config.Deletable || i == Block.Air);
                if (block == 0) place &= delete;
                Packet.WriteBlockPermission((ushort)i, place, delete, extBlocks, bulk, i * size);
            }
            Send(bulk);
        }
        public bool AddVisibleSelection(string label, Vec3U16 min, Vec3U16 max, ColorDesc color, object instance)
        {
            lock (selections.locker)
                return Session.SendAddSelection(FindOrAddSelection(selections.Items, instance), label, min, max, color);
        }
        public bool RemoveVisibleSelection(object instance)
        {
            lock (selections.locker)
            {
                VisibleSelection[] items = selections.Items;
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].data != instance) continue;
                    selections.Remove(items[i]);
                    return Session.SendRemoveSelection(items[i].ID);
                }
            }
            return false;
        }
        unsafe byte FindOrAddSelection(VisibleSelection[] items, object instance)
        {
            byte* used = stackalloc byte[256];
            for (int i = 0; i < 256; i++) used[i] = 0;
            byte id;
            for (int i = 0; i < items.Length; i++)
            {
                id = items[i].ID;
                if (instance == items[i].data) return id;
                used[id] = 1;
            }
            for (id = 0; id < 255; id++)
                if (used[id] == 0) break;
            selections.Add(new()
            {
                data = instance,
                ID = id
            });
            return id;
        }
    }
}
