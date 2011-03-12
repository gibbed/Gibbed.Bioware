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
using System.Threading;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ERF = Gibbed.Bioware.FileFormats.EncapsulatedResourceFile;

namespace Gibbed.Bioware.ErfViewer
{
	public partial class SaveProgress : Form
	{
		public SaveProgress()
		{
			this.InitializeComponent();
		}

		delegate void SetStatusDelegate(string status, int percent);
		private void SetStatus(string status, int percent)
		{
			if (this.progressBar.InvokeRequired || this.statusLabel.InvokeRequired)
			{
				SetStatusDelegate callback = new SetStatusDelegate(SetStatus);
				this.Invoke(callback, new object[] { status, percent });
				return;
			}

			this.statusLabel.Text = status;
			this.progressBar.Value = percent;
		}

		delegate void SaveDoneDelegate();
		private void SaveDone()
		{
			if (this.InvokeRequired)
			{
				SaveDoneDelegate callback = new SaveDoneDelegate(SaveDone);
				this.Invoke(callback);
				return;
			}

			this.Close();
		}

		public void SaveAll(object oinfo)
		{
			SaveAllInformation info = (SaveAllInformation)oinfo;
            IEnumerable<ERF.Entry> saving;

            if (info.Saving == null)
            {
                saving = info.Archive.Entries;
            }
            else
            {
                saving = info.Saving;
            }

            this.SetStatus("", 0);

            int total = saving.Count();
            int current = 0;

            var buffer = new byte[0x100000];
			foreach (var entry in saving)
			{
                current++;

				string fileName = null;

                if (entry.Name != null)
                {
                    fileName = entry.Name;
                }
                else
                {
                    if (info.FileNames.ContainsKey(entry.NameHash) == true)
                    {
                        fileName = info.FileNames[entry.NameHash];
                    }
                    else
                    {
                        if (info.Settings.SaveOnlyKnownFiles)
                        {
                            this.SetStatus("Skipping...", (int)(((float)current / (float)total) * 100.0f));
                            continue;
                        }

                        if (info.TypeNames != null &&
                            info.TypeNames.ContainsKey(entry.TypeHash) == true)
                        {
                            fileName =
                                entry.NameHash.ToString("X16") + "." +
                                info.TypeNames[entry.TypeHash];
                            fileName = Path.Combine(info.TypeNames[entry.TypeHash], fileName);
                        }
                        else
                        {
                            fileName =
                                entry.NameHash.ToString("X16") + "." +
                                entry.TypeHash.ToString("X8");
                        }

                        fileName = Path.Combine("__UNKNOWN", fileName);
                    }
                }

				string path = Path.Combine(info.BasePath, fileName);
                if (File.Exists(path) == true &&
                    info.Settings.DontOverwriteFiles == true)
                {
                    this.SetStatus("Skipping...", (int)(((float)current / (float)total) * 100.0f));
                    continue;
                }

                this.SetStatus(fileName, (int)(((float)current / (float)total) * 100.0f));

                Directory.CreateDirectory(Path.Combine(info.BasePath, Path.GetDirectoryName(fileName)));

                info.Stream.Seek(entry.Offset, SeekOrigin.Begin);

                using (var output = File.Create(path))
                {
                    Stream data = info.Stream;

                    switch (info.Archive.Encryption)
                    {
                        case ERF.EncryptionScheme.None:
                        {
                            //data = data;
                            break;
                        }

                        default:
                        {
                            throw new NotSupportedException("unsupported encryption scheme");
                        }
                    }

                    switch (info.Archive.Compression)
                    {
                        case ERF.CompressionScheme.BiowareZlib:
                        {
                            /* Bioware's zlib has a custom header of 1 byte
                             * rather than the normal two-byte zlib header.
                             * 
                             * The upper 4 bits are the window bits.
                             * 
                             * The uppermost bit must always be set to 1,
                             * since a valid window bits size must be from
                             * 8 to 15.
                             *
                             * Don't know what the lower 4 bits are for yet.
                             * 
                             * So for now, we'll ignore it their header.
                             */
                            data.Seek(1, SeekOrigin.Current);
                            data = new InflaterInputStream(
                                data, new Inflater(true));
                            break;
                        }

                        case ERF.CompressionScheme.HeaderlessZlib:
                        {
                            data = new InflaterInputStream(
                                data, new Inflater(true));
                            break;
                        }

                        case ERF.CompressionScheme.None:
                        {
                            //data = data;
                            break;
                        }

                        default:
                        {
                            throw new NotSupportedException("unsupported compression scheme");
                        }
                    }

                    long left = entry.UncompressedSize;
                    while (left > 0)
                    {
                        int block = (int)(Math.Min(left, buffer.Length));
                        int read = data.Read(buffer, 0, block);
                        output.Write(buffer, 0, read);
                        left -= read;
                    }
                }
			}

			this.SaveDone();
		}

        public struct SaveAllSettings
        {
            public bool SaveOnlyKnownFiles;
            public bool DontOverwriteFiles;
        }

		private struct SaveAllInformation
		{
			public string BasePath;
			public Stream Stream;
            public ERF Archive;
            public IEnumerable<ERF.Entry> Saving;
			public Dictionary<ulong, string> FileNames;
            public Dictionary<uint, string> TypeNames;
            public SaveAllSettings Settings;
		}

        private SaveAllInformation Info;
		private Thread SaveThread;

		public void ShowSaveProgress(
            IWin32Window owner,
            Stream stream,
            ERF archive,
            IEnumerable<ERF.Entry> saving,
            Dictionary<ulong, string> fileNames,
            Dictionary<uint, string> typeNames,
            string basePath,
            SaveAllSettings settings)
		{
			this.Info.BasePath = basePath;
            this.Info.Stream = stream;
            this.Info.Archive = archive;
            this.Info.Saving = saving;
            this.Info.FileNames = fileNames;
            this.Info.TypeNames = typeNames;
            this.Info.Settings = settings;

			this.progressBar.Value = 0;
			this.progressBar.Maximum = 100;

            this.SaveThread = new Thread(new ParameterizedThreadStart(SaveAll));
            this.ShowDialog(owner);
		}

		private void OnCancel(object sender, EventArgs e)
		{
			if (this.SaveThread != null)
			{
				this.SaveThread.Abort();
			}

			this.Close();
		}

        private void OnShown(object sender, EventArgs e)
        {
            this.SaveThread.Start(this.Info);
        }
	}
}
