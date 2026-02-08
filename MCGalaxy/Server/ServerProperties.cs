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
using System;
using System.IO;
namespace MCGalaxy
{
    public static class SrvProperties
    {
        public static void Load()
        {
            PropertiesFile.Read(Paths.ServerPropsFile, (key, value) => ConfigElement.Parse(Server.serverConfig, Server.Config, key, value));
            ApplyChanges();
            Save();
        }
        public static void ApplyChanges()
        {
            if (!Directory.Exists(Server.Config.BackupDirectory))
            {
                Server.Config.BackupDirectory = "levels/backups";
            }
            Server.SetMainLevel(Server.Config.MainLevel);
        }
        static readonly object saveLock = new();
        public static void Save()
        {
            try
            {
                lock (saveLock)
                {
                    using StreamWriter w = FileIO.CreateGuarded(Paths.ServerPropsFile);
                    SaveProps(w);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving " + Paths.ServerPropsFile, ex);
            }
        }
        static void SaveProps(StreamWriter w)
        {
            w.WriteLine("# Edit the settings below to modify how your server operates.");
            w.WriteLine("#");
            w.WriteLine("# Explanation of Server settings:");
            w.WriteLine("#   server-name                   = The name which displays on classicube.net");
            w.WriteLine("#   motd                          = The message which displays when a player connects");
            w.WriteLine("#   port                          = The port to list for connections on");
            w.WriteLine("#   public                        = Set to true to appear in the public server list");
            w.WriteLine("#   max-players                   = The maximum number of players allowed");
            w.WriteLine("#   max-guests                    = The maximum number of guests allowed");
            w.WriteLine("#   verify-names                  = Verify the validity of names");
            w.WriteLine("#   server-owner                  = The username of the owner of the server");
            w.WriteLine("# Explanation of Level settings:");
            w.WriteLine("#   world-chat                    = Set to true to enable world chat");
            w.WriteLine("# Explanation of IRC settings:");
            w.WriteLine("#   irc                           = Set to true to enable the IRC bot");
            w.WriteLine("#   irc-nick                      = The name of the IRC bot");
            w.WriteLine("#   irc-server                    = The server to connect to");
            w.WriteLine("#   irc-channel                   = The channel to join");
            w.WriteLine("#   irc-opchannel                 = The channel to join (posts OpChat)");
            w.WriteLine("#   irc-port                      = The port to use to connect");
            w.WriteLine("#   irc-identify                  = (true/false)    Do you want the IRC bot to Identify itself with nickserv. Note: You will need to register it's name with nickserv manually.");
            w.WriteLine("#   irc-password                  = The password you want to use if you're identifying with nickserv");
            w.WriteLine("# Explanation of Backup settings:");
            w.WriteLine("#   backup-time                   = The number of seconds between automatic backups");
            w.WriteLine("# Explanation of Color settings:");
            w.WriteLine("#   defaultColor                  = The color code of the default messages (Default = &e)");
            w.WriteLine("# Explanation of Other settings:");
            w.WriteLine("#   use-whitelist                 = Switch to allow use of a whitelist to override IP bans for certain players.  Default false.");
            w.WriteLine("#   profanity-filter              = Replace certain bad words in the chat.  Default false.");
            w.WriteLine("#   notify-on-join-leave          = Show a balloon popup in tray notification area when a player joins/leaves the server.  Default false.");
            w.WriteLine("#   allow-tp-to-higher-ranks      = Allows the teleportation to players of higher ranks");
            w.WriteLine("#   agree-to-rules-on-entry       = Forces all new players to the server to agree to the rules before they can build or use commands.");
            w.WriteLine("#   admins-join-silent            = Players who have adminchat permission join the game silently. Default true");
            w.WriteLine("#   guest-limit-notify            = Show -Too Many Guests- message in chat when maxGuests has been reached. Default false");
            w.WriteLine("#   guest-join-notify             = Shows when guests and lower ranks join server in chat and IRC. Default true");
            w.WriteLine("#   guest-leave-notify            = Shows when guests and lower ranks leave server in chat and IRC. Default true");
            w.WriteLine("#   announcement-interval         = The delay in between server announcements in minutes. Default 5");
            w.WriteLine();
            w.WriteLine("#   kick-on-hackrank              = Set to true if hackrank should kick players");
            w.WriteLine("#   hackrank-kick-time            = Number of seconds until player is kicked");
            w.WriteLine("# Explanation of Security settings:");
            w.WriteLine("#   admin-verification            = Determines whether admins have to verify on entry to the server.  Default true.");
            w.WriteLine("#   verify-admin-perm             = The minimum rank required for admin verification to occur.");
            w.WriteLine("# Explanation of Spam Control settings:");
            w.WriteLine("#   mute-on-spam                  = If enabled it mutes a player for spamming.  Default false.");
            w.WriteLine("#   spam-messages                 = The amount of messages that have to be sent \"consecutively\" to be muted.");
            w.WriteLine("#   spam-mute-time                = The amount of seconds a player is muted for spam.");
            w.WriteLine("#   spam-counter-reset-time       = The amount of seconds the \"consecutive\" messages have to fall between to be considered spam.");
            w.WriteLine();
            w.WriteLine("#   As an example, if you wanted the spam to only mute if a user posts 5 messages in a row within 2 seconds, you would use the folowing:");
            w.WriteLine("#   mute-on-spam                  = true");
            w.WriteLine("#   spam-messages                 = 5");
            w.WriteLine("#   spam-mute-time                = 60");
            w.WriteLine("#   spam-counter-reset-time       = 2");
            w.WriteLine();
            ConfigElement.Serialise(Server.serverConfig, w, Server.Config);
        }
    }
}
