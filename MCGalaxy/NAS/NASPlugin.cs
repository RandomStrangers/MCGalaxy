using MCGalaxy.DB;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Events.ServerEvents;
using System;
using System.Collections.Generic;
using System.IO;
namespace MCGalaxy
{
    public partial class NASPlugin : Plugin
    {
        public override string Name => "NAS";
        static readonly List<string> Devs = new()
        {
            "JuneSolis",
        };
        public const string Path = "NAS/";
        static bool firstEverPluginLoad = false;
        static readonly Command[] Commands = new Command[]
        {
            new CmdBarrelMode(),
            new CmdGravestones(),
            new CmdMyGravestones(),
            new CmdNASSpawn(),
            new CmdPVP(),
            new CmdSpawnDungeon(),
        };
        static void EnsureNASFilesExist()
        {
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/selectorColors.png", Path + "selectorColors.png");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/terrain.png", Path + "terrain.png");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/effects/breakdust.properties", NASEffect.Path + "breakdust.properties");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/effects/breakleaf.properties", NASEffect.Path + "breakleaf.properties");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/effects/breakmeter.properties", NASEffect.Path + "breakmeter.properties");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Uploads/global.json", "blockdefs/global.json");
            EnsureFileExists("https://github.com/RandomStrangers/MCGalaxy/raw/NAS/Newtonsoft.Json.dll", "Newtonsoft.Json.dll");
        }
        public override void Load(bool _)
        {
            EnsureDirectoriesExists(Path, NASPlayer.Path,
                NASTimeCycle.Path, NASEffect.Path,
                NASLevel.Path, NASBlock.Path,
                NASPlayer.DeathsPath, "blockprops",
                "blockdefs", "text");
            EnsureNASFilesExist();
            if (File.Exists(Path + "loaded.txt"))
            {
                firstEverPluginLoad = false;
            }
            else
            {
                firstEverPluginLoad = true;
                FileIO.TryWriteAllText(Path + "loaded.txt", "Do not delete this file unless you are using the plugin for the first time!");
            }
            if (firstEverPluginLoad)
            {
                LoadFirstTime();
            }
            Updater.Setup();
            OnlineStat.Stats.Add(PvP);
            OnlineStat.Stats.Add(Kills);
            OnlineStat.Stats.Add(Dev);
            OfflineStat.Stats.Add(Dev);
            Register(Commands);
            NASPlayer.Register();
            NASBlock.Setup();
            if (!NASEffect.Setup())
            {
                Log("NAS: FAILED to load plugin. Please report this to randomstrangers on Discord!");
            }
            else
            {
                if (!NASBlockChange.Setup())
                {
                    Log("NAS: FAILED to load plugin. Please report this to randomstrangers on Discord!");
                }
                else
                {
                    NASItemProp.Setup();
                    Crafting.Setup();
                    if (!NASColor.Setup())
                    {
                        Log("NAS: FAILED to load plugin. Please report this to randomstrangers on Discord!");
                    }
                    else
                    {
                        Collision.Setup();
                        OnPlayerConnectEvent.Register(OnPlayerConnect, 2);
                        OnPlayerClickEvent.Register(OnPlayerClick, 2);
                        OnBlockChangingEvent.Register(NASBlockChange.PlaceBlock, 2);
                        OnBlockChangedEvent.Register(NASBlockChange.OnBlockChanged, 2);
                        OnPlayerMoveEvent.Register(OnPlayerMove, 2);
                        OnPlayerChatEvent.Register(OnPlayerMessage, 1);
                        OnPlayerDisconnectEvent.Register(OnPlayerDisconnect, 0);
                        OnPlayerCommandEvent.Register(OnPlayerCommand, 2);
                        OnShuttingDownEvent.Register(OnShutdown, 0);
                        OnSentMapEvent.Register(HandleSentMap, 3);
                        NASGen.Setup();
                        NASLevel.Setup();
                        NASTimeCycle.Setup();
                        if (firstEverPluginLoad)
                        {
                            GenLevel();
                        }
                    }
                }
            }
        }
        void PvP(Player p, Player target)
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
        void Dev(Player p, Player target)
        {
            if (Devs.CaselessContains(target.truename))
            {
                p.Message("&S  {0} developer.", Name.Capitalize());
            }
        }
        void Dev(Player p, PlayerData target)
        {
            if (Devs.CaselessContains(target.Name))
            {
                p.Message("&S  {0} developer.", Name.Capitalize());
            }
        }
        void Kills(Player p, Player target) => p.Message("&S  " + target.Pronouns.Subject.Capitalize() + " " + target.Pronouns.PresentPerfectVerb + " " + NASPlayer.GetPlayer(target).kills + " kills.");
        public override void Unload(bool shutdown)
        {
            Chat.MessageAll("Attempting to unload NAS.");
            if (!shutdown)
            {
                throw new InvalidOperationException("You cannot unload NAS manually, it can only be unloaded on server shutdown.");
            }
            else
            {
                Updater.TakeDown();
                NASPlayer.Unregister();
                NASColor.TakeDown();
                Command.Unregister(Commands);
                OnlineStat.Stats.Remove(PvP);
                OnlineStat.Stats.Remove(Kills);
                OnlineStat.Stats.Remove(Dev);
                OfflineStat.Stats.Remove(Dev);
                OnPlayerConnectEvent.Unregister(OnPlayerConnect);
                OnPlayerClickEvent.Unregister(OnPlayerClick);
                OnBlockChangingEvent.Unregister(NASBlockChange.PlaceBlock);
                OnBlockChangedEvent.Unregister(NASBlockChange.OnBlockChanged);
                OnPlayerMoveEvent.Unregister(OnPlayerMove);
                OnPlayerDisconnectEvent.Unregister(OnPlayerDisconnect);
                OnJoinedLevelEvent.Register(OnLevelJoined, 2);
                OnPlayerCommandEvent.Unregister(OnPlayerCommand);
                OnShuttingDownEvent.Unregister(OnShutdown);
                OnSentMapEvent.Unregister(HandleSentMap);
                NASLevel.TakeDown();
                NASTimeCycle.TakeDown();
                NASGen.TakeDown();
            }
        }
    }
}
