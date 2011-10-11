using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Gibbed.Bioware.FileFormats
{
    public static class ProjectHelpers
    {
        public static ProjectData.HashList<ulong> LoadListsFileNames(
            this ProjectData.Manager manager)
        {
            return manager.LoadLists(
                    "*.filelist",
                    s => Path.GetFileName(s).HashFNV64(),
                    s => s.ToLowerInvariant());
        }

        public static ProjectData.HashList<ulong> LoadListsFileNames(
            this ProjectData.Project project)
        {
            return project.LoadLists(
                    "*.filelist",
                    s => Path.GetFileName(s).HashFNV64(),
                    s => s.ToLowerInvariant());
        }

        public static ProjectData.HashList<uint> LoadListsTypeNames(
            this ProjectData.Manager manager)
        {
            return manager.LoadLists(
                    "*.typelist",
                    s => s.HashFNV32(),
                    s => s.ToLowerInvariant());
        }

        public static ProjectData.HashList<uint> LoadListsTypeNames(
            this ProjectData.Project project)
        {
            return project.LoadLists(
                    "*.typelist",
                    s => s.HashFNV32(),
                    s => s.ToLowerInvariant());
        }

        public static ProjectData.HashList<uint> LoadListsColumnNames(
            this ProjectData.Manager manager)
        {
            return manager.LoadLists(
                    "*.columnlist",
                    s => s.HashCRC32(),
                    s => s.ToLowerInvariant());
        }

        public static ProjectData.HashList<uint> LoadListsColumnNames(
            this ProjectData.Project project)
        {
            return project.LoadLists(
                    "*.columnlist",
                    s => s.HashCRC32(),
                    s => s.ToLowerInvariant());
        }
    }
}
