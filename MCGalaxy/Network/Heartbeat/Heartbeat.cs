/*
    Copyright 2012 MCForge
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
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Cache;
using System.Net.Sockets;
using System.Text;
namespace MCGalaxy.Network
{
    /// <summary> Repeatedly sends a heartbeat request every certain interval to a web server. </summary>
    public abstract class Heartbeat
    {
        /// <summary> List of all heartbeats to pump </summary>
        public static List<Heartbeat> Heartbeats = new();
        /// <summary> The URL this heartbeat is sent to </summary
        public string URL;
        /// <summary> Authentication service potentially associated with the heartbeat </summary>
        /// <example> ClassiCube beats use the Salt of the service for name authentication </example>
        public AuthService Auth;
        public string GetHost()
        {
            try
            {
                return new Uri(URL).Host;
            }
            catch (Exception ex)
            {
                Logger.LogError("Getting host of " + URL, ex);
                return URL;
            }
        }
        /// <summary> Gets the data to be sent for the next heartbeat </summary>
        protected abstract string GetHeartbeatData();
        /// <summary> Called when a heartbeat is about to be sent to the web server </summary>
        protected abstract void OnRequest(HttpWebRequest request);
        /// <summary> Called when a response is received from the web server </summary>
        protected abstract void OnResponse(WebResponse response);
        /// <summary> Called when a failure HTTP response is received from the web server </summary>
        protected abstract void OnFailure(string response);
        /// <summary> Sends a heartbeat to the web server and then reads the response </summary>
        public void Pump()
        {
            byte[] data = Encoding.ASCII.GetBytes(GetHeartbeatData());
            Exception lastEx = null;
            string lastResp = null;
            for (int i = 0; i < 3; i++)
                try
                {
                    HttpWebRequest req = HttpUtil.CreateRequest(URL);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                    req.Timeout = 10000;
                    OnRequest(req);
                    HttpUtil.SetRequestData(req, data);
                    WebResponse res = req.GetResponse();
                    OnResponse(res);
                    return;
                }
                catch (Exception ex)
                {
                    lastResp = HttpUtil.GetErrorResponse(ex);
                    HttpUtil.DisposeErrorResponse(ex);
                    lastEx = ex;
                    continue;
                }
            OnFailure(lastResp);
            Logger.Log(LogType.Warning, "Failed to send heartbeat to {0} ({1})", GetHost(), lastEx.Message);
        }
        /// <summary> Adds the given heartbeat to the list of automatically pumped heartbeats </summary>
        public static void Register(Heartbeat beat) => Heartbeats.Add(beat);
        /// <summary> Starts pumping heartbeats </summary>
        public static void Start()
        {
            OnBeat(null);
            Server.Heartbeats.QueueRepeat(OnBeat, null, TimeSpan.FromSeconds(30));
        }
        public static void OnBeat(SchedulerTask task)
        {
            if (Server.Listener.Listening)
                foreach (Heartbeat beat in Heartbeats)
                    beat.Pump();
        }
        public static string lastUrls;
        internal static void ReloadDefault()
        {
            string urls = Server.Config.HeartbeatURL;
            if (urls != lastUrls)
            {
                lastUrls = urls;
                Heartbeats.Clear();
                foreach (string url in urls.SplitComma())
                    Register(new ClassiCubeBeat
                    {
                        URL = url,
                        Auth = AuthService.GetOrCreate(url)
                    });
            }
        }
        protected string EnsureIPv4Url(string hostUrl)
        {
            bool hasIPv6 = false;
            IPAddress firstIPv4 = null;
            if (URL.CaselessStarts("https://")) return null;
            IPAddress[] addresses = Dns.GetHostAddresses(hostUrl);
            foreach (IPAddress ip in addresses)
            {
                AddressFamily family = ip.AddressFamily;
                if (family == AddressFamily.InterNetworkV6)
                    hasIPv6 = true;
                if (family == AddressFamily.InterNetwork && firstIPv4 == null)
                    firstIPv4 = ip;
            }
            return !hasIPv6 || firstIPv4 == null ? null : "http://" + firstIPv4 + ":80";
        }
    }
}
