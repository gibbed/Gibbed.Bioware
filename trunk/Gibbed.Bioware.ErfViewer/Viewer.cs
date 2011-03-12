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
using System.Windows.Forms;
using ERF = Gibbed.Bioware.FileFormats.EncapsulatedResourceFile;

namespace Gibbed.Bioware.ErfViewer
{
    public partial class Viewer : Form
    {
        public Viewer()
        {
            this.InitializeComponent();
            this.infoStatusLabel.Text = "";
            this.modeStatusLabel.Text = "";
        }

        private Setup.Manager Manager;

        private void OnLoad(object sender, EventArgs e)
        {
            this.LoadProject();
        }

        private void LoadProject()
        {
            try
            {
                this.Manager = Setup.Manager.Load();
                this.projectComboBox.Items.AddRange(this.Manager.ToArray());
                this.SetProject(this.Manager.ActiveProject);
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "There was an error while loading project data." +
                    Environment.NewLine + Environment.NewLine +
                    e.ToString() +
                    Environment.NewLine + Environment.NewLine +
                    "(You can press Ctrl+C to copy the contents of this dialog)",
                    "Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void SetProject(Setup.Project project)
        {
            if (project != null)
            {
                try
                {
                    project.Load();
                    this.openDialog.InitialDirectory = project.InstallPath;
                    this.saveKnownFileListDialog.InitialDirectory = project.ListsPath;
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        "There was an error while loading project data." +
                        Environment.NewLine + Environment.NewLine +
                        e.ToString() +
                        Environment.NewLine + Environment.NewLine +
                        "(You can press Ctrl+C to copy the contents of this dialog)",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    project = null;
                }
            }

            if (project != this.Manager.ActiveProject)
            {
                this.Manager.ActiveProject = project;
            }

            this.projectComboBox.SelectedItem = project;
        }

        private ERF Archive;
        private void BuildFileTree()
        {
            this.fileList.Nodes.Clear();
            this.fileList.BeginUpdate();

            if (this.Archive != null)
            {
                var dirNodes = new Dictionary<string, TreeNode>();

                var baseNode = new TreeNode(Path.GetFileName(this.openDialog.FileName), 0, 0);
                var knownNode = new TreeNode("Known", 1, 1);
                var unknownNode = new TreeNode("Unknown", 1, 1);

                var fileLookup = 
                    this.Manager.ActiveProject == null ? null : this.Manager.ActiveProject.FileHashLookup;
                var typeLookup =
                    this.Manager.ActiveProject == null ? null : this.Manager.ActiveProject.TypeHashLookup;

                long knownCount = 0;
                long unknownCount = 0;
                foreach (var entry in this.Archive.Entries
                    .OrderBy(k => k, new EntryComparer(fileLookup)))
                {
                    TreeNode node = null;

                    if (entry.Name != null || (fileLookup != null && fileLookup.ContainsKey(entry.NameHash) == true))
                    {
                        var fileName = entry.Name ?? fileLookup[entry.NameHash];
                        var pathName = Path.GetDirectoryName(fileName);
                        TreeNodeCollection parentNodes = knownNode.Nodes;

                        if (pathName.Length > 0)
                        {
                            string[] dirs = pathName.Split(new char[] { '\\' });

                            foreach (string dir in dirs)
                            {
                                if (parentNodes.ContainsKey(dir))
                                {
                                    parentNodes = parentNodes[dir].Nodes;
                                }
                                else
                                {
                                    TreeNode parentNode = parentNodes.Add(dir, dir, 2, 2);
                                    parentNodes = parentNode.Nodes;
                                }
                            }
                        }

                        var name = Path.GetFileName(fileName);

                        if (entry.Name != null)
                        {
                            name += " *";
                        }

                        node = parentNodes.Add(null, name, 3, 3);
                        knownCount++;
                    }
                    else
                    {
                        string fileName;
                        string typeName;

                        if (typeLookup != null && typeLookup.ContainsKey(entry.TypeHash) == true)
                        {
                            fileName =
                                entry.NameHash.ToString("X16") + "." +
                                typeLookup[entry.TypeHash];
                            typeName = "." + typeLookup[entry.TypeHash];
                        }
                        else
                        {
                            fileName =
                                entry.NameHash.ToString("X16") + "." +
                                entry.TypeHash.ToString("X8");
                            typeName = entry.TypeHash.ToString("X8");
                        }

                        var parentNodes =
                            unknownNode.Nodes;

                        if (parentNodes.ContainsKey(typeName) == true)
                        {
                            parentNodes = parentNodes[typeName].Nodes;
                        }
                        else
                        {
                            var parentNode = parentNodes.Add(typeName, typeName, 2, 2);
                            parentNodes = parentNode.Nodes;
                        }

                        node = parentNodes.Add(null,
                            fileName,
                            3, 3);
                        unknownCount++;
                    }

                    node.Tag = entry;
                }

                if (knownNode.Nodes.Count > 0)
                {
                    baseNode.Nodes.Add(knownNode);
                    knownNode.Text = "Known (" + knownCount.ToString() + ")";
                }

                if (unknownNode.Nodes.Count > 0)
                {
                    baseNode.Nodes.Add(unknownNode);
                    unknownNode.Text = "Unknown (" + unknownCount.ToString() + ")";
                }

                if (knownNode.Nodes.Count > 0)
                {
                    knownNode.Expand();
                }
                else if (unknownNode.Nodes.Count > 0)
                {
                    //unknownNode.Expand();
                }

                baseNode.Expand();
                this.fileList.Nodes.Add(baseNode);
            }

            //this.fileList.Sort();
            this.fileList.EndUpdate();
        }

        private void OnOpen(object sender, EventArgs e)
        {
            if (this.openDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (this.openDialog.InitialDirectory != null)
            {
                this.openDialog.InitialDirectory = null;
            }

            ERF archive;
            using (var input = this.openDialog.OpenFile())
            {
                archive = new ERF();
                archive.Deserialize(input);
            }
            this.Archive = archive;

            this.modeStatusLabel.Text =
                string.Format("Compression: {0}, Encryption: {1}",
                    archive.Compression, archive.Encryption);

            this.BuildFileTree();
        }

        private void OnSave(object sender, EventArgs e)
        {
            if (this.fileList.SelectedNode == null)
            {
                return;
            }

            string basePath;
            Dictionary<ulong, string> lookup;
            List<ERF.Entry> saving;

            SaveProgress.SaveAllSettings settings;
            settings.SaveOnlyKnownFiles = false;
            settings.DontOverwriteFiles = this.dontOverwriteFilesMenuItem.Checked;

            var root = this.fileList.SelectedNode;
            if (root.Nodes.Count == 0)
            {
                this.saveFileDialog.FileName = root.Text;

                if (this.saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var entry = (ERF.Entry)root.Tag;

                saving = new List<ERF.Entry>();
                saving.Add(entry);

                lookup = new Dictionary<ulong, string>();
                lookup.Add(entry.NameHash, Path.GetFileName(this.saveFileDialog.FileName));
                basePath = Path.GetDirectoryName(this.saveFileDialog.FileName);

                settings.DontOverwriteFiles = false;
            }
            else
            {
                if (this.saveFilesDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                saving = new List<ERF.Entry>();
                
                var nodes = new List<TreeNode>();
                nodes.Add(root);

                while (nodes.Count > 0)
                {
                    var node = nodes[0];
                    nodes.RemoveAt(0);

                    if (node.Nodes.Count > 0)
                    {
                        foreach (TreeNode child in node.Nodes)
                        {
                            if (child.Nodes.Count > 0)
                            {
                                nodes.Add(child);
                            }
                            else
                            {
                                saving.Add((ERF.Entry)child.Tag);
                            }
                        }
                    }
                }

                lookup = this.Manager.ActiveProject == null ? null : this.Manager.ActiveProject.FileHashLookup;
                basePath = this.saveFilesDialog.SelectedPath;
            }

            using (var input = File.OpenRead(this.openDialog.FileName))
            {
                using (var progress = new SaveProgress())
                {
                    progress.ShowSaveProgress(
                        this,
                        input,
                        this.Archive,
                        saving,
                        lookup,
                        this.Manager.ActiveProject == null ? null : this.Manager.ActiveProject.TypeHashLookup,
                        basePath,
                        settings);
                }
            }
        }

        private void OnSaveAll(object sender, EventArgs e)
        {
            if (this.saveFilesDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using (var input = File.OpenRead(this.openDialog.FileName))
            {
                string basePath = this.saveFilesDialog.SelectedPath;

                Dictionary<ulong, string> lookup =
                    this.Manager.ActiveProject == null ? null : this.Manager.ActiveProject.FileHashLookup;

                SaveProgress.SaveAllSettings settings;
                settings.SaveOnlyKnownFiles = this.saveOnlyKnownFilesMenuItem.Checked;
                settings.DontOverwriteFiles = this.dontOverwriteFilesMenuItem.Checked;

                using (var progress = new SaveProgress())
                {
                    progress.ShowSaveProgress(
                        this,
                        input,
                        this.Archive,
                        null,
                        lookup,
                        this.Manager.ActiveProject == null ? null : this.Manager.ActiveProject.TypeHashLookup,
                        basePath,
                        settings);
                }
            }
        }

        private void OnNodeSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || !(e.Node.Tag is ERF.Entry))
            {
                this.infoStatusLabel.Text = "";
                return;
            }

            var entry = (ERF.Entry)e.Node.Tag;
            this.infoStatusLabel.Text =
                string.Format("@ {0} : {1}",
                    entry.Offset, entry.CompressedSize);
        }

        private void OnProjectSelect(object sender, EventArgs e)
        {
            this.projectComboBox.Invalidate();
            var project = this.projectComboBox.SelectedItem as Setup.Project;
            if (project == null)
            {
                this.projectComboBox.Items.Remove(this.projectComboBox.SelectedItem);
            }
            this.SetProject(project);
            this.BuildFileTree();
        }

        private void OnReloadLists(object sender, EventArgs e)
        {
            if (this.Manager.ActiveProject != null)
            {
                this.Manager.ActiveProject.Reload();
            }

            this.BuildFileTree();
        }

        private void SaveKnownFileList(string outputPath)
        {
            var names = new List<string>();
            string headerLine = null;

            if (this.Archive != null &&
                this.Manager.ActiveProject != null)
            {
                int count = 0;
                foreach (var entry in this.Archive.Entries)
                {
                    string name = entry.Name;

                    if (name == null)
                    {
                        if (this.Manager.ActiveProject.FileHashLookup.ContainsKey(entry.NameHash) == true)
                        {
                            name = this.Manager.ActiveProject.FileHashLookup[entry.NameHash];
                        }
                    }

                    if (name != null && names.Contains(name) == false)
                    {
                        names.Add(name);
                        count++;
                    }
                }

                headerLine = string.Format("{0}/{1} ({2}%)",
                    count, this.Archive.Entries.Count,
                    (int)Math.Floor(((float)count / (float)this.Archive.Entries.Count) * 100));
            }

            names.Sort();

            using (var output = new StreamWriter(outputPath))
            {
                if (headerLine != null)
                {
                    output.WriteLine("; {0}", headerLine);
                }

                foreach (string name in names)
                {
                    output.WriteLine(name);
                }
            }
        }

        private void OnSaveKnownFileList(object sender, EventArgs e)
        {
            if (this.saveKnownFileListDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            this.SaveKnownFileList(this.saveKnownFileListDialog.FileName);
        }

        private void OnSaveKnownFileListToDefaultLocation(object sender, EventArgs e)
        {
            if (this.Archive == null)
            {
                MessageBox.Show(
                    "No open archive.",
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (this.Manager.ActiveProject == null)
            {
                MessageBox.Show(
                    "No active project.",
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // case insensitive, but who cares eh?

            var inputPath = this.openDialog.FileName.ToLowerInvariant();
            var installPath = this.Manager.ActiveProject.InstallPath.ToLowerInvariant();

            if (inputPath.StartsWith(installPath) == false)
            {
                MessageBox.Show(
                    "This archive is not within the install directory.",
                    "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var baseName = inputPath.Substring(installPath.Length + 1);
            
            string outputPath;
            outputPath = Path.Combine(this.Manager.ActiveProject.ListsPath, "files");
            outputPath = Path.Combine(outputPath, baseName);
            outputPath = Path.ChangeExtension(outputPath, ".filelist");

            if (MessageBox.Show(
                "Save to\n" + outputPath + "\n?",
                "Question",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            this.SaveKnownFileList(outputPath);
        }
    }
}
