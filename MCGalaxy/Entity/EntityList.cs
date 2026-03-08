/*
    Copyright 2015-2024 MCGalaxy
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
using MCGalaxy.Events.EntityEvents;
using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    class VisibleEntity
    {
        public readonly Entity e;
        public readonly byte id;
        public readonly string displayName;
        public VisibleEntity(Entity e, byte id, string displayName)
        {
            this.e = e;
            this.id = id;
            this.displayName = displayName;
        }
    }
    class TabObject
    {
        public readonly object o;
        public readonly byte id;
        public string name, nick, group;
        public byte groupRank;
        public TabObject(object o, byte id, string name, string nick, string group, byte groupRank)
        {
            this.o = o;
            this.id = id;
            this.name = name;
            this.nick = nick;
            this.group = group;
            this.groupRank = groupRank;
        }
        public void UpdateFields(string name, string nick, string group, byte groupRank)
        {
            this.name = name;
            this.nick = nick;
            this.group = group;
            this.groupRank = groupRank;
        }
    }
    class WaitingEntity : VisibleEntity
    {
        public readonly bool tabList;
        public WaitingEntity(Entity e, byte id, string displayName, bool tabList) : base(e, id, displayName) => this.tabList = tabList;
    }
    /// <summary>
    /// Manages a collection of entities that a player is intended to see.
    /// </summary>
    public sealed class EntityList
    {
        readonly Player p;
        readonly Dictionary<Entity, VisibleEntity> visible = new();
        readonly List<WaitingEntity> invisible = new();
        WaitingEntity IsWaitingToSpawn(Entity e)
        {
            foreach (WaitingEntity vis in invisible)
                if (vis.e == e) return vis;
            return null;
        }
        readonly Stack<byte> freeIDs;
        readonly object locker = new();
        #region TabList
        readonly Dictionary<object, TabObject> tabObjects = new();
        readonly bool[] usedTabIDs;
        void AddTab(Entity e)
        {
            if (e is Player player)
            {
                if (!Server.Config.TablistGlobal) TabList.Add(p, player);
            }
            else if (e is PlayerBot bot)
            {
                if (Server.Config.TablistBots) TabList.Add(p, bot);
            }
        }
        void RemoveTab(Entity e)
        {
            if (e is Player player)
            {
                if (!Server.Config.TablistGlobal) TabList.Remove(p, player);
            }
            else if (e is PlayerBot bot)
            {
                if (Server.Config.TablistBots) TabList.Remove(p, bot);
            }
        }
        public void SendAddTabEntry(object o, string name, string nick, string group, byte groupRank)
        {
            if (!p.hasExtList) return;
            bool self = o == p;
            lock (locker)
            {
                if (tabObjects.TryGetValue(o, out TabObject tabby))
                    tabby.UpdateFields(name, nick, group, groupRank);
                else
                {
                    int tentativeID = FindFreeTabID(o, self);
                    if (tentativeID == -1) return;
                    byte ID = (byte)tentativeID;
                    tabby = new(o, ID, name, nick, group, groupRank);
                    tabObjects[o] = tabby;
                }
                p.Session.SendAddTabEntry(tabby.id, tabby.name, tabby.nick, tabby.group, tabby.groupRank);
            }
        }
        int FindFreeTabID(object o, bool self)
        {
            if (self) return 0xFF;
            if (o is Entity entity)
            {
                Entity e = entity;
                if (visible.TryGetValue(e, out VisibleEntity vis) && usedTabIDs[vis.id] != true)
                {
                    usedTabIDs[vis.id] = true;
                    return vis.id;
                }
            }
            for (int i = maxEntityID; i >= 0; i--)
                if (usedTabIDs[i] == false)
                {
                    usedTabIDs[i] = true;
                    return i;
                }
            return -1;
        }
        public void SendRemoveTabEntry(object o)
        {
            if (!p.hasExtList) return;
            lock (locker)
                if (tabObjects.TryGetValue(o, out TabObject tabby))
                {
                    tabby = tabObjects[o];
                    if (o != p) usedTabIDs[tabby.id] = false;
                    tabObjects.Remove(o);
                    p.Session.SendRemoveTabEntry(tabby.id);
                }
        }
        #endregion
        readonly byte maxEntityID;
        public EntityList(Player p, byte maxEntityID)
        {
            this.p = p;
            this.maxEntityID = maxEntityID;
            lock (locker)
            {
                freeIDs = new(maxEntityID);
                for (int i = maxEntityID; i >= 0; i--)
                    freeIDs.Push((byte)i);
                usedTabIDs = new bool[maxEntityID + 1];
            }
        }
        /// <summary>
        /// Adds the given entity and calls OnSendingModelEvent. Returns true if an entity was spawned, otherwise false if it could not immediately be spawned (it will spawn later if enough are removed)
        /// If this returns false and tabList is true, once the entity spawns, it will be added to the tab list.
        /// </summary>
        public bool Add(Entity e, Position pos, Orientation rot, string skin, string name, string model, bool tabList)
        {
            bool self = e == p;
            OnSendingModelEvent.Call(e, ref model, p);
            lock (locker)
            {
                if (freeIDs.Count > 0 || self)
                {
                    if (!visible.TryGetValue(e, out VisibleEntity vis))
                    {
                        byte ID = self ? (byte)0xFF : freeIDs.Pop();
                        vis = new(e, ID, name);
                        visible[e] = vis;
                    }
                    Spawn(vis, pos, rot, skin, name, model);
                    if (tabList) AddTab(e);
                    if (tabObjects.TryGetValue(vis.e, out TabObject tabby) && tabby.id != vis.id)
                    {
                        SendRemoveTabEntry(vis.e);
                        SendAddTabEntry(vis.e, tabby.name, tabby.nick, tabby.group, tabby.groupRank);
                    }
                    return true;
                }
                if (IsWaitingToSpawn(e) == null)
                    invisible.Add(new(e, 0, name, tabList));
                return false;
            }
        }
        /// <summary>
        /// Remove the given entity and despawns it for this player. Returns true if an entity was despawned, otherwise false.
        /// </summary>
        public bool Remove(Entity e, bool tabList)
        {
            bool self = e == p;
            lock (locker)
            {
                WaitingEntity waiting = IsWaitingToSpawn(e);
                if (waiting != null)
                {
                    invisible.Remove(waiting);
                    return false;
                }
                if (visible.TryGetValue(e, out _))
                {
                    VisibleEntity vis = visible[e];
                    if (!self) freeIDs.Push(vis.id);
                    visible.Remove(e);
                    if (tabList) RemoveTab(e);
                    Despawn(vis);
                    if (invisible.Count > 0 && freeIDs.Count > 0 && !self)
                    {
                        waiting = invisible[0];
                        invisible.RemoveAt(0);
                        Add(waiting.e, waiting.e.Pos, waiting.e.Rot, waiting.e.SkinName, waiting.displayName, waiting.e.Model, waiting.tabList);
                    }
                    return true;
                }
                else
                    return false;
            }
        }
        void Spawn(VisibleEntity vis, Position pos, Orientation rot, string skin, string name, string model)
        {
            p.Session.SendSpawnEntity(vis.id, name, skin, pos, rot);
            SendModel(vis, model);
            SendRot(vis, rot);
            SendScales(vis);
        }
        void Despawn(VisibleEntity vis) => p.Session.SendRemoveEntity(vis.id);
        /// <summary>
        /// Calls OnSendingModelEvent and changes the model of the given entity.
        /// </summary>
        public void SendModel(Entity e, string model)
        {
            OnSendingModelEvent.Call(e, ref model, p);
            lock (locker)
            {
                if (!visible.TryGetValue(e, out VisibleEntity vis)) return;
                SendModel(vis, model);
            }
        }
        void SendModel(VisibleEntity vis, string model)
        {
            if (p.hasChangeModel)
                p.Session.SendChangeModel(vis.id, model);
        }
        void SendRot(VisibleEntity vis, Orientation rot)
        {
            if (p.Supports(CpeExt.EntityProperty))
            {
                p.Session.SendEntityProperty(vis.id, EntityProp.RotX, Orientation.PackedToDegrees(rot.RotX));
                p.Session.SendEntityProperty(vis.id, EntityProp.RotZ, Orientation.PackedToDegrees(rot.RotZ));
            }
        }
        public void SendScales(Entity e)
        {
            lock (locker)
            {
                if (!visible.TryGetValue(e, out VisibleEntity vis)) return;
                SendScales(vis);
            }
        }
        void SendScales(VisibleEntity vis)
        {
            if (!p.Supports(CpeExt.EntityProperty)) return;
            float max = ModelInfo.MaxScale(vis.e, vis.e.Model);
            SendScale(vis, EntityProp.ScaleX, vis.e.ScaleX, max);
            SendScale(vis, EntityProp.ScaleY, vis.e.ScaleY, max);
            SendScale(vis, EntityProp.ScaleZ, vis.e.ScaleZ, max);
        }
        void SendScale(VisibleEntity vis, EntityProp axis, float value, float max)
        {
            if (value == 0) return;
            value = Math.Min(value, max);
            int packed = (int)(value * 1000);
            if (packed == 0) return;
            p.Session.SendEntityProperty(vis.id, axis, packed);
        }
        public void SendProp(Entity e, EntityProp prop, int value)
        {
            if (!p.Supports(CpeExt.EntityProperty)) return;
            lock (locker)
            {
                if (!visible.TryGetValue(e, out VisibleEntity vis)) return;
                p.Session.SendEntityProperty(vis.id, prop, value);
            }
        }
        public bool GetID(Entity e, out byte id)
        {
            lock (locker)
                if (visible.TryGetValue(e, out VisibleEntity vis))
                {
                    id = vis.id;
                    return true;
                }
            id = 0;
            return false;
        }
        readonly Dictionary<Entity, VisibleEntity> cachedVisible = new(32);
        internal unsafe void BroadcastEntityPositions()
        {
            byte* src = stackalloc byte[16 * 256],
                ptr = src;
            Player dst = p;
            lock (locker)
            {
                cachedVisible.Clear();
                foreach (KeyValuePair<Entity, VisibleEntity> pair in visible)
                {
                    if (!pair.Key.autoBroadcastPosition) continue;
                    cachedVisible[pair.Key] = pair.Value;
                    if (pair.Key.untracked)
                        pair.Key._positionUpdatePos = pair.Key.Pos;
                }
            }
            foreach (KeyValuePair<Entity, VisibleEntity> pair in cachedVisible)
            {
                Entity e = pair.Key;
                byte id = pair.Value.id;
                if (dst == e || dst.Level != e.Level || !dst.CanSeeEntity(e)) continue;
                Orientation rot = e.Rot;
                byte pitch = rot.HeadX;
                if (e is Player pl)
                {
                    if (Server.flipHead || pl.flipHead) pitch = FlippedPitch(pitch);
                    if (!dst.hasChangeModel && pl.infected)
                        pitch = FlippedPitch(pitch);
                }
                rot.HeadX = pitch;
                p.Session.GetPositionPacket(ref ptr, id, e.hasExtPositions, dst.hasExtPositions,
                                           e._positionUpdatePos, e._lastPos, rot, e._lastRot);
            }
            int count = (int)(ptr - src);
            if (count == 0) return;
            byte[] packet = new byte[count];
            for (int i = 0; i < packet.Length; i++)
                packet[i] = src[i];
            dst.Send(packet);
            foreach (KeyValuePair<Entity, VisibleEntity> pair in cachedVisible)
            {
                if (pair.Key.untracked)
                {
                    pair.Key._lastPos = pair.Key._positionUpdatePos;
                    pair.Key._lastRot = pair.Key.Rot;
                }
            }
        }
        static byte FlippedPitch(byte pitch) => pitch > 64 && pitch < 192 ? pitch : (byte)128;
    }
}
