namespace CBT_Forms
{
    partial class frmAdmin
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
            lblAdmin4 = new Label();
            lblAdmin3 = new Label();
            lblAdmin2 = new Label();
            lblAdmin1 = new Label();
            panel1 = new Panel();
            lblIsThisCorrect = new Label();
            btnRestart = new Button();
            btnReset = new Button();
            ctlNumberPad = new ctlNumberPad();
            lblAdminUnlock = new Label();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblAdmin4
            // 
            lblAdmin4.BackColor = Color.White;
            lblAdmin4.BorderStyle = BorderStyle.FixedSingle;
            lblAdmin4.Font = new Font("Arial", 48F, FontStyle.Bold);
            lblAdmin4.ForeColor = Color.Black;
            lblAdmin4.Location = new Point(498, 52);
            lblAdmin4.Margin = new Padding(4, 0, 4, 0);
            lblAdmin4.Name = "lblAdmin4";
            lblAdmin4.Padding = new Padding(12, 0, 0, 0);
            lblAdmin4.Size = new Size(76, 91);
            lblAdmin4.TabIndex = 26;
            lblAdmin4.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblAdmin3
            // 
            lblAdmin3.BackColor = Color.White;
            lblAdmin3.BorderStyle = BorderStyle.FixedSingle;
            lblAdmin3.Font = new Font("Arial", 48F, FontStyle.Bold);
            lblAdmin3.ForeColor = Color.Black;
            lblAdmin3.Location = new Point(395, 52);
            lblAdmin3.Margin = new Padding(4, 0, 4, 0);
            lblAdmin3.Name = "lblAdmin3";
            lblAdmin3.Padding = new Padding(12, 0, 0, 0);
            lblAdmin3.Size = new Size(76, 91);
            lblAdmin3.TabIndex = 25;
            lblAdmin3.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblAdmin2
            // 
            lblAdmin2.BackColor = Color.White;
            lblAdmin2.BorderStyle = BorderStyle.FixedSingle;
            lblAdmin2.Font = new Font("Arial", 48F, FontStyle.Bold);
            lblAdmin2.ForeColor = Color.Black;
            lblAdmin2.Location = new Point(292, 52);
            lblAdmin2.Margin = new Padding(4, 0, 4, 0);
            lblAdmin2.Name = "lblAdmin2";
            lblAdmin2.Padding = new Padding(12, 0, 0, 0);
            lblAdmin2.Size = new Size(76, 91);
            lblAdmin2.TabIndex = 24;
            lblAdmin2.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblAdmin1
            // 
            lblAdmin1.BackColor = Color.White;
            lblAdmin1.BorderStyle = BorderStyle.FixedSingle;
            lblAdmin1.Font = new Font("Arial", 48F, FontStyle.Bold);
            lblAdmin1.ForeColor = Color.Black;
            lblAdmin1.Location = new Point(190, 52);
            lblAdmin1.Margin = new Padding(4, 0, 4, 0);
            lblAdmin1.Name = "lblAdmin1";
            lblAdmin1.Padding = new Padding(12, 0, 0, 0);
            lblAdmin1.Size = new Size(76, 91);
            lblAdmin1.TabIndex = 23;
            lblAdmin1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Transparent;
            panel1.Controls.Add(lblIsThisCorrect);
            panel1.Controls.Add(btnRestart);
            panel1.Controls.Add(btnReset);
            panel1.Location = new Point(30, 244);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(723, 261);
            panel1.TabIndex = 43;
            // 
            // lblIsThisCorrect
            // 
            lblIsThisCorrect.BackColor = Color.Transparent;
            lblIsThisCorrect.Font = new Font("Arial", 21.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblIsThisCorrect.ForeColor = Color.Yellow;
            lblIsThisCorrect.Location = new Point(5, 14);
            lblIsThisCorrect.Margin = new Padding(4, 0, 4, 0);
            lblIsThisCorrect.Name = "lblIsThisCorrect";
            lblIsThisCorrect.Size = new Size(715, 44);
            lblIsThisCorrect.TabIndex = 45;
            lblIsThisCorrect.Text = "Is this correct ?";
            lblIsThisCorrect.TextAlign = ContentAlignment.MiddleCenter;
            lblIsThisCorrect.Visible = false;
            // 
            // btnRestart
            // 
            btnRestart.AutoSize = true;
            btnRestart.BackColor = Color.Black;
            btnRestart.BackgroundImageLayout = ImageLayout.Stretch;
            btnRestart.FlatAppearance.BorderColor = Color.Gray;
            btnRestart.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnRestart.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnRestart.FlatStyle = FlatStyle.Flat;
            btnRestart.Font = new Font("Arial", 15.25F, FontStyle.Bold);
            btnRestart.ForeColor = Color.White;
            btnRestart.Location = new Point(244, 148);
            btnRestart.Margin = new Padding(4, 3, 4, 3);
            btnRestart.Name = "btnRestart";
            btnRestart.Size = new Size(230, 80);
            btnRestart.TabIndex = 43;
            btnRestart.TabStop = false;
            btnRestart.Text = "RESTART CBT";
            btnRestart.UseVisualStyleBackColor = false;
            btnRestart.Visible = false;
            btnRestart.Click += btnRestart_Click;
            // 
            // btnReset
            // 
            btnReset.BackgroundImageLayout = ImageLayout.Stretch;
            btnReset.FlatAppearance.BorderColor = Color.Gray;
            btnReset.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnReset.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.Font = new Font("Arial", 15.25F, FontStyle.Bold);
            btnReset.ForeColor = Color.White;
            btnReset.Location = new Point(244, 61);
            btnReset.Margin = new Padding(4, 3, 4, 3);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(230, 80);
            btnReset.TabIndex = 44;
            btnReset.TabStop = false;
            btnReset.Text = "RESET SESSION";
            btnReset.UseVisualStyleBackColor = true;
            btnReset.Visible = false;
            btnReset.Click += btnReset_Click;
            // 
            // ctlNumberPad
            // 
            ctlNumberPad.BackColor = Color.Transparent;
            ctlNumberPad.Data = "";
            ctlNumberPad.DialogResult = DialogResult.None;
            ctlNumberPad.Location = new Point(643, 178);
            ctlNumberPad.Margin = new Padding(4, 3, 4, 3);
            ctlNumberPad.Name = "ctlNumberPad";
            ctlNumberPad.Size = new Size(259, 294);
            ctlNumberPad.TabIndex = 0;
            ctlNumberPad.DataChanged += CtlNumberPad_DataChanged;
            ctlNumberPad.DialogResultChanged += CtlNumberPad_DialogResultChanged;
            // 
            // lblAdminUnlock
            // 
            lblAdminUnlock.AutoSize = true;
            lblAdminUnlock.BackColor = Color.Transparent;
            lblAdminUnlock.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblAdminUnlock.ForeColor = Color.Yellow;
            lblAdminUnlock.Location = new Point(278, 9);
            lblAdminUnlock.Name = "lblAdminUnlock";
            lblAdminUnlock.Size = new Size(226, 32);
            lblAdminUnlock.TabIndex = 44;
            lblAdminUnlock.Text = "CBT Admin Unlock";
            // 
            // frmAdmin
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(784, 514);
            ControlBox = false;
            Controls.Add(lblAdminUnlock);
            Controls.Add(lblAdmin4);
            Controls.Add(lblAdmin3);
            Controls.Add(lblAdmin2);
            Controls.Add(lblAdmin1);
            Controls.Add(ctlNumberPad);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmAdmin";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Load += FrmAdmin_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ctlNumberPad ctlNumberPad;
        private System.Windows.Forms.Label lblAdmin4;
        private System.Windows.Forms.Label lblAdmin3;
        private System.Windows.Forms.Label lblAdmin2;
        private System.Windows.Forms.Label lblAdmin1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblIsThisCorrect;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnReset;
        private Label lblAdminUnlock;
    }
}