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
        public const string BlockPermsFile = "props/block" + PropertiesFileExt;
        public const string PlacePermsFile = "props/place" + PropertiesFileExt;
        public const string DeletePermsFile = "props/delete" + PropertiesFileExt;
        public const string CmdPermsFile = "props/command" + PropertiesFileExt;
        public const string CmdExtraPermsFile = "props/ExtraCommandPermissions" + PropertiesFileExt;
        public const string EconomyPropsFile = "props/economy" + PropertiesFileExt;
        public const string ServerPropsFile = "props/server" + PropertiesFileExt;
        public const string RankPropsFile = "props/ranks" + PropertiesFileExt;
        public const string AuthServicesFile = "props/authservices" + PropertiesFileExt;
        public const string CPEDisabledFile = "props/cpe" + PropertiesFileExt;
        public const string ImportsDir = "extra/import/";
        public const string WAYPOINTS_DIR = "extra/Waypoints/";
        public const string PropertiesFileExt = ".properties";
        public static string BotsPath(string map) => "extra/bots/" + map + ".json";
        public static string MapBlockDefs(string map) => "blockdefs/lvl_" + map + ".json";
        public static string DeletedMapFile(string level) => File.Exists("levels/deleted/" + level.ToLower() + ".mcf")
                ? "levels/deleted/" + level + ".mcf"
                : "levels/deleted/" + level + ".lvl";
        public static string PrevMapFile(string level) => File.Exists("levels/" + level.ToLower() + ".mcf") ? "levels/prev/" + level.ToLower() + ".mcf.prev" : "levels/prev/" + level.ToLower() + ".lvl.prev";
        public static string BlockPropsPath(string group) => "blockprops/" + group + ".txt";
    }
}