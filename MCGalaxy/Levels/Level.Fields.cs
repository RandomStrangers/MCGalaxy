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
        /// <summary>
        /// The name of the map file, sans extension.
        /// </summary>
        public string MapName;
        /// <summary>
        /// Same as MapName, unless <cref>IsMuseum</cref>, then it will be prefixed and suffixed to denote museum.
        /// </summary>
        public string name;
        public string ColoredName => Config.Color + name;
        public LevelConfig Config = new();
        public byte rotx, roty;
        public ushort spawnx, spawny, spawnz;
        public Position SpawnPos => new(16 + spawnx * 32, 32 + spawny * 32, 16 + spawnz * 32);
        public BlockDefinition[] CustomBlockDefs = new BlockDefinition[1024];
        public BlockProps[] Props = new BlockProps[1024];
        public ExtrasCollection Extras = new();
        public VolatileArray<PlayerBot> Bots = new();
        bool unloadedBots;
        public HandleDelete[] DeleteHandlers = new HandleDelete[1024];
        public HandlePlace[] PlaceHandlers = new HandlePlace[1024];
        public HandleWalkthrough[] WalkthroughHandlers = new HandleWalkthrough[1024];
        public HandlePhysics[] PhysicsHandlers = new HandlePhysics[1024];
        internal HandlePhysics[] physicsDoorsHandlers = new HandlePhysics[1024];
        internal AABB[] blockAABBs = new AABB[1024];
        /// <summary> The width of this level (Number of blocks across in X dimension) </summary>
        public ushort Width;
        /// <summary> The height of this level (Number of blocks tall in Y dimension) </summary>
        public ushort Height;
        /// <summary> The length of this level (Number of blocks across in Z dimension) </summary>
        public ushort Length;
        /// <summary> Whether this level should be treated as a readonly museum </summary>
        public bool IsMuseum;
        public int ReloadThreshold => Math.Max(10000, (int)(Server.Config.DrawReloadThreshold * Width * Height * Length));
        /// <summary> Maximum valid X coordinate (Width - 1) </summary>
        public int MaxX => Width - 1;
        /// <summary> Maximum valid Y coordinate (Height - 1) </summary>
        public int MaxY => Height - 1;
        /// <summary> Maximum valid Z coordinate (Length - 1) </summary>
        public int MaxZ => Length - 1;
        public bool Changed;
        /// <summary> Whether block changes made on this level should be saved to the BlockDB and .lvl files. </summary>
        public bool SaveChanges = true;
        public bool ChangedSinceBackup;
        /// <summary> Whether players on this level sees server-wide chat. </summary>
        public bool SeesServerWideChat => Config.ServerWideChat && Server.Config.ServerWideChat;
        internal readonly object saveLock = new(), botsIOLock = new();
        public BlockQueue blockqueue = new();
        BufferedBlockSender bulkSender;
        public List<UndoPos> UndoBuffer = new();
        public VolatileArray<Zone> Zones = new();
        public BlockDB BlockDB;
        public LevelAccessController VisitAccess, BuildAccess;
        // Physics fields and settings
        public int LevelPhysics => Physicsint;
        int Physicsint;
        public int currentUndo;
        public int lastCheck, lastUpdate;
        internal FastList<Check> ListCheck = new(); //A list of blocks that need to be updated
        internal FastList<Update> ListUpdate = new(); //A list of block to change after calculation
        internal SparseBitSet listCheckExists, listUpdateExists;
        public Random physRandom = new();
        public bool PhysicsPaused;
        Thread physThread;
        readonly object physThreadLock = new();
        internal readonly object physTickLock = new();
        bool physThreadStarted = false;
        internal DateTime lastBackup;
        public List<C4Data> C4list = new();
        public bool hasPortals, hasMessageBlocks;
    }
}
