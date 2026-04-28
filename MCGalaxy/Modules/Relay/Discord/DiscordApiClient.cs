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
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
namespace MCGalaxy.Modules.Relay.Discord
{
    /// <summary> Implements a basic web client for sending messages to the Discord API </summary>
    /// <remarks> https://discord.com/developers/docs/reference </remarks>
    /// <remarks> https://discord.com/developers/docs/resources/channel#create-message </remarks>
    public class DiscordApiClient : AsyncWorker<DiscordApiMessage>
    {
        public string Token, Host;
        public DiscordApiMessage GetNextRequest()
        {
            if (queue.Count != 0)
            {
                DiscordApiMessage first = queue.Dequeue();
                while (queue.Count > 0)
                {
                    DiscordApiMessage next = queue.Peek();
                    if (!next.CombineWith(first)) break;
                    queue.Dequeue();
                }
                return first;
            }
            return null;
        }
        public override string ThreadName => "Discord-ApiClient";
        public override void HandleNext()
        {
            DiscordApiMessage msg = null;
            WebResponse res = null;
            lock (queueLock)
                msg = GetNextRequest();
            if (msg == null)
            { 
                WaitForWork(); 
                return;
            }
            for (int retry = 0; retry < 10; retry++)
                try
                {
                    HttpWebRequest req = HttpUtil.CreateRequest(Host + msg.Path);
                    req.Method = msg.Method;
                    req.Headers[HttpRequestHeader.Authorization] = "Bot " + Token;
                    JsonObject obj = msg.ToJson();
                    if (obj != null)
                    {
                        req.ContentType = "application/json";
                        HttpUtil.SetRequestData(req, Encoding.UTF8.GetBytes(Json.SerialiseObject(obj)));
                    }
                    msg.OnRequest(req);
                    res = req.GetResponse();
                    msg.ProcessResponse(HttpUtil.GetResponseText(res));
                    break;
                }
                catch (WebException ex)
                {
                    bool canRetry = HandleErrorResponse(ex, msg, retry);
                    HttpUtil.DisposeErrorResponse(ex);
                    if (!canRetry) return;
                }
                catch (Exception ex)
                {
                    LogError(ex, msg);
                    return;
                }
            if (res.Headers["X-RateLimit-Remaining"] == "1") SleepForRetryPeriod(res);
        }
        public static bool HandleErrorResponse(WebException ex, DiscordApiMessage msg, int retry)
        {
            string err = HttpUtil.GetErrorResponse(ex);
            HttpStatusCode status = GetStatus(ex);
            if (status == (HttpStatusCode)429)
            {
                SleepForRetryPeriod(ex.Response);
                return true;
            }
            if (status >= HttpStatusCode.InternalServerError && status <= HttpStatusCode.GatewayTimeout)
            {
                LogWarning(ex);
                LogResponse(err);
                return retry < 2;
            }
            if (ex.Status == WebExceptionStatus.NameResolutionFailure)
            {
                LogWarning(ex);
                return false;
            }
            if (ex.InnerException is IOException)
            {
                LogWarning(ex);
                return retry < 2;
            }
            LogError(ex, msg);
            LogResponse(err);
            return false;
        }
        public static HttpStatusCode GetStatus(WebException ex) => ex.Response == null ? 0 : ((HttpWebResponse)ex.Response).StatusCode;
        public static void LogError(Exception ex, DiscordApiMessage msg) => Logger.LogError("Error sending request to Discord API (" + msg.Method + " " + msg.Path + ")", ex);
        public static void LogWarning(Exception ex) => Logger.Log(LogType.Warning, "Error sending request to Discord API - " + ex.Message);
        public static void LogResponse(string err)
        {
            if (!string.IsNullOrEmpty(err))
            {
                if (err.Length > 200) err = err.Substring(0, 200) + "...";
                Logger.Log(LogType.Warning, "Discord API returned: " + err);
            }
        }
        public static void SleepForRetryPeriod(WebResponse res)
        {
            if ((!NumberUtils.TryParseSingle(res.Headers["X-RateLimit-Reset-After"], out float delay) || delay <= 0) && (!NumberUtils.TryParseSingle(res.Headers["Retry-After"], out delay) || delay <= 0))
                delay = 30;
            Logger.Log(LogType.SystemActivity, "Discord bot ratelimited! Trying again in {0} seconds..", delay);
            Thread.Sleep(TimeSpan.FromSeconds(delay + 0.5f));
        }
    }
}