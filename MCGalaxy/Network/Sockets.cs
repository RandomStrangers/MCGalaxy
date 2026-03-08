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
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
namespace MCGalaxy.Network
{
    public enum SendFlags
    {
        None = 0x00,
        Synchronous = 0x01,
        LowPriority = 0x02,
    }
    public delegate INetProtocol ProtocolConstructor(INetSocket socket);
    public abstract class INetSocket
    {
        public INetProtocol protocol;
        public bool Disconnected;
        byte[] leftData;
        int leftLen;
        public abstract IPAddress IP { get; }
        public abstract bool LowLatency { set; }
        public abstract void Init();
        public abstract void Send(byte[] buffer, SendFlags flags);
        public abstract void Close();
        protected void HandleReceived(byte[] data, int len)
        {
            if (protocol == null)
            {
                IdentifyProtocol(data[0]);
                if (protocol == null)
                    return;
            }
            byte[] src;
            if (leftLen == 0)
            {
                src = data;
                leftLen = len;
            }
            else
            {
                int totalLen = leftLen + len;
                if (totalLen > leftData.Length)
                    Array.Resize(ref leftData, totalLen);
                Buffer.BlockCopy(data, 0, leftData, leftLen, len);
                src = leftData;
                leftLen = totalLen;
            }
            int processedLen = protocol.ProcessReceived(src, leftLen);
            leftLen -= processedLen;
            if (leftLen == 0)
                return;
            if (leftData == null || leftLen > leftData.Length)
                leftData = new byte[leftLen];
            for (int i = 0; i < leftLen; i++)
                leftData[i] = src[processedLen + i];
        }
        internal static VolatileArray<INetSocket> pending = new();
        public static ProtocolConstructor[] Protocols = new ProtocolConstructor[256];
        void IdentifyProtocol(byte opcode)
        {
            ProtocolConstructor cons = Protocols[opcode];
            if (cons != null)
                protocol = cons(this);
            if (protocol != null)
                return;
            Logger.Log(LogType.UserActivity, "Disconnected {0} (unknown opcode {1})", IP, opcode);
            Close();
        }
        static INetSocket()
        {
            Protocols[0] = ConstructClassic;
            Protocols['G'] = ConstructWebsocket;
        }
        static INetProtocol ConstructClassic(INetSocket socket) => new ClassicProtocol(socket);
        static INetProtocol ConstructWebsocket(INetSocket socket) => !Server.Config.WebClient ? null : (INetProtocol)new WebSocket(socket);
    }
    public interface INetProtocol
    {
        int ProcessReceived(byte[] buffer, int length);
        void Disconnect();
    }
    public sealed class TcpSocket : INetSocket
    {
        readonly Socket socket;
        readonly byte[] recvBuffer = new byte[256],
            sendBuffer = new byte[4096];
        readonly SocketAsyncEventArgs recvArgs = new(),
            sendArgs = new();
        readonly object sendLock = new();
        readonly Queue<byte[]> sendQueue = new(64);
        volatile bool sendInProgress;
        public TcpSocket(Socket s)
        {
            socket = s;
            recvArgs.UserToken = this;
            recvArgs.SetBuffer(recvBuffer, 0, recvBuffer.Length);
            sendArgs.UserToken = this;
            sendArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
        }
        public override void Init()
        {
            recvArgs.Completed += recvCallback;
            sendArgs.Completed += sendCallback;
            ReceiveNextAsync();
        }
        public override IPAddress IP => SocketUtil.GetIP(socket);
        public override bool LowLatency
        {
            set 
            { 
                socket.NoDelay = value; 
            } 
        }
        static readonly EventHandler<SocketAsyncEventArgs> recvCallback = RecvCallback;
        void ReceiveNextAsync()
        {
            if (!socket.ReceiveAsync(recvArgs))
                RecvCallback(null, recvArgs);
        }
        static void RecvCallback(object sender, SocketAsyncEventArgs e)
        {
            TcpSocket s = (TcpSocket)e.UserToken;
            if (!s.Disconnected)
            {
                try
                {
                    int recvLen = e.BytesTransferred;
                    if (recvLen == 0) 
                    { 
                        s.Disconnect(); 
                        return;
                    }
                    s.HandleReceived(s.recvBuffer, recvLen);
                    if (!s.Disconnected)
                        s.ReceiveNextAsync();
                }
                catch (SocketException)
                {
                    s.Disconnect();
                }
                catch (ObjectDisposedException)
                {
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    s.Disconnect();
                }
            }
        }
        static readonly EventHandler<SocketAsyncEventArgs> sendCallback = SendCallback;
        public override void Send(byte[] buffer, SendFlags flags)
        {
            if (Disconnected || !socket.Connected)
                return;
            try
            {
                if ((flags & SendFlags.Synchronous) != 0)
                {
                    socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
                    return;
                }
                lock (sendLock)
                {
                    if (sendInProgress)
                        sendQueue.Enqueue(buffer);
                    else
                        sendInProgress = TrySendAsync(buffer);
                }
            }
            catch (SocketException)
            {
                Disconnect();
            }
            catch (ObjectDisposedException)
            {
            }
        }
        bool TrySendAsync(byte[] buffer)
        {
            if (buffer.Length <= 16)
                for (int i = 0; i < buffer.Length; i++)
                    sendBuffer[i] = buffer[i];
            else
                Buffer.BlockCopy(buffer, 0, sendBuffer, 0, buffer.Length);
            sendArgs.SetBuffer(0, buffer.Length);
            return socket.SendAsync(sendArgs);
        }
        static void SendCallback(object sender, SocketAsyncEventArgs e)
        {
            TcpSocket s = (TcpSocket)e.UserToken;
            try
            {
                lock (s.sendLock)
                {
                    for (; ; )
                    {
                        int sent = e.BytesTransferred,
                            count = e.Count;
                        if (sent >= count || sent <= 0)
                            break;
                        s.sendArgs.SetBuffer(e.Offset + sent, e.Count - sent);
                        s.sendInProgress = s.socket.SendAsync(s.sendArgs);
                        if (s.sendInProgress)
                            return;
                    }
                    s.sendInProgress = false;
                    while (s.sendQueue.Count > 0)
                    {
                        s.sendInProgress = s.TrySendAsync(s.sendQueue.Dequeue());
                        if (s.sendInProgress)
                            return;
                        if (s.Disconnected)
                            s.sendQueue.Clear();
                    }
                }
            }
            catch (SocketException)
            {
                s.Disconnect();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
        void Disconnect()
        {
            protocol?.Disconnect();
            Close();
        }
        public override void Close()
        {
            Disconnected = true;
            pending.Remove(this);
            try 
            {
                socket.Shutdown(SocketShutdown.Both); 
            } 
            catch 
            { 
            }
            try 
            { 
                socket.Close(); 
            }
            catch 
            { 
            }
            lock (sendLock)
                sendQueue.Clear();
            try
            { 
                recvArgs.Dispose(); 
            } 
            catch 
            {
            }
            try 
            { 
                sendArgs.Dispose(); 
            }
            catch 
            { 
            }
        }
    }
    public sealed class WebSocket : ServerWebSocket
    {
        readonly INetSocket s;
        IPAddress clientIP;
        public WebSocket(INetSocket socket) => s = socket;
        public override void Init() { }
        public override IPAddress IP => clientIP ?? s.IP;
        public override bool LowLatency 
        {
            set 
            { 
                s.LowLatency = value; 
            } 
        }
        protected override void SendRaw(byte[] data, SendFlags flags) => s.Send(data, flags);
        public override void Send(byte[] buffer, SendFlags flags) => s.Send(WrapData(buffer), flags);
        protected override void HandleData(byte[] data, int len) => HandleReceived(data, len);
        protected override void OnDisconnected(int reason)
        {
            protocol?.Disconnect();
            s.Close();
        }
        public override void Close() => s.Close();
        protected override void OnGotHeader(string name, string value)
        {
            base.OnGotHeader(name, value);
            if (name == "X-Real-IP" && Server.Config.AllowIPForwarding && IsTrustedForwarderIP())
            {
                Logger.Log(LogType.SystemActivity, "{0} is forwarding a connection from {1}", IP, value);
                IPAddress.TryParse(value, out clientIP);
            }
        }
        bool IsTrustedForwarderIP() => IPAddress.IsLoopback(IP) || IP.Equals(new IPAddress(0xFA05DF22));
    }
}