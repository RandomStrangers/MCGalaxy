/*
Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
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
using System.Net;
using MCGalaxy.Drawing;
using MCGalaxy.Drawing.Brushes;
using MCGalaxy.Drawing.Transforms;
using MCGalaxy.Events.PlayerEvents;
using MCGalaxy.Games;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Tasks;
using MCGalaxy.Undo;
namespace MCGalaxy
{
    public partial class Player : IDisposable
    {
        bool gotSQLData;
        public PlayerIgnores Ignores = new();
        public static string lastMSG = "";
        internal PersistentMessages persistentMessages = new();
        public Zone ZoneIn;
        public CinematicGui CinematicGui = new();
        internal bool Request;
        internal string senderName = "", currentTpa = "";
        public string truename, afkMessage, BrushName = Brush.DefaultBrush,
            DefaultBrushArgs = "", name, DisplayName,
            prefix = "", title = "", titlecolor = "",
            ip, color, SuperName, whisperTo = "",
            following = "", possess = "",
            prevMsg = "", PreTeleportMap, summonedMap,
            VerifiedVia, lastCMD = "";
        public INetSocket Socket;
        public IGameSession Session;
        public EntityList EntityList;
        public DateTime LastAction, AFKCooldown, 
            NextReviewTime, NextEat, NextTeamInvite,
            SessionStartTime, FirstLogin, LastLogin, lastCmdTime,
            drownTime = DateTime.MaxValue, deathCooldown, LastPatrol;
        public bool IsAfk, AutoAfk, cmdTimer, UsingWom,
            hidden, painting, checkingBotInfo,
            muted, agreed = true, invincible, hasreadrules, hackrank,
            deleteMode, ignoreGrief,
            parseEmotes = Server.Config.ParseEmotes,
            opchat, adminchat, whisper, ClickToMark = true,
            trainGrab, onTrain, trainInvincible,
            frozen, staticCommands, isFlying, aiming,
            joker, Unverified, verifiedPass, voice, useCheckpointSpawn,
            voted, flipHead, infected, showPortals, showMBs,
            Loading = true, cancelcommand, cancelchat, cancellogin,
            cancelconnecting, loggedIn, verifiedName,
            possessed, AllowBuild = true;
        public Transform Transform = Transform.DefaultTransform;
        public Pronouns Pronouns => pronounsList[0];
        internal List<Pronouns> pronounsList = new() { Pronouns.Default };
        public IPAddress IP;
        public Group group;
        public LevelPermission hideRank = LevelPermission.Banned;
        public float ReachDistance = 5;
        public readonly bool IsSuper;
        public bool IsConsole => this == Console;
        public virtual string FullName => color + prefix + DisplayName;
        public string ColoredName => color + DisplayName;
        public string GroupPrefix => group.Prefix.Length == 0 ? "" : "&f" + group.Prefix;
        string partialMessage = "";
        int mbRecursion;
        public int money, TimesVisited, TimesBeenKicked, TimesDied, 
            TotalMessagesSent, lastCheckpointIndex = -1, DatabaseID, 
            CurrentCopySlot, passtries, warn;
        public long TotalModified, TotalDrawn, TotalPlaced, TotalDeleted;
        long startModified;
        public long SessionModified => TotalModified - startModified;
        DateTime startTime;
        public TimeSpan TotalTime
        {
            get { return DateTime.UtcNow - startTime; }
            set { startTime = DateTime.UtcNow.Subtract(value); }
        }
        internal DateTime lastAccessStatus, cmdUnblocked;
        public VolatileArray<SchedulerTask> CriticalTasks;
        public Weapon weapon;
        internal BufferedBlockSender weaponBuffer;
        public CommandData DefaultCmdData
        {
            get
            {
                CommandData data = default;
                data.Rank = Rank; 
                return data;
            }
        }
        public ushort ModeBlock = 0xff, ClientHeldBlock = 1,
            checkpointX, checkpointY, checkpointZ;
        public byte checkpointRotX, checkpointRotY;
        public GameProps Game = new();
        public List<CopyState> CopySlots = new();
        public CopyState CurrentCopy
        {
            get { return CurrentCopySlot >= CopySlots.Count ? null : CopySlots[CurrentCopySlot]; }
            set
            {
                while (CurrentCopySlot >= CopySlots.Count) 
                { 
                    CopySlots.Add(null); 
                }
                CopySlots[CurrentCopySlot] = value;
            }
        }
        internal BlockDefinition gbBlock, lbBlock;
        public VolatileArray<UndoDrawOpEntry> DrawOps = new();
        internal readonly object pendingDrawOpsLock = new();
        internal List<PendingDrawOp> PendingDrawOps = new();
        internal int gbStep, lbStep, oldIndex = -1, lastWalkthrough = -1, 
            startFallY = -1, lastFallY = -1,
            UsingGoto, GeneratingMap, LoadingMuseum;
        public ushort[] BlockBindings = new ushort[1024];
        public Dictionary<string, string> CmdBindings = new(StringComparer.OrdinalIgnoreCase);
        public sbyte c4circuitNumber = -1;
        public Level level;
        public Vec3U16 lastClick = new(0, 0, 0);
        public Position PreTeleportPos;
        public Orientation PreTeleportRot;
        public ExtrasCollection Extras = new();
        readonly SpamChecker spamChecker;
        readonly List<DateTime> partialLog;
        public LevelPermission Rank => group.Permission;
        readonly Queue<SerialCommand> serialCmds = new();
        readonly object serialCmdsLock = new();
        struct SerialCommand 
        { 
            public Command cmd; 
            public string args; 
            public CommandData data; 
        }
        public event SelectionBlockChange Blockchange;
        public void ClearBlockchange() => ClearSelection();
        public object blockchangeObject;
        public delegate bool SelectionHandler(Player p, Vec3S32[] marks, object state, ushort block);
        public delegate void SelectionMarkHandler(Player p, Vec3S32[] marks, int i, object state, ushort block);
    }
}