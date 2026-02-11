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
using MCGalaxy.Maths;
using System;
namespace MCGalaxy.Blocks.Physics
{
    internal static class PlayerPhysics
    {
        internal static void Walkthrough(Player p, AABB bb)
        {
            Vec3S32 min = bb.BlockMin, max = bb.BlockMax;
            bool hitWalkthrough = false;
            for (int y = min.Y; y <= max.Y; y++)
            {
                for (int z = min.Z; z <= max.Z; z++)
                {
                    for (int x = min.X; x <= max.X; x++)
                    {
                        ushort xP = (ushort)x, yP = (ushort)y, zP = (ushort)z,
                            block = p.Level.GetBlock(xP, yP, zP);
                        if (block == 0xff) continue;
                        AABB blockBB = p.Level.blockAABBs[block].Offset(x * 32, y * 32, z * 32);
                        if (!AABB.Intersects(ref bb, ref blockBB)) continue;
                        if (!hitWalkthrough)
                        {
                            HandleWalkthrough handler = p.Level.WalkthroughHandlers[block];
                            if (handler != null && handler(p, block, xP, yP, zP))
                            {
                                p.lastWalkthrough = p.Level.PosToInt(xP, yP, zP);
                                hitWalkthrough = true;
                            }
                        }
                        if (!p.Level.Props[block].KillerBlock) continue;
                        if (block == 230 && p.trainInvincible) continue;
                        if (p.Level.Config.KillerBlocks) p.HandleDeath(block);
                    }
                }
            }
            if (!hitWalkthrough) p.lastWalkthrough = -1;
        }
        internal static void Fall(Player p, AABB bb, bool movingDown)
        {
            if (!movingDown)
            {
                bb.Min.X -= 1; 
                bb.Max.X += 1;
                bb.Min.Z -= 1; 
                bb.Max.Z += 1;
            }
            bb.Min.Y -= 2;
            Vec3S32 min = bb.BlockMin, max = bb.BlockMax;
            bool allGas = true;
            for (int z = min.Z; z <= max.Z; z++)
            {
                for (int x = min.X; x <= max.X; x++)
                {
                    ushort block = GetSurvivalBlock(p, x, min.Y, z);
                    byte collide = p.Level.CollideType(block);
                    allGas = allGas && collide == 0;
                    if (!DefaultSet.IsSolid(collide)) continue;
                    int fallHeight = p.startFallY - bb.Min.Y;
                    if (fallHeight > p.Level.Config.FallHeight * 32)
                    {
                        p.HandleDeath(0, null, false, true);
                    }
                    p.startFallY = -1;
                    return;
                }
            }
            if (!allGas) return;
            if (bb.Min.Y > p.lastFallY) p.startFallY = -1;
            p.startFallY = Math.Max(bb.Min.Y, p.startFallY);
        }
        internal static void Drown(Player p, AABB bb)
        {
            bb.Max.X -= (bb.Max.X - bb.Min.X) / 2;
            bb.Max.Z -= (bb.Max.Z - bb.Min.Z) / 2;
            Vec3S32 P = bb.BlockMax;
            ushort bHead = GetSurvivalBlock(p, P.X, P.Y, P.Z);
            if (Block.IsPhysicsType(bHead)) bHead = Block.Convert(bHead);
            if (p.Level.Props[bHead].Drownable)
            {
                p.startFallY = -1;
                DateTime now = DateTime.UtcNow;
                if (p.drownTime == DateTime.MaxValue)
                {
                    p.drownTime = now.AddSeconds(p.Level.Config.DrownTime / 10.0);
                }
                if (now > p.drownTime)
                {
                    p.HandleDeath(bHead);
                    p.drownTime = DateTime.MaxValue;
                }
            }
            else
            {
                bool isGas = p.Level.CollideType(bHead) == 0;
                if (bHead == Block.Rope) isGas = false;
                if (!isGas) p.startFallY = -1;
                p.drownTime = DateTime.MaxValue;
            }
        }
        static ushort GetSurvivalBlock(Player p, int x, int y, int z) => y < 0 ? (ushort)7 : y >= p.Level.Height ? (ushort)0 : p.Level.GetBlock((ushort)x, (ushort)y, (ushort)z);
    }
}
