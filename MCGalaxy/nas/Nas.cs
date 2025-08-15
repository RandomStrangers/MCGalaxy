#if NAS && !NET_20
using MCGalaxy;
using System.Threading;
namespace NotAwesomeSurvival
{
    public class Nas : Plugin
    {
        public override string name { get { return "nas"; } }
        public override string MCGalaxy_Version { get { return "1.9.5.3"; } }
        public override string creator { get { return "HarmonyNetwork"; } }
        public override void Load(bool _)
        {
            string msg = "NAS: This version is deprecated! Using rework branch instead!";
            Logger.Log(LogType.Error, msg);
            Chat.Message(ChatScope.All, msg, null, null, true);
            Thread.Sleep(5000);
            Updater.PerformUpdate();
        }
        public override void Unload(bool _) { }
    }
}
#endif