// DHI Urban Catchment Delineation
// Copyright (c) 2007, 2010, 2012-2017 DHI Water & Environment, Inc.
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
	partial class UrbanDelineationForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UrbanDelineationForm));
      this.chkStopAtDisabled = new System.Windows.Forms.CheckBox();
      this.lblInfo = new System.Windows.Forms.Label();
      this.chkExtendOverland = new System.Windows.Forms.CheckBox();
      this.btnDelineate = new System.Windows.Forms.Button();
      this.lblOutletId = new System.Windows.Forms.Label();
      this.lblOutletLocations = new System.Windows.Forms.Label();
      this.cbxOutletSource = new System.Windows.Forms.ComboBox();
      this.cbxOutletField = new System.Windows.Forms.ComboBox();
      this.chkSnapToPourPoint = new System.Windows.Forms.CheckBox();
      this.tbxSnapDistance = new System.Windows.Forms.TextBox();
      this.lblSnapUnit = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // chkStopAtDisabled
      // 
      this.chkStopAtDisabled.AutoSize = true;
      this.chkStopAtDisabled.Checked = true;
      this.chkStopAtDisabled.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkStopAtDisabled.Location = new System.Drawing.Point(15, 117);
      this.chkStopAtDisabled.Name = "chkStopAtDisabled";
      this.chkStopAtDisabled.Size = new System.Drawing.Size(222, 17);
      this.chkStopAtDisabled.TabIndex = 3;
      this.chkStopAtDisabled.Text = "Stop tracing at disabled network features.";
      this.chkStopAtDisabled.UseVisualStyleBackColor = true;
      this.chkStopAtDisabled.CheckedChanged += new System.EventHandler(this.chkStopAtDisabled_CheckedChanged);
      // 
      // lblInfo
      // 
      this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.lblInfo.Location = new System.Drawing.Point(12, 9);
      this.lblInfo.Name = "lblInfo";
      this.lblInfo.Size = new System.Drawing.Size(298, 29);
      this.lblInfo.TabIndex = 2;
      this.lblInfo.Text = "Select nodes of interest on the drainage network, then click on the Delineate but" +
    "ton.";
      // 
      // chkExtendOverland
      // 
      this.chkExtendOverland.AutoSize = true;
      this.chkExtendOverland.Checked = true;
      this.chkExtendOverland.CheckState = System.Windows.Forms.CheckState.Checked;
      this.chkExtendOverland.Location = new System.Drawing.Point(15, 94);
      this.chkExtendOverland.Name = "chkExtendOverland";
      this.chkExtendOverland.Size = new System.Drawing.Size(289, 17);
      this.chkExtendOverland.TabIndex = 1;
      this.chkExtendOverland.Text = "Extend area through upstream outlets and subnetworks.";
      this.chkExtendOverland.UseVisualStyleBackColor = true;
      this.chkExtendOverland.CheckedChanged += new System.EventHandler(this.chkExtendOverland_CheckedChanged);
      // 
      // btnDelineate
      // 
      this.btnDelineate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.btnDelineate.BackColor = System.Drawing.SystemColors.ActiveCaption;
      this.btnDelineate.Location = new System.Drawing.Point(98, 171);
      this.btnDelineate.Name = "btnDelineate";
      this.btnDelineate.Size = new System.Drawing.Size(127, 23);
      this.btnDelineate.TabIndex = 0;
      this.btnDelineate.Text = "Delineate";
      this.btnDelineate.UseVisualStyleBackColor = false;
      this.btnDelineate.Click += new System.EventHandler(this.btnDelineate_Click);
      // 
      // lblOutletId
      // 
      this.lblOutletId.AutoSize = true;
      this.lblOutletId.Location = new System.Drawing.Point(12, 71);
      this.lblOutletId.Name = "lblOutletId";
      this.lblOutletId.Size = new System.Drawing.Size(92, 13);
      this.lblOutletId.TabIndex = 4;
      this.lblOutletId.Text = "Outlet Label Field:";
      // 
      // lblOutletLocations
      // 
      this.lblOutletLocations.AutoSize = true;
      this.lblOutletLocations.Location = new System.Drawing.Point(12, 44);
      this.lblOutletLocations.Name = "lblOutletLocations";
      this.lblOutletLocations.Size = new System.Drawing.Size(87, 13);
      this.lblOutletLocations.TabIndex = 6;
      this.lblOutletLocations.Text = "Outlet Locations:";
      // 
      // cbxOutletSource
      // 
      this.cbxOutletSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbxOutletSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxOutletSource.FormattingEnabled = true;
      this.cbxOutletSource.Location = new System.Drawing.Point(110, 41);
      this.cbxOutletSource.Name = "cbxOutletSource";
      this.cbxOutletSource.Size = new System.Drawing.Size(200, 21);
      this.cbxOutletSource.TabIndex = 7;
      this.cbxOutletSource.SelectedIndexChanged += new System.EventHandler(this.cbxOutletSource_SelectedIndexChanged);
      // 
      // cbxOutletField
      // 
      this.cbxOutletField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.cbxOutletField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbxOutletField.FormattingEnabled = true;
      this.cbxOutletField.Location = new System.Drawing.Point(110, 68);
      this.cbxOutletField.Name = "cbxOutletField";
      this.cbxOutletField.Size = new System.Drawing.Size(200, 21);
      this.cbxOutletField.TabIndex = 8;
      this.cbxOutletField.SelectedIndexChanged += new System.EventHandler(this.cbxOutletField_SelectedIndexChanged);
      // 
      // chkSnapToPourPoint
      // 
      this.chkSnapToPourPoint.AutoSize = true;
      this.chkSnapToPourPoint.Location = new System.Drawing.Point(15, 140);
      this.chkSnapToPourPoint.Name = "chkSnapToPourPoint";
      this.chkSnapToPourPoint.Size = new System.Drawing.Size(131, 17);
      this.chkSnapToPourPoint.TabIndex = 9;
      this.chkSnapToPourPoint.Text = "Snap points to stream.";
      this.chkSnapToPourPoint.UseVisualStyleBackColor = true;
      this.chkSnapToPourPoint.CheckedChanged += new System.EventHandler(this.chkSnapToPourPoint_CheckedChanged);
      // 
      // tbxSnapDistance
      // 
      this.tbxSnapDistance.Location = new System.Drawing.Point(163, 138);
      this.tbxSnapDistance.Name = "tbxSnapDistance";
      this.tbxSnapDistance.Size = new System.Drawing.Size(74, 20);
      this.tbxSnapDistance.TabIndex = 10;
      this.tbxSnapDistance.TextChanged += new System.EventHandler(this.tbxSnapDistance_TextChanged);
      // 
      // lblSnapUnit
      // 
      this.lblSnapUnit.AutoSize = true;
      this.lblSnapUnit.Location = new System.Drawing.Point(243, 141);
      this.lblSnapUnit.Name = "lblSnapUnit";
      this.lblSnapUnit.Size = new System.Drawing.Size(25, 13);
      this.lblSnapUnit.TabIndex = 11;
      this.lblSnapUnit.Text = "feet";
      // 
      // UrbanDelineationForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(322, 206);
      this.Controls.Add(this.lblSnapUnit);
      this.Controls.Add(this.tbxSnapDistance);
      this.Controls.Add(this.chkSnapToPourPoint);
      this.Controls.Add(this.cbxOutletField);
      this.Controls.Add(this.cbxOutletSource);
      this.Controls.Add(this.lblOutletLocations);
      this.Controls.Add(this.lblOutletId);
      this.Controls.Add(this.chkStopAtDisabled);
      this.Controls.Add(this.lblInfo);
      this.Controls.Add(this.chkExtendOverland);
      this.Controls.Add(this.btnDelineate);
      this.HelpButton = true;
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "UrbanDelineationForm";
      this.Text = "Urban Catchment Delineation";
      this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.UrbanDelineationForm_HelpButtonClicked);
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Button btnDelineate;
		private System.Windows.Forms.CheckBox chkExtendOverland;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.CheckBox chkStopAtDisabled;
        private System.Windows.Forms.Label lblOutletId;
        private System.Windows.Forms.Label lblOutletLocations;
        private System.Windows.Forms.ComboBox cbxOutletSource;
        private System.Windows.Forms.ComboBox cbxOutletField;
        private System.Windows.Forms.CheckBox chkSnapToPourPoint;
        private System.Windows.Forms.TextBox tbxSnapDistance;
        private System.Windows.Forms.Label lblSnapUnit;
	}
}
