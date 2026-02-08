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
        public PlayerIgnores Ignores = new();
        public static string lastMSG = "";
        internal PersistentMessages persistentMessages = new();
        public Zone ZoneIn;
        internal bool Request;
        internal string senderName = "", currentTpa = "";
        public string truename;
        public INetSocket Socket;
        public IGameSession Session;
        public EntityList EntityList;
        public DateTime LastAction, AFKCooldown;
        public bool IsAfk, AutoAfk, cmdTimer, UsingWom;
        public string afkMessage;
        public bool ClickToMark = true;
        public string BrushName = Brush.DefaultBrush;
        public Transform Transform = Transform.DefaultTransform;
        public string DefaultBrushArgs = "", name, DisplayName;
        public Pronouns Pronouns => pronounsList[0];
        internal List<Pronouns> pronounsList = new() { Pronouns.Default };
        public int warn;
        public IPAddress IP;
        public string ip, color;
        public Group group;
        public LevelPermission hideRank = LevelPermission.Banned;
        public bool hidden, painting, checkingBotInfo,
            muted, agreed = true, invincible;
        public string prefix = "", title = "", titlecolor = "";
        public int passtries;
        public bool hasreadrules;
        public DateTime NextReviewTime, NextEat, NextTeamInvite;
        public float ReachDistance = 5;
        public bool hackrank;
        public string SuperName;
        public readonly bool IsSuper;
        public bool IsConsole => this == Console;
        public virtual string FullName => color + prefix + DisplayName;
        public string ColoredName => color + DisplayName;
        public string GroupPrefix => group.Prefix.Length == 0 ? "" : "&f" + group.Prefix;
        public bool deleteMode, ignoreGrief,
            parseEmotes = Server.Config.ParseEmotes, 
            opchat, adminchat, whisper;
        public string whisperTo = "";
        string partialMessage = "";
        public bool trainGrab, onTrain, trainInvincible;
        int mbRecursion;
        public bool frozen;
        public string following = "", possess = "";
        public bool possessed, AllowBuild = true;
        public int money;
        public long TotalModified, TotalDrawn, TotalPlaced, TotalDeleted;
        public int TimesVisited, TimesBeenKicked, TimesDied,
            TotalMessagesSent;
        long startModified;
        public long SessionModified => TotalModified - startModified;
        DateTime startTime;
        public TimeSpan TotalTime
        {
            get { return DateTime.UtcNow - startTime; }
            set { startTime = DateTime.UtcNow.Subtract(value); }
        }
        public DateTime SessionStartTime, FirstLogin, LastLogin;
        public bool staticCommands;
        internal DateTime lastAccessStatus;
        public VolatileArray<SchedulerTask> CriticalTasks;
        public bool isFlying, aiming;
        public Weapon weapon;
        internal BufferedBlockSender weaponBuffer;
        public bool joker, Unverified, verifiedPass, voice;
        public CommandData DefaultCmdData
        {
            get
            {
                CommandData data = default;
                data.Rank = Rank; 
                return data;
            }
        }
        public bool useCheckpointSpawn;
        public int lastCheckpointIndex = -1;
        public ushort checkpointX, checkpointY, checkpointZ;
        public byte checkpointRotX, checkpointRotY;
        public bool voted, flipHead, infected;
        public GameProps Game = new();
        public int DatabaseID;
        public List<CopyState> CopySlots = new();
        public int CurrentCopySlot;
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
        internal int gbStep, lbStep;
        internal BlockDefinition gbBlock, lbBlock;
        public VolatileArray<UndoDrawOpEntry> DrawOps = new();
        internal readonly object pendingDrawOpsLock = new();
        internal List<PendingDrawOp> PendingDrawOps = new();
        public bool showPortals, showMBs;
        public string prevMsg = "";
        internal int oldIndex = -1, lastWalkthrough = -1, 
            startFallY = -1, lastFallY = -1;
        public DateTime drownTime = DateTime.MaxValue, deathCooldown;
        public ushort ModeBlock = 0xff, ClientHeldBlock = 1;
        public ushort[] BlockBindings = new ushort[1024];
        public Dictionary<string, string> CmdBindings = new(StringComparer.OrdinalIgnoreCase);
        public string lastCMD = "";
        public DateTime lastCmdTime;
        public sbyte c4circuitNumber = -1;
        public Level level;
        public bool Loading = true;
        internal int UsingGoto, GeneratingMap, LoadingMuseum;
        public Vec3U16 lastClick = Vec3U16.Zero;
        public Position PreTeleportPos;
        public Orientation PreTeleportRot;
        public string PreTeleportMap, summonedMap;
        public ExtrasCollection Extras = new();
        readonly SpamChecker spamChecker;
        internal DateTime cmdUnblocked;
        readonly List<DateTime> partialLog;
        public DateTime LastPatrol;
        public LevelPermission Rank => group.Permission;
        public bool loggedIn, verifiedName;
        public string VerifiedVia;
        bool gotSQLData;
        public bool cancelcommand, cancelchat, 
            cancellogin, cancelconnecting;
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