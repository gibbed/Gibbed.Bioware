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
using System.Text;
using Gibbed.IO;

namespace Gibbed.Bioware.FileFormats
{
    public class EncapsulatedResourceFile
    {
        #region Fields
        private FileVersion _Version;
        private uint _Flags;
        private EncryptionScheme _Encryption;
        private CompressionScheme _Compression;
        private uint _ContentId;
        private byte[] _PasswordDigest;
        private readonly List<Entry> _Entries;
        #endregion

        public EncapsulatedResourceFile()
        {
            this._Entries = new List<Entry>();
        }

        #region Properties
        public FileVersion Version
        {
            get { return this._Version; }
            set { this._Version = value; }
        }

        public uint Flags
        {
            get { return this._Flags; }
            set { this._Flags = value; }
        }

        public EncryptionScheme Encryption
        {
            get { return this._Encryption; }
            set { this._Encryption = value; }
        }

        public CompressionScheme Compression
        {
            get { return this._Compression; }
            set { this._Compression = value; }
        }

        public uint ContentId
        {
            get { return this._ContentId; }
            set { this._ContentId = value; }
        }

        public byte[] PasswordDigest
        {
            get { return this._PasswordDigest; }
            set { this._PasswordDigest = value; }
        }

        public List<Entry> Entries
        {
            get { return this._Entries; }
        }
        #endregion

        public static long CalculateHeaderSize(FileVersion version, IEnumerable<string> strings, int entries)
        {
            if (version == FileVersion.V3_0)
            {
                long size = 0;
                size += 16;
                size += 4 + 4 + 4;
                size += 4 + 16;

                int stringTableSize = 0;
                foreach (var str in strings)
                {
                    if (str == null)
                    {
                        continue;
                    }

                    stringTableSize += Encoding.UTF8.GetBytes(str).Length + 1;
                }
                size += stringTableSize.Align(16);
                size += entries * 28;
                return size;
            }

            throw new NotSupportedException();
        }

        public void Serialize(Stream output)
        {
            var version = this._Version;

            if (version == FileVersion.V3_0)
            {
                output.WriteString("ERF V3.0", Encoding.Unicode);

                var strings = new List<string>();
                foreach (var entry in this._Entries)
                {
                    if (entry.Name != null && strings.Contains(entry.Name) == false)
                    {
                        strings.Add(entry.Name);
                    }
                }
                strings.Sort();

                var stringOffsets = new Dictionary<string, uint>();
                var stringTable = new MemoryStream();
                foreach (var value in strings)
                {
                    stringOffsets[value] = (uint)stringTable.Length;
                    stringTable.WriteStringZ(value, Encoding.UTF8);
                }
                stringTable.SetLength(stringTable.Length.Align(16));
                stringTable.Position = 0;

                output.WriteValueU32((uint)stringTable.Length);
                output.WriteValueS32(this._Entries.Count);

                if (this._Encryption != EncryptionScheme.None || this._Compression != CompressionScheme.None)
                {
                    throw new NotSupportedException();
                }

                uint flags = 0;
                flags |= ((uint)this._Encryption) << 4;
                flags |= ((uint)this._Compression) << 29;
                output.WriteValueU32(flags);

                output.WriteValueU32(this._ContentId);
                if (this._PasswordDigest == null)
                {
                    output.Write(new byte[16], 0, 16);
                }
                else
                {
                    output.Write(this._PasswordDigest, 0, 16);
                }

                output.WriteFromStream(stringTable, stringTable.Length, 0x00100000);

                foreach (var entry in this._Entries)
                {
                    if (entry.Name != null)
                    {
                        entry.CalculateHashes();
                        output.WriteValueU32(stringOffsets[entry.Name]);
                    }
                    else
                    {
                        output.WriteValueU32(0xFFFFFFFF);
                    }

                    output.WriteValueU64(entry.NameHash);
                    output.WriteValueU32(entry.TypeHash);
                    output.WriteValueU32((uint)entry.Offset);
                    output.WriteValueU32(entry.CompressedSize);
                    output.WriteValueU32(entry.UncompressedSize);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Deserialize(Stream input)
        {
            var basePosition = input.Position;

            // read as two unsigned longs so we don't have to actually
            // decode the strings
            var version1 = input.ReadValueU64(Endian.Big);
            var version2 = input.ReadValueU64(Endian.Big);

            if (version1 == 0x4552462056322E31) // ERF V2.1
            {
                input.Seek(basePosition + 8, SeekOrigin.Begin);
                throw new NotSupportedException();
            }
            else if (version1 == 0x4500520046002000 &&
                     version2 == 0x560032002E003000) // ERF V2.0
            {
                input.Seek(basePosition + 16, SeekOrigin.Begin);

                var fileCount = input.ReadValueU32();
                var unknown14 = input.ReadValueU32();
                var unknown18 = input.ReadValueU32();
                var unknown1C = input.ReadValueU32();

                var entries = new List<Entry>();
                for (uint i = 0; i < fileCount; i++)
                {
                    var entry = new Entry();
                    entry.Name = input.ReadString(64, true, Encoding.Unicode);
                    entry.CalculateHashes();
                    entry.Offset = input.ReadValueU32();
                    entry.UncompressedSize = input.ReadValueU32();
                    entry.CompressedSize = entry.UncompressedSize;
                    entries.Add(entry);
                }

                this._Version = FileVersion.V2_0;
                this._Flags = 0;
                this._Encryption = EncryptionScheme.None;
                this._Compression = CompressionScheme.None;
                this._ContentId = 0;
                this._PasswordDigest = null;
                this._Entries.Clear();
                this._Entries.AddRange(entries);
            }
            else if (version1 == 0x4500520046002000 &&
                     version2 == 0x560032002E003200) // ERF V2.2
            {
                input.Seek(basePosition + 16, SeekOrigin.Begin);

                var fileCount = input.ReadValueU32();
                var year = input.ReadValueU32();
                var day = input.ReadValueU32();
                var unknown1C = input.ReadValueU32(); // always 0xFFFFFFFF?
                var flags = input.ReadValueU32();
                var contentId = input.ReadValueU32();
                var passwordDigest = new byte[16];
                input.Read(passwordDigest, 0, passwordDigest.Length);

                if (unknown1C != 0xFFFFFFFF)
                {
                    throw new InvalidOperationException();
                }

                var encryption = (EncryptionScheme)((flags & 0x000000F0) >> 4);
                var compression = (CompressionScheme)((flags & 0xE0000000) >> 29);
                flags &= 0x1FFFFF0F;

                if (flags != 0 && flags != 1)
                {
                    throw new FormatException("unknown flags value");
                }

                var entries = new List<Entry>();
                for (uint i = 0; i < fileCount; i++)
                {
                    var entry = new Entry();
                    entry.Name = input.ReadString(64, true, Encoding.Unicode);
                    entry.CalculateHashes();
                    entry.Offset = input.ReadValueU32();
                    entry.CompressedSize = input.ReadValueU32();
                    entry.UncompressedSize = input.ReadValueU32();
                    entries.Add(entry);
                }

                this._Version = FileVersion.V2_2;
                this._Flags = flags;
                this._Encryption = encryption;
                this._Compression = compression;
                this._ContentId = contentId;
                this._PasswordDigest = passwordDigest;
                this._Entries.Clear();
                this._Entries.AddRange(entries);
            }
            else if (version1 == 0x4500520046002000 &&
                     version2 == 0x560033002E003000) // ERF V3.0
            {
                input.Seek(basePosition + 16, SeekOrigin.Begin);

                var stringTableSize = input.ReadValueU32();
                var fileCount = input.ReadValueU32();
                var flags = input.ReadValueU32();
                var contentId = input.ReadValueU32();
                var passwordDigest = new byte[16];
                input.Read(passwordDigest, 0, passwordDigest.Length);

                var encryption = (EncryptionScheme)((flags & 0x000000F0) >> 4);
                var compression = (CompressionScheme)((flags & 0xE0000000) >> 29);
                flags &= 0x1FFFFF0F;

                if (flags != 0 && flags != 1)
                {
                    throw new FormatException("unknown flags value");
                }

                var entries = new List<Entry>();
                using (var stringTable = stringTableSize != 0
                                             ? input.ReadToMemoryStream(stringTableSize)
                                             : null)
                {
                    for (uint i = 0; i < fileCount; i++)
                    {
                        var entry = new Entry();

                        uint nameOffset = input.ReadValueU32();
                        entry.NameHash = input.ReadValueU64();

                        if (nameOffset != 0xFFFFFFFF)
                        {
                            if (nameOffset + 1 > stringTable.Length)
                            {
                                throw new FormatException("file name exceeds string table bounds");
                            }

                            stringTable.Position = nameOffset;
                            entry.Name = stringTable.ReadStringZ(Encoding.ASCII);

                            if (entry.Name.HashFNV64() != entry.NameHash)
                            {
                                throw new InvalidOperationException("hash mismatch");
                            }
                        }
                        else
                        {
                            entry.Name = null;
                        }

                        entry.TypeHash = input.ReadValueU32();
                        entry.Offset = input.ReadValueU32();
                        entry.CompressedSize = input.ReadValueU32();
                        entry.UncompressedSize = input.ReadValueU32();
                        entries.Add(entry);
                    }
                }

                this._Version = FileVersion.V3_0;
                this._Flags = flags;
                this._Encryption = encryption;
                this._Compression = compression;
                this._ContentId = contentId;
                this._PasswordDigest = passwordDigest;
                this._Entries.Clear();
                this._Entries.AddRange(entries);
            }
            else
            {
                throw new FormatException("unsupported / unknown ERF format");
            }
        }

        public enum FileVersion
        {
            // ReSharper disable InconsistentNaming
            Invalid = 0,
            V2_0,
            V2_2,
            V3_0,
            // ReSharper restore InconsistentNaming
        }

        public enum EncryptionScheme : byte
        {
            None = 0,
            XOR = 1,
            Blowfish2 = 2,
            Blowfish3 = 3,
        }

        public enum CompressionScheme : byte
        {
            None = 0, // all platforms
            BiowareZlib = 1, // all platforms
            LZMA = 2, // PS3
            XMemLZX = 3, // XBOX 360
            HeaderlessZlib = 7, // all platforms
        }

        public class Entry
        {
            public string Name;
            public ulong NameHash;
            public uint TypeHash;
            public long Offset;
            public uint UncompressedSize;
            public uint CompressedSize;

            public void CalculateHashes()
            {
                if (this.Name == null)
                {
                    throw new InvalidOperationException();
                }

                this.NameHash = this.Name.HashFNV64();
                var extension = Path.GetExtension(this.Name);
                this.TypeHash = extension.TrimStart('.').HashFNV32();
            }
        }
    }
}
