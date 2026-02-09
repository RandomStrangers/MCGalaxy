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
using MCGalaxy.Maths;
namespace MCGalaxy
{
    /// <summary> Represents the position of an entity in the world. </summary>
    public struct Position
    {
        public int X, Y, Z;
        public readonly Vec3F32 ToVec3F32() => new(X / 32.0f, Y / 32.0f, Z / 32.0f);
        public Position(int x, int y, int z) { X = x; Y = y; Z = z; }
        public static Position FromFeet(int x, int y, int z) => new(x, y + 51, z);
        public static Position FromFeetBlockCoords(int bX, int bY, int bZ) => FromFeet(16 + bX * 32, bY * 32, 16 + bZ * 32);
        /// <summary> World/block coordinate of this position. </summary>
        public readonly Vec3S32 BlockCoords => new(X >> 5, Y >> 5, Z >> 5);
        /// <summary> World/block coordinate of this position. </summary>
        public readonly Vec3S32 FeetBlockCoords => new(X >> 5, (Y - 51) >> 5, Z >> 5);
        /// <summary> X block coordinate of this position. </summary>
        public readonly int BlockX => X >> 5;
        /// <summary> Y block coordinate of this position. </summary>
        public readonly int BlockY => Y >> 5;
        /// <summary> Z block coordinate of this position. </summary>
        public readonly int BlockZ => Z >> 5;
        internal readonly long Pack() => (X & (long)0x1FFFFF) | ((Y & (long)0x1FFFFF) << 21) | ((Z & (long)0x1FFFFF) << 42);
        internal static Position Unpack(long raw) => new()
        {
            X = SignExtend(raw),
            Y = SignExtend(raw >> 21),
            Z = SignExtend(raw >> 42)
        };
        static int SignExtend(long parts)
        {
            int value = (int)(parts & 0x1FFFFF);
            value <<= 32 - 21;
            value >>= 32 - 21;
            return value;
        }
    }
    /// <summary> Represents orientation / rotation of an entity. </summary>
    public struct Orientation
    {
        public byte RotX, RotY, RotZ, HeadX;
        public Orientation(byte yaw, byte pitch) 
        { 
            RotX = 0;
            RotY = yaw; 
            RotZ = 0; 
            HeadX = pitch;
        }
        /// <summary> Converts angle in range [0, 256) into range [0, 360). </summary>
        public static int PackedToDegrees(byte packed) => packed * 360 / 256;
        /// <summary> Converts angle in degrees into range [0, 256) </summary>
        public static byte DegreesToPacked(int degrees) => (byte)(degrees * 256 / 360);
        internal readonly uint Pack() => (uint)(RotX | (RotY << 8) | (RotZ << 16) | (HeadX << 24));
        internal static Orientation Unpack(uint raw) => new()
        {
            RotX = (byte)raw,
            RotY = (byte)(raw >> 8),
            RotZ = (byte)(raw >> 16),
            HeadX = (byte)(raw >> 24)
        };
    }
}
