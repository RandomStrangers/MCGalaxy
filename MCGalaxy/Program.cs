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
using System.IO;
using System.Reflection;
using System.Threading;
namespace MCGalaxy
{
    public static class Program
    {
        static string lastCMD = "";
        static void HandleChat(string text)
        {
            if (text != null)
            {
                text = text.Trim();
            }
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            if (ChatModes.Handle(Player.Console, text))
            {
                return;
            }
            Chat.MessageChat(ChatScope.Global, Player.Console, "λFULL: &f" + text, null, null, true);
        }
        static void RepeatCommand()
        {
            if (lastCMD.Length == 0)
            {
                Logger.Log(LogType.CommandUsage, "(console): Cannot repeat command - no commands used yet.");
                return;
            }
            Logger.Log(LogType.CommandUsage, "Repeating &T/" + lastCMD);
            HandleCommand(lastCMD);
        }
        static void HandleCommand(string text)
        {
            if (text != null)
            {
                text = text.Trim();
            }
            if (string.IsNullOrEmpty(text))
            {
                Logger.Log(LogType.CommandUsage, "(console): Whitespace commands are not allowed.");
                return;
            }
            if (text[0] == '/' && text.Length > 1)
            {
                text = text.Substring(1);
            }
            lastCMD = text;
            text.Separate(' ', out string name, out string args);
            if (name.CaselessEq("exit"))
            {
                Environment.Exit(0);
            }
            Command.Search(ref name, ref args);
            Command cmd = Command.Find(name);
            if (cmd == null)
            {
                Logger.Log(LogType.CommandUsage, "(console): Unknown command \"{0}\"", name);
                return;
            }
            if (!cmd.SuperUseable)
            {
                Logger.Log(LogType.CommandUsage, "(console): /{0} can only be used in-game.", cmd.Name);
                return;
            }
            Utils.StartBackgroundThread("ConsoleCMD_" + name,
                () =>
                {
                    try
                    {
                        cmd.Use(Player.Console, args);
                        if (args.Length == 0)
                        {
                            Logger.Log(LogType.CommandUsage, "(console) used /" + cmd.Name);
                        }
                        else
                        {
                            Logger.Log(LogType.CommandUsage, "(console) used /" + cmd.Name + " " + args);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);
                        Logger.Log(LogType.CommandUsage, "(console): FAILED COMMAND");
                    }
                });
        }
        static string OutputPart(ref char nextCol, ref int start, string message)
        {
            int next = NextPart(start, message);
            string part;
            if (next == -1)
            {
                part = message.Substring(start);
                start = message.Length;
            }
            else
            {
                part = message.Substring(start, next - start);
                start = next + 2;
                nextCol = message[next + 1];
            }
            return part;
        }
        static int NextPart(int start, string message)
        {
            for (int i = start; i < message.Length; i++)
            {
                if (message[i] != '&')
                {
                    continue;
                }
                if (i == message.Length - 1)
                {
                    return -1;
                }
                char col = Colors.Lookup(message[i + 1]);
                if (col != '\0')
                {
                    return i;
                }
            }
            return -1;
        }
        [STAThread]
        public static void Main()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                Environment.CurrentDirectory = path;
            }
            catch
            {
                Console.Out.WriteLine("Failed to set working directory to '{0}', running in current directory..", path);
            }
            FileLogger.Init();
            AppDomain.CurrentDomain.UnhandledException += GlobalExHandler;
            try
            {
                Logger.LogHandler += LogMessage;
                Server.Start();
                Console.Title = Server.Config.Name + " - " + Server.SoftwareNameVersioned;
                Console.CancelKeyPress += OnCancelKeyPress;
                CheckNameVerification();
                ConsoleLoop();
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                FileLogger.Flush(null);
            }
        }
        static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            switch (e.SpecialKey)
            {
                case ConsoleSpecialKey.ControlBreak:
                    Write("&e-- Server shutdown (Ctrl+Break) --");
                    Thread stopThread = Server.Stop(false, Server.Config.DefaultShutdownMessage);
                    stopThread.Join();
                    break;
                case ConsoleSpecialKey.ControlC:
                    e.Cancel = true;
                    Write("&e-- Server shutdown (Ctrl+C) --");
                    Server.Stop(false, Server.Config.DefaultShutdownMessage);
                    break;
            }
        }
        static void LogAndRestart(Exception ex)
        {
            Logger.LogError(ex);
            FileLogger.Flush(null);
            Thread.Sleep(500);
            if (Server.Config.restartOnError)
            {
                Thread stopThread = Server.Stop(true, "Server restart - unhandled error");
                stopThread.Join();
            }
        }
        static void GlobalExHandler(object sender, UnhandledExceptionEventArgs e) => LogAndRestart((Exception)e.ExceptionObject);
        static void LogMessage(LogType type, string message)
        {
            if (Server.Config.ConsoleLogging[(int)type])
            {
                switch (type)
                {
                    case LogType.Error:
                        Write("&c!!!Error" + ExtractErrorMessage(message)
                              + " - See " + FileLogger.err.Path + " for more details.");
                        break;
                    case LogType.BackgroundActivity:
                        break;
                    case LogType.Warning:
                        Write("&e" + DateTime.Now.ToString("(HH:mm:ss) ") + message);
                        break;
                    default:
                        Write(DateTime.Now.ToString("(HH:mm:ss) ") + message);
                        break;
                }
            }
        }
        static string ExtractErrorMessage(string raw)
        {
            int beg = raw.IndexOf(Environment.NewLine + "Message: ");
            if (beg == -1)
            {
                return "";
            }
            beg += (Environment.NewLine + "Message: ").Length;
            int end = raw.IndexOf(Environment.NewLine, beg);
            return end == -1 ? "" : " (" + raw.Substring(beg, end - beg) + ")";
        }
        static void CheckNameVerification()
        {
            if (!Server.Config.VerifyNames)
            {
                Write("&e==============================================");
                Write("&eWARNING: Name verification is disabled! This means players can login as anyone, including YOU");
                Write("&eUnless you know EXACTLY what you are doing, you should change verify-names to true in server.properties");
                Write("&e==============================================");
            }
        }
        static void ConsoleLoop()
        {
            int eofs = 0;
            while (true)
            {
                try
                {
                    string msg = Console.ReadLine();
                    if (msg == null)
                    {
                        eofs++;
                        if (eofs >= 15)
                        {
                            Write("&e** EOF, console no longer accepts input **");
                            break;
                        }
                        continue;
                    }
                    msg = msg.Trim();
                    if (msg == "/")
                    {
                        RepeatCommand();
                    }
                    else if (msg.Length > 0 && msg[0] == '/')
                    {
                        HandleCommand(msg.Substring(1));
                    }
                    else
                    {
                        HandleChat(msg);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Write("&e** Access denied to stdin, console no longer accepts input **");
                    Write("&e** If nohup is being used, remove that to avoid this issue **");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                }
            }
        }
        static void Write(string message)
        {
            int index = 0;
            char col = 'S';
            message = Colors.Escape(message.Replace("%S", "&f"));
            while (index < message.Length)
            {
                char curCol = col;
                string part = OutputPart(ref col, ref index, message);
                if (part.Length == 0)
                {
                    continue;
                }
                ConsoleColor color = GetConsoleColor(curCol);
                if (color == ConsoleColor.White)
                {
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = color;
                }
                Console.Out.Write(part);
            }
            Console.ResetColor();
            Console.Out.WriteLine();
        }
        static ConsoleColor GetConsoleColor(char c)
        {
            if (c == 'S')
            {
                return ConsoleColor.White;
            }
            Colors.Map(ref c);
            switch (c)
            {
                case '0':
                    return ConsoleColor.DarkGray;
                case '1':
                    return ConsoleColor.DarkBlue;
                case '2':
                    return ConsoleColor.DarkGreen;
                case '3':
                    return ConsoleColor.DarkCyan;
                case '4':
                    return ConsoleColor.DarkRed;
                case '5':
                    return ConsoleColor.DarkMagenta;
                case '6':
                    return ConsoleColor.DarkYellow;
                case '7':
                    return ConsoleColor.Gray;
                case '8':
                    return ConsoleColor.DarkGray;
                case '9':
                    return ConsoleColor.Blue;
                case 'a':
                    return ConsoleColor.Green;
                case 'b':
                    return ConsoleColor.Cyan;
                case 'c':
                    return ConsoleColor.Red;
                case 'd':
                    return ConsoleColor.Magenta;
                case 'e':
                    return ConsoleColor.Yellow;
                case 'f':
                    return ConsoleColor.White;
                default:
                    if (!Colors.IsDefined(c))
                    {
                        return ConsoleColor.White;
                    }
                    return GetConsoleColor(Colors.Get(c).Fallback);
            }
        }
    }
}
