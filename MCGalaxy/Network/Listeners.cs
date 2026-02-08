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
using MCGalaxy.Events.ServerEvents;
using System;
using System.Net;
using System.Net.Sockets;
namespace MCGalaxy.Network
{
    public class INetListen
    {
        public IPAddress IP;
        public int Port;
        public bool Listening;
        Socket socket;
        void DisableIPV6OnlyListener()
        {
            if (socket.AddressFamily != AddressFamily.InterNetworkV6)
            {
                return;
            }
            try
            {
                socket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to disable IPv6 only listener setting", ex);
            }
        }
        void EnableAddressReuse()
        {
            if (Server.RunningOnMono())
            {
                try
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                }
                catch
                {
                }
            }
        }
        public void Listen(IPAddress ip, int port)
        {
            if (IP == ip && Port == port)
            {
                return;
            }
            Close();
            IP = ip;
            Port = port;
            try
            {
                socket = new(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                DisableIPV6OnlyListener();
                EnableAddressReuse();
                socket.Bind(new IPEndPoint(ip, port));
                socket.Listen((int)SocketOptionName.MaxConnections);
                AcceptNextAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                Logger.Log(LogType.Warning, "Failed to start listening on port {0} ({1})", port, ex.Message);
                socket = null;
                return;
            }
            Listening = true;
            Logger.Log(LogType.SystemActivity, "Started listening on port {0}... ", port);
        }
        void AcceptNextAsync()
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    socket.BeginAccept(acceptCallback, this); 
                    return;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
        }
        static readonly AsyncCallback acceptCallback = new(AcceptCallback);
        static void AcceptCallback(IAsyncResult result)
        {
            if (!Server.shuttingDown)
            {
                INetListen listen = (INetListen)result.AsyncState;
                INetSocket s = null;
                try
                {
                    Socket raw = listen.socket.EndAccept(result);
                    bool cancel = false, announce = true;
                    OnConnectionReceivedEvent.Call(raw, ref cancel, ref announce);
                    if (cancel)
                    {
                        try 
                        { 
                            raw.Close(); 
                        } 
                        catch 
                        { 
                        }
                    }
                    else
                    {
                        s = new TcpSocket(raw);
                        if (announce)
                        {
                            Logger.Log(LogType.UserActivity, s.IP + " connected to the server.");
                        }
                        s.Init();
                    }
                }
                catch (Exception ex)
                {
                    if (ex is not SocketException)
                    {
                        Logger.LogError(ex);
                    }
                    s?.Close();
                }
                listen.AcceptNextAsync();
            }
        }
        public void Close()
        {
            try
            {
                Listening = false;
                socket?.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }
    }
}