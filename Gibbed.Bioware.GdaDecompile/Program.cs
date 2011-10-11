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
using System.Text;
using System.Xml;
using Gibbed.Bioware.FileFormats;
using NDesk.Options;
using GFF = Gibbed.Bioware.FileFormats.GenericFileFormat;

namespace Gibbed.Bioware.GdaDecompile
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

        private enum G2DAColumnType : byte
        {
            @string = 0,
            @int = 1,
            @float = 2,
            @bool = 3,
            @resource = 4,
            @invalid = 0xFF,
        }

        private enum ExportType
        {
            CSV,
            XLSX,
        }

        private static Dictionary<uint, string> Names = new Dictionary<uint, string>();

        public static void Main(string[] args)
        {
            var exportType = ExportType.CSV;
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
                    "csv",
                    "set output type to CSV", 
                    v => exportType = v != null ? ExportType.CSV : exportType
                },
                {
                    "xslx",
                    "set output type to XSLX", 
                    v => exportType = v != null ? ExportType.XLSX : exportType
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
                Console.WriteLine("Decompile GDA files to their 2DA counterparts.");
                Console.WriteLine();
                Console.WriteLine("Options:");
                options.WriteOptionDescriptions(Console.Out);

                Pause(pauseOnError);
                return;
            }

            string inputPath = extras[0];
            string outputPath = extras.Count > 1 ? extras[1] : Path.ChangeExtension(inputPath, "." + exportType.ToString().ToLowerInvariant());

            /*
            var manager = Setup.Manager.Load();
            if (manager.ActiveProject != null)
            {
                manager.ActiveProject.Load();
                Names = manager.ActiveProject.ColumnHashLookup;
            }
            else
            {
                Console.WriteLine("Warning: no active project loaded.");
            }
            */

            using (var gff = new GenericFile_Data())
            {
                using (var input = File.OpenRead(inputPath))
                {
                    gff.Deserialize(input);
                }

                if (gff.FormatType != GFF.FormatType.G2DA)
                {
                    Console.WriteLine("'{0}' is not a GDA file.", Path.GetFileName(outputPath));
                    Pause(pauseOnError);
                    return;
                }
                else if (gff.FormatVersion != 0x56302E32)
                {
                    Console.WriteLine("'{0}' has an unexpected version (wanted V0.2).", Path.GetFileName(outputPath));
                    Pause(pauseOnError);
                    return;
                }

                Console.WriteLine("Importing GDA...");

                var root = gff.Export();
                var columnDefinitions = root[10002].As<List<KeyValue>>();
                var rows = root[10003].As<List<KeyValue>>();

                Console.WriteLine("Validating GDA...");

                if (rows != null)
                {
                    foreach (var kv in rows)
                    {
                        if (kv.Values.Count != columnDefinitions.Count)
                        {
                            throw new FormatException("mismatching column count for row");
                        }

                        for (int i = 0; i < kv.Values.Count; i++)
                        {
                            var columnDefinition = columnDefinitions[i];
                            var type = columnDefinition[10999].As<byte>(0xFF);
                            var column = kv[10005 + i];

                            if (type > 4)
                            {
                                throw new FormatException("bad variable type");
                            }
                        }
                    }
                }

                using (var output = File.Create(outputPath))
                {
                    string name = Path.GetFileNameWithoutExtension(inputPath);
                    switch (exportType)
                    {
                        case ExportType.XLSX: ExportXSLT(name, output, root); break;
                        case ExportType.CSV: ExportCSV(name, output, root); break;
                        default:
                        {
                            throw new InvalidOperationException();
                        }
                    }
                }
            }
        }

        private static string GetColumnName(uint hash)
        {
            if (Names.ContainsKey(hash) == true)
            {
                return Names[hash];
            }

            return "0x" + hash.ToString("X8");
        }

        private static string EscapeCSV(string input)
        {
            bool enquote = false;

            if (input.Contains("\n") == true ||
                input.Contains("\n") == true ||
                input.Contains(",") == true ||
                input.Contains("\"") == true)
            {
                enquote = true;
            }
            
            if (input.Contains("\"") == true)
            {
                input = input.Replace("\"", "\"\"");
            }

            if (enquote == true)
            {
                input = "\"" + input + "\"";
            }

            return input;
        }

        private static void ExportCSV(string name, Stream output, KeyValue root)
        {
            Console.WriteLine("Decompiling data to CSV...");

            var columnDefinitions = root[10002].As<List<KeyValue>>();
            var rows = root[10003].As<List<KeyValue>>();

            using (var writer = new StreamWriter(output, Encoding.Unicode))
            {
                // names
                writer.WriteLine(columnDefinitions.Implode(
                    c => EscapeCSV(GetColumnName(c[10001].As<uint>(0xFFFFFFFF))),
                    ","));
                // types
                writer.WriteLine(columnDefinitions.Implode(
                    c => EscapeCSV(((G2DAColumnType)c[10999].As<byte>(0xFF)).ToString()),
                    ","));

                // write rows
                if (rows != null)
                {
                    foreach (var kv in rows)
                    {
                        for (int i = 0; i < kv.Values.Count; i++)
                        {
                            var columnDefinition = columnDefinitions[i];
                            var type = (G2DAColumnType)columnDefinition[10999].As<byte>(0xFF);
                            var column = kv[10005 + i];

                            switch (type)
                            {
                                case G2DAColumnType.@string:
                                {
                                    writer.Write((string)column.Value);
                                    break;
                                }

                                case G2DAColumnType.@int:
                                {
                                    writer.Write((int)column.Value);
                                    break;
                                }

                                case G2DAColumnType.@float:
                                {
                                    writer.Write((float)column.Value);
                                    break;
                                }

                                case G2DAColumnType.@bool:
                                {
                                    writer.Write((byte)column.Value);
                                    break;
                                }

                                case G2DAColumnType.@resource:
                                {
                                    writer.Write((string)column.Value);
                                    break;
                                }

                                default:
                                {
                                    throw new FormatException("unsupported variable type");
                                }
                            }

                            if (i + 1 < kv.Values.Count)
                            {
                                writer.Write(",");
                            }
                        }

                        writer.WriteLine();
                    }
                }
            }
        }

        private static void ExportXSLT(string name, Stream output, KeyValue root)
        {
            Console.WriteLine("Decompiling data to XLSX...");

            var columnDefinitions = root[10002].As<List<KeyValue>>();
            var rows = root[10003].As<List<KeyValue>>();

            var xml = XmlWriter.Create(output,
                new XmlWriterSettings()
                {
                    Indent = true,
                    Encoding = Encoding.ASCII,
                });

            // workbook
            xml.WriteStartDocument();
            xml.WriteStartElement("ss", "Workbook", "urn:schemas-microsoft-com:office:spreadsheet");
            xml.WriteAttributeString("xmlns", "x", null, "urn:schemas-microsoft-com:office:excel");
            xml.WriteAttributeString("xmlns", "o", null, "urn:schemas-microsoft-com:office:office");

            // worksheet
            xml.WriteStartElement("ss", "Worksheet", "urn:schemas-microsoft-com:office:spreadsheet");
            xml.WriteAttributeString("Name", "urn:schemas-microsoft-com:office:spreadsheet",
                name);

            // table
            xml.WriteStartElement("ss", "Table", "urn:schemas-microsoft-com:office:spreadsheet");

            // column definitions

            // name
            xml.WriteStartElement("ss", "Row", "urn:schemas-microsoft-com:office:spreadsheet");
            for (int i = 0; i < columnDefinitions.Count; i++)
            {
                var columnDefinition = columnDefinitions[i];
                var nameHash = columnDefinition[10001].As<uint>(0xFFFFFFFF);

                xml.WriteStartElement("ss", "Cell", "urn:schemas-microsoft-com:office:spreadsheet");
                xml.WriteStartElement("ss", "Data", "urn:schemas-microsoft-com:office:spreadsheet");
                xml.WriteAttributeString("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet",
                    "String");
                xml.WriteString(GetColumnName(nameHash));
                xml.WriteEndElement();
                xml.WriteEndElement();
            }
            xml.WriteEndElement();

            // type
            xml.WriteStartElement("ss", "Row", "urn:schemas-microsoft-com:office:spreadsheet");
            for (int i = 0; i < columnDefinitions.Count; i++)
            {
                var columnDefinition = columnDefinitions[i];
                var type = (G2DAColumnType)columnDefinition[10999].As<byte>(0xFF);

                xml.WriteStartElement("ss", "Cell", "urn:schemas-microsoft-com:office:spreadsheet");
                xml.WriteStartElement("ss", "Data", "urn:schemas-microsoft-com:office:spreadsheet");
                xml.WriteAttributeString("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet",
                    "String");
                xml.WriteString(type.ToString());
                xml.WriteEndElement();
                xml.WriteEndElement();
            }
            xml.WriteEndElement();

            // write rows
            if (rows != null)
            {
                foreach (var kv in rows)
                {
                    xml.WriteStartElement("ss", "Row", "urn:schemas-microsoft-com:office:spreadsheet");

                    for (int i = 0; i < kv.Values.Count; i++)
                    {
                        var columnDefinition = columnDefinitions[i];
                        var type = (G2DAColumnType)columnDefinition[10999].As<byte>(0xFF);
                        var column = kv[10005 + i];

                        xml.WriteStartElement("ss", "Cell", "urn:schemas-microsoft-com:office:spreadsheet");
                        xml.WriteStartElement("ss", "Data", "urn:schemas-microsoft-com:office:spreadsheet");

                        switch (type)
                        {
                            case G2DAColumnType.@string:
                            {
                                xml.WriteAttributeString("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet",
                                    "String");
                                xml.WriteString((string)column.Value);
                                break;
                            }

                            case G2DAColumnType.@int:
                            {
                                xml.WriteAttributeString("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet",
                                    "Number");
                                xml.WriteString(((int)column.Value).ToString());
                                break;
                            }

                            case G2DAColumnType.@float:
                            {
                                xml.WriteAttributeString("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet",
                                    "Number");
                                xml.WriteString(((float)column.Value).ToString());
                                break;
                            }

                            case G2DAColumnType.@bool:
                            {
                                throw new NotSupportedException();
                                /*
                                xml.WriteAttributeString("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet",
                                    "Boolean");
                                xml.WriteString(((int)column.Value).ToString());
                                break;*/
                            }

                            case G2DAColumnType.@resource:
                            {
                                xml.WriteAttributeString("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet",
                                    "String");
                                xml.WriteString((string)column.Value);
                                break;
                            }

                            default:
                            {
                                throw new FormatException("unsupported variable type");
                            }
                        }

                        xml.WriteEndElement();
                        xml.WriteEndElement();
                    }

                    xml.WriteEndElement();
                }
            }

            xml.WriteEndElement();
            xml.WriteEndElement();
            xml.WriteEndElement();
            xml.WriteEndDocument();
            xml.Flush();
        }
    }
}
