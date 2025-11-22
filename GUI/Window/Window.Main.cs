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
using MCGalaxy.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
namespace MCGalaxy.Gui
{
    public partial class Window : Form
    {
        Player GetSelectedPlayer()
        {
            string name = GetSelected(main_Players);
            if (name == null)
            {
                return null;
            }
            return PlayerInfo.FindExact(name);
        }
        void PlayerCmd(string command)
        {
            Player player = GetSelectedPlayer();
            if (player == null)
            {
                return;
            }
            UIHelpers.HandleCommand(command + " " + player.name);
        }
        void PlayerCmd(string command, string prefix, string suffix)
        {
            Player player = GetSelectedPlayer();
            if (player == null)
            {
                return;
            }
            UIHelpers.HandleCommand(command + " " + prefix + player.name + suffix);
        }
        void TsPlayer_Clones_Click(object sender, EventArgs e)
        {
            PlayerCmd("Clones");
        }
        void TsPlayer_Voice_Click(object sender, EventArgs e)
        {
            PlayerCmd("Voice");
        }
        void TsPlayer_Whois_Click(object sender, EventArgs e)
        {
            PlayerCmd("WhoIs");
        }
        void TsPlayer_Ban_Click(object sender, EventArgs e)
        {
            PlayerCmd("Ban");
        }
        void TsPlayer_Kick_Click(object sender, EventArgs e)
        {
            PlayerCmd("Kick");
        }
        void TsPlayer_Promote_Click(object sender, EventArgs e)
        {
            PlayerCmd("SetRank", "+up ", "");
        }
        void TsPlayer_Demote_Click(object sender, EventArgs e)
        {
            PlayerCmd("SetRank", "-down ", "");
        }
        Level GetSelectedLevel()
        {
            string name = GetSelected(main_Maps);
            if (name == null)
            {
                return null;
            }
            return LevelInfo.FindExact(name);
        }
        void LevelCmd(string command)
        {
            Level level = GetSelectedLevel();
            if (level == null)
            {
                return;
            }
            UIHelpers.HandleCommand(command + " " + level.name);
        }
        void LevelCmd(string command, string prefix, string suffix)
        {
            Level level = GetSelectedLevel();
            if (level == null)
            {
                return;
            }
            UIHelpers.HandleCommand(command + " " + prefix + level.name + suffix);
        }
        void TsMap_Info_Click(object sender, EventArgs e)
        {
            LevelCmd("Map");
            LevelCmd("MapInfo");
        }
        void TsMap_MoveAll_Click(object sender, EventArgs e)
        {
            LevelCmd("MoveAll");
        }
        void TsMap_Physics0_Click(object sender, EventArgs e)
        {
            LevelCmd("Physics", "", " 0");
        }
        void TsMap_Physics1_Click(object sender, EventArgs e)
        {
            LevelCmd("Physics", "", " 1");
        }
        void TsMap_Physics2_Click(object sender, EventArgs e)
        {
            LevelCmd("Physics", "", " 2");
        }
        void TsMap_Physics3_Click(object sender, EventArgs e)
        {
            LevelCmd("Physics", "", " 3");
        }
        void TsMap_Physics4_Click(object sender, EventArgs e)
        {
            LevelCmd("Physics", "", " 4");
        }
        void TsMap_Physics5_Click(object sender, EventArgs e)
        {
            LevelCmd("Physics", "", " 5");
        }
        void TsMap_Save_Click(object sender, EventArgs e)
        {
            LevelCmd("Save");
        }
        void TsMap_Unload_Click(object sender, EventArgs e)
        {
            LevelCmd("Unload");
        }
        void TsMap_Reload_Click(object sender, EventArgs e)
        {
            LevelCmd("Reload");
        }
        readonly List<string> inputLog = new(21);
        int inputIndex = -1;
        void Main_TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
            {
                inputIndex = Math.Min(inputIndex + 1, inputLog.Count - 1);
                if (inputIndex > -1)
                {
                    SetInputText();
                }
            }
            else if (e.KeyCode == Keys.Down)
            {
                inputIndex = Math.Max(inputIndex - 1, -1);
                if (inputIndex > -1)
                {
                    SetInputText();
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                InputText();
            }
            else
            {
                inputIndex = -1;
                return;
            }
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        void SetInputText()
        {
            if (inputIndex == -1)
            {
                return;
            }
            main_txtInput.Text = inputLog[inputIndex];
            main_txtInput.SelectionLength = 0;
            main_txtInput.SelectionStart = main_txtInput.Text.Length;
        }
        void AddInputLog(string text)
        {
            if (inputLog.Count > 0 && text == inputLog[0])
            {
                return;
            }
            inputLog.Insert(0, text);
            if (inputLog.Count > 20)
            {
                inputLog.RemoveAt(20);
            }
        }
        void InputText()
        {
            string text = main_txtInput.Text;
            if (text.Length == 0)
            {
                return;
            }
            AddInputLog(text);
            if (text == "/")
            {
                UIHelpers.RepeatCommand();
            }
            else if (text[0] == '/' && text.Length > 1 && text[1] == '/')
            {
                UIHelpers.HandleChat(text.Substring(1));
            }
            else if (text[0] == '/')
            {
                UIHelpers.HandleCommand(text.Substring(1));
            }
            else
            {
                UIHelpers.HandleChat(text);
            }
            main_txtInput.Clear();
        }
        void Main_BtnRestart_Click(object sender, EventArgs e)
        {
            if (Popup.OKCancel("Are you sure you want to restart?", "Restart"))
            {
                Server.Stop(true, Server.Config.DefaultRestartMessage);
            }
        }
        void Main_TxtUrl_DoubleClick(object sender, EventArgs e)
        {
            if (!Main_IsUsingUrl())
            {
                return;
            }
            GuiUtils.OpenBrowser(main_txtUrl.Text);
        }
        void Main_BtnSaveAll_Click(object sender, EventArgs e)
        {
            UIHelpers.HandleCommand("Save all");
        }
        void Main_BtnKillPhysics_Click(object sender, EventArgs e)
        {
            UIHelpers.HandleCommand("Physics kill");
        }
        void Main_BtnUnloadEmpty_Click(object sender, EventArgs e)
        {
            UIHelpers.HandleCommand("Unload empty");
        }
        void TsLog_Night_Click(object sender, EventArgs e)
        {
            tsLog_night.Checked = !tsLog_night.Checked;
            main_txtLog.NightMode = tsLog_night.Checked;
        }
        void TsLog_Colored_Click(object sender, EventArgs e)
        {
            main_txtLog.Colorize = !tsLog_Colored.Checked;
            tsLog_Colored.Checked = !tsLog_Colored.Checked;
        }
        void TsLog_DateStamp_Click(object sender, EventArgs e)
        {
            main_txtLog.DateStamp = !tsLog_dateStamp.Checked;
            tsLog_dateStamp.Checked = !tsLog_dateStamp.Checked;
        }
        void TsLog_AutoScroll_Click(object sender, EventArgs e)
        {
            main_txtLog.AutoScroll = !tsLog_autoScroll.Checked;
            tsLog_autoScroll.Checked = !tsLog_autoScroll.Checked;
        }
        void TsLog_CopySelected_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(main_txtLog.SelectedText))
            {
                return;
            }
            Clipboard.SetText(main_txtLog.SelectedText, TextDataFormat.Text);
        }
        void TsLog_CopyAll_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(main_txtLog.Text, TextDataFormat.Text);
        }
        void TsLog_Clear_Click(object sender, EventArgs e)
        {
            if (Popup.OKCancel("Are you sure you want to clear logs?", "Clear logs"))
            {
                main_txtLog.ClearLog();
            }
        }
        bool Main_IsUsingUrl()
        {
            return Uri.TryCreate(main_txtUrl.Text, UriKind.Absolute, out _);
        }
        void Main_UpdateUrl(string s)
        {
            main_txtUrl.Text = s;
            bool isUrl = Main_IsUsingUrl();
            Color linkCol = Color.FromArgb(255, 0, 102, 204);
            main_txtUrl.BackColor = main_txtUrl.BackColor;
            main_txtUrl.ForeColor = isUrl ? linkCol : SystemColors.WindowText;
            main_txtUrl.Font = new(main_txtUrl.Font,
                isUrl ? FontStyle.Underline : FontStyle.Regular);
        }
        void Main_UpdateMapList()
        {
            Level[] loaded = LevelInfo.Loaded.Items;
            string selected = GetSelected(main_Maps);
            main_Maps.Rows.Clear();
            foreach (Level lvl in loaded)
            {
                main_Maps.Rows.Add(lvl.name, lvl.players.Count, lvl.physics);
            }
            Reselect(main_Maps, selected);
            main_Maps.Refresh();
        }
        void Main_UpdatePlayersList()
        {
            UpdateNotifyIconText();
            Player[] players = PlayerInfo.Online.Items;
            string selected = GetSelected(main_Players);
            main_Players.Rows.Clear();
            foreach (Player pl in players)
            {
                main_Players.Rows.Add(pl.truename, pl.level.name, pl.group.Name);
            }
            Reselect(main_Players, selected);
            main_Players.Refresh();
        }
        static string GetSelected(DataGridView view)
        {
            DataGridViewSelectedRowCollection selected = view.SelectedRows;
            if (selected.Count <= 0)
            {
                return null;
            }
            return (string)selected[0].Cells[0].Value;
        }
        static void Reselect(DataGridView view, string selected)
        {
            if (selected == null)
            {
                return;
            }
            foreach (DataGridViewRow row in view.Rows)
            {
                string name = (string)row.Cells[0].Value;
                if (name.CaselessEq(selected))
                {
                    row.Selected = true;
                }
            }
        }
    }
}
