/*    
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using MCGalaxy.Events;
using MCGalaxy.Events.LevelEvents;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Generator;
using MCGalaxy.Gui.Popups;
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
namespace MCGalaxy.Gui
{
    public partial class Window : Form
    {
        delegate void StringCallback(string s);
        delegate void PlayerListCallback(List<Player> players);
        delegate void VoidDelegate();
        bool mapgen, loaded;
        readonly NotifyIcon notifyIcon = new();
        Player curPlayer;
        public Window()
        {
            logCallback = LogMessageCore;
            InitializeComponent();
        }
        static void CheckVersions()
        {
            string gui_version = Server.InternalVersion,
                dll_version = Server.Version;
            if (gui_version.CaselessEq(dll_version))
            {
                return;
            }
            const string fmt =
@"Currently you are using:
  {2} for {0} {1}
  {4} for {0} {3}
Trying to mix two versions is unsupported - you may experience issues";
            string msg = string.Format(fmt, Server.SoftwareName,
                                       gui_version, AssemblyFile(typeof(Window), "MCGalaxy.exe"),
                                       dll_version, AssemblyFile(typeof(Server), "MCGalaxy_.dll"));
            RunAsync(() => Popup.Warning(msg));
        }
        static string AssemblyFile(Type type, string defPath)
        {
            try
            {
                string path = type.Assembly.CodeBase;
                return Path.GetFileName(path);
            }
            catch
            {
                return defPath;
            }
        }
        void Window_Load(object sender, EventArgs e)
        {
            LoadIcon();
            if (loaded)
            {
                return;
            }
            loaded = true;
            Text = "Starting " + Server.SoftwareNameVersioned + "...";
            Show();
            BringToFront();
            WindowState = FormWindowState.Normal;
            CheckVersions();
            InitServer();
            foreach (MapGen gen in MapGen.Generators)
            {
                if (gen.Type == GenType.Advanced)
                {
                    continue;
                }
                map_cmbType.Items.Add(gen.Theme);
            }
            Text = Server.Config.Name + " - " + Server.SoftwareNameVersioned;
            MakeNotifyIcon();
            main_Players.Font = new("Calibri", 8.25f);
            main_Maps.Font = new("Calibri", 8.25f);
        }
        void LoadIcon()
        {
            try
            {
                Icon = GetIcon();
                GuiUtils.WinIcon = Icon;
            }
            catch 
            { 
            }
        }
        void UpdateNotifyIconText()
        {
            int playerCount = PlayerInfo.Online.Count;
            string players = " (" + playerCount + " players)",
                text = Server.Config.Name + players;
            if (text.Length > 63)
            {
                text = text.Substring(0, 63);
            }
            notifyIcon.Text = text;
        }
        void MakeNotifyIcon()
        {
            UpdateNotifyIconText();
            notifyIcon.ContextMenuStrip = icon_context;
            notifyIcon.Icon = Icon;
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
        }
        void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Icon_OpenConsole_Click(sender, e);
            }
        }
        void InitServer()
        {
            Logger.LogHandler += LogMessage;
            Updater.NewerVersionDetected += OnNewerVersionDetected;
            Server.OnURLChange += UpdateUrl;
            Server.OnSettingsUpdate += SettingsUpdate;
            Server.Background.QueueOnce(InitServerTask);
        }
        delegate void LogCallback(LogType type, string message);
        readonly LogCallback logCallback;
        void LogMessage(LogType type, string message)
        {
            if (!Server.Config.ConsoleLogging[(int)type])
            {
                return;
            }
            try
            {
                BeginInvoke(logCallback, type, message);
            }
            catch (InvalidOperationException)
            {
            }
        }
        void LogMessageCore(LogType type, string message)
        {
            if (Server.shuttingDown)
            {
                return;
            }
            string newline = Environment.NewLine;
            switch (type)
            {
                case LogType.Error:
                    main_txtLog.AppendLog("&c!!!Error" + ExtractErrorMessage(message)
                                          + " - See Logs tab for more details" + newline);
                    message = FormatError(message);
                    logs_txtError.AppendText(message + newline);
                    break;
                case LogType.BackgroundActivity:
                    message = DateTime.Now.ToString("(HH:mm:ss) ") + message;
                    logs_txtSystem.AppendText(message + newline);
                    break;
                case LogType.CommandUsage:
                    message = DateTime.Now.ToString("(HH:mm:ss) ") + message;
                    main_txtLog.AppendLog(message + newline, main_txtLog.ForeColor, false);
                    break;
                default:
                    main_txtLog.AppendLog(message + newline);
                    break;
            }
        }
        static string FormatError(string message)
        {
            string date = "----" + DateTime.Now + "----";
            return date + Environment.NewLine + message + Environment.NewLine + "-------------------------";
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
        void OnNewerVersionDetected(object sender, EventArgs e)
        {
            RunOnUI_Async(ShowUpdateMessageBox);
        }
        void ShowUpdateMessageBox()
        {
            if (UpdateAvailable.Active)
            {
                return;
            }
            UpdateAvailable form = new();
            form.Location = new(Location.X + (Width - form.Width) / 2,
                Location.Y + (Height - form.Height) / 2);
            form.Show(this);
        }
        static void RunAsync(ThreadStart func)
        {
            Thread thread = new(func)
            {
                Name = "MsgBox"
            };
            thread.Start();
        }
        void InitServerTask(SchedulerTask task)
        {
            Server.Start();
            Server.Background.QueueRepeat(Updater.UpdaterTask, null, TimeSpan.FromSeconds(10));
            OnPlayerConnectEvent.Register(Player_PlayerConnect, Priority.Low);
            OnPlayerDisconnectEvent.Register(Player_PlayerDisconnect, Priority.Low);
            OnSentMapEvent.Register(Player_OnJoinedLevel, Priority.Low);
            OnModActionEvent.Register(Player_OnModAction, Priority.Low);
            OnLevelAddedEvent.Register(Level_LevelAdded, Priority.Low);
            OnLevelRemovedEvent.Register(Level_LevelRemoved, Priority.Low);
            OnPhysicsLevelChangedEvent.Register(Level_PhysicsLevelChanged, Priority.Low);
            RunOnUI_Async(() => main_btnProps.Enabled = true);
        }
        public void RunOnUI_Async(UIAction act) 
        { 
            BeginInvoke(act); 
        }
        void Player_PlayerConnect(Player p)
        {
            RunOnUI_Async(() =>
            {
                Main_UpdatePlayersList();
                Players_UpdateList();
            });
        }
        void Player_PlayerDisconnect(Player p, string reason)
        {
            RunOnUI_Async(() =>
            {
                Main_UpdateMapList();
                Main_UpdatePlayersList();
                Players_UpdateList();
            });
        }
        void Player_OnJoinedLevel(Player p, Level prevLevel, Level lvl)
        {
            RunOnUI_Async(() =>
            {
                Main_UpdateMapList();
                Main_UpdatePlayersList();
                Players_UpdateSelected();
            });
        }
        void Player_OnModAction(ModAction action)
        {
            if (action.Type != ModActionType.Rank)
            {
                return;
            }
            RunOnUI_Async(() =>
            {
                Main_UpdatePlayersList();
            });
        }
        void Level_LevelAdded(Level lvl)
        {
            RunOnUI_Async(() =>
            {
                Main_UpdateMapList();
                Map_UpdateLoadedList();
                Map_UpdateUnloadedList();
            });
        }
        void Level_LevelRemoved(Level lvl)
        {
            RunOnUI_Async(() =>
            {
                Main_UpdateMapList();
                Map_UpdateLoadedList();
                Map_UpdateUnloadedList();
            });
        }
        void Level_PhysicsLevelChanged(Level lvl, int level)
        {
            RunOnUI_Async(() =>
            {
                Main_UpdateMapList();
                Map_UpdateLoadedList();
            });
        }
        void SettingsUpdate()
        {
            RunOnUI_Async(() =>
            {
                if (Server.shuttingDown)
                {
                    return;
                }
                Text = Server.Config.Name + " - " + Server.SoftwareNameVersioned;
                UpdateNotifyIconText();
            });
        }
        void UpdateUrl(string s)
        {
            RunOnUI_Async(() => 
            { 
                Main_UpdateUrl(s); 
            });
        }
        void Window_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                Server.Stop(false, "Server shutdown - PC turning off");
                notifyIcon.Dispose();
            }
            if (Server.shuttingDown || Popup.OKCancel("Really shutdown the server? All players will be disconnected!", "Exit"))
            {
                Server.Stop(false, Server.Config.DefaultShutdownMessage);
                notifyIcon.Dispose();
            }
            else
            {
                e.Cancel = true;
            }
        }
        void BtnClose_Click(object sender, EventArgs e) 
        {
            Close(); 
        }
        void BtnProperties_Click(object sender, EventArgs e)
        {
            if (!hasPropsForm)
            {
                propsForm = new();
                hasPropsForm = true;
            }
            propsForm.Show();
            if (!propsForm.Focused)
            {
                propsForm.Focus();
            }
        }
        public static bool hasPropsForm;
        PropertyWindow propsForm;
        bool alwaysInTaskbar = true;
        void Window_Resize(object sender, EventArgs e)
        {
            ShowInTaskbar = alwaysInTaskbar;
        }
        void Icon_HideWindow_Click(object sender, EventArgs e)
        {
            alwaysInTaskbar = !alwaysInTaskbar;
            ShowInTaskbar = alwaysInTaskbar;
            icon_hideWindow.Text = alwaysInTaskbar ? "Hide from taskbar" : "Show in taskbar";
        }
        void Icon_OpenConsole_Click(object sender, EventArgs e)
        {
            Show();
            BringToFront();
            WindowState = FormWindowState.Normal;
        }
        void Icon_Shutdown_Click(object sender, EventArgs e)
        {
            Close();
        }
        void Icon_restart_Click(object sender, EventArgs e)
        {
            Main_BtnRestart_Click(sender, e);
        }
        void Tabs_Click(object sender, EventArgs e)
        {
            try 
            {
                Map_UpdateUnloadedList(); 
            }
            catch 
            { 
            }
            try 
            { 
                Players_UpdateList(); 
            }
            catch 
            { 
            }
            try
            {
                if (logs_txtGeneral.Text.Length == 0)
                {
                    logs_dateGeneral.Value = DateTime.Now;
                }
            }
            catch 
            { 
            }
            foreach (TabPage page in tabs.TabPages)
            {
                foreach (Control control in page.Controls)
                {
                    if (!control.GetType().IsSubclassOf(typeof(TextBox)))
                    {
                        continue;
                    }
                    control.Update();
                }
            }
            tabs.Update();
        }
        void Main_players_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            e.PaintParts &= ~DataGridViewPaintParts.Focus;
        }
    }
}
