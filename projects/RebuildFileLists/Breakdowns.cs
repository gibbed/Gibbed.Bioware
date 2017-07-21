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
 *    
 */
using System.Collections.Generic;
using System.Linq;

namespace RebuildFileLists
{
    internal class Breakdowns
    {
        public Dictionary<string, Breakdown> Entries
            = new Dictionary<string, Breakdown>();

        public Breakdown ToBreakdown()
        {
            return new Breakdown()
            {
                Known = this.Entries.Sum(e => e.Value.Known),
                Total = this.Entries.Sum(e => e.Value.Total),
            };
        }

        public Breakdown this[string id]
        {
            get
            {
                if (this.Entries.ContainsKey(id) == false)
                {
                    return this.Entries[id] = new Breakdown();
                }

                return this.Entries[id];
            }
        }

        public static Breakdowns operator +(Breakdowns a, Breakdowns b)
        {
            var c = new Breakdowns();

            foreach (var kvp in a.Entries)
            {
                c[kvp.Key].Known += kvp.Value.Known;
                c[kvp.Key].Total += kvp.Value.Total;
            }

            foreach (var kvp in b.Entries)
            {
                c[kvp.Key].Known += kvp.Value.Known;
                c[kvp.Key].Total += kvp.Value.Total;
            }

            return c;
        }
    }
}
