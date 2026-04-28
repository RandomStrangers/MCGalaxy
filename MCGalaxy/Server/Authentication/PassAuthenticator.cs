/*
    Written by Jack1312
    Copyright 2011-2012 MCForge
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
using MCGalaxy.Events.PlayerEvents;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace MCGalaxy.Authentication
{
    /// <summary> Manages optional additional verification for certain users </summary>
    public class ExtraAuthenticator
    {
        public static void SetActive()
        {
            Deactivate();
            Activate();
        }
        public static bool HasPassword(string name) => GetHashPath(name) != null;
        public static bool VerifyPassword(string name, string password) => GetHashPath(name) != null && CheckHash(GetHashPath(name), name, password);
        public static void StorePassword(string name, string password)
        {
            Server.EnsureDirectoryExists("extra/passwords/");
            FileIO.TryWriteAllBytes(HashPath(name), ComputeHash(name, password));
        }
        public static bool ResetPassword(string name)
        {
            if (GetHashPath(name) == null)
                return false;
            FileIO.TryDelete(GetHashPath(name));
            return true;
        }
        public static string GetHashPath(string name) => File.Exists(HashPath(name)) ? HashPath(name) : null;
        public static string HashPath(string name) => "extra/passwords/" + Server.ToRawUsername(name).ToLower() + ".pwd";
        public static bool CheckHash(string path, string name, string pass) => FileIO.TryReadBytes(path, out byte[] stored) && ArraysEqual(ComputeHash(name, pass), stored);
        public static byte[] ComputeHash(string name, string pass) => SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes("0bec662b-416f-450c-8f50-664fd4a41d49" + name.ToLower() + " " + pass));
        public static bool ArraysEqual(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;
            return true;
        }
        public static void RequiresVerification(Player p, string action) => p.Message("&WYou must first verify with &T/Pass [password] &Wbefore you can {0}", action);
        public static void NeedVerification(Player p)
        {
            if (!HasPassword(p.name))
                p.Message("&WPlease set your account verification password with &T/SetPass [password]");
            else
                p.Message("&WPlease complete account verification with &T/Pass [password]");
        }
        public static void AutoVerify(Player p, string mppass)
        {
            if (HasPassword(p.name) && VerifyPassword(p.name, mppass))
                Verify(p);
        }
        public static void Activate()
        {
            OnPlayerHelpEvent.Register(OnPlayerHelp, Priority.Low);
            OnPlayerCommandEvent.Register(OnPlayerCommand, Priority.Low);
        }
        public static void Deactivate()
        {
            OnPlayerHelpEvent.Unregister(OnPlayerHelp);
            OnPlayerCommandEvent.Unregister(OnPlayerCommand);
        }
        public static void OnPlayerHelp(Player p, string target, ref bool cancel)
        {
            if (!(target.CaselessEq("pass") || target.CaselessEq("password") || target.CaselessEq("setpass")))
                return;
            PrintHelp(p);
            cancel = true;
        }
        public static void OnPlayerCommand(Player p, string cmd, string args, CommandData data)
        {
            if (cmd.CaselessEq("pass"))
            {
                ExecPassCommand(p, args, data);
                p.cancelcommand = true;
            }
            else if (cmd.CaselessEq("setpass"))
            {
                ExecPassCommand(p, "set " + args, data);
                p.cancelcommand = true;
            }
            else if (cmd.CaselessEq("resetpass"))
            {
                ExecPassCommand(p, "reset " + args, data);
                p.cancelcommand = true;
            }
        }
        public static void ExecPassCommand(Player p, string message, CommandData data)
        {
            if (!Server.Config.verifyadmins)
            {
                p.Message("Password verification is not currently enabled.");
                return;
            }
            if (data.Rank < Server.Config.VerifyAdminsRank)
            {
                Formatter.MessageNeedMinPerm(p, "+ require password verification",
                                             Server.Config.VerifyAdminsRank);
                return;
            }
            message = message.Trim();
            if (message.Length == 0)
            {
                PrintHelp(p);
                return;
            }
            string[] args = message.SplitSpaces(2);
            switch (args.Length)
            {
                case 2 when args[0].CaselessEq("set"):
                    DoSetPassword(p, args[1]);
                    break;
                case 2 when args[0].CaselessEq("reset"):
                    DoResetPassword(p, args[1], data);
                    break;
                default:
                    DoVerifyPassword(p, message);
                    break;
            }
        }
        public static void DoVerifyPassword(Player p, string password)
        {
            if (!p.Unverified)
            {
                p.Message("&WYou are already verified.");
                return;
            }
            if (p.passtries >= 3)
            {
                p.Kick("Did you really think you could keep on guessing?");
                return;
            }
            if (password.IndexOf(' ') >= 0)
            {
                p.Message("Your password must be &Wone &Sword!");
                return;
            }
            if (!HasPassword(p.name))
            {
                p.Message("You have not &Wset a verification password yet, &Suse &T/SetPass [password] &Wto set one");
                p.Message("Make sure to use a different password than your Minecraft one!");
                return;
            }
            if (VerifyPassword(p.name, password))
            {
                Verify(p);
                return;
            }
            p.passtries++;
            p.Message("&WWrong Password. &SRemember your password is &Wcase sensitive.");
            p.Message("Forgot your password? Contact &W{0} &Sto &Wreset it.", Server.Config.OwnerName);
        }
        public static void DoSetPassword(Player p, string password)
        {
            if (p.Unverified && HasPassword(p.name))
            {
                RequiresVerification(p, "can change your verification password");
                p.Message("Forgot your password? Contact &W{0} &Sto &Wreset it.", Server.Config.OwnerName);
                return;
            }
            if (password.IndexOf(' ') >= 0)
            {
                p.Message("&WPassword must be one word.");
                return;
            }
            StorePassword(p.name, password);
            p.Message("Your verification password was &aset to: &c" + password);
        }
        public static void DoResetPassword(Player p, string name, CommandData data)
        {
            string target = PlayerInfo.FindMatchesPreferOnline(p, name);
            if (target == null)
                return;
            if (p.Unverified)
            {
                RequiresVerification(p, "can reset verification passwords");
                return;
            }
            if (data.Rank < Server.Config.ResetPasswordRank)
            {
                p.Message("Only {0}&S+ can reset verification passwords",
                          Group.GetColoredName(Server.Config.ResetPasswordRank));
                return;
            }
            if (ResetPassword(target))
                p.Message("Reset verification password for {0}", p.FormatNick(target));
            else
                p.Message("{0} &Sdoes not have a verification password.", p.FormatNick(target));
        }
        public static void PrintHelp(Player p)
        {
            p.Message("&T/Pass reset [player] &H- Resets the password for that player");
            p.Message("&H Note that only {0}&S+ can reset passwords",
                      Group.GetColoredName(Server.Config.ResetPasswordRank));
            p.Message("&T/Pass set [password] &H- Sets your password to [password]");
            p.Message("&H Note: &WDo NOT set this as your Minecraft password!");
            p.Message("&T/Pass [password]");
            p.Message("&H If you are {0}&H+, use this command to verify your login.",
                      Group.GetColoredName(Server.Config.VerifyAdminsRank));
            p.Message("&H You must be verified to use commands, modify blocks, and chat");
        }
        public static void Verify(Player p)
        {
            p.Message("You are now &averified &Sand can now &ause commands, modify blocks, and chat.");
            p.verifiedPass = true;
            p.Unverified = false;
        }
    }
}