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
using System.IO;

namespace MCGalaxy
{
    /// <summary> Provides a centralised list of files and paths used. </summary>
    public static class Paths
    {
        public const string CustomColorsFile = "text/customcolors.txt";
        public const string TempRanksFile = "text/tempranks.txt";
        public const string TempBansFile = "text/tempbans.txt";
        public const string CustomTokensFile = "text/custom$s.txt";
        public const string BadWordsFile = "text/badwords.txt";
        public const string BadWordsExceptionsFile = "text/badwords_exceptions.txt";
        public const string EatMessagesFile = "text/eatmessages.txt";
        public const string RulesFile = "text/rules.txt";
        public const string OprulesFile = "text/oprules.txt";
        public const string FaqFile = "text/faq.txt";
        public const string AnnouncementsFile = "text/messages.txt";
        public const string AliasesFile = "text/aliases.txt";
        public const string NewsFile = "text/news.txt";
        public const string WelcomeFile = "text/welcome.txt";
        public const string JokerFile = "text/joker.txt";
        public const string EightBallFile = "text/8ball.txt";
        public const string BlockPermsFile = "props/block.properties";
        public const string PlacePermsFile = "props/place.properties";
        public const string DeletePermsFile = "props/delete.properties";
        public const string CmdPermsFile = "props/command.properties";
        public const string CmdExtraPermsFile = "props/ExtraCommandPermissions.properties";
        public const string EconomyPropsFile = "props/economy.properties";
        public const string ServerPropsFile = "props/server.properties";
        public const string RankPropsFile = "props/ranks.properties";
        public const string AuthServicesFile = "props/authservices.properties";
        public const string CPEDisabledFile = "props/cpe.properties";
        public const string ImportsDir = "extra/import/";
        public const string WAYPOINTS_DIR = "extra/Waypoints/";
        /// <summary> Relative path of the file containing a map's bots. </summary>
        public static string BotsPath(string map)
        { 
            return "extra/bots/" + map + ".json"; 
        }
        /// <summary> Relative path of the file containing a map's block definitions. </summary>
        public static string MapBlockDefs(string map) 
        { 
            return "blockdefs/lvl_" + map + ".json"; 
        }
        /// <summary> Relative path of a deleted level's map file. </summary>
        public static string DeletedMapFile(string level)
        {
            bool mcf = File.Exists("levels/deleted/" + level.ToLower() + ".mcf"),
                map = File.Exists("levels/deleted/" + level.ToLower() + ".map"),
                ucl = File.Exists("levels/deleted/" + level.ToLower() + ".ucl");
            if (mcf)
            {
                return "levels/deleted/" + level + ".mcf";
            }
            else if (map)
            {
                return "levels/deleted/" + level + ".map";
            }
            else if (ucl)
            {
                return "levels/deleted/" + level + ".ucl";
            }
            else
            {
                return "levels/deleted/" + level + ".lvl";
            }
        }
        /// <summary> Relative path of a level's previous save map file. </summary>
        public static string PrevMapFile(string level)
        {
            bool mcf = File.Exists("levels/" + level.ToLower() + ".mcf"),
                map = File.Exists("levels/" + level.ToLower() + ".map"),
                ucl = File.Exists("levels/" + level.ToLower() + ".ucl");
            if (mcf)
            {
                return "levels/prev/" + level.ToLower() + ".mcf.prev";
            }
            if (map)
            {
                return "levels/prev/" + level.ToLower() + ".map.prev";
            }
            if (ucl)
            {
                return "levels/prev/" + level.ToLower() + ".ucl.prev";
            }
            else
            {
                return "levels/prev/" + level.ToLower() + ".lvl.prev";
            }
        }
        /// <summary> Relative path of a block properties file. </summary>     
        public static string BlockPropsPath(string group) 
        { 
            return "blockprops/" + group + ".txt"; 
        }
    }
}