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
    public static class DirUtils
    {
        public static void EightYaw(byte yaw, out int dirX, out int dirZ)
        {
            dirX = 0;
            dirZ = 0;
            switch (yaw)
            {
                case < 0 + 64 / 4 * 3:
                case > 256 - 64 / 4 * 3:
                    dirZ = -1;
                    break;
                case > 128 - 64 / 4 * 3 and < 128 + 64 / 4 * 3:
                    dirZ = 1;
                    break;
            }
            switch (yaw)
            {
                case > 64 - 64 / 4 * 3 and < 64 + 64 / 4 * 3:
                    dirX = 1;
                    break;
                case > 192 - 64 / 4 * 3 and < 192 + 64 / 4 * 3:
                    dirX = -1;
                    break;
            }
        }
        public static void FourYaw(byte yaw, out int dirX, out int dirZ)
        {
            dirX = 0;
            dirZ = 0;
            switch (yaw)
            {
                case <= 0 + 64 / 2:
                case >= 256 - 64 / 2:
                    dirZ = -1;
                    break;
                case <= 128 - 64 / 2:
                    dirX = 1;
                    break;
                case <= 128 + 64 / 2:
                    dirZ = 1;
                    break;
                default:
                    dirX = -1;
                    break;
            }
        }
        public static void Pitch(byte pitch, out int dirY)
        {
            dirY = 0;
            switch (pitch)
            {
                case >= 192 and <= 192 + 32:
                    dirY = 1;
                    break;
                case >= 32 and <= 64:
                    dirY = -1;
                    break;
            }
        }
        public static Vec3F32 GetDirVector(double yaw, double pitch) => new((float)(Math.Sin(yaw) * Math.Cos(pitch)), (float)-Math.Sin(pitch), (float)(-Math.Cos(yaw) * Math.Cos(pitch)));
        public static Vec3F32 GetDirVectorExt(ushort yaw, ushort pitch) => GetDirVector(yaw * (2 * Math.PI / 65536.0), pitch * (2 * Math.PI / 65536.0));
        public static Vec3F32 GetDirVector(byte yaw, byte pitch) => GetDirVector(yaw * (2 * Math.PI / 256.0), pitch * (2 * Math.PI / 256.0));
        public static void GetYawPitch(Vec3F32 dir, out byte yaw, out byte pitch)
        {
            yaw = (byte)(Math.Atan2(dir.X, -dir.Z) * (256.0 / (2 * Math.PI)));
            pitch = (byte)(Math.Asin(-dir.Y) * (256.0 / (2 * Math.PI)));
        }
    }
}