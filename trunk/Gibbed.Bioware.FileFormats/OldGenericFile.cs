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
using System.Text;
using Gibbed.IO;
using GFF = Gibbed.Bioware.FileFormats.OldGenericFileFormat;

namespace Gibbed.Bioware.FileFormats
{
    // GenericFileFormat (GFF), 3.2
    public class OldGenericFile : IDisposable
    {
        ~OldGenericFile()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private bool Disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (this.Disposed == false)
            {
                if (disposing == true)
                {
                }

                this.Disposed = true;
            }
        }

        public byte FileVersion;
        public GFF.FormatType FormatType;
        protected bool LittleEndian;
        public GFF.StructureData Root;

        public void Deserialize(Stream input)
        {
            input.Seek(0, SeekOrigin.Begin);

            this.FormatType = input.ReadValueEnum<GFF.FormatType>(false);
            var version = input.ReadValueU32(false);
            if (version != 0x56332E32) // 3.2
            {
                throw new FormatException("unsupported version");
            }

            this.FileVersion = (byte)(version - 0x56332E32);

            this.LittleEndian = true;

            var structOffset = input.ReadValueU32(this.LittleEndian);
            var structCount = input.ReadValueU32(this.LittleEndian);
            var fieldOffset = input.ReadValueU32(this.LittleEndian);
            var fieldCount = input.ReadValueU32(this.LittleEndian);
            var labelOffset = input.ReadValueU32(this.LittleEndian);
            var labelCount = input.ReadValueU32(this.LittleEndian);
            var fieldDataOffset = input.ReadValueU32(this.LittleEndian);
            var fieldDataSize = input.ReadValueU32(this.LittleEndian);
            var fieldIndicesOffset = input.ReadValueU32(this.LittleEndian);
            var fieldIndicesSize = input.ReadValueU32(this.LittleEndian);
            var listIndicesOffset = input.ReadValueU32(this.LittleEndian);
            var listIndicesSize = input.ReadValueU32(this.LittleEndian);

            if (structCount < 1)
            {
                throw new FormatException();
            }

            // field data
            input.Seek(fieldDataOffset, SeekOrigin.Begin);
            var data = input.ReadToMemoryStream(fieldDataSize);

            // labels
            var labels = new string[labelCount];
            input.Seek(labelOffset, SeekOrigin.Begin);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i] = input.ReadString(16, true, Encoding.ASCII);
            }

            // field indices
            if ((fieldIndicesSize % 4) != 0)
            {
                throw new FormatException();
            }

            var fieldIndices = new uint[fieldIndicesSize / 4];
            input.Seek(fieldIndicesOffset, SeekOrigin.Begin);
            for (int i = 0; i < fieldIndices.Length; i++)
            {
                fieldIndices[i] = input.ReadValueU32(this.LittleEndian);
            }

            // list indices
            if ((listIndicesSize % 4) != 0)
            {
                throw new FormatException();
            }

            var listIndices = new uint[listIndicesSize / 4];
            input.Seek(listIndicesOffset, SeekOrigin.Begin);
            for (int i = 0; i < listIndices.Length; i++)
            {
                listIndices[i] = input.ReadValueU32(this.LittleEndian);
            }

            // fields
            var fields = new FieldFormat[fieldCount];
            input.Seek(fieldOffset, SeekOrigin.Begin);
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i].Type = input.ReadValueEnum<GFF.FieldType>(this.LittleEndian);
                fields[i].LabelIndex = input.ReadValueU32(this.LittleEndian);
                fields[i].DataOrDataOffset = input.ReadValueU32(this.LittleEndian);
            }

            // structures
            var structs = new GFF.StructureData[structCount];
            for (int i = 0; i < structs.Length; i++)
            {
                structs[i] = new GFF.StructureData();
            }

            input.Seek(structOffset, SeekOrigin.Begin);
            for (int i = 0; i < structs.Length; i++)
            {
                var structData = structs[i];
                var structFormat = new StructFormat();
                structData.Type = input.ReadValueU32(this.LittleEndian);
                if (i == 0 && structData.Type != 0xFFFFFFFF)
                {
                    throw new FormatException();
                }
                structFormat.DataOrDataOffset = input.ReadValueU32(this.LittleEndian);
                structFormat.FieldCount = input.ReadValueU32(this.LittleEndian);

                var indices = new uint[structFormat.FieldCount];
                
                if (structFormat.FieldCount == 1)
                {
                    indices[0] = structFormat.DataOrDataOffset;
                }
                else
                {
                    if ((structFormat.DataOrDataOffset % 4) != 0)
                    {
                        throw new FormatException();
                    }

                    uint baseIndex = structFormat.DataOrDataOffset / 4;
                    for (uint j = 0; j < structFormat.FieldCount; j++)
                    {
                        indices[j] = fieldIndices[baseIndex + j];
                    }
                }

                var _input = input;
                input = null;

                foreach (var index in indices)
                {
                    var fieldData = new GFF.FieldData();
                    var fieldFormat = fields[index];
                    var label = labels[fieldFormat.LabelIndex];

                    fieldData.Type = fieldFormat.Type;
                    switch (fieldFormat.Type)
                    {
                        case GFF.FieldType.UInt8:
                        {
                            fieldData.Value = (byte)fieldFormat.DataOrDataOffset;
                            break;
                        }

                        case GFF.FieldType.Int8:
                        {
                            fieldData.Value = (sbyte)((byte)fieldFormat.DataOrDataOffset);
                            break;
                        }

                        case GFF.FieldType.UInt16:
                        {
                            fieldData.Value = (ushort)fieldFormat.DataOrDataOffset;
                            break;
                        }

                        case GFF.FieldType.Int16:
                        {
                            fieldData.Value = (short)((ushort)fieldFormat.DataOrDataOffset);
                            break;
                        }

                        case GFF.FieldType.UInt32:
                        {
                            fieldData.Value = (uint)fieldFormat.DataOrDataOffset;
                            break;
                        }

                        case GFF.FieldType.Int32:
                        {
                            fieldData.Value = (int)fieldFormat.DataOrDataOffset;
                            break;
                        }

                        case GFF.FieldType.UInt64:
                        {
                            if (fieldFormat.DataOrDataOffset + 8 > data.Length)
                            {
                                throw new FormatException();
                            }

                            data.Seek(fieldFormat.DataOrDataOffset, SeekOrigin.Begin);
                            fieldData.Value = data.ReadValueU64(this.LittleEndian);
                            break;
                        }

                        case GFF.FieldType.Int64:
                        {
                            if (fieldFormat.DataOrDataOffset + 8 > data.Length)
                            {
                                throw new FormatException();
                            }

                            data.Seek(fieldFormat.DataOrDataOffset, SeekOrigin.Begin);
                            fieldData.Value = data.ReadValueS64(this.LittleEndian);
                            break;
                        }

                        case GFF.FieldType.Single:
                        {
                            fieldData.Value = BitConverter.ToSingle(BitConverter.GetBytes(fieldFormat.DataOrDataOffset), 0);
                            break;
                        }

                        case GFF.FieldType.Double:
                        {
                            if (fieldFormat.DataOrDataOffset + 8 > data.Length)
                            {
                                throw new FormatException();
                            }

                            data.Seek(fieldFormat.DataOrDataOffset, SeekOrigin.Begin);
                            fieldData.Value = data.ReadValueF64(this.LittleEndian);
                            break;
                        }

                        case GFF.FieldType.String:
                        {
                            if (fieldFormat.DataOrDataOffset + 4 > data.Length)
                            {
                                throw new FormatException();
                            }

                            data.Seek(fieldFormat.DataOrDataOffset, SeekOrigin.Begin);
                            var length = data.ReadValueU32();
                            if (fieldFormat.DataOrDataOffset + 4 + length > fieldDataOffset)
                            {
                                throw new FormatException();
                            }

                            fieldData.Value = data.ReadString(length, true, Encoding.ASCII);
                            break;
                        }

                        case GFF.FieldType.Resource:
                        {
                            if (fieldFormat.DataOrDataOffset + 1 > data.Length)
                            {
                                throw new FormatException();
                            }

                            data.Seek(fieldFormat.DataOrDataOffset, SeekOrigin.Begin);
                            var length = data.ReadValueU8();

                            if (fieldFormat.DataOrDataOffset + 1 + length > data.Length)
                            {
                                throw new FormatException();
                            }

                            fieldData.Value = data.ReadString(length, true, Encoding.ASCII);
                            break;
                        }

                        case GFF.FieldType.LocalizedString:
                        {
                            if (fieldFormat.DataOrDataOffset + 4 + 8 > data.Length)
                            {
                                throw new FormatException();
                            }

                            data.Seek(fieldFormat.DataOrDataOffset, SeekOrigin.Begin);
                            var size = data.ReadValueU32();

                            if (fieldFormat.DataOrDataOffset + 4 + size > data.Length)
                            {
                                throw new FormatException();
                            }

                            var loc = new GFF.LocalizedString();
                            loc.Id = data.ReadValueU32(this.LittleEndian);

                            var count = data.ReadValueU32(this.LittleEndian);
                            for (uint j = 0; j < count; j++)
                            {
                                var id = data.ReadValueS32(this.LittleEndian);
                                var length = data.ReadValueU32();
                                loc.Strings.Add(id, data.ReadString(length, true, Encoding.ASCII));
                            }

                            fieldData.Value = loc;
                            break;
                        }

                        case GFF.FieldType.Void:
                        {
                            throw new NotSupportedException();
                        }

                        case GFF.FieldType.Structure:
                        {
                            throw new NotSupportedException();
                        }

                        case GFF.FieldType.List:
                        {
                            if ((fieldFormat.DataOrDataOffset % 4) != 0)
                            {
                                throw new FormatException();
                            }

                            var baseIndex = fieldFormat.DataOrDataOffset / 4;
                            var list = new List<GFF.StructureData>();
                            var count = listIndices[baseIndex];
                            for (uint j = 0; j < count; j++)
                            {
                                list.Add(structs[listIndices[baseIndex + 1 + j]]);
                            }
                            fieldData.Value = list;
                            break;
                        }
                    }

                    structData.Fields.Add(label, fieldData);
                }

                input = _input;
            }

            this.Root = structs[0];
        }

        private struct StructFormat
        {
            public uint DataOrDataOffset;
            public uint FieldCount;
        }

        public struct FieldFormat
        {
            public GFF.FieldType Type;
            public uint LabelIndex;
            public uint DataOrDataOffset;
        }
    }
}
