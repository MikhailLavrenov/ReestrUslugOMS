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
    public partial class ucMainMenu : MetroFramework.Controls.MetroUserControl
    {
        public ucMainMenu()
        {
            InitializeComponent();
        }

        private void metroTile1_Click(object sender, EventArgs e)
        {
            MainForm.Instance.AddUserControl(new ucSettingsDb());
        }

        private void metroTile5_Click(object sender, EventArgs e)
        {
            MainForm.Instance.AddUserControl(new ucSettingsUsers());
        }

        private void metroTile2_Click(object sender, EventArgs e)
        {
            MainForm.Instance.AddUserControl(new ucSettingsReport());
        }

        private void metroTile3_Click(object sender, EventArgs e)
        {
            MainForm.Instance.AddUserControl(new ucReport(enReportMode.Отчет));
        }

        private void metroTile6_Click(object sender, EventArgs e)
        {
            MainForm.Instance.AddUserControl(new ucReport(enReportMode.ПланВрача));
        }

        private void metroTile7_Click(object sender, EventArgs e)
        {
            MainForm.Instance.AddUserControl(new ucReport(enReportMode.ПланОтделения));
        }

        private void metroTile8_Click(object sender, EventArgs e)
        {
            MainForm.Instance.AddUserControl(new ucImportDbf());
        }

        private void metroTile9_Click(object sender, EventArgs e)
        {
            MainForm.Instance.AddUserControl(new ucUserQueries());
        }
    }
}
