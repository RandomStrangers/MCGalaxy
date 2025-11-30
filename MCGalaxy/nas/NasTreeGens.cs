#if NAS && TEN_BIT_BLOCKS
using MCGalaxy;
using MCGalaxy.Drawing.Brushes;
using MCGalaxy.Drawing.Ops;
using MCGalaxy.Generator.Foliage;
using MCGalaxy.Maths;
using System;
using System.Collections.Generic;
namespace NotAwesomeSurvival
{
    public class BirchTree : Tree
    {
        public override long EstimateBlocksAffected()
        {
            return height + size * size * size; 
        }
        public override int DefaultSize(Random rnd) 
        { 
            return rnd.Next(5, 8);
        }
        public override void SetData(Random rnd, int value)
        {
            height = value;
            size = height - rnd.Next(1, 3);
            this.rnd = rnd;
        }
        public override void Generate(ushort x, ushort y, ushort z, TreeOutput output)
        {
            for (ushort dy = 0; dy < height + size - 1; dy++)
            {
                output(x, (ushort)(y + dy), z, Nas.FromRaw(242));
            }
            for (int dy = -size; dy <= size; ++dy)
            {
                for (int dz = -size; dz <= size; ++dz)
                {
                    for (int dx = -size; dx <= size; ++dx)
                    {
                        int dist = (int)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                        if ((dist < size + 1) && rnd.Next(dist) < 2)
                        {
                            ushort xx = (ushort)(x + dx), yy = (ushort)(y + dy + height), zz = (ushort)(z + dz);
                            if (xx != x || zz != z || dy >= size - 1)
                            {
                                output(xx, yy, zz, Nas.FromRaw(103));
                            }
                        }
                    }
                }
            }
        }
    }
    public class SwampTree : Tree
    {
        public override long EstimateBlocksAffected()
        {
            return height + 145;
        }
        public override int DefaultSize(Random rnd)
        {
            return rnd.Next(4, 8);
        }
        public override void SetData(Random rnd, int value)
        {
            height = value;
            size = 3;
            this.rnd = rnd;
        }
        public override void Generate(ushort x, ushort y, ushort z, TreeOutput output)
        {
            for (int i = 0; i <= height; i++)
            {
                output(x, (ushort)(y + i), z, 17);
            }
            for (int j = height - 2; j <= height + 1; j++)
            {
                int num = (j > height - 1) ? 2 : 3;
                for (int k = -num; k <= num; k++)
                {
                    for (int l = -num; l <= num; l++)
                    {
                        ushort num2 = (ushort)(x + l),
                            y2 = (ushort)(y + j),
                            num3 = (ushort)(z + k);
                        if (num2 != x || num3 != z || j > height)
                        {
                            if (Math.Abs(l) == num && Math.Abs(k) == num)
                            {
                                if (j <= height && rnd.Next(2) == 0)
                                {
                                    output(num2, y2, num3, Nas.FromRaw(146));
                                }
                            }
                            else
                            {
                                output(num2, y2, num3, Nas.FromRaw(146));
                                if (rnd.Next(15) == 0)
                                {
                                    output(num2, (ushort)(height - 3 + y), num3, Nas.FromRaw(107));
                                    if (rnd.Next(3) == 0)
                                    {
                                        output(num2, (ushort)(height - 4 + y), num3, Nas.FromRaw(107));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    public class SpruceTree : Tree
    {
        public int branchBaseHeight, branchAmount;
        public List<Vec3S32> branch = new();
        public override long EstimateBlocksAffected()
        {
            return height * (long)height * height;
        }
        public override int DefaultSize(Random rnd)
        {
            return rnd.Next(5, 8);
        }
        public override void SetData(Random rnd, int value)
        {
            this.rnd = rnd;
            height = value;
            size = 8;
            branchBaseHeight = height / 4;
            branchAmount = rnd.Next(10, 25);
        }
        public override void Generate(ushort x, ushort y, ushort z, TreeOutput output)
        {
            Vec3S32 p = new(x, y, z),
                p2 = new(x, y + height, z);
            Line(p, p2, output);
            for (int i = 0; i < branchAmount; i++)
            {
                DoBranch(x, y, z, output);
            }
        }
        public void DoBranch(int x, int y, int z, TreeOutput output)
        {
            int num = rnd.Next(-5, 5),
                num2 = rnd.Next(-5, 5),
                num3 = rnd.Next(1, 3),
                num4 = rnd.Next(branchBaseHeight, height),
                num5 = num4 + rnd.Next(3, 10);
            Vec3S32 p = new(x, y + num4, z),
                p2 = new(x + num, y + num5, z + num2);
            Line(p, p2, output);
            int num6 = num3;
            Vec3S32[] marks = new Vec3S32[] 
            {
                new(x + num - num6, y + num5 - num6, z + num2 - num6),
                new(x + num + num6, y + num5 + num6, z + num2 + num6)
            };
            DrawOp drawOp = new EllipsoidDrawOp();
            Brush brush = new SolidBrush(19);
            drawOp.SetMarks(marks);
            drawOp.Perform(marks, brush, delegate (DrawOpBlock b)
            {
                output(b.X, b.Y, b.Z, (byte)b.Block);
            });
        }
        public void Line(Vec3S32 p1, Vec3S32 p2, TreeOutput output)
        {
            ThingDrawOp.DrawLine(p1.X, p1.Y, p1.Z, 10000, p2.X, p2.Y, p2.Z, branch);
            foreach (Vec3S32 current in branch)
            {
                output((ushort)current.X, (ushort)current.Y, (ushort)current.Z, Nas.FromRaw(250));
            }
            branch.Clear();
        }
    }
    public class ThingDrawOp : DrawOp
    {
        public struct Line
        {
            public int len2, dir, axis;
        }
        public bool WallsMode;
        public override string Name
        {
            get
            {
                return "Line";
            }
        }
        public override void Perform(Vec3S32[] marks, Brush brush, DrawOpOutput output)
        {
            Vec3U16 vec3U = Clamp(marks[0]),
                vec3U2 = Clamp(marks[1]);
            List<Vec3S32> list = new();
            DrawLine(vec3U.X, vec3U.Y, vec3U.Z, 2147483647, vec3U2.X, vec3U2.Y, vec3U2.Z, list);
            if (WallsMode)
            {
                ushort y = vec3U.Y,
                    y2 = vec3U2.Y;
                vec3U.Y = Math.Min(y, y2);
                vec3U2.Y = Math.Max(y, y2);
            }
            for (int i = 0; i < list.Count; i++)
            {
                Vec3U16 vec3U3 = (Vec3U16)list[i];
                if (WallsMode)
                {
                    for (ushort num = vec3U.Y; num <= vec3U2.Y; num += 1)
                    {
                        output(Place(vec3U3.X, num, vec3U3.Z, brush));
                    }
                }
                else
                {
                    output(Place(vec3U3.X, vec3U3.Y, vec3U3.Z, brush));
                }
            }
        }
        public override long BlocksAffected(Level lvl, Vec3S32[] marks)
        {
            Vec3S32 vec3S = marks[0],
                vec3S2 = marks[1];
            double num = Math.Abs(vec3S2.X - vec3S.X) + 0.25,
                num2 = Math.Abs(vec3S2.Y - vec3S.Y) + 0.25,
                num3 = Math.Abs(vec3S2.Z - vec3S.Z) + 0.25;
            if (WallsMode)
            {
                int val = (int)Math.Ceiling(Math.Sqrt(num * num + num3 * num3));
                return Math.Min(val, 2147483647) * (Math.Abs(vec3S2.Y - vec3S.Y) + 1);
            }
            int val2 = (int)Math.Ceiling(Math.Sqrt(num * num + num2 * num2 + num3 * num3));
            return Math.Min(val2, 2147483647);
        }
        public static void DrawLine(int x1, int y1, int z1, int maxLen, int x2, int y2, int z2, List<Vec3S32> buffer)
        {
            int[] array = new int[]
            {
                x1,
                y1,
                z1
            };
            int value = x2 - x1,
                value2 = y2 - y1,
                value3 = z2 - z1;
            Line line = new()
            {
                dir = Math.Sign(value)
            },
            line2 = new()
            {
                dir = Math.Sign(value2)
            },
            line3 = new()
            {
                dir = Math.Sign(value3)
            };
            int num = Math.Abs(value),
                num2 = Math.Abs(value2),
                num3 = Math.Abs(value3);
            line.len2 = num << 1;
            line2.len2 = num2 << 1;
            line3.len2 = num3 << 1;
            line.axis = 0;
            line2.axis = 1;
            line3.axis = 2;
            if (num >= num2 && num >= num3)
            {
                DoLine(line2, line3, line, num, array, maxLen, buffer);
            }
            else
            {
                if (num2 >= num && num2 >= num3)
                {
                    DoLine(line, line3, line2, num2, array, maxLen, buffer);
                }
                else
                {
                    DoLine(line2, line, line3, num3, array, maxLen, buffer);
                }
            }
            Vec3S32 item = new()
            {
                X = array[0],
                Y = array[1],
                Z = array[2]
            };
            buffer.Add(item);
        }
        public static void DoLine(Line l1, Line l2, Line l3, int len, int[] pixel, int maxLen, List<Vec3S32> buffer)
        {
            int num = l1.len2 - len,
                num2 = l2.len2 - len,
                num3 = 0;
            while (num3 < len && num3 < maxLen - 1)
            {
                Vec3S32 item = new()
                {
                    X = pixel[0],
                    Y = pixel[1],
                    Z = pixel[2]
                };
                buffer.Add(item);
                if (num > 0)
                {
                    pixel[l1.axis] += l1.dir;
                    num -= l3.len2;
                }
                if (num2 > 0)
                {
                    pixel[l2.axis] += l2.dir;
                    num2 -= l3.len2;
                }
                num += l1.len2;
                num2 += l2.len2;
                pixel[l3.axis] += l3.dir;
                num3++;
            }
        }
    }
}
#endif