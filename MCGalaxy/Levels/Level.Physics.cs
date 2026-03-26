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
using MCGalaxy.Events.LevelEvents;
using System;
using System.Threading;
namespace MCGalaxy
{
    public enum PhysicsState 
    { 
        Stopped, Warning, Other 
    }
    public sealed partial class Level : IDisposable
    {
        public void SetPhysics(int level)
        {
            if (IsMuseum) return;
            if (LevelPhysics == 0 && level != 0 && blocks != null)
                for (int i = 0; i < blocks.Length; i++)
                    if (blocks[i] > 183 && Block.NeedRestart(blocks[i]))
                        AddCheck(i);
            if (LevelPhysics != level) OnPhysicsLevelChangedEvent.Call(this, level);
            if (level > 0 && LevelPhysics == 0) StartPhysics();
            Physicsint = level;
            Config.Physics = level;
        }
        public void StartPhysics()
        {
            lock (physThreadLock)
            {
                if (physThread != null && physThread.ThreadState == ThreadState.Running || ListCheck.Count == 0 || physThreadStarted) return;
                Utils.StartBackgroundThread(out physThread, "Physics_" + name,
                                   PhysicsLoop);
                physThreadStarted = true;
            }
        }
        public void PhysicsLoop()
        {
            int wait = Config.PhysicsSpeed;
            while (true)
            {
                try
                {
                    if (PhysicsPaused)
                    {
                        if (LevelPhysics == 0) break;
                        Thread.Sleep(500); 
                        continue;
                    }
                    if (wait > 0) Thread.Sleep(wait);
                    if (LevelPhysics == 0) break;
                    if (ListCheck.Count == 0)
                    {
                        lastCheck = 0;
                        wait = Config.PhysicsSpeed;
                        continue;
                    }
                    DateTime tickStart = default;
                    try
                    {
                        lock (physTickLock)
                        {
                            tickStart = DateTime.UtcNow;
                            PhysicsTick();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Error in physics tick", ex);
                    }
                    TimeSpan elapsed = DateTime.UtcNow - tickStart;
                    wait = Config.PhysicsSpeed - (int)elapsed.TotalMilliseconds;
                    if (wait < (int)(-Config.PhysicsOverload * 0.75f))
                    {
                        if (wait < -Config.PhysicsOverload)
                        {
                            if (!Server.Config.PhysicsRestart) SetPhysics(0);
                            ClearPhysics();
                            Chat.MessageGlobal("Physics shutdown on {0}", ColoredName);
                            Logger.Log(LogType.Warning, "Physics shutdown on " + name);
                            OnPhysicsStateChangedEvent.Call(this, PhysicsState.Stopped);
                            wait = Config.PhysicsSpeed;
                        }
                        else
                        {
                            Message("Physics warning!");
                            Logger.Log(LogType.Warning, "Physics warning on " + name);
                            OnPhysicsStateChangedEvent.Call(this, PhysicsState.Warning);
                        }
                    }
                }
                catch
                {
                    wait = Config.PhysicsSpeed;
                }
            }
            lastCheck = 0;
            physThreadStarted = false;
        }
        public PhysicsArgs FoundInfo(ushort x, ushort y, ushort z)
        {
            if (!listCheckExists.Get(x, y, z))
                return default;
            int index = PosToInt(x, y, z);
            for (int i = 0; i < ListCheck.Count; i++)
            {
                Check C = ListCheck.Items[i];
                if (C.Index != index) continue;
                return C.data;
            }
            return default;
        }
        public void PhysicsTick()
        {
            lastCheck = ListCheck.Count;
            HandlePhysics[] handlers = PhysicsHandlers;
            ExtraInfoHandler extraHandler = ExtraInfoPhysics.normalHandler;
            if (LevelPhysics == 5)
            {
                handlers = physicsDoorsHandlers;
                extraHandler = ExtraInfoPhysics.doorsHandler;
            }
            PhysInfo C;
            for (int i = 0; i < ListCheck.Count; i++)
            {
                Check chk = ListCheck.Items[i];
                IntToPos(chk.Index, out C.X, out C.Y, out C.Z);
                C.Index = chk.Index; C.Data = chk.data;
                try
                {
                    if (OnPhysicsUpdateEvent.handlers.Count > 0)
                        OnPhysicsUpdateEvent.Call(C.X, C.Y, C.Z, C.Data, this);
                    C.Block = blocks[chk.Index];
                    ushort extended = Block.ExtendedBase[C.Block];
                    if (extended > 0)
                        C.Block = (ushort)(extended | FastGetExtTile(C.X, C.Y, C.Z));
                    if ((C.Data.Raw & 0x3F) == 0 || C.Data.Type1 == 7 || extraHandler(this, ref C))
                    {
                        HandlePhysics handler = handlers[C.Block];
                        if (handler != null)
                            handler(this, ref C);
                        else if ((C.Data.Raw & 0x3F) == 0 || !C.Data.HasWait)
                            C.Data.Data = 255;
                    }
                    ListCheck.Items[i].data = C.Data;
                }
                catch
                {
                    listCheckExists.Set(C.X, C.Y, C.Z, false);
                    ListCheck.RemoveAt(i);
                }
            }
            RemoveExpiredChecks();
            lastUpdate = ListUpdate.Count;
            if (ListUpdate.Count > 0 && bulkSender == null)
                bulkSender = new(this);
            for (int i = 0; i < ListUpdate.Count; i++)
            {
                Update U = ListUpdate.Items[i];
                try
                {
                    ushort block = U.data.Data;
                    U.data.Data = 0;
                    byte extBits = U.data.ExtBlock;
                    if (extBits != 0 && (U.data.Raw & 0x3F) == 0)
                    {
                        block |= (ushort)(extBits << 8);
                        U.data.Raw &= ~((1u << 30) | (1u << 31));
                    }
                    if (DoPhysicsBlockchange(U.Index, block, false, U.data, true))
                        bulkSender.Add(U.Index, block);
                }
                catch
                {
                    Logger.Log(LogType.Warning, "Phys update issue");
                }
            }
            bulkSender?.Flush();
            ListUpdate.Clear(); listUpdateExists.Clear();
        }
        /// <summary> Adds the given coordinates to the list of ticked coordinates with empty data </summary>
        public void AddCheck(int index, bool overRide = false) => AddCheck(index, overRide, default);
        /// <summary> Adds the given coordinates to the list of ticked coordinates with the given data </summary>
        public void AddCheck(int index, bool overRide, PhysicsArgs data)
        {
            try
            {
                int x = index % Width,
                    y = index / Width / Length,
                    z = index / Width % Length;
                if (x >= Width || y >= Height || z >= Length) return;
                if (listCheckExists.TrySetOn(x, y, z))
                {
                    Check check = new()
                    {
                        Index = index,
                        data = data
                    };
                    ListCheck.Add(check); 
                }
                else if (overRide)
                {
                    Check[] items = ListCheck.Items;
                    int count = ListCheck.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (items[i].Index != index) continue;
                        items[i].data = data; 
                        return;
                    }
                }
                if (!physThreadStarted && LevelPhysics > 0)
                    StartPhysics();
            }
            catch
            {
            }
        }
        /// <summary> Adds the given entry to the list of updates to be applied at the end of the current physics tick </summary>
        /// <remarks> Must only be called from the physics thread (i.e. in a HandlePhysics handler function) </remarks>
        public bool AddUpdate(int index, ushort block, bool overRide = false)
        {
            PhysicsArgs args = default;
            args.Raw |= (uint)((1u << 30) * (block >> 8));
            return AddUpdate(index, block, args, overRide);
        }
        /// <summary> Adds the given entry to the list of updates to be applied at the end of the current physics tick </summary>
        /// <remarks> Must only be called from the physics thread (i.e. in a HandlePhysics handler function) </remarks>
        public bool AddUpdate(int index, ushort block, PhysicsArgs data, bool overRide = false)
        {
            try
            {
                int x = index % Width,
                    y = index / Width / Length,
                    z = index / Width % Length;
                if (x >= Width || y >= Height || z >= Length) return false;
                if (overRide)
                {
                    if (data.ExtBlock != 0 && (data.Raw & 0x3F) == 0)
                        data.Raw &= ~((1u << 30) | (1u << 31));
                    AddCheck(index, true, data);
                    Blockchange((ushort)x, (ushort)y, (ushort)z, block, true, data);
                    return true;
                }
                if (listUpdateExists.TrySetOn(x, y, z))
                {
                }
                else if (block == 12 || block == 13)
                    RemoveUpdatesAtPos(index);
                else
                    return false;
                data.Data = (byte)block;
                Update update = new()
                {
                    Index = index,
                    data = data
                };
                ListUpdate.Add(update);
                if (!physThreadStarted && LevelPhysics > 0)
                    StartPhysics();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void RemoveExpiredChecks()
        {
            Check[] items = ListCheck.Items;
            int j = 0, count = ListCheck.Count;
            for (int i = 0; i < count; i++)
            {
                if (items[i].data.Data == 255)
                {
                    IntToPos(items[i].Index, out ushort x, out ushort y, out ushort z);
                    listCheckExists.Set(x, y, z, false);
                    continue;
                }
                items[j] = items[i]; 
                j++;
            }
            ListCheck.Items = items;
            ListCheck.Count = j;
        }
        public void RemoveUpdatesAtPos(int b)
        {
            Update[] items = ListUpdate.Items;
            int j = 0, count = ListUpdate.Count;
            for (int i = 0; i < count; i++)
            {
                if (items[j].Index == b) continue;
                items[j] = items[i];
                j++;
            }
            ListUpdate.Items = items;
            ListUpdate.Count = j;
        }
        public void ClearPhysicsLists()
        {
            ListCheck.Count = 0; 
            listCheckExists.Clear();
            ListUpdate.Count = 0; 
            listUpdateExists.Clear();
        }
        public void ClearPhysics()
        {
            for (int i = 0; i < ListCheck.Count; i++)
                RevertPhysics(ListCheck.Items[i]);
            ClearPhysicsLists();
        }
        public void RevertPhysics(Check C)
        {
            switch (blocks[C.Index])
            {
                case 200:
                case 202:
                case 203:
                case 204:
                    blocks[C.Index] = 0; 
                    break;
            }
            try
            {
                PhysicsArgs args = C.data;
                switch (args.Type1)
                {
                    case 2:
                        Blockchange(C.Index, (ushort)(args.Value1 | (args.ExtBlock << 8)), true, default);
                        break;
                    default:
                        if (args.Type2 == 2)
                            Blockchange(C.Index, (ushort)(args.Value2 | (args.ExtBlock << 8)), true, default);
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
        internal bool ActivatesPhysics(ushort block) => !Props[block].IsMessageBlock && !Props[block].IsPortal && !Props[block].IsDoor && !Props[block].IsTDoor && !Props[block].OPBlock && PhysicsHandlers[block] != null;
        internal bool CheckSpongeWater(ushort x, ushort y, ushort z)
        {
            for (int yy = y - 2; yy <= y + 2; ++yy)
            {
                if (yy < 0 || yy >= Height) continue;
                for (int zz = z - 2; zz <= z + 2; ++zz)
                {
                    if (zz < 0 || zz >= Length) continue;
                    for (int xx = x - 2; xx <= x + 2; ++xx)
                    {
                        if (xx < 0 || xx >= Width) continue;
                        if (blocks[xx + Width * (zz + yy * Length)] == 19)
                            return true;
                    }
                }
            }
            return false;
        }
        internal bool CheckSpongeLava(ushort x, ushort y, ushort z)
        {
            for (int yy = y - 2; yy <= y + 2; ++yy)
            {
                if (yy < 0 || yy >= Height) continue;
                for (int zz = z - 2; zz <= z + 2; ++zz)
                {
                    if (zz < 0 || zz >= Length) continue;
                    for (int xx = x - 2; xx <= x + 2; ++xx)
                    {
                        if (xx < 0 || xx >= Width) continue;
                        if (blocks[xx + Width * (zz + yy * Length)] == 109)
                            return true;
                    }
                }
            }
            return false;
        }
        public void MakeExplosion(ushort x, ushort y, ushort z, int size, bool force = false) => TntPhysics.MakeExplosion(this, x, y, z, size, force, null);
    }
    /// <summary> Represents a physics tick entry </summary>
    public struct PhysInfo
    {
        public ushort X, Y, Z, Block;
        /// <summary> Packed coordinates of this tick entry </summary>
        public int Index;
        /// <summary> Data/State of this tick entry </summary>
        public PhysicsArgs Data;
    }
    public struct Check
    {
        public int Index;
        public PhysicsArgs data;
    }
    public struct Update
    {
        public int Index;
        public PhysicsArgs data;
    }
}
