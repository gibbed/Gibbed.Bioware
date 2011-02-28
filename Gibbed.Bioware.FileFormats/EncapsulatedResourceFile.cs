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
using Gibbed.Bioware.FileFormats.EncapsulatedResource;
using Gibbed.Helpers;

namespace Gibbed.Bioware.FileFormats
{
    public class EncapsulatedResourceFile
    {
        public List<EncapsulatedResource.Entry> Entries
            = new List<Entry>();

        public void Deserialize(Stream input)
        {
            var basePosition = input.Position;

            // read as two unsigned longs so we don't have to actually
            // decode the strings
            var version1 = input.ReadValueU64(false);
            var version2 = input.ReadValueU64(false);

            IVersion reader = null;

            if (version1 == 0x4552462056322E31) // ERF V2.1
            {
                input.Seek(basePosition + 8, SeekOrigin.Begin);
                reader = new EncapsulatedResource.Version21();
            }
            else if (version1 == 0x4500520046002000 &&
                version2 == 0x560032002E003000) // ERF V2.0
            {
                input.Seek(basePosition + 16, SeekOrigin.Begin);
                reader = new EncapsulatedResource.Version20();
            }
            else if (version1 == 0x4500520046002000 &&
                version2 == 0x560032002E003200) // ERF V2.2
            {
                input.Seek(basePosition + 16, SeekOrigin.Begin);
                reader = new EncapsulatedResource.Version22();
            }
            else if (version1 == 0x4500520046002000 &&
                version2 == 0x560033002E003000) // ERF V3.0
            {
                input.Seek(basePosition + 16, SeekOrigin.Begin);
                reader = new EncapsulatedResource.Version30();
            }
            else
            {
                throw new FormatException("unsupported / unknown ERF format");
            }

            this.Entries.Clear();
            this.Entries.AddRange(reader.Deserialize(input));
        }
    }
}
