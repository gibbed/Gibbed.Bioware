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

using System.Collections.Generic;
using GFF = Gibbed.Bioware.FileFormats.GenericFileFormat;

namespace Gibbed.Bioware.FileFormats
{
    [GFF.StructureDefinition(0x544C4B20)]
    public class TLK
    {
        [GFF.FieldDefinition(19001)]
        public List<STRN> Strings = new List<STRN>();

        [GFF.StructureDefinition(0x5354524E)]
        public class STRN
        {
            [GFF.FieldDefinition(19002)]
            public uint Id;

            [GFF.FieldDefinition(19003)]
            public string String;
        }
    }
}
