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
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace MCGalaxy
{
    public static class FileLogger
    {
        public static bool disposed;
        public static DateTime last;
        public static readonly object logLock = new();
        public static readonly FileLogGroup err = new(), msg = new();
        public static SchedulerTask logTask;
        public static void Init()
        {
            Server.EnsureDirectoryExists("logs");
            Server.EnsureDirectoryExists("logs/errors");
            UpdatePaths();
            Logger.LogHandler += LogMessage;
            logTask = Server.MainScheduler.QueueRepeat(Flush, null,
                                                       TimeSpan.FromMilliseconds(500));
        }
        public static void UpdatePaths()
        {
            DateTime now = DateTime.Now;
            if (now.Year == last.Year && now.Month == last.Month && now.Day == last.Day)
                return;
            last = now;
            msg.Path = "logs/" + now.ToString("yyyy-MM-dd") + ".txt";
            err.Path = "logs/errors/" + now.ToString("yyyy-MM-dd") + ".txt";
            err.Close();
            msg.Close();
        }
        public static void LogMessage(LogType type, string message)
        {
            if (!string.IsNullOrEmpty(message) && Server.Config.FileLogging[(int)type])
            {
                if (type == LogType.Error)
                {
                    StringBuilder sb = new();
                    sb.AppendLine("----" + DateTime.Now + " ----");
                    sb.AppendLine(message);
                    sb.Append('-', 25);
                    string output = sb.ToString();
                    lock (logLock)
                        err.Cache.Enqueue(output);
                    message = "!!!Error! See " + err.Path + " for more information.";
                }
                string now = DateTime.Now.ToString("(HH:mm:ss) ");
                lock (logLock)
                    msg.Cache.Enqueue(now + message);
            }
        }
        public static void Flush(SchedulerTask task)
        {
            lock (logLock)
            {
                int errsCount = err.Cache.Count,
                    msgsCount = msg.Cache.Count;
                if (errsCount > 0 || msgsCount > 0)
                    UpdatePaths();
                if (errsCount > 0)
                    err.FlushCache();
                if (msgsCount > 0)
                    msg.FlushCache();
            }
        }
        public static void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                Server.MainScheduler.Cancel(logTask);
                lock (logLock)
                {
                    if (err.Cache.Count > 0)
                        err.FlushCache();
                    msg.Cache.Clear();
                }
            }
        }
    }
    public class FileLogGroup
    {
        public string Path;
        public Queue<string> Cache = new();
        public Stream stream;
        public StreamWriter writer;
        public void FlushCache()
        {
            if (stream == null)
            {
                stream = new FileStream(Path, FileMode.Append, FileAccess.Write,
                                        FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
                writer = new(stream);
            }
            try
            {
                if (stream.Length > 1073741824)
                {
                    Cache.Clear();
                    return;
                }
                while (Cache.Count > 0)
                    writer.WriteLine(Colors.Strip(Cache.Dequeue()));
                writer.Flush();
            }
            catch
            {
                Close();
                throw;
            }
        }
        public void Close()
        {
            if (stream != null)
            {
                stream.Dispose();
                stream = null;
                writer = null;
            }
        }
    }
}