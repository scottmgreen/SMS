using CBT_Forms;

using Microsoft.Web.WebView2.Core;

namespace CBT_Browser;

public partial class frmMain : Form
{
    private int topLeftClickCount = 0;
    private int topRightClickCount = 0;
    private int bottomLeftClickCount = 0;
    private int bottomRightClickCount = 0;
    private bool isTopLeftClicked = false;
    private bool isTopRightClicked = false;
    private bool isBottomLeftClicked = false;
    private bool isBottomRightClicked = false;
    public frmMain()
    {
        InitializeComponent();
    }
    public void ShowAdminForm()
    {
       
        this.BringToFront();
        using (frmAdmin subForm = new frmAdmin(this))
        {
            subForm.StartPosition = FormStartPosition.CenterParent;
            subForm.TopMost = true;
            subForm.ShowDialog(this); // This will open the subform as a modal dialog
        }

        
        
    }
    private void frmMain_Load(object sender, EventArgs e)
    {
        labelTopLeft.Click += Label_Click;
        labelTopRight.Click += Label_Click;
        labelBottomLeft.Click += Label_Click;
        labelBottomRight.Click += Label_Click;

        // Set positions and anchors (assuming you've already set them as in the previous answer)
        labelTopLeft.Location = new Point(10, 10);
        labelTopLeft.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        labelTopLeft.BringToFront();

        labelTopRight.Location = new Point(this.ClientSize.Width - labelTopRight.Width - 10, 10);
        labelTopRight.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        labelTopRight.BringToFront();

        labelBottomLeft.Location = new Point(10, this.ClientSize.Height - labelBottomLeft.Height - 10);
        labelBottomLeft.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        labelBottomLeft.BringToFront();

        labelBottomRight.Location = new Point(this.ClientSize.Width - labelBottomRight.Width - 10, this.ClientSize.Height - labelBottomRight.Height - 10);
        labelBottomRight.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        labelBottomRight.BringToFront();
        
    }

    private void Label_Click(object sender, EventArgs e)
    {
        // Cast sender to a Label to know which label was clicked
        Label clickedLabel = sender as Label;

        if (clickedLabel != null)
        {
            // Track click counts and set boolean flags for each label
            if (clickedLabel == labelTopLeft)
            {
                topLeftClickCount++;
                isTopLeftClicked = true;
                
            }
            else if (clickedLabel == labelTopRight)
            {
                topRightClickCount++;
                isTopRightClicked = true;
                
            }
            else if (clickedLabel == labelBottomLeft)
            {
                bottomLeftClickCount++;
                isBottomLeftClicked = true;
                
            }
            else if (clickedLabel == labelBottomRight)
            {
                bottomRightClickCount++;
                isBottomRightClicked = true;
                
            }

            // Check if all labels have been clicked at least once
            if (isTopLeftClicked && isTopRightClicked && isBottomLeftClicked && isBottomRightClicked)
            {
                AllLabelsClickedAsync();
            }
        }
    }

    // Remove this line from AllLabelsClickedAsync:
    // using Microsoft.Web.WebView2.Core; // Add this using directive at the top of the file

    // The correct usage is to have the using directive at the top of the file, not inside a method.
    // The following code block shows the corrected AllLabelsClickedAsync method:

    private async Task AllLabelsClickedAsync()
    {
        List<CoreWebView2Cookie> cookies = await this.cbtWebView.CoreWebView2.CookieManager.GetCookiesAsync("https://localhost:7230");
        // You can trigger any other action here (e.g., enable a button, display a message, etc.)
        ShowAdminForm();
    }

    private void cbtWebView_Click(object sender, EventArgs e)
    {

    }
}

