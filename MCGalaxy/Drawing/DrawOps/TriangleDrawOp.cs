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
using MCGalaxy.Drawing.Brushes;
using MCGalaxy.Maths;
using System;
namespace MCGalaxy.Drawing.Ops
{
    public class TriangleDrawOp : DrawOp
    {
        public override string Name => "Triangle";
        public override long BlocksAffected(Level lvl, Vec3S32[] marks)
        {
            float a = (marks[0] - marks[2]).Length,
                b = (marks[1] - marks[2]).Length,
                c = (marks[0] - marks[1]).Length,
                s = (a + b + c) / 2;
            return (int)Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output)
        {
            Vec3F32 A = marks[0], B = marks[1], C = marks[2],
                N = Vec3F32.Cross(B - A, C - A);
            N = Vec3F32.Normalise(N);
            Vec3U16 min = Clamp(Min), max = Clamp(Max);
            for (ushort yy = min.Y; yy <= max.Y; yy++)
            {
                for (ushort zz = min.Z; zz <= max.Z; zz++)
                {
                    for (ushort xx = min.X; xx <= max.X; xx++)
                    {
                        Vec3F32 P = new(xx, yy, zz);
                        float t = Vec3F32.Dot(N, A) - Vec3F32.Dot(N, P);
                        if (Math.Abs(t) > 0.5) continue;
                        Vec3F32 P0 = P + t * N,
                            v0 = C - A, v1 = B - A, v2 = P0 - A;
                        float dot00 = Vec3F32.Dot(v0, v0),
                            dot01 = Vec3F32.Dot(v0, v1),
                            dot02 = Vec3F32.Dot(v0, v2),
                            dot11 = Vec3F32.Dot(v1, v1),
                            dot12 = Vec3F32.Dot(v1, v2),
                            invDenom = 1 / (dot00 * dot11 - dot01 * dot01),
                            u = (dot11 * dot02 - dot01 * dot12) * invDenom,
                            v = (dot00 * dot12 - dot01 * dot02) * invDenom;
                        if (u >= 0 && v >= 0 && u + v <= 1)
                        {
                            output(Place(xx, yy, zz, brush));
                        }
                        else if (Axis(P, A, B) || Axis(P, A, C) || Axis(P, B, C))
                        {
                            output(Place(xx, yy, zz, brush));
                        }
                    }
                }
            }
        }
        bool Axis(Vec3F32 P, Vec3F32 P1, Vec3F32 P2)
        {
            float bottom = (P2 - P1).LengthSquared;
            if (bottom == 0) return (P1 - P).Length <= 0.5f;
            float t = Vec3F32.Dot(P2 - P1, P - P1) / bottom;
            if (t < 0 || t > 1) return false;
            Vec3F32 proj = P1 + t * (P2 - P1);
            return (P - proj).Length <= 0.5f;
        }
    }
}
