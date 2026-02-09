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
namespace MCGalaxy.Modules.Relay.Discord
{
    public static class DiscordUtils
    {
        static readonly string[] markdown_special = 
        {
            @"\", @"*", @"_", @"~", @"`", @"|", @"-", @"#" 
        },
        markdown_escaped = 
        { 
            @"\\", @"\*", @"\_", @"\~", @"\`", @"\|", @"\-", @"\#" 
        };
        public static string EscapeMarkdown(string message)
        {
            // don't let user use bold/italic etc markdown
            for (int i = 0; i < markdown_special.Length; i++)
            {
                message = message.Replace(markdown_special[i], markdown_escaped[i]);
            }
            return message;
        }
        public static string MarkdownToSpecial(string input) => input
                .Replace('_', '\uEDC1')
                .Replace('~', '\uEDC2')
                .Replace('*', '\uEDC3')
                .Replace('`', '\uEDC4')
                .Replace('|', '\uEDC5');
        public static string SpecialToMarkdown(string input) => input
                .Replace('\uEDC1', '_')
                .Replace('\uEDC2', '~')
                .Replace('\uEDC3', '*')
                .Replace('\uEDC4', '`')
                .Replace('\uEDC5', '|');
    }
}
