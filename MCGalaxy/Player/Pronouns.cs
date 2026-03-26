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
using MCGalaxy.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
namespace MCGalaxy
{
    public class Pronouns
    {
        public static readonly object locker = new();
        public static readonly List<Pronouns> Loaded = new();
        public static Pronouns Default;
        /// <summary>
        /// Called once to initialize the defaults and write/read the config file as necessary.
        /// </summary>
        public static void Init(SchedulerTask _)
        {
            if (!Directory.Exists("text/pronouns/"))
                Directory.CreateDirectory("text/pronouns/");
            Default = new("default", "they", "their", "themselves", true, "them");
            if (!File.Exists("props/pronouns.properties"))
            {
                Loaded.Add(new("they/them", "they", "their", "themselves", true, "them"));
                Loaded.Add(new("he/him", "he", "his", "himself", false, "him"));
                Loaded.Add(new("she/her", "she", "her", "herself", false, "her"));
                using (StreamWriter w = new("props/pronouns.properties"))
                {
                    w.WriteLine("# Below are the pronouns that players may choose from by using /pronouns");
                    w.WriteLine("# Lines starting with # are ignored");
                    w.WriteLine("# Each pronouns is on its own line, and is formatted like so:");
                    w.WriteLine("# Name [subject form] [object form] [reflexive form] [singular or plural] [third person objective singular]");
                    w.WriteLine("# -");
                    w.WriteLine("# [singular or plural] is for grammar's sake and will");
                    w.WriteLine("# determine which word is used in cases such as are/is, were/was, and have/has.");
                    w.WriteLine("# For instance, \"and he has captured the flag\" vs \"and they have captured the flag\".");
                    w.WriteLine();
                    foreach (Pronouns p in Loaded)
                        p.Write(w);
                }
                Logger.Log(LogType.SystemActivity, "CREATED NEW: props/pronouns.properties");
            }
            Events.ServerEvents.OnConfigUpdatedEvent.Register(OnConfigUpdated, Priority.Low);
            OnConfigUpdated();
        }
        public static void OnConfigUpdated()
        {
            lock (locker)
            {
                Loaded.Clear();
                Loaded.Add(Default);
                try
                {
                    using StreamReader r = new("props/pronouns.properties");
                    while (!r.EndOfStream)
                    {
                        string line = r.ReadLine();
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                            continue;
                        LoadFrom(line);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
            }
            foreach (Player p in PlayerInfo.Online.Items)
                p.pronounsList = GetFor(p.name);
        }
        public static void LoadFrom(string line)
        {
            string[] words = line.ToLower().SplitSpaces();
            if (words.Length < 5)
            {
                Logger.Log(LogType.Warning,
                    "Failed to load malformed pronouns \"{0}\" from config (expected at least {1} arguments, got {2}).",
                    line, 5, words.Length);
                return;
            }
            string name = words[0];
            bool plural;
            if (words[4].CaselessEq("singular"))
                plural = false;
            else if (words[4].CaselessEq("plural"))
                plural = true;
            else
            {
                Logger.Log(LogType.Warning, "Failed to load the pronouns \"{0}\" because the 5th argument was not \"singular\" or \"plural\"", name);
                return;
            }
            if (FindExact(name) != null)
            {
                Logger.Log(LogType.Warning, "Cannot load pronouns \"{0}\" because it is already defined.", name);
                return;
            }
            string tpos = words.Length switch
            {
                > 5 => words[5],
                _ => name.CaselessContains("him") ? "him" : name.CaselessContains("her") ? "her" : Default.ThirdPersonObjectiveSingular,
            };
            Loaded.Add(new(name, words[1], words[2], words[3], plural, tpos));
        }
        public static string PlayerPath(string playerName) => "text/pronouns/" + playerName + ".txt";
        /// <summary>
        /// Find the pronouns associated with the playerName. Returns Default pronouns if none were specified.
        /// </summary>
        public static List<Pronouns> GetFor(string playerName)
        {
            string myPath = PlayerPath(playerName);
            try
            {
                string data;
                lock (locker)
                {
                    if (!File.Exists(myPath))
                        return new()
                        {
                            Default
                        };
                    data = FileIO.TryReadAllText(myPath);
                }
                data = data.Trim();
                if (data.Length == 0)
                    return new()
                    {
                        Default
                    };
                List<Pronouns> pros = new();
                string[] names = data.SplitSpaces();
                foreach (string name in names)
                {
                    Pronouns p = FindExact(name);
                    if (p == null) continue;
                    pros.Add(p);
                }
                return pros.Count != 0
                    ? pros
                    : new()
                {
                    Default
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                return new() 
                { 
                    Default 
                };
            }
        }
        /// <summary>
        /// Returns the Pronoun with a name that caselessly matches the input. Returns null if no matches found.
        /// </summary>
        public static Pronouns FindExact(string name)
        {
            lock (locker)
                foreach (Pronouns p in Loaded)
                    if (name.CaselessEq(p.Name))
                        return p;
            return null;
        }
        /// <summary> Finds partial matches of 'name' against the list of all pronouns </summary>
        public static Pronouns FindMatch(Player p, string name)
        {
            lock (locker)
                return Matcher.Find(p, name, out _, Loaded,
                                           null, pro => pro.Name, "pronouns");
        }
        /// <summary>
        /// Returns a list of the names of all currently available pronouns.
        /// </summary>
        public static List<string> GetNames()
        {
            List<string> names = new();
            lock (locker)
                foreach (Pronouns p in Loaded)
                    names.Add(p.Name);
            return names;
        }
        public static string ListFor(Player p, string separator) => p.pronounsList.Join((pro) => pro.Name, separator);
        public readonly string Name;
        /// <summary>
        /// They/He/She/It
        /// </summary>
        public readonly string Subject;
        /// <summary>
        /// Their/His/Her/Its
        /// </summary>
        public readonly string Object;
        /// <summary>
        /// Themselves/Himself/Herself/Itself
        /// </summary>
        public readonly string Reflexive;
        /// <summary>
        /// Them/Him/Her/It
        /// </summary>
        public readonly string ThirdPersonObjectiveSingular;
        /// <summary>
        /// They/them uses "plural" style verbs, so this is required for grammar that sounds correct
        /// </summary>
        public readonly bool Plural;
        /// <summary>
        /// are, is
        /// </summary>
        public readonly string PresentVerb;
        /// <summary>
        /// were, was
        /// </summary>
        public readonly string PastVerb;
        /// <summary>
        /// have, has
        /// </summary>
        public readonly string PresentPerfectVerb;
        public Pronouns(string name, string subject, string obj, string reflexive, bool plural, string thirdPersonObjectiveSingular)
        {
            Name = name;
            Subject = subject;
            Object = obj;
            Reflexive = reflexive;
            Plural = plural;
            ThirdPersonObjectiveSingular = thirdPersonObjectiveSingular;
            PresentVerb = Plural ? "are" : "is";
            PastVerb = Plural ? "were" : "was";
            PresentPerfectVerb = Plural ? "have" : "has";
        }
        public void Write(StreamWriter w)
        {
            w.WriteLine(string.Format("{0} {1} {2} {3} {4} {5}",
                Name, Subject, Object, Reflexive, Plural ? "plural" : "singular", ThirdPersonObjectiveSingular));
            w.WriteLine();
        }
        public static void SaveFor(Player p)
        {
            string path = PlayerPath(p.name);
            try
            {
                if (p.pronounsList.Count == 1 && p.pronounsList[0] == Default)
                {
                    FileIO.TryDelete(path);
                    return;
                }
                FileIO.TryWriteAllText(path, ListFor(p, " "));
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                p.Message("&WThere was an error when saving your pronouns: &S{0}", e.Message);
            }
        }
    }
}
