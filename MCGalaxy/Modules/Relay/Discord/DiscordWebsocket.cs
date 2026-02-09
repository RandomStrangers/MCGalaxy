/*
    Copyright 2015-2024 MCGalaxy
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
using MCGalaxy.Config;
using MCGalaxy.Network;
using MCGalaxy.Platform;
using MCGalaxy.Tasks;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace MCGalaxy.Modules.Relay.Discord
{
    public class DiscordSession
    {
        public string ID, LastSeq;
        public int Intents;
    }
    public delegate string DiscordGetStatus();
    public delegate void GatewayEventCallback(string eventName, JsonObject data);
    /// <summary> Implements a basic websocket for communicating with Discord's gateway </summary>
    /// <remarks> https://discord.com/developers/docs/topics/gateway </remarks>
    /// <remarks> https://i.imgur.com/Lwc5Wde.png </remarks>
    public class DiscordWebsocket : ClientWebSocket
    {
        /// <summary> Authorisation token for the bot account </summary>
        public string Token, Host;
        public bool CanReconnect = true, SentIdentify;
        public DiscordSession Session;
        /// <summary> Whether presence support is enabled </summary>
        public bool Presence = true;
        /// <summary> Presence status (E.g. online) </summary>
        public PresenceStatus Status;
        /// <summary> Presence activity (e.g. Playing) </summary>
        public PresenceActivity Activity;
        /// <summary> Callback function to retrieve the activity status message </summary>
        public DiscordGetStatus GetStatus;
        /// <summary> Callback invoked when a ready event has been received </summary>
        public Action<JsonObject> OnReady;
        /// <summary> Callback invoked when a resumed event has been received </summary>
        public Action<JsonObject> OnResumed;
        /// <summary> Callback invoked when a message created event has been received </summary>
        public Action<JsonObject> OnMessageCreate;
        /// <summary> Callback invoked when a channel created event has been received </summary>
        public Action<JsonObject> OnChannelCreate;
        /// <summary> Callback invoked when a gateway event has been received </summary>
        public GatewayEventCallback OnGatewayEvent;
        readonly object sendLock = new();
        SchedulerTask heartbeat;
        TcpClient client;
        SslStream stream;
        bool readable;
        public DiscordWebsocket(string apiPath) => path = apiPath;
        // stubs
        public override bool LowLatency { set { } }
        public override IPAddress IP => null;
        public void Connect()
        {
            client = new();
            client.Connect(Host, 443);
            readable = true;
            stream = HttpUtil.WrapSSLStream(client.GetStream(), Host);
            protocol = this;
            Init();
        }
        protected override void WriteCustomHeaders()
        {
            WriteHeader("Authorization: Bot " + Token);
            WriteHeader("Host: " + Host);
        }
        public override void Close()
        {
            readable = false;
            Server.Heartbeats.Cancel(heartbeat);
            try
            {
                client.Close();
            }
            catch
            {
                // ignore errors when closing socket
            }
        }
        protected override void OnDisconnected(int reason)
        {
            SentIdentify = false;
            if (readable) Logger.Log(LogType.SystemActivity, "Discord relay bot closing: " + reason);
            Close();
            if (reason == 4004)
            {
                CanReconnect = false;
                throw new InvalidOperationException("Discord relay: Invalid bot token provided - unable to connect");
            }
            else if (reason == 4014)
            {
                // privileged intent since August 2022 https://support-dev.discord.com/hc/en-us/articles/4404772028055
                CanReconnect = false;
                throw new InvalidOperationException("Discord relay: Message Content Intent is not enabled in Bot Account settings, " +
                    "therefore Discord will prevent the bot from being able to see the contents of Discord messages\n" +
                    "(See https://github.com/ClassiCube/MCGalaxy/wiki/Discord-relay-bot#read-permissions)");
            }
        }
        public void ReadLoop()
        {
            byte[] data = new byte[4096];
            readable = true;
            while (readable)
            {
                int len = stream.Read(data, 0, 4096);
                if (len == 0) throw new IOException("stream.Read returned 0");
                HandleReceived(data, len);
            }
        }
        protected override void HandleData(byte[] data, int len)
        {
            string value = Encoding.UTF8.GetString(data, 0, len);
            JsonReader ctx = new(value);
            JsonObject obj = (JsonObject)ctx.Parse();
            if (obj == null) return;
            int opcode = NumberUtils.ParseInt32((string)obj["op"]);
            DispatchPacket(opcode, obj);
        }
        void DispatchPacket(int opcode, JsonObject obj)
        {
            if (opcode == 0)
            {
                HandleDispatch(obj);
            }
            else if (opcode == 10)
            {
                HandleHello(obj);
            }
            else if (opcode == 9)
            {
                // See notes at https://discord.com/developers/docs/topics/gateway#resuming
                //  (note that in this implementation, if resume fails, the bot just
                //   gives up altogether instead of trying to resume again later)
                Session.ID = null;
                Session.LastSeq = null;
                Logger.Log(LogType.Warning, "Discord relay: Resuming failed, trying again in 5 seconds");
                Thread.Sleep(5 * 1000);
                Identify();
            }
        }
        void HandleHello(JsonObject obj)
        {
            JsonObject data = (JsonObject)obj["d"];
            string interval = (string)data["heartbeat_interval"];
            int msInterval = NumberUtils.ParseInt32(interval);
            heartbeat = Server.Heartbeats.QueueRepeat(SendHeartbeat, null,
                                          TimeSpan.FromMilliseconds(msInterval));
            Identify();
        }
        void HandleDispatch(JsonObject obj)
        {
            // update last sequence number
            if (obj.TryGetValue("s", out object sequence))
                Session.LastSeq = (string)sequence;
            string eventName = (string)obj["t"];
            obj.TryGetValue("d", out object rawData);
            JsonObject data = rawData as JsonObject;
            if (eventName == "READY")
            {
                HandleReady(data);
                OnReady(data);
            }
            else if (eventName == "RESUMED")
            {
                OnResumed(data);
            }
            else if (eventName == "MESSAGE_CREATE")
            {
                OnMessageCreate(data);
            }
            else if (eventName == "CHANNEL_CREATE")
            {
                OnChannelCreate(data);
            }
            OnGatewayEvent(eventName, data);
        }
        void HandleReady(JsonObject data)
        {
            if (data.TryGetValue("session_id", out object session))
                Session.ID = (string)session;
        }
        public void SendMessage(int opcode, JsonObject data) => SendMessage(new()
            {
                { "op", opcode },
                { "d",  data }
            });
        public void SendMessage(JsonObject obj) => Send(Encoding.UTF8.GetBytes(Json.SerialiseObject(obj)), SendFlags.None);
        protected override void SendRaw(byte[] data, SendFlags flags)
        {
            lock (sendLock) stream.Write(data);
        }
        void SendHeartbeat(SchedulerTask task)
        {
            JsonObject obj = new()
            {
                ["op"] = 1
            };
            if (Session.LastSeq != null)
            {
                obj["d"] = NumberUtils.ParseInt32(Session.LastSeq);
            }
            else
            {
                obj["d"] = null;
            }
            SendMessage(obj);
        }
        public void Identify()
        {
            if (Session.ID != null && Session.LastSeq != null)
            {
                SendMessage(6, MakeResume());
            }
            else
            {
                SendMessage(2, MakeIdentify());
            }
            SentIdentify = true;
        }
        JsonObject MakeResume() => new()
            {
                { "token",      Token },
                { "session_id", Session.ID },
                { "seq",        NumberUtils.ParseInt32(Session.LastSeq) }
            };
        JsonObject MakeIdentify() => new()
            {
                { "token",      Token },
                { "intents",    Session.Intents },
                { "properties", new JsonObject()
                    {
                        { "$os",      IOperatingSystem.Get() },
                        { "$browser", Server.SoftwareName },
                        { "$device",  Server.SoftwareName }
                    }
                },
                { "presence",   MakePresence() }
            };
        public JsonObject MakePresence() => !Presence
                ? null
                : new()
            {
                { "since",      null },
                { "activities", new JsonArray()
                    { new JsonObject()
                        {
                            { "name", GetStatus() },
                            { "type", (int)Activity }
                        }
                    }
                },
                { "status",     Status.ToString() },
                { "afk",        false }
            };
    }
}
