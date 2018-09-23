using MetroFramework.Controls;
using ReestrUslugOMS.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using ReestrUslugOMS.Classes_and_structures;

namespace ReestrUslugOMS
{
    public partial class MainForm : MetroFramework.Forms.MetroForm
    {
        private DateTime dateStart = DateTime.Today;
        private DateTime dateEnd = DateTime.Today;

        public DataTable dt;
        private Report report;
        
        public MetroPanel ucContainer { get; set; }
        public static MainForm Instance { get; private set; }


        public static MainForm Create()
        {
            if (Instance == null)
                Instance = new MainForm();
            return Instance;
        }

        private MainForm()
        {
            InitializeComponent();

            Config.Create();
            metroLabel1.Text = Config.Instance.Runtime.CurrentUserName;

            ucContainer = metroPanel1;
            ucContainer.Controls.Clear();
            ucContainer.Controls.Add(new ucMainMenu());
        }

        public void RemoveUserControl(UserControl userControl)
        {
            int last;
            ucContainer.Controls.Remove(userControl);
            userControl.Dispose();
            last = ucContainer.Controls.Count - 1;
            if (last >= 0)
                ucContainer.Controls[last].Visible = true;
        }
        public void AddUserControl(UserControl userControl)
        {
            int last;
            last = ucContainer.Controls.Count - 1;
            if (last >= 0)
                ucContainer.Controls[last].Visible = false;

            ucContainer.Controls.Add(userControl);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Config.Instance.Runtime.db != null)
                Config.Instance.Runtime.db.Dispose();
            if (Config.Instance.Runtime.dbContext != null)
                Config.Instance.Runtime.dbContext.Dispose();
        }


        //строит отчет в экселе
        private void button4_Click(object sender, EventArgs e)
        {
            report = new Report(enReportMode.Отчет);
            report.SetParams(dateStart, dateEnd, enErrorMode.БезОшибок, enInsuranceMode.ВсеПациенты);

            //report.SaveExcel(xlsxFile);
            Process.Start(Config.Instance.xlsxFile, null);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            dateStart = dateStart.AddMonths(1);
            textBox5.Text = dateStart.ToString("MM.yyyy");
            if (textBox6.Visible == false)
                dateEnd = dateStart;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            dateStart = dateStart.AddMonths(-1);
            textBox5.Text = dateStart.ToString("MM.yyyy");
            if (textBox6.Visible == false)
                dateEnd = dateStart;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //var NewResultValues = new Report.ResultItem[report.Rows.Length, report.Cols.Length];

            //for (int i = 0; i < NewResultValues.GetLength(0); i++)
            //    for (int j = 0; j < NewResultValues.GetLength(1); j++)
            //        if (dataGridView2[j + report.MaxRowLevel, i + report.MaxColLevel].Value != null)
            //            NewResultValues[i, j].Value = Convert.ToDouble(dataGridView2[j + report.MaxRowLevel, i + report.MaxColLevel].Value.ToString());

            //report.SavePlan(NewResultValues);
            //report.SetResultValues();

            //report.ToGrid(dataGridView2);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            enReportMode reportMode = 0;
            enErrorMode errorMode = 0;
            enInsuranceMode nonResidentMode = 0;
            DateTime date1 = dateStart;
            DateTime date2 = dateEnd;



            if (radioButton3.Checked)
            {
                reportMode = enReportMode.Отчет;

                //диапазон дат
                if (radioButton6.Checked)
                    date2 = date1;
                else if (radioButton7.Checked)
                {
                    date1 = dateStart.AddMonths(1 - dateStart.Month);
                    date2 = dateStart;
                }

                //ошибки
                if (radioButton9.Checked)
                    errorMode = enErrorMode.БезОшибок;
                else if (radioButton10.Checked)
                    errorMode = enErrorMode.ВсеУслуги;
                else if (radioButton11.Checked)
                    errorMode = enErrorMode.ТолькоОшибки;

                //иногородние
                if (radioButton12.Checked)
                    nonResidentMode = enInsuranceMode.БезИногодних;
                else if (radioButton13.Checked)
                    nonResidentMode = enInsuranceMode.ВсеПациенты;
                else if (radioButton13.Checked)
                    nonResidentMode = enInsuranceMode.ТолькоИногородние;

                if ((errorMode == 0) || (nonResidentMode == 0))
                {
                    MessageBox.Show("Заполнены не все параметры", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

            }
            else if (radioButton4.Checked)
                reportMode = enReportMode.ПланОтделения;
            else if (radioButton5.Checked)
                reportMode = enReportMode.ПланВрача;

            if (reportMode == 0)
            {
                MessageBox.Show("Заполнены не все параметры", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            report = new Report(reportMode);

            //для выбора уровня детализации отчета
            comboBox6.Items.Clear();
            for (int i = 1; i <= report.MaxRowLevel - 1; i++)
                comboBox6.Items.Add(i);
            comboBox6.SelectedIndex = comboBox6.Items.Count - 1;

            comboBox7.Items.Clear();
            for (int i = 1; i <= report.MaxColLevel - 1; i++)
                comboBox7.Items.Add(i);
            comboBox7.SelectedIndex = comboBox7.Items.Count - 1;

            report.SetParams(date1, date2,  errorMode, nonResidentMode);

            //report.ToGrid(dataGridView2);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            dateEnd = dateEnd.AddMonths(-1);
            textBox6.Text = dateEnd.ToString("MM.yyyy");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            dateEnd = dateEnd.AddMonths(1);
            textBox6.Text = dateEnd.ToString("MM.yyyy");
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Visible = radioButton3.Checked;
            groupBox3.Visible = radioButton3.Checked;
            groupBox4.Visible = radioButton3.Checked;


            if (radioButton3.Checked)
                textBox6.Visible = radioButton8.Checked;
            else
                textBox6.Visible = false;
        }

        private void textBox6_VisibleChanged(object sender, EventArgs e)
        {
            if (textBox6.Visible)
                dateEnd = dateStart;
            else
                dateStart = dateEnd;

            textBox5.Text = dateStart.ToString("MM.yyyy");
            textBox6.Text = dateEnd.ToString("MM.yyyy");

            button14.Visible = textBox6.Visible;
            button15.Visible = textBox6.Visible;
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            textBox6.Visible = radioButton8.Checked;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            button12.Visible = radioButton4.Checked;
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            button12.Visible = radioButton5.Checked;
        }

        private void dataGridView2_DoubleClick(object sender, EventArgs e)
        {
            //report.ExpandCollapse(dataGridView2, new Point { X = dataGridView2.CurrentCell.ColumnIndex, Y = dataGridView2.CurrentCell.RowIndex });
        }

        private void button11_Click_1(object sender, EventArgs e)
        {
            //report.ExpandCollapse(dataGridView2, int.Parse(comboBox6.Text), int.Parse(comboBox7.Text));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            

            //включаем двойной буфер чтобы не лагала таблица DataGridView
            typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataGridView2, new object[] { true });


            textBox5.Text = dateStart.ToString("MM.yyyy");
            textBox6.Text = dateEnd.ToString("MM.yyyy");

            tabControl1.SelectedIndex = -1;
        }

        private void button16_MouseLeave(object sender, EventArgs e)
        {
            button16.FlatAppearance.BorderSize = 0;
        }

        private void button16_MouseEnter(object sender, EventArgs e)
        {
            button16.FlatAppearance.BorderSize = 2;
        }

        private void button16_MouseCaptureChanged(object sender, EventArgs e)
        {
            button16.FlatAppearance.BorderSize = 0;
            button16.Update();
            Thread.Sleep(120);
            button16.FlatAppearance.BorderSize = 2;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            //удаляем кэш EF


            foreach (var entry in Config.Instance.Runtime.dbContext.ChangeTracker.Entries())
                Config.Instance.Runtime.dbContext.Entry(entry.Entity).State = System.Data.Entity.EntityState.Detached;


            ucContainer.Controls.Clear();
            ucContainer.Controls.Add(new ucMainMenu());
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
