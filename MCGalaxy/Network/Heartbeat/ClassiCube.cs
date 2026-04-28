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
using MCGalaxy.Config;
using MCGalaxy.Events.ServerEvents;
using System;
using System.Net;
namespace MCGalaxy.Network
{
    /// <summary> Heartbeat to ClassiCube.net's web server. </summary>
    public class ClassiCubeBeat : Heartbeat
    {
        public string proxyUrl, LastResponse;
        public bool checkedAddr;
        public void CheckAddress()
        {
            string hostUrl = "";
            checkedAddr = true;
            try
            {
                hostUrl = GetHost();
                proxyUrl = EnsureIPv4Url(hostUrl);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error retrieving DNS information for " + hostUrl, ex);
            }
            hostUrl = hostUrl.Replace("www.", "");
            Logger.Log(LogType.SystemActivity, "Finding " + hostUrl + " url..");
        }
        public override string GetHeartbeatData()
        {
            string name = Server.Config.Name;
            OnSendingHeartbeatEvent.Call(this, ref name);
            name = Colors.StripUsed(name);
            return
                "&port=" + Server.Config.Port +
                "&max=" + Server.Config.MaxPlayers +
                "&name=" + Uri.EscapeDataString(name) +
                "&public=" + Server.Config.Public +
                "&version=7" +
                "&salt=" + Auth.Salt +
                "&users=" + PlayerInfo.NonHiddenUniqueIPCount() +
                "&software=" + Uri.EscapeDataString(Server.SoftwareNameVersioned) +
                "&web=" + Server.Config.WebClient;
        }
        public override void OnRequest(HttpWebRequest request)
        {
            if (!checkedAddr) CheckAddress();
            if (proxyUrl != null)
                request.Proxy = new WebProxy(proxyUrl);
        }
        public override void OnResponse(WebResponse response)
        {
            string text = HttpUtil.GetResponseText(response);
            if (NeedsProcessing(text))
            {
                if (!text.Contains("\"errors\":"))
                    OnSuccess(text);
                else
                {
                    string error = GetError(text) ?? "Error while finding URL. Is the port open?";
                    OnError(error);
                }
            }
        }
        public override void OnFailure(string response)
        {
            if (NeedsProcessing(response)) OnError(response);
        }
        public bool NeedsProcessing(string text)
        {
            if (string.IsNullOrEmpty(text) || text == LastResponse) return false;
            LastResponse = text;
            return true;
        }
        public static void OnSuccess(string text)
        {
            text = Truncate(text);
            FileIO.TryWriteAllText("text/externalurl.txt", text);
            Logger.Log(LogType.SystemActivity, "Server URL found: " + text);
        }
        public static void OnError(string error) => Logger.Log(LogType.Warning, Truncate(error));
        public static string GetError(string json)
        {
            JsonReader reader = new(json);
            if (reader.Parse() is not JsonObject obj || !obj.ContainsKey("errors") || obj["errors"] is not JsonArray errors) return null;
            foreach (object raw in errors)
                if (raw is JsonArray err && err.Count > 0) return (string)err[0];
            return null;
        }
        public static string Truncate(string text) => text.Length < 256 ? text : text.Substring(0, 256) + "..";
    }
}