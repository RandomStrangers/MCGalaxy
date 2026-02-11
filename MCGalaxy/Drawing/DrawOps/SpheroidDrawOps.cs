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
    public abstract class ShapedDrawOp : DrawOp
    {
        public double XRadius => (Max.X - Min.X) / 2.0 + 0.25;
        public double YRadius => (Max.Y - Min.Y) / 2.0 + 0.25;
        public double ZRadius => (Max.Z - Min.Z) / 2.0 + 0.25;
        public double XCentre => (Min.X + Max.X) / 2.0;
        public double YCentre => (Min.Y + Max.Y) / 2.0;
        public double ZCentre => (Min.Z + Max.Z) / 2.0;
        public int Height => Max.Y - Min.Y + 1;
        public static double EllipsoidVolume(double rx, double ry, double rz) => Math.PI * 4.0 / 3.0 * (rx * ry * rz);
        public static double ConeVolume(double rx, double rz, double height) => Math.PI / 3.0 * (rx * rz * height);
        public static double CylinderVolume(double rx, double rz, double height) => Math.PI * (rx * rz * height);
    }
    public class EllipsoidDrawOp : ShapedDrawOp
    {
        public override string Name => "Ellipsoid";
        public override long BlocksAffected(Level lvl, Vec3S32[] marks) => (long)EllipsoidVolume(XRadius, YRadius, ZRadius);
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output)
        {
            double cx = XCentre, cy = YCentre, cz = ZCentre,
                rx = XRadius, ry = YRadius, rz = ZRadius,
                rx2 = 1 / (rx * rx), ry2 = 1 / (ry * ry), rz2 = 1 / (rz * rz);
            Vec3U16 min = Clamp(Min), max = Clamp(Max);
            for (ushort y = min.Y; y <= max.Y; y++)
            {
                for (ushort z = min.Z; z <= max.Z; z++)
                {
                    for (ushort x = min.X; x <= max.X; x++)
                    {
                        double dx = x - cx, dy = y - cy, dz = z - cz;
                        if (dx * dx * rx2 + dy * dy * ry2 + dz * dz * rz2 <= 1)
                            output(Place(x, y, z, brush));
                    }
                }
            }
        }
    }
    public class EllipsoidHollowDrawOp : ShapedDrawOp
    {
        public override string Name => "Ellipsoid Hollow";
        public override long BlocksAffected(Level lvl, Vec3S32[] marks)
        {
            double rx = XRadius, ry = YRadius, rz = ZRadius,
                outer = EllipsoidVolume(rx, ry, rz),
                inner = EllipsoidVolume(rx - 1, ry - 1, rz - 1);
            return (long)(outer - inner);
        }
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output)
        {
            double cx = XCentre, cy = YCentre, cz = ZCentre,
                rx = XRadius, ry = YRadius, rz = ZRadius,
                outer_rx2 = 1 / (rx * rx),
                outer_ry2 = 1 / (ry * ry),
                outer_rz2 = 1 / (rz * rz),
                inner_rx2 = 1 / ((rx - 1) * (rx - 1)),
                inner_ry2 = 1 / ((ry - 1) * (ry - 1)),
                inner_rz2 = 1 / ((rz - 1) * (rz - 1));
            Vec3U16 min = Clamp(Min), max = Clamp(Max);
            for (ushort y = min.Y; y <= max.Y; y++)
            {
                for (ushort z = min.Z; z <= max.Z; z++)
                {
                    for (ushort x = min.X; x <= max.X; x++)
                    {
                        double dx = x - cx, dy = y - cy, dz = z - cz;
                        dx *= dx; 
                        dy *= dy; 
                        dz *= dz;
                        if (dx * outer_rx2 + dy * outer_ry2 + dz * outer_rz2 > 1)
                            continue;
                        if (dx * inner_rx2 + dy * inner_ry2 + dz * inner_rz2 > 1)
                            output(Place(x, y, z, brush));
                    }
                }
            }
        }
    }
    public class CylinderDrawOp : ShapedDrawOp
    {
        public override string Name => "Cylinder";
        public override long BlocksAffected(Level lvl, Vec3S32[] marks)
        {
            double rx = XRadius, rz = ZRadius, h = Height,
                outer = CylinderVolume(rx, rz, h),
                inner = CylinderVolume(rx - 1, rz - 1, h);
            return (long)(outer - inner);
        }
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output)
        {
            double cx = XCentre, cz = ZCentre,
                rx = XRadius, rz = ZRadius,
                outer_rx2 = 1 / (rx * rx),
                outer_rz2 = 1 / (rz * rz),
                inner_rx2 = 1 / ((rx - 1) * (rx - 1)),
                inner_rz2 = 1 / ((rz - 1) * (rz - 1));
            Vec3U16 p1 = Clamp(Min), p2 = Clamp(Max);
            for (ushort y = p1.Y; y <= p2.Y; y++)
            {
                for (ushort z = p1.Z; z <= p2.Z; z++)
                {
                    for (ushort x = p1.X; x <= p2.X; x++)
                    {
                        double dx = x - cx, dz = z - cz;
                        dx *= dx; 
                        dz *= dz;
                        if (dx * outer_rx2 + dz * outer_rz2 > 1)
                            continue;
                        if (dx * inner_rx2 + dz * inner_rz2 > 1)
                            output(Place(x, y, z, brush));
                    }
                }
            }
        }
    }
    public class ConeDrawOp : ShapedDrawOp
    {
        public bool Invert;
        public override string Name => "Cone";
        public ConeDrawOp(bool invert = false) => Invert = invert;
        public override long BlocksAffected(Level lvl, Vec3S32[] marks) => (long)ConeVolume(XRadius, ZRadius, Height);
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output)
        {
            Vec3U16 p1 = Clamp(Min), p2 = Clamp(Max);
            double cx = XCentre, cz = ZCentre;
            int height = Height;
            for (ushort y = p1.Y; y <= p2.Y; y++)
            {
                int dy = Invert ? y - Min.Y : Max.Y - y;
                double T = (double)(dy + 1) / height,
                    rx = (Max.X - Min.X) / 2.0 * T + 0.25,
                    rz = (Max.Z - Min.Z) / 2.0 * T + 0.25,
                    rx2 = 1 / (rx * rx), rz2 = 1 / (rz * rz);
                for (ushort z = p1.Z; z <= p2.Z; z++)
                {
                    for (ushort x = p1.X; x <= p2.X; x++)
                    {
                        double dx = x - cx, dz = z - cz;
                        if (dx * dx * rx2 + dz * dz * rz2 <= 1)
                            output(Place(x, y, z, brush));
                    }
                }
            }
        }
    }
}
