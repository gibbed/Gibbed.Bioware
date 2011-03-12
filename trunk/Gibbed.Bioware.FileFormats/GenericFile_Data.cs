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
    public class GenericFile_Data : GenericFile
    {
        protected class ExportState
        {
            public Dictionary<long, object> References
                = new Dictionary<long, object>();
        }

        protected class ImportState
        {
        }

        public void Import(KeyValue root)
        {
            this.Data = new MemoryStream();
            this.FileVersion = 0;

            long newOffset = this.Structures[0].DataSize;
            this.ImportStructure(this.Structures[0], root, 0, ref newOffset, new ImportState());
        }

        private void ImportStructure(GFF.StructureDefinition def, KeyValue data, long offset, ref long newOffset, ImportState state)
        {
            foreach (var fieldDef in def.Fields)
            {
                if (fieldDef.Id == 16208)
                {
                }

                KeyValue value;
                if (data.Values.ContainsKey(fieldDef.Id) == false)
                {
                    //throw new InvalidOperationException();
                    value = new KeyValue(fieldDef.Type, null);
                }
                else
                {
                    value = data[fieldDef.Id];
                }
                this.ImportField(fieldDef, value, offset, ref newOffset, state);
            }
        }

        private void ImportField(GFF.FieldDefinition def, KeyValue data, long offset, ref long newOffset, ImportState state)
        {
            var output = this.Data;
            output.Seek(offset + def.Offset, SeekOrigin.Begin);

            if (def.IsList == true)
            {
                if (def.IsReference == true &&
                    (data.Value != null || def.Type != GFF.FieldType.Generic))
                {
                    throw new NotSupportedException();
                }

                var list = (IList)data.Value;
                if (list == null)
                {
                    output.WriteValueU32(0xFFFFFFFF, LittleEndian);
                }
                else
                {
                    output.WriteValueU32((uint)newOffset, LittleEndian);

                    output.Seek(newOffset, SeekOrigin.Begin);
                    output.WriteValueS32(list.Count, LittleEndian);
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

                    newOffset += list.Count * itemSize;

                    switch (def.Type)
                    {
                        case GFF.FieldType.String:
                        {
                            throw new NotImplementedException();
                        }

                        case GFF.FieldType.Structure:
                        {
                            var subdef = this.Structures[def.StructureId];

                            long itemOffset = output.Position;
                            foreach (var item in list)
                            {
                                this.ImportStructure(
                                    subdef,
                                    (KeyValue)item,
                                    itemOffset,
                                    ref newOffset,
                                    state);
                                itemOffset += itemSize;
                            }

                            break;
                        }

                        default:
                        {
                            if (def.Type == GFF.FieldType.UInt8 &&
                                list.GetType() == typeof(byte[]))
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
            }
            else
            {
                if (def.IsReference == true &&
                    def.Type != GFF.FieldType.Structure)
                {
                    throw new NotSupportedException();
                }

                switch (def.Type)
                {
                    case GFF.FieldType.String:
                    {
                        var s = data.As<string>();

                        if (s == null || s.Length == 0)
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

                    case GFF.FieldType.TalkString:
                    {
                        var s = data.As<GFF.Builtins.TalkString>();
                        output.WriteValueU32(s.Id, LittleEndian);

                        if (s.String == null)
                        {
                            output.WriteValueU32(0xFFFFFFFF, LittleEndian);
                        }
                        else if (s.String.Length == 0)
                        {
                            output.WriteValueU32(0, LittleEndian);
                        }
                        else
                        {
                            var length = s.String.Length + 1;

                            output.WriteValueU32((uint)newOffset, LittleEndian);

                            output.Seek(newOffset, SeekOrigin.Begin);
                            output.WriteValueS32(length, LittleEndian);
                            output.WriteString(s.String, LittleEndian ? Encoding.Unicode : Encoding.BigEndianUnicode);
                            output.WriteValueU16(0, LittleEndian);
                            newOffset += 4 + (2 * length);
                        }

                        break;
                    }

                    case GFF.FieldType.Structure:
                    {
                        if (def.IsReference == true)
                        {
                            if (data == null || data.Values == null)
                            {
                                output.WriteValueU32(0xFFFFFFFF, LittleEndian);
                            }
                            else
                            {
                                output.WriteValueU32((uint)newOffset, LittleEndian);
                                output.Seek(newOffset, SeekOrigin.Begin);
                                
                                var subdef = this.Structures[def.StructureId];
                                newOffset += subdef.DataSize;

                                this.ImportStructure(
                                    subdef,
                                    data,
                                    output.Position,
                                    ref newOffset,
                                    state);
                            }
                        }
                        else
                        {
                            var subdef = this.Structures[def.StructureId];
                            this.ImportStructure(
                            subdef,
                            data,
                            output.Position,
                            ref newOffset,
                            state);
                        }
                        break;
                    }

                    default:
                    {
                        GFF.Builtin.Serialize(output, def.Type, data.Value, LittleEndian);
                        break;
                    }
                }
            }
        }

        public KeyValue Export()
        {
            if (this.Structures.Count == 0)
            {
                return null;
            }

            return this.ExportStructure(this.Structures[0], 0, new ExportState());
        }

        private KeyValue ExportStructure(GFF.StructureDefinition def, long offset, ExportState state)
        {
            var kv = new KeyValue(GFF.FieldType.Structure, null);
            kv.StructureId = def.Id;

            foreach (var fieldDef in def.Fields)
            {
                var value = this.ExportField(fieldDef, offset, state);

                if (value is KeyValue)
                {
                    kv[fieldDef.Id] = (KeyValue)value;
                }
                else
                {
                    kv[fieldDef.Id] = new KeyValue(fieldDef.Type, value);
                }
            }
            return kv;
        }

        private object ExportGeneric(ExportState state)
        {
            var input = this.Data;
            var type = input.ReadValueU16(LittleEndian);
            var flags = (GFF.FieldFlags)input.ReadValueU16(LittleEndian);
            var offset = input.ReadValueU32(LittleEndian);

            if (state.References.ContainsKey(offset) == true)
            {
                return state.References[offset];
            }

            var def = new GFF.FieldDefinition();
            def.Id = 0;
            def.Offset = 0;

            if (flags.HasFlag(GFF.FieldFlags.IsStructure) == true)
            {
                flags &= ~GFF.FieldFlags.IsStructure;
                def.Type = GFF.FieldType.Structure;
                def.StructureId = type;
            }
            else
            {
                def.Type = (GFF.FieldType)type;
            }

            def.Flags = flags;

            var instance = this.ExportField(def, offset, state);
            state.References.Add(offset, instance);
            return instance;
        }

        private object ExportField(GFF.FieldDefinition def, long offset, ExportState state)
        {
            var input = this.Data;
            input.Seek(offset + def.Offset, SeekOrigin.Begin);

            if (def.IsReference == true && def.IsList == true)
            {
                var listOffset = input.ReadValueU32(LittleEndian);
                if (listOffset == 0xFFFFFFFF)
                {
                    return null;
                }

                input.Seek(listOffset, SeekOrigin.Begin);
                var count = input.ReadValueU32(LittleEndian);

                long itemOffset = input.Position;
                var list = new List<object>();
                for (uint i = 0; i < count; i++)
                {
                    list.Add(this.ExportGeneric(state));
                    itemOffset += 8;
                    input.Seek(itemOffset, SeekOrigin.Begin);
                }
                return list;
            }
            else if (def.IsList == true)
            {
                //var type = GFF.Builtin.ToNativeType(def.Type);

                var listOffset = input.ReadValueU32(LittleEndian);
                if (listOffset == 0xFFFFFFFF)
                {
                    /*if (def.Type == GFF.FieldType.UInt8)
                    {
                        return new byte[0];
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }*/
                    return null;
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
                        var subdef = this.Structures[def.StructureId];
                        var list = new List<KeyValue>();
                        for (uint i = 0; i < count; i++)
                        {
                            list.Add(this.ExportStructure(
                                subdef, itemOffset, state));
                            itemOffset += subdef.DataSize;
                        }
                        return list;
                    }

                    default:
                    {
                        if (def.Type == GFF.FieldType.UInt8)
                        {
                            var list = new byte[count];
                            input.Read(list, 0, list.Length);
                            return list;
                        }
                        else
                        {
                            var type = GFF.Builtin.ToNativeType(def.Type);
                            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
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
                if (def.IsReference == true)
                {
                    var referenceOffset = input.ReadValueU32(LittleEndian);
                    if (referenceOffset == 0xFFFFFFFF)
                    {
                        return null;
                    }
                    else if (state.References.ContainsKey(referenceOffset) == true)
                    {
                        return state.References[referenceOffset];
                    }

                    input.Seek(referenceOffset, SeekOrigin.Begin);
                }

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

                    case GFF.FieldType.TalkString:
                    {
                        var tlk = new GFF.Builtins.TalkString();
                        tlk.Id = input.ReadValueU32(LittleEndian);

                        var dataOffset = input.ReadValueU32(LittleEndian);
                        if (dataOffset == 0xFFFFFFFF)
                        {
                            tlk.String = null;
                        }
                        else if (dataOffset == 0)
                        {
                            tlk.String = "";
                        }
                        else
                        {
                            if (this.FileVersion < 1)
                            {
                                input.Seek(dataOffset, SeekOrigin.Begin);
                                var count = input.ReadValueU32(LittleEndian);
                                tlk.String = input.ReadString(count * 2, true,
                                    LittleEndian == true ? Encoding.Unicode : Encoding.BigEndianUnicode);
                            }
                            else
                            {
                                tlk.String = this.StringTable[(int)dataOffset];
                            }
                        }

                        return tlk;
                    }

                    case GFF.FieldType.Structure:
                    {
                        var subdef = this.Structures[def.StructureId];
                        return this.ExportStructure(subdef, input.Position, state);
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
