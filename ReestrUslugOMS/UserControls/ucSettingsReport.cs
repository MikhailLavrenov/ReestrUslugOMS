using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Entity;
using ReestrUslugOMS.Classes_and_structures;
using System.Data.Entity.Migrations;

namespace ReestrUslugOMS.UserControls
{
    public partial class ucSettingsReport : MetroFramework.Controls.MetroUserControl
    {
        string mainFilterName = "";

        public ucSettingsReport()
        {
            InitializeComponent();
            Config.Instance.Runtime.dbContext.dbtNode.Load();
            Config.Instance.Runtime.dbContext.dbtFormula.Load();

            metroButton4_Click(new object(), new EventArgs());
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            int id = (int)metroGrid1["NodeId", metroGrid1.CurrentCell.RowIndex].Value;

            int order = 1;
            order+=Config.Instance.Runtime.dbContext.dbtNode.Local
                .Where(x => x.ParentId == id)
                .DefaultIfEmpty()
                .Max(x => x == null ? 0 : int.Parse(x.Order));

            var dbtRow = new dbtNode();
            dbtRow.ParentId = id;
            dbtRow.Order = order.ToString();

            var userControl = new ucSettingsReportEdit(dbtRow, mainFilterName, RefreshGrid);
            MainForm.Instance.AddUserControl(userControl);
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            int id = (int)metroGrid1["NodeId", metroGrid1.CurrentCell.RowIndex].Value;
            var dbtRow = Config.Instance.Runtime.dbContext.dbtNode.Local.Where(x => x.NodeId == id).FirstOrDefault();

            if (dbtRow == null || dbtRow.Name == "Строки" || dbtRow.Name == "Столбцы")
                return;

            var userControl = new ucSettingsReportEdit(dbtRow, mainFilterName, RefreshGrid);
            MainForm.Instance.AddUserControl(userControl);
        }

        private void metroGrid1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in metroGrid1.Rows)
            {
                var cell = row.Cells["Color"];
                if (cell.Value != null)
                {
                    var curColor = ColorTranslator.FromHtml(cell.Value.ToString());
                    cell.Style.BackColor = curColor;
                }
            }
        }

        private void ucSettingsReport_Load(object sender, EventArgs e)
        {
            this.Dock = DockStyle.Fill;
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            int id = (int)metroGrid1["NodeId", metroGrid1.CurrentCell.RowIndex].Value;
            var dbtRow = Config.Instance.Runtime.dbContext.dbtNode.Local.Where(x => x.NodeId == id).FirstOrDefault();


            if (dbtRow == null || dbtRow.Name== "Строки" || dbtRow.Name == "Столбцы")
                return;

            DeleteRow(dbtRow);
            Config.Instance.Runtime.dbContext.SaveChanges();
            RefreshGrid();
        }

        private void metroTextBox1_ButtonClick(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        private void metroTextBox1_ClearClicked()
        {
            RefreshGrid();
        }

        private void metroGrid1_DoubleClick(object sender, EventArgs e)
        {
            metroButton2_Click(new object(), new EventArgs());
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            if (metroButton4.Text == "Строки")
                metroButton4.Text = "Столбцы";
            else
                metroButton4.Text = "Строки";

            mainFilterName = metroButton4.Text;
            metroTextBox1.Text = "";
            RefreshGrid();
        }

        private void metroButton5_Click(object sender, EventArgs e)
        {
            int id = (int)metroGrid1["NodeId", metroGrid1.CurrentCell.RowIndex].Value;
            var row = Config.Instance.Runtime.dbContext.dbtNode.Local.Where(x => x.NodeId == id).FirstOrDefault();

            var newRow = CopySubTree(row);

            int maxOrder = Config.Instance.Runtime.dbContext.dbtNode.Local
                .Where(x => x.ParentId == newRow.ParentId)
                .Max(x => int.Parse(x.Order)) + 1;

            newRow.Order = maxOrder.ToString();
            newRow.Name = string.Format("{0}{1}", "Копия ", newRow.Name);

            Config.Instance.Runtime.dbContext.dbtNode.AddOrUpdate(newRow);
            Config.Instance.Runtime.dbContext.SaveChanges();
            RefreshGrid();

            var userControl = new ucSettingsReportEdit(newRow, mainFilterName, RefreshGrid);
            MainForm.Instance.AddUserControl(userControl);
        }

        dbtNode CopySubTree(dbtNode instance, dbtNode newParent = null)
        {
            var newRow = CopyReport(instance, newParent);

            var list = instance.Next.ToList();

            for (int i = 0; i < list.Count; i++)
                CopySubTree(list[i], newRow);

            return newRow;
        }

        dbtNode CopyReport(dbtNode fromReport, dbtNode newParent = null)
        {
            var newReport = fromReport.PartialCopy();
            if (newParent != null)
                newReport.ParentId = newParent.NodeId;

            Config.Instance.Runtime.dbContext.dbtNode.AddOrUpdate(newReport);
            Config.Instance.Runtime.dbContext.SaveChanges();

            //копируем формулы
            foreach (var item in fromReport.Formula)
            {
                var newFormula = item.PartialCopy();
                newFormula.NodeId = newReport.NodeId;
                Config.Instance.Runtime.dbContext.dbtFormula.AddOrUpdate(newFormula);
            }
            Config.Instance.Runtime.dbContext.SaveChanges();

            return newReport;
        }

        void DeleteRow(dbtNode row)
        {
            var list = row.Next.ToList();

            for (int i = 0; i < list.Count; i++)
                    DeleteRow(list[i]);

            Config.Instance.Runtime.dbContext.dbtNode.Remove(row);
        }


        public void RefreshGrid()
        {
            string additionalFilterName = metroTextBox1.Text;
            Config.Instance.Runtime.dbContext.dbtNode.Local.Where(x => x.Prev == null).ToList().ForEach(x => x.SetAllFullNamesAndOrders());


            var list = Config.Instance.Runtime.dbContext.dbtNode.Local
                .Select(x => new
                {
                    x.NodeId,
                    x.FullOrder,
                    x.FullName,
                    x.Color,
                    x.ReadOnly,
                    DataSource = Tools.GetEnumDescription(x.DataSource)
                })
                 .Where(x => x.FullName.StartsWith(mainFilterName) && x.FullName.Contains(additionalFilterName))
                 .OrderBy(x => x.FullOrder)
                 .ToList();

            if (metroGrid1.CurrentCell != null)
            {
                Point cur = new Point(metroGrid1.CurrentCell.ColumnIndex, metroGrid1.CurrentCell.RowIndex);
                int index = metroGrid1.FirstDisplayedScrollingRowIndex;

                metroGrid1.DataSource = list;

                if (cur.Y >= metroGrid1.Rows.Count)
                    cur.Y = metroGrid1.Rows.Count - 1;

                metroGrid1.CurrentCell = metroGrid1[cur.X, cur.Y];

                if (index < metroGrid1.Rows.Count)
                    metroGrid1.FirstDisplayedScrollingRowIndex = index;
            }
            else
                metroGrid1.DataSource = list;
        }




    }
}
