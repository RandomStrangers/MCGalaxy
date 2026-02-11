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
using System;
using System.Security.Cryptography;
using System.Text;
namespace MCGalaxy.Network
{
    /// <summary> Abstracts WebSocket handling </summary>
    /// <remarks> See RFC 6455 for websocket specification </remarks>
    public abstract class BaseWebSocket : INetSocket, INetProtocol
    {
        protected bool conn, upgrade, readingHeaders = true;
        /// <summary> Computes a base64-encoded handshake verification key </summary>
        protected static string ComputeKey(string rawKey) => Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(rawKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
        protected abstract void OnGotAllHeaders();
        protected abstract void OnGotHeader(string name, string value);
        void ProcessHeader(string raw)
        {
            // end of all headers
            if (raw.Length == 0) OnGotAllHeaders();
            // check that got a proper header
            int sep = raw.IndexOf(':');
            if (sep == -1) return;
            string name = raw.Substring(0, sep),
                value = raw.Substring(sep + 1).Trim();
            // RFC 6455, section 1.3 - Opening Handshake
            //   To this end, the WebSocket client's handshake is an HTTP Upgrade request
            if (name.CaselessEq("Connection"))
            {
                conn = value.CaselessContains("Upgrade");
            }
            else if (name.CaselessEq("Upgrade"))
            {
                upgrade = value.CaselessEq("websocket");
            }
            else
            {
                OnGotHeader(name, value);
            }
        }
        int ReadHeaders(byte[] buffer, int bufferLen)
        {
            int i;
            for (i = 0; i < bufferLen - 1;)
            {
                int end = -1;
                // find end of header
                for (int j = i; j < bufferLen - 1; j++)
                {
                    if (buffer[j] != '\r' || buffer[j + 1] != '\n') continue;
                    end = j; 
                    break;
                }
                if (end == -1) break;
                string value = Encoding.ASCII.GetString(buffer, i, end - i);
                ProcessHeader(value);
                i = end + 2;
            }
            return i;
        }
        int state, opcode, frameLen, maskRead, frameRead;
        private readonly byte[] mask = new byte[4];
        private byte[] frame;
        int GetDisconnectReason()
        {
            if (frameLen < 2) return 1000;
            // RFC 6455, section 5.5.1 - Close
            //   If there is a body, the first two bytes of the body MUST
            //    be a 2-byte unsigned integer (in network byte order)...
            return (frame[0] << 8) | frame[1];
        }
        void DecodeFrame()
        {
            for (int i = 0; i < frameLen; i++)
            {
                frame[i] ^= mask[i & 3];
            }
            switch (opcode)
            {
                // TODO: reply to ping frames
                case 0:
                case 2:
                case 1:
                    if (frameLen == 0) return;
                    HandleData(frame, frameLen);
                    break;
                case 8:
                    // Connection is getting closed
                    Disconnect(GetDisconnectReason()); 
                    break;
                default:
                    Disconnect(1003); 
                    break;
            }
        }
        int ProcessData(byte[] data, int offset, int len)
        {
            switch (state)
            {
                case 0:
                    if (offset >= len) break;
                    opcode = data[offset++] & 0x0F;
                    state = 1;
                    goto case 1;
                case 1:
                    if (offset >= len) break;
                    int flags = data[offset] & 0x7F;
                    // if mask bit is     zero: maskRead is set to 0x80 (therefore skipping reading the 4 bytes)
                    // if mask bit is non-zero: maskRead is set to 0x00 (therefore actually reading the data)
                    maskRead = 0x80 - (data[offset] & 0x80);
                    offset++;
                    if (flags == 127)
                    {
                        // unsupported 8 byte extended length
                        Disconnect(1009);
                        return len;
                    }
                    else if (flags == 126)
                    {
                        // two byte extended length
                        state = 2;
                        goto case 2;
                    }
                    else
                    {
                        // length is inline
                        frameLen = flags;
                        state = 4;
                        goto case 4;
                    }
                case 2:
                    if (offset >= len) break;
                    frameLen = data[offset++] << 8;
                    state = 3;
                    goto case 3;
                case 3:
                    if (offset >= len) break;
                    frameLen |= data[offset++];
                    state = 4;
                    goto case 4;
                case 4:
                    for (; maskRead < 4; maskRead++)
                    {
                        if (offset >= len) return offset;
                        mask[maskRead] = data[offset++];
                    }
                    maskRead = 0;
                    state = 5;
                    goto case 5;
                case 5:
                    if (frame == null || frameLen > frame.Length) frame = new byte[frameLen];
                    int copy = Math.Min(len - offset, frameLen - frameRead);
                    Buffer.BlockCopy(data, offset, frame, frameRead, copy);
                    offset += copy; 
                    frameRead += copy;
                    if (frameRead == frameLen)
                    {
                        DecodeFrame();
                        frameRead = 0;
                        state = 0;
                    }
                    break;
            }
            return offset;
        }
        int INetProtocol.ProcessReceived(byte[] buffer, int bufferLen)
        {
            int offset = 0;
            if (readingHeaders)
            {
                offset = ReadHeaders(buffer, bufferLen);
                if (readingHeaders) return offset;
            }
            while (offset < bufferLen)
            {
                offset = ProcessData(buffer, offset, bufferLen);
            }
            return offset;
        }
        protected static byte[] WrapDisconnect(int reason)
        {
            byte[] packet = new byte[4];
            packet[0] = 8 | 0x80;
            packet[1] = 2;
            packet[2] = (byte)(reason >> 8);
            packet[3] = (byte)reason;
            return packet;
        }
        public void Disconnect() => Disconnect(1000);
        protected void Disconnect(int reason)
        {
            try
            {
                SendRaw(WrapDisconnect(reason), SendFlags.Synchronous);
            }
            catch
            {
            }
            OnDisconnected(reason);
        }
        protected abstract void OnDisconnected(int reason);
        protected abstract void HandleData(byte[] data, int len);
        protected abstract void SendRaw(byte[] data, SendFlags flags);
    }
    public abstract class ServerWebSocket : BaseWebSocket
    {
        bool version;
        string verKey;
        void AcceptConnection()
        {
            SendRaw(Encoding.ASCII.GetBytes(string.Format("HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                "Sec-WebSocket-Accept: {0}\r\n" +
                "Sec-WebSocket-Protocol: ClassiCube\r\n" +
                "\r\n", ComputeKey(verKey))), SendFlags.None);
            readingHeaders = false;
        }
        protected override void OnGotAllHeaders()
        {
            if (conn && upgrade && version && verKey != null)
            {
                AcceptConnection();
            }
            else
            {
                Close();
            }
        }
        protected override void OnGotHeader(string name, string value)
        {
            if (name.CaselessEq("Sec-WebSocket-Version"))
            {
                version = value.CaselessEq("13");
            }
            else if (name.CaselessEq("Sec-WebSocket-Key"))
            {
                verKey = value;
            }
        }
        protected static byte[] WrapData(byte[] data)
        {
            int headerLen = data.Length >= 126 ? 4 : 2;
            byte[] packet = new byte[headerLen + data.Length];
            packet[0] = 2 | 0x80;
            if (headerLen > 2)
            {
                packet[1] = 126;
                packet[2] = (byte)(data.Length >> 8);
                packet[3] = (byte)data.Length;
            }
            else
            {
                packet[1] = (byte)data.Length;
            }
            Buffer.BlockCopy(data, 0, packet, headerLen, data.Length);
            return packet;
        }
    }
    public abstract class ClientWebSocket : BaseWebSocket
    {
        protected string path = "/";
        string verKey;
        void AcceptConnection() => readingHeaders = false;
        protected override void OnGotAllHeaders()
        {
            if (conn && upgrade && verKey == ComputeKey("xTNDiuZRoMKtxrnJDWyLmA=="))
            {
                AcceptConnection();
            }
            else
            {
                Close();
            }
        }
        protected override void OnGotHeader(string name, string value)
        {
            if (name.CaselessEq("Sec-WebSocket-Accept"))
            {
                verKey = value;
            }
        }
        protected static byte[] WrapData(byte[] data)
        {
            int headerLen = data.Length >= 126 ? 4 : 2;
            byte[] packet = new byte[headerLen + 4 + data.Length];
            packet[0] = 1 | 0x80;
            if (headerLen > 2)
            {
                packet[1] = 126;
                packet[2] = (byte)(data.Length >> 8);
                packet[3] = (byte)data.Length;
            }
            else
            {
                packet[1] = (byte)data.Length;
            }
            packet[1] |= 0x80;
            Buffer.BlockCopy(data, 0, packet, headerLen + 4, data.Length);
            return packet;
        }
        public override void Send(byte[] buffer, SendFlags flags) => SendRaw(WrapData(buffer), flags);
        protected void WriteHeader(string header) => SendRaw(Encoding.ASCII.GetBytes(header + "\r\n"), SendFlags.None);
        protected virtual void WriteCustomHeaders() { }
        public override void Init()
        {
            WriteHeader("GET " + path + " HTTP/1.1");
            WriteHeader("Upgrade: websocket");
            WriteHeader("Connection: Upgrade");
            WriteHeader("Sec-WebSocket-Version: 13");
            WriteHeader("Sec-WebSocket-Key: xTNDiuZRoMKtxrnJDWyLmA==");
            WriteCustomHeaders();
            WriteHeader("");
        }
    }
}
