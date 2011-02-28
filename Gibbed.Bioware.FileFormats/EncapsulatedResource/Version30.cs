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
using Gibbed.Helpers;

namespace Gibbed.Bioware.FileFormats.EncapsulatedResource
{
    internal class Version30 : IVersion
    {
        public void Serialize(Stream output, IEnumerable<Entry> entries)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Entry> Deserialize(Stream input)
        {
            var entries = new List<Entry>();

            var stringTableSize = input.ReadValueU32();
            var fileCount = input.ReadValueU32();
            var flags = input.ReadValueU32();
            var unknown1C = input.ReadValueU32();
            var unknown20 = input.ReadValueU32();
            var unknown24 = input.ReadValueU32();
            var unknown28 = input.ReadValueU32();
            var unknown2C = input.ReadValueU32();

            if ((flags != 0x20000000 && flags != 0) ||
                unknown1C != 0 ||
                unknown20 != 0 ||
                unknown24 != 0 ||
                unknown28 != 0 ||
                unknown2C != 0)
            {
                throw new FormatException();
            }

            var stringTable = input.ReadToMemoryStream(stringTableSize);

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

            return entries;
        }
    }
}
