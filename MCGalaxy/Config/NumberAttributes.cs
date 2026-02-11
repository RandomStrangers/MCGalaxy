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
namespace MCGalaxy.Config
{
    public abstract class ConfigIntegerAttribute : ConfigAttribute
    {
        public ConfigIntegerAttribute(string name, string section)
            : base(name, section) { }
        // separate function to avoid boxing in derived classes
        protected int ParseInteger(string raw, int def, int min, int max)
        {
            if (!NumberUtils.TryParseInt32(raw, out int value))
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" has invalid integer '{2}', using default of {1}", Name, def, raw);
                value = def;
            }
            if (value < min)
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too small an integer, using {1}", Name, min);
                value = min;
            }
            if (value > max)
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too big an integer, using {1}", Name, max);
                value = max;
            }
            return value;
        }
        public override string Serialise(object value) => value is int v ? NumberUtils.StringifyInt(v) : base.Serialise(value);
    }
    public sealed class ConfigIntAttribute : ConfigIntegerAttribute
    {
        readonly int defValue, minValue, maxValue;
        public ConfigIntAttribute()
            : this(null, null, 0, int.MinValue, int.MaxValue) { }
        public ConfigIntAttribute(string name, string section, int def,
                                  int min = int.MinValue, int max = int.MaxValue)
            : base(name, section) { defValue = def; minValue = min; maxValue = max; }
        public override object Parse(string value) => ParseInteger(value, defValue, minValue, maxValue);
    }
    public sealed class ConfigBlockAttribute : ConfigIntegerAttribute
    {
        readonly ushort defBlock;
        public ConfigBlockAttribute() : this(null, null, 0) { }
        public ConfigBlockAttribute(string name, string section, ushort def)
            : base(name, section) => defBlock = def;
        public override object Parse(string raw)
        {
            ushort block = (ushort)ParseInteger(raw, defBlock, 0, 1023);
            return block == Block.Invalid ? Block.Invalid : (object)Block.MapOldRaw(block);
        }
    }
    public class ConfigByteAttribute : ConfigIntegerAttribute
    {
        public ConfigByteAttribute() : this(null, null) { }
        public ConfigByteAttribute(string name, string section) : base(name, section) { }
        public override object Parse(string raw) => (byte)ParseInteger(raw, 0, 0, byte.MaxValue);
    }
    public class ConfigUShortAttribute : ConfigIntegerAttribute
    {
        public ConfigUShortAttribute() : this(null, null) { }
        public ConfigUShortAttribute(string name, string section) : base(name, section) { }
        public override object Parse(string raw) => (ushort)ParseInteger(raw, 0, 0, ushort.MaxValue);
    }
    public abstract class ConfigRealAttribute : ConfigAttribute
    {
        public ConfigRealAttribute(string name, string section)
            : base(name, section) { }
        protected double ParseReal(string raw, double def, double min, double max)
        {
            if (!NumberUtils.TryParseDouble(raw, out double value))
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" has invalid number '{2}', using default of {1}", Name, def, raw);
                value = def;
            }
            if (value < min)
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too small a number, using {1}", Name, min);
                value = min;
            }
            if (value > max)
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too big a number, using {1}", Name, max);
                value = max;
            }
            return value;
        }
        public override string Serialise(object value) => value is float v
                ? NumberUtils.StringifyDouble(v)
                : value is double v1 ? NumberUtils.StringifyDouble(v1) : base.Serialise(value);
    }
    public class ConfigFloatAttribute : ConfigRealAttribute
    {
        readonly float defValue, minValue, maxValue;
        public ConfigFloatAttribute()
            : this(null, null, 0, float.NegativeInfinity, float.PositiveInfinity) { }
        public ConfigFloatAttribute(string name, string section, float def,
                                    float min = float.NegativeInfinity, float max = float.PositiveInfinity)
            : base(name, section) { defValue = def; minValue = min; maxValue = max; }
        public override object Parse(string raw) => (float)ParseReal(raw, defValue, minValue, maxValue);
    }
    public class ConfigTimespanAttribute : ConfigRealAttribute
    {
        readonly bool mins; readonly int def;
        public ConfigTimespanAttribute(string name, string section, int def, bool mins)
            : base(name, section) { this.def = def; this.mins = mins; }
        public override object Parse(string raw) => ParseInput(ParseReal(raw, def, 0, int.MaxValue));
        protected TimeSpan ParseInput(double value) => mins ? TimeSpan.FromMinutes(value) : TimeSpan.FromSeconds(value);
        public override string Serialise(object value) => (mins ? ((TimeSpan)value).TotalMinutes : ((TimeSpan)value).TotalSeconds).ToString();
    }
    public class ConfigOptTimespanAttribute : ConfigTimespanAttribute
    {
        public ConfigOptTimespanAttribute(string name, string section, bool mins)
            : base(name, section, -1, mins) { }
        public override object Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            double value = ParseReal(raw, -1, -1, int.MaxValue);
            return value < 0 ? null : ParseInput(value);
        }
        public override string Serialise(object value) => value == null ? "" : base.Serialise(value);
    }
}
