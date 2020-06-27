using System;
using System.Windows.Forms;


namespace Match3_GameForest
{
    public partial class Tile : PictureBox
    {
        //private variables
        private int mPointValue; //how many points this tile is worth before factoring in combo multiplier

        //DO NOT TOUCH - makes any clicks on picturebox register as clicks on the main form to trigger main form event
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = (-1);

            if (m.Msg == WM_NCHITTEST)
                m.Result = (IntPtr)HTTRANSPARENT;
            else
                base.WndProc(ref m);
        }

        public int PointValue //property to get set this tile's base point value
        {
            get { return mPointValue; }
            set { mPointValue = value; }
        }

    }
}
