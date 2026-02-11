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
using System;
namespace MCGalaxy.Maths
{
    /// <summary> 3 component vector (unsigned 16 bit integer) </summary>
    public struct Vec3U16 : IEquatable<Vec3U16>
    {
        public ushort X, Y, Z;
        public Vec3U16(ushort x, ushort y, ushort z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public static explicit operator Vec3U16(Vec3S32 a) => new((ushort)a.X, (ushort)a.Y, (ushort)a.Z);
        public readonly int LengthSquared => X * X + Y * Y + Z * Z;
        public readonly float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public override readonly bool Equals(object obj) => (obj is Vec3U16 u) && Equals(u);
        public readonly bool Equals(Vec3U16 other) => X == other.X & Y == other.Y && Z == other.Z;
        public override readonly int GetHashCode()
        {
            int hashCode = 1000000007 * X;
            hashCode += 1000000009 * Y;
            hashCode += 1000000021 * Z;
            return hashCode;
        }
        public static bool operator ==(Vec3U16 a, Vec3U16 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        public static bool operator !=(Vec3U16 a, Vec3U16 b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        public override readonly string ToString() => X + ", " + Y + ", " + Z;
    }
    /// <summary> 3 component vector (signed 32 bit integer) </summary>
    public struct Vec3S32 : IEquatable<Vec3S32>
    {
        public int X, Y, Z;
        public Vec3S32(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public readonly float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public int this[int index]
        {
            readonly get
            {
                return index == 0 ? X : index == 1 ? Y : Z;
            }
            set
            {
                if (index == 0)
                {
                    X = value;
                }
                else if (index == 1)
                {
                    Y = value;
                }
                else
                {
                    Z = value;
                }
            }
        }
        public static Vec3S32 Max(Vec3S32 a, Vec3S32 b) => new(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));
        public static Vec3S32 Min(Vec3S32 a, Vec3S32 b) => new(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));
        public static implicit operator Vec3S32(Vec3U16 a) => new(a.X, a.Y, a.Z);
        public static Vec3S32 operator +(Vec3S32 a, Vec3S32 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vec3S32 operator -(Vec3S32 a, Vec3S32 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3S32 operator *(Vec3S32 a, int b) => new(a.X * b, a.Y * b, a.Z * b);
        public static Vec3S32 operator /(Vec3S32 a, int b) => new(a.X / b, a.Y / b, a.Z / b);
        public static Vec3S32 operator *(Vec3S32 a, float b) => new((int)(a.X * b), (int)(a.Y * b), (int)(a.Z * b));
        public override readonly bool Equals(object obj) => (obj is Vec3S32 s) && Equals(s);
        public readonly bool Equals(Vec3S32 other) => X == other.X & Y == other.Y && Z == other.Z;
        public override readonly int GetHashCode()
        {
            int hashCode = 1000000007 * X;
            hashCode += 1000000009 * Y;
            hashCode += 1000000021 * Z;
            return hashCode;
        }
        public static bool operator ==(Vec3S32 a, Vec3S32 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        public static bool operator !=(Vec3S32 a, Vec3S32 b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        public override readonly string ToString() => X + ", " + Y + ", " + Z;
    }
    /// <summary> 3 component vector (32 bit floating point) </summary>
    public struct Vec3F32 : IEquatable<Vec3F32>
    {
        public float X, Y, Z;
        public Vec3F32(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public readonly float LengthSquared => X * X + Y * Y + Z * Z;
        public readonly float Length => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public static float Dot(Vec3F32 a, Vec3F32 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static Vec3F32 Cross(Vec3F32 a, Vec3F32 b) => new(a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        public static Vec3F32 Normalise(Vec3F32 a)
        {
            float invLen = 1 / a.Length;
            if (invLen == float.PositiveInfinity)
            {
                return a;
            }
            a.X *= invLen;
            a.Y *= invLen;
            a.Z *= invLen;
            return a;
        }
        public static Vec3F32 operator *(float a, Vec3F32 b) => new(a * b.X, a * b.Y, a * b.Z);
        public static Vec3F32 operator *(Vec3F32 a, float b) => new(a.X * b, a.Y * b, a.Z * b);
        public static Vec3F32 operator -(Vec3F32 a, Vec3F32 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vec3F32 operator +(Vec3F32 a, Vec3F32 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static implicit operator Vec3F32(Vec3S32 a) => new(a.X, a.Y, a.Z);
        public override readonly bool Equals(object obj) => (obj is Vec3F32 f) && Equals(f);
        public readonly bool Equals(Vec3F32 other) => X == other.X & Y == other.Y && Z == other.Z;
        public override int GetHashCode()
        {
            int hashCode = 1000000007 * X.GetHashCode();
            hashCode += 1000000009 * Y.GetHashCode();
            hashCode += 1000000021 * Z.GetHashCode();
            return hashCode;
        }
        public static bool operator ==(Vec3F32 a, Vec3F32 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        public static bool operator !=(Vec3F32 a, Vec3F32 b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        public override readonly string ToString() => X + "," + Y + "," + Z;
    }
}
