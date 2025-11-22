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
using MCGalaxy.UI;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
namespace MCGalaxy.Cli
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] _)
        {
            SetCurrentDirectory();
            try
            {
                EnableCLIMode();
            }
            catch (FileNotFoundException ex)
            {
                Console.Out.WriteLine("Cannot start server as {0} is missing from {1}",
                                  GetFilename(ex.FileName), Environment.CurrentDirectory);
                Console.Out.WriteLine("Download from " + Updater.UploadsURL);
                Console.Out.WriteLine("Press any key to exit...");
                Console.ReadKey(true);
                return;
            }
            StartCLI();
        }
        static string GetFilename(string rawName)
        {
            try
            {
                return new AssemblyName(rawName).Name + ".dll";
            }
            catch
            {
                return rawName;
            }
        }
        static void SetCurrentDirectory()
        {
#if !MCG_STANDALONE
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                Environment.CurrentDirectory = path;
            }
            catch
            {
                Console.Out.WriteLine("Failed to set working directory to '{0}', running in current directory..", path);
            }
#endif
        }
        static void EnableCLIMode()
        {
            try
            {
                Server.CLIMode = true;
            }
            catch
            {
            }
#if !MCG_STANDALONE
            Server.RestartPath = Assembly.GetEntryAssembly().Location;
#endif
        }
        static void StartCLI()
        {
            FileLogger.Init();
            AppDomain.CurrentDomain.UnhandledException += GlobalExHandler;
            try
            {
                Logger.LogHandler += LogMessage;
                Updater.NewerVersionDetected += LogNewerVersionDetected;
                EnableCLIMode();
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
        static void GlobalExHandler(object sender, UnhandledExceptionEventArgs e)
        {
            LogAndRestart((Exception)e.ExceptionObject);
        }
        static string CurrentDate() 
        { 
            return DateTime.Now.ToString("(HH:mm:ss) "); 
        }
        static void LogMessage(LogType type, string message)
        {
            if (!Server.Config.ConsoleLogging[(int)type])
            {
                return;
            }
            switch (type)
            {
                case LogType.Error:
                    Write("&c!!!Error" + ExtractErrorMessage(message)
                          + " - See " + FileLogger.err.Path + " for more details.");
                    break;
                case LogType.BackgroundActivity:
                    break;
                case LogType.Warning:
                    Write("&e" + CurrentDate() + message);
                    break;
                default:
                    Write(CurrentDate() + message);
                    break;
            }
        }
        static readonly string msgPrefix = Environment.NewLine + "Message: ";
        static string ExtractErrorMessage(string raw)
        {
            int beg = raw.IndexOf(msgPrefix);
            if (beg == -1)
            {
                return "";
            }
            beg += msgPrefix.Length;
            int end = raw.IndexOf(Environment.NewLine, beg);
            if (end == -1)
            {
                return "";
            }
            return " (" + raw.Substring(beg, end - beg) + ")";
        }
        static void CheckNameVerification()
        {
            if (Server.Config.VerifyNames)
            {
                return;
            }
            Write("&e==============================================");
            Write("&eWARNING: Name verification is disabled! This means players can login as anyone, including YOU");
            Write("&eUnless you know EXACTLY what you are doing, you should change verify-names to true in server.properties");
            Write("&e==============================================");
        }
        static void LogNewerVersionDetected(object sender, EventArgs e)
        {
            Write("&cMCGalaxy update available! Update by replacing with the files from " + Updater.UploadsURL);
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
                        UIHelpers.RepeatCommand();
                    }
                    else if (msg.Length > 0 && msg[0] == '/')
                    {
                        UIHelpers.HandleCommand(msg.Substring(1));
                    }
                    else
                    {
                        UIHelpers.HandleChat(msg);
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
            message = UIHelpers.Format(message);
            while (index < message.Length)
            {
                char curCol = col;
                string part = UIHelpers.OutputPart(ref col, ref index, message);
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