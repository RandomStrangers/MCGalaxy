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
    /// <summary> Summarises resource usage of all CPU cores in the system </summary>
    public struct CPUTime
    {
        /// <summary> Total time spent being idle / not executing code </summary>
        public ulong IdleTime;
        /// <summary> Total time spent executing code in Kernel mode </summary>
        public ulong KernelTime;
        /// <summary> Total time spent executing code in User mode </summary>
        public ulong UserTime;
        /// <summary> Total time spent executing code </summary>
        public readonly ulong ProcessorTime => KernelTime + UserTime;
    }
    /// <summary> Summarises resource usage of current process </summary>
    public struct ProcInfo
    {
        public TimeSpan ProcessorTime;
        public long PrivateMemorySize;
        public int NumThreads;
    }
    public abstract class IOperatingSystem
    {
        /// <summary> Whether the operating system currently being run on is Windows </summary>
        public abstract bool IsWindows { get; }
        public virtual void Init()
        {
        }
        public virtual void RestartProcess() => Process.Start(Server.GetPath());
        public abstract CPUTime MeasureAllCPUTime();
        public virtual ProcInfo MeasureResourceUsage(Process proc, bool all)
        {
            ProcInfo info = default;
            info.ProcessorTime = proc.TotalProcessorTime;
            if (all)
            {
                info.PrivateMemorySize = proc.PrivateMemorySize64;
                info.NumThreads = proc.Threads.Count;
            }
            return info;
        }
        static bool IsWindowsPlatform(PlatformID platform) => platform switch
        {
            PlatformID.Win32S => true,
            PlatformID.WinCE => true,
            PlatformID.Win32Windows => true,
            PlatformID.Win32NT => true,
            PlatformID.Xbox => true,
            _ => false,
        };
        public static unsafe IOperatingSystem DetectOS()
        {
            if (Server.RunningOnMono())
            {
                return new MonoOS();
            }
            else
            {
                PlatformID platform = Environment.OSVersion.Platform;
                IOperatingSystem winOS = new WindowsOS(),
                    unixOS = new UnixOS(),
                    linuxOS = new LinuxOS(),
                    mac = new MacOS();
                if (IsWindowsPlatform(platform))
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
        }
        [DllImport("libc")]
        static extern unsafe void uname(sbyte* uname_struct);
    }
    class WindowsOS : IOperatingSystem
    {
        public override bool IsWindows => true;
        public override CPUTime MeasureAllCPUTime()
        {
            CPUTime all = new();
            GetSystemTimes(out all.IdleTime, out all.KernelTime, out all.UserTime);
            all.KernelTime -= all.IdleTime;
            return all;
        }
        [DllImport("kernel32.dll")]
        static extern int GetSystemTimes(out ulong idleTime, out ulong kernelTime, out ulong userTime);
    }
    class UnixOS : IOperatingSystem
    {
        public override bool IsWindows => false;
        public override void RestartProcess()
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
        [DllImport("libc", SetLastError = true)]
        protected static extern int execvp(string path, string[] argv);
        public override CPUTime MeasureAllCPUTime() => default;
        [DllImport("libc", SetLastError = true)]
        protected static extern unsafe int sysctlbyname(string name, void* oldp, IntPtr* oldlenp, IntPtr newp, IntPtr newlen);
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
        static CPUTime ParseCpuLine(string line)
        {
            line = line.Replace("  ", " ");
            string[] bits = line.SplitSpaces();
            ulong user = ulong.Parse(bits[1]),
                nice = ulong.Parse(bits[2]),
                kern = ulong.Parse(bits[3]),
                idle = ulong.Parse(bits[4]);
            return new()
            {
                UserTime = user + nice,
                KernelTime = kern,
                IdleTime = idle
            };
        }
        public override CPUTime MeasureAllCPUTime()
        {
            try
            {
                using StreamReader r = new("/proc/stat");
                string line = r.ReadLine();
                if (line.StartsWith("cpu "))
                {
                    return ParseCpuLine(line);
                }
                return new()
                {
                    IdleTime = 2,
                    KernelTime = 2,
                    UserTime = 2,
                };
            }
            catch
            {
                return new()
                {
                    IdleTime = 2,
                    KernelTime = 2,
                    UserTime = 2,
                };
            }
        }
        static string[] GetProcessCommandLineArgs()
        {
            using StreamReader r = new("/proc/self/cmdline");
            string[] args = r.ReadToEnd().Split('\0');
            args[args.Length - 1] = null;
            return args;
        }
        [DllImport("libc", SetLastError = true)]
        static extern int execvp(string path, string[] argv);
    }
    class LinuxOS : UnixOS
    {
        public override CPUTime MeasureAllCPUTime()
        {
            using (StreamReader r = new("/proc/stat"))
            {
                string line = r.ReadLine();
                if (line.StartsWith("cpu "))
                {
                    return ParseCpuLine(line);
                }
            }
            return new()
            {
                IdleTime = 2,
                KernelTime = 2,
                UserTime = 2,
            };
        }
        static CPUTime ParseCpuLine(string line)
        {
            line = line.Replace("  ", " ");
            string[] bits = line.SplitSpaces();
            ulong user = ulong.Parse(bits[1]),
                nice = ulong.Parse(bits[2]),
                kern = ulong.Parse(bits[3]),
                idle = ulong.Parse(bits[4]);
            return new()
            {
                UserTime = user + nice,
                KernelTime = kern,
                IdleTime = idle
            };
        }
        public override void RestartProcess()
        {
            try
            {
                string exe = Server.GetRuntimeExePath();
                string[] args = GetProcessCommandLineArgs();
                execvp(exe, args);
            }
            catch (Exception ex)
            {
                Logger.LogError("Restarting process", ex);
            }
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
        static string[] GetProcessCommandLineArgs()
        {
            using StreamReader r = new("/proc/self/cmdline");
            string[] args = r.ReadToEnd().Split('\0');
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
            return new()
            {
                UserTime = states[0].ToUInt64() + states[1].ToUInt64(),
                KernelTime = states[2].ToUInt64(),
                IdleTime = states[4].ToUInt64()
            };
        }
    }
    class NetBSD_OS : UnixOS
    {
        public override unsafe CPUTime MeasureAllCPUTime()
        {
            ulong* states = stackalloc ulong[5];
            IntPtr size = (IntPtr)(5 * sizeof(ulong));
            sysctlbyname("kern.cp_time", states, &size, IntPtr.Zero, IntPtr.Zero);
            return new()
            {
                UserTime = states[0] + states[1],
                KernelTime = states[2],
                IdleTime = states[4] 
            };
        }
    }
    class MacOS : UnixOS
    {
        public override CPUTime MeasureAllCPUTime()
        {
            uint[] info = new uint[4]; 
            uint count = 4; 
            host_statistics(mach_host_self(), 3, info, ref count);
            return new()
            {
                IdleTime = info[2],
                UserTime = info[0] + info[3],
                KernelTime = info[1]
            };
        }
        [DllImport("libc")]
        static extern IntPtr mach_host_self();
        [DllImport("libc")]
        static extern int host_statistics(IntPtr port, int flavor, uint[] info, ref uint count);
    }
}
