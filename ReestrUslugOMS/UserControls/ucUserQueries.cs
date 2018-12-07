using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ReestrUslugOMS.Classes_and_structures;

namespace ReestrUslugOMS.UserControls
{
    public partial class ucUserQueries : MetroFramework.Controls.MetroUserControl
    {
        private UserQuery selectedQuery;

        public ucUserQueries()
        {
            InitializeComponent();

            ucPeriodSelector1.Date = DateTime.Today.FirstDayDate().AddDays(14);
            Config.Instance.Runtime.db = new MSSQLDB();
            Dock = DockStyle.Fill;
            metroComboBox1.DataSource = Config.Instance.UserQueries;
        }

        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedQuery = Config.Instance.UserQueries.Where(x => x.ProcName == metroComboBox1.SelectedValue.ToString()).FirstOrDefault();
            metroLabel7.Text = selectedQuery.Description;
            metroButton3.Visible = !selectedQuery.WithPatient;
            metroPanel1.Visible = selectedQuery.WithPatient;
            metroGrid1.Columns.Clear();
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void metroTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPressHandler(e);
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            metroTextBox1.Text = "";
            metroTextBox2.Text = "";
            metroTextBox3.Text = "";
            metroTextBox1.Focus();
            metroGrid1.Columns.Clear();           
        }

        private void metroTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPressHandler(e);
        }

        private void metroTextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            KeyPressHandler(e);
        }

        private void RefreshGrid()
        {
            var sql = new StringBuilder();
            sql.Append($"exec {selectedQuery.ProcName} @period = '{ucPeriodSelector1.Date}', @mcod = '{Config.Instance.LpuCode}'");

            if (selectedQuery.WithPatient == true)
                sql.Append($", @fam = '{metroTextBox1.Text}', @im = '{metroTextBox2.Text}', @ot = '{metroTextBox3.Text}' ");

            var data = Config.Instance.Runtime.db.Select(sql.ToString());
            metroGrid1.Columns.Clear();
            metroGrid1.DataSource = data;
            metroGrid1.Visible = true;

            for (int i = 0; i < metroGrid1.Columns.Count; i++)
                metroGrid1.Columns[i].Width = selectedQuery.ColsWidth[i];
            metroGrid1.Columns[metroGrid1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void KeyPressHandler(KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
                metroButton1_Click(new object(), new EventArgs());
            if (e.KeyChar == Convert.ToChar(Keys.Escape))
                metroButton2_Click(new object(), new EventArgs());
        }
    }
}
