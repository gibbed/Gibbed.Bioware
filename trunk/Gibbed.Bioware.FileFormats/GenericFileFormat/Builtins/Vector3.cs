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

using System.IO;
using Gibbed.Helpers;

namespace Gibbed.Bioware.FileFormats.GenericFileFormat.Builtins
{
    public class Vector3 : IFieldBuiltinType
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3()
            : this(0, 0, 0)
        {
        }

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public void Serialize(Stream output, bool littleEndian)
        {
            output.WriteValueF32(this.X, littleEndian);
            output.WriteValueF32(this.Y, littleEndian);
            output.WriteValueF32(this.Z, littleEndian);
        }

        public void Deserialize(Stream input, bool littleEndian)
        {
            this.X = input.ReadValueF32(littleEndian);
            this.Y = input.ReadValueF32(littleEndian);
            this.Z = input.ReadValueF32(littleEndian);
        }
    }
}
