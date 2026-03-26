/*
    Copyright 2010 MCLawl Team - Written by Valek (Modified by MCGalaxy)
    Edited for use with MCGalaxy
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
namespace MCGalaxy.Modules.Compiling
{
    /// <summary> Exception raised when attempting to load a new command/plugin
    /// that has the same name as an already loaded command/plugin </summary>
    public sealed class AlreadyLoadedException : Exception
    {
        public AlreadyLoadedException(string msg) : base(msg)
        {
        }
    }
    /// <summary> Compiles source code files for a particular programming language into a .dll </summary>
    public class Compiler
    {
        public const string COMMANDS_SOURCE_DIR = "extra/commands/source/",
            PLUGINS_DIR = "plugins/",
            ERROR_LOG_PATH = "logs/errors/compiler.txt",
            COMMANDS_DLL_DIR = "extra/commands/dll/";
        /// <summary> Returns the default .dll path for the custom command with the given name </summary>
        public static string CommandDLLPath(string name) => COMMANDS_DLL_DIR + "Cmd" + name + ".dll";
        public static void Init()
        {
            Directory.CreateDirectory(COMMANDS_DLL_DIR);
            Directory.CreateDirectory(PLUGINS_DIR);
            AppDomain.CurrentDomain.AssemblyResolve += ResolveMissingAssembly;
        }
        public static Assembly ResolveMissingAssembly(object sender, ResolveEventArgs args)
        {
            Assembly source = args.RequestingAssembly;
            Assembly match = ResolvePluginAssembly(source, args.Name);
            if (match != null)
                return match;
            Logger.Log(LogType.Warning, "{0} [{1}] tried to load [{2}], but it could not be found",
                       IsPluginDLL(source) ? "Custom command/plugin" : "Assembly",
                       source == null ? "(unknown)" : source.FullName,
                       args.Name);
            return null;
        }
        public static Assembly ResolvePluginAssembly(Assembly source, string target)
        {
            if (source == null || !IsPluginDLL(source))
                return null;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assem in assemblies)
            {
                if (!IsPluginDLL(assem))
                    continue;
                if (target == assem.FullName)
                    return assem;
            }
            ;
            return null;
        }
        public static bool IsPluginDLL(Assembly a) => string.IsNullOrEmpty(a.Location);
        /// <summary> Constructs instances of all types which derive from T in the given assembly. </summary>
        /// <returns> The list of constructed instances. </returns>
        public static List<T> LoadTypes<T>(Assembly lib)
        {
            List<T> instances = new();
            foreach (Type t in lib.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface || !t.IsSubclassOf(typeof(T)))
                    continue;
                object instance = Activator.CreateInstance(t);
                if (instance == null)
                {
                    Logger.Log(LogType.Warning, "{0} \"{1}\" could not be loaded", typeof(T).Name, t.Name);
                    throw new BadImageFormatException();
                }
                instances.Add((T)instance);
            }
            return instances;
        }
        /// <summary> Loads the given assembly from disc (and associated .pdb debug data) </summary>
        public static Assembly LoadAssembly(string path, bool loadDebug = true)
        {
            if (!FileIO.TryReadBytes(path, out byte[] data))
                return null;
            return loadDebug ? Assembly.Load(data, GetDebugData(path)) : Assembly.Load(data);
        }
        public static byte[] GetDebugData(string path)
        {
            string pdb_path = Path.ChangeExtension(path, ".pdb");
            byte[] bytes;
            try
            {
                FileIO.TryReadBytes(pdb_path, out bytes);
                return bytes;
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading .pdb " + pdb_path, ex);
                return null;
            }
            if (!Server.RunningOnMono())
                return null;
            string mdb_path = path + ".mdb";
            try
            {
                FileIO.TryReadBytes(mdb_path, out bytes);
                return bytes;
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading .mdb " + mdb_path, ex);
            }
            return null;
        }
        public static void AutoloadCommands()
        {
            string[] files = FileIO.TryGetFiles(COMMANDS_DLL_DIR, "*.dll");
            if (files == null)
                return;
            foreach (string path in files)
                AutoloadCommands(path);
        }
        public static void AutoloadCommands(string path)
        {
            List<Command> cmds;
            try
            {
                cmds = LoadCommands(path);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading commands from " + path, ex);
                return;
            }
            Logger.Log(LogType.SystemActivity, "AUTOLOAD: Loaded {0} from {1}",
                       cmds.Join(c => "/" + c.Name), Path.GetFileName(path));
        }
        /// <summary> Loads and registers all the commands from the given .dll path </summary>
        public static List<Command> LoadCommands(string path)
        {
            Assembly lib = LoadAssembly(path);
            List<Command> commands = LoadTypes<Command>(lib);
            if (commands.Count == 0)
                throw new InvalidOperationException("No commands in " + path);
            foreach (Command cmd in commands)
            {
                if (Command.Find(cmd.Name) != null)
                    throw new AlreadyLoadedException("/" + cmd.Name + " is already loaded");
                Command.Register(cmd);
            }
            return commands;
        }
        public static string DescribeLoadError(string path, Exception ex)
        {
            string file = Path.GetFileName(path);
            return ex switch
            {
                BadImageFormatException => "&W" + file + " is not a valid assembly, or has an invalid dependency. Details in the error log.",
                FileLoadException => "&W" + file + " or one of its dependencies could not be loaded. Details in the error log.",
                _ => "&WAn unknown error occured. Details in the error log.",
            };
        }
        public static void AutoloadPlugins()
        {
            string[] files = FileIO.TryGetFiles(PLUGINS_DIR, "*.dll");
            if (files == null)
                return;
            Array.Sort(files);
            foreach (string path in files)
                try
                {
                    LoadPlugin(path, true);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error loading plugins from " + path, ex);
                }
        }
        /// <summary> Loads all plugins from the given .dll path. </summary>
        public static List<Plugin> LoadPlugin(string path, bool auto)
        {
            Assembly lib = LoadAssembly(path);
            List<Plugin> plugins = LoadTypes<Plugin>(lib);
            foreach (Plugin pl in plugins)
            {
                if (Plugin.FindCustom(pl.Name) != null)
                    throw new AlreadyLoadedException("Plugin " + pl.Name + " is already loaded");
                Plugin.Load(pl, auto);
            }
            return plugins;
        }
        public static bool LoadCommands(Player p, string path)
        {
            if (!File.Exists(path))
            {
                p.Message("File &9{0} &Snot found.", path);
                return false;
            }
            try
            {
                List<Command> cmds = LoadCommands(path);
                p.Message("Successfully loaded &T{0}",
                          cmds.Join(c => "/" + c.Name));
                return true;
            }
            catch (AlreadyLoadedException ex)
            {
                p.Message(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                p.Message(DescribeLoadError(path, ex));
                Logger.LogError("Error loading commands from " + path, ex);
                return false;
            }
        }
        public static bool LoadPlugins(Player p, string path)
        {
            if (!File.Exists(path))
            {
                p.Message("File &9{0} &Snot found.", path);
                return false;
            }
            try
            {
                List<Plugin> plugins = LoadPlugin(path, false);
                p.Message("Plugin {0} loaded successfully",
                          plugins.Join(pl => pl.Name));
                return true;
            }
            catch (AlreadyLoadedException ex)
            {
                p.Message(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                p.Message(DescribeLoadError(path, ex));
                Logger.LogError("Error loading plugins from " + path, ex);
                return false;
            }
        }
        public static bool UnloadCommand(Player p, Command cmd)
        {
            if (Command.IsCore(cmd))
            {
                p.Message("&T/{0} &Sis a core command, you cannot unload it.", cmd.Name);
                return false;
            }
            Command.Unregister(cmd);
            p.Message("Command &T/{0} &Sunloaded successfully", cmd.Name);
            return true;
        }
        public static bool UnloadPlugin(Player p, Plugin plugin)
        {
            if (!Plugin.Unload(plugin))
            {
                p.Message("&WError unloading plugin. See error logs for more information.");
                return false;
            }
            p.Message("Plugin {0} &Sunloaded successfully", plugin.Name);
            return true;
        }
        public static string FileExtension = ".cs";
        protected static void AddCoreAssembly(StringBuilder sb)
        {
            if (Server.RunningOnMono())
                return;
            string coreAssemblyFileName = typeof(object).Assembly.Location;
            if (!string.IsNullOrEmpty(coreAssemblyFileName))
            {
                sb.Append("/nostdlib+ ");
                sb.AppendFormat("/R:{0} ", Quote(coreAssemblyFileName));
            }
        }
        protected static void AddReferencedAssemblies(StringBuilder sb, List<string> referenced)
        {
            foreach (string path in referenced)
                sb.AppendFormat("/R:{0} ", Quote(path));
        }
        protected static string GetExecutable()
        {
            string root = RuntimeEnvironment.GetRuntimeDirectory();
            string[] paths = new string[]
            {
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\Roslyn\csc.exe",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe",
                @"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
                Path.Combine(root, "csc.exe"),
                Path.Combine(root, @"../../../bin/mcs"),
                Path.Combine(root, "mcs.exe"),
                "/usr/bin/mcs",
            };
            foreach (string path in paths)
                if (File.Exists(path))
                    return path;
            return paths[0];
        }
        public static ICompilerErrors Compile(string[] srcPaths, string dstPath, List<string> referenced)
        {
            string args = GetCommandLineArguments(srcPaths, dstPath, referenced),
                exe = GetExecutable();
            ICompilerErrors errors = new();
            List<string> output = new();
            if (Compile(exe, args, output) != 0)
                foreach (string line in output)
                    ProcessCompilerOutputLine(errors, line);
            return errors;
        }
        protected static string GetCommandLineArguments(string[] srcPaths, string dstPath,
                                                         List<string> referencedAssemblies)
        {
            StringBuilder sb = new();
            sb.Append("/t:library ");
            sb.Append("/utf8output /noconfig /fullpaths ");
            AddCoreAssembly(sb);
            AddReferencedAssemblies(sb, referencedAssemblies);
            dstPath = Path.GetFullPath(dstPath);
            sb.AppendFormat("/out:{0} ", Quote(dstPath));
            sb.Append("/D:DEBUG /debug+ /optimize- ");
            sb.Append("/warnaserror- /unsafe ");
            foreach (string path in srcPaths)
                sb.AppendFormat("{0} ", Quote(path));
            return sb.ToString();
        }
        protected static string Quote(string value) => "\"" + value.Trim() + "\"";
        public static int Compile(string path, string args, List<string> output)
        {
            ProcessStartInfo psi = CreateStartInfo(path, args);
            using Process p = new();
            p.OutputDataReceived += (s, e) => { if (e.Data != null) output.Add(e.Data); };
            p.ErrorDataReceived += (s, e) => { if (e.Data != null) output.Add(e.Data); };
            p.StartInfo = psi;
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            return !p.WaitForExit(120 * 1000)
                ? throw new InvalidOperationException("C# compiler ran for over two minutes! Giving up..")
                : p.ExitCode;
        }
        protected static ProcessStartInfo CreateStartInfo(string path, string args) => new(path, args)
        {
            WorkingDirectory = Environment.CurrentDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        public static void ProcessCompilerOutputLine(ICompilerErrors errors, string line)
        {
            Match m = new Regex(@"(^(.*)(\(([0-9]+),([0-9]+)\)): )(error|warning) ([A-Z]+[0-9]+) ?: (.*)").Match(line);
            bool full;
            if (m.Success)
                full = true;
            else
            {
                m = new Regex(@"(error|warning) ([A-Z]+[0-9]+) ?: (.*)").Match(line);
                full = false;
            }
            if (!m.Success)
                return;
            ICompilerError ce = new();
            if (full)
            {
                ce.FileName = m.Groups[2].Value;
                ce.Line = NumberUtils.ParseInt32(m.Groups[4].Value);
                ce.Column = NumberUtils.ParseInt32(m.Groups[5].Value);
            }
            ce.IsWarning = m.Groups[full ? 6 : 1].Value.CaselessEq("warning");
            ce.ErrorNumber = m.Groups[full ? 7 : 2].Value;
            ce.ErrorText = m.Groups[full ? 8 : 3].Value;
            errors.Add(ce);
        }
        public static readonly string CommandSkeleton = @"//\tAuto-generated command skeleton class
//\tUse this as a basis for custom MCGalaxy commands
//\tNaming should be kept consistent (e.g. /update command should have a class name of 'CmdUpdate' and a filename of 'CmdUpdate.cs')
// As a note, MCGalaxy is designed for .NET 4.0
// To reference other assemblies, put a ""//reference [assembly filename]"" at the top of the file
//   e.g. to reference the System.Data assembly, put ""//reference System.Data.dll""
// Add any other using statements you need after this
using System;
using MCGalaxy;
public class Cmd{0} : Command
{{
\t// The command's name (what you put after a slash to use this command)
\tpublic override string Name {{ get {{ return ""{0}""; }} }}
\t// Command's shortcut, can be left blank (e.g. ""/Copy"" has a shortcut of ""c"")
\tpublic override string Shortcut {{ get {{ return """"; }} }}
\t// Which submenu this command displays in under /Help
\tpublic override string Type {{ get {{ return ""other""; }} }}
\t// Whether or not this command can be used in a museum. Block/map altering commands should return false to avoid errors.
\tpublic override bool MuseumUsable {{ get {{ return true; }} }}
\t// The default rank required to use this command. Valid values are:
\t//   LevelPermission.Guest, LevelPermission.Builder, LevelPermission.AdvBuilder,
\t//   LevelPermission.Operator, LevelPermission.Admin, LevelPermission.Owner
\tpublic override LevelPermission DefaultRank {{ get {{ return LevelPermission.Guest; }} }}
\t// This is for when a player executes this command by doing /{0}
\t//   p is the player object for the player executing the command.
\t//   message is the arguments given to the command. (e.g. for '/{0} this', message is ""this"")
\tpublic override void Use(Player p, string message)
\t{{
\t\tp.Message(""Hello World!"");
\t}}
\t// This is for when a player does /Help {0}
\tpublic override void Help(Player p)
\t{{
\t\tp.Message(""/{0} - Does stuff. Example command."");
\t}}
}}";
        public static readonly string PluginSkeleton = @"//\tAuto-generated plugin skeleton class
//\tUse this as a basis for custom MCGalaxy plugins
// To reference other assemblies, put a ""//reference [assembly filename]"" at the top of the file
//   e.g. to reference the System.Data assembly, put ""//reference System.Data.dll""
// Add any other using statements you need after this
using System;
namespace MCGalaxy
{{
\tpublic class {0} : Plugin
\t{{
\t\t// The plugin's name (i.e what shows in /Plugins)
\t\tpublic override string Name {{ get {{ return ""{0}""; }} }
\t\t// The oldest version of MCGalaxy this plugin is compatible with
\t\tpublic override string MCGalaxy_Version {{ get {{ return ""{2}""; }} }}
\t\t// Who created/authored this plugin
\t\tpublic override string Creator {{ get {{ return ""{1}""; }} }}
\t\t// Called when this plugin is being loaded (e.g. on server startup)
\t\tpublic override void Load(bool startup)
\t\t{{
\t\t\t//code to hook into events, load state/resources etc goes here
\t\t}}
\t\t// Called when this plugin is being unloaded (e.g. on server shutdown)
\t\tpublic override void Unload(bool shutdown)
\t\t{{
\t\t\t//code to unhook from events, dispose of state/resources etc goes here
\t\t}}
\t\t// Displays help for or information about this plugin
\t\tpublic override void Help(Player p)
\t\t{{
\t\t\tp.Message(""No help is available for this plugin."");
\t\t}}
\t}}
}}";
        public static string CommandPath(string name) => COMMANDS_SOURCE_DIR + "Cmd" + name + FileExtension;
        public static string PluginPath(string name) => PLUGINS_DIR + name + FileExtension;
        public static string PluginDLLPath(string name) => PLUGINS_DIR + name + ".dll";
        public static string FormatSource(string source, params string[] args) => string.Format(source.Replace(@"\t", "\t").Replace("\n", "\r\n"), args);
        /// <summary> Generates source code for an example command,
        /// preformatted with the given command name </summary>
        public static string GenExampleCommand(string cmdName) => FormatSource(CommandSkeleton, cmdName.ToLower().Capitalize());
        /// <summary> Generates source code for an example plugin,
        /// preformatted with the given name and creator </summary>
        public static string GenExamplePlugin(string plugin, string creator) => FormatSource(PluginSkeleton, plugin, creator, Server.InternalVersion);
        /// <summary> Attempts to compile the given source code files to a .dll file. </summary>
        /// <param name="logErrors"> Whether to log compile errors to ERROR_LOG_PATH </param>
        public static ICompilerErrors Compile(string[] srcPaths, string dstPath, bool logErrors)
        {
            ICompilerErrors errors = Compile(srcPaths, dstPath, ProcessInput(srcPaths, "//"));
            if (!errors.HasErrors || !logErrors)
                return errors;
            SourceMap sources = new(srcPaths);
            StringBuilder sb = new();
            sb.AppendLine("############################################################");
            sb.AppendLine("Errors when compiling " + srcPaths.Join());
            sb.AppendLine("############################################################");
            sb.AppendLine();
            foreach (ICompilerError err in errors)
            {
                string type = err.IsWarning ? "Warning" : "Error";
                sb.AppendLine(DescribeError(err, srcPaths, "") + ":");
                if (err.Line > 0)
                    sb.AppendLine(sources.Get(err.FileName, err.Line - 1));
                if (err.Column > 0)
                    sb.Append(' ', err.Column - 1);
                sb.AppendLine("^-- " + type + " #" + err.ErrorNumber + " - " + err.ErrorText);
                sb.AppendLine();
                sb.AppendLine("-------------------------");
                sb.AppendLine();
            }
            using (StreamWriter w = new(ERROR_LOG_PATH, true))
                w.Write(sb.ToString());
            return errors;
        }
        public static string DescribeError(ICompilerError err, string[] srcs, string text) => string.Format("{0}{1}{2}{3}", err.IsWarning ? "Warning" : "Error", text,
                                 err.Line > 0 ? " on line " + err.Line : "",
                                 srcs.Length > 1 ? " in " + Path.GetFileName(err.FileName) : "");
        /// <summary> Converts source file paths to full paths,
        /// then returns list of parsed referenced assemblies </summary>
        protected static List<string> ProcessInput(string[] srcPaths, string commentPrefix)
        {
            List<string> referenced = new();
            for (int i = 0; i < srcPaths.Length; i++)
            {
                string path = Path.GetFullPath(srcPaths[i]);
                AddReferences(path, commentPrefix, referenced);
                srcPaths[i] = path;
            }
            referenced.Add(Server.GetPath());
            return referenced;
        }
        public static void AddReferences(string path, string commentPrefix, List<string> referenced)
        {
            using StreamReader r = new(path);
            string refPrefix = commentPrefix + "reference ",
                plgPrefix = commentPrefix + "pluginref ",
                line;
            while ((line = r.ReadLine()) != null)
            {
                if (line.CaselessStarts(refPrefix))
                    referenced.Add(GetDLL(line));
                else if (line.CaselessStarts(plgPrefix))
                {
                    path = Path.Combine(PLUGINS_DIR, GetDLL(line));
                    referenced.Add(Path.GetFullPath(path));
                }
            }
        }
        protected static string GetDLL(string line) => line.Substring(line.IndexOf(' ') + 1).Replace(";", "");
    }
    public class ICompilerErrors : List<ICompilerError>
    {
        public bool HasErrors => FindIndex(ce => !ce.IsWarning) >= 0;
    }
    public class ICompilerError
    {
        public int Line, Column;
        public string ErrorNumber, ErrorText, FileName;
        public bool IsWarning;
    }
    public class SourceMap
    {
        public readonly string[] files;
        public readonly List<string>[] sources;
        public SourceMap(string[] paths)
        {
            files = paths;
            sources = new List<string>[paths.Length];
        }
        public int FindFile(string file)
        {
            for (int i = 0; i < files.Length; i++)
                if (file.CaselessEq(files[i]))
                    return i;
            return -1;
        }
        /// <summary> Returns the given line in the given source code file </summary>
        public string Get(string file, int line)
        {
            int i = FindFile(file);
            if (i == -1)
                return "";
            List<string> source = sources[i];
            if (source == null)
            {
                try
                {
                    source = Utils.ReadAllLinesList(file);
                }
                catch
                {
                    source = new();
                }
                sources[i] = source;
            }
            return line < source.Count ? source[line] : "";
        }
    }
}