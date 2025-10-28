namespace Showcase.PUOneToOne
{
    partial class PUOneToOne
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cbPorts = new ComboBox();
            btnConnect = new Button();
            btnExport = new Button();
            dgv = new DataGridView();
            txtLog = new TextBox();
            btnRefresh = new Button();
            fwversionLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)dgv).BeginInit();
            SuspendLayout();
            // 
            // cbPorts
            // 
            cbPorts.FormattingEnabled = true;
            cbPorts.Location = new Point(363, 24);
            cbPorts.Name = "cbPorts";
            cbPorts.Size = new Size(387, 23);
            cbPorts.TabIndex = 0;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(756, 24);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(90, 23);
            btnConnect.TabIndex = 1;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            // 
            // btnExport
            // 
            btnExport.Location = new Point(852, 24);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(75, 23);
            btnExport.TabIndex = 2;
            btnExport.Text = "Export";
            btnExport.UseVisualStyleBackColor = true;
            // 
            // dgv
            // 
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.Location = new Point(363, 68);
            dgv.Name = "dgv";
            dgv.Size = new Size(483, 256);
            dgv.TabIndex = 3;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(12, 68);
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(345, 23);
            txtLog.TabIndex = 4;
            // 
            // btnRefresh
            // 
            btnRefresh.Location = new Point(852, 67);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(75, 23);
            btnRefresh.TabIndex = 5;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = true;
            // 
            // fwversionLabel
            // 
            fwversionLabel.AutoSize = true;
            fwversionLabel.Location = new Point(12, 28);
            fwversionLabel.Name = "fwversionLabel";
            fwversionLabel.Size = new Size(73, 15);
            fwversionLabel.TabIndex = 6;
            fwversionLabel.Text = "FW Version:";
            // 
            // PUOneToOne
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(939, 339);
            Controls.Add(fwversionLabel);
            Controls.Add(btnRefresh);
            Controls.Add(txtLog);
            Controls.Add(dgv);
            Controls.Add(btnExport);
            Controls.Add(btnConnect);
            Controls.Add(cbPorts);
            Name = "PUOneToOne";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)dgv).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cbPorts;
        private Button btnConnect;
        private Button btnExport;
        private DataGridView dgv;
        private TextBox txtLog;
        private Button btnRefresh;
        private Label fwversionLabel;
    }
}
