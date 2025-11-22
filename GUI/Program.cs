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
using System.Windows.Forms;
namespace MCGalaxy.Gui
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] _)
        {
            SetCurrentDirectory();
            try
            {
                StartGUI();
            }
            catch (FileNotFoundException)
            {
                Popup.Error("Cannot start server as MCGalaxy_.dll is missing from " + Environment.CurrentDirectory
                            + "\n\nDownload it from " + Updater.UploadsURL);
                return;
            }
        }
        static void SetCurrentDirectory()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                Environment.CurrentDirectory = path;
            }
            catch
            {
            }
        }
        static void StartGUI()
        {
            FileLogger.Init();
            Server.RestartPath = Application.ExecutablePath;
            AppDomain.CurrentDomain.UnhandledException += GlobalExHandler;
            Application.ThreadException += ThreadExHandler;
            DetectBuggyCursors();
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Window());
            }
            catch (Exception e)
            {
                Logger.LogError(e);
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
        static void ThreadExHandler(object sender, ThreadExceptionEventArgs e)
        {
            LogAndRestart(e.Exception);
        }
        static void DetectBuggyCursors()
        {
            try
            {
                Cursor c = Cursors.SizeNWSE;
            }
            catch (ArgumentException ex)
            {
                Logger.LogError("checking Cursors", ex);
                try
                {
                    BypassCursorsHACK();
                }
                catch
                {
                }
                Popup.Warning("Video driver appears to be returning buggy cursor sizes\n\nAttempted to workaround this issue (might not work)");
            }
            catch (Exception ex)
            {
                Logger.LogError("checking Cursors", ex);
            }
        }
        static void BypassCursorsHACK()
        {
            if (!Server.RunningOnMono())
            {
                return;
            }
            Type stdCursorType = typeof(Cursor).Assembly.GetType("System.Windows.Forms.StdCursor");
            ConstructorInfo cursor_cons = typeof(Cursor).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { stdCursorType }, null);
            object cursor = cursor_cons.Invoke(new object[] { 23 });
            FieldInfo nwse_field = typeof(Cursors).GetField("size_nwse", BindingFlags.NonPublic | BindingFlags.Static);
            nwse_field.SetValue(null, cursor);
        }
    }
}
