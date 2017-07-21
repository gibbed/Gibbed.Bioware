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
using System.Text;
using Gibbed.Bioware.FileFormats;
using NDesk.Options;
using GFF = Gibbed.Bioware.FileFormats.GenericFileFormat;

namespace Gibbed.Bioware.TlkDowngrade
{
    internal class Program
    {
        private static string GetExecutableName()
        {
            return Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
        }

        private static void Pause(bool shouldPause)
        {
            if (shouldPause == true)
            {
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadKey(true);
            }
        }

        public static void Main(string[] args)
        {
            bool overwriteFiles = false;
            bool showHelp = false;
            bool pauseOnError = true;

            OptionSet options = new OptionSet()
            {
                {
                    "np|nopause",
                    "don't pause on errors",
                    v => pauseOnError = v == null
                },
                {
                    "o|overwrite",
                    "overwrite files if they already exist", 
                    v => overwriteFiles = v != null
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

                Pause(pauseOnError);
                return;
            }

            if (extras.Count < 1 || extras.Count > 2 || showHelp == true)
            {
                Console.WriteLine("Usage: {0} [OPTIONS]+ input_tlk [output_tlk]", GetExecutableName());
                Console.WriteLine("Convert TLK V0.5 files to V0.2.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);

                Pause(pauseOnError);
                return;
            }

            string inputPath = extras[0];

            string outputPath;
            if (extras.Count > 1)
            {
                outputPath = extras[1];
            }
            else
            {
                outputPath = Path.GetFileNameWithoutExtension(inputPath);
                outputPath += "_downgraded";
                outputPath = Path.ChangeExtension(outputPath, Path.GetExtension(inputPath));
                outputPath = Path.Combine(Path.GetDirectoryName(inputPath), outputPath);
            }

            if (overwriteFiles == false &&
                File.Exists(outputPath))
            {
                Console.WriteLine("'{0}' already exists.", Path.GetFileName(outputPath));
                Pause(pauseOnError);
                return;
            }

            using (var gff = new GenericFile_Type())
            {
                using (var input = File.OpenRead(inputPath))
                {
                    gff.Deserialize(input);
                }

                if (gff.FormatType != GFF.FormatType.TLK ||
                    gff.FormatVersion != 0x56302E35)
                {
                    Console.WriteLine("'{0}' is not a TLK V0.5 file.", Path.GetFileName(outputPath));
                    Pause(pauseOnError);
                    return;
                }

                Console.WriteLine("Importing HTLK...");

                var htlk = gff.ExportType<HTLK>();
                var tlk = new TLK();

                //int cursor = Console.CursorLeft;
                //Console.Write("Decoding {0} strings... ", htlk.Strings.Count);

                Console.WriteLine("Decoding {0} strings... ", htlk.Strings.Count);

                foreach (var str in htlk.Strings)
                {
                    //Console.CursorLeft = cursor;
                    //Console.Write(str.Id.ToString());

                    tlk.Strings.Add(
                        new TLK.STRN()
                        {
                            Id = str.Id,
                            String = Decode(htlk, str),
                        });
                }

                //Console.CursorLeft = cursor;
                //Console.WriteLine("done.     ");

                Console.WriteLine("Exporting TLK...");

                gff.ImportType(tlk);
                gff.FormatVersion = 0x56302E32;

                using (var output = File.Create(outputPath))
                {
                    gff.Serialize(output);
                }
            }
        }

        private static string Decode(HTLK htlk, HTLK.HSTR str)
        {
            var sb = new StringBuilder();

            var index = (int)(str.Start >> 5);
            var shift = (int)(str.Start & 0x1F);

            while (true)
            {
                int e = (htlk.Data1.Count / 2) - 1;
                while (e > 0)
                {
                    int offset = (int)((htlk.Data2[index] >> shift) & 1);
                    e = htlk.Data1[(e * 2) + offset];

                    shift++;
                    index += (shift >> 5);

                    shift %= 32;
                }

                var c = (ushort)(0xFFFF - e);
                if (c == 0)
                {
                    break;
                }

                sb.Append((char)c);
            }

            return sb.ToString();
        }
    }
}
