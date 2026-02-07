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
using System.Runtime.InteropServices;
namespace MCGalaxy.Blocks.Physics
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PhysicsArgs
    {
        public uint Raw;
        public byte Type1
        {
            readonly get { return (byte)(Raw & 0x07); }
            set { Raw &= ~(uint)0x07; Raw |= (uint)value << 0; }
        }
        public byte Type2
        {
            readonly get { return (byte)((Raw >> 3) & 0x07); }
            set { Raw &= ~((uint)0x07 << 3); Raw |= (uint)value << 3; }
        }
        public byte Value1
        {
            readonly get { return (byte)(Raw >> 6); }
            set { Raw &= ~(((uint)0xFF) << 6); Raw |= (uint)value << 6; }
        }
        public byte Value2
        {
            readonly get { return (byte)(Raw >> 14); }
            set { Raw &= ~((uint)0xFF << 14); Raw |= (uint)value << 14; }
        }
        public byte Data
        {
            readonly get { return (byte)(Raw >> 22); }
            set { Raw &= ~((uint)0xFF << 22); Raw |= (uint)value << 22; }
        }
        public byte ExtBlock
        {
            readonly get { return (byte)(Raw >> 30); }
            set { Raw &= ~((1u << 30) | (1u << 31)); Raw |= (uint)value << 30; }
        }
        public readonly bool HasWait => (Raw & 0x07) == 1 || ((Raw >> 3) & 0x07) == 1;
        public void ResetTypes() => Raw &= ~(uint)0x3F;
    }
}