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

        public void Serialize(Stream output, bool littleEndian)
        {
            output.WriteValueF32(this.AA, littleEndian);
            output.WriteValueF32(this.AB, littleEndian);
            output.WriteValueF32(this.AC, littleEndian);
            output.WriteValueF32(this.AD, littleEndian);

            output.WriteValueF32(this.BA, littleEndian);
            output.WriteValueF32(this.BB, littleEndian);
            output.WriteValueF32(this.BC, littleEndian);
            output.WriteValueF32(this.BD, littleEndian);

            output.WriteValueF32(this.CA, littleEndian);
            output.WriteValueF32(this.CB, littleEndian);
            output.WriteValueF32(this.CC, littleEndian);
            output.WriteValueF32(this.CD, littleEndian);

            output.WriteValueF32(this.DA, littleEndian);
            output.WriteValueF32(this.DB, littleEndian);
            output.WriteValueF32(this.DC, littleEndian);
            output.WriteValueF32(this.DD, littleEndian);
        }

        public void Deserialize(Stream input, bool littleEndian)
        {
            this.AA = input.ReadValueF32(littleEndian);
            this.AB = input.ReadValueF32(littleEndian);
            this.AC = input.ReadValueF32(littleEndian);
            this.AD = input.ReadValueF32(littleEndian);

            this.BA = input.ReadValueF32(littleEndian);
            this.BB = input.ReadValueF32(littleEndian);
            this.BC = input.ReadValueF32(littleEndian);
            this.BD = input.ReadValueF32(littleEndian);

            this.CA = input.ReadValueF32(littleEndian);
            this.CB = input.ReadValueF32(littleEndian);
            this.CC = input.ReadValueF32(littleEndian);
            this.CD = input.ReadValueF32(littleEndian);

            this.DA = input.ReadValueF32(littleEndian);
            this.DB = input.ReadValueF32(littleEndian);
            this.DC = input.ReadValueF32(littleEndian);
            this.DD = input.ReadValueF32(littleEndian);
        }
    }
}
