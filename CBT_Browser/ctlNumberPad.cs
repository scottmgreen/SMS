using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CBT_Forms
{
    public partial class ctlNumberPad : UserControl
    {
        private string _data = string.Empty;
        public string Data
        {
            get => _data;
            set
            {
                _data = value;
                DataChanged?.Invoke(this, new EventArgs());
            }
        }
        public event DataChangedEventHandler DataChanged;
        public delegate void DataChangedEventHandler(object sender, EventArgs e);

        
        public event UsedForChangedEventHandler OnUsedForChange;
        public delegate void UsedForChangedEventHandler(object sender, EventArgs e);

        public DialogResult DialogResult { get; set; }
        public event DialogResultChangedEventHandler DialogResultChanged;
        public delegate void DialogResultChangedEventHandler(object sender, EventArgs e);

        private void NumericMouseUp(object sender)
        {
            Button button = sender as Button;
            if (Data.Length < 4)
            {
                Data += button.Text;
                lblNumber.Text = Data;
                btnBackSpace.Enabled = true;
                btnBackSpace.BackColor = Color.White;
                btnBackSpace.Font = new Font("Arial", 27.5F, FontStyle.Bold);
                btnBackSpace.Text = "⌫";
                btnBackSpace.ForeColor = Color.Black;


                btnEnter.Enabled = false;
                btnEnter.BackColor = Color.Gray;
                btnEnter.Text = "";

            }
            if (Data.Length == 4)
            {
                btnEnter.Enabled = true;
                btnEnter.BackColor = Color.Green;
                btnEnter.Font = new Font("Segoe ", 50.5F, FontStyle.Bold);
                btnEnter.Text = "\u21B5";
                btnEnter.ForeColor = Color.Black;
                
                btnBackSpace.BackColor = Color.White;
                btnBackSpace.Font = new Font("Arial", 27.5F, FontStyle.Bold);
                btnBackSpace.Text = "⌫";
                foreach (Button btn in Controls.OfType<Button>())
                {

                    if (!btn.Name.Equals("btnEnter") && !btn.Name.Equals("btnBackSpace"))
                    {
                        btn.Enabled = false;
                        btn.Text = "";
                    }

                }
            }



        }
        public ctlNumberPad()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

        }

        private void CtlNumberPad_Load(object sender, EventArgs e)
        {
            lblNumber.Text = string.Empty;
        }

        private void BtnEnter_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            DialogResultChanged?.Invoke(sender, e);
        }

        private void BtnBackSpace_MouseUp(object sender, MouseEventArgs e)
        {
            foreach (Button btn in Controls.OfType<Button>())
            {
                btn.Enabled = true;
            }

            if (Data.Length == 0)
            {
                Data = string.Empty;
                lblNumber.Text = Data;
            }

            else
            {
                Data = Data.Substring(0, Data.Length - 1);
                lblNumber.Text = Data;
                btnEnter.Enabled = false;
                btnEnter.BackColor = Color.Gray;
                btnEnter.Text = "";
                if (Data.Length == 0)
                {
                    btnBackSpace.Enabled = false;
                    btnBackSpace.BackColor = Color.Gray;
                    btnBackSpace.Text = "";

                }
            }

        }

        private void Btn1_MouseUp(object sender, MouseEventArgs e)
        {
            NumericMouseUp(sender);
        }

        private void Btn2_MouseUp(object sender, MouseEventArgs e)
        {
            NumericMouseUp(sender);
        }

        private void Btn3_MouseUp(object sender, MouseEventArgs e)
        {
            NumericMouseUp(sender);
        }

        private void Btn4_MouseUp(object sender, MouseEventArgs e)
        {
            NumericMouseUp(sender);
        }

        private void Btn5_MouseUp(object sender, MouseEventArgs e)
        {
            NumericMouseUp(sender);
        }

        private void Btn6_MouseUp(object sender, MouseEventArgs e)
        {
            NumericMouseUp(sender);
        }

        private void Btn7_MouseUp(object sender, MouseEventArgs e)
        {
            NumericMouseUp(sender);
        }

        private void Btn8_MouseUp(object sender, MouseEventArgs e)
        {
            NumericMouseUp(sender);
        }

        private void Btn9_MouseUp(object sender, MouseEventArgs e)
        {
            NumericMouseUp(sender);
        }

        private void Btn0_MouseUp(object sender, MouseEventArgs e)
        {
            NumericMouseUp(sender);
        }

        private void CtlNumberPad_VisibleChanged(object sender, EventArgs e)
        {



            _data = string.Empty;
            lblNumber.Text = Data;

            foreach (Button btn in Controls.OfType<Button>())
            {
                if (!btn.Name.Equals("btnEnter") && !btn.Name.Equals("btnBackSpace"))
                {
                    btn.Enabled = true;
                }

            }

        }


    }
}
