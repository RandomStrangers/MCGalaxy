using MCGalaxy.Network;
using MCGalaxy.Tasks;
using System;
using System.IO;
using System.Net;
namespace MCGalaxy
{
    public static class Updater
    {
        public static string Latest;
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
                        Logger.Log(1, "No NAS update found!");
                    }
                }
                catch (WebException ex)
                {
                    bool canRetry = NASPlugin.HandleErrorResponse(ex, "https://github.com/RandomStrangers/MCGalaxy/tree/NAS", retry);
                    HttpUtil.DisposeErrorResponse(ex);
                    if (!canRetry)
                    {
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
            Latest = new WebClient().DownloadString("https://raw.githubusercontent.com/RandomStrangers/MCGalaxy/NAS/Uploads/version.txt");
            Version l = new(Latest), v = new(Server.Version);
            return l > v;
        }
        public static void Setup()
        {
            if (!SetupDone)
            {
                updateScheduler ??= new("NASUpdater");
                if (Environment.Version.Major == 4)
                {
                    UpdateTask = updateScheduler.QueueRepeat(UpdaterTask, null, TimeSpan.FromSeconds(10));
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
            if (Environment.Version.Major == 4)
            {
                updateScheduler.Cancel(UpdateTask);
            }
        }
        public static void PerformUpdate()
        {
            if (!SetupDone)
            {
                Setup();
            }
            if (Environment.Version.Major == 4)
            {
                try
                {
                    try
                    {
                        FileIO.TryDelete("MCGalaxy.update");
                        FileIO.TryDelete("prev_MCGalaxy.exe");
                    }
                    catch
                    {
                    }
                    Logger.Log(1, "Downloading NAS update files");
                    WebClient client = new();
                    if (!Server.RunningOnMono())
                    {
                        Logger.Log(1, "Downloading {0} to {1}",
                            "https://raw.githubusercontent.com/RandomStrangers/MCGalaxy/NAS/Uploads/MCGalaxy.exe", Path.GetFileName("MCGalaxy.update"));
                        client.DownloadFile("https://raw.githubusercontent.com/RandomStrangers/MCGalaxy/NAS/Uploads/MCGalaxy.exe", "MCGalaxy.update");
                    }
                    Server.SaveAllLevels();
                    Player[] players = PlayerInfo.Online.Items;
                    foreach (Player pl in players)
                    {
                        pl.SaveStats();
                    }
                    FileIO.TryMove("MCGalaxy.exe", "prev_MCGalaxy.exe");
                    FileIO.TryMove("MCGalaxy_update", "MCGalaxy.exe");
                    Server.Stop(true, "Updating server.");
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error performing update", ex);
                }
            }
        }
    }
}
