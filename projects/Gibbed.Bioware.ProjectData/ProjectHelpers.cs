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

using Gibbed.Bioware.FileFormats;

namespace Gibbed.Bioware.ProjectData
{
    public static class ProjectHelpers
    {
        public static Gibbed.ProjectData.HashList<ulong> LoadListsFileNames(
            this Gibbed.ProjectData.Manager manager)
        {
            return manager.LoadLists(
                    "*.filelist",
                    s => s.HashFNV64(),
                    s => s.ToLowerInvariant());
        }

        public static Gibbed.ProjectData.HashList<ulong> LoadListsFileNames(
            this Gibbed.ProjectData.Project project)
        {
            return project.LoadLists(
                    "*.filelist",
                    s => s.HashFNV64(),
                    s => s.ToLowerInvariant());
        }

        public static Gibbed.ProjectData.HashList<uint> LoadListsTypeNames(
            this Gibbed.ProjectData.Manager manager)
        {
            return manager.LoadLists(
                    "*.typelist",
                    s => s.HashFNV32(),
                    s => s.ToLowerInvariant());
        }

        public static Gibbed.ProjectData.HashList<uint> LoadListsTypeNames(
            this Gibbed.ProjectData.Project project)
        {
            return project.LoadLists(
                    "*.typelist",
                    s => s.HashFNV32(),
                    s => s.ToLowerInvariant());
        }

        public static Gibbed.ProjectData.HashList<uint> LoadListsColumnNames(
            this Gibbed.ProjectData.Manager manager)
        {
            return manager.LoadLists(
                    "*.columnlist",
                    s => s.HashCRC32(),
                    s => s.ToLowerInvariant());
        }

        public static Gibbed.ProjectData.HashList<uint> LoadListsColumnNames(
            this Gibbed.ProjectData.Project project)
        {
            return project.LoadLists(
                    "*.columnlist",
                    s => s.HashCRC32(),
                    s => s.ToLowerInvariant());
        }
    }
}
