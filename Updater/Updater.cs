using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
namespace MCGalaxyUpdater
{
    public static class Updater
    {
        class CustomWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri address)
            {
                HttpWebRequest req = (HttpWebRequest)base.GetWebRequest(address);
                req.ServicePoint.BindIPEndPointDelegate = BindIPEndPointCallback;
                req.UserAgent = "MCGalaxyUpdater";
                return req;
            }
        }
        public static INetListen Listener = new TcpListen();
        static IPEndPoint BindIPEndPointCallback(ServicePoint servicePoint, IPEndPoint remoteEP, int retryCount)
        {
            IPAddress localIP;
            if (Listener.IP != null)
            {
                localIP = Listener.IP;
            }
            else if (!IPAddress.TryParse("0.0.0.0", out localIP))
            {
                return null;
            }
            if (remoteEP.AddressFamily != localIP.AddressFamily) return null;
            return new IPEndPoint(localIP, 0);
        }

        public static WebClient CreateWebClient() { return new CustomWebClient(); }
        public const string BaseURL = "https://github.com/RandomStrangers/MCGalaxy/raw/master/Uploads/";
        public static string dll = BaseURL + "MCGalaxy_.dll";
        public static string cli = BaseURL + "MCGalaxyCLI.exe";
        public static string exe = BaseURL + "MCGalaxy.exe";

        public static void PerformUpdate()
        {
            try
            {
                try
                {
                    DeleteFiles("MCGalaxy.update", "MCGalaxy_.update", "MCGalaxyCLI.update",
                        "prev_MCGalaxy.exe", "prev_MCGalaxy_.dll", "prev_MCGalaxyCLI.exe");
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error deleting files:");
                    Console.WriteLine(e.ToString());
                    Console.ReadKey(false);
                    return;
                }
                    try
                    {
                        WebClient client = HttpUtil.CreateWebClient();
                        File.Move("MCGalaxy.exe", "prev_MCGalaxy.exe");
                        File.Move("MCGalaxyCLI.exe", "prev_MCGalaxyCLI.exe");
                        File.Move("MCGalaxy_.dll", "prev_MCGalaxy_.dll");
                        client.DownloadFile(dll, "MCGalaxy_.update");
                        client.DownloadFile(cli, "MCGalaxyCLI.update");
                        client.DownloadFile(exe, "MCGalaxy.update");

                }
                catch (Exception x) 
                    {
                        Console.WriteLine("Error downloading update:");
                        Console.WriteLine(x.ToString());
                        Console.ReadKey(false);
                        return;
                    }
                File.Move("MCGalaxy.update", "MCGalaxy.exe");
                File.Move("MCGalaxyCLI.update", "MCGalaxyCLI.exe");
                File.Move("MCGalaxy_.update", "MCGalaxy_.dll");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error performing update:");
                Console.WriteLine(ex.ToString());
                Console.ReadKey(false);
                return;
            }
        }
        static void DeleteFiles(params string[] paths)
        {
            foreach (string path in paths) { AtomicIO.TryDelete(path); }
        }
    }
}
