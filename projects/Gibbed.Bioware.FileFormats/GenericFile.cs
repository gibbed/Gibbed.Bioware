/* Copyright (c) 2017 Rick (rick 'at' gibbed 'dot' us)
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
using System.Linq;
using System.Text;
using Gibbed.IO;
using GFF = Gibbed.Bioware.FileFormats.GenericFileFormat;

namespace Gibbed.Bioware.FileFormats
{
    // GenericFileFormat (GFF)
    public class GenericFile : IDisposable
    {
        ~GenericFile()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private bool _Disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (this._Disposed == false)
            {
                if (disposing == true)
                {
                    if (this.Data != null)
                    {
                        this.Data.Dispose();
                    }
                }

                this._Disposed = true;
            }
        }

        public byte FileVersion;
        public GFF.FilePlatform FilePlatform;
        public GFF.FormatType FormatType;
        public uint FormatVersion;

        protected Endian Endian
        {
            get { return this.FilePlatform == GFF.FilePlatform.PC ? Endian.Little : Endian.Big; }
        }

        protected MemoryStream Data = null;
        protected List<string> StringTable = null;
        protected List<GFF.StructureDefinition> Structures
            = new List<GFF.StructureDefinition>();

        protected List<GFF.StructureDefinition> Printed
            = new List<GFF.StructureDefinition>();

        public void PrintStructure()
        {
            this.PrintStructure(this.Structures[0], 0);

            Console.WriteLine("///--");

            foreach (var def in this.Structures)
            {
                if (Printed.Contains(def) == false)
                {
                    PrintStructure(def, 1);
                }
            }
        }

        private static string ParseId(uint id)
        {
            if (id == 0)
            {
                return "__ROOT__";
            }

            return Encoding.ASCII.GetString(BitConverter.GetBytes(id.Swap())).Trim();
        }

        private static void PrintLine(int level, string format, params object[] arg)
        {
            level++;
            for (int i = 0; i < level; i++)
            {
                Console.Write("    ");
            }
            Console.WriteLine(format, arg);
        }

        private void PrintStructure(GFF.StructureDefinition def, int level)
        {
            var queue = new Queue<GFF.StructureDefinition>();
            if (Printed.Contains(def) == false)
            {
                Printed.Add(def);
            }

            PrintLine(level, "[GFF.StructureDefinition(0x{0:X8})] // {1}", def.Id, ParseId(def.Id));
            PrintLine(level, "public class {0}", ParseId(def.Id));
            PrintLine(level, "{{");

            foreach (var fieldDef in def.Fields)
            {
                string type;

                if (fieldDef.Type == GFF.FieldType.Structure)
                {
                    var subdef = this.Structures[fieldDef.StructureId];
                    type = ParseId(subdef.Id);

                    if (queue.Contains(subdef) == false)
                    {
                        queue.Enqueue(subdef);
                    }
                }
                else
                {
                    type = fieldDef.Type.ToString();
                }

                if (fieldDef.Type == GFF.FieldType.Color ||
                    fieldDef.Type == GFF.FieldType.Matrix4x4 ||
                    fieldDef.Type == GFF.FieldType.Quaternion ||
                    fieldDef.Type == GFF.FieldType.TalkString ||
                    fieldDef.Type == GFF.FieldType.Vector3 ||
                    fieldDef.Type == GFF.FieldType.Vector4)
                {
                    type = "GFF.Builtins." + type;
                }

                if (fieldDef.Type == GFF.FieldType.UInt8 &&
                    fieldDef.IsList == true)
                {
                    type = "byte[]";
                }
                else if (fieldDef.IsList == true)
                {
                    type = "List<" + type + ">";
                }

                if (fieldDef.IsReference == true)
                {
                    type = "*" + type;
                }

                PrintLine(level + 1, "");
                PrintLine(level + 1, "[GFF.FieldDefinition({0})]", fieldDef.Id);
                PrintLine(level + 1, "public {0} Unknown{1};", type, fieldDef.Id);
            }

            if (queue.Count > 0)
            {
                while (queue.Count > 0)
                {
                    PrintLine(level + 1, "");
                    PrintStructure(queue.Dequeue(), level + 1);
                }
            }

            PrintLine(level, "}}");
        }

        public void Serialize(Stream output)
        {
            output.WriteValueU32(0x47464620u, Endian.Big);
            output.WriteValueU32(0x56342E30u + this.FileVersion, Endian.Big);
            output.WriteValueEnum<GFF.FilePlatform>(this.FilePlatform, Endian.Big);
            output.WriteValueEnum<GFF.FormatType>(this.FormatType, Endian.Big);
            output.WriteValueU32(this.FormatVersion, Endian.Big);

            var endian = this.FilePlatform == GFF.FilePlatform.PC ? Endian.Little : Endian.Big;

            output.WriteValueS32(this.Structures.Count, endian);

            int structuresOffset = (int)output.Position + 4;
            int fieldsOffset = structuresOffset + (this.Structures.Count * 16);
            int dataOffset = fieldsOffset + (this.Structures.Sum(s => s.Fields.Count) * 12);
            dataOffset = dataOffset.Align(16);
            output.WriteValueS32(dataOffset, endian);

            int runningFieldCount = 0;
            foreach (var structDef in this.Structures)
            {
                output.WriteValueU32(structDef.Id, Endian.Big);
                output.WriteValueS32(structDef.Fields.Count, endian);
                output.WriteValueS32(fieldsOffset + 
                    (runningFieldCount * 12), endian);
                output.WriteValueU32(structDef.DataSize, endian);
                runningFieldCount += structDef.Fields.Count;
            }

            foreach (var structDef in this.Structures)
            {
                foreach (var fieldDef in structDef.Fields)
                {
                    output.WriteValueS32(fieldDef.Id, endian);
                    
                    var flags = fieldDef.Flags;
                    ushort type;

                    if (fieldDef.Type == GFF.FieldType.Structure)
                    {
                        flags |= GFF.FieldFlags.IsStructure;
                        type = fieldDef.StructureId;
                    }
                    else
                    {
                        type = (ushort)fieldDef.Type;
                    }

                    uint rawFlags = 0;
                    rawFlags |= type;
                    rawFlags |= (uint)flags << 16;

                    output.WriteValueU32(rawFlags, endian);
                    output.WriteValueU32(fieldDef.Offset, endian);
                }
            }

            this.Data.Position = 0;
            output.Seek(dataOffset, SeekOrigin.Begin);
            output.WriteFromStream(this.Data, this.Data.Length);
        }

        public void Deserialize(Stream input)
        {
            input.Seek(0, SeekOrigin.Begin);

            var magic = input.ReadValueU32(Endian.Big);
            if (magic != 0x47464620)
            {
                throw new FormatException();
            }

            var version = input.ReadValueU32(Endian.Big);
            if (version != 0x56342E30 && // 4.0
                version != 0x56342E31) // 4.1
            {
                throw new FormatException("unsupported version");
            }

            this.FileVersion = (byte)(version - 0x56342E30);
            this.FilePlatform = input.ReadValueEnum<GFF.FilePlatform>(Endian.Big);
            this.FormatType = input.ReadValueEnum<GFF.FormatType>(Endian.Big);
            this.FormatVersion = input.ReadValueU32(Endian.Big);

            var endian = this.FilePlatform == GFF.FilePlatform.PC ? Endian.Little : Endian.Big;

            var structCount = input.ReadValueU32(endian);
            var stringCount = this.FileVersion < 1 ? 0 : input.ReadValueU32(endian);
            var stringOffset = this.FileVersion < 1 ? 0 : input.ReadValueU32(endian);
            var dataOffset = input.ReadValueU32(endian);

            if (this.FileVersion < 1)
            {
                stringOffset = dataOffset;
            }
            else
            {
                if (dataOffset < stringOffset)
                {
                    throw new FormatException();
                }
            }

            this.Structures.Clear();
            for (uint i = 0; i < structCount; i++)
            {
                var structDef = new GFF.StructureDefinition();
                //structDef.Id = input.ReadValueU32(endian);
                structDef.Id = input.ReadValueU32(Endian.Big);
                var fieldCount = input.ReadValueU32(endian);
                var fieldOffset = input.ReadValueU32(endian);
                structDef.DataSize = input.ReadValueU32(endian);

                long nextOffset = input.Position;

                structDef.Fields.Clear();
                input.Seek(fieldOffset, SeekOrigin.Begin);
                for (uint j = 0; j < fieldCount; j++)
                {
                    var fieldDef = new GFF.FieldDefinition();
                    fieldDef.Id = input.ReadValueS32(endian);
                    var rawFlags = input.ReadValueU32(endian);
                    fieldDef.Offset = input.ReadValueU32(endian);

                    var type = (ushort)(rawFlags & 0xFFFF);
                    var flags = (GFF.FieldFlags)((rawFlags >> 16) & 0xFFFF);

                    if ((flags & GFF.FieldFlags.IsStructure) != 0)
                    {
                        flags &= ~GFF.FieldFlags.IsStructure;
                        fieldDef.Type = GFF.FieldType.Structure;
                        fieldDef.StructureId = type;
                    }
                    else
                    {
                        fieldDef.Type = (GFF.FieldType)type;
                    }

                    fieldDef.Flags = flags;
                    structDef.Fields.Add(fieldDef);
                }

                this.Structures.Add(structDef);
                input.Seek(nextOffset, SeekOrigin.Begin);
            }

            if (this.FileVersion >= 1)
            {
                input.Seek(stringOffset, SeekOrigin.Begin);
                this.StringTable = new List<string>();
                for (uint i = 0; i < stringCount; i++)
                {
                    this.StringTable.Add(input.ReadStringZ(Encoding.UTF8));
                }
            }

            input.Seek(dataOffset, SeekOrigin.Begin);
            this.Data = input.ReadToMemoryStream(input.Length - dataOffset);
        }
    }
}
