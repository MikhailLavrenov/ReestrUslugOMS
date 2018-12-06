using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ReestrUslugOMS.UserControls
{
    public partial class Spinner : MetroFramework.Controls.MetroUserControl
    {
        public Spinner()
        {
            InitializeComponent();
            Visible = false;
        }

        private void Spinner_VisibleChanged(object sender, EventArgs e)
        {
        }
        public void Start()
        {
            BringToFront();            
            metroProgressSpinner1.Spinning = true;
            Visible = true;
            Update();
        }
        public void Stop()
        {            
            metroProgressSpinner1.Spinning = false;
            Visible = false;
            Update();
        }


    }
}
