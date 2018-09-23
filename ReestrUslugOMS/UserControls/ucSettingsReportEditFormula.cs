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
    public partial class ucSettingsReportEditFormula : MetroFramework.Controls.MetroUserControl
    {
        dbtFormula dbtFormulaRow;
        Action onSave;

        public ucSettingsReportEditFormula(dbtFormula dbtFormulaRow, Action onSave)
        {
            this.dbtFormulaRow = dbtFormulaRow;
            this.onSave = onSave;

            InitializeComponent();

            metroComboBox2.DataSource = Tools.EnumToDataSource<enResultType>();
            metroComboBox3.DataSource = Tools.EnumToDataSource<enDataType>();
            metroComboBox4.DataSource = Tools.EnumToDataSource<enOperation>();

            metroComboBox2.SelectedValue=dbtFormulaRow.ResultType;
            metroComboBox3.SelectedValue=dbtFormulaRow.DataType;
            metroTextBox1.Text=dbtFormulaRow.DataValue;
            metroComboBox4.SelectedValue=dbtFormulaRow.Operation;
            metroTextBox2.Text=dbtFormulaRow.FactorValue.ToString();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            MainForm.Instance.RemoveUserControl(this);            
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            dbtFormulaRow.ResultType = (enResultType)metroComboBox2.SelectedValue;
            dbtFormulaRow.DataType = (enDataType)metroComboBox3.SelectedValue;
            dbtFormulaRow.DataValue = metroTextBox1.Text;
            dbtFormulaRow.Operation = (enOperation)metroComboBox4.SelectedValue;
            dbtFormulaRow.FactorValue = metroTextBox2.Text==""?(double?)null:double.Parse(metroTextBox2.Text);

            Config.Instance.Runtime.dbContext.dbtFormula.AddOrUpdate(dbtFormulaRow);
            Config.Instance.Runtime.dbContext.SaveChanges();
            onSave();

            MainForm.Instance.RemoveUserControl(this);
        }


    }
}
