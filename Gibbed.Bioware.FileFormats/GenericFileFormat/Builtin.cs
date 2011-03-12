/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Bioware.FileFormats.GenericFileFormat
{
    public static class Builtin
    {
        public static FieldType FromNativeType(Type type)
        {
            return FromNativeType(type, FieldType.Default);
        }

        public static Type ToNativeType(FieldType type)
        {
            switch (type)
            {
                case FieldType.UInt8: return typeof(byte);
                case FieldType.Int8: return typeof(sbyte);
                case FieldType.UInt16: return typeof(ushort);
                case FieldType.Int16: return typeof(short);
                case FieldType.UInt32: return typeof(uint);
                case FieldType.Int32: return typeof(int);
                case FieldType.UInt64: return typeof(ulong);
                case FieldType.Int64: return typeof(long);
                case FieldType.Single: return typeof(float);
                case FieldType.Double: return typeof(double);
                case FieldType.Vector3: return typeof(Builtins.Vector3);
                case FieldType.Vector4: return typeof(Builtins.Vector4);
                case FieldType.Quaternion: return typeof(Builtins.Quaternion);
                case FieldType.String: return typeof(string);
                case FieldType.Color: return typeof(Builtins.Color);
                case FieldType.Matrix4x4: return typeof(Builtins.Matrix4x4);
                case FieldType.TalkString: return typeof(Builtins.TalkString);
            }

            throw new NotSupportedException();
        }

        public static FieldType FromNativeType(Type type, FieldType other)
        {
            if (other != FieldType.Default)
            {
                if (other == FieldType.Unknown ||
                    other == FieldType.Generic)
                {
                    throw new InvalidOperationException();
                }

                return other;
            }

            if (type.IsGenericType == true &&
                type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }

            if (type == typeof(byte) || type == typeof(byte[]))
            {
                return FieldType.UInt8;
            }
            else if (type == typeof(sbyte))
            {
                return FieldType.Int8;
            }
            else if (type == typeof(ushort))
            {
                return FieldType.UInt16;
            }
            else if (type == typeof(short))
            {
                return FieldType.Int16;
            }
            else if (type == typeof(uint))
            {
                return FieldType.UInt32;
            }
            else if (type == typeof(int))
            {
                return FieldType.Int32;
            }
            else if (type == typeof(ulong))
            {
                return FieldType.UInt64;
            }
            else if (type == typeof(long))
            {
                return FieldType.Int64;
            }
            else if (type == typeof(float))
            {
                return FieldType.Single;
            }
            else if (type == typeof(double))
            {
                return FieldType.Double;
            }
            else if (type == typeof(Builtins.Vector3))
            {
                return FieldType.Vector3;
            }
            else if (type == typeof(Builtins.Vector4))
            {
                return FieldType.Vector4;
            }
            else if (type == typeof(Builtins.Quaternion))
            {
                return FieldType.Quaternion;
            }
            else if (type == typeof(string))
            {
                return FieldType.String;
            }
            else if (type == typeof(Builtins.Color))
            {
                return FieldType.Color;
            }
            else if (type == typeof(Builtins.Matrix4x4))
            {
                return FieldType.Matrix4x4;
            }
            else if (type == typeof(Builtins.TalkString))
            {
                return FieldType.TalkString;
            }
            else if (type.IsClass == true)
            {
                return FieldType.Structure;
            }

            throw new NotSupportedException();
        }

        public static uint SizeOf(FieldType type)
        {
            switch (type)
            {
                case FieldType.UInt8: return 1;
                case FieldType.Int8: return 1;
                case FieldType.UInt16: return 2;
                case FieldType.Int16: return 2;
                case FieldType.UInt32: return 4;
                case FieldType.Int32: return 4;
                case FieldType.UInt64: return 8;
                case FieldType.Int64: return 8;
                case FieldType.Single: return 4;
                case FieldType.Double: return 8;
                case FieldType.Vector3: return 4 * 3;
                case FieldType.Vector4: return 4 * 4;
                case FieldType.Quaternion: return 4 * 4;
                case FieldType.String: return 4;
                case FieldType.Color: return 4 * 4;
                case FieldType.Matrix4x4: return 4 * 4 * 4;
                case FieldType.TalkString: return 8;
            }

            throw new NotSupportedException();
        }

        public static void Serialize(Stream output, FieldType type, object value, bool littleEndian)
        {
            switch (type)
            {
                case FieldType.UInt8: output.WriteValueU8((byte)value); break;
                case FieldType.Int8: output.WriteValueS8((sbyte)value); break;
                case FieldType.UInt16: output.WriteValueU16((ushort)value); break;
                case FieldType.Int16: output.WriteValueS16((short)value); break;
                case FieldType.UInt32: output.WriteValueU32((uint)value); break;
                case FieldType.Int32: output.WriteValueS32((int)value); break;
                case FieldType.UInt64: output.WriteValueU64((ulong)value); break;
                case FieldType.Int64: output.WriteValueS64((long)value); break;
                case FieldType.Single: output.WriteValueF32((float)value); break;
                case FieldType.Double: output.WriteValueF64((double)value); break;
                case FieldType.Vector3:
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException();
                    }
                    ((Builtins.Vector3)value).Serialize(output, littleEndian);
                    break;
                }
                case FieldType.Vector4:
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException();
                    }
                    ((Builtins.Vector4)value).Serialize(output, littleEndian);
                    break;
                }
                case FieldType.Quaternion:
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException();
                    }
                    ((Builtins.Quaternion)value).Serialize(output, littleEndian);
                    break;
                }
                case FieldType.String:
                {
                    throw new NotSupportedException("cannot serialize strings via Builtin");
                }
                case FieldType.Color:
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException();
                    }
                    ((Builtins.Color)value).Serialize(output, littleEndian);
                    break;
                }
                case FieldType.Matrix4x4:
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException();
                    }
                    ((Builtins.Matrix4x4)value).Serialize(output, littleEndian);
                    break;
                }
                default:
                {
                    throw new NotSupportedException("unsupported builtin type");
                }
            }
        }

        public static object Deserialize(Stream input, FieldType type, bool littleEndian)
        {
            switch (type)
            {
                case FieldType.UInt8: return input.ReadValueU8();
                case FieldType.Int8: return input.ReadValueS8();
                case FieldType.UInt16: return input.ReadValueU16(littleEndian);
                case FieldType.Int16: return input.ReadValueS16(littleEndian);
                case FieldType.UInt32: return input.ReadValueU32(littleEndian);
                case FieldType.Int32: return input.ReadValueS32(littleEndian);
                case FieldType.UInt64: return input.ReadValueU64(littleEndian);
                case FieldType.Int64: return input.ReadValueS64(littleEndian);
                case FieldType.Single: return input.ReadValueF32(littleEndian);
                case FieldType.Double: return input.ReadValueF64(littleEndian);
                case FieldType.Vector3:
                {
                    var value = new Builtins.Vector3();
                    value.Deserialize(input, littleEndian);
                    return value;
                }
                case FieldType.Vector4:
                {
                    var value = new Builtins.Vector4();
                    value.Deserialize(input, littleEndian);
                    return value;
                }
                case FieldType.Quaternion:
                {
                    var value = new Builtins.Quaternion();
                    value.Deserialize(input, littleEndian);
                    return value;
                }
                case FieldType.String:
                {
                    throw new NotSupportedException("cannot deserialize strings via Builtin");
                }
                case FieldType.Color:
                {
                    var value = new Builtins.Color();
                    value.Deserialize(input, littleEndian);
                    return value;
                }
                case FieldType.Matrix4x4:
                {
                    var value = new Builtins.Matrix4x4();
                    value.Deserialize(input, littleEndian);
                    return value;
                }
            }

            throw new NotSupportedException("unsupported builtin type");
        }
    }
}
