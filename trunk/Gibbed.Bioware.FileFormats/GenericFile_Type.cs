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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gibbed.Helpers;
using GFF = Gibbed.Bioware.FileFormats.GenericFileFormat;

namespace Gibbed.Bioware.FileFormats
{
    public class GenericFile_Type : GenericFile
    {

        private static bool ValidateStructure(
            GFF.StructureDefinition def,
            Type nativeType)
        {
            // i'm too lazy to write this code right now :)
            return true;
        }

        private static bool ValidateField(
            GFF.FieldDefinition def,
            Type nativeType)
        {
            // i'm too lazy to write this code right now :)
            return true;
        }

        private void ImportTypeToStructures(Type root)
        {
            var structs = new List<GFF.StructureDefinition>();
            var map = new Dictionary<Type, GFF.StructureDefinition>();

            var types = new List<Type>();
            var queue = new Queue<Type>();
            queue.Enqueue(root);

            // discover non-native types we need to add
            while (queue.Count > 0)
            {
                var type = queue.Dequeue();
                types.Add(type);

                foreach (var field in type.GetFields())
                {
                    var subtype = field.FieldType;

                    if (GFF.Builtin.FromNativeType(subtype) != GFF.FieldType.Structure)
                    {
                        continue;
                    }

                    if (subtype.IsGenericType == true &&
                        subtype.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        subtype = subtype.GetGenericArguments()[0];
                    }

                    if (types.Contains(subtype) == true ||
                        queue.Contains(subtype) == true)
                    {
                        continue;
                    }

                    queue.Enqueue(subtype);
                }
            }

            if (types.Count > 0xFFFF)
            {
                throw new InvalidOperationException();
            }

            // now define them
            foreach (var type in types)
            {
                var structDef = new GFF.StructureDefinition();
                var reflected = GFF.ReflectedStructureType.For(type);

                map.Add(type, structDef);
                structs.Add(structDef);

                structDef.Id = reflected.Id;

                uint offset = 0;
                foreach (var kvp in reflected.Fields)
                {
                    var subtype = kvp.Value.Field.FieldType;

                    var fieldDef = new GFF.FieldDefinition();

                    if (subtype.IsGenericType == true &&
                        subtype.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        subtype = subtype.GetGenericArguments()[0];
                        fieldDef.Flags |= GFF.FieldFlags.IsList;
                    }
                    else if (subtype.IsArray == true)
                    {
                        fieldDef.Flags |= GFF.FieldFlags.IsList;
                    }

                    fieldDef.Id = kvp.Key;
                    fieldDef.Type = GFF.Builtin
                        .FromNativeType(subtype, kvp.Value.Type);
                    fieldDef.Offset = offset;
                    fieldDef.NativeType = subtype;

                    structDef.Fields.Add(fieldDef);

                    if (fieldDef.Flags.HasFlag(GFF.FieldFlags.IsList) == true)
                    {
                        offset += 4;
                    }
                    else if (fieldDef.Type == GFF.FieldType.Structure)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        offset += GFF.Builtin.SizeOf(fieldDef.Type);
                    }
                }

                structDef.DataSize = offset;
            }

            // update ids
            foreach (var type in types)
            {
                var structDef = map[type];

                foreach (var fieldDef in structDef.Fields)
                {
                    if (fieldDef.Type == GFF.FieldType.Structure)
                    {
                        var structRef = map[fieldDef.NativeType];

                        int index = structs.IndexOf(structRef);
                        if (index < 0)
                        {
                            throw new InvalidOperationException();
                        }

                        fieldDef.StructureReference = structRef;
                        fieldDef.StructureId = (ushort)index;
                    }
                }
            }

            this.Structures = structs;
        }

        public void ImportType<TType>(TType instance)
            where TType : class
        {
            this.ImportTypeToStructures(typeof(TType));
            this.Data = new MemoryStream();

            this.FileVersion = 0;

            long newOffset = this.Structures[0].DataSize;
            this.ImportTypeStructure(this.Structures[0], instance, 0, ref newOffset);
        }

        private void ImportTypeStructure(GFF.StructureDefinition def, object instance, long offset, ref long newOffset)
        {
            var type = instance.GetType();

            if (ValidateStructure(def, type) == false)
            {
                throw new FormatException("structure validation failed");
            }

            var reflected = GFF.ReflectedStructureType.For(type);

            foreach (var fieldDef in def.Fields)
            {
                var value = reflected.GetField(instance, fieldDef.Id);
                this.ImportTypeField(fieldDef, reflected.GetFieldType(fieldDef.Id), value, offset, ref newOffset);
            }
        }

        private void ImportTypeField(GFF.FieldDefinition def, Type type, object value, long offset, ref long newOffset)
        {
            if (ValidateField(def, type) == false)
            {
                throw new FormatException("field validation failed");
            }

            var output = this.Data;
            output.Seek(offset + def.Offset, SeekOrigin.Begin);

            if (def.IsList == true)
            {
                output.WriteValueU32((uint)newOffset, LittleEndian);

                output.Seek(newOffset, SeekOrigin.Begin);
                output.WriteValueS32(((IList)value).Count, LittleEndian);
                newOffset += 4;

                uint itemSize;
                if (def.Type == GFF.FieldType.Structure)
                {
                    var subdef = this.Structures[def.StructureId];
                    itemSize = subdef.DataSize;
                }
                else
                {
                    itemSize = GFF.Builtin.SizeOf(def.Type);
                }

                var list = (IList)value;
                newOffset += list.Count * itemSize;

                switch (def.Type)
                {
                    case GFF.FieldType.String:
                    {
                        throw new NotImplementedException();
                    }

                    case GFF.FieldType.Structure:
                    {
                        var subtype = type.GetGenericArguments()[0];
                        var subdef = this.Structures[def.StructureId];

                        long itemOffset = output.Position;
                        foreach (var item in list)
                        {
                            this.ImportTypeStructure(
                                subdef,
                                item,
                                itemOffset,
                                ref newOffset);
                            itemOffset += itemSize;
                        }

                        break;
                    }

                    default:
                    {
                        if (def.Type == GFF.FieldType.UInt8 &&
                            type == typeof(byte[]))
                        {
                            var bytes = (byte[])list;
                            output.Write(bytes, 0, bytes.Length);
                        }
                        else
                        {
                            long itemOffset = output.Position;
                            foreach (var item in list)
                            {
                                GFF.Builtin.Serialize(output, def.Type, item, LittleEndian);
                                itemOffset += itemSize;
                            }
                        }

                        break;
                    }
                }
            }
            else
            {
                switch (def.Type)
                {
                    case GFF.FieldType.String:
                    {
                        var s = (string)value;

                        if (s.Length == 0)
                        {
                            output.WriteValueU32(0xFFFFFFFF, LittleEndian);
                        }
                        else
                        {
                            var length = s.Length + 1;

                            output.WriteValueU32((uint)newOffset, LittleEndian);

                            output.Seek(newOffset, SeekOrigin.Begin);
                            output.WriteValueS32(length, LittleEndian);
                            output.WriteString(s, LittleEndian ? Encoding.Unicode : Encoding.BigEndianUnicode);
                            output.WriteValueU16(0, LittleEndian);
                            newOffset += 4 + (2 * length);
                        }

                        break;
                    }

                    case GFF.FieldType.Structure:
                    {
                        var subdef = this.Structures[def.StructureId];
                        this.ImportTypeStructure(
                            subdef,
                            value,
                            output.Position,
                            ref newOffset);
                        break;
                    }

                    default:
                    {
                        GFF.Builtin.Serialize(output, def.Type, value, LittleEndian);
                        break;
                    }
                }
            }
        }

        public TType ExportType<TType>()
            where TType : class, new()
        {
            if (this.Structures.Count == 0)
            {
                return null;
            }

            return this.ExportTypeStructure<TType>(this.Structures[0], 0);
        }

        private TType ExportTypeStructure<TType>(GFF.StructureDefinition def, long offset)
            where TType : class, new()
        {
            return (TType)ExportTypeStructure(def, typeof(TType), offset);
        }

        private object ExportTypeStructure(GFF.StructureDefinition def, Type type, long offset)
        {
            if (ValidateStructure(def, type) == false)
            {
                throw new FormatException("structure validation failed");
            }

            var instance = Activator.CreateInstance(type);
            var reflected = GFF.ReflectedStructureType.For(type);

            foreach (var fieldDef in def.Fields)
            {
                var value = this.ExportTypeField(
                    fieldDef, reflected.GetFieldType(fieldDef.Id), offset);
                reflected.SetField(instance, fieldDef.Id, value);
            }

            return instance;
        }

        private object ExportTypeField(GFF.FieldDefinition def, Type type, long offset)
        {
            if (ValidateField(def, type) == false)
            {
                throw new FormatException("field validation failed");
            }

            var input = this.Data;
            input.Seek(offset + def.Offset, SeekOrigin.Begin);

            if (def.IsReference == true)
            {
                throw new NotSupportedException();
            }

            if (def.IsList == true)
            {
                var listOffset = input.ReadValueU32(LittleEndian);
                if (listOffset == 0xFFFFFFFF)
                {
                    if (def.Type == GFF.FieldType.UInt8 &&
                        type == typeof(byte[]))
                    {
                        return new byte[0];
                    }
                    else
                    {
                        return Activator.CreateInstance(type);
                    }
                }

                input.Seek(listOffset, SeekOrigin.Begin);
                var count = input.ReadValueU32(LittleEndian);

                switch (def.Type)
                {
                    case GFF.FieldType.String:
                    {
                        long itemOffset = input.Position;
                        var list = new List<string>();
                        for (uint i = 0; i < count; i++)
                        {
                            var dataOffset = input.ReadValueU32(LittleEndian);
                            if (dataOffset == 0xFFFFFFFF)
                            {
                                return "";
                            }

                            if (this.FileVersion < 1)
                            {
                                input.Seek(dataOffset, SeekOrigin.Begin);
                                var length = input.ReadValueU32(LittleEndian);
                                list.Add(input.ReadString(length * 2, true,
                                    LittleEndian == true ? Encoding.Unicode : Encoding.BigEndianUnicode));
                            }
                            else
                            {
                                input.Seek(dataOffset, SeekOrigin.Begin);
                                list.Add(this.StringTable[(int)dataOffset]);
                            }

                            itemOffset += 4;
                            input.Seek(itemOffset, SeekOrigin.Begin);
                        }

                        return list;
                    }

                    case GFF.FieldType.Structure:
                    {
                        long itemOffset = input.Position;
                        var subtype = type.GetGenericArguments()[0];
                        var subdef = this.Structures[def.StructureId];
                        var list = (IList)Activator.CreateInstance(type);
                        for (uint i = 0; i < count; i++)
                        {
                            list.Add(this.ExportTypeStructure(
                                subdef,
                                subtype,
                                itemOffset));
                            itemOffset += subdef.DataSize;
                        }
                        return list;
                    }

                    default:
                    {
                        if (def.Type == GFF.FieldType.UInt8 &&
                            type == typeof(byte[]))
                        {
                            var list = new byte[count];
                            input.Read(list, 0, list.Length);
                            return list;
                        }
                        else
                        {
                            var list = (IList)Activator.CreateInstance(type);
                            for (uint i = 0; i < count; i++)
                            {
                                list.Add(GFF.Builtin.Deserialize(input, def.Type, LittleEndian));
                            }
                            return list;
                        }
                    }
                }
            }
            else
            {
                switch (def.Type)
                {
                    case GFF.FieldType.String:
                    {
                        var dataOffset = input.ReadValueU32(LittleEndian);
                        if (dataOffset == 0xFFFFFFFF)
                        {
                            return "";
                        }

                        if (this.FileVersion < 1)
                        {
                            input.Seek(dataOffset, SeekOrigin.Begin);
                            var count = input.ReadValueU32(LittleEndian);
                            return input.ReadString(count * 2, true,
                                LittleEndian == true ? Encoding.Unicode : Encoding.BigEndianUnicode);
                        }
                        else
                        {
                            return this.StringTable[(int)dataOffset];
                        }
                    }

                    case GFF.FieldType.Structure:
                    {
                        var subdef = this.Structures[def.StructureId];
                        return this.ExportTypeStructure(subdef, type, input.Position);
                    }

                    default:
                    {
                        return GFF.Builtin.Deserialize(input, def.Type, LittleEndian);
                    }
                }
            }
        }
    }
}
