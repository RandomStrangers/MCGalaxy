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
using MCGalaxy.Platform;
using MCGalaxy.SQL;
using System;
using System.Diagnostics;
using System.Threading;
namespace MCGalaxy.Commands.Info
{
    public sealed class CmdServerInfo : Command
    {
        public override string Name => "ServerInfo";
        public override string Shortcut => "SInfo";
        public override string Type => CommandTypes.Information;
        public override bool UseableWhenFrozen => true;
        public override CommandAlias[] Aliases => new[] { new CommandAlias("Host"), new("ZAll") };
        public override CommandPerm[] ExtraPerms => new[] { new CommandPerm(LevelPermission.Admin, "can see server host, operating system, CPU and memory usage") };
        public override void Use(Player p, string message)
        {
            p.Message("About &b{0}&S", Server.Config.Name);
            p.Message("  &a{0} &Splayers total. (&a{1} &Sonline, &8{2} banned&S)",
                      Database.CountRows("Players"), PlayerInfo.GetOnlineCanSee(p, p.Rank).Count, Group.BannedRank.Players.Count);
            p.Message("  &a{0} &Slevels total (&a{1} &Sloaded). Currency is &3{2}&S.",
                      LevelInfo.AllMapFiles().Length, LevelInfo.Loaded.Count, Server.Config.Currency);
            TimeSpan up = DateTime.UtcNow - Server.StartTime;
            p.Message("  Been up for &a{0}&S, running &b{1} &a{2}",
                      up.Shorten(true), Server.SoftwareName, Server.Version);
            p.Message("&fhttps://github.com/RandomStrangers/MCGalaxy/tree/NAS");
            int updateInterval = 1000 / Server.Config.PositionUpdateInterval;
            p.Message("  Player positions are updated &a{0} &Stimes/second", updateInterval);
            string owner = Server.Config.OwnerName;
            if (!owner.CaselessEq("Notch") && !owner.CaselessEq("the owner"))
            {
                p.Message("  Owner is &3{0}", owner);
            }
            if (HasExtraPerm(p.Rank, 1))
            {
                OutputResourceUsage(p);
            }
        }
        static DateTime startTime;
        static ProcInfo startUsg;
        static string Get()
        {
            string bitType = " unknown bit type (IntPtr size is " + IntPtr.Size + ")",
                name = "Unix";
            if (IntPtr.Size == 8)
            {
                bitType = " 64-bit";
            }
            else if (IntPtr.Size == 4)
            {
                bitType = " 32-bit";
            }
            else if (IntPtr.Size == 2)
            {
                bitType = " 16-bit";
            }
            IOperatingSystem operatingSystem = IOperatingSystem.DetectOS();
            if (operatingSystem is MonoOS)
            {
                name = "Mono";
            }
            else if (operatingSystem is WindowsOS)
            {
                name = "Windows";
            }
            else if (operatingSystem is MacOS)
            {
                name = "Mac";
            }
            else if (operatingSystem is LinuxOS)
            {
                name = "Linux";
            }
            else if (operatingSystem is FreeBSD_OS)
            {
                name = "FreeBSD";
            }
            else if (operatingSystem is NetBSD_OS)
            {
                name = "NetBSD";
            }
            else if (operatingSystem is UnixOS)
            {
                name = "Unix";
            }
            return name + bitType;
        }
        static void OutputResourceUsage(Player p)
        {
            p.Message("Host: {0}", Environment.MachineName);
            p.Message("OS: {0}", Get());
            Process proc = Process.GetCurrentProcess();
            p.Message("Measuring resource usage...one second");
            IOperatingSystem os = IOperatingSystem.DetectOS();
            if (startTime == default)
            {
                startTime = DateTime.UtcNow;
                startUsg = os.MeasureResourceUsage(proc, false);
            }
            CPUTime allBeg = os.MeasureAllCPUTime();
            ProcInfo begUsg = os.MeasureResourceUsage(proc, false);
            Thread.Sleep(1000);
            ProcInfo endUsg = os.MeasureResourceUsage(proc, true);
            CPUTime allEnd = os.MeasureAllCPUTime();
            p.Message("&a{0}% &SCPU usage now, &a{1}% &Soverall",
                MeasureCPU(begUsg.ProcessorTime, endUsg.ProcessorTime, TimeSpan.FromSeconds(1)),
                MeasureCPU(startUsg.ProcessorTime, endUsg.ProcessorTime, DateTime.UtcNow - startTime));
            ulong idl = allEnd.IdleTime - allBeg.IdleTime,
                sys = allEnd.ProcessorTime - allBeg.ProcessorTime;
            double cpu = sys * 100.0 / (sys + idl);
            int cores = Environment.ProcessorCount;
            p.Message("  &a{0}% &Sby all processes across {1} CPU core{2}",
                double.IsNaN(cpu) ? "(unknown)" : cpu.ToString("F2"),
                cores, cores.Plural());
            int memory = (int)Math.Round(endUsg.PrivateMemorySize / 1048576.0);
            p.Message("&a{0} &Sthreads, using &a{1} &Smegabytes of memory",
                endUsg.NumThreads, memory);
        }
        static string MeasureCPU(TimeSpan beg, TimeSpan end, TimeSpan interval)
        {
            if (end < beg)
            {
                return "0.00";
            }
            int cores = Math.Max(1, Environment.ProcessorCount);
            TimeSpan used = end - beg;
            double elapsed = 100.0 * (used.TotalSeconds / interval.TotalSeconds);
            return (elapsed / cores).ToString("F2");
        }
        public override void Help(Player p) => p.Message("&T/ServerInfo &H- Displays the server information.");
    }
}
