﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Match3_GameForest
{
    public partial class frmEnd : Form
    {
        public frmEnd()
        {
            InitializeComponent();
            
           
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
            frmMainMenu NewWindow;
            NewWindow = new frmMainMenu();
            NewWindow.ShowDialog();
            NewWindow.Dispose();

            
        }
    }
}
