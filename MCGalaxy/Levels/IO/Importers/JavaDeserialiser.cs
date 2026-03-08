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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
namespace MCGalaxy.Levels.IO
{
    public class JClassDesc
    {
        public string Name;
        public byte Flags;
        public JFieldDesc[] Fields;
        public JClassDesc SuperClass;
    }
    public class JClassData
    {
        public object[] Values;
    }
    public class JClass
    {
        public JClassDesc Desc;
    }
    public class JObject
    {
        public JClassDesc Desc;
        public JClassData[] ClassData;
    }
    public class JArray
    {
        public JClassDesc Desc;
        public object Values;
    }
    public class JFieldDesc
    {
        public char Type;
        public string Name, ClassName;
    }
    public class JEnum
    {
        public JClassDesc Desc;
        public object Name;
        public override string ToString() => Desc.Name + "." + Name;
    }
    public sealed class JavaReader
    {
        public BinaryReader src;
        public List<object> handles = new();
        public byte[] ReadBytes(int count) => src.ReadBytes(count);
        public byte ReadUInt8() => src.ReadByte();
        public short ReadInt16() => IPAddress.HostToNetworkOrder(src.ReadInt16());
        public ushort ReadUInt16() => (ushort)IPAddress.HostToNetworkOrder(src.ReadInt16());
        public int ReadInt32() => IPAddress.HostToNetworkOrder(src.ReadInt32());
        public long ReadInt64() => IPAddress.HostToNetworkOrder(src.ReadInt64());
        public string ReadUtf8() => Encoding.UTF8.GetString(src.ReadBytes(ReadUInt16()));
        public object ReadObject() => ReadObject(ReadUInt8());
        object ReadObject(byte typeCode) => typeCode switch
        {
            0x74 => NewString(),
            0x70 => null,
            0x71 => PrevObject(),
            0x73 => NewObject(),
            0x75 => NewArray(),
            0x7E => NewEnum(),
            0x76 => NewClass(),
            _ => throw new InvalidDataException("Invalid typecode: " + typeCode),
        };
        string NewString()
        {
            string value = ReadUtf8();
            handles.Add(value);
            return value;
        }
        object PrevObject()
        {
            int handle = ReadInt32() - 0x7E0000;
            return handle >= 0 && handle < handles.Count ? handles[handle] : throw new InvalidDataException("Invalid stream handle: " + handle);
        }
        JObject NewObject()
        {
            JObject obj = new()
            {
                Desc = ClassDesc()
            };
            handles.Add(obj);
            List<JClassDesc> descs = new();
            JClassDesc tmp = obj.Desc;
            while (tmp != null)
            {
                descs.Add(tmp);
                tmp = tmp.SuperClass;
            }
            obj.ClassData = new JClassData[descs.Count];
            for (int i = descs.Count - 1; i >= 0; i--)
                obj.ClassData[i] = ClassData(descs[i]);
            return obj;
        }
        JArray NewArray()
        {
            JArray array = new()
            {
                Desc = ClassDesc()
            };
            handles.Add(array);
            char type = array.Desc.Name[1];
            int size = ReadInt32();
            switch (type)
            {
                case 'B':
                    array.Values = ReadBytes(size);
                    break;
                default:
                    {
                        object[] values = new object[size];
                        for (int i = 0; i < values.Length; i++)
                            values[i] = Value(type);
                        array.Values = values;
                        break;
                    }
            }
            return array;
        }
        JClassDesc NewClassDesc()
        {
            JClassDesc desc = new()
            {
                Name = ReadUtf8()
            };
            ReadInt64();
            handles.Add(desc);
            desc.Flags = ReadUInt8();
            desc.Fields = new JFieldDesc[ReadUInt16()];
            for (int i = 0; i < desc.Fields.Length; i++)
                desc.Fields[i] = FieldDesc();
            SkipAnnotation();
            desc.SuperClass = ClassDesc();
            return desc;
        }
        JEnum NewEnum()
        {
            JEnum je = new()
            {
                Desc = ClassDesc()
            };
            handles.Add(je);
            je.Name = ReadObject();
            return je;
        }
        JClass NewClass()
        {
            JClass jc = new()
            {
                Desc = ClassDesc()
            };
            handles.Add(jc);
            return jc;
        }
        JClassDesc ClassDesc()
        {
            byte typeCode = ReadUInt8();
            return typeCode == 0x72
                ? NewClassDesc()
                : typeCode == 0x70
                ? null
                : typeCode == 0x71 ? (JClassDesc)PrevObject() : throw new InvalidDataException("Invalid type code: " + typeCode);
        }
        JClassData ClassData(JClassDesc desc)
        {
            if ((desc.Flags & 0x02) == 0)
                throw new InvalidDataException("Invalid class data flags: " + desc.Flags);
            JClassData data = new()
            {
                Values = new object[desc.Fields.Length]
            };
            for (int i = 0; i < data.Values.Length; i++)
                data.Values[i] = Value(desc.Fields[i].Type);
            if ((desc.Flags & 0x01) != 0)
                SkipAnnotation();
            return data;
        }
        unsafe object Value(char type)
        {
            switch (type)
            {
                case 'B':
                    return ReadUInt8();
                case 'C':
                    return (char)ReadUInt16();
                case 'D':
                    {
                        long tmp = ReadInt64();
                        return *(double*)&tmp;
                    }
                case 'F':
                    {
                        int tmp = ReadInt32();
                        return *(float*)&tmp;
                    }
                default:
                    return type switch
                    {
                        'I' => ReadInt32(),
                        'J' => ReadInt64(),
                        _ => type == 'S'
                        ? ReadInt16()
                        : type == 'Z'
                        ? ReadUInt8() != 0
                        : type == 'L' ? ReadObject() : type == '[' ? ReadObject() : throw new InvalidDataException("Invalid value code: " + type)
                    };
            }
        }
        JFieldDesc FieldDesc()
        {
            JFieldDesc desc = new();
            byte type = ReadUInt8();
            desc.Type = (char)type;
            if (type == 'B' || type == 'C' || type == 'D' || type == 'F' || type == 'I' || type == 'J' || type == 'S' || type == 'Z')
                desc.Name = ReadUtf8();
            else if (type == '[' || type == 'L')
            {
                desc.Name = ReadUtf8();
                desc.ClassName = (string)ReadObject();
            }
            else
                throw new InvalidDataException("Invalid field type: " + type);
            return desc;
        }
        void SkipAnnotation()
        {
            byte typeCode;
            while ((typeCode = ReadUInt8()) != 0x78)
                switch (typeCode)
                {
                    case 0x77:
                        ReadBytes(ReadUInt8());
                        break;
                    default:
                        ReadObject(typeCode);
                        break;
                }
        }
    }
}
