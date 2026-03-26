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
using MCGalaxy.Drawing.Ops;
using MCGalaxy.Maths;
using System;
namespace MCGalaxy.Drawing.Transforms
{
    public sealed class RotateTransform : Transform
    {
        public override string Name => "Rotate";
        public bool CentreOrigin;
        public Shear2D shearX, shearY, shearZ;
        public Vec3S32 P;
        public struct Shear2D
        {
            public int xMulX, xMulY, yMulX, yMulY;
            public double alpha, beta;
        };
        public void SetAngles(double xDeg, double yDeg, double zDeg)
        {
            CalcShear2D(xDeg, ref shearX);
            CalcShear2D(yDeg, ref shearY);
            CalcShear2D(zDeg, ref shearZ);
        }
        public void CalcShear2D(double angle, ref Shear2D shear)
        {
            angle %= 360.0;
            if (angle < 0) angle += 360.0;
            switch (angle)
            {
                case >= 0 and <= 90:
                    shear.xMulX = 1;
                    shear.yMulY = 1;
                    break;
                case > 90 and <= 180:
                    angle -= 90;
                    shear.xMulY = 1;
                    shear.yMulX = -1;
                    break;
                case > 180 and <= 270:
                    angle -= 180;
                    shear.xMulX = -1;
                    shear.yMulY = -1;
                    break;
                default:
                    angle -= 270;
                    shear.xMulY = -1;
                    shear.yMulX = 1;
                    break;
            }
            angle = -angle;
            angle *= Math.PI / 180.0;
            shear.alpha = -Math.Tan(angle / 2);
            shear.beta = Math.Sin(angle);
        }
        public void DoShear2D(ref int x, ref int y, ref Shear2D shear)
        {
            int X_ = (int)(x + shear.alpha * (y + 0.5)),
                Y_ = (int)(y + shear.beta * (X_ + 0.5));
            X_ = (int)(X_ + shear.alpha * (Y_ + 0.5));
            x = shear.xMulX * X_ + shear.xMulY * Y_;
            y = shear.yMulX * X_ + shear.yMulY * Y_;
        }
        public override void Perform(Vec3S32[] marks, DrawOp op, Brush brush, DrawOpOutput output)
        {
            P = (op.Min + op.Max) / 2;
            if (!CentreOrigin) P = op.Origin;
            op.Perform(marks, brush, b => OutputBlock(b, output));
        }
        public void OutputBlock(DrawOpBlock b, DrawOpOutput output)
        {
            int dx = b.X - P.X, dy = b.Y - P.Y, dz = b.Z - P.Z;
            DoShear2D(ref dy, ref dz, ref shearX);
            DoShear2D(ref dx, ref dz, ref shearY);
            DoShear2D(ref dx, ref dy, ref shearZ);
            b.X = (ushort)(P.X + dx);
            b.Y = (ushort)(P.Y + dy);
            b.Z = (ushort)(P.Z + dz);
            output(b);
        }
    }
}
