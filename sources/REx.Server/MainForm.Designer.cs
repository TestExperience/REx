namespace REx.Server
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.serviceIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.serviceIconMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.rtbLogging = new System.Windows.Forms.RichTextBox();
            this.showLoggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shutdownServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serviceIconMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // serviceIcon
            // 
            this.serviceIcon.ContextMenuStrip = this.serviceIconMenuStrip;
            this.serviceIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("serviceIcon.Icon")));
            this.serviceIcon.Text = "REx Service";
            this.serviceIcon.Visible = true;
            // 
            // serviceIconMenuStrip
            // 
            this.serviceIconMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showLoggingToolStripMenuItem,
            this.shutdownServerToolStripMenuItem});
            this.serviceIconMenuStrip.Name = "serviceIconMenuStrip";
            this.serviceIconMenuStrip.Size = new System.Drawing.Size(164, 70);
            // 
            // rtbLogging
            // 
            this.rtbLogging.BackColor = System.Drawing.Color.Black;
            this.rtbLogging.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbLogging.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbLogging.ForeColor = System.Drawing.Color.White;
            this.rtbLogging.Location = new System.Drawing.Point(0, 0);
            this.rtbLogging.Name = "rtbLogging";
            this.rtbLogging.ReadOnly = true;
            this.rtbLogging.Size = new System.Drawing.Size(647, 394);
            this.rtbLogging.TabIndex = 1;
            this.rtbLogging.Text = "";
            // 
            // showLoggingToolStripMenuItem
            // 
            this.showLoggingToolStripMenuItem.Name = "showLoggingToolStripMenuItem";
            this.showLoggingToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.showLoggingToolStripMenuItem.Text = "Show Logging";
            this.showLoggingToolStripMenuItem.Click += new System.EventHandler(this.showLoggingToolStripMenuItem_Click);
            // 
            // shutdownServerToolStripMenuItem
            // 
            this.shutdownServerToolStripMenuItem.Name = "shutdownServerToolStripMenuItem";
            this.shutdownServerToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.shutdownServerToolStripMenuItem.Text = "Shutdown Server";
            this.shutdownServerToolStripMenuItem.Click += new System.EventHandler(this.shutdownServerToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(647, 394);
            this.Controls.Add(this.rtbLogging);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.Text = "REx Server";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.serviceIconMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon serviceIcon;
        private System.Windows.Forms.ContextMenuStrip serviceIconMenuStrip;
        private System.Windows.Forms.RichTextBox rtbLogging;
        private System.Windows.Forms.ToolStripMenuItem showLoggingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shutdownServerToolStripMenuItem;
    }
}