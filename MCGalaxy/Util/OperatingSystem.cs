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
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
namespace MCGalaxy.Platform
{
    public class CPUTime
    {
        public CPUTime(ulong idle, ulong kern, ulong user)
        {
            IdleTime = idle;
            KernelTime = kern;
            UserTime = user;
        }
        public ulong IdleTime, KernelTime, UserTime;
        public ulong ProcessorTime => KernelTime + UserTime;
    }
    public class ProcInfo
    {
        public ProcInfo(TimeSpan processorTime) 
        {
            ProcessorTime = processorTime;
        }
        public TimeSpan ProcessorTime;
        public long PrivateMemorySize;
        public int NumThreads;
    }
    public abstract class IOperatingSystem
    {
        public abstract bool IsWindows { get; }
        public virtual void RestartProcess()
        {
            string runtime = Server.GetRuntimeExePath(),
                exePath = Server.GetPath();
            execvp(runtime, new string[] { runtime, exePath, null });
            Console.Out.WriteLine("execvp {0} failed: {1}", runtime, Marshal.GetLastWin32Error());
            if (Server.RunningOnMono())
            {
                execvp("mono", new string[] { "mono", exePath, null });
                Console.Out.WriteLine("execvp mono failed: {0}", Marshal.GetLastWin32Error());
            }
        }
        public abstract CPUTime MeasureAllCPUTime();
        public virtual ProcInfo MeasureResourceUsage(Process proc, bool all)
        {
            ProcInfo info = new(proc.TotalProcessorTime);
            if (all)
            {
                info.PrivateMemorySize = proc.PrivateMemorySize64;
                info.NumThreads = proc.Threads.Count;
            }
            return info;
        }
        public static unsafe IOperatingSystem DetectOS()
        {
            if (Server.RunningOnMono())
            {
                return new MonoOS();
            }
            PlatformID platform = Environment.OSVersion.Platform;
            IOperatingSystem winOS = new WindowsOS(),
                unixOS = new UnixOS(),
                linuxOS = new LinuxOS(),
                mac = new MacOS();
            static bool IsWin(PlatformID platform) => platform switch
            {
                PlatformID.Win32S => true,
                PlatformID.WinCE => true,
                PlatformID.Win32Windows => true,
                PlatformID.Win32NT => true,
                PlatformID.Xbox => true,
                _ => false,
            };
            if (IsWin(platform))
            {
                return winOS;
            }
            else if (platform == PlatformID.MacOSX)
            {
                return mac;
            }
            else
            {
                sbyte* utsname = stackalloc sbyte[8192];
                uname(utsname);
                string kernel = new(utsname);
                if (kernel.CaselessContains("linux"))
                {
                    return linuxOS;
                }
                else if (kernel.CaselessContains("freeBSD"))
                {
                    return new FreeBSD_OS();
                }
                else if (kernel.CaselessContains("netBSD"))
                {
                    return new NetBSD_OS();
                }
                else if (kernel.CaselessContains("darwin"))
                {
                    return mac;
                }
                else
                {
                    return unixOS;
                }
            }
        }
        [DllImport("libc", SetLastError = true)]
        public static extern int execvp(string path, string[] argv);
        [DllImport("libc")]
        static extern unsafe void uname(sbyte* uname_struct);
    }
    class WindowsOS : IOperatingSystem
    {
        public override bool IsWindows => true;
        public override void RestartProcess() => Process.Start(Server.GetPath());
        public override CPUTime MeasureAllCPUTime()
        {
            CPUTime all = new(2, 2, 2);
            GetSystemTimes(out all.IdleTime, out all.KernelTime, out all.UserTime);
            all.KernelTime -= all.IdleTime;
            return all;
        }
        [DllImport("kernel32.dll")]
        static extern int GetSystemTimes(out ulong idleTime, out ulong kernelTime, out ulong userTime);
    }
    public class MonoOS : IOperatingSystem
    {
        public override bool IsWindows => false;
        public override void RestartProcess()
        {
            try
            {
                execvp(Server.GetRuntimeExePath(), GetProcessCommandLineArgs());
            }
            catch (Exception ex)
            {
                Logger.LogError("Restarting process", ex);
            }
            execvp("mono", new string[] { "mono", Server.GetPath(), null });
            Console.Out.WriteLine("execvp mono failed: {0}", Marshal.GetLastWin32Error());
        }
        public override CPUTime MeasureAllCPUTime()
        {
            try
            {
                string line = new StreamReader("/proc/stat").ReadLine();
                if (line.StartsWith("cpu "))
                {
                    string[] bits = line.Replace("  ", " ").SplitSpaces();
                    return new(ulong.Parse(bits[4]), ulong.Parse(bits[3]), ulong.Parse(bits[1]) + ulong.Parse(bits[2]));
                }
                return new(2, 2, 2);
            }
            catch
            {
                return new(2, 2, 2);
            }
        }
        static string[] GetProcessCommandLineArgs()
        {
            string[] args = new StreamReader("/proc/self/cmdline").ReadToEnd().Split('\0');
            args[args.Length - 1] = null;
            return args;
        }
    }
    class UnixOS : IOperatingSystem
    {
        public override bool IsWindows => false;
        public override CPUTime MeasureAllCPUTime() => new(2, 2, 2);
    }
    class LinuxOS : UnixOS
    {
        public override CPUTime MeasureAllCPUTime()
        {
            string line = new StreamReader("/proc/stat").ReadLine();
            if (line.StartsWith("cpu "))
            {
                string[] bits = line.Replace("  ", " ").SplitSpaces();
                return new(ulong.Parse(bits[4]), ulong.Parse(bits[3]), ulong.Parse(bits[1]) + ulong.Parse(bits[2]));
            }
            return new(2, 2, 2);
        }
        public override void RestartProcess()
        {
            try
            {
                execvp(Server.GetRuntimeExePath(), GetProcessCommandLineArgs());
            }
            catch (Exception ex)
            {
                Logger.LogError("Restarting process", ex);
            }
        }
        static string[] GetProcessCommandLineArgs()
        {
            string[] args = new StreamReader("/proc/self/cmdline").ReadToEnd().Split('\0');
            args[args.Length - 1] = null;
            return args;
        }
    }
    class FreeBSD_OS : UnixOS
    {
        public override unsafe CPUTime MeasureAllCPUTime()
        {
            UIntPtr* states = stackalloc UIntPtr[5];
            IntPtr size = (IntPtr)(5 * IntPtr.Size);
            sysctlbyname("kern.cp_time", states, &size, IntPtr.Zero, IntPtr.Zero);
            return new(states[4].ToUInt64(), states[2].ToUInt64(), states[0].ToUInt64() + states[1].ToUInt64());
        }
        [DllImport("libc", SetLastError = true)]
        static extern unsafe int sysctlbyname(string name, void* oldp, IntPtr* oldlenp, IntPtr newp, IntPtr newlen);
    }
    class NetBSD_OS : UnixOS
    {
        public override unsafe CPUTime MeasureAllCPUTime()
        {
            ulong* states = stackalloc ulong[5];
            IntPtr size = (IntPtr)(5 * sizeof(ulong));
            sysctlbyname("kern.cp_time", states, &size, IntPtr.Zero, IntPtr.Zero);
            return new(states[4], states[2], states[0] + states[1]);
        }
        [DllImport("libc", SetLastError = true)]
        static extern unsafe int sysctlbyname(string name, void* oldp, IntPtr* oldlenp, IntPtr newp, IntPtr newlen);
    }
    class MacOS : UnixOS
    {
        public override CPUTime MeasureAllCPUTime()
        {
            uint[] info = new uint[4];
            uint count = 4;
            host_statistics(mach_host_self(), 3, info, ref count);
            return new(info[2], info[1], info[0] + info[3]);
        }
        [DllImport("libc")]
        static extern IntPtr mach_host_self();
        [DllImport("libc")]
        static extern int host_statistics(IntPtr port, int flavor, uint[] info, ref uint count);
    }
}