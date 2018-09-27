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
using unvell.ReoGrid;
using System.Reflection;

namespace ReestrUslugOMS.UserControls
{
    public partial class ucReport : MetroFramework.Controls.MetroUserControl
    {
        enReportMode ReportMode { get; set; }

        private DateTime date1;
        private DateTime date2;
        public PointF gridScrollBarsPosition;    //из-за бага ReoGridControl после hide/show строк/столбцов сбрасывается позиция scrollBars        
        private Report report;       

        public ucReport(enReportMode reportMode= enReportMode.Отчет)
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;

            ReportMode = reportMode;
            report = new Report(ReportMode);

            if (reportMode != enReportMode.Отчет)
                metroPanel2.Visible = false;

            date1 = DateTime.Today;
            date1 = date1.AddDays(1 - date1.Day);
            date2 = date1;           
          
            metroTextBox1.Text = date1.ToString("MM.yyyy");
            metroTextBox2.Text = date2.ToString("MM.yyyy");

            metroComboBox1.DataSource = Tools.EnumToDataSource<enInsuranceMode>();
            metroComboBox2.DataSource = Tools.EnumToDataSource<enErrorMode>();

            metroComboBox3.SelectedIndex = 0;
            metroComboBox1.SelectedValue =enInsuranceMode.БезИногодних;
            metroComboBox2.SelectedValue =enErrorMode.БезОшибок;
           
            //задаем стиль таблицы
            ControlAppearanceStyle rgcs = new ControlAppearanceStyle(Color.DarkGray, Color.LightGray, false);
            
            rgcs[ControlAppearanceColors.ColHeadNormalStart] = Color.LightGray;
            rgcs[ControlAppearanceColors.ColHeadNormalEnd] = Color.LightGray;
            rgcs[ControlAppearanceColors.RowHeadNormal] = Color.LightGray;

            rgcs[ControlAppearanceColors.ColHeadSelectedStart] = Color.DarkGray;
            rgcs[ControlAppearanceColors.ColHeadSelectedEnd] = Color.DarkGray;
            rgcs[ControlAppearanceColors.RowHeadSelected] = Color.DarkGray;

            rgcs[ControlAppearanceColors.ColHeadFullSelectedStart] = Color.DarkGray;
            rgcs[ControlAppearanceColors.ColHeadFullSelectedEnd] = Color.DarkGray;
            rgcs[ControlAppearanceColors.RowHeadFullSelected] = Color.DarkGray;

            rgcs[ControlAppearanceColors.GridLine] = Color.DarkGray;

            rgcs[ControlAppearanceColors.SelectionBorder] = Color.DarkGray;


            reoGridControl1.ControlStyle = rgcs;
            reoGridControl1.CellsSelectionCursor = Cursors.Default;            

            //настройка шкалы детализации
            if (report.MaxRowLevel > 2)
            {
                metroPanel7.Enabled = true;
                metroTrackBar1.Maximum = report.MaxRowLevel - 1;
            }

            if (report.MaxColLevel > 2)
            {
                metroPanel8.Enabled = true;
                metroTrackBar2.Maximum = report.MaxColLevel - 1;
            }

            if (ReportMode == enReportMode.Отчет)
                metroTrackBar1.Value = (int)Math.Ceiling((double)metroTrackBar1.Maximum / 2);
            else
                metroTrackBar1.Value = metroTrackBar1.Maximum;
            metroTrackBar2.Value = (int)Math.Ceiling((double)metroTrackBar2.Maximum / 2);
            
            report.HeadersToReoGrid(reoGridControl1.CurrentWorksheet);

            metroTrackBar1_MouseCaptureChanged(new object(), new EventArgs());
            metroTrackBar2_MouseCaptureChanged(new object(), new EventArgs());
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (metroComboBox3.SelectedIndex == 0)
                date2 = date1;
            else if (metroComboBox3.SelectedIndex == 2)
            {
                date2 = date1;
                date2.AddMonths(1 - date1.Month);
            }
            
            report.SetParams(date1, date2, (enErrorMode)metroComboBox1.SelectedValue, (enInsuranceMode)metroComboBox2.SelectedValue);
            report.SetResultValues();
            report.ValuesToReoGrid(reoGridControl1.CurrentWorksheet);

            if (reoGridControl1.Visible == false)
                reoGridControl1.Visible = true;
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            date1 = date1.AddMonths(-1);
            metroTextBox1.Text = date1.ToString("MM.yyyy");
            if (metroTextBox2.Visible == false)
                date2 = date1;
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            date1 = date1.AddMonths(1);
            metroTextBox1.Text = date1.ToString("MM.yyyy");
            if (metroTextBox2.Visible == false)
                date2 = date1;
        }

        private void metroButton5_Click(object sender, EventArgs e)
        {
            date2 = date2.AddMonths(-1);
            metroTextBox2.Text = date2.ToString("MM.yyyy");
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            date2 = date2.AddMonths(1);
            metroTextBox2.Text = date2.ToString("MM.yyyy");
        }

        private void metroComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (metroComboBox3.SelectedIndex == 2)
                metroPanel4.Visible = true;
            else
                metroPanel4.Visible = false;

            metroLabel7.Text = metroComboBox3.Text;
        }

        private void reoGridControl1_DoubleClick(object sender, EventArgs e)
        {          
            report.ExpandCollapse(reoGridControl1, ref gridScrollBarsPosition);
        }

        private void metroButton7_Click(object sender, EventArgs e)
        {
        }

        private void metroTrackBar1_MouseCaptureChanged(object sender, EventArgs e)
        {
            report.ExpandCollapse(reoGridControl1, ref gridScrollBarsPosition,metroTrackBar1.Value,-1);
        }

        private void metroTrackBar2_MouseCaptureChanged(object sender, EventArgs e)
        {
            report.ExpandCollapse(reoGridControl1, ref gridScrollBarsPosition ,- 1, metroTrackBar2.Value);
        }

        private void metroLabel8_Click(object sender, EventArgs e)
        {
            if (metroTrackBar1.Value > metroTrackBar1.Minimum)
            {
                metroTrackBar1.Value--;
                metroTrackBar1_MouseCaptureChanged(new object(), new EventArgs());
            }                
        }

        private void metroLabel9_Click(object sender, EventArgs e)
        {
            if (metroTrackBar1.Value < metroTrackBar1.Maximum)
            {
                metroTrackBar1.Value++;
                metroTrackBar1_MouseCaptureChanged(new object(), new EventArgs());
            }
        }

        private void metroLabel11_Click(object sender, EventArgs e)
        {
            if (metroTrackBar2.Value > metroTrackBar2.Minimum)
            {
                metroTrackBar2.Value--;
                metroTrackBar2_MouseCaptureChanged(new object(), new EventArgs());
            }
        }

        private void metroLabel12_Click(object sender, EventArgs e)
        {
            if (metroTrackBar2.Value < metroTrackBar2.Maximum)
            {
                metroTrackBar2.Value++;
                metroTrackBar2_MouseCaptureChanged(new object(), new EventArgs());
            }
        }

        private void metroButton7_Click_1(object sender, EventArgs e)
        {
            reoGridControl1.CurrentWorksheet.EndEdit(EndEditReason.NormalFinish);
            report.SavePlan(reoGridControl1.CurrentWorksheet);
            report.SetResultValues();
            report.ValuesToReoGrid(reoGridControl1.CurrentWorksheet);
        }

        private void reoGridControl1_WorksheetScrolled(object sender, unvell.ReoGrid.Events.WorksheetScrolledEventArgs e)
        {
            gridScrollBarsPosition.X += e.OffsetX;
            gridScrollBarsPosition.Y += e.OffsetY;
        }

        private void metroButton6_Click(object sender, EventArgs e)
        {
            reoGridControl1.CurrentWorksheet.HideColumns(5, 5);
        }
    }
}
