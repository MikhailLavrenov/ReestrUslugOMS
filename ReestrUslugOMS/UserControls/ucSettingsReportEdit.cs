using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using ReestrUslugOMS.Classes_and_structures;
using ReestrUslugOMS.UserControls;

namespace ReestrUslugOMS.UserControls
{
    public partial class ucSettingsReportEdit : MetroFramework.Controls.MetroUserControl
    {
        dbtNode dbtNodeRow;
        Action onSave;

        public ucSettingsReportEdit(dbtNode dbtNodeRow, string filterParentNames, Action onSave)
        {
            InitializeComponent();
            metroGrid1.AutoGenerateColumns = false;
            

            this.dbtNodeRow = dbtNodeRow;
            this.onSave = onSave;

            var list = Config.Instance.Runtime.dbContext.dbtNode.Local
                .Select(x => new { x.NodeId, x.FullName, x.FullOrder })
                .Where(x => x.FullName.StartsWith(filterParentNames))
                .OrderBy(x => x.FullOrder)
                .ToList();
            metroComboBox1.DataSource = list;
            metroComboBox2.DataSource = Tools.EnumToDataSource<enDataSource>();
            

            metroLabel2.Text = dbtNodeRow.NodeId.ToString();
            metroComboBox1.SelectedValue = dbtNodeRow.ParentId;
            metroTextBox1.Text = dbtNodeRow.Name;
            metroTextBox2.Text = dbtNodeRow.Order;
            metroTextBox4.Text = dbtNodeRow.Color;
            metroCheckBox1.Checked = dbtNodeRow.ReadOnly;
            metroCheckBox2.Checked = dbtNodeRow.Hidden;
            metroComboBox2.SelectedValue = dbtNodeRow.DataSource;

            RefreshGrid();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            MainForm.Instance.RemoveUserControl(this);
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            SaveReportChanges();
            MainForm.Instance.RemoveUserControl(this);
            onSave();
        }

        private void ucSettingsReportEdit_Load(object sender, EventArgs e)
        {
        }

        private void metroTextBox4_ButtonClick(object sender, EventArgs e)
        {
            var cd = new ColorDialog();

            if (metroTextBox4.Text != null)
            {
                var curColor = ColorTranslator.FromHtml(metroTextBox4.Text.ToString());
                cd.CustomColors = new int[] { ColorTranslator.ToWin32(curColor) };
            }

            if (cd.ShowDialog() == DialogResult.OK)
                metroTextBox4.Text = ColorTranslator.ToHtml(cd.Color);
        }

        private void metroTextBox4_TextChanged(object sender, EventArgs e)
        {
            if (metroTextBox4.Text != null)
            {
                var curColor = ColorTranslator.FromHtml(metroTextBox4.Text.ToString());
                metroTextBox4.BackColor = curColor;
            }
        }

        private void metroButton5_Click(object sender, EventArgs e)
        {
            int id = (int)metroGrid1["FormulaId", metroGrid1.CurrentCell.RowIndex].Value;
            var dbtRow = Config.Instance.Runtime.dbContext.dbtFormula.Local.Where(x => x.FormulaId == id).FirstOrDefault();

            if (dbtRow == null)
                return;

            Config.Instance.Runtime.dbContext.dbtFormula.Remove(dbtRow);
            Config.Instance.Runtime.dbContext.SaveChanges();
            RefreshGrid();
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (dbtNodeRow.NodeId == 0)
                SaveReportChanges();
            var dbtRow = new dbtFormula();
            dbtRow.NodeId = dbtNodeRow.NodeId;
            var userControl = new ucSettingsReportEditFormula(dbtRow, RefreshGrid);
            MainForm.Instance.AddUserControl(userControl);
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            int id = (int)metroGrid1["FormulaId", metroGrid1.CurrentCell.RowIndex].Value;
            var dbtRow = Config.Instance.Runtime.dbContext.dbtFormula.Local.Where(x => x.FormulaId == id).FirstOrDefault();

            if (dbtRow == null)
                return;

            var userControl = new ucSettingsReportEditFormula(dbtRow, RefreshGrid);
            MainForm.Instance.AddUserControl(userControl);
        }

        private void metroGrid1_DoubleClick(object sender, EventArgs e)
        {
            metroButton4_Click(new object(), new EventArgs());
        }

        private void SaveReportChanges()
        {
            dbtNodeRow.ParentId = (int)metroComboBox1.SelectedValue;
            dbtNodeRow.Name = metroTextBox1.Text;
            dbtNodeRow.Order = metroTextBox2.Text;
            dbtNodeRow.Color = metroTextBox4.Text;
            dbtNodeRow.ReadOnly = metroCheckBox1.Checked;
            dbtNodeRow.Hidden = metroCheckBox2.Checked;
            dbtNodeRow.DataSource = (enDataSource)metroComboBox2.SelectedValue;

            Config.Instance.Runtime.dbContext.dbtNode.AddOrUpdate(dbtNodeRow);
            Config.Instance.Runtime.dbContext.SaveChanges();
        }

        private void metroButton6_Click(object sender, EventArgs e)
        {
            int id = (int)metroGrid1["FormulaId", metroGrid1.CurrentCell.RowIndex].Value;
            var dbtRow = Config.Instance.Runtime.dbContext.dbtFormula.Local.Where(x => x.FormulaId == id).FirstOrDefault();
            dbtRow = dbtRow.PartialCopy();
            var userControl = new ucSettingsReportEditFormula(dbtRow, RefreshGrid);
            MainForm.Instance.AddUserControl(userControl);
        }
        private void RefreshGrid()
        {
            var list = Config.Instance.Runtime.dbContext.dbtFormula.Local
                .Where(x => x.NodeId == dbtNodeRow.NodeId)
                .Select(x => new
                {
                    x.FormulaId,
                    ResultType = Tools.GetEnumDescription(x.ResultType),
                    DataType = Tools.GetEnumDescription(x.DataType), 
                    Operation = Tools.GetEnumDescription(x.Operation), 
                    x.DataValue,
                    x.FactorValue,
                    DateBegin = x.DateBegin == null ? null : ((DateTime)x.DateBegin).ToString("MMMM yyyy"),
                    DateEnd = x.DateEnd == null ? null : ((DateTime)x.DateEnd).ToString("MMMM yyyy")
                }).ToList();


            Point cur = new Point();
            int index=0;

            if (metroGrid1.CurrentCell != null)
            {
                cur = new Point(metroGrid1.CurrentCell.ColumnIndex, metroGrid1.CurrentCell.RowIndex);
                index = metroGrid1.FirstDisplayedScrollingRowIndex;
            }
                metroGrid1.DataSource = list;

            if (metroGrid1.CurrentCell != null)
            {
                if (cur.Y >= metroGrid1.Rows.Count)
                    cur.Y = metroGrid1.Rows.Count - 1;

                metroGrid1.CurrentCell = metroGrid1[cur.X, cur.Y];

                if (index < metroGrid1.Rows.Count)
                    metroGrid1.FirstDisplayedScrollingRowIndex = index;
            }

        }
    }
}
