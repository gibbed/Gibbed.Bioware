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

namespace Gibbed.Bioware.FileFormats
{
    public static class StringHelpers
    {
        public static UInt32 HashFNV32(this string input)
        {
            return input.HashFNV32(0x811C9DC5);
        }

        public static UInt32 HashFNV32(this string input, UInt32 hash)
        {
            string lower = input.ToLowerInvariant();

            for (int i = 0; i < lower.Length; i++)
            {
                hash *= 0x1000193;
                hash ^= (char)(lower[i]);
            }

            return hash;
        }

        public static UInt64 HashFNV64(this string input)
        {
            return input.HashFNV64(0xCBF29CE484222325);
        }

        public static UInt64 HashFNV64(this string input, UInt64 hash)
        {
            string lower = input.ToLowerInvariant();

            for (int i = 0; i < lower.Length; i++)
            {
                hash *= 0x00000100000001B3;
                hash ^= (char)(lower[i]);
            }

            return hash;
        }
    }
}
