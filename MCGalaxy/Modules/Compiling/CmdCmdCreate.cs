/*
    Copyright 2010 MCLawl Team - Written by Valek (Modified for use with MCForge)
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
using MCGalaxy.Commands;
using System.IO;
namespace MCGalaxy.Modules.Compiling
{
    sealed class CmdCmdCreate : CmdCompile
    {
        public override string Name => "CmdCreate";
        public override string Shortcut => "";
        public override CommandAlias[] Aliases => new[] { new CommandAlias("PCreate", "plugin") };
        protected override void CompileCommand(Player p, string[] paths)
        {
            foreach (string cmd in paths)
            {
                CreateFile(p, cmd, Compiler.CommandDLLPath(cmd), "command &fCmd", Compiler.GenExampleCommand(cmd));
            }
        }
        protected override void CompilePlugin(Player p, string[] paths)
        {
            foreach (string name in paths)
            {
                CreateFile(p, name, Compiler.PluginPath(name), "plugin &f", Compiler.GenExamplePlugin(name, p.IsSuper ? Server.Config.Name : p.truename));
            }
        }
        static bool CreateFile(Player p, string name, string path, string type, string source)
        {
            if (File.Exists(path))
            {
                p.Message("File {0} already exists. Choose another name.", path);
                return false;
            }
            FileIO.TryWriteAllText(path, source);
            p.Message("Successfully saved example {2}{0} &Sto {1}", name, path, type);
            return true;
        }
        public override void Help(Player p)
        {
            p.Message("&T/CmdCreate [name]");
            p.Message("&HCreates an example C# command named Cmd[name]");
            p.Message("&H  This can be used as the basis for creating a new command");
            p.Message("&T/CmdCreate plugin [name]");
            p.Message("&HCreate a example C# plugin named [name]");
        }
    }
}