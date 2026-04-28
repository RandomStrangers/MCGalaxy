/*
    Copyright 2011 MCForge
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
    public class CmdCompile : Command2
    {
        public override string Name => "Compile";
        public override string Type => CommandTypes.Other;
        public override LevelPermission DefaultRank => LevelPermission.Owner;
        public override CommandAlias[] Aliases => new[] { new CommandAlias("PCompile", "plugin") };
        public override bool MessageBlockRestricted => true;
        public override void Use(Player p, string message, CommandData data)
        {
            string[] args = message.SplitSpaces();
            bool plugin = args[0].CaselessEq("plugin");
            string name = plugin ? args.Length > 1 ? args[1] : "" : args[0];
            if (name.Length == 0)
            {
                Help(p);
                return;
            }
            if (!Formatter.ValidFilename(p, name))
                return;
            string[] paths = name.SplitComma();
            if (plugin)
                CompilePlugin(p, paths);
            else
                CompileCommand(p, paths);
        }
        public virtual void CompilePlugin(Player p, string[] paths)
        {
            string pln = paths[0],
                dstPath = Compiler.PluginDLLPath(pln);
            for (int i = 0; i < paths.Length; i++)
                paths[i] = Compiler.PluginPath(paths[i]);
            paths = TryDirectory(Compiler.PLUGINS_DIR, pln, paths);
            Compile(p, "Plugin", paths, dstPath);
        }
        public static void Compile(Player p, string type, string[] srcs, string dst)
        {
            foreach (string path in srcs)
            {
                if (File.Exists(path))
                    continue;
                p.Message("File &9{0} &Snot found.", path);
                return;
            }
            ICompilerErrors errors = Compiler.Compile(srcs, dst, true);
            if (!errors.HasErrors)
            {
                p.Message("{0} compiled successfully from {1}",
                        type, srcs.Join(file => Path.GetFileName(file)));
                return;
            }
            int logged = 0;
            foreach (ICompilerError err in errors)
            {
                p.Message("&W{1} - {0}", err.ErrorText,
                          Compiler.DescribeError(err, srcs, " #" + err.ErrorNumber));
                logged++;
                if (logged >= 5)
                    break;
            }
            if (logged < errors.Count)
                p.Message(" &W.. and {0} more", errors.Count - logged);
            p.Message("&WCompiling failed. See " + Compiler.ERROR_LOG_PATH + " for more detail");
        }
        public virtual void CompileCommand(Player p, string[] paths)
        {
            string cmd = paths[0],
                dstPath = Compiler.CommandDLLPath(cmd);
            for (int i = 0; i < paths.Length; i++)
                paths[i] = Compiler.CommandPath(paths[i]);
            paths = TryDirectory(Compiler.COMMANDS_SOURCE_DIR, cmd, paths);
            Compile(p, "Command", paths, dstPath);
        }
        public string[] TryDirectory(string root, string name, string[] srcPaths)
        {
            if (File.Exists(srcPaths[0]))
                return srcPaths;
            string dir = Path.Combine(root, name);
            return !Directory.Exists(dir) ? srcPaths : FileIO.TryGetFiles(dir, "*" + Compiler.FileExtension);
        }
        public override void Help(Player p)
        {
            p.Message("&T/Compile [command name]");
            p.Message("&HCompiles a .cs file containing a C# command into a DLL");
            p.Message("&H  Compiles from &f{0}", Compiler.CommandPath("&H<name>&f"));
            p.Message("&T/Compile plugin [plugin name]");
            p.Message("&HCompiles a .cs file containing a C# plugin into a DLL");
            p.Message("&H  Compiles from &f{0}", Compiler.PluginPath("&H<name>&f"));
        }
    }
}