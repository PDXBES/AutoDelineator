// DHI Urban Catchment Delineation
// Copyright (c) 2007, 2010, 2012 DHI Water & Environment, Inc.
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
    partial class ZingerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZingerForm));
            this.lblFromLayer = new System.Windows.Forms.Label();
            this.lblFromField = new System.Windows.Forms.Label();
            this.cbxFromLayer = new System.Windows.Forms.ComboBox();
            this.cbxFromField = new System.Windows.Forms.ComboBox();
            this.cbxToField = new System.Windows.Forms.ComboBox();
            this.cbxToLayer = new System.Windows.Forms.ComboBox();
            this.lblToField = new System.Windows.Forms.Label();
            this.lblToLayer = new System.Windows.Forms.Label();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblFromLayer
            // 
            this.lblFromLayer.AutoSize = true;
            this.lblFromLayer.Location = new System.Drawing.Point(12, 60);
            this.lblFromLayer.Name = "lblFromLayer";
            this.lblFromLayer.Size = new System.Drawing.Size(58, 13);
            this.lblFromLayer.TabIndex = 0;
            this.lblFromLayer.Text = "From layer:";
            // 
            // lblFromField
            // 
            this.lblFromField.AutoSize = true;
            this.lblFromField.Location = new System.Drawing.Point(12, 87);
            this.lblFromField.Name = "lblFromField";
            this.lblFromField.Size = new System.Drawing.Size(55, 13);
            this.lblFromField.TabIndex = 1;
            this.lblFromField.Text = "From field:";
            // 
            // cbxFromLayer
            // 
            this.cbxFromLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxFromLayer.FormattingEnabled = true;
            this.cbxFromLayer.Location = new System.Drawing.Point(76, 57);
            this.cbxFromLayer.Name = "cbxFromLayer";
            this.cbxFromLayer.Size = new System.Drawing.Size(204, 21);
            this.cbxFromLayer.TabIndex = 2;
            this.cbxFromLayer.SelectedIndexChanged += new System.EventHandler(this.cbxFromLayer_SelectedIndexChanged);
            // 
            // cbxFromField
            // 
            this.cbxFromField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxFromField.FormattingEnabled = true;
            this.cbxFromField.Location = new System.Drawing.Point(76, 84);
            this.cbxFromField.Name = "cbxFromField";
            this.cbxFromField.Size = new System.Drawing.Size(204, 21);
            this.cbxFromField.TabIndex = 3;
            // 
            // cbxToField
            // 
            this.cbxToField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxToField.FormattingEnabled = true;
            this.cbxToField.Location = new System.Drawing.Point(76, 153);
            this.cbxToField.Name = "cbxToField";
            this.cbxToField.Size = new System.Drawing.Size(204, 21);
            this.cbxToField.TabIndex = 7;
            // 
            // cbxToLayer
            // 
            this.cbxToLayer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxToLayer.FormattingEnabled = true;
            this.cbxToLayer.Location = new System.Drawing.Point(76, 126);
            this.cbxToLayer.Name = "cbxToLayer";
            this.cbxToLayer.Size = new System.Drawing.Size(204, 21);
            this.cbxToLayer.TabIndex = 6;
            this.cbxToLayer.SelectedIndexChanged += new System.EventHandler(this.cbxToLayer_SelectedIndexChanged);
            // 
            // lblToField
            // 
            this.lblToField.AutoSize = true;
            this.lblToField.Location = new System.Drawing.Point(12, 156);
            this.lblToField.Name = "lblToField";
            this.lblToField.Size = new System.Drawing.Size(45, 13);
            this.lblToField.TabIndex = 5;
            this.lblToField.Text = "To field:";
            // 
            // lblToLayer
            // 
            this.lblToLayer.AutoSize = true;
            this.lblToLayer.Location = new System.Drawing.Point(12, 129);
            this.lblToLayer.Name = "lblToLayer";
            this.lblToLayer.Size = new System.Drawing.Size(48, 13);
            this.lblToLayer.TabIndex = 4;
            this.lblToLayer.Text = "To layer:";
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(76, 193);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(99, 23);
            this.btnAdd.TabIndex = 8;
            this.btnAdd.Text = "Add Zingers";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(181, 193);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(99, 23);
            this.btnRemove.TabIndex = 9;
            this.btnRemove.Text = "Remove Zingers";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // lblDescription
            // 
            this.lblDescription.Location = new System.Drawing.Point(12, 9);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(268, 43);
            this.lblDescription.TabIndex = 10;
            this.lblDescription.Text = "Zingers are added as graphics on top of the \"from\" layer. To remove them, select " +
                "the \"from\" layer they were added to.";
            // 
            // ZingerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 230);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.cbxToField);
            this.Controls.Add(this.cbxToLayer);
            this.Controls.Add(this.lblToField);
            this.Controls.Add(this.lblToLayer);
            this.Controls.Add(this.cbxFromField);
            this.Controls.Add(this.cbxFromLayer);
            this.Controls.Add(this.lblFromField);
            this.Controls.Add(this.lblFromLayer);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ZingerForm";
            this.Text = "Zingers";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.ZingerForm_HelpButtonClicked);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFromLayer;
        private System.Windows.Forms.Label lblFromField;
        private System.Windows.Forms.ComboBox cbxFromLayer;
        private System.Windows.Forms.ComboBox cbxFromField;
        private System.Windows.Forms.ComboBox cbxToField;
        private System.Windows.Forms.ComboBox cbxToLayer;
        private System.Windows.Forms.Label lblToField;
        private System.Windows.Forms.Label lblToLayer;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label lblDescription;
    }
}