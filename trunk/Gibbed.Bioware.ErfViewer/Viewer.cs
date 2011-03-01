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
using Gibbed.Bioware.FileFormats;
using ERF = Gibbed.Bioware.FileFormats.EncapsulatedResourceFile;

namespace Gibbed.Bioware.ErfViewer
{
    public partial class Viewer : Form
    {
        public Viewer()
        {
            this.InitializeComponent();
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

                        node = parentNodes.Add(null, Path.GetFileName(fileName), 3, 3);
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

            /*
            TextWriter writer = new StreamWriter("all_file_hashes.txt");
            foreach (var hash in table.Keys.OrderBy(k => k))
            {
                writer.WriteLine(hash.ToString("X8"));
            }
            writer.Close();
            */

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
                
                List<TreeNode> nodes = new List<TreeNode>();
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

        private void OnReloadLists(object sender, EventArgs e)
        {
            if (this.Manager.ActiveProject != null)
            {
                this.Manager.ActiveProject.Reload();
            }

            this.BuildFileTree();
        }

        private void OnProjectSelected(object sender, EventArgs e)
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

        private void OnSaveKnownFileList(object sender, EventArgs e)
        {
            if (this.saveKnownFileListDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            List<string> names = new List<string>();

            if (this.Archive != null &&
                this.Manager.ActiveProject != null)
            {
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
                    }
                }
            }

            names.Sort();

            TextWriter output = new StreamWriter(this.saveKnownFileListDialog.OpenFile());
            foreach (string name in names)
            {
                output.WriteLine(name);
            }
            output.Close();
        }
    }
}
