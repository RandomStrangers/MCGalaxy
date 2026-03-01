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
using System.Collections.Generic;
namespace MCGalaxy.Generator.Foliage
{
    public sealed class AshTree : Tree
    {
        int branchBaseHeight, branchAmount;
        readonly List<Vec3S32> branch = new();
        public override long EstimateBlocksAffected() => (long)height * height * height;
        public override int DefaultSize(Random rnd) => rnd.Next(5, 10);
        public override void SetData(Random rnd, int value)
        {
            this.rnd = rnd;
            height = value;
            size = 5 + 3;
            branchBaseHeight = height / 4;
            branchAmount = rnd.Next(10, 25);
        }
        public override void Generate(ushort x, ushort y, ushort z, TreeOutput output)
        {
            Vec3S32 p1 = new(x, y, z),
                p2 = new(x, y + height, z);
            Line(p1, p2, output);
            for (int i = 0; i < branchAmount; i++)
            {
                DoBranch(x, y, z, output);
            }
        }
        void DoBranch(int x, int y, int z, TreeOutput output)
        {
            int dx = rnd.Next(-5, 5),
                dz = rnd.Next(-5, 5),
                clusterSize = rnd.Next(1, 3),
                branchStart = rnd.Next(branchBaseHeight, height),
                branchMax = branchStart + rnd.Next(3, 10);
            Vec3S32 p1 = new(x, y + branchStart, z),
                p2 = new(x + dx, y + branchMax, z + dz);
            Line(p1, p2, output);
            int R = clusterSize;
            Vec3S32[] marks = new Vec3S32[] {
                new(x + dx - R, y + branchMax - R, z + dz - R),
                new(x + dx + R, y + branchMax + R, z + dz + R) };
            DrawOp op = new EllipsoidDrawOp();
            Brush brush = new SolidBrush(LeafBlock);
            op.SetMarks(marks);
            op.Perform(marks, brush, b => output(b.X, b.Y, b.Z, b.Block));
        }
        void Line(Vec3S32 p1, Vec3S32 p2, TreeOutput output)
        {
            LineDrawOp.DrawLine(p1.X, p1.Y, p1.Z, 10000, p2.X, p2.Y, p2.Z, branch);
            foreach (Vec3S32 P in branch)
            {
                output((ushort)P.X, (ushort)P.Y, (ushort)P.Z, TrunkBlock);
            }
            branch.Clear();
        }
    }
}
