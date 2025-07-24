
using CBT_Browser;

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static CBT_Browser.frmMain;

namespace CBT_Forms
{

    public partial class frmAdmin : Form
    {
        private readonly frmMain _frmMain = Application.OpenForms["frmMain"] as frmMain;
        
        private string _attempted_unlocknum = string.Empty;
        private string _actual_unlocknum = "1234";
        private string _attempted_exittodesktop = string.Empty;
        private string _actual_exittodesktop = string.Empty;
        private int _attempted_unlock_count;




        
        public frmAdmin(frmMain frmmain)
        {
            _frmMain = frmmain;
            

            InitializeComponent();
            var settingsHelper = new SettingsHelper();
            _actual_unlocknum = settingsHelper.GetAppSetting("AdminPasscode");
            ctlNumberPad.DataChanged += CtlNumberPad_DataChanged;
            ctlNumberPad.DialogResultChanged += CtlNumberPad_DialogResultChanged;

        }
        
        private void FrmAdmin_Load(object sender, EventArgs e)
        {
            panel1.Left = ((ClientSize.Width - panel1.Width) / 2);
            lblAdminUnlock.Left= ((ClientSize.Width - lblAdminUnlock.Width) / 2);
            ctlNumberPad.Left = (ClientSize.Width - ctlNumberPad.Width) / 2;
            ResetAdminLabels();
            

            btnReset.Left = ((panel1.Width - btnReset.Width) / 2);
            btnRestart.Left = ((panel1.Width - btnRestart.Width) / 2);




        }
        private void ResetAdminLabels()
        {
            lblAdmin1.Text = string.Empty;
            lblAdmin1.BackColor = Color.DarkGray;
            lblAdmin2.Text = string.Empty;
            lblAdmin2.BackColor = Color.DarkGray;
            lblAdmin3.Text = string.Empty;
            lblAdmin3.BackColor = Color.DarkGray;
            lblAdmin4.Text = string.Empty;
            lblAdmin4.BackColor = Color.DarkGray;

            
        }
        private void ResetLabel(Label label)
        {
            label.Text = string.Empty;
            label.BackColor = Color.DarkGray;
        }

        private void SetLabel(Label label, string text)
        {
            label.Text = text;
            label.BackColor = Color.White;
        }
        
        private void EnterButton()
        {
            _attempted_unlock_count += 1;

            if (_attempted_unlocknum.Length == 4 && _attempted_unlocknum.Equals(_actual_unlocknum))
            {

                // bool isLocked = (PDX_CBT.Properties.Settings.Default.TrainingStationStatus.Equals(TrainingStationStatus.GetName(TrainingStationStatus.LOCKED.GetType(), TrainingStationStatus.LOCKED)));

                // two scenarios 
                // 1) clear the lock and restart
                // 2) clear the lock and redirect

                panel1.Visible = true;
                ctlNumberPad.Visible = false;
                lblAdmin1.Visible = false;
                lblAdmin2.Visible = false;
                lblAdmin3.Visible = false;
                lblAdmin4.Visible = false;
                btnReset.Visible = true;
                btnRestart.Visible = true;



                // scenario 1 first

                //DialogResult result = MessageBox.Show("This is an Admin Unlock, The lock will be cleared and the CBT Station will restart..Continue? ", "", MessageBoxButtons.YesNo);
                //string choice = ShowResetOrRestartDialog("Would you like to Reset the Session or Restart the System?");

                //if (choice == "Reset Session")
                //{
                //    // Logic for resetting session
                //    MessageBox.Show("You chose to Reset the Session.");
                //    panel1.Visible = true;
                //    ctlNumberPad.Visible = false;
                //    lblAdmin1.Visible = false;
                //    lblAdmin2.Visible = false;
                //    lblAdmin3.Visible = false;
                //    lblAdmin4.Visible = false;
                //    btnReset.Visible = true;
                //    //btnRestart.Visible = true;
                //}
                //else if (choice == "Restart System")
                //{
                //    // Logic for restarting system
                //    MessageBox.Show("You chose to Restart the System.");
                //    panel1.Visible = true;
                //    ctlNumberPad.Visible = false;
                //    lblAdmin1.Visible = false;
                //    lblAdmin2.Visible = false;
                //    lblAdmin3.Visible = false;
                //    lblAdmin4.Visible = false;
                //    //btnReset.Visible = true;
                //    btnRestart.Visible = true;
                //}



            }
        }

        private void CtlNumberPad_DataChanged(object sender, EventArgs e)
        {
            DataChange(ctlNumberPad.Data);
        }


        private void CtlNumberPad_DialogResultChanged(object sender, EventArgs e)
        {
            EnterButton();
        }

        private void DataChange(string data)
        {

            switch (data.Length)
            {
                case 0:
                    _attempted_unlocknum = string.Empty;
                    ResetAdminLabels();
                    ResetLabel(lblAdmin1);
                    ResetLabel(lblAdmin2);
                    ResetLabel(lblAdmin3);
                    ResetLabel(lblAdmin4);
                    break;
                case 1:
                    SetLabel(lblAdmin1, data.Substring(0, 1));
                    ResetLabel(lblAdmin2);
                    ResetLabel(lblAdmin3);
                    ResetLabel(lblAdmin4);
                    break;
                case 2:
                    SetLabel(lblAdmin2, data.Substring(1, 1));
                    ResetLabel(lblAdmin3);
                    ResetLabel(lblAdmin4);
                    break;
                case 3:
                    SetLabel(lblAdmin3, data.Substring(2, 1));
                    ResetLabel(lblAdmin4);
                    break;
                case 4:
                    SetLabel(lblAdmin4, data.Substring(3, 1));
                    break;
                default:
                    break;
            }
            _attempted_unlocknum = data;
            _attempted_exittodesktop = data;


        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            PDX_CBT_Restart();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            

        }
        public void PDX_CBT_Restart()
        {
            try
            {
                //Mark station as UNLOCKED
                
                //run the program again and close this one
                string appname = System.AppDomain.CurrentDomain.FriendlyName;

                Process.Start($"{Application.StartupPath}\\{appname}");
                //or you can use Application.ExecutablePath

                //close this one
                
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {

                
            }
        }
    }
}
