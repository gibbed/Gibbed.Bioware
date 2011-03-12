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

namespace Gibbed.Bioware.FileFormats.GenericFileFormat
{
    public class FieldDefinition
    {
        public int Id;
        public FieldType Type;
        public ushort StructureId;
        public FieldFlags Flags;
        public uint Offset;

        internal Type NativeType;
        internal StructureDefinition StructureReference;

        public bool IsList
        {
            get { return this.Flags.HasFlag(FieldFlags.IsList); }
            set
            {
                if (value == true)
                {
                    this.Flags |= FieldFlags.IsList;
                }
                else
                {
                    this.Flags &= ~FieldFlags.IsList;
                }
            }
        }

        public bool IsReference
        {
            get { return this.Flags.HasFlag(FieldFlags.IsReference); }
            set
            {
                if (value == true)
                {
                    this.Flags |= FieldFlags.IsReference;
                }
                else
                {
                    this.Flags &= FieldFlags.IsReference;
                }
            }
        }

        public override string ToString()
        {
            var name = this.Id.ToString();
            var type = this.Type.ToString();
            
            if (this.IsList == true)
            {
                type = "[" + type + "]";
            }

            if (this.IsReference == true)
            {
                type = "*" + type;
            }

            return name + " : " + type;
        }
    }
}
