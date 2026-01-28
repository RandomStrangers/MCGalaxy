#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using System;
using System.Net;
namespace NotAwesomeSurvival
{
    public static class NASUpdater
    {
        public static string BaseURL = "https://raw.githubusercontent.com/RandomStrangers/MCGalaxy/nas-rework/Uploads/",
            ActionsURL = "https://github.com/RandomStrangers/MCGalaxy/actions",
            CurrentVersionURL = BaseURL + "nas_version.txt",
            SourceURL = "https://github.com/RandomStrangers/MCGalaxy/tree/nas-rework", 
            UploadsURL = "https://github.com/RandomStrangers/MCGalaxy/tree/nas-rework/Uploads",
            DLL = BaseURL + "MCGalaxy_nas", NetVer, GUI, CLI, Latest;
        public static Command UpdateCommand = new CmdNASUpdate();
        static event EventHandler NewerVersionDetected;
        static SchedulerTask UpdateTask;
        static Scheduler updateScheduler;
        public static bool SetupDone = false;
        public static void UpdaterTask(SchedulerTask task)
        {
            if (!SetupDone)
            {
                Setup();
            }
            UpdateCheck();
            task.Delay = TimeSpan.FromHours(2);
        }
        public static void UpdateCheck()
        {
            if (!SetupDone)
            {
                Setup();
            }
            for (int retry = 0; retry < 10; retry++)
            {
                try
                {
                    if (!NeedsUpdating())
                    {
                        Logger.Log(LogType.SystemActivity, "No NAS update found!");
                    }
                    else
                    {
                        NewerVersionDetected?.Invoke(null, EventArgs.Empty);
                    }
                }
                catch (WebException ex)
                {
                    bool canRetry = Nas.HandleErrorResponse(ex, SourceURL, retry);
                    HttpUtil.DisposeErrorResponse(ex);
                    if (!canRetry)
                    {
                        updateScheduler ??= new("NASUpdater");
                        updateScheduler.Cancel(UpdateTask);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error checking for NAS updates", ex);
                }
            }
        }
        public static bool NeedsUpdating()
        {
            if (!SetupDone)
            {
                Setup();
            }
            using WebClient client = new Nas.CustomWebClient();
            Latest = client.DownloadString(CurrentVersionURL);
            Version l = new(Latest), v = new(Nas.NasVersion);
            return l > v;
        }
        public static void Setup()
        {
            if (!SetupDone)
            {
                if (Environment.Version.Major == 2)
                {
                    NetVer = "net20";
                    DLL += "_net20";
                }
                else if (Environment.Version.Major == 4)
                {
                    NetVer = "net40";
                }
                else
                {
                    NetVer = "unknown";
                }
                if (!NetVer.CaselessEq("unknown"))
                {
                    CLI = "https://cdn.classicube.net/client/mcg/latest/" + NetVer + "/MCGalaxyCLI.exe";
                    GUI = "https://cdn.classicube.net/client/mcg/latest/" + NetVer + "/MCGalaxy.exe";
                    DLL += ".dll";
                    Command.Register(UpdateCommand);
                    updateScheduler ??= new("NASUpdater");
                    UpdateTask = updateScheduler.QueueRepeat(UpdaterTask, null, TimeSpan.FromSeconds(10));
                    NewerVersionDetected += LogNewerVersionDetected;
                }
                SetupDone = true;
            }
        }
        public static void TakeDown()
        {
            if (!SetupDone)
            {
                Setup();
            }
            if (!NetVer.CaselessEq("unknown"))
            {
                updateScheduler ??= new("NASUpdater");
                updateScheduler.Cancel(UpdateTask);
                Command.Unregister(UpdateCommand);
                NewerVersionDetected -= LogNewerVersionDetected;
            }
        }
        public static void LogNewerVersionDetected(object sender, EventArgs e)
        {
            Logger.Log(LogType.SystemActivity, "&cNAS update available! Update by using /{0}.", UpdateCommand.name);
        }
        public static void PerformUpdate()
        {
            if (!SetupDone)
            {
                Setup();
            }
            if (NetVer.CaselessContains("unknown"))
            {
                Logger.Log(LogType.Warning, "Plase update NAS by downloading the needed artifact from {0}.", ActionsURL);
                return;
            }
            try
            {
                try
                {
                    DeleteFiles("Changelog.txt", "MCGalaxy_.update", "MCGalaxy.update", "MCGalaxyCLI.update",
                                "prev_MCGalaxy_.dll", "prev_MCGalaxy.exe", "prev_MCGalaxyCLI.exe");
                }
                catch
                {
                }
                Logger.Log(LogType.SystemActivity, "Downloading NAS update files");
                WebClient client = new Nas.CustomWebClient();
                DownloadFile(client, DLL, "MCGalaxy_.update");
                if (!Server.RunningOnMono())
                {
                    DownloadFile(client, GUI, "MCGalaxy.update");
                }
                DownloadFile(client, CLI, "MCGalaxyCLI.update");
                Server.SaveAllLevels();
                Player[] players = PlayerInfo.Online.Items;
                foreach (Player pl in players)
                {
                    pl.SaveStats();
                }
                string serverDLL = Server.GetServerDLLPath(),
                    serverGUI = "MCGalaxy.exe",
                    serverCLI = "MCGalaxyCLI.exe";
                FileIO.TryMove(serverDLL, "prev_MCGalaxy_.dll");
                FileIO.TryMove(serverGUI, "prev_MCGalaxy.exe");
                FileIO.TryMove(serverCLI, "prev_MCGalaxyCLI.exe");
                FileIO.TryMove("MCGalaxy_.update", serverDLL);
                if (!Server.RunningOnMono())
                {
                    FileIO.TryMove("MCGalaxy.update", serverGUI);
                }
                FileIO.TryMove("MCGalaxyCLI.update", serverCLI);
                Server.Stop(true, "Updating server.");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error performing update", ex);
            }
        }
        public static void DownloadFile(WebClient client, string url, string dst)
        {
            Logger.Log(LogType.SystemActivity, "Downloading {0} to {1}",
                       url, System.IO.Path.GetFileName(dst));
            client.DownloadFile(url, dst);
        }
        public static void DeleteFiles(params string[] paths)
        {
            foreach (string path in paths)
            {
                FileIO.TryDelete(path);
            }
        }
    }
    public class CmdNASUpdate : Command
    {
        public override string name { get { return "NASUpdate"; } }
        public override string type { get { return CommandTypes.Moderation; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Owner; } }
        public override void Use(Player p, string message)
        {
            if (!NASUpdater.SetupDone)
            {
                NASUpdater.Setup();
            }
            if (message.CaselessEq("check"))
            {
                p.Message("Checking for updates..");
                bool needsUpdating = NASUpdater.NeedsUpdating();
                p.Message("NAS {0}", needsUpdating ? "&cneeds updating" : "&ais up to date");
                if (needsUpdating)
                {
                    if (!string.IsNullOrEmpty(NASUpdater.Latest))
                    {
                        p.Message("Current NAS version: {0}.", Nas.NasVersion);
                        p.Message("Latest NAS version: {0}.", NASUpdater.Latest);
                    }
                }
            }
            else if (message.CaselessEq("latest"))
            {
                if (NASUpdater.NetVer.CaselessContains("unknown"))
                {
                    if (!p.IsSuper)
                    {
                        p.Message("Cannot update NAS using /NASUpdate, see logs for more info.");
                    }
                    else
                    {
                        p.Message("Cannot update NAS using /NASUpdate.");
                    }
                    Logger.Log(LogType.Warning, "Plase update NAS by downloading the needed artifact from {0}.", NASUpdater.ActionsURL);
                    return;
                }
                NASUpdater.PerformUpdate();
            }
            else
            {
                Help(p);
            }
        }
        public override void Help(Player p)
        {
            p.Message("&T/NASUpdate check");
            p.Message("&HChecks whether NAS needs updating");
            p.Message("&T/NASUpdate latest");
            p.Message("&HUpdates the server to the latest NAS build");
        }
    }
}
#endif