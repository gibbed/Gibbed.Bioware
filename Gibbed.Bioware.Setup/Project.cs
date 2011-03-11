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
using System.IO;
using System.Xml.XPath;
using Gibbed.Bioware.FileFormats;
using Microsoft.Win32;

namespace Gibbed.Bioware.Setup
{
    public class Project
    {
        public string Name { get; private set; }
        public bool Hidden { get; private set; }
        public string InstallPath { get; private set; }
        public string ListsPath { get; private set; }
        public List<string> Dependencies { get; private set; }
        private bool Loaded;

        public Dictionary<ulong, string> FileHashLookup { get; private set; }
        public Dictionary<uint, string> TypeHashLookup { get; private set; }
        public Dictionary<uint, string> GDAHashLookup { get; private set; }
        
        internal Manager Manager;

        private Project()
        {
            this.Dependencies = new List<string>();
            this.FileHashLookup = new Dictionary<ulong, string>();
            this.TypeHashLookup = new Dictionary<uint, string>();
            this.GDAHashLookup = new Dictionary<uint, string>();
            this.Loaded = false;
        }

        public void Load()
        {
            if (this.Loaded == false)
            {
                this.Reload();
            }
        }

        public void Reload()
        {
            this.Loaded = true;
            this.LoadFileLists();
            this.LoadTypeLists();
            this.LoadGDALists();
        }

        private void LoadFileLists()
        {
            this.FileHashLookup.Clear();

            foreach (var name in this.Dependencies)
            {
                var dependency = this.Manager[name];
                if (dependency != null)
                {
                    this.LoadFileListsFrom(dependency.ListsPath);
                }
            }

            this.LoadFileListsFrom(this.ListsPath);
        }

        private void LoadFileListsFrom(string basePath)
        {
            if (Directory.Exists(basePath) == false)
            {
                return;
            }

            foreach (string listPath in Directory.GetFiles(basePath, "*.filelist", SearchOption.AllDirectories))
            {
                using (var input = File.Open(listPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var reader = new StreamReader(input);

                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        else if (line.StartsWith(";") == true)
                        {
                            continue;
                        }

                        line = line.Trim();
                        if (line.Length <= 0)
                        {
                            continue;
                        }

                        line = line.Replace('/', '\\');
                        line = line.ToLowerInvariant();

                        //uint hash = Path.GetFileName(line).HashFileName();
                        ulong hash = line.HashFNV64();

                        if (this.FileHashLookup.ContainsKey(hash) == true &&
                            this.FileHashLookup[hash] != line)
                        {
                            string otherLine = this.FileHashLookup[hash];
                            throw new InvalidOperationException(
                                string.Format(
                                    "duplicate hash ('{0}' vs '{1}')",
                                    line,
                                    otherLine));
                        }

                        this.FileHashLookup[hash] = line; // .Add(hash, line);
                    }

                    reader.Close();
                }
            }
        }

        private void LoadTypeLists()
        {
            this.TypeHashLookup.Clear();

            foreach (var name in this.Dependencies)
            {
                var dependency = this.Manager[name];
                if (dependency != null)
                {
                    this.LoadTypeListsFrom(dependency.ListsPath);
                }
            }

            this.LoadTypeListsFrom(this.ListsPath);
        }

        private void LoadTypeListsFrom(string basePath)
        {
            if (Directory.Exists(basePath) == false)
            {
                return;
            }

            foreach (string listPath in Directory.GetFiles(basePath, "*.typelist", SearchOption.AllDirectories))
            {
                using (var input = File.Open(listPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    TextReader reader = new StreamReader(input);

                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        else if (line.StartsWith(";") == true)
                        {
                            continue;
                        }

                        line = line.Trim();
                        if (line.Length <= 0)
                        {
                            continue;
                        }

                        //uint hash = Path.GetFileName(line).HashFileName();
                        uint hash = line.HashFNV32();

                        if (this.TypeHashLookup.ContainsKey(hash) &&
                            this.TypeHashLookup[hash] != line)
                        {
                            string otherLine = this.TypeHashLookup[hash];
                            throw new InvalidOperationException("duplicate hash");
                        }

                        this.TypeHashLookup[hash] = line; // .Add(hash, line);
                    }

                    reader.Close();
                }
            }
        }

        private void LoadGDALists()
        {
            this.GDAHashLookup.Clear();

            foreach (var name in this.Dependencies)
            {
                var dependency = this.Manager[name];
                if (dependency != null)
                {
                    this.LoadGDAListsFrom(dependency.ListsPath);
                }
            }

            this.LoadGDAListsFrom(this.ListsPath);
        }

        private void LoadGDAListsFrom(string basePath)
        {
            if (Directory.Exists(basePath) == false)
            {
                return;
            }

            foreach (string listPath in Directory.GetFiles(basePath, "*.gdalist", SearchOption.AllDirectories))
            {
                using (var input = File.Open(listPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    TextReader reader = new StreamReader(input);

                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }
                        else if (line.StartsWith(";") == true)
                        {
                            continue;
                        }

                        line = line.Trim();
                        if (line.Length <= 0)
                        {
                            continue;
                        }

                        uint hash = line.HashCRC32();

                        if (this.GDAHashLookup.ContainsKey(hash) &&
                            this.GDAHashLookup[hash] != line)
                        {
                            string otherLine = this.GDAHashLookup[hash];
                            throw new InvalidOperationException("duplicate hash");
                        }

                        this.GDAHashLookup[hash] = line; // .Add(hash, line);
                    }

                    reader.Close();
                }
            }
        }

        internal static Project Create(string path, Manager manager)
        {
            var project = new Project();
            project.Manager = manager;

            var doc = new XPathDocument(path);
            var nav = doc.CreateNavigator();

            project.Name = nav.SelectSingleNode("/project/name").Value;
            project.ListsPath = nav.SelectSingleNode("/project/list_location").Value;

            project.Hidden = nav.SelectSingleNode("/project/hidden") != null;

            if (Path.IsPathRooted(project.ListsPath) == false)
            {
                project.ListsPath = Path.Combine(Path.GetDirectoryName(path), project.ListsPath);
            }

            project.Dependencies = new List<string>();
            var dependencies = nav.Select("/project/dependencies/dependency");
            while (dependencies.MoveNext() == true)
            {
                project.Dependencies.Add(dependencies.Current.Value);
            }

            project.InstallPath = null;
            var locations = nav.Select("/project/install_locations/install_location");
            while (locations.MoveNext() == true)
            {
                bool failed = true;

                var actions = locations.Current.Select("action");
                string locationPath = null;
                while (actions.MoveNext() == true)
                {
                    string type = actions.Current.GetAttribute("type", "");

                    switch (type)
                    {
                        case "registry":
                        {
                            string key = actions.Current.GetAttribute("key", "");
                            string value = actions.Current.GetAttribute("value", "");
                            
                            path = (string)Registry.GetValue(key, value, null);

                            if (path != null) // && Directory.Exists(path) == true)
                            {
                                locationPath = path;
                                failed = false;
                            }

                            break;
                        }

                        case "path":
                        {
                            locationPath = actions.Current.Value;

                            if (Directory.Exists(locationPath) == true)
                            {
                                failed = false;
                            }

                            break;
                        }

                        case "combine":
                        {
                            locationPath = Path.Combine(locationPath, actions.Current.Value);

                            if (Directory.Exists(locationPath) == true)
                            {
                                failed = false;
                            }

                            break;
                        }

                        case "directory_name":
                        {
                            locationPath = Path.GetDirectoryName(locationPath);

                            if (Directory.Exists(locationPath) == true)
                            {
                                failed = false;
                            }

                            break;
                        }

                        case "fix":
                        {
                            locationPath = locationPath.Replace('/', '\\');
                            failed = false;
                            break;
                        }

                        default:
                        {
                            throw new InvalidOperationException("unhandled install location action type");
                        }
                    }

                    if (failed == true)
                    {
                        break;
                    }
                }

                if (failed == false && Directory.Exists(locationPath) == true)
                {
                    project.InstallPath = locationPath;
                    break;
                }
            }

            return project;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
