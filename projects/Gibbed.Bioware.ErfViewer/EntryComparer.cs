﻿/* Copyright (c) 2017 Rick (rick 'at' gibbed 'dot' us)
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
using ERF = Gibbed.Bioware.FileFormats.EncapsulatedResourceFile;

namespace Gibbed.Bioware.ErfViewer
{
    internal class EntryComparer : IComparer<ERF.Entry>
    {
        private Gibbed.ProjectData.HashList<ulong> FileNames;

        public EntryComparer(Gibbed.ProjectData.HashList<ulong> names)
        {
            this.FileNames = names;
        }

        public int Compare(ERF.Entry x, ERF.Entry y)
        {
            if (x.Name != null && y.Name != null)
            {
                return String.Compare(x.Name, y.Name);
            }

            string x_n = x.Name;
            string y_n = y.Name;

            if (this.FileNames != null)
            {
                if (x_n == null && this.FileNames.Contains(x.NameHash) == true)
                {
                    x_n = this.FileNames[x.NameHash];
                }

                if (y_n == null && this.FileNames.Contains(y.NameHash) == true)
                {
                    y_n = this.FileNames[y.NameHash];
                }
            }

            if (x_n != null && y_n != null)
            {
                return String.Compare(x_n, y_n);
            }
            else if (x_n != null)
            {
                return -1;
            }
            else if (y_n != null)
            {
                return 1;
            }

            if (x.NameHash == y.NameHash)
            {
                return 0;
            }

            return x.NameHash < y.NameHash ? -1 : 1;
        }
    }
}
