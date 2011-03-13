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
using System.Linq;
using Gibbed.Bioware.FileFormats;
using Gibbed.Helpers;
using NDesk.Options;
using ERF = Gibbed.Bioware.FileFormats.EncapsulatedResourceFile;

namespace Gibbed.Bioware.ErfPack
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        public static void Main(string[] args)
        {
            bool showHelp = false;
            bool verbose = false;
            bool stripFileNames = false;

            OptionSet options = new OptionSet()
            {
                {
                    "v|verbose",
                    "be verbose",
                    v => verbose = v != null
                },
                {
                    "s|strip",
                    "strip file names",
                    v => stripFileNames = v != null
                },
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

            if (extras.Count < 1 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ output_erf input_directory+", GetExecutableName());
                Console.WriteLine("Pack files from input directories into a Encapsulated Resource File.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            var inputPaths = new List<string>();
            string outputPath;

            if (extras.Count == 1)
            {
                inputPaths.Add(extras[0]);
                outputPath = Path.ChangeExtension(extras[0], ".erf");
            }
            else
            {
                outputPath = extras[0];
                inputPaths.AddRange(extras.Skip(1));
            }

            var paths = new SortedDictionary<ulong, string>();
            var lookup = new Dictionary<ulong, string>();

            if (verbose == true)
            {
                Console.WriteLine("Finding files...");
            }

            foreach (var relPath in inputPaths)
            {
                string inputPath = Path.GetFullPath(relPath);

                if (inputPath.EndsWith(Path.DirectorySeparatorChar.ToString()) == true)
                {
                    inputPath = inputPath.Substring(0, inputPath.Length - 1);
                }

                foreach (string path in Directory.GetFiles(inputPath, "*", SearchOption.AllDirectories))
                {
                    bool hasName;
                    string fullPath = Path.GetFullPath(path);
                    string partPath = fullPath.Substring(inputPath.Length + 1).ToLowerInvariant();

                    ulong hash = 0xFFFFFFFFFFFFFFFFul;
                    if (partPath.ToUpper().StartsWith("__UNKNOWN") == true)
                    {
                        string partName;
                        partName = Path.GetFileNameWithoutExtension(partPath);
                        if (partName.Length > 8)
                        {
                            partName = partName.Substring(0, 8);
                        }

                        hash = ulong.Parse(
                            partName,
                            System.Globalization.NumberStyles.AllowHexSpecifier);
                        hasName = false;
                    }
                    else
                    {
                        hash = partPath.ToLowerInvariant().HashFNV64();
                        hasName = true;
                    }

                    if (paths.ContainsKey(hash) == true)
                    {
                        Console.WriteLine("Ignoring {0} duplicate.", partPath);
                        continue;
                    }

                    paths[hash] = fullPath;
                    lookup[hash] = (stripFileNames == false && hasName == true) ? partPath : null;
                }
            }

            using (var output = File.Open(outputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var erf = new ERF();
                erf.Version = 3;
                erf.Compression = ERF.CompressionScheme.None;
                erf.Encryption = ERF.EncryptionScheme.None;
                erf.ContentId = 0;

                if (verbose == true)
                {
                    Console.WriteLine("Adding files...");
                }

                long headerSize = erf.CalculateHeaderSize(
                    lookup.Values.ToArray(), paths.Count);
                long baseOffset = headerSize.Align(16);

                if (verbose == true)
                {
                    Console.WriteLine("Writing to disk...");
                }

                erf.Entries.Clear();
                output.Seek(baseOffset, SeekOrigin.Begin);
                foreach (var kvp in paths)
                {
                    if (verbose == true)
                    {
                        Console.WriteLine(kvp.Value);
                    }

                    using (var input = File.Open(kvp.Value, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var entry = new ERF.Entry()
                        {
                            Name = lookup[kvp.Key],
                            NameHash = kvp.Key,
                            TypeHash = 0,
                            Offset = output.Position,
                            CompressedSize = (uint)input.Length,
                            UncompressedSize = (uint)input.Length,
                        };

                        if (entry.Name != null)
                        {
                            entry.CalculateHashes();
                        }
                        else
                        {
                            var extension = Path.GetExtension(kvp.Value);
                            entry.TypeHash = extension == null ?
                                0 : extension.Trim('.').HashFNV32();
                        }

                        output.WriteFromStream(input, input.Length);
                        output.Seek(output.Position.Align(16), SeekOrigin.Begin);
                        erf.Entries.Add(entry);
                    }
                }

                output.Seek(0, SeekOrigin.Begin);
                erf.Serialize(output);

                if (output.Position != headerSize)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
