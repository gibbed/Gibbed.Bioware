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
using NDesk.Options;
using ProjectData = Gibbed.ProjectData;

namespace RebuildFileLists
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        private static string GetListPath(string installPath, string inputPath)
        {
            installPath = installPath.ToLowerInvariant();
            inputPath = inputPath.ToLowerInvariant();

            if (inputPath.StartsWith(installPath) == false)
            {
                return null;
            }

            var baseName = inputPath.Substring(installPath.Length + 1);

            string outputPath;
            outputPath = Path.Combine("files", baseName);
            outputPath = Path.ChangeExtension(outputPath, ".filelist");
            return outputPath;
        }

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
            var fileNames = project.LoadListsFileNames();
            var typeNames = project.LoadListsTypeNames();

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

            Console.WriteLine("Searching for ERFs...");
            var inputPaths = new List<string>();
            /* would have added just the root paths
             * but that nets the addins directory too... */
            /*
            inputPaths.AddRange(Directory.GetFiles(Path.Combine(installPath, "modules"), "*.erf", SearchOption.AllDirectories));
            inputPaths.AddRange(Directory.GetFiles(Path.Combine(installPath, "modules"), "*.rim", SearchOption.AllDirectories));
            inputPaths.AddRange(Directory.GetFiles(Path.Combine(installPath, "packages"), "*.erf", SearchOption.AllDirectories));
            inputPaths.AddRange(Directory.GetFiles(Path.Combine(installPath, "packages"), "*.rim", SearchOption.AllDirectories));
            */
            inputPaths.AddRange(Directory.GetFiles(installPath, "*.erf", SearchOption.AllDirectories));
            inputPaths.AddRange(Directory.GetFiles(installPath, "*.crf", SearchOption.AllDirectories));
            inputPaths.AddRange(Directory.GetFiles(installPath, "*.rim", SearchOption.AllDirectories));

            var outputPaths = new List<string>();

            var breakdowns = new Breakdowns();

            Console.WriteLine("Processing...");
            foreach (var inputpath in inputPaths)
            {
                var outputPath = GetListPath(installPath, inputpath);
                if (outputPath == null)
                {
                    throw new InvalidOperationException();
                }

                Console.WriteLine(outputPath);
                outputPath = Path.Combine(listsPath, outputPath);

                if (outputPaths.Contains(outputPath) == true)
                {
                    throw new InvalidOperationException();
                }

                outputPaths.Add(outputPath);

                var erf = new EncapsulatedResourceFile();
                using (var input = File.OpenRead(inputpath))
                {
                    erf.Deserialize(input);
                }

                var localBreakdowns = new Breakdowns();

                var names = new List<string>();
                foreach (var entry in erf.Entries)
                {
                    string name = entry.Name;

                    if (name == null)
                    {
                        name = fileNames[entry.NameHash];
                    }

                    var type = typeNames[entry.TypeHash] ?? entry.TypeHash.ToString("X8");

                    if (name != null)
                    {
                        name = name.ToLowerInvariant();
                        if (names.Contains(name) == false)
                        {
                            names.Add(name);

                            localBreakdowns[type].Known++;
                        }
                    }

                    localBreakdowns[type].Total++;
                }

                breakdowns += localBreakdowns;

                names.Sort();

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                using (var output = new StreamWriter(outputPath))
                {
                    var breakdown = localBreakdowns.ToBreakdown();

                    output.WriteLine("; {0}", breakdown);

                    if (breakdown.Known == 0 ||
                        localBreakdowns.Entries.Count != 1)
                    {
                        output.WriteLine("; ");

                        int padding = localBreakdowns.Entries.Max(e => e.Key.Length);
                        foreach (var kvp in localBreakdowns.Entries.OrderBy(e => e.Key))
                        {
                            output.WriteLine("; {0} - {1}",
                                kvp.Key.PadLeft(padding),
                                kvp.Value.ToString());
                        }
                        output.WriteLine("; ");
                    }

                    foreach (string name in names)
                    {
                        output.WriteLine(name);
                    }
                }
            }

            using (var output = new StreamWriter(Path.Combine(Path.Combine(listsPath, "files"), "status.txt")))
            {
                output.WriteLine("{0}", breakdowns.ToBreakdown());

                int padding = breakdowns.Entries.Max(e => e.Key.Length);

                var incomplete = breakdowns.Entries.Where(e => e.Value.Percent < 100.0);
                var complete = breakdowns.Entries.Where(e => e.Value.Percent >= 100.0);

                if (incomplete.Count() > 0)
                {
                    output.WriteLine();
                    foreach (var kvp in incomplete.OrderBy(e => e.Value.Percent))
                    {
                        output.WriteLine("{0} - {1}",
                            kvp.Key.PadLeft(padding),
                            kvp.Value.ToString());
                    }
                }

                if (complete.Count() > 0)
                {
                    output.WriteLine();
                    foreach (var kvp in complete.OrderBy(e => e.Key))
                    {
                        output.WriteLine("{0} - {1}",
                            kvp.Key.PadLeft(padding),
                            kvp.Value.ToString());
                    }
                }
            }
        }
    }
}
