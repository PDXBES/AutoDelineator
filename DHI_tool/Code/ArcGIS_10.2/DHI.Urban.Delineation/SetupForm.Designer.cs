// DHI Urban Catchment Delineation
// Copyright (c) 2007, 2010, 2012-2014 DHI Water & Environment, Inc.
// Author: Arnold Engelmann, ahe@dhigroup.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
//
// All other rights reserved.

namespace DHI.Urban.Delineation
{
	partial class SetupForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupForm));
      this.lblPipes = new System.Windows.Forms.Label();
      this.lblDEM = new System.Windows.Forms.Label();
      this.lblFlowDirGrid = new System.Windows.Forms.Label();
      this.btnOK = new System.Windows.Forms.Button();
      this.btnCancel = new System.Windows.Forms.Button();
      this.cbxDEM = new System.Windows.Forms.ComboBox();
      this.cbxFlowDirGrid = new System.Windows.Forms.ComboBox();
      this.lblInlets = new System.Windows.Forms.Label();
      this.chkIncludeUp = new System.Windows.Forms.CheckBox();
      this.cbxNetwork = new System.Windows.Forms.ComboBox();
      this.cbxInlets = new System.Windows.Forms.ComboBox();
      this.chkExcludeDown = new System.Windows.Forms.CheckBox();
      this.btnPreprocess = new System.Windows.Forms.Button();
      this.cbxCatchments = new System.Windows.Forms.ComboBox();
      this.lblInletCatchments = new System.Windows.Forms.Label();
      this.grpAdvanced = new System.Windows.Forms.GroupBox();
      this.chkExcludeDisabled = new System.Windows.Forms.CheckBox();
      this.chkSmooth = new System.Windows.Forms.CheckBox();
      this.grpAdvanced.SuspendLayout();
      this.SuspendLayout();
      // 
      // lblPipes
      // 
      this.lblPipes.AutoSize = true;
      this.lblPipes.Location = new System.Drawing.Point(18, 15);
      this.lblPipes.Name = "lblPipes";
      this.lblPipes.Size = new System.Drawing.Size(72, 13);
      this.lblPipes.TabIndex = 0;
      this.lblPipes.Text = "Pipe network:";
      // 
      // lblDEM
      // 
      this.lblDEM.AutoSize = true;
      this.lblDEM.Location = new System.Drawing.Point(18, 138);
      this.lblDEM.Name = "lblDEM";
      this.lblDEM.Size = new System.Drawing.Size(34, 13);
      this.lblDEM.TabIndex = 6;
      this.lblDEM.Text = "DEM:";
      // 
      // lblFlowDirGrid
      // 
      this.lblFlowDirGrid.AutoSize = true;
      this.lblFlowDirGrid.Location = new System.Drawing.Point(10, 22);
      this.lblFlowDirGrid.Name = "lblFlowDirGrid";
      this.lblFlowDirGrid.Size = new System.Drawing.Size(95, 13);
      this.lblFlowDirGrid.TabIndex = 13;
      this.lblFlowDirGrid.Text = "Flow direction grid:";
      // 
      // btnOK
      // 
      this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnOK.Location = new System.Drawing.Point(118, 297);
      this.btnOK.Name = "btnOK";
      this.btnOK.Size = new System.Drawing.Size(88, 23);
      this.btnOK.TabIndex = 15;
      this.btnOK.Text = "OK";
      this.btnOK.UseVisualStyleBackColor = true;
      this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
      // 
      // btnCancel
      // 
      this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.btnCancel.Location = new System.Drawing.Point(217, 297);
      this.btnCancel.Name = "btnCancel";
      this.btnCancel.Size = new System.Drawing.Size(88, 23);
      this.btnCancel.TabIndex = 16;
      this.btnCancel.Text = "Cancel";
      this.btnCancel.UseVisualStyleBackColor = true;
      this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
      // 
      // cbxDEM
      // 
      this.cbxDEM.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbxDEM.DisplayMember = "Display";
      this.cbxDEM.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxDEM.FormattingEnabled = true;
      this.cbxDEM.Location = new System.Drawing.Point(118, 135);
      this.cbxDEM.Name = "cbxDEM";
      this.cbxDEM.Size = new System.Drawing.Size(187, 21);
      this.cbxDEM.TabIndex = 7;
      this.cbxDEM.ValueMember = "Value";
      this.cbxDEM.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
      // 
      // cbxFlowDirGrid
      // 
      this.cbxFlowDirGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbxFlowDirGrid.DisplayMember = "Display";
      this.cbxFlowDirGrid.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxFlowDirGrid.FormattingEnabled = true;
      this.cbxFlowDirGrid.Location = new System.Drawing.Point(111, 19);
      this.cbxFlowDirGrid.Name = "cbxFlowDirGrid";
      this.cbxFlowDirGrid.Size = new System.Drawing.Size(187, 21);
      this.cbxFlowDirGrid.TabIndex = 14;
      this.cbxFlowDirGrid.ValueMember = "Value";
      this.cbxFlowDirGrid.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
      // 
      // lblInlets
      // 
      this.lblInlets.AutoSize = true;
      this.lblInlets.Location = new System.Drawing.Point(18, 42);
      this.lblInlets.Name = "lblInlets";
      this.lblInlets.Size = new System.Drawing.Size(77, 13);
      this.lblInlets.TabIndex = 2;
      this.lblInlets.Text = "Network inlets:";
      // 
      // chkIncludeUp
      // 
      this.chkIncludeUp.AutoSize = true;
      this.chkIncludeUp.Location = new System.Drawing.Point(118, 66);
      this.chkIncludeUp.Name = "chkIncludeUp";
      this.chkIncludeUp.Size = new System.Drawing.Size(159, 17);
      this.chkIncludeUp.TabIndex = 4;
      this.chkIncludeUp.Text = "Include upstream pipe ends.";
      this.chkIncludeUp.UseVisualStyleBackColor = true;
      // 
      // cbxNetwork
      // 
      this.cbxNetwork.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbxNetwork.DisplayMember = "Display";
      this.cbxNetwork.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxNetwork.FormattingEnabled = true;
      this.cbxNetwork.Location = new System.Drawing.Point(118, 12);
      this.cbxNetwork.Name = "cbxNetwork";
      this.cbxNetwork.Size = new System.Drawing.Size(187, 21);
      this.cbxNetwork.TabIndex = 1;
      this.cbxNetwork.ValueMember = "Value";
      this.cbxNetwork.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
      // 
      // cbxInlets
      // 
      this.cbxInlets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbxInlets.DisplayMember = "Display";
      this.cbxInlets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxInlets.FormattingEnabled = true;
      this.cbxInlets.Location = new System.Drawing.Point(118, 39);
      this.cbxInlets.Name = "cbxInlets";
      this.cbxInlets.Size = new System.Drawing.Size(187, 21);
      this.cbxInlets.TabIndex = 3;
      this.cbxInlets.ValueMember = "Value";
      this.cbxInlets.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
      // 
      // chkExcludeDown
      // 
      this.chkExcludeDown.AutoSize = true;
      this.chkExcludeDown.Checked = true;
      this.chkExcludeDown.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkExcludeDown.Location = new System.Drawing.Point(118, 112);
      this.chkExcludeDown.Name = "chkExcludeDown";
      this.chkExcludeDown.Size = new System.Drawing.Size(176, 17);
      this.chkExcludeDown.TabIndex = 5;
      this.chkExcludeDown.Text = "Exclude downstream pipe ends.";
      this.chkExcludeDown.UseVisualStyleBackColor = true;
      // 
      // btnPreprocess
      // 
      this.btnPreprocess.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.btnPreprocess.Location = new System.Drawing.Point(118, 185);
      this.btnPreprocess.Name = "btnPreprocess";
      this.btnPreprocess.Size = new System.Drawing.Size(187, 23);
      this.btnPreprocess.TabIndex = 17;
      this.btnPreprocess.Text = "Preprocess Data";
      this.btnPreprocess.UseVisualStyleBackColor = true;
      this.btnPreprocess.Click += new System.EventHandler(this.btnPreprocess_Click);
      // 
      // cbxCatchments
      // 
      this.cbxCatchments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbxCatchments.DisplayMember = "Display";
      this.cbxCatchments.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxCatchments.FormattingEnabled = true;
      this.cbxCatchments.Location = new System.Drawing.Point(111, 46);
      this.cbxCatchments.Name = "cbxCatchments";
      this.cbxCatchments.Size = new System.Drawing.Size(187, 21);
      this.cbxCatchments.TabIndex = 19;
      this.cbxCatchments.ValueMember = "Value";
      this.cbxCatchments.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
      // 
      // lblInletCatchments
      // 
      this.lblInletCatchments.AutoSize = true;
      this.lblInletCatchments.Location = new System.Drawing.Point(10, 49);
      this.lblInletCatchments.Name = "lblInletCatchments";
      this.lblInletCatchments.Size = new System.Drawing.Size(88, 13);
      this.lblInletCatchments.TabIndex = 18;
      this.lblInletCatchments.Text = "Inlet catchments:";
      // 
      // grpAdvanced
      // 
      this.grpAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.grpAdvanced.BackColor = System.Drawing.SystemColors.ControlLight;
      this.grpAdvanced.Controls.Add(this.cbxCatchments);
      this.grpAdvanced.Controls.Add(this.lblInletCatchments);
      this.grpAdvanced.Controls.Add(this.cbxFlowDirGrid);
      this.grpAdvanced.Controls.Add(this.lblFlowDirGrid);
      this.grpAdvanced.Location = new System.Drawing.Point(6, 214);
      this.grpAdvanced.Name = "grpAdvanced";
      this.grpAdvanced.Size = new System.Drawing.Size(304, 77);
      this.grpAdvanced.TabIndex = 20;
      this.grpAdvanced.TabStop = false;
      this.grpAdvanced.Text = "Results";
      // 
      // chkExcludeDisabled
      // 
      this.chkExcludeDisabled.AutoSize = true;
      this.chkExcludeDisabled.Location = new System.Drawing.Point(118, 89);
      this.chkExcludeDisabled.Name = "chkExcludeDisabled";
      this.chkExcludeDisabled.Size = new System.Drawing.Size(141, 17);
      this.chkExcludeDisabled.TabIndex = 20;
      this.chkExcludeDisabled.Text = "Exclude disabled nodes.";
      this.chkExcludeDisabled.UseVisualStyleBackColor = true;
      // 
      // chkSmooth
      // 
      this.chkSmooth.AutoSize = true;
      this.chkSmooth.Checked = true;
      this.chkSmooth.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkSmooth.Location = new System.Drawing.Point(118, 162);
      this.chkSmooth.Name = "chkSmooth";
      this.chkSmooth.Size = new System.Drawing.Size(173, 17);
      this.chkSmooth.TabIndex = 21;
      this.chkSmooth.Text = "Smooth catchment boundaries.";
      this.chkSmooth.UseVisualStyleBackColor = true;
      // 
      // SetupForm
      // 
      this.AcceptButton = this.btnOK;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.btnCancel;
      this.ClientSize = new System.Drawing.Size(325, 330);
      this.Controls.Add(this.chkExcludeDisabled);
      this.Controls.Add(this.chkSmooth);
      this.Controls.Add(this.grpAdvanced);
      this.Controls.Add(this.btnPreprocess);
      this.Controls.Add(this.chkExcludeDown);
      this.Controls.Add(this.cbxNetwork);
      this.Controls.Add(this.chkIncludeUp);
      this.Controls.Add(this.cbxDEM);
      this.Controls.Add(this.cbxInlets);
      this.Controls.Add(this.btnCancel);
      this.Controls.Add(this.btnOK);
      this.Controls.Add(this.lblDEM);
      this.Controls.Add(this.lblInlets);
      this.Controls.Add(this.lblPipes);
      this.HelpButton = true;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(333, 351);
      this.Name = "SetupForm";
      this.Text = "Delineation Setup";
      this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.SetupForm_HelpButtonClicked);
      this.Load += new System.EventHandler(this.SetupForm_Load);
      this.grpAdvanced.ResumeLayout(false);
      this.grpAdvanced.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label lblPipes;
        private System.Windows.Forms.Label lblDEM;
		private System.Windows.Forms.Label lblFlowDirGrid;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox cbxDEM;
        private System.Windows.Forms.ComboBox cbxFlowDirGrid;
		private System.Windows.Forms.Label lblInlets;
		private System.Windows.Forms.CheckBox chkIncludeUp;
        private System.Windows.Forms.ComboBox cbxNetwork;
        private System.Windows.Forms.ComboBox cbxInlets;
        private System.Windows.Forms.CheckBox chkExcludeDown;
        private System.Windows.Forms.Button btnPreprocess;
        private System.Windows.Forms.ComboBox cbxCatchments;
        private System.Windows.Forms.Label lblInletCatchments;
        private System.Windows.Forms.GroupBox grpAdvanced;
        private System.Windows.Forms.CheckBox chkSmooth;
        private System.Windows.Forms.CheckBox chkExcludeDisabled;
	}
}