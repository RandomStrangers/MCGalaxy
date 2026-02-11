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
using System;
using System.Collections.Generic;
namespace MCGalaxy
{
    public struct ModelInfo
    {
        public readonly string Model;
        public readonly Vec3S32 BaseSize;
        public readonly int EyeHeight;
        public ModelInfo(string model, int sizeX, int sizeY, int sizeZ, int eyeHeight)
        {
            Model = model;
            BaseSize.X = sizeX;
            BaseSize.Y = sizeY;
            BaseSize.Z = sizeZ;
            EyeHeight = eyeHeight;
        }
        public static List<ModelInfo> Models = new() {
            new("humanoid",    18,56,18, 28),
            new("sit",         18,56,18, 18),
            new("chicken",     16,24,16, 12),
            new("creeper",     16,52,16, 22),
            new("chibi",        8,40,8,  14),
            new("head",        31,31,31,  4),
            new("pig",         28,28,28, 12),
            new("sheep",       20,40,20, 19),
            new("sheep_nofur", 20,40,20, 19),
            new("skeleton",    16,56,16, 28),
            new("spider",      30,24,30,  8),
        };
        public static ModelInfo Get(string model)
        {
            foreach (ModelInfo m in Models)
            {
                if (m.Model.CaselessEq(model)) return m;
            }
            return Models[0];
        }
        public static string GetRawModel(string model) => model.IndexOf('|') == -1 ? model : model.Substring(0, model.IndexOf('|'));
        public static float GetRawScale(string model)
        {
            int sep = model.IndexOf('|');
            string str = sep == -1 ? null : model.Substring(sep + 1);
            if (!NumberUtils.TryParseSingle(str, out float scale)) scale = 1.0f;
            if (scale < 0.01f) scale = 0.01f;
            if (model.CaselessEq("giant")) scale *= 2;
            if (model.CaselessStarts("hold|") && scale > 1) scale = 1;
            return scale;
        }
        public static float DefaultMaxScale(string model) => model.CaselessEq("chibi") ? 3 : 2;
        public static float MaxScale(Entity entity, string model) => !entity.RestrictsScale ? float.MaxValue : DefaultMaxScale(GetRawModel(model));
        public static Vec3F32 CalcScale(Entity entity) => new(
                entity.ScaleX != 0 ? entity.ScaleX : GetRawScale(entity.Model),
                entity.ScaleY != 0 ? entity.ScaleY : GetRawScale(entity.Model),
                entity.ScaleZ != 0 ? entity.ScaleZ : GetRawScale(entity.Model)
            );
        public static AABB CalcAABB(Entity entity)
        {
            string model = GetRawModel(entity.Model);
            AABB bb;
            if (ushort.TryParse(model, out ushort raw) && raw <= 767)
            {
                ushort block = Block.FromRaw(raw);
                bb = Block.BlockAABB(block, entity.Level);
                bb = bb.Offset(-16, 0, -16);
            }
            else
            {
                bb = AABB.Make(new(0, 0, 0), Get(model).BaseSize);
            }
            bb = bb.Expand(-1);
            Vec3F32 scale = CalcScale(entity);
            float max = DefaultMaxScale(model);
            scale.X = Math.Min(scale.X, max);
            scale.Y = Math.Min(scale.Y, max);
            scale.Z = Math.Min(scale.Z, max);
            bb.Min.X = (int)(bb.Min.X * scale.X); 
            bb.Max.X = (int)(bb.Max.X * scale.X);
            bb.Min.Y = (int)(bb.Min.Y * scale.Y);
            bb.Max.Y = (int)(bb.Max.Y * scale.Y);
            bb.Min.Z = (int)(bb.Min.Z * scale.Z);
            bb.Max.Z = (int)(bb.Max.Z * scale.Z);
            return bb;
        }
        /// <summary> Gives distance (in half-pixel world units) from feet to camera height </summary>
        public static int CalcEyeHeight(Entity entity)
        {
            Vec3F32 scale = CalcScale(entity);
            string model = GetRawModel(entity.Model);
            if (ushort.TryParse(model, out ushort raw) && raw <= 767) return 16; 
            float eyeHeight = Get(model).EyeHeight;
            eyeHeight *= scale.Y;
            eyeHeight *= 2f; 
            return (int)eyeHeight;
        }
    }
}
