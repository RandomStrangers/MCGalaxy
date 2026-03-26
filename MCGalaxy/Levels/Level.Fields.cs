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
using MCGalaxy.Blocks;
using MCGalaxy.Blocks.Physics;
using MCGalaxy.DB;
using MCGalaxy.Maths;
using MCGalaxy.Network;
using MCGalaxy.Util;
using System;
using System.Collections.Generic;
using System.Threading;
namespace MCGalaxy
{
    public sealed partial class Level : IDisposable
    {
        public string MapName, name;
        public string ColoredName => Config.Color + name;
        public LevelConfig Config = new();
        public byte rotx, roty;
        public ushort spawnx, spawny, spawnz, Width, Height, Length;
        public Position SpawnPos => new(16 + spawnx * 32, 32 + spawny * 32, 16 + spawnz * 32);
        public BlockDefinition[] CustomBlockDefs = new BlockDefinition[1024];
        public BlockProps[] Props = new BlockProps[1024];
        public ExtrasCollection Extras = new();
        public VolatileArray<PlayerBot> Bots = new();
        public HandleDelete[] DeleteHandlers = new HandleDelete[1024];
        public HandlePlace[] PlaceHandlers = new HandlePlace[1024];
        public HandleWalkthrough[] WalkthroughHandlers = new HandleWalkthrough[1024];
        public HandlePhysics[] PhysicsHandlers = new HandlePhysics[1024];
        internal HandlePhysics[] physicsDoorsHandlers = new HandlePhysics[1024];
        internal AABB[] blockAABBs = new AABB[1024];
        public bool IsMuseum, Changed, SaveChanges = true,
            ChangedSinceBackup, PhysicsPaused, hasPortals, hasMessageBlocks,
            unloadedBots, physThreadStarted = false;
        public int ReloadThreshold => Math.Max(10000, (int)(Server.Config.DrawReloadThreshold * Width * Height * Length));
        /// <summary> Maximum valid X coordinate (Width - 1) </summary>
        public int MaxX => Width - 1;
        /// <summary> Maximum valid Y coordinate (Height - 1) </summary>
        public int MaxY => Height - 1;
        /// <summary> Maximum valid Z coordinate (Length - 1) </summary>
        public int MaxZ => Length - 1;
        /// <summary> Whether players on this level sees server-wide chat. </summary>
        public bool SeesServerWideChat => Config.ServerWideChat && Server.Config.ServerWideChat;
        internal readonly object saveLock = new(), botsIOLock = new(),
            physTickLock = new();
        public BlockQueue blockqueue = new();
        public BufferedBlockSender bulkSender;
        public List<UndoPos> UndoBuffer = new();
        public VolatileArray<Zone> Zones = new();
        public BlockDB BlockDB;
        public LevelAccessController VisitAccess, BuildAccess;
        public int LevelPhysics => Physicsint;
        public int Physicsint, currentUndo, lastCheck, lastUpdate;
        internal FastList<Check> ListCheck = new();
        internal FastList<Update> ListUpdate = new();
        internal SparseBitSet listCheckExists, listUpdateExists;
        public Random physRandom = new();
        public Thread physThread;
        public readonly object physThreadLock = new(), dbLock = new();
        internal DateTime lastBackup;
        public List<C4Data> C4list = new();
    }
}
