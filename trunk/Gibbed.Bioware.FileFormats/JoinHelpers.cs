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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gibbed.Bioware.FileFormats
{
    public static class JoinHelper
    {
        public static string Implode<T>(
            this IEnumerable<T> source,
            Func<T, string> projection,
            string separator)
        {
            if (source.Count() == 0)
            {
                return "";
            }

            var builder = new StringBuilder();

            builder.Append(projection(source.First()));
            foreach (T element in source.Skip(1))
            {
                builder.Append(separator);
                builder.Append(projection(element));
            }

            return builder.ToString();
        }

        public static string Implode<T>(
            this IEnumerable<T> source,
            string separator)
        {
            return Implode(source, t => t.ToString(), separator);
        }
    }
}
