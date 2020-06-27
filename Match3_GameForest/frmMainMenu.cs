using System;
using System.Windows.Forms;

namespace Match3_GameForest
{
    public partial class frmMainMenu : Form
    {
        public frmMainMenu()
        {
            InitializeComponent();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            frmGame NewWindow;
            NewWindow = new frmGame();

            this.Visible = false;
            NewWindow.ShowDialog();
            NewWindow.Dispose();
            this.Close();
        }

        
    }
}
