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
    public class Matrix4x4 : IFieldBuiltinType
    {
        public float AA;
        public float AB;
        public float AC;
        public float AD;

        public float BA;
        public float BB;
        public float BC;
        public float BD;

        public float CA;
        public float CB;
        public float CC;
        public float CD;

        public float DA;
        public float DB;
        public float DC;
        public float DD;

        public void Serialize(Stream output, Endian endian)
        {
            output.WriteValueF32(this.AA, endian);
            output.WriteValueF32(this.AB, endian);
            output.WriteValueF32(this.AC, endian);
            output.WriteValueF32(this.AD, endian);

            output.WriteValueF32(this.BA, endian);
            output.WriteValueF32(this.BB, endian);
            output.WriteValueF32(this.BC, endian);
            output.WriteValueF32(this.BD, endian);

            output.WriteValueF32(this.CA, endian);
            output.WriteValueF32(this.CB, endian);
            output.WriteValueF32(this.CC, endian);
            output.WriteValueF32(this.CD, endian);

            output.WriteValueF32(this.DA, endian);
            output.WriteValueF32(this.DB, endian);
            output.WriteValueF32(this.DC, endian);
            output.WriteValueF32(this.DD, endian);
        }

        public void Deserialize(Stream input, Endian endian)
        {
            this.AA = input.ReadValueF32(endian);
            this.AB = input.ReadValueF32(endian);
            this.AC = input.ReadValueF32(endian);
            this.AD = input.ReadValueF32(endian);

            this.BA = input.ReadValueF32(endian);
            this.BB = input.ReadValueF32(endian);
            this.BC = input.ReadValueF32(endian);
            this.BD = input.ReadValueF32(endian);

            this.CA = input.ReadValueF32(endian);
            this.CB = input.ReadValueF32(endian);
            this.CC = input.ReadValueF32(endian);
            this.CD = input.ReadValueF32(endian);

            this.DA = input.ReadValueF32(endian);
            this.DB = input.ReadValueF32(endian);
            this.DC = input.ReadValueF32(endian);
            this.DD = input.ReadValueF32(endian);
        }
    }
}
