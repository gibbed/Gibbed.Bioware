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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Gibbed.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ERF = Gibbed.Bioware.FileFormats.EncapsulatedResourceFile;

namespace RebuildColumnLists
{
    public class Loader : IEnumerable<ERF.Entry>
    {
        private ERF Archive;

        public Loader(ERF erf)
        {
            this.Archive = erf;
        }

        public MemoryStream Load(
            Stream input,
            ERF.Entry entry)
        {
            if (this.Archive.Encryption != ERF.EncryptionScheme.None)
            {
                return null;
            }
            else if (
                this.Archive.Compression != ERF.CompressionScheme.None &&
                this.Archive.Compression != ERF.CompressionScheme.BiowareZlib &&
                this.Archive.Compression != ERF.CompressionScheme.HeaderlessZlib)
            {
                return null;
            }

            input.Seek(entry.Offset, SeekOrigin.Begin);

            Stream data = input;
            if (this.Archive.Compression != ERF.CompressionScheme.None)
            {
                if (this.Archive.Compression == ERF.CompressionScheme.BiowareZlib)
                {
                    input.Seek(1, SeekOrigin.Current);
                }

                data = new InflaterInputStream(
                    data, new Inflater(true));
            }

            return data.ReadToMemoryStream(entry.UncompressedSize);
        }

        public IEnumerator<ERF.Entry> GetEnumerator()
        {
            return this.Archive.Entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Archive.Entries.GetEnumerator();
        }
    }
}
