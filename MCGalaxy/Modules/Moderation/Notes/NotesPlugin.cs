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
using MCGalaxy.Events;
using System;

namespace MCGalaxy.Modules.Moderation.Notes
{
    public sealed class NotesPlugin : Plugin
    {
        public override string name { get { return "Notes"; } }


        static readonly Command[] cmds = new Command[] { new CmdNotes(), new CmdMyNotes(), new CmdNote(), new CmdOpNote(), };

        public override void Load(bool startup)
        {
            OnModActionEvent.Register(HandleModerationAction, Priority.Low);
            Command.Register(cmds);
            NoteAcronym.Init();
        }

        public override void Unload(bool shutdown)
        {
            OnModActionEvent.Unregister(HandleModerationAction);
            Command.Unregister(cmds);
        }


        static void HandleModerationAction(ModAction action)
        {
            string acronym = NoteAcronym.GetAcronym(action);
            if (acronym == null) return;

            AddNote(action, acronym);
        }

        static void AddNote(ModAction e, string type)
        {
            if (!Server.Config.LogNotes) return;
            string src = e.Actor.name;

            string time = DateTime.UtcNow.ToString("dd/MM/yyyy");
            string data = e.Target + " " + type + " " + src + " " + time + " " +
                          e.Reason.Replace(" ", "%20") + " " + e.Duration.Ticks;
            Server.Notes.Append(data);
        }
    }

    /// <summary>
    /// Moderation note actions are logged to disk using single-letter acronyms. This class handles translating these to and from human-readable actions.
    /// </summary>
    public class NoteAcronym
    {
        public readonly string Acronym;
        public readonly string Action;

        private NoteAcronym(string acronym, string action)
        {
            Acronym = acronym;
            Action = action;
        }


        private static readonly NoteAcronym Warned = new("W", "Warned");
        private static readonly NoteAcronym Kicked = new("K", "Kicked");
        private static readonly NoteAcronym Muted = new("M", "Muted");
        private static readonly NoteAcronym Banned = new("B", "Banned");
        private static readonly NoteAcronym Jailed = new("J", "Jailed"); // Jailing was removed, but still appears in notes for historical reasons
        private static readonly NoteAcronym Frozen = new("F", "Frozen");
        private static readonly NoteAcronym TempBanned = new("T", "Temp-Banned");
        private static readonly NoteAcronym Noted = new("N", "Noted");
        public static readonly NoteAcronym OpNoted = new("O", "OpNoted");

        static NoteAcronym[] All;

        internal static void Init()
        {
            All = new NoteAcronym[] { Warned, Kicked, Muted, Banned, Jailed, Frozen, TempBanned, Noted, OpNoted };
        }

        /// <summary>
        /// Returns the appropriate Acronym to log when a mod action occurs.
        /// </summary>
        public static string GetAcronym(ModAction action)
        {
            if (action.Type == ModActionType.Ban)
            {
                return action.Duration.Ticks != 0 ? TempBanned.Acronym : Banned.Acronym;
            }

            string modActionString = action.Type.ToString();
            foreach (NoteAcronym na in All)
            {
                if (na.Action == modActionString) { return na.Acronym; }
            }
            return null;
        }
        /// <summary>
        /// Returns the appropriate Action from a mod note acronym. If none are found, returns the original argument.
        /// </summary>
        public static string GetAction(string acronym)
        {
            foreach (NoteAcronym na in All)
            {
                if (na.Acronym == acronym) { return na.Action; }
            }
            return acronym;
        }
    }
}
