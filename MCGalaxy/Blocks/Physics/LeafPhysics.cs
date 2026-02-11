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
namespace MCGalaxy.Blocks.Physics
{
    public static unsafe class LeafPhysics
    {
        public static void DoLeaf(Level lvl, ref PhysInfo C)
        {
            if (!lvl.Config.LeafDecay)
            {
                if (lvl.LevelPhysics > 1) ActivateablePhysics.CheckNeighbours(lvl, C.X, C.Y, C.Z);
                C.Data.Data = PhysicsArgs.RemoveFromChecks;
                return;
            }
            if (C.Data.Data < 5)
            {
                Random rand = lvl.physRandom;
                if (rand.Next(10) == 0) C.Data.Data++;
                return;
            }
            if (DoLeafDecay(lvl, ref C))
            {
                lvl.AddUpdate(C.Index, Block.Air, default(PhysicsArgs));
                if (lvl.LevelPhysics > 1) ActivateablePhysics.CheckNeighbours(lvl, C.X, C.Y, C.Z);
            }
            C.Data.Data = PhysicsArgs.RemoveFromChecks;
        }
        static bool DoLeafDecay(Level lvl, ref PhysInfo C)
        {
            int* dists = stackalloc int[729];
            ushort x = C.X, y = C.Y, z = C.Z;
            int idx = 0;
            for (int xx = -4; xx <= 4; xx++)
            {
                for (int yy = -4; yy <= 4; yy++)
                {
                    for (int zz = -4; zz <= 4; zz++, idx++)
                    {
                        int index = lvl.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz));
                        byte type = index < 0 ? Block.Air : lvl.blocks[index];
                        if (type == Block.Log)
                            dists[idx] = 0;
                        else if (type == Block.Leaves)
                            dists[idx] = -2;
                        else
                            dists[idx] = -1;
                    }
                }
            }
            for (int dist = 1; dist <= 4; dist++)
            {
                idx = 0;
                for (int xx = -4; xx <= 4; xx++)
                {
                    for (int yy = -4; yy <= 4; yy++)
                    {
                        for (int zz = -4; zz <= 4; zz++, idx++)
                        {
                            if (dists[idx] != dist - 1) continue;
                            if (xx > -4) PropagateDist(dists, dist, idx - 1);
                            if (xx < 4) PropagateDist(dists, dist, idx + 1);
                            if (yy > -4) PropagateDist(dists, dist, idx - 9);
                            if (yy < 4) PropagateDist(dists, dist, idx + 9);
                            if (zz > -4) PropagateDist(dists, dist, idx - 81);
                            if (zz < 4) PropagateDist(dists, dist, idx + 81);
                        }
                    }
                }
            }
            idx = 364;
            return dists[idx] < 0;
        }
        static void PropagateDist(int* dists, int dist, int index)
        {
            if (dists[index] == -2) dists[index] = dist;
        }
        public static void DoLog(Level lvl, ref PhysInfo C)
        {
            ushort x = C.X, y = C.Y, z = C.Z;
            for (int xx = -4; xx <= 4; xx++)
            {
                for (int yy = -4; yy <= 4; yy++)
                {
                    for (int zz = -4; zz <= 4; zz++)
                    {
                        int index = lvl.PosToInt((ushort)(x + xx), (ushort)(y + yy), (ushort)(z + zz));
                        if (index < 0 || lvl.blocks[index] != Block.Leaves) continue;
                        lvl.AddCheck(index);
                    }
                }
            }
            C.Data.Data = PhysicsArgs.RemoveFromChecks;
        }
    }
}
