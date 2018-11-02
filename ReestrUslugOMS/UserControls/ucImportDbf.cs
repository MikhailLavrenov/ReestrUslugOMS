using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ReestrUslugOMS.Classes_and_structures;
using System.Data.Entity;

using System.IO;

namespace ReestrUslugOMS.UserControls
{
    public partial class ucImportDbf : MetroFramework.Controls.MetroUserControl
    {
        private int historyLimit;

        public ucImportDbf()
        {
            Config.Instance.Runtime.db = new MSSQLDB();

            InitializeComponent();
            Dock = DockStyle.Fill;

            metroCheckBox1.Checked = true;
            ucPeriodSelector1.Date = System.DateTime.Today.FirstDayDate();

            metroButton2_Click(new object(), null);
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            var list = new List<enImportItems>();

            if (metroCheckBox1.Checked)
                list.Add(enImportItems.УслугиПациентыДоРазложения);
            if (metroCheckBox2.Checked)
                list.Add(enImportItems.УслугиПациентыОшибкиПослеРазложения);
            if (metroCheckBox3.Checked)
                list.Add(enImportItems.МедПерсонал);
            if (metroCheckBox4.Checked)
                list.Add(enImportItems.КлассификаторУслуг);
            if (metroCheckBox5.Checked)
                list.Add(enImportItems.СРЗ);

            var importer = new ImportDbf(ucPeriodSelector1.Date, list);
            importer.Import();

            RefreshGrid();
        }

        private void RefreshGrid()
        {
            var list = Config.Instance.Runtime.dbContext.dbtImportHistory.Local
                .Select(x => new
                {
                    x.ImportHistoryId,
                    Table=Tools.GetEnumDescription(x.Table),
                    x.Organisation,
                    Period = x.Period?.ToString("MMMM yyyy"),
                    x.Count,
                    Status = Tools.GetEnumDescription(x.Status),
                    x.DateTime,
                }) .OrderByDescending(x=>x.DateTime)  
                .Take(historyLimit)
                .ToList();

            metroGrid1.DataSource = list;
        }

        private void metroCheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (metroCheckBox1.Checked)
                metroCheckBox2.Checked = false;
        }

        private void metroCheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (metroCheckBox2.Checked)
                metroCheckBox1.Checked = false;
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            int oldLimit = historyLimit;
            historyLimit= Convert.ToInt32(metroTextBox1.Text);

            if (oldLimit < historyLimit)
                Config.Instance.Runtime.dbContext.dbtImportHistory.OrderBy(x => x.DateTime).Take(historyLimit).Load();

            RefreshGrid();
        }
    }


}
