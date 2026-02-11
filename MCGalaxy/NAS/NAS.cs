using MCGalaxy.DB;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
using System;
using System.Collections.Generic;
using System.IO;
namespace MCGalaxy
{
    public partial class NAS
    {
        public static List<string> Devs = new()
        {
            "JuneSolis",
        };
        public const string PlayerKey = "NAS_NASPlayer",
            Path = "NAS/";
        static bool firstEverLoad = false;
        public static Command[] Commands = new Command[]
        {
            new CmdBarrelMode(),
            new CmdGravestones(),
            new CmdMyGravestones(),
            new CmdNASSpawn(),
            new CmdPVP(),
            new CmdSpawnDungeon(),
        };
        public static void EnsureNASFilesExist()
        {
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/selectorColors.png", Path + "selectorColors.png");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/terrain.png", Path + "terrain.png");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/effects/breakdust.properties", NASEffect.Path + "breakdust.properties");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/effects/breakleaf.properties", NASEffect.Path + "breakleaf.properties");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/effects/breakmeter.properties", NASEffect.Path + "breakmeter.properties");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/global.json", "blockdefs/global.json");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Newtonsoft.Json.dll", "Newtonsoft.Json.dll");
        }
        public static void Load()
        {
            EnsureDirectoriesExist(Path, NASPlayer.Path,
                NASTimeCycle.Path, NASEffect.Path,
                NASLevel.Path, NASBlock.Path,
                NASPlayer.DeathsPath, "blockprops",
                "blockdefs", "text");
            EnsureNASFilesExist();
            if (!File.Exists("Newtonsoft.Json.dll"))
            {
                Log("NAS: FAILED to load. Could not find Newtonsoft.Json.dll");
                return;
            }
            if (!File.Exists("NAS/Loaded.txt"))
            {
                firstEverLoad = true;
                FileIO.TryWriteAllText("NAS/Loaded.txt", "Do not delete this file unless you are using NAS for the first time!");
            }
            if (firstEverLoad)
            {
                LoadFirstTime();
            }
            OnlineStat.Stats.Add(PvP);
            OnlineStat.Stats.Add(Kills);
            OnlineStat.Stats.Add(Dev);
            OfflineStat.Stats.Add(Dev);
            Register(Commands);
            NASPlayer.Register();
            NASBlock.Setup();
            if (!NASEffect.Setup())
            {
                Log("NAS: FAILED to load. Please report this to randomstrangers on Discord!");
                return;
            }
            if (!NASBlockChange.Setup())
            {
                Log("NAS: FAILED to load. Please report this to randomstrangers on Discord!");
                return;
            }
            NASItemProp.Setup();
            NASCrafting.Setup();
            if (!NASColor.Setup())
            {
                Log("NAS: FAILED to load. Please report this to randomstrangers on Discord!");
                return;
            }
            NASCollision.Setup();
            OnPlayerConnectEvent.Register(OnPlayerConnect, Priority.High);
            OnPlayerClickEvent.Register(OnPlayerClick, Priority.High);
            OnBlockChangingEvent.Register(OnBlockChanging, Priority.High);
            OnBlockChangedEvent.Register(OnBlockChanged, Priority.High);
            OnPlayerMoveEvent.Register(OnPlayerMove, Priority.High);
            OnPlayerChatEvent.Register(OnPlayerMessage, Priority.Normal);
            OnPlayerDisconnectEvent.Register(OnPlayerDisconnect, Priority.Low);
            OnPlayerCommandEvent.Register(OnPlayerCommand, Priority.High);
            OnShuttingDownEvent.Register(OnShutdown, Priority.Low);
            OnSentMapEvent.Register(HandleSentMap, Priority.Critical);
            NASGen.Setup();
            NASLevel.Setup();
            NASTimeCycle.Setup();
            if (firstEverLoad)
            {
                GenLevel();
            }
            Logger.Log(LogType.SystemActivity, "NAS loaded.");
        }
        public static void PvP(Player p, Player target)
        {
            if (NASPlayer.GetPlayer(target).pvpEnabled)
            {
                p.Message("&S  " + target.Pronouns.Subject.Capitalize() + " " + target.Pronouns.PresentPerfectVerb + " PVP &2enabled&S.");
            }
            else
            {
                p.Message("&S  " + target.Pronouns.Subject.Capitalize() + " " + target.Pronouns.PresentPerfectVerb + " PVP &cdisabled&S.");
            }
        }
        public static void Dev(Player p, Player target)
        {
            if (Devs.CaselessContains(target.truename))
            {
                p.Message("&S  NAS developer.");
            }
        }
        public static void Dev(Player p, PlayerData target)
        {
            if (Devs.CaselessContains(target.Name))
            {
                p.Message("&S  NAS developer.");
            }
        }
        public static void Kills(Player p, Player target) => p.Message("&S  " + target.Pronouns.Subject.Capitalize() + " " + target.Pronouns.PresentPerfectVerb + " " + NASPlayer.GetPlayer(target).kills + " kills.");
        public static void Unload()
        {
            Chat.MessageAll("Attempting to unload NAS.");
            if (!Server.shuttingDown)
            {
                throw new InvalidOperationException("You cannot unload NAS manually, it can only be unloaded on server shutdown.");
            }
            NASPlayer.Unregister();
            NASColor.TakeDown();
            Command.Unregister(Commands);
            OnlineStat.Stats.Remove(PvP);
            OnlineStat.Stats.Remove(Kills);
            OnlineStat.Stats.Remove(Dev);
            OfflineStat.Stats.Remove(Dev);
            OnPlayerConnectEvent.Unregister(OnPlayerConnect);
            OnPlayerClickEvent.Unregister(OnPlayerClick);
            OnBlockChangingEvent.Unregister(OnBlockChanging);
            OnBlockChangedEvent.Unregister(OnBlockChanged);
            OnPlayerMoveEvent.Unregister(OnPlayerMove);
            OnPlayerDisconnectEvent.Unregister(OnPlayerDisconnect);
            OnJoinedLevelEvent.Register(OnLevelJoined, Priority.High);
            OnPlayerCommandEvent.Unregister(OnPlayerCommand);
            OnShuttingDownEvent.Unregister(OnShutdown);
            OnSentMapEvent.Unregister(HandleSentMap);
            NASLevel.TakeDown();
            NASTimeCycle.TakeDown();
            NASGen.TakeDown();
        }
    }
}
