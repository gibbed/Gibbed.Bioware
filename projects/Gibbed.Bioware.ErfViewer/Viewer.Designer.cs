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

namespace Gibbed.Bioware.ErfViewer
{
    partial class Viewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Viewer));
            this.mainToolStrip = new System.Windows.Forms.ToolStrip();
            this.projectComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.openButton = new System.Windows.Forms.ToolStripButton();
            this.saveAllButton = new System.Windows.Forms.ToolStripButton();
            this.reloadListsButton = new System.Windows.Forms.ToolStripButton();
            this.settingsButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.saveOnlyKnownFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dontOverwriteFilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileList = new System.Windows.Forms.TreeView();
            this.fileMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFilesDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.saveKnownFileListDialog = new System.Windows.Forms.SaveFileDialog();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.infoStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.spacerStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.modeStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainToolStrip.SuspendLayout();
            this.fileMenuStrip.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainToolStrip
            // 
            this.mainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectComboBox,
            this.openButton,
            this.saveAllButton,
            this.reloadListsButton,
            this.settingsButton});
            this.mainToolStrip.Location = new System.Drawing.Point(0, 0);
            this.mainToolStrip.Name = "mainToolStrip";
            this.mainToolStrip.Size = new System.Drawing.Size(640, 25);
            this.mainToolStrip.TabIndex = 0;
            this.mainToolStrip.Text = "toolStrip1";
            // 
            // projectComboBox
            // 
            this.projectComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.projectComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.projectComboBox.Name = "projectComboBox";
            this.projectComboBox.Size = new System.Drawing.Size(181, 25);
            this.projectComboBox.Sorted = true;
            this.projectComboBox.SelectedIndexChanged += new System.EventHandler(this.OnProjectSelect);
            // 
            // openButton
            // 
            this.openButton.Image = global::Gibbed.Bioware.ErfViewer.Properties.Resources.OpenArchive;
            this.openButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(56, 22);
            this.openButton.Text = "&Open";
            this.openButton.Click += new System.EventHandler(this.OnOpen);
            // 
            // saveAllButton
            // 
            this.saveAllButton.Image = global::Gibbed.Bioware.ErfViewer.Properties.Resources.SaveAllFiles;
            this.saveAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveAllButton.Name = "saveAllButton";
            this.saveAllButton.Size = new System.Drawing.Size(68, 22);
            this.saveAllButton.Text = "Save &All";
            this.saveAllButton.Click += new System.EventHandler(this.OnSaveAll);
            // 
            // reloadListsButton
            // 
            this.reloadListsButton.Image = global::Gibbed.Bioware.ErfViewer.Properties.Resources.ReloadLists;
            this.reloadListsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.reloadListsButton.Name = "reloadListsButton";
            this.reloadListsButton.Size = new System.Drawing.Size(89, 22);
            this.reloadListsButton.Text = "&Reload Lists";
            this.reloadListsButton.Click += new System.EventHandler(this.OnReloadLists);
            // 
            // settingsButton
            // 
            this.settingsButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveOnlyKnownFilesMenuItem,
            this.dontOverwriteFilesMenuItem});
            this.settingsButton.Image = global::Gibbed.Bioware.ErfViewer.Properties.Resources.Settings;
            this.settingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(78, 22);
            this.settingsButton.Text = "Settings";
            // 
            // saveOnlyKnownFilesMenuItem
            // 
            this.saveOnlyKnownFilesMenuItem.CheckOnClick = true;
            this.saveOnlyKnownFilesMenuItem.Name = "saveOnlyKnownFilesMenuItem";
            this.saveOnlyKnownFilesMenuItem.Size = new System.Drawing.Size(187, 22);
            this.saveOnlyKnownFilesMenuItem.Text = "Save only &known files";
            // 
            // dontOverwriteFilesMenuItem
            // 
            this.dontOverwriteFilesMenuItem.CheckOnClick = true;
            this.dontOverwriteFilesMenuItem.Name = "dontOverwriteFilesMenuItem";
            this.dontOverwriteFilesMenuItem.Size = new System.Drawing.Size(187, 22);
            this.dontOverwriteFilesMenuItem.Text = "Don\'t &overwrite files";
            // 
            // fileList
            // 
            this.fileList.ContextMenuStrip = this.fileMenuStrip;
            this.fileList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileList.Location = new System.Drawing.Point(0, 25);
            this.fileList.Name = "fileList";
            this.fileList.Size = new System.Drawing.Size(640, 273);
            this.fileList.TabIndex = 1;
            this.fileList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnNodeSelect);
            // 
            // fileMenuStrip
            // 
            this.fileMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem});
            this.fileMenuStrip.Name = "fileMenuStrip";
            this.fileMenuStrip.Size = new System.Drawing.Size(99, 26);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.OnSave);
            // 
            // openDialog
            // 
            this.openDialog.Filter = "Bioware Archives (*.erf, *.crf, *.rim)|*.erf;*.crf;*.rim|All Files (*.*)|*.*";
            // 
            // saveFilesDialog
            // 
            this.saveFilesDialog.Description = "Select a directory to save all files from the archive to.";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "All Files (*.*)|*.*";
            this.saveFileDialog.RestoreDirectory = true;
            // 
            // saveKnownFileListDialog
            // 
            this.saveKnownFileListDialog.DefaultExt = "filelist";
            this.saveKnownFileListDialog.Filter = "File List (*.filelist)|*.filelist";
            this.saveKnownFileListDialog.RestoreDirectory = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.infoStatusLabel,
            this.spacerStatusLabel,
            this.modeStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 298);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(640, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // infoStatusLabel
            // 
            this.infoStatusLabel.Name = "infoStatusLabel";
            this.infoStatusLabel.Size = new System.Drawing.Size(36, 17);
            this.infoStatusLabel.Text = "(info)";
            // 
            // spacerStatusLabel
            // 
            this.spacerStatusLabel.Name = "spacerStatusLabel";
            this.spacerStatusLabel.Size = new System.Drawing.Size(543, 17);
            this.spacerStatusLabel.Spring = true;
            this.spacerStatusLabel.Text = " ";
            // 
            // modeStatusLabel
            // 
            this.modeStatusLabel.Name = "modeStatusLabel";
            this.modeStatusLabel.Size = new System.Drawing.Size(46, 17);
            this.modeStatusLabel.Text = "(mode)";
            // 
            // Viewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 320);
            this.Controls.Add(this.fileList);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.mainToolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Viewer";
            this.Text = "Gibbed\'s Bioware ERF Viewer";
            this.Load += new System.EventHandler(this.OnLoad);
            this.mainToolStrip.ResumeLayout(false);
            this.mainToolStrip.PerformLayout();
            this.fileMenuStrip.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip mainToolStrip;
        private System.Windows.Forms.ToolStripButton openButton;
        private System.Windows.Forms.TreeView fileList;
        private System.Windows.Forms.OpenFileDialog openDialog;
        private System.Windows.Forms.FolderBrowserDialog saveFilesDialog;
        private System.Windows.Forms.ToolStripComboBox projectComboBox;
        private System.Windows.Forms.ContextMenuStrip fileMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripButton saveAllButton;
        private System.Windows.Forms.ToolStripDropDownButton settingsButton;
        private System.Windows.Forms.ToolStripMenuItem saveOnlyKnownFilesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dontOverwriteFilesMenuItem;
        private System.Windows.Forms.SaveFileDialog saveKnownFileListDialog;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel infoStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel modeStatusLabel;
        private System.Windows.Forms.ToolStripStatusLabel spacerStatusLabel;
        private System.Windows.Forms.ToolStripButton reloadListsButton;
    }
}

