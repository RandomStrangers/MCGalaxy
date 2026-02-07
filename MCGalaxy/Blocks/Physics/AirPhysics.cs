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
namespace MCGalaxy.Blocks.Physics
{
    public static class AirPhysics
    {
        public static void DoAir(Level lvl, ref PhysInfo C)
        {
            if (C.Data.Type1 == 7)
            {
                DoorPhysics.Do(lvl, ref C); return;
            }
            ushort x = C.X, y = C.Y, z = C.Z;
            ActivateablePhysics.CheckNeighbours(lvl, x, y, z);
            ActivateablePhysics.CheckAt(lvl, x, (ushort)(y - 1), z);
            //Edge of map water
            if (lvl.Config.EdgeWater && (x == 0 || x == lvl.Width - 1 || z == 0 || z == lvl.Length - 1))
            {
                int edgeLevel = lvl.GetEdgeLevel();
                int sidesOffset = lvl.Config.SidesOffset;
                if (sidesOffset == int.MaxValue) sidesOffset = -2; // EnvConfig.DefaultEnvProp(EnvProp.SidesOffset, lvl.Height);
                if (y < edgeLevel && y >= (edgeLevel + sidesOffset))
                {
                    ushort horizon = lvl.Config.HorizonBlock;
                    lvl.AddUpdate(C.Index, horizon == 0xff ? (ushort)8 : horizon);
                }
            }
            if (!C.Data.HasWait) C.Data.Data = 255;
        }
        public static void DoFlood(Level lvl, ref PhysInfo C, int mode, ushort block)
        {
            if (C.Data.Data >= 1)
            {
                lvl.AddUpdate(C.Index, 0, default(PhysicsArgs));
                C.Data.Data = 255; return;
            }
            ushort x = C.X, y = C.Y, z = C.Z;
            FloodAir(lvl, (ushort)(x + 1), y, z, block);
            FloodAir(lvl, (ushort)(x - 1), y, z, block);
            FloodAir(lvl, x, y, (ushort)(z + 1), block);
            FloodAir(lvl, x, y, (ushort)(z - 1), block);
            switch (mode)
            {
                case 0:
                    FloodAir(lvl, x, (ushort)(y - 1), z, block);
                    FloodAir(lvl, x, (ushort)(y + 1), z, block);
                    break;
                case 1:
                    break;
                case 2:
                    FloodAir(lvl, x, (ushort)(y - 1), z, block);
                    break;
                case 3:
                    FloodAir(lvl, x, (ushort)(y + 1), z, block);
                    break;
            }
            C.Data.Data++;
        }
        static void FloodAir(Level lvl, ushort x, ushort y, ushort z, ushort block)
        {
            ushort curBlock = Block.Convert(lvl.GetBlock(x, y, z, out int index));
            if (curBlock == 8 || curBlock == 10)
            {
                lvl.AddUpdate(index, block);
            }
        }
    }
}
