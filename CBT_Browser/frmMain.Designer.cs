namespace CBT_Browser;

partial class frmMain
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
        cbtWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
        labelTopLeft = new Label();
        labelBottomLeft = new Label();
        labelTopRight = new Label();
        labelBottomRight = new Label();
        ((System.ComponentModel.ISupportInitialize)cbtWebView).BeginInit();
        SuspendLayout();
        // 
        // cbtWebView
        // 
        cbtWebView.AllowExternalDrop = false;
        cbtWebView.BackgroundImageLayout = ImageLayout.Stretch;
        cbtWebView.CreationProperties = null;
        cbtWebView.DefaultBackgroundColor = Color.Silver;
        cbtWebView.Dock = DockStyle.Fill;
        cbtWebView.Location = new Point(0, 0);
        cbtWebView.Name = "cbtWebView";
        cbtWebView.Size = new Size(1240, 603);
        cbtWebView.Source = new Uri("https://localhost:7230", UriKind.Absolute);
        cbtWebView.TabIndex = 0;
        cbtWebView.ZoomFactor = 1D;
        cbtWebView.Click += cbtWebView_Click;
        // 
        // labelTopLeft
        // 
        labelTopLeft.BorderStyle = BorderStyle.FixedSingle;
        labelTopLeft.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
        labelTopLeft.Location = new Point(0, -1);
        labelTopLeft.Name = "labelTopLeft";
        labelTopLeft.Size = new Size(65, 65);
        labelTopLeft.TabIndex = 1;
        labelTopLeft.Text = "1";
        labelTopLeft.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // labelBottomLeft
        // 
        labelBottomLeft.BorderStyle = BorderStyle.FixedSingle;
        labelBottomLeft.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        labelBottomLeft.Location = new Point(0, 540);
        labelBottomLeft.Name = "labelBottomLeft";
        labelBottomLeft.Size = new Size(65, 65);
        labelBottomLeft.TabIndex = 2;
        labelBottomLeft.Text = "3";
        labelBottomLeft.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // labelTopRight
        // 
        labelTopRight.BorderStyle = BorderStyle.FixedSingle;
        labelTopRight.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        labelTopRight.Location = new Point(1175, 0);
        labelTopRight.Name = "labelTopRight";
        labelTopRight.Size = new Size(65, 65);
        labelTopRight.TabIndex = 3;
        labelTopRight.Text = "2";
        labelTopRight.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // labelBottomRight
        // 
        labelBottomRight.BorderStyle = BorderStyle.FixedSingle;
        labelBottomRight.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        labelBottomRight.Location = new Point(1175, 538);
        labelBottomRight.Name = "labelBottomRight";
        labelBottomRight.Size = new Size(65, 65);
        labelBottomRight.TabIndex = 4;
        labelBottomRight.Text = "4";
        labelBottomRight.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // frmMain
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackgroundImageLayout = ImageLayout.Stretch;
        ClientSize = new Size(1240, 603);
        ControlBox = false;
        Controls.Add(labelBottomRight);
        Controls.Add(labelTopRight);
        Controls.Add(labelBottomLeft);
        Controls.Add(labelTopLeft);
        Controls.Add(cbtWebView);
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "frmMain";
        ShowIcon = false;
        StartPosition = FormStartPosition.CenterScreen;
        TopMost = true;
        WindowState = FormWindowState.Maximized;
        Load += frmMain_Load;
        ((System.ComponentModel.ISupportInitialize)cbtWebView).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Microsoft.Web.WebView2.WinForms.WebView2 cbtWebView;
    private Label labelTopLeft;
    private Label labelBottomLeft;
    private Label labelTopRight;
    private Label labelBottomRight;
}
