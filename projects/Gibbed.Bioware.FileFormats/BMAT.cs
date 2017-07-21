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
    public class BMAT
    {
        private static string ReadString(Stream input)
        {
            var length = input.ReadValueU32();
            if (length >= 1024)
            {
                throw new InvalidOperationException();
            }
            return input.ReadString(length, true, Encoding.ASCII);
        }

        public List<Subtype1> Unknown0
            = new List<Subtype1>();

        public void Deserialize(Stream input)
        {
            if (input.ReadValueU32(Endian.Big) != 0x626D6174)
            {
                throw new FormatException();
            }

            uint count = input.ReadValueU32();
            this.Unknown0.Clear();
            for (uint i = 0; i < count; i++)
            {
                var unknown0 = new Subtype1();
                unknown0.Deserialize(input);
                this.Unknown0.Add(unknown0);
            }
        }

        public class Subtype1
        {
            public string Unknown0;
            public string Unknown1;
            public List<Subtype2> Unknown2
                = new List<Subtype2>();

            public void Deserialize(Stream input)
            {
                this.Unknown0 = ReadString(input);
                this.Unknown1 = ReadString(input);

                uint count = input.ReadValueU32();
                this.Unknown2.Clear();
                for (uint i = 0; i < count; i++)
                {
                    var unknown2 = new Subtype2();
                    unknown2.Deserialize(input);
                    this.Unknown2.Add(unknown2);
                }
            }
        }

        public class Subtype2
        {
            public string Unknown0;
            public uint Unknown1;
            public uint Unknown2;
            public uint Unknown3;
            public uint Unknown4;
            public uint Unknown5;
            public uint Unknown6;
            public List<Subtype3> Unknown7
                = new List<Subtype3>();

            public void Deserialize(Stream input)
            {
                this.Unknown0 = ReadString(input);
                this.Unknown1 = input.ReadValueU32();
                this.Unknown2 = input.ReadValueU32();
                this.Unknown3 = input.ReadValueU32();
                this.Unknown4 = input.ReadValueU32();
                this.Unknown5 = input.ReadValueU32();
                this.Unknown6 = input.ReadValueU32();

                var count = input.ReadValueU32();
                this.Unknown7.Clear();
                for (uint i = 0; i < count; i++)
                {
                    var unknown7 = new Subtype3();
                    unknown7.Deserialize(input);
                    this.Unknown7.Add(unknown7);
                }
            }
        }

        public class Subtype3
        {
            public string Unknown0;
            public uint Flags;
            public string DepthShader;
            public string HShader;
            public string Unknown3;
            public string CShader;

            public string VertexShader;
            public string PixelShader;

            public List<Subtype4> Unknown6
                = new List<Subtype4>();
            public List<Subtype5> Unknown7
                = new List<Subtype5>();
            public List<Subtype6> Unknown8
                = new List<Subtype6>();
            public List<Subtype7> Unknown9
                = new List<Subtype7>();

            public void Deserialize(Stream input)
            {
                this.Unknown0 = ReadString(input);

                this.Flags = input.ReadValueU32();
                if ((this.Flags & 8) != 0)
                {
                    this.DepthShader = ReadString(input);
                    this.HShader = ReadString(input);
                }

                if ((this.Flags & 16) != 0)
                {
                    this.Unknown3 = ReadString(input);
                }

                if ((this.Flags & 32) != 0)
                {
                    this.CShader = ReadString(input);
                }

                this.VertexShader = ReadString(input);
                this.PixelShader = ReadString(input);

                this.Unknown6.Clear();
                {
                    var count = input.ReadValueU32();
                    for (uint i = 0; i < count; i++)
                    {
                        var unknown5 = new Subtype4();
                        unknown5.Deserialize(input);
                        this.Unknown6.Add(unknown5);
                    }
                }

                this.Unknown7.Clear();
                {
                    var count = input.ReadValueU32();
                    for (uint i = 0; i < count; i++)
                    {
                        var unknown6 = new Subtype5();
                        unknown6.Deserialize(input);
                        this.Unknown7.Add(unknown6);
                    }
                }

                this.Unknown8.Clear();
                {
                    var count = input.ReadValueU32();
                    for (uint i = 0; i < count; i++)
                    {
                        var unknown7 = new Subtype6();
                        unknown7.Deserialize(input);
                        this.Unknown8.Add(unknown7);
                    }
                }

                this.Unknown9.Clear();
                {
                    var count = input.ReadValueU32();
                    for (uint i = 0; i < count; i++)
                    {
                        var unknown8 = new Subtype7();
                        unknown8.Deserialize(input);
                        this.Unknown9.Add(unknown8);
                    }
                }
            }
        }

        public class Subtype4
        {
            public string Unknown0;
            public string Unknown1;
            public uint Unknown2;
            public uint Unknown3;
            public uint Unknown4;

            public void Deserialize(Stream input)
            {
                this.Unknown0 = ReadString(input);
                this.Unknown1 = ReadString(input);
                this.Unknown2 = input.ReadValueU32();
                this.Unknown3 = input.ReadValueU32();
                this.Unknown4 = input.ReadValueU32();
            }
        }

        public class Subtype5
        {
            public string Unknown0;
            public string Unknown1;
            public string Unknown2;
            public uint Unknown3;
            public uint Unknown4;
            public uint Unknown5;

            public void Deserialize(Stream input)
            {
                this.Unknown0 = ReadString(input);
                this.Unknown1 = ReadString(input);
                this.Unknown2 = ReadString(input);
                this.Unknown3 = input.ReadValueU32();
                this.Unknown4 = input.ReadValueU32();
                this.Unknown5 = input.ReadValueU32();
            }
        }


        public class Subtype6
        {
            public uint Unknown0;
            public uint Unknown1;

            public void Deserialize(Stream input)
            {
                this.Unknown0 = input.ReadValueU32();
                this.Unknown1 = input.ReadValueU32();
            }
        }

        public class Subtype7
        {
            public uint Unknown0;
            public uint Unknown1;
            public uint Unknown2;

            public void Deserialize(Stream input)
            {
                this.Unknown0 = input.ReadValueU32();
                this.Unknown1 = input.ReadValueU32();
                this.Unknown2 = input.ReadValueU32();
            }
        }
    }
}
