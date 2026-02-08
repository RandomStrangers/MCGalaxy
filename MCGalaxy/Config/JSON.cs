// ClassicalSharp copyright 2014-2016 UnknownShadow200 | Licensed under MIT
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace MCGalaxy.Config
{
    public sealed class JsonArray : List<object> { }
    public sealed class JsonObject : Dictionary<string, object>
    {
        public object Meta;
        public void Deserialise(ConfigElement[] elems, object instance)
        {
            foreach (KeyValuePair<string, object> kvp in this)
            {
                ConfigElement.Parse(elems, instance, kvp.Key, (string)kvp.Value);
            }
        }
    }
    public delegate void JsonOnMember(JsonObject obj, string key, object value);
    /// <summary> Implements a simple JSON parser. </summary>
    public sealed class JsonReader
    {
        public readonly string Value;
        /// <summary> Whether an error occurred while parsing the given JSON. </summary>
        public bool Failed;
        /// <summary> Callback invoked when a member of an object has been parsed. </summary>
        public JsonOnMember OnMember;
        int offset;
        char Cur => Value[offset];
        readonly StringBuilder strBuffer = new(96);
        public JsonReader(string value)
        {
            Value = value;
            OnMember = (obj, key, value) => obj[key] = value;
        }
        static bool IsWhitespace(char c) => c == '\r' || c == '\n' || c == '\t' || c == ' ';
        bool NextConstant(string value)
        {
            if (offset + value.Length > Value.Length) return false;
            for (int i = 0; i < value.Length; i++)
            {
                if (Value[offset + i] != value[i]) return false;
            }
            offset += value.Length;
            return true;
        }
        int NextToken()
        {
            for (; offset < Value.Length && IsWhitespace(Cur); offset++) ;
            if (offset >= Value.Length) return 0;
            char c = Cur; 
            offset++;
            if (c == '{' || c == '}') return c;
            if (c == '[' || c == ']') return c;
            if (c == ',' || c == '"' || c == ':') return c;
            if (IsNumber(c)) return 1;
            offset--;
            if (NextConstant("true")) return 2;
            if (NextConstant("false")) return 3;
            if (NextConstant("null")) return 4;
            // invalid token
            offset++; 
            return 0;
        }
        /// <summary> Parses the given JSON and then returns the root element. </summary>
        /// <returns> Either a JsonObject, a JsonArray, a string, or null </returns>
        public object Parse() => ParseValue(NextToken());
        object ParseValue(int token) => token switch
        {
            '{' => ParseObject(),
            '[' => ParseArray(),
            '"' => ParseString(),
            1 => ParseNumber(),
            2 => "true",
            3 => "false",
            4 => null,
            _ => null,
        };
        JsonObject ParseObject()
        {
            JsonObject obj = new();
            while (true)
            {
                int token = NextToken();
                if (token == ',') continue;
                if (token == '}') return obj;
                if (token != '"') 
                { 
                    Failed = true; 
                    return null;
                }
                string key = ParseString();
                token = NextToken();
                if (token != ':') 
                {
                    Failed = true;
                    return null; 
                }
                token = NextToken();
                if (token == 0) 
                { 
                    Failed = true; 
                    return null;
                }
                OnMember(obj, key, ParseValue(token));
            }
        }
        JsonArray ParseArray()
        {
            JsonArray arr = new();
            while (true)
            {
                int token = NextToken();
                if (token == ',') continue;
                if (token == ']') return arr;
                if (token == 0)
                { 
                    Failed = true;
                    return null; 
                }
                arr.Add(ParseValue(token));
            }
        }
        string ParseString()
        {
            StringBuilder s = strBuffer;
            s.Length = 0;
            for (; offset < Value.Length;)
            {
                char c = Cur; 
                offset++;
                if (c == '"') return s.ToString();
                if (c != '\\') 
                { 
                    s.Append(c);
                    continue; 
                }
                if (offset >= Value.Length) break;
                c = Cur; offset++;
                if (c == '/' || c == '\\' || c == '"') 
                { 
                    s.Append(c); 
                    continue;
                }
                if (c == 'n') 
                { 
                    s.Append('\n'); 
                    continue;
                }
                // TODO any other escape codes to add support for
                if (c != 'u') break;
                if (offset + 4 > Value.Length) break;
                // form of \uYYYY
                int aH = Colors.UnHex(Value[offset + 0]),
                    aL = Colors.UnHex(Value[offset + 1]),
                    bH = Colors.UnHex(Value[offset + 2]),
                    bL = Colors.UnHex(Value[offset + 3]);
                if (aH == -1 || aL == -1 || bH == -1 || bL == -1) break;
                int codePoint = (aH << 12) | (aL << 8) | (bH << 4) | bL;
                s.Append((char)codePoint);
                offset += 4;
            }
            Failed = true; 
            return null;
        }
        static bool IsNumber(char c) => c == '-' || c == '.' || (c >= '0' && c <= '9');
        static bool IsNumberPart(char c) =>
            // same as IsNumber, but also accepts exponential notation (e.g. "3.40E+38")
            c == '-' || c == '.' || (c >= '0' && c <= '9') || c == 'E' || c == '+';
        string ParseNumber()
        {
            int start = offset - 1;
            for (; offset < Value.Length && IsNumberPart(Cur); offset++) ;
            return Value.Substring(start, offset - start);
        }
    }
    public class JsonWriter
    {
        readonly TextWriter w;
        public JsonWriter(TextWriter dst) => w = dst;
        static char Hex(char c, int shift) => (char)(((c >> shift) & 0x0F) <= 9 ? ('0' + ((c >> shift) & 0x0F)) : ('a' + (((c >> shift) & 0x0F) - 10)));
        public void WriteNull() => w.Write("null");
        public void Write(string value) => w.Write(value);
        public void WriteString(string value)
        {
            w.Write('"');
            foreach (char c in value)
            {
                if (c == '/')
                {
                    w.Write("\\/");
                }
                else if (c == '\\')
                {
                    w.Write("\\\\");
                }
                else if (c == '"')
                {
                    w.Write("\\\"");
                }
                else if (c >= ' ' && c <= '~')
                {
                    w.Write(c);
                }
                else
                {
                    w.Write("\\u");
                    w.Write(Hex(c, 12)); w.Write(Hex(c, 8));
                    w.Write(Hex(c, 4)); w.Write(Hex(c, 0));
                }
            }
            w.Write('"');
        }
        public void WriteArray<T>(IList<T> array)
        {
            w.Write("[\r\n");
            string separator = "";
            for (int i = 0; i < array.Count; i++)
            {
                w.Write(separator);
                WriteValue(array[i]);
                separator = ",\r\n";
            }
            w.Write("]\r\n");
        }
        public void WriteObject(object value)
        {
            w.Write("{\r\n");
            SerialiseObject(value);
            w.Write("\r\n}");
        }
        internal void WriteObjectKey(string name)
        {
            Write("    "); 
            WriteString(name);
            Write(": ");
        }
        protected virtual void WriteValue(object value)
        {
            // TODO this is awful code
            if (value == null)
            {
                WriteNull();
            }
            else if (value is int)
            {
                Write(value.ToString());
            }
            else if (value is bool val)
            {
                Write(val ? "true" : "false");
            }
            else if (value is string v)
            {
                WriteString(v);
            }
            else if (value is JsonArray array)
            {
                WriteArray(array);
            }
            else if (value is JsonObject)
            {
                WriteObject(value);
            }
            else
            {
                throw new InvalidOperationException("Unknown datatype: " + value.GetType());
            }
        }
        protected virtual void SerialiseObject(object value)
        {
            string separator = null;
            JsonObject obj = (JsonObject)value;
            foreach (KeyValuePair<string, object> kvp in obj)
            {
                Write(separator);
                WriteObjectKey(kvp.Key);
                WriteValue(kvp.Value);
                separator = ",\r\n";
            }
        }
    }
    public class JsonConfigWriter : JsonWriter
    {
        readonly ConfigElement[] elems;
        public JsonConfigWriter(TextWriter dst, ConfigElement[] cfg) : base(dst) => elems = cfg;
        // Only ever write an object
        protected override void WriteValue(object value) => WriteObject(value);
        void WriteConfigValue(ConfigAttribute a, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                WriteNull();
            }
            else if (a is ConfigBoolAttribute || a is ConfigIntegerAttribute || a is ConfigRealAttribute)
            {
                Write(value);
            }
            else
            {
                WriteString(value);
            }
        }
        protected override void SerialiseObject(object value)
        {
            string separator = null;
            for (int i = 0; i < elems.Length; i++)
            {
                ConfigElement elem = elems[i];
                ConfigAttribute a = elem.Attrib;
                Write(separator);
                WriteObjectKey(a.Name);
                WriteConfigValue(a, elem.Attrib.Serialise(elem.Field.GetValue(value)));
                separator = ",\r\n";
            }
        }
    }
    public static class Json
    {
        /// <summary> Shorthand for serialising an object to a JSON object </summary>
        public static string SerialiseObject(object obj)
        {
            StringWriter dst = new();
            JsonWriter w = new(dst);
            w.WriteObject(obj);
            return dst.ToString();
        }
    }
}
