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

namespace Gibbed.Bioware.FileFormats
{
    public class KeyValue
    {
        private static KeyValue Invalid = new KeyValue();

        public GenericFileFormat.FieldType Type;
        public uint StructureId;
        public bool Valid;
        public object Value;
        public Dictionary<int, KeyValue> Values = null;

        public KeyValue()
            : this(GenericFileFormat.FieldType.Unknown, null)
        {
            this.Valid = false;
        }

        public KeyValue(GenericFileFormat.FieldType type, object value)
        {
            this.Type = type;
            this.Value = value;
            this.Valid = true;
        }

        public KeyValue this[int id]
        {
            get
            {
                if (this.Values == null)
                {
                    return Invalid;
                }
                else if (this.Values.ContainsKey(id) == false)
                {
                    return Invalid;
                }

                return this.Values[id];
            }

            set
            {
                if (this.Values == null)
                {
                    if (value != null)
                    {
                        this.Values = new Dictionary<int, KeyValue>();
                        this.Values[id] = value;
                    }
                }
                else
                {
                    if (value == null)
                    {
                        this.Values.Remove(id);
                    }
                    else
                    {
                        this.Values[id] = value;
                    }
                }
            }
        }

        public TType As<TType>()
        {
            return this.As<TType>(default(TType));
        }

        public TType As<TType>(TType defaultValue)
        {
            if (this.Valid == false)
            {
                return defaultValue;
            }
            else if (this.Value == null)
            {
                return defaultValue;
            }

            return (TType)this.Value;
        }
    }
}
