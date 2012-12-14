using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace installer
{
    public enum windowType {
        INSTALL, DOWNLOAD
    }

    public partial class ProgressMeter : Form
    {
        public ProgressMeter()
        {
            InitializeComponent();
        }

        public void setProgress(int i, string XBianVer, windowType type)
        {
            this.progressBar1.Value = i;

            if (type.Equals(windowType.DOWNLOAD))
                label1.Text = "(1/2) Downloading XBian " + XBianVer + "  -  " + i + "%";
            else
                label1.Text = "(2/2) Installing XBian " + XBianVer + " on your SD card  -  " + i + "%";
        }
    }
}
