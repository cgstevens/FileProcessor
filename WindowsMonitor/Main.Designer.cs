namespace WinForms
{
    partial class Main
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
            this.DownClusterButton = new System.Windows.Forms.Button();
            this.LeaveClusterButton = new System.Windows.Forms.Button();
            this.clusterListView = new System.Windows.Forms.ListView();
            this.loggerBox = new System.Windows.Forms.ListBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.unreachableListView = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.seenByListView = new System.Windows.Forms.ListView();
            this.label3 = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // DownClusterButton
            // 
            this.DownClusterButton.Location = new System.Drawing.Point(165, 1);
            this.DownClusterButton.Name = "DownClusterButton";
            this.DownClusterButton.Size = new System.Drawing.Size(75, 23);
            this.DownClusterButton.TabIndex = 20;
            this.DownClusterButton.Text = "Down";
            this.DownClusterButton.UseVisualStyleBackColor = true;
            this.DownClusterButton.Click += new System.EventHandler(this.DownClusterButton_Click);
            // 
            // LeaveClusterButton
            // 
            this.LeaveClusterButton.Location = new System.Drawing.Point(246, 1);
            this.LeaveClusterButton.Name = "LeaveClusterButton";
            this.LeaveClusterButton.Size = new System.Drawing.Size(75, 23);
            this.LeaveClusterButton.TabIndex = 19;
            this.LeaveClusterButton.Text = "Leave Cluster";
            this.LeaveClusterButton.UseVisualStyleBackColor = true;
            this.LeaveClusterButton.Click += new System.EventHandler(this.LeaveClusterButton_Click);
            // 
            // clusterListView
            // 
            this.clusterListView.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.clusterListView.Location = new System.Drawing.Point(0, 26);
            this.clusterListView.Name = "clusterListView";
            this.clusterListView.Size = new System.Drawing.Size(807, 110);
            this.clusterListView.TabIndex = 18;
            this.clusterListView.UseCompatibleStateImageBehavior = false;
            // 
            // loggerBox
            // 
            this.loggerBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.loggerBox.FormattingEnabled = true;
            this.loggerBox.Location = new System.Drawing.Point(0, 19);
            this.loggerBox.Name = "loggerBox";
            this.loggerBox.Size = new System.Drawing.Size(807, 160);
            this.loggerBox.TabIndex = 17;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.SlateGray;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.DownClusterButton);
            this.panel2.Controls.Add(this.LeaveClusterButton);
            this.panel2.Controls.Add(this.clusterListView);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(811, 140);
            this.panel2.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(0, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(129, 17);
            this.label2.TabIndex = 23;
            this.label2.Text = "Cluster Members";
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Controls.Add(this.unreachableListView);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 140);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(811, 121);
            this.panel3.TabIndex = 25;
            // 
            // unreachableListView
            // 
            this.unreachableListView.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.unreachableListView.Location = new System.Drawing.Point(0, 23);
            this.unreachableListView.Name = "unreachableListView";
            this.unreachableListView.Size = new System.Drawing.Size(807, 94);
            this.unreachableListView.TabIndex = 25;
            this.unreachableListView.UseCompatibleStateImageBehavior = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 17);
            this.label1.TabIndex = 24;
            this.label1.Text = "Unreachable Members";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel4.Controls.Add(this.seenByListView);
            this.panel4.Controls.Add(this.label3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 261);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(811, 136);
            this.panel4.TabIndex = 26;
            // 
            // seenByListView
            // 
            this.seenByListView.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.seenByListView.Location = new System.Drawing.Point(0, 22);
            this.seenByListView.Name = "seenByListView";
            this.seenByListView.Size = new System.Drawing.Size(807, 110);
            this.seenByListView.TabIndex = 25;
            this.seenByListView.UseCompatibleStateImageBehavior = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1, 1);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(133, 17);
            this.label3.TabIndex = 24;
            this.label3.Text = "SeenBy Members";
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel5.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel5.Controls.Add(this.label4);
            this.panel5.Controls.Add(this.loggerBox);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel5.Location = new System.Drawing.Point(0, 397);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(811, 183);
            this.panel5.TabIndex = 27;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(0, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 17);
            this.label4.TabIndex = 24;
            this.label4.Text = "Logger";
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(811, 578);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Name = "Main";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Main_Load);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button DownClusterButton;
        private System.Windows.Forms.Button LeaveClusterButton;
        private System.Windows.Forms.ListView clusterListView;
        private System.Windows.Forms.ListBox loggerBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView unreachableListView;
        private System.Windows.Forms.ListView seenByListView;
    }
}

