namespace CBT_Forms
{
    partial class ctlNumberPad
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblNumber = new Label();
            btnEnter = new Button();
            btnBackSpace = new Button();
            btn0 = new Button();
            btn9 = new Button();
            btn8 = new Button();
            btn7 = new Button();
            btn6 = new Button();
            btn5 = new Button();
            btn4 = new Button();
            btn3 = new Button();
            btn2 = new Button();
            btn1 = new Button();
            SuspendLayout();
            // 
            // lblNumber
            // 
            lblNumber.BackColor = Color.White;
            lblNumber.BorderStyle = BorderStyle.Fixed3D;
            lblNumber.Font = new Font("Arial", 27.75F, FontStyle.Bold);
            lblNumber.Location = new Point(15, 297);
            lblNumber.Name = "lblNumber";
            lblNumber.Size = new Size(231, 51);
            lblNumber.TabIndex = 43;
            lblNumber.Text = "7001234";
            lblNumber.TextAlign = ContentAlignment.MiddleCenter;
            lblNumber.Visible = false;
            // 
            // btnEnter
            // 
            btnEnter.BackColor = Color.DimGray;
            btnEnter.BackgroundImageLayout = ImageLayout.Zoom;
            btnEnter.Enabled = false;
            btnEnter.FlatAppearance.BorderColor = Color.Black;
            btnEnter.FlatAppearance.BorderSize = 3;
            btnEnter.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnEnter.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnEnter.FlatStyle = FlatStyle.Popup;
            btnEnter.Font = new Font("Arial", 27.75F, FontStyle.Bold);
            btnEnter.ForeColor = Color.Gray;
            btnEnter.Location = new Point(15, 215);
            btnEnter.Name = "btnEnter";
            btnEnter.Size = new Size(73, 61);
            btnEnter.TabIndex = 45;
            btnEnter.TabStop = false;
            btnEnter.TextAlign = ContentAlignment.TopCenter;
            btnEnter.UseVisualStyleBackColor = false;
            btnEnter.Click += BtnEnter_Click;
            // 
            // btnBackSpace
            // 
            btnBackSpace.BackColor = Color.DimGray;
            btnBackSpace.BackgroundImageLayout = ImageLayout.Zoom;
            btnBackSpace.Enabled = false;
            btnBackSpace.FlatAppearance.BorderColor = Color.Black;
            btnBackSpace.FlatAppearance.BorderSize = 3;
            btnBackSpace.FlatAppearance.MouseDownBackColor = Color.Transparent;
            btnBackSpace.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnBackSpace.FlatStyle = FlatStyle.Popup;
            btnBackSpace.Font = new Font("Arial", 27.75F, FontStyle.Bold);
            btnBackSpace.ForeColor = Color.Gray;
            btnBackSpace.Location = new Point(173, 215);
            btnBackSpace.Name = "btnBackSpace";
            btnBackSpace.Size = new Size(73, 61);
            btnBackSpace.TabIndex = 44;
            btnBackSpace.TabStop = false;
            btnBackSpace.UseVisualStyleBackColor = false;
            btnBackSpace.MouseUp += BtnBackSpace_MouseUp;
            // 
            // btn0
            // 
            btn0.FlatAppearance.BorderColor = Color.Yellow;
            btn0.FlatAppearance.BorderSize = 3;
            btn0.FlatStyle = FlatStyle.Popup;
            btn0.Font = new Font("Arial", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn0.ForeColor = Color.White;
            btn0.Location = new Point(94, 215);
            btn0.Name = "btn0";
            btn0.Size = new Size(73, 61);
            btn0.TabIndex = 42;
            btn0.TabStop = false;
            btn0.Text = "0";
            btn0.UseVisualStyleBackColor = true;
            btn0.MouseUp += Btn0_MouseUp;
            // 
            // btn9
            // 
            btn9.FlatAppearance.BorderColor = Color.Yellow;
            btn9.FlatAppearance.BorderSize = 3;
            btn9.FlatStyle = FlatStyle.Popup;
            btn9.Font = new Font("Arial", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn9.ForeColor = Color.White;
            btn9.Location = new Point(173, 148);
            btn9.Name = "btn9";
            btn9.Size = new Size(73, 61);
            btn9.TabIndex = 41;
            btn9.TabStop = false;
            btn9.Text = "9";
            btn9.UseVisualStyleBackColor = true;
            btn9.MouseUp += Btn9_MouseUp;
            // 
            // btn8
            // 
            btn8.FlatAppearance.BorderColor = Color.Yellow;
            btn8.FlatAppearance.BorderSize = 3;
            btn8.FlatStyle = FlatStyle.Popup;
            btn8.Font = new Font("Arial", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn8.ForeColor = Color.White;
            btn8.Location = new Point(94, 148);
            btn8.Name = "btn8";
            btn8.Size = new Size(73, 61);
            btn8.TabIndex = 40;
            btn8.TabStop = false;
            btn8.Text = "8";
            btn8.UseVisualStyleBackColor = true;
            btn8.MouseUp += Btn8_MouseUp;
            // 
            // btn7
            // 
            btn7.FlatAppearance.BorderColor = Color.Yellow;
            btn7.FlatAppearance.BorderSize = 3;
            btn7.FlatStyle = FlatStyle.Popup;
            btn7.Font = new Font("Arial", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn7.ForeColor = Color.White;
            btn7.Location = new Point(15, 148);
            btn7.Name = "btn7";
            btn7.Size = new Size(73, 61);
            btn7.TabIndex = 39;
            btn7.TabStop = false;
            btn7.Text = "7";
            btn7.UseVisualStyleBackColor = true;
            btn7.MouseUp += Btn7_MouseUp;
            // 
            // btn6
            // 
            btn6.FlatAppearance.BorderColor = Color.Yellow;
            btn6.FlatAppearance.BorderSize = 3;
            btn6.FlatStyle = FlatStyle.Popup;
            btn6.Font = new Font("Arial", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn6.ForeColor = Color.White;
            btn6.Location = new Point(173, 81);
            btn6.Name = "btn6";
            btn6.Size = new Size(73, 61);
            btn6.TabIndex = 38;
            btn6.TabStop = false;
            btn6.Text = "6";
            btn6.UseVisualStyleBackColor = true;
            btn6.MouseUp += Btn6_MouseUp;
            // 
            // btn5
            // 
            btn5.FlatAppearance.BorderColor = Color.Yellow;
            btn5.FlatAppearance.BorderSize = 3;
            btn5.FlatStyle = FlatStyle.Popup;
            btn5.Font = new Font("Arial", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn5.ForeColor = Color.White;
            btn5.Location = new Point(94, 81);
            btn5.Name = "btn5";
            btn5.Size = new Size(73, 61);
            btn5.TabIndex = 37;
            btn5.TabStop = false;
            btn5.Text = "5";
            btn5.UseVisualStyleBackColor = true;
            btn5.MouseUp += Btn5_MouseUp;
            // 
            // btn4
            // 
            btn4.FlatAppearance.BorderColor = Color.Yellow;
            btn4.FlatAppearance.BorderSize = 3;
            btn4.FlatStyle = FlatStyle.Popup;
            btn4.Font = new Font("Arial", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn4.ForeColor = Color.White;
            btn4.Location = new Point(15, 81);
            btn4.Name = "btn4";
            btn4.Size = new Size(73, 61);
            btn4.TabIndex = 36;
            btn4.TabStop = false;
            btn4.Text = "4";
            btn4.UseVisualStyleBackColor = true;
            btn4.MouseUp += Btn4_MouseUp;
            // 
            // btn3
            // 
            btn3.FlatAppearance.BorderColor = Color.Yellow;
            btn3.FlatAppearance.BorderSize = 3;
            btn3.FlatStyle = FlatStyle.Popup;
            btn3.Font = new Font("Arial", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn3.ForeColor = Color.White;
            btn3.Location = new Point(173, 14);
            btn3.Name = "btn3";
            btn3.Size = new Size(73, 61);
            btn3.TabIndex = 35;
            btn3.TabStop = false;
            btn3.Text = "3";
            btn3.UseVisualStyleBackColor = true;
            btn3.MouseUp += Btn3_MouseUp;
            // 
            // btn2
            // 
            btn2.FlatAppearance.BorderColor = Color.Yellow;
            btn2.FlatAppearance.BorderSize = 3;
            btn2.FlatStyle = FlatStyle.Popup;
            btn2.Font = new Font("Arial", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn2.ForeColor = Color.White;
            btn2.Location = new Point(94, 14);
            btn2.Name = "btn2";
            btn2.Size = new Size(73, 61);
            btn2.TabIndex = 34;
            btn2.TabStop = false;
            btn2.Text = "2";
            btn2.UseVisualStyleBackColor = true;
            btn2.MouseUp += Btn2_MouseUp;
            // 
            // btn1
            // 
            btn1.FlatAppearance.BorderColor = Color.Yellow;
            btn1.FlatAppearance.BorderSize = 3;
            btn1.FlatStyle = FlatStyle.Popup;
            btn1.Font = new Font("Arial", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btn1.ForeColor = Color.White;
            btn1.Location = new Point(15, 14);
            btn1.Name = "btn1";
            btn1.Size = new Size(73, 61);
            btn1.TabIndex = 33;
            btn1.TabStop = false;
            btn1.Text = "1";
            btn1.UseVisualStyleBackColor = true;
            btn1.MouseUp += Btn1_MouseUp;
            // 
            // ctlNumberPad
            // 
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.Transparent;
            Controls.Add(btnEnter);
            Controls.Add(btnBackSpace);
            Controls.Add(lblNumber);
            Controls.Add(btn0);
            Controls.Add(btn9);
            Controls.Add(btn8);
            Controls.Add(btn7);
            Controls.Add(btn6);
            Controls.Add(btn5);
            Controls.Add(btn4);
            Controls.Add(btn3);
            Controls.Add(btn2);
            Controls.Add(btn1);
            Name = "ctlNumberPad";
            Size = new Size(262, 288);
            Load += CtlNumberPad_Load;
            VisibleChanged += CtlNumberPad_VisibleChanged;
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btn0;
        private System.Windows.Forms.Button btn9;
        private System.Windows.Forms.Button btn8;
        private System.Windows.Forms.Button btn7;
        private System.Windows.Forms.Button btn6;
        private System.Windows.Forms.Button btn5;
        private System.Windows.Forms.Button btn4;
        private System.Windows.Forms.Button btn3;
        private System.Windows.Forms.Button btn2;
        private System.Windows.Forms.Button btn1;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.Button btnBackSpace;
        private System.Windows.Forms.Button btnEnter;
    }
}
