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

using System.IO;
using Gibbed.IO;

namespace Gibbed.Bioware.FileFormats.GenericFileFormat.Builtins
{
    public class Color : IFieldBuiltinType
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public void Serialize(Stream output, bool littleEndian)
        {
            output.WriteValueF32(this.R, littleEndian);
            output.WriteValueF32(this.G, littleEndian);
            output.WriteValueF32(this.B, littleEndian);
            output.WriteValueF32(this.A, littleEndian);
        }

        public void Deserialize(Stream input, bool littleEndian)
        {
            this.R = input.ReadValueF32(littleEndian);
            this.G = input.ReadValueF32(littleEndian);
            this.B = input.ReadValueF32(littleEndian);
            this.A = input.ReadValueF32(littleEndian);
        }
    }
}
