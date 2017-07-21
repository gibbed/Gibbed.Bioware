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
using System.IO;
using System.Linq;
using Gibbed.Bioware.FileFormats;
using Gibbed.Bioware.ProjectData;
using NDesk.Options;
using ProjectData = Gibbed.ProjectData;

namespace RebuildColumnLists
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        private static string GetListPath(string inputPath)
        {
            inputPath = inputPath.ToLowerInvariant();

            var baseName = Path.GetFileNameWithoutExtension(inputPath);

            string outputPath;
            outputPath = Path.Combine("2da", baseName);
            outputPath = Path.Combine("columns", outputPath);
            outputPath = Path.ChangeExtension(outputPath, ".columnlist");
            return outputPath;
        }

        private static readonly uint EXT = "gda".HashFNV32();

        public static void Main(string[] args)
        {
            bool showHelp = false;

            OptionSet options = new OptionSet()
            {
                {
                    "h|help",
                    "show this message and exit", 
                    v => showHelp = v != null
                },
            };

            List<string> extras;

            try
            {
                extras = options.Parse(args);
            }
            catch (OptionException e)
            {
                Console.Write("{0}: ", GetExecutableName());
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `{0} --help' for more information.", GetExecutableName());
                return;
            }

            if (extras.Count != 0 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+", GetExecutableName());
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            Console.WriteLine("Loading project...");

            var manager = ProjectData.Manager.Load();
            if (manager.ActiveProject == null)
            {
                Console.WriteLine("Nothing to do: no active project loaded.");
                return;
            }

            var project = manager.ActiveProject;
            var fileNames = manager.LoadListsFileNames();
            var columnNames = manager.LoadListsColumnNames();

            var installPath = project.InstallPath;
            var listsPath = project.ListsPath;

            if (installPath == null)
            {
                Console.WriteLine("Could not detect install path.");
                return;
            }
            else if (listsPath == null)
            {
                Console.WriteLine("Could not detect lists path.");
                return;
            }

            var inputPath = Path.Combine(installPath, @"packages\core\data\2da.rim");

            Console.WriteLine("Processing...");
            
            var results = new Dictionary<uint, string>();

            var erf = new EncapsulatedResourceFile();
            using (var input = File.OpenRead(inputPath))
            {
                erf.Deserialize(input);

                var loader = new Loader(erf);
                foreach (var entry in loader)
                {
                    if (entry.TypeHash != EXT)
                    {
                        continue;
                    }

                    string inputName = entry.Name;

                    if (inputName == null)
                    {
                        if (fileNames.Contains(entry.NameHash) == false)
                        {
                            continue;
                        }

                        inputName = fileNames[entry.NameHash];
                    }

                    var outputPath = GetListPath(inputName);
                    outputPath = Path.Combine(listsPath, outputPath);

                    var data = loader.Load(input, entry);

                    var localResults = new Dictionary<uint, string>();

                    var gff = new GenericFile_Data();
                    gff.Deserialize(data);

                    var root = gff.Export();
                    var columns = root[10002].As<List<KeyValue>>(null);
                    if (columns != null)
                    {
                        foreach (var column in columns)
                        {
                            var id = column[10001].As<uint>();
                            var name = columnNames[id];
                            localResults.Add(id, name);
                            results[id] = name;
                        }
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                    using (var output = new StreamWriter(outputPath))
                    {
                        var breakdown = new Breakdown();
                        breakdown.Known = localResults.Where(r => r.Value != null).Count();
                        breakdown.Total = localResults.Count;

                        output.WriteLine("; {0}", breakdown.ToString());

                        foreach (var kvp in localResults)
                        {
                            if (kvp.Value == null)
                            {
                                output.WriteLine("; {0:X8}", kvp.Key);
                            }
                            else
                            {
                                output.WriteLine("{0}", kvp.Value);
                            }
                        }
                    }
                }
            }

            using (var output = new StreamWriter(Path.Combine(Path.Combine(listsPath, "columns"), "status.txt")))
            {
                var breakdown = new Breakdown();
                breakdown.Known = results.Where(r => r.Value != null).Count();
                breakdown.Total = results.Count;
                output.WriteLine("{0}", breakdown.ToString());
            }

            using (var output = new StreamWriter(Path.Combine(Path.Combine(listsPath, "columns"), "master.columnlist")))
            {
                foreach (var result in results.Where(r => r.Value != null).OrderBy(r => r.Value))
                {
                    output.WriteLine(result.Value);
                }
            }
        }
    }
}
