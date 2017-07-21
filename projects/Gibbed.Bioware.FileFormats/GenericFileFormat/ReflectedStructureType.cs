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
using System.Reflection;

namespace Gibbed.Bioware.FileFormats.GenericFileFormat
{
    internal class ReflectedStructureType
    {
        #region ReflectionCache
        // reflection: it's fucking slow :)
        private static Dictionary<Type, ReflectedStructureType> Cache = null;

        public static ReflectedStructureType For(Type type)
        {
            if (Cache == null)
            {
                Cache = new Dictionary<Type, ReflectedStructureType>();
            }

            if (Cache.ContainsKey(type) == true)
            {
                return Cache[type];
            }

            return Cache[type] = new ReflectedStructureType(type);
        }
        #endregion

        public uint Id { get; private set; }
        public Dictionary<int, ReflectedFieldType> Fields { get; private set; }

        private ReflectedStructureType(Type type)
        {
            foreach (StructureDefinitionAttribute attribute in
                type.GetCustomAttributes(typeof(StructureDefinitionAttribute), true))
            {
                this.Id = attribute.Id;
                break;
            }

            this.Fields = new Dictionary<int, ReflectedFieldType>();
            foreach (var field in type.GetFields())
            {
                foreach (FieldDefinitionAttribute attribute in
                    field.GetCustomAttributes(typeof(FieldDefinitionAttribute), true))
                {
                    this.Fields.Add(attribute.Id,
                        new ReflectedFieldType()
                        {
                            Type = attribute.Type,
                            Field = field,
                        });
                    break;
                }
            }
        }

        public Type GetFieldType(int id)
        {
            if (this.Fields.ContainsKey(id) == false)
            {
                throw new KeyNotFoundException();
            }

            return this.Fields[id].Field.FieldType;
        }

        public object GetField(object instance, int id)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            else if (this.Fields.ContainsKey(id) == false)
            {
                throw new KeyNotFoundException();
            }

            return this.Fields[id].Field.GetValue(instance);
        }

        public void SetField(object instance, int id, object value)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }
            else if (this.Fields.ContainsKey(id) == false)
            {
                throw new KeyNotFoundException();
            }

            this.Fields[id].Field.SetValue(instance, value);
        }

        public class ReflectedFieldType
        {
            public FieldType Type;
            public FieldInfo Field;
        }
    }
}
