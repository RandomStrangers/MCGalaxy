/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCForge)
    Dual-licensed under the    Educational Community License, Version 2.0 and
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
using MCGalaxy.Bots;
using MCGalaxy.Commands;
using MCGalaxy.Maths;
using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    public sealed class PlayerBot : Entity
    {
        public bool hunt = false, kill = false,
            nodUp = false, movement = false;
        public string AIName = "", color, name, DisplayName, 
            ClickedOnText, DeathMessage, Owner;
        public string ColoredName => color + DisplayName;
        public Level level;
        public int cur = 0, countdown = 0, movementSpeed = 3;
        public List<InstructionData> Instructions = new();
        public Position TargetPos;
        internal int curJump = 0;
        public long CreationDate = 0;
        public PlayerBot(string n, Level lvl)
        {
            name = n; 
            DisplayName = n; 
            SkinName = n;
            color = "&1";
            Level = lvl;
            SetModel(Model);
            hasExtPositions = true;
            BotsScheduler.Activate();
        }
        public override bool CanSeeEntity(Entity other) => true;
        public override Level Level 
        {
            get 
            { 
                return level; 
            } 
            set
            { 
                level = value; 
            }
        }
        public override bool RestrictsScale => false;
        public bool EditableBy(Player p, string attemptedAction = "modify")
        {
            if (CanEditAny(p)) 
            {
                return true; 
            }
            if (Owner == p.name) 
            { 
                return true; 
            }
            p.Message("&WYou are not allowed to {0} bots that you did not create.", attemptedAction);
            return false;
        }
        public static bool CanEditAny(Player p)
        {
            if (LevelInfo.IsRealmOwner(p.Level.name, p.name)) 
            { 
                return true; 
            }
            ItemPerms perms = CommandExtraPerms.Find("Bot", 1) ?? new ItemPerms(LevelPermission.Operator);
            return perms.UsableBy(p);
        }
        public static void Add(PlayerBot bot, bool save = true)
        {
            lock (bot.Level.Bots.locker)
            {
                bot.Level.Bots.Add(bot);
            }
            bot.GlobalSpawn();
            if (save) BotsFile.Save(bot.Level);
        }
        public static void Remove(PlayerBot bot, bool save = true)
        {
            bot.Level.Bots.Remove(bot);
            bot.GlobalDespawn();
            bot.curJump = 0;
            if (save) BotsFile.Save(bot.Level);
        }
        internal static int RemoveLoadedBots(Level lvl, bool save)
        {
            PlayerBot[] bots = lvl.Bots.Items;
            for (int i = 0; i < bots.Length; i++)
            {
                Remove(bots[i], save);
            }
            return bots.Length;
        }
        internal static int RemoveBotsOwnedBy(Player _, string ownerName, Level lvl, bool save)
        {
            PlayerBot[] bots = lvl.Bots.Items;
            int removedCount = 0;
            for (int i = 0; i < bots.Length; i++)
            {
                if (ownerName.CaselessEq(bots[i].Owner))
                {
                    Remove(bots[i], save);
                    removedCount++;
                }
            }
            return removedCount;
        }
        public void GlobalSpawn()
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players)
            {
                if (p.Level == Level) Entities.Spawn(p, this);
            }
        }
        public void GlobalDespawn()
        {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player p in players)
            {
                if (p.Level == Level) Entities.Despawn(p, this);
            }
        }
        public void NextInstruction()
        {
            cur++;
            if (cur == Instructions.Count) cur = 0;
        }
        public void FaceTowards(Position srcPos, Position dstPos)
        {
            Vec3F32 dir = new()
            {
                X = dstPos.X - srcPos.X,
                Y = dstPos.Y - srcPos.Y,
                Z = dstPos.Z - srcPos.Z
            };
            dir = Vec3F32.Normalise(dir);
            Orientation rot = Rot;
            DirUtils.GetYawPitch(dir, out rot.RotY, out rot.HeadX);
            Rot = rot;
        }
        internal static void GlobalUpdatePosition()
        {
            Level[] levels = LevelInfo.Loaded.Items;
            for (int i = 0; i < levels.Length; i++)
            {
                UpdatePositions(levels[i]);
            }
        }
        internal static void GlobalPostBroadcastPosition()
        {
            Level[] levels = LevelInfo.Loaded.Items;
            for (int i = 0; i < levels.Length; i++)
            {
                PostBroadcastPosition(levels[i]);
            }
        }
        static void UpdatePositions(Level lvl)
        {
            PlayerBot[] bots = lvl.Bots.Items;
            for (int i = 0; i < bots.Length; i++)
            {
                PlayerBot bot = bots[i];
                if (bot.movement) bot.PerformMovement();
                bot._positionUpdatePos = bot.Pos;
            }
        }
        static void PostBroadcastPosition(Level lvl)
        {
            PlayerBot[] bots = lvl.Bots.Items;
            for (int i = 0; i < bots.Length; i++)
            {
                PlayerBot bot = bots[i];
                bot._lastPos = bot._positionUpdatePos;
                bot._lastRot = bot.Rot;
            }
        }
        static AABB[] downs = new AABB[16], ups = new AABB[16];
        static int downsCount, upsCount;
        void RecalcDownExtent(ref AABB bb, int steps, int dx, int dz) => downsCount = AABB.FindIntersectingSolids(bb.Adjust(dx * steps, -32, dz * steps), Level, ref downs);
        void RecalcUpExtent(ref AABB bb, int steps, int dx, int dz) => upsCount = AABB.FindIntersectingSolids(bb.Adjust(dx * steps, 32, dz * steps), Level, ref ups);
        void PerformMovement()
        {
            double scale = Math.Ceiling(Server.Config.PositionUpdateInterval / 25.0);
            int steps = movementSpeed * (int)scale;
            downsCount = -1;
            for (int i = 0; i < steps; i++) DoMove(steps);
        }
        void DoMove(int steps)
        {
            Position pos = Pos;
            AABB bb = ModelBB.OffsetPosition(pos);
            int dx = Math.Sign(TargetPos.X - pos.X),
                dz = Math.Sign(TargetPos.Z - pos.Z);
            if (downsCount == -1)
            {
                RecalcDownExtent(ref bb, steps, dx, dz);
                RecalcUpExtent(ref bb, steps, dx, dz);
            }
            bb = bb.Offset(dx, 0, dz);
            AABB bbCopy = bb;
            int hitY = -32;
            for (int dy = 0; dy >= -32; dy--)
            {
                bool intersectsAny = false;
                for (int i = 0; i < downsCount; i++)
                {
                    if (AABB.Intersects(ref bb, ref downs[i])) 
                    {
                        intersectsAny = true;
                        break;
                    }
                }
                if (intersectsAny) 
                { 
                    hitY = dy + 1; 
                    break; 
                }
                bb.Min.Y--; 
                bb.Max.Y--;
            }
            if (hitY < 0)
            {
                pos.X += dx; 
                pos.Y += hitY; 
                pos.Z += dz; 
                Pos = pos;
                RecalcDownExtent(ref bb, steps, dx, dz);
                RecalcUpExtent(ref bb, steps, dx, dz);
                return;
            }
            bb = bbCopy;
            for (int dy = 0; dy <= 32; dy++)
            {
                bool intersectsAny = false;
                for (int i = 0; i < upsCount; i++)
                {
                    if (AABB.Intersects(ref bb, ref ups[i])) 
                    { 
                        intersectsAny = true; 
                        break; 
                    }
                }
                if (!intersectsAny)
                {
                    pos.X += dx; 
                    pos.Y += dy; 
                    pos.Z += dz; 
                    Pos = pos;
                    if (dy != 0)
                    {
                        RecalcDownExtent(ref bb, steps, dx, dz);
                        RecalcUpExtent(ref bb, steps, dx, dz);
                    }
                    return;
                }
                bb.Min.Y++; 
                bb.Max.Y++;
            }
        }
        public void DisplayInfo(Player p)
        {
            p.Message("Bot {0} &S({1}) has:", ColoredName, name);
            p.Message("  Owner: &f{0}", string.IsNullOrEmpty(Owner) ? "no one" : p.FormatNick(Owner));
            if (CreationDate != 0) 
            { 
                p.Message("  Created: &f{0}", CreationDate.FromUnixTime().ToString("yyyy-MM-dd"));
            }
            if (!string.IsNullOrEmpty(AIName)) 
            { 
                p.Message("  AI: &f{0}", AIName); 
            }
            if (hunt || kill) 
            { 
                p.Message("  Hunt: &f{0}&S, Kill: &f{1}", hunt, kill);
            }
            if (SkinName != name) 
            { 
                p.Message("  Skin: &f{0}", SkinName); 
            }
            if (Model != "humanoid") 
            { 
                p.Message("  Model: &f{0}", Model); 
            }
            if (!(ScaleX == 0 && ScaleY == 0 && ScaleZ == 0))
            {
                p.Message("  X scale: &a{0}&S, Y scale: &a{1}&S, Z scale: &a{2}",
                         ScaleX == 0 ? "none" : ScaleX.ToString(),
                         ScaleY == 0 ? "none" : ScaleY.ToString(),
                         ScaleZ == 0 ? "none" : ScaleZ.ToString()
                        );
            }
            if (string.IsNullOrEmpty(ClickedOnText)) return;
            ItemPerms perms = CommandExtraPerms.Find("About", 1) ?? new ItemPerms(LevelPermission.AdvBuilder);
            if (!perms.UsableBy(p)) return;
            p.Message("  Clicked-on text: {0}", ClickedOnText);
        }
    }
}
